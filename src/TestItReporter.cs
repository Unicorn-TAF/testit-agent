using Unicorn.Taf.Api;
using Unicorn.Taf.Core;

namespace Unicorn.Reporting.TestIt;

/// <summary>
/// TestIT reporter instance. Contains subscriptions to corresponding Unicorn events.
/// </summary>
public sealed class TestItReporter : ITestReporter
{
    private readonly TestItListener _listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestItReporter"/> class.<br/>
    /// Automatic subscribtion to all test events.
    /// </summary>
    public TestItReporter()
    {
        _listener = new TestItListener();

        TafEvents.OnTestStart += _listener.StartTest;
        TafEvents.OnTestFinish += _listener.FinishTest;
        TafEvents.OnTestSkip += _listener.SkipTest;

        TafEvents.OnSuiteMethodStart += _listener.StartFixture;
        TafEvents.OnSuiteMethodFinish += _listener.FinishFixture;

        TafEvents.OnSuiteStart += _listener.StartSuite;
        TafEvents.OnSuiteFinish += _listener.FinishSuite;

        TafEvents.OnStepStart += _listener.StartStep;
        TafEvents.OnStepFinish += _listener.FinishStep;
    }

    /// <summary>
    /// Unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        TafEvents.OnTestStart -= _listener.StartTest;
        TafEvents.OnTestFinish -= _listener.FinishTest;
        TafEvents.OnTestSkip -= _listener.SkipTest;

        TafEvents.OnSuiteMethodStart -= _listener.StartFixture;
        TafEvents.OnSuiteMethodFinish -= _listener.FinishFixture;

        TafEvents.OnSuiteStart -= _listener.StartSuite;
        TafEvents.OnSuiteFinish -= _listener.FinishSuite;

        TafEvents.OnStepStart -= _listener.StartStep;
        TafEvents.OnStepFinish -= _listener.FinishStep;
    }
}
