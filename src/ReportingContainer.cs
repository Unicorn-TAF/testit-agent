using System;
using System.Collections.Concurrent;
using Unicorn.Taf.Core.Testing;

namespace Unicorn.Reporting.TestIt;

internal sealed class ReportingContainer
{
    internal ConcurrentDictionary<Guid, (string, TestSuite)> Suites { get; } = new();
    internal ConcurrentDictionary<Guid, (string, string)> TestContainers { get; } = new();
    internal ConcurrentDictionary<Guid, string> Fixtures { get; } = new();
}
