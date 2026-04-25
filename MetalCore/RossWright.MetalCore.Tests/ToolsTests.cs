using System.Text.Json;

namespace RossWright;

public class ParseOrNullTests
{
    // ── Bool ──────────────────────────────────────────────────────────────────────
    [Fact] public void Bool_ValidTrue_ReturnsTrue() => ParseOrNull.Bool("true").ShouldBe(true);
    [Fact] public void Bool_ValidFalse_ReturnsFalse() => ParseOrNull.Bool("false").ShouldBe(false);
    [Fact] public void Bool_Invalid_ReturnsNull() => ParseOrNull.Bool("yes").ShouldBeNull();
    [Fact] public void Bool_Null_ReturnsNull() => ParseOrNull.Bool(null).ShouldBeNull();
    [Fact] public void Bool_Whitespace_ReturnsNull() => ParseOrNull.Bool("   ").ShouldBeNull();

    // ── Int ───────────────────────────────────────────────────────────────────────
    [Fact] public void Int_ValidNumber_ReturnsInt() => ParseOrNull.Int("42").ShouldBe(42);
    [Fact] public void Int_Invalid_ReturnsNull() => ParseOrNull.Int("abc").ShouldBeNull();
    [Fact] public void Int_Null_ReturnsNull() => ParseOrNull.Int(null).ShouldBeNull();

    // ── Guid ──────────────────────────────────────────────────────────────────────
    [Fact] public void Guid_ValidGuid_ReturnsGuid()
    {
        var id = Guid.NewGuid();
        ParseOrNull.Guid(id.ToString()).ShouldBe(id);
    }
    [Fact] public void Guid_Invalid_ReturnsNull() => ParseOrNull.Guid("not-a-guid").ShouldBeNull();
    [Fact] public void Guid_Null_ReturnsNull() => ParseOrNull.Guid(null).ShouldBeNull();

    // ── Double ────────────────────────────────────────────────────────────────────
    [Fact] public void Double_ValidDouble_ReturnsDouble() => ParseOrNull.Double("3.14").ShouldBe(3.14);
    [Fact] public void Double_Invalid_ReturnsNull() => ParseOrNull.Double("not-a-number").ShouldBeNull();
    [Fact] public void Double_Null_ReturnsNull() => ParseOrNull.Double(null).ShouldBeNull();

    // ── DateTime ──────────────────────────────────────────────────────────────────
    [Fact] public void DateTime_ValidDate_ReturnsDateTime()
        => ParseOrNull.DateTime("2025-01-15").ShouldNotBeNull();
    [Fact] public void DateTime_Invalid_ReturnsNull() => ParseOrNull.DateTime("not-a-date").ShouldBeNull();
    [Fact] public void DateTime_Null_ReturnsNull() => ParseOrNull.DateTime(null).ShouldBeNull();

    // ── DateOnly ──────────────────────────────────────────────────────────────────
    [Fact] public void DateOnly_ValidDate_ReturnsDateOnly()
        => ParseOrNull.DateOnly("2025-01-15").ShouldBe(new DateOnly(2025, 1, 15));
    [Fact] public void DateOnly_Invalid_ReturnsNull() => ParseOrNull.DateOnly("invalid").ShouldBeNull();
    [Fact] public void DateOnly_Null_ReturnsNull() => ParseOrNull.DateOnly(null).ShouldBeNull();
}

public class CombineUrlTests
{
    [Fact] public void CombineUrl_TrimsSlashesAndJoins() =>
        Tools.CombineUrl("https://example.com/", "/api/", "/users").ShouldBe("https://example.com/api/users");

    [Fact] public void CombineUrl_SkipsNullSegments() =>
        Tools.CombineUrl("a", null!, "b").ShouldBe("a/b");

    [Fact] public void CombineUrl_SkipsEmptySegments() =>
        Tools.CombineUrl("a", "", "b").ShouldBe("a/b");

    [Fact] public void CombineUrl_NoSegments_ReturnsEmpty() =>
        Tools.CombineUrl().ShouldBe(string.Empty);

    [Fact] public void CombineUrl_AllNullSegments_ReturnsEmpty() =>
        Tools.CombineUrl(null!, null!).ShouldBe(string.Empty);
}

public class BuildQueryTests
{
    [Fact] public void BuildQuery_NonNullParams_AppendedToUrl()
    {
        var result = Tools.BuildQuery("https://example.com/api", ("name", "Alice"), ("age", "30"));
        result.ShouldContain("name=Alice");
        result.ShouldContain("age=30");
    }

    [Fact] public void BuildQuery_NullParamsSkipped()
    {
        var result = Tools.BuildQuery("https://example.com/api", ("name", "Alice"), ("skip", null));
        result.ShouldContain("name=Alice");
        result.ShouldNotContain("skip");
    }

    [Fact] public void BuildQuery_NoParams_ReturnsOriginalUrl()
    {
        var result = Tools.BuildQuery("https://example.com/api");
        result.ShouldBe("https://example.com/api");
    }

    // Bug #6 — UriBuilder throws UriFormatException on relative URLs
    [Fact] public void BuildQuery_RelativeUrl_AppendsQueryString()
    {
        var result = Tools.BuildQuery("/api/items", ("page", 1), ("size", 10));
        result.ShouldContain("page=1");
        result.ShouldContain("size=10");
        result.ShouldStartWith("/api/items");
    }

    [Fact] public void BuildQuery_RelativeUrl_NoParams_ReturnsOriginalUrl()
    {
        var result = Tools.BuildQuery("/api/items");
        result.ShouldBe("/api/items");
    }
}

public class ColorToolsTests
{
    [Fact] public void GetLighterColor_ValidHex_ReturnsHexString()
    {
        var result = Tools.GetLighterColor("#336699");
        result.ShouldNotBeNull();
        result.ShouldStartWith("#");
        result.Length.ShouldBe(7);
    }

    [Fact] public void GetLighterColor_InvalidHex_ThrowsArgumentException() =>
        Should.Throw<ArgumentException>(() => Tools.GetLighterColor("notahex"));

    [Fact] public void GetDesaturatedColor_ValidHex_ReturnsHexString()
    {
        var result = Tools.GetDesaturatedColor("#FF6600");
        result.ShouldNotBeNull();
        result.ShouldStartWith("#");
        result.Length.ShouldBe(7);
    }

    [Fact] public void GetDesaturatedColor_InvalidHex_ThrowsArgumentException() =>
        Should.Throw<ArgumentException>(() => Tools.GetDesaturatedColor("invalid"));

    [Fact] public void GetDarkerColor_ValidHex_ReturnsHexString()
    {
        var result = Tools.GetDarkerColor("#336699");
        result.ShouldNotBeNull();
        result.ShouldStartWith("#");
        result.Length.ShouldBe(7);
    }

    [Fact] public void GetDarkerColor_InvalidHex_ThrowsArgumentException() =>
        Should.Throw<ArgumentException>(() => Tools.GetDarkerColor("notahex"));

    [Fact] public void GetSaturatedColor_ValidHex_ReturnsHexString()
    {
        var result = Tools.GetSaturatedColor("#336699");
        result.ShouldNotBeNull();
        result.ShouldStartWith("#");
        result.Length.ShouldBe(7);
    }

    [Fact] public void GetSaturatedColor_InvalidHex_ThrowsArgumentException() =>
        Should.Throw<ArgumentException>(() => Tools.GetSaturatedColor("notahex"));
}

public class JsonFormatterTests
{
    [Fact] public void Format_CompactJson_ReturnsIndented()
    {
        var result = JsonFormatter.Format("{\"a\":1,\"b\":2}");
        result.ShouldContain("\n");
        result.ShouldContain("\"a\"");
    }

    [Fact] public void Format_AlreadyPrettyJson_ReturnsIndented()
    {
        var pretty = "{\n  \"a\": 1\n}";
        var result = JsonFormatter.Format(pretty);
        result.ShouldContain("\"a\"");
    }

    [Fact] public void Format_InvalidJson_ThrowsJsonException() =>
        Should.Throw<JsonException>(() => JsonFormatter.Format("{not valid json"));
}

public class LoadGuardTests
{
    [Fact] public async Task Load_FirstCall_InvokesLoader()
    {
        var guard = new LoadGuard();
        int callCount = 0;
        await guard.Load("key", () => { callCount++; return Task.CompletedTask; });
        callCount.ShouldBe(1);
    }

    [Fact] public async Task Load_SecondConcurrentCall_ReturnsSameTaskWithoutInvokingTwice()
    {
        var guard = new LoadGuard();
        int callCount = 0;
        var tcs = new TaskCompletionSource();
        var task1 = guard.Load("key", () => { callCount++; return tcs.Task; });
        var task2 = guard.Load("key", () => { callCount++; return Task.CompletedTask; });
        task1.ShouldBeSameAs(task2);
        callCount.ShouldBe(1);
        tcs.SetResult();
        await Task.WhenAll(task1, task2);
    }

    [Fact] public async Task Load_WithinReloadThreshold_DoesNotInvokeAgain()
    {
        var guard = new LoadGuard { ReloadAfterSeconds = 60 };
        int callCount = 0;
        await guard.Load("key", () => { callCount++; return Task.CompletedTask; });
        await guard.Load("key", () => { callCount++; return Task.CompletedTask; });
        callCount.ShouldBe(1);
    }

    [Fact] public async Task Load_AfterReloadThresholdExpired_InvokesAgain()
    {
        var guard = new LoadGuard { ReloadAfterSeconds = 0 };
        int callCount = 0;
        await guard.Load("key", () => { callCount++; return Task.CompletedTask; });
        await guard.Load("key", () => { callCount++; return Task.CompletedTask; });
        callCount.ShouldBe(2);
    }
}

public class SecurityToolsTests
{
    [Fact] public void RandomString_ReturnsCorrectLength()
    {
        var result = SecurityTools.RandomString(16);
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact] public void RandomString_SuccessiveCallsDiffer()
    {
        var a = SecurityTools.RandomString(32);
        var b = SecurityTools.RandomString(32);
        a.ShouldNotBe(b);
    }

    [Fact] public void RandomNumber_ReturnsOnlyDigits()
    {
        var result = SecurityTools.RandomNumber(8);
        result.ShouldNotBeNull();
        result.ShouldAllBe(c => char.IsDigit(c));
    }

    [Fact] public void RandomNumber_ReturnsCorrectLength()
    {
        var result = SecurityTools.RandomNumber(6);
        result.Length.ShouldBe(6);
    }

    [Fact] public void RandomNumber_SuccessiveCallsDiffer()
    {
        var a = SecurityTools.RandomNumber(8);
        var b = SecurityTools.RandomNumber(8);
        a.ShouldNotBe(b);
    }

    // ── P2-E: Hash ────────────────────────────────────────────────────────────
    [Fact] public void Hash_WithText_ReturnsSaltAndHash()
    {
        var (salt, hash) = SecurityTools.Hash("mypassword");
        salt.ShouldNotBeNull();
        salt.ShouldNotBeEmpty();
        hash.ShouldNotBeNull();
        hash.ShouldNotBeEmpty();
    }

    [Fact] public void Hash_SameTextTwice_ProducesDifferentSalts()
    {
        var (salt1, _) = SecurityTools.Hash("mypassword");
        var (salt2, _) = SecurityTools.Hash("mypassword");
        salt1.ShouldNotBe(salt2);
    }

    [Fact] public void Hash_WithTextAndSalt_IsDeterministic()
    {
        var first = SecurityTools.Hash("mypassword", "mysalt");
        var second = SecurityTools.Hash("mypassword", "mysalt");
        first.ShouldBe(second);
    }

    [Fact] public void Hash_WithTextAndDifferentSalts_ProducesDifferentHashes()
    {
        var hash1 = SecurityTools.Hash("mypassword", "salt1");
        var hash2 = SecurityTools.Hash("mypassword", "salt2");
        hash1.ShouldNotBe(hash2);
    }

    [Fact] public void Hash_Roundtrip_TupleAndSaltOverloadAreCompatible()
    {
        var (salt, hash) = SecurityTools.Hash("mypassword");
        SecurityTools.Hash("mypassword", salt).ShouldBe(hash);
    }
}

public class ExceptionExtensionTests
{
    [Fact] public void ToBetterString_SimpleException_ContainsTypeAndMessage()
    {
        var ex = new InvalidOperationException("something went wrong");
        var result = ex.ToBetterString();
        result.ShouldContain("InvalidOperationException");
        result.ShouldContain("something went wrong");
    }

    [Fact] public void ToBetterString_ExceptionWithInner_ContainsInnerInfo()
    {
        var inner = new ArgumentException("inner message");
        var outer = new InvalidOperationException("outer message", inner);
        var result = outer.ToBetterString();
        result.ShouldContain("outer message");
        result.ShouldContain("inner message");
    }

    [Fact] public void ToBetterString_ExceptionWithStackTrace_ContainsStackTrace()
    {
        Exception? ex = null;
        try { throw new Exception("with stack"); }
        catch (Exception caught) { ex = caught; }
        var result = ex!.ToBetterString();
        result.ShouldContain("ToBetterString_ExceptionWithStackTrace");
    }
}
