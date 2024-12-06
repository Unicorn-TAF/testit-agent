![Nuget](https://img.shields.io/nuget/v/Unicorn.Reporting.TestIT?style=plastic)
![Nuget](https://img.shields.io/nuget/dt/Unicorn.Reporting.TestIT?style=plastic)

# TestIT reporting agent

Unicorn has ability to generate powerful test results report using [TestIT TMS](https://testit.software)

Just deploy Allure Framework instance, add tests project dependency to [Unicorn.Reporting.TestIT](https://www.nuget.org/packages/Unicorn.Reporting.TestIT) package and initialize reporter during tests assembly initialization.
***
Place **Tms.config.json** configuration file to directory with test assemblies. Sample content is presented below:
```json
{
  "url": "url_to_testit_instance",
  "privateToken": "token_value",
  "projectId": "project_id_value",
  "configurationId": "configuration_id_value",
  "testRunId": "id_of_started_test_run <if id not specified, new run starts automatically>",
  "testRunName": "custom_run_name_in_case_testRunId_is_not_specified"
  "automaticCreationTestCases": false,
  "automaticUpdationLinksToTestCases": false,
  "certValidation": true,
  "isDebug": false
}
```
then add code with reporting initialization to `[TestsAssembly]`
```csharp
using Unicorn.Taf.Api;
using Unicorn.Core.Testing.Tests.Attributes;
using Unicorn.Reporting.TestIt;

namespace Tests
{
    [TestsAssembly]
    public static class TestsAssembly
    {
        private static ITestReporter reporter;

        [RunInitialize]
        public static void InitRun()
        {
            reporter = new TestItReporter(); // initializes reporter and subscribes to TAF events.
        }

        [RunFinalize]
        public static void FinalizeRun()
        {
            reporter.Dispose(); // Unsubscribe reporter from unicorn events.
            reporter = null;
        }
    }
}
```
