using System;
using System.Linq;
using Tms.Adapter.Core.Models;
using Tms.Adapter.Core.Service;
using Tms.Adapter.Core.Utils;
using Unicorn.Taf.Core.Testing;
using TestItStatus = Tms.Adapter.Core.Models.Status;
using UnicornStatus = Unicorn.Taf.Core.Testing.Status;

namespace Unicorn.Reporting.TestIt;

internal static class ReportingHelper
{
    internal static ClassContainer NewContainer() =>
         new() { Id = Hash.NewId() };

    internal static TestItStatus GetStatus(TestOutcome outcome) =>
        outcome.Result switch
        {
            UnicornStatus.Failed => TestItStatus.Failed,
            UnicornStatus.Skipped => TestItStatus.Skipped,
            _ => TestItStatus.Passed,
        };

    internal static void FillFailedTest(string uuid, TestOutcome outcome)
    {
        AdapterManager.Instance.UpdateTestCase(uuid, tc =>
        {
            tc.Message = outcome.FailMessage;
            tc.Trace = outcome.FailStackTrace;

            if (tc.Steps.Any())
            {
                tc.Steps.Last().Status = TestItStatus.Failed;
            }

            if (outcome.Defect != null && !string.IsNullOrEmpty(outcome.Defect.Comment))
            {
                Link link = new(outcome.Defect.Comment, outcome.Defect.Id, string.Empty, LinkType.Defect);
                //tc.Links.Add(link);
                Adapter.AddLinks(link);
            }

            foreach (Attachment attachment in outcome.Attachments)
            {
                Adapter.AddAttachments(attachment.FilePath);
                //tc.Attachments.Add(attachment.FilePath);
            }
        });
    }

    internal static void FillFailedFixture(string uuid, TestOutcome outcome)
    {
        AdapterManager.Instance.UpdateFixture(uuid, fr =>
        {
            foreach (Attachment attachment in outcome.Attachments)
            {
                //fxt.Attachments.Add(attachment.FilePath);
                Adapter.AddAttachments(attachment.FilePath);
            }

            if (fr.Steps.Any())
            {
                fr.Steps.Last().Status = TestItStatus.Failed;
            }
        });
    }

    internal static string GetClassName(string value) =>
        value.Split('.')[^1];

    internal static string GetNameSpace(string value) =>
        value[..value.LastIndexOf(".")];

    internal static void LogException(string methodName, Exception e) =>
        Console.WriteLine($"[{nameof(TestItListener)}] Exception in {methodName}:" + Environment.NewLine + e);
}
