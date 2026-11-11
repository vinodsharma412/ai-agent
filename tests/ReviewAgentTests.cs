using System.Text.Json;
using DiffReviewAgent;
using Xunit;

namespace DiffReviewAgent.Tests;

public sealed class ReviewAgentTests
{
    [Fact]
    public void ParseAddedLineNumbersAndPaths()
    {
        const string diff =
            "diff --git a/src/example.py b/src/example.py\n" +
            "--- a/src/example.py\n" +
            "+++ b/src/example.py\n" +
            "@@ -2,2 +2,3 @@\n" +
            " keep()\n" +
            "+added()\n" +
            "-removed()\n" +
            "+also_added()\n";

        Assert.Equal(
            new[]
            {
                ("src/example.py", 3, "added()"),
                ("src/example.py", 4, "also_added()")
            },
            ReviewEngine.ParseChangedLines(diff));
    }

    [Fact]
    public void OfflineReviewFindsRisksAndMissingTests()
    {
        const string diff =
            "+++ b/app.py\n" +
            "@@ -1 +1,3 @@\n" +
            "+password = 'sample_secret_value_123'\n" +
            "+try:\n" +
            "+    run()\n" +
            "+except Exception:\n" +
            "+    pass\n";

        var review = ReviewEngine.OfflineReview(diff);

        Assert.Equal("offline-heuristics", review.Engine);
        Assert.Contains(review.Findings, finding => finding.Category == "risk");
        Assert.Contains(review.Findings, finding => finding.Category == "possible_bug");
        Assert.Contains(review.Findings, finding => finding.Category == "missing_test");
    }

    [Fact]
    public void ValidateModelFindings()
    {
        using var payload = JsonDocument.Parse(
            """
            {
              "findings": [
                {
                  "category": "style",
                  "severity": "low",
                  "file": "a.py",
                  "line": 2,
                  "title": "Long line",
                  "details": "Long",
                  "suggestion": "Wrap it"
                }
              ]
            }
            """);

        var findings = ReviewEngine.ValidateModelReview(payload.RootElement);

        Assert.Equal(2, findings[0].Line);
    }

    [Fact]
    public void RejectInvalidModelFinding()
    {
        using var payload = JsonDocument.Parse("""{"findings":[{"category":"unknown"}]}""");

        Assert.Throws<ArgumentException>(() =>
            ReviewEngine.ValidateModelReview(payload.RootElement));
    }
}
