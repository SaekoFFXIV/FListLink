using EasySocialMediaPlugin;

var cases = new[]
{
    new TestCase("/c/", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("RP profile: /c", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("visit c/Foo", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("visit /c/Foo", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("visit c/Foo_Bar", "Jane Doe", true, "https://f-list.net/c/Foo_Bar"),
    new TestCase("visit /c/Foo-Bar", "Jane Doe", true, "https://f-list.net/c/Foo-Bar"),
    new TestCase("https://f-list.net/c/Foo%20Bar", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("c/Foo123", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("c/♥", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("c/", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("/c/ RP-friendly", "Jane Doe", true, "https://f-list.net/c/Jane%20Doe"),
    new TestCase("No profile marker here", "Jane Doe", false, string.Empty),
    new TestCase("abc/c/not-a-marker", "Jane Doe", false, string.Empty),
    new TestCase("/c/", "B'eta Hera", true, "https://f-list.net/c/Beta%20Hera"),
    new TestCase("RP profile: /c", "N'arita Tia", true, "https://f-list.net/c/Narita%20Tia"),
};

var failures = 0;
foreach (var test in cases)
{
    var resolved = FListProfileResolver.TryResolve(test.SearchComment, test.PlayerName, out var url);
    if (resolved == test.ExpectedResolved && url == test.ExpectedUrl)
    {
        Console.WriteLine($"PASS: {test.SearchComment}");
        continue;
    }

    failures++;
    Console.Error.WriteLine(
        $"FAIL: {test.SearchComment} => resolved={resolved}, url={url}; expected resolved={test.ExpectedResolved}, url={test.ExpectedUrl}");
}

return failures == 0 ? 0 : 1;

internal sealed record TestCase(
    string SearchComment,
    string PlayerName,
    bool ExpectedResolved,
    string ExpectedUrl);
