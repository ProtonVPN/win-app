/*
 * Copyright (c) 2020 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ProtonVPN.UI.Test.TestsHelper;
using TestRail;
using TestRail.Enums;
using TestRail.Types;
using TestRail.Utils;

namespace ProtonVPN.UI.Test.ApiClient
{
    public class TestRailAPIClient : UITestSession
    {
        private readonly TestRailClient _client;
        private const int ProjectId = 5;
        private const int MilestoneId = 43;
        private const int TestSuiteId = 5;
        private static ulong _testRunId;

        public TestRailAPIClient(string baseUrl, string username, string apiKey)
        {
            _client = new TestRailClient(baseUrl, username, apiKey);
        }

        public void CreateTestRun(string testRunName)
        {
            RequestResult<Run> testRun = _client.AddRun(ProjectId, TestSuiteId, testRunName, "Automated regression " + testRunName, MilestoneId);
            _testRunId = testRun.Payload.Id.Value;
        }

        public void MarkTestsByStatus()
        {
            if(TestEnvironment.AreTestsRunningLocally())
            {
                return;
            }

            if (TestCaseId == 0)
            {
                return;
            }

            TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;
            switch (status)
            {
                case TestStatus.Failed:
                    MarkAsRetest(TestCaseId);
                    break;
                case TestStatus.Passed:
                case TestStatus.Inconclusive:
                    MarkAsPassed(TestCaseId);
                    break;
            }
        }

        public bool ShouldUpdateRun()
        {
            string branchName = Environment.GetEnvironmentVariable("CI_COMMIT_BRANCH");
            IList<Run> runs = _client.GetRuns(ProjectId).Payload;
            foreach(Run run in runs)
            {
                if (run.Name.Contains(branchName) && run.IsCompleted != true && run.Id.HasValue)
                {
                    _testRunId = run.Id.Value;
                    return true;
                }
            }
            return false;
        }

        private void MarkAsPassed(ulong testCaseId)
        {
            _client.AddResultForCase(_testRunId, testCaseId, ResultStatus.Passed);
        }

        private void MarkAsRetest(ulong testCaseId)
        {
            _client.AddResultForCase(_testRunId, testCaseId, ResultStatus.Retest);
        }
    }
}
