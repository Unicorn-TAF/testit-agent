using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tms.Adapter.Core.Configurator;
using Tms.Adapter.Core.Models;
using Tms.Adapter.Core.Service;
using Tms.Adapter.Core.Utils;
using Unicorn.Taf.Core.Steps;
using Unicorn.Taf.Core.Testing;

namespace Unicorn.Reporting.TestIt;

/// <summary>
/// TestIT listener, which handles reporting stuff for all test items.
/// </summary>
public sealed class TestItListener
{
    private readonly ReportingContainer _reportingContainer = new();

    internal TestItListener()
    {
        if (string.IsNullOrEmpty(Configurator.GetConfig().TestRunId))
        {
            AdapterManager.Instance.CreateTestRun().Wait();
        }
    }

    internal void StartSuite(TestSuite suite)
    {
        try
        {
            ClassContainer classContainer = ReportingHelper.NewContainer();
            AdapterManager.Instance.StartTestContainer(classContainer);

            _reportingContainer.Suites.TryAdd(suite.Outcome.Id, (classContainer.Id, suite));
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(StartSuite), e);
        }
    }

    internal void FinishSuite(TestSuite suite)
    {
        try
        {
            _reportingContainer.Suites.TryRemove(suite.Outcome.Id, out var container);
            AdapterManager.Instance.StopTestContainer(container.Item1);
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(FinishSuite), e);
        }
    }

    internal void StartTest(SuiteMethod suiteMethod)
    {
        try
        {
            TestOutcome outcome = suiteMethod.Outcome;

            _reportingContainer.Suites.TryGetValue(outcome.ParentId, out var suiteContainer);
            string suiteUuid = suiteContainer.Item1;
            
            ClassContainer fakeTestContainer = ReportingHelper.NewContainer();
            AdapterManager.Instance.StartTestContainer(suiteUuid, fakeTestContainer);

            TestContainer test = new()
            {
                Id = Hash.NewId(),
                ClassName = ReportingHelper.GetClassName(suiteMethod.TestMethod.DeclaringType.FullName),
                Namespace = ReportingHelper.GetNameSpace(suiteMethod.TestMethod.DeclaringType.FullName),
                ExternalId = outcome.Id.ToString(),
                DisplayName = suiteMethod.TestMethod.Name,
                Title = outcome.Title,
                Labels = GenerateTestLabels(suiteMethod)
            };

            if (!string.IsNullOrEmpty(outcome.TestCaseId))
            {
                test.WorkItemIds.Add(outcome.TestCaseId);
            }

            AdapterManager.Instance.StartTestCase(fakeTestContainer.Id, test);
            
            _reportingContainer.TestContainers.TryAdd(outcome.Id, (fakeTestContainer.Id, test.Id));
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(StartTest), e);
        }
    }

    internal void FinishTest(SuiteMethod suiteMethod)
    {
        try
        {
            TestOutcome outcome = suiteMethod.Outcome;

            _reportingContainer.TestContainers.TryRemove(outcome.Id, out var test);
            string containerUuid = test.Item1;
            string testUuid = test.Item2;

            AdapterManager.Instance.UpdateTestCase(testUuid, tc => tc.Status = ReportingHelper.GetStatus(outcome));

            if (outcome.Result == Taf.Core.Testing.Status.Failed)
            {
                ReportingHelper.FillFailedTest(testUuid, outcome);
            }

            AdapterManager.Instance.StopTestCase(testUuid);
            AdapterManager.Instance.StopTestContainer(containerUuid);
            AdapterManager.Instance.WriteTestCase(testUuid, containerUuid);
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(FinishTest), e);
        }
    }

    internal void StartFixture(SuiteMethod suiteMethod)
    {
        try
        {
            TestOutcome outcome = suiteMethod.Outcome;

            var result = new FixtureResult
            {
                DisplayName = suiteMethod.TestMethod.Name
            };

            _reportingContainer.Suites.TryGetValue(outcome.ParentId, out var container);
            string suiteUuid = container.Item1;
            string uuid = Hash.NewId();

            switch (suiteMethod.MethodType)
            {
                case SuiteMethodType.BeforeSuite:
                case SuiteMethodType.BeforeTest:
                    AdapterManager.Instance.StartBeforeFixture(suiteUuid, uuid, result);
                    break;
                case SuiteMethodType.AfterSuite:
                case SuiteMethodType.AfterTest:
                    AdapterManager.Instance.StartAfterFixture(suiteUuid, uuid, result);
                    break;
                default:
                    break;
            }

            _reportingContainer.Fixtures.TryAdd(outcome.Id, uuid);
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(StartFixture), e);
        }
    }

    internal void FinishFixture(SuiteMethod suiteMethod)
    {
        try
        {
            TestOutcome outcome = suiteMethod.Outcome;
            _reportingContainer.Fixtures.TryRemove(outcome.Id, out string uuid);
            AdapterManager.Instance.UpdateFixture(uuid, fr => fr.Status = ReportingHelper.GetStatus(outcome));

            if (outcome.Result == Taf.Core.Testing.Status.Failed)
            {
                ReportingHelper.FillFailedFixture(uuid, outcome);
            }
            
            AdapterManager.Instance.StopFixture(uuid);
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(FinishFixture), e);
        }
    }

    internal void SkipTest(SuiteMethod suiteMethod)
    {
        StartTest(suiteMethod);
        FinishTest(suiteMethod);
    }

    internal void StartStep(MethodBase method, object[] arguments)
    {
        try
        {
            StepResult result = new()
            {
                DisplayName = StepsUtilities.GetStepInfo(method, arguments),
                Status = Tms.Adapter.Core.Models.Status.Passed,
            };

            // TODO: Restriction: by now steps are not correctly reported for parallel execution

            string parentUuid = _reportingContainer.TestContainers.Any() ?
                _reportingContainer.TestContainers.First().Value.Item2 :
                _reportingContainer.Fixtures.First().Value;

            AdapterManager.Instance.StartStep(parentUuid, Hash.NewId(), result);
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(StartStep), e);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    internal void FinishStep(MethodBase method, object[] arguments)
    {
        try
        {
            AdapterManager.Instance.StopStep(step => { });
        }
        catch (Exception e)
        {
            ReportingHelper.LogException(nameof(FinishStep), e);
        }
    }

    private List<string> GenerateTestLabels(SuiteMethod suiteMethod)
    {
        _reportingContainer.Suites.TryGetValue(suiteMethod.Outcome.ParentId, out var container);
        return container.Item2.Tags.Union((suiteMethod as Test).Categories).ToList();
    }
}
