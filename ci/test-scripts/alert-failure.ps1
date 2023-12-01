if ($env:CI_JOB_STATUS -eq 'failed' -and $env:CI_PIPELINE_SOURCE -eq 'schedule') {
    $text = ":warn: Scheduled BTI tests failed for Windows client :windows-10: ! `n$env:CI_JOB_URL"
    $body = @{
        text = $text
    } | ConvertTo-Json

    $headers = @{
        "Content-Type" = "application/json"
    }

    $output = Invoke-WebRequest -Uri $env:SLACK_QA_HOOK -Method Post -Headers $headers -Body $body -UseBasicParsing
}