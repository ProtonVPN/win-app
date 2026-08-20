$versionFile = "$env:UI_TEST_REPORT_PATH\version.txt"
$version = if (Test-Path $versionFile) { (Get-Content $versionFile -Raw).Trim() } else { "unknown" }

Write-Host "-------------`nBranch: $env:CI_COMMIT_REF_NAME `nVersion: $version`n-------------"

function Main {
    $projectId = 1
    $testmoUrl = $env:TESTMO_URL.Replace('/api/v1', '')

    $xmlFiles = @(Get-ChildItem -Path $env:UI_TEST_REPORT_PATH -Filter "results_*.xml" -Recurse)
    $isSmokeTest = $xmlFiles.Count -gt 0 -and $xmlFiles[0].Name -match "SMOKE"
    $isArmTest = $xmlFiles.Count -gt 0 -and $xmlFiles[0].Name -match "ARM"

    $subfolders, $folderMap = Get-Subfolders $projectId

    $automatedCount, $semiAutomatedCount, $manualCount, $testMoCasesMap = Get-TestCases $projectId $subfolders

    $runId = New-Run $isSmokeTest $isArmTest

    $passedCount = 0
    $failedCount = 0
    $skippedKnownIssue = @()
    $skippedCountKnownIssue = 0
    $skippedManualRetest = @()
    $skippedCountManualRetest = 0

    $skippedNoTc = @()
    $skippedCountNoTestCase = 0
    $seenNoTcTests = @{}

    $seenTestCaseIds = @{}
    $mergedTestCases = @{}
    $uploadedTestCount = 0

    $parameterizedTests = @{}
    $parameterizedUploadCount = 0
    $allParameterizedVariants = @{}

    $totalRunElapsed = 0

    foreach ($xmlFile in $xmlFiles) {
        [xml]$xml = Get-Content $xmlFile.FullName
        $testResults = @()

        foreach ($testcase in $xml.testsuites.testsuite.testcase) {
            if ($testcase.name -match "\(.*\)") {
                $baseTestName = $testcase.name -replace "\(.*\)$", ""
                if (-not $allParameterizedVariants.ContainsKey($baseTestName)) {
                    $allParameterizedVariants[$baseTestName] = 0
                }
                $allParameterizedVariants[$baseTestName] += 1
            }

            $testCaseIdRaw = $testcase.properties.property | Where-Object { $_.name -eq "TestCaseId" } | Select-Object -ExpandProperty value
            if ($testCaseIdRaw -eq "IGNORE") {
                continue
            }

            $status = Get-TestStatus $testcase $testCaseIdRaw

            switch ($status) {
                "failed" { $failedCount++; }
                "passed" { $passedCount++; }
                "skipped" { $skippedCountKnownIssue++; $skippedKnownIssue += $testcase.name }
                "retest" { $skippedCountManualRetest++; $skippedManualRetest += $testcase.name }
            }

            if (-not $testCaseIdRaw -or $testCaseIdRaw -eq "NO_TC_FOUND" -or $testCaseIdRaw -eq "IGNORE") {
                $baseTestName = $testcase.name -replace "\(.*\)$", ""
                if (-not $seenNoTcTests.ContainsKey($baseTestName)) {
                    $skippedNoTc += $testcase.name
                    $skippedCountNoTestCase++
                    $seenNoTcTests[$baseTestName] = $true
                }
                continue
            }

            $testCaseIds = $testCaseIdRaw -split "," | ForEach-Object { $_.Trim() }

            if ($testCaseIds.Count -gt 1) {
                $mergedTestCases[$testcase.name] = $testCaseIds
            }

            foreach ($testCaseId in $testCaseIds) {
                if ($seenTestCaseIds.ContainsKey($testCaseId)) {
                    continue
                }
                $seenTestCaseIds[$testCaseId] = $true
                $uploadedTestCount++

                if ($testcase.name -match "\(.*\)") {
                    $baseTestName = $testcase.name -replace "\(.*\)$", ""
                    if (-not $parameterizedTests.ContainsKey($baseTestName)) {
                        $parameterizedTests[$baseTestName] = 0
                        $parameterizedUploadCount++
                    }
                    $parameterizedTests[$baseTestName] += 1
                }

                $testMoCase = $testMoCasesMap[$testCaseId]
                $folderName = if ($testMoCase) { $folderMap[$testMoCase.folder_id.ToString()] } else { $testcase.classname }

                $failureMessage = if ($testcase.failure) { $testcase.failure.InnerText } else { $null }

                $testFields = @(
                    @{ type = 5; name = "Linked Test"; value = "$testmoUrl/repositories/1?case_id=$testCaseId" }
                    @{ type = 1; name = "C# Test Name"; value = $testcase.name }
                    @{ type = 4; name = "Message"; value = $failureMessage; is_highlight = $true }
                    @{ type = 5; name = "CI Job"; value = $env:CI_JOB_URL }
                )

                $testResults += @{
                    key     = $testCaseId
                    folder  = $folderName
                    name    = if ($testMoCase) { $testMoCase.name } else { $testcase.name }
                    status  = $status
                    elapsed = [long]([double]$testcase.time * 1000000)
                    fields  = $testFields
                }
            }
        }
        $threadElapsed = [long](($testResults | ForEach-Object { $_.elapsed } | Measure-Object -Sum).Sum)
        $totalRunElapsed += $threadElapsed

        New-Thread $xmlFile $runId $testResults $threadElapsed
    }

    $automatedCountInCode = $passedCount + $failedCount
    $manualCount = $manualCount - 9 #Precondition tests to exclude from manual count
    $totalTestsInTestMo = $automatedCount + $semiAutomatedCount + $manualCount
    $automatedPercentage = if ($totalTestsInTestMo -gt 0) { [math]::Round((($automatedCount + $semiAutomatedCount) / $totalTestsInTestMo) * 100, 1) } else { 0 }
    $totalMinutes = [math]::Round($totalRunElapsed / 60000000, 1)
    $mergedTestCount = $mergedTestCases.Count
    
    $totalTestsInCode = $automatedCountInCode + $skippedCountKnownIssue + $skippedCountManualRetest
    
    $totalParameterizedInstances = 0
    foreach ($count in $allParameterizedVariants.Values) {
        $totalParameterizedInstances += $count
    }
    
    $parameterizedDuplicates = $totalParameterizedInstances - $($allParameterizedVariants.Count)
    
    Complete-Run $runId $totalMinutes $skippedCountNoTestCase $automatedPercentage

    Show-Summary $isSmokeTest $automatedCountInCode $automatedCount $semiAutomatedCount $manualCount $totalTestsInTestMo $automatedPercentage $passedCount $failedCount $skippedCountNoTestCase $skippedCountKnownIssue $skippedCountManualRetest $totalMinutes $skippedNoTc $skippedManualRetest $skippedKnownIssue $uploadedTestCount $mergedTestCount $mergedTestCases $totalTestsInCode $parameterizedUploadCount $allParameterizedVariants $totalParameterizedInstances $parameterizedTests $parameterizedDuplicates
}

function Get-TestStatus {
    param(
        $testcase,
        $testCaseIdRaw
    )

    if ($testcase.failure) {
        $status = "failed"
    }
    elseif ($null -ne $testcase.skipped) {
        $systemOut = $testcase."system-out"

        if ($systemOut -match "JIRA") {
            $status = "skipped"
        }
        else {
            $status = "retest"
        }
    }
    else {
        # Case ID of the flaky Port Forwarding tests
        if ($testCaseIdRaw -eq "602439" -or $testCaseIdRaw -eq "602438") {
            $systemOut = $testcase."system-out"
            if ($systemOut -match "SUCCESS") {
                $status = "passed"
            }
            else {
                $status = "retest"
            }
        }
        else {
            $status = "passed"
        }
    }

    return $status
}

function Get-Subfolders {
    param(
        $projectId)

    $mainFolderId = 91214

    $subfolders = @()
    $folderMap = @{}
    $foldersUrl = "$env:TESTMO_URL/projects/$projectId/folders?parent_id=$mainFolderId"

    $foldersResponse = Invoke-ApiCall `
        -Uri $foldersUrl `
        -Method Get `

    foreach ($folder in $foldersResponse.result) {
        $subfolders += $folder.id
        $folderMap[$folder.id.ToString()] = $folder.name
    }

    return $subfolders, $folderMap
}

function Get-TestCases {
    param(
        $projectId,
        $subfolders)

    $automationTagId = 43821
    $semiAutomationTagId = 61722

    $automatedCount = 0
    $semiAutomatedCount = 0
    $manualCount = 0
    $testMoCasesMap = @{}

    foreach ($folderId in $subfolders) {
        $page = 1
        $hasMore = $true
 
        while ($hasMore) {
            $casesUrl = "$env:TESTMO_URL/projects/$projectId/cases?folder_id=$folderId&expands=tags&page=$page"

            $casesResponse = Invoke-ApiCall `
                -Uri $casesUrl `
                -Method Get `

            foreach ($case in $casesResponse.result) {
                if ($case.tags -contains $automationTagId) {
                    $automatedCount++
                }
                elseif ($case.tags -contains $semiAutomationTagId) {
                    $semiAutomatedCount++
                }
                else {
                    $manualCount++
                }

                $testMoCasesMap[$case.id.ToString()] = $case
            }
     
            $page++
            $hasMore = $page -le $casesResponse.last_page
        }
    }

    return $automatedCount, $semiAutomatedCount, $manualCount, $testMoCasesMap
}

function New-Run {
    param(
        $isSmokeTest,
        $isArmTest)

    $runType = if ($isSmokeTest) { "Smoke" } else { "Full regression" }
    $architectures = if ($isArmTest) { "arm64" } else { "x64" }
    $runName = "$runType $version - $architectures"
    $source = if ($env:CI_COMMIT_REF_NAME -like "release/*") { "Release" } elseif ($env:CI_COMMIT_REF_NAME -eq "develop") { "Develop" } else { "Automation" }
    $tag = $env:CI_COMMIT_REF_NAME.Replace("/", "-").Replace(".", "-") -replace "VPNWIN-\d+-", ""

    $runPayload = @{
        name   = $runName
        source = $source
        tags   = @($tag)
    } | ConvertTo-Json

    $runResponse = Invoke-ApiCall `
        -Uri "$env:TESTMO_URL/projects/1/automation/runs" `
        -Method Post `
        -Body $runPayload `
        -OperationName "Run created`n-------------"

    return $runResponse.id
}

function New-Thread {
    param(
        $xmlFile,
        $runId, 
        $testResults,
        $threadElapsed)

    # Create thread
    $category = $xmlFile.Name.Replace("results_", "").Replace(".xml", "")

    $threadPayload = @{ elapsed_observed = $threadElapsed } | ConvertTo-Json

    $threadResponse = Invoke-ApiCall `
        -Uri "$env:TESTMO_URL/automation/runs/$runId/threads" `
        -Method Post `
        -Body $threadPayload `
        -OperationName "Thread created for Category[$category]"

    $threadId = $threadResponse.id

    # Add tests to thread
    $appendPayload = @{
        tests = $testResults
    } | ConvertTo-Json -Depth 10 -Compress

    $threadAppendResponse = Invoke-ApiCall `
        -Uri "$env:TESTMO_URL/automation/runs/threads/$threadId/append" `
        -Method Post `
        -Body $appendPayload `
        -OperationName "Thread appended, added $($testResults.Count) tests"

    # Complete the thread
    $completeThreadPayload = @{ elapsed_observed = $threadElapsed } | ConvertTo-Json

    $threadCompleteResponse = Invoke-ApiCall `
        -Uri "$env:TESTMO_URL/automation/runs/threads/$threadId/complete" `
        -Method Post `
        -Body $completeThreadPayload `
        -OperationName "Thread completed`n-------------"
}

function Complete-Run {
    param(
        $runId, 
        $totalMinutes, 
        $skippedCountNoTestCase, 
        $automatedPercentage)

    # Add info to run
    $appendRunPayload = @{
        fields = @(
            @{ type = 1; name = "Total elapsed"; value = "$totalMinutes min" }
            @{ type = 1; name = "Missing Tests"; value = "$skippedCountNoTestCase" }
            @{ type = 1; name = "Coverage"; value = "$automatedPercentage%" }
        )
    } | ConvertTo-Json -Depth 5

    $runAppendResponse = Invoke-ApiCall `
        -Uri "$env:TESTMO_URL/automation/runs/$runId/append" `
        -Method Post `
        -Body $appendRunPayload `
        -OperationName "Run appended"

    # Complete the run
    $completeRunPayload = @{ measure_elapsed = $false } | ConvertTo-Json

    $runCompleteResponse = Invoke-ApiCall `
        -Uri "$env:TESTMO_URL/automation/runs/$runId/complete" `
        -Method Post `
        -Body $completeRunPayload `
        -OperationName "Run completed"
}

function Show-Summary {
    param(
        $isSmokeTest,
        $automatedCountInCode,
        $automatedCount, 
        $semiAutomatedCount,
        $manualCount, 
        $totalTestsInTestMo, 
        $automatedPercentage, 
        $passedCount,
        $failedCount, 
        $skippedCountNoTestCase, 
        $skippedCountKnownIssue, 
        $skippedCountManualRetest, 
        $totalMinutes,
        $skippedNoTc, 
        $skippedManualRetest, 
        $skippedKnownIssue,
        $uploadedTestCount,
        $mergedTestCount,
        $mergedTestCases,
        $totalTestsInCode,
        $parameterizedUploadCount,
        $allParameterizedVariants,
        $totalParameterizedInstances,
        $parameterizedTests,
        $parameterizedDuplicates)

    $executedTests = $passedCount + $failedCount
    $mergedUploadedTcIds = $mergedTestCount * 2
    $uploadedFromExecuted = $executedTests - $skippedCountNoTestCase - $parameterizedDuplicates - $mergedTestCount + $mergedUploadedTcIds

    Write-Host "`n========================================"
    Write-Host "Test Coverage & Results"
    Write-Host "========================================"

    $automatedSmokeTests = if ($isSmokeTest) { "[smoke tests only]" } else { "" }

    Write-Host "Automated (Code): $totalTestsInCode $automatedSmokeTests"
    Write-Host "Executed: $executedTests = Passed($passedCount) + Failed($failedCount)"
    Write-Host "Upload Calculation: $uploadedFromExecuted = Executed($executedTests) - Missing tests from TestMo($skippedCountNoTestCase) - Parameterized duplicates($parameterizedDuplicates) + Additional TC from Merged tests($mergedTestCount)"
    Write-Host "Final Upload: $uploadedTestCount = Upload Calculation($uploadedFromExecuted) + Skipped($skippedCountKnownIssue) + Retest($skippedCountManualRetest)"
    Write-Host "Automated (TestMo): $($automatedCount)"
    Write-Host "Semi Automated (TestMo): $($semiAutomatedCount)"
    Write-Host "Manual (TestMo): $manualCount"
    Write-Host "Total Tests (TestMo): $totalTestsInTestMo"
    Write-Host "Automation coverage: $automatedPercentage%"
    Write-Host "-------------" 
    Write-Host "Uploaded to Testmo:"
    Write-Host "Unique TC IDs: $uploadedTestCount"
    Write-Host "Total elapsed: $totalMinutes min"
    Write-Host "========================================"

    foreach ($name in $mergedTestCases.Keys) { $tcIds = $mergedTestCases[$name] -join ", "; Write-Host "Merged: $name > TC IDs: $tcIds" }
    Write-Host "-------------"
    foreach ($baseName in $allParameterizedVariants.Keys) { 
        $allVariants = $allParameterizedVariants[$baseName]
        $uploadedVariants = if ($parameterizedTests.ContainsKey($baseName)) { $parameterizedTests[$baseName] } else { 0 }
        if ($uploadedVariants -eq 0) {
            Write-Host "Parameterized: $baseName - has $allVariants scenarios (ALL SKIPPED - No TC ID)"
        }
        else {
            Write-Host "Parameterized: $baseName - has $allVariants scenarios (uploaded: $uploadedVariants)"
        }
    }
    Write-Host "-------------" 
    foreach ($name in $skippedNoTc) { Write-Host "Skipped (No TC in TestMo): $name" }
    Write-Host "-------------" 
    foreach ($name in $skippedKnownIssue) { Write-Host "Skipped (Known issue): $name" }
    Write-Host "-------------" 
    foreach ($name in $skippedManualRetest) { Write-Host "Skipped (Retest Manually): $name" }
}

function Invoke-ApiCall {
    param(
        $Uri,
        $Method,
        $Body,
        $OperationName
    )
    
    try {
        $params = @{
            Uri     = $Uri
            Method  = $Method
            Headers = @{ Authorization = "Bearer $env:TESTMO_TOKEN" }
        }

        if ($Body) {
            $params['Body'] = $Body
            $params['ContentType'] = "application/json"
        }

        $response = Invoke-RestMethod @params

        if ($OperationName) { Write-Host "$OperationName" }

        return $response
    }
    catch {
        $errorMessage = $_.Exception.Message
        $statusCode = $_.Exception.Response.StatusCode.Value__ 2>$null
        
        if ($OperationName) { Write-Host "$OperationName failed" }
        Write-Host "Status: $statusCode"
        Write-Host "Error: $errorMessage"
        
        return $null
    }
}

Main