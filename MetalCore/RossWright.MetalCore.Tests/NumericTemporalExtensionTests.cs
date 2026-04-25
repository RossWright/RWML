namespace RossWright;

public class NullableBoolExtensionTests
{
    [Fact] public void IsNullOrTrue_Null_ReturnsTrue() => ((bool?)null).IsNullOrTrue().ShouldBeTrue();
    [Fact] public void IsNullOrTrue_True_ReturnsTrue() => ((bool?)true).IsNullOrTrue().ShouldBeTrue();
    [Fact] public void IsNullOrTrue_False_ReturnsFalse() => ((bool?)false).IsNullOrTrue().ShouldBeFalse();

    [Fact] public void IsNullOrFalse_Null_ReturnsTrue() => ((bool?)null).IsNullOrFalse().ShouldBeTrue();
    [Fact] public void IsNullOrFalse_False_ReturnsTrue() => ((bool?)false).IsNullOrFalse().ShouldBeTrue();
    [Fact] public void IsNullOrFalse_True_ReturnsFalse() => ((bool?)true).IsNullOrFalse().ShouldBeFalse();
}

public class IntClampTests
{
    [Fact] public void Clamp_WithinRange_ReturnsValue() => 5.Clamp(1, 10).ShouldBe(5);
    [Fact] public void Clamp_BelowMin_ReturnsMin() => 0.Clamp(1, 10).ShouldBe(1);
    [Fact] public void Clamp_AboveMax_ReturnsMax() => 15.Clamp(1, 10).ShouldBe(10);
    [Fact] public void Clamp_OnlyMin_NullMax_NoUpperLimit() => 100.Clamp(1, null).ShouldBe(100);
    [Fact] public void Clamp_OnlyMax_NullMin_NoLowerLimit() => (-100).Clamp(null, 10).ShouldBe(-100);
    [Fact] public void Clamp_BothNull_ReturnsValue() => 42.Clamp(null, null).ShouldBe(42);
}

public class DoubleExtensionTests
{
    // ── NullIfNotReal ─────────────────────────────────────────────────────────────
    [Fact] public void NullIfNotReal_NaN_ReturnsNull() => double.NaN.NullIfNotReal().ShouldBeNull();
    [Fact] public void NullIfNotReal_PositiveInfinity_ReturnsNull() => double.PositiveInfinity.NullIfNotReal().ShouldBeNull();
    [Fact] public void NullIfNotReal_NegativeInfinity_ReturnsNull() => double.NegativeInfinity.NullIfNotReal().ShouldBeNull();
    [Fact] public void NullIfNotReal_NormalValue_ReturnsSameValue() => (5.0).NullIfNotReal().ShouldBe(5.0);

    // ── ToAccountingString ────────────────────────────────────────────────────────
    [Fact] public void ToAccountingString_Zero_ReturnsDash() => (0.0).ToAccountingString().ShouldBe("-");
    [Fact] public void ToAccountingString_Positive_NoBrackets() => (1234.56).ToAccountingString().ShouldNotContain("(");
    [Fact] public void ToAccountingString_Negative_HasBrackets() => (-1234.56).ToAccountingString().ShouldContain("(");
    [Fact] public void ToAccountingString_MatchesFormatString() =>
        (1234.56).ToAccountingString().ShouldBe((1234.56).ToString("#,##0.00;(#,##0.00);-"));

    // ── Degrees / Radians ─────────────────────────────────────────────────────────
    [Fact] public void FromDegreesToRadians_Zero_ReturnsZero() => (0.0).FromDegreesToRadians().ShouldBe(0.0, 1e-10);
    [Fact] public void FromDegreesToRadians_90_ReturnsHalfPi() => (90.0).FromDegreesToRadians().ShouldBe(Math.PI / 2, 1e-10);
    [Fact] public void FromDegreesToRadians_180_ReturnsPi() => (180.0).FromDegreesToRadians().ShouldBe(Math.PI, 1e-10);
    [Fact] public void FromDegreesToRadians_360_ReturnsTwoPi() => (360.0).FromDegreesToRadians().ShouldBe(2 * Math.PI, 1e-10);

    [Fact] public void FromRadiansToDegrees_Zero_ReturnsZero() => (0.0).FromRadiansToDegrees().ShouldBe(0.0, 1e-10);
    [Fact] public void FromRadiansToDegrees_HalfPi_Returns90() => (Math.PI / 2).FromRadiansToDegrees().ShouldBe(90.0, 1e-10);
    [Fact] public void FromRadiansToDegrees_Pi_Returns180() => Math.PI.FromRadiansToDegrees().ShouldBe(180.0, 1e-10);

    // ── Clamp ─────────────────────────────────────────────────────────────────────
    [Fact] public void DoubleClamp_WithinRange_ReturnsValue() => (5.0).Clamp(1.0, 10.0).ShouldBe(5.0);
    [Fact] public void DoubleClamp_BelowMin_ReturnsMin() => (0.0).Clamp(1.0, 10.0).ShouldBe(1.0);
    [Fact] public void DoubleClamp_AboveMax_ReturnsMax() => (15.0).Clamp(1.0, 10.0).ShouldBe(10.0);
    [Fact] public void DoubleClamp_BothNull_ReturnsValue() => (42.0).Clamp(null, null).ShouldBe(42.0);
    // Bug #1 — null max/min fell back to int.MinValue/int.MaxValue instead of double bounds
    [Fact] public void DoubleClamp_NullMax_ValueBeyondIntMaxValue_PassesThrough() =>
        (3_000_000_000.0).Clamp(null, null).ShouldBe(3_000_000_000.0);
    [Fact] public void DoubleClamp_NullMin_ValueBeyondIntMinValue_PassesThrough() =>
        (-3_000_000_000.0).Clamp(null, null).ShouldBe(-3_000_000_000.0);
    [Fact] public void DoubleClamp_NullMax_ClampsToExplicitMax_AboveIntMaxValue() =>
        (5_000_000_000.0).Clamp(null, 3_000_000_000.0).ShouldBe(3_000_000_000.0);

    // ── Downsample ────────────────────────────────────────────────────────────────
    [Fact] public void Downsample_LargerThanSampleCount_ReturnsReducedArray()
    {
        var source = Enumerable.Range(0, 100).Select(i => (double)i).ToArray();
        var result = source.Downsample(10);
        result.Length.ShouldBe(10);
    }

    [Fact] public void Downsample_ExactlySampleCount_ReturnsOriginalArray()
    {
        var source = Enumerable.Range(0, 10).Select(i => (double)i).ToArray();
        var result = source.Downsample(10);
        result.ShouldBeSameAs(source);
    }

    [Fact] public void Downsample_SmallerThanSampleCount_ReturnsOriginalArray()
    {
        var source = new[] { 1.0, 2.0, 3.0 };
        var result = source.Downsample(10);
        result.ShouldBeSameAs(source);
    }

    // ── StandardDeviation ─────────────────────────────────────────────────────────
    [Fact] public void StandardDeviation_KnownInput_ReturnsExpectedValue()
    {
        // mean=5, variance=4, stddev=2
        var values = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };
        values.StandardDeviation().ShouldBe(2.0, 1e-10);
    }

    [Fact] public void StandardDeviation_SingleElement_ReturnsZero()
    {
        new[] { 42.0 }.StandardDeviation().ShouldBe(0.0, 1e-10);
    }

    [Fact] public void StandardDeviation_EmptySequence_ThrowsInvalidOperationException()
        => Should.Throw<InvalidOperationException>(() => Array.Empty<double>().StandardDeviation());

    // ── P3-A: Downsample nullable overload ────────────────────────────────────
    [Fact] public void Downsample_NullableArray_NoNulls_MatchesNonNullableResult()
    {
        var nullableResult = new double?[] { 1.0, 2.0, 3.0, 4.0 }.Downsample(2);
        var nonNullableResult = new double[] { 1.0, 2.0, 3.0, 4.0 }.Downsample(2);
        nullableResult.Length.ShouldBe(nonNullableResult.Length);
        for (var i = 0; i < nullableResult.Length; i++)
            nullableResult[i].ShouldBe(nonNullableResult[i]);
    }

    [Fact] public void Downsample_NullableArray_WithNullValues_ProducesExpectedBucketCount()
    {
        var result = new double?[] { 1.0, null, 3.0, null }.Downsample(2);
        result.Length.ShouldBe(2);
    }

    [Fact] public void Downsample_NullableArray_AllNulls_ResultLengthMatchesBuckets()
    {
        var result = new double?[] { null, null, null, null }.Downsample(2);
        result.Length.ShouldBe(2);
    }
}

public class TimeSpanExtensionTests
{
    [Fact] public void ToRelativeTime_MoreThan365Days_ReturnsYears() =>
        TimeSpan.FromDays(2 * 365).ToRelativeTime().ShouldBe("2 years");

    [Fact] public void ToRelativeTime_MoreThan14Days_ReturnsWeeks() =>
        TimeSpan.FromDays(21).ToRelativeTime().ShouldBe("3 weeks");

    [Fact] public void ToRelativeTime_MoreThan24Hours_ReturnsDays() =>
        TimeSpan.FromHours(48).ToRelativeTime().ShouldBe("2 days");

    [Fact] public void ToRelativeTime_MoreThan1Hour_ReturnsHours() =>
        TimeSpan.FromHours(2).ToRelativeTime().ShouldBe("2 hours");

    [Fact] public void ToRelativeTime_MoreThan1Minute_ReturnsMinutes() =>
        TimeSpan.FromMinutes(2).ToRelativeTime().ShouldBe("2 minutes");

    [Fact] public void ToRelativeTime_MoreThan1Second_ReturnsSeconds() =>
        TimeSpan.FromSeconds(2).ToRelativeTime().ShouldBe("2 seconds");

    [Fact] public void ToRelativeTime_LessThan1Second_ReturnsMilliseconds() =>
        TimeSpan.FromMilliseconds(500).ToRelativeTime().ShouldBe("500 milliseconds");
}

public class DateTimeExtensionTests
{
    // ── ToRelativeTime ────────────────────────────────────────────────────────────
    [Fact] public void DateTime_ToRelativeTime_JustNow_ReturnsJustNow() =>
        DateTime.UtcNow.AddSeconds(-30).ToRelativeTime().ShouldBe("Just now");

    [Fact] public void DateTime_ToRelativeTime_AMinuteAgo() =>
        DateTime.UtcNow.AddSeconds(-90).ToRelativeTime().ShouldBe("A minute ago");

    [Fact] public void DateTime_ToRelativeTime_XMinutesAgo() =>
        DateTime.UtcNow.AddMinutes(-30).ToRelativeTime().ShouldBe("30 minutes ago");

    [Fact] public void DateTime_ToRelativeTime_AnHourAgo() =>
        DateTime.UtcNow.AddMinutes(-90).ToRelativeTime().ShouldBe("An hour ago");

    [Fact] public void DateTime_ToRelativeTime_XHoursAgo() =>
        DateTime.UtcNow.AddHours(-6).ToRelativeTime().ShouldBe("6 hours ago");

    [Fact] public void ToRelativeTime_SameDayMoreThan12HoursAgo_ReturnsTodayAtTime()
    {
        var dt = DateTime.Today.ToUniversalTime(); // midnight local today; always "today" in local time
        if ((DateTime.UtcNow - dt).TotalHours < 12) return; // before noon: "Today at" branch not yet reachable
        dt.ToRelativeTime().ShouldStartWith("Today at ");
    }

    // ── ToLocalShortDateTimeString ─────────────────────────────────────────────────
    [Fact] public void ToLocalShortDateTimeString_FormatMatchesLocalTime()
    {
        var dt = new DateTime(2025, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        var local = dt.ToLocalTime();
        var expected = $"{local.ToShortDateString()} {local.ToShortTimeString()}";
        dt.ToLocalShortDateTimeString().ShouldBe(expected);
    }

    // ── ToShortDateTimeString ──────────────────────────────────────────────────────
    [Fact] public void ToShortDateTimeString_FormatMatchesShortDateAndLocalTime()
    {
        var dt = new DateTime(2025, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        var expected = $"{dt.ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}";
        dt.ToShortDateTimeString().ShouldBe(expected);
    }

    // ── DayOfWeek.Abbr ────────────────────────────────────────────────────────────
    [Fact] public void Abbr_Sunday_ReturnsSun() => DayOfWeek.Sunday.Abbr().ShouldBe("Sun");
    [Fact] public void Abbr_Monday_ReturnsMon() => DayOfWeek.Monday.Abbr().ShouldBe("Mon");
    [Fact] public void Abbr_Tuesday_ReturnsTue() => DayOfWeek.Tuesday.Abbr().ShouldBe("Tue");
    [Fact] public void Abbr_Wednesday_ReturnsWed() => DayOfWeek.Wednesday.Abbr().ShouldBe("Wed");
    [Fact] public void Abbr_Thursday_ReturnsThu() => DayOfWeek.Thursday.Abbr().ShouldBe("Thu");
    [Fact] public void Abbr_Friday_ReturnsFri() => DayOfWeek.Friday.Abbr().ShouldBe("Fri");
    [Fact] public void Abbr_Saturday_ReturnsSat() => DayOfWeek.Saturday.Abbr().ShouldBe("Sat");

    // ── P2-D: ToRelativeTime branches ─────────────────────────────────────────
    [Fact] public void ToRelativeTime_PreviousDay_ReturnsYesterdayAtTime()
    {
        // Anchor to local today so dt.ToLocalTime() always falls on local yesterday,
        // regardless of timezone offset. Age is at least 14 hours, safely past the
        // "X hours ago" branch (which only fires below 12 hours).
        var dt = DateTime.Today.ToUniversalTime().AddDays(-1).AddHours(10);
        dt.ToRelativeTime().ShouldStartWith("Yesterday at ");
    }

    [Fact] public void ToRelativeTime_ThreeDaysAgo_ReturnsDayOfWeekAtTime()
    {
        var dt = DateTime.UtcNow.AddDays(-3);
        var expectedPrefix = dt.ToLocalTime().DayOfWeek.Abbr() + " at ";
        dt.ToRelativeTime().ShouldStartWith(expectedPrefix);
    }

    [Fact] public void ToRelativeTime_TenDaysAgoSameYear_ReturnsMmmDAtTime()
    {
        var dt = DateTime.UtcNow.AddDays(-10);
        var result = dt.ToRelativeTime();
        result.ShouldContain(" at ");
        result.ShouldNotContain(",");
    }

    [Fact] public void ToRelativeTime_PriorYear_ReturnsMmmDYearTime()
    {
        var dt = DateTime.UtcNow.AddDays(-400);
        var result = dt.ToRelativeTime();
        result.ShouldContain(",");
        result.ShouldContain(dt.ToLocalTime().Year.ToString());
    }
}
