using System;
using Unicorn.Taf.Core.Steps;
using Unicorn.Taf.Core.Testing;

namespace Unicorn.Reporting.TestIt;

/// <summary>
/// TestIT reporter instance. Contains subscriptions to corresponding Unicorn events.
/// </summary>
public sealed class ReporterInstance : IDisposable
{
    private readonly TestItListener _listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReporterInstance"/> class.<br/>
    /// Automatic subscribtion to all test events.
    /// </summary>
    public ReporterInstance()
    {
        _listener = new TestItListener();

        Test.OnTestStart += _listener.StartTest;
        Test.OnTestFinish += _listener.FinishTest;
        Test.OnTestSkip += _listener.SkipTest;

        SuiteMethod.OnSuiteMethodStart += _listener.StartFixture;
        SuiteMethod.OnSuiteMethodFinish += _listener.FinishFixture;

        TestSuite.OnSuiteStart += _listener.StartSuite;
        TestSuite.OnSuiteFinish += _listener.FinishSuite;

        StepEvents.OnStepStart += _listener.StartStep;
        StepEvents.OnStepFinish += _listener.FinishStep;
    }

    /// <summary>
    /// Unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        Test.OnTestStart -= _listener.StartTest;
        Test.OnTestFinish -= _listener.FinishTest;
        Test.OnTestSkip -= _listener.SkipTest;

        SuiteMethod.OnSuiteMethodStart -= _listener.StartFixture;
        SuiteMethod.OnSuiteMethodFinish -= _listener.FinishFixture;

        TestSuite.OnSuiteStart -= _listener.StartSuite;
        TestSuite.OnSuiteFinish -= _listener.FinishSuite;

        StepEvents.OnStepStart -= _listener.StartStep;
        StepEvents.OnStepFinish -= _listener.FinishStep;
    }
}
