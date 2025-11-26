namespace VHS.Services.Common.Enums.DateTimeFormats;

public enum DateTimeFormatEnum
{
    // --- European Style (Day first) ---

    [System.ComponentModel.Description("dd-MM-yyyy HH:mm")]
    DayMonthYear24h,

    [System.ComponentModel.Description("dd-MMM-yyyy HH:mm")]
    DayMonthAbbrYear24h,

    [System.ComponentModel.Description("d-M-yyyy H:mm")]
    DayMonthYear24h_NoLeadZero,

    [System.ComponentModel.Description("dd-MM-yy HH:mm")]
    DayMonthYear24h_yy,

    [System.ComponentModel.Description("dd-MMM-yy HH:mm")]
    DayMonthAbbrYear24h_yy,

    [System.ComponentModel.Description("d-M-yy H:mm")]
    DayMonthYear24h_NoLeadZero_yy,

    // --- US Style (Month first) ---

    [System.ComponentModel.Description("MM/dd/yyyy hh:mm tt")]
    MonthDayYear12h,

    [System.ComponentModel.Description("MMM d, yyyy h:mm tt")]
    MonthAbbrDayYear12h,

    [System.ComponentModel.Description("M/d/yyyy h:mm tt")]
    MonthDayYear12h_NoLeadZero,

    [System.ComponentModel.Description("MMMM d, yyyy h:mm tt")]
    MonthFullDayYear12h,

    [System.ComponentModel.Description("MM/dd/yy hh:mm tt")]
    MonthDayYear12h_yy,

    [System.ComponentModel.Description("MMM d, yy h:mm tt")]
    MonthAbbrDayYear12h_yy,

    [System.ComponentModel.Description("M/d/yy h:mm tt")]
    MonthDayYear12h_NoLeadZero_yy,

    [System.ComponentModel.Description("MMMM d, yy h:mm tt")]
    MonthFullDayYear12h_yy,

    // --- ISO Style (Year first) ---

    [System.ComponentModel.Description("yyyy-MM-dd HH:mm")]
    YearMonthDay24h,

    [System.ComponentModel.Description("yy-MM-dd HH:mm")]
    YearMonthDay24h_yy,

    // --- Full Date Formats ---

    [System.ComponentModel.Description("dddd, d MMMM yyyy HH:mm")]
    FullDateLong24h,

    [System.ComponentModel.Description("dddd, MMMM d, yyyy h:mm tt")]
    FullDateLong12h,

    [System.ComponentModel.Description("ddd, d MMM yyyy HH:mm")]
    FullDateShort24h,

    [System.ComponentModel.Description("ddd, MMM d, yyyy h:mm tt")]
    FullDateShort12h,

    [System.ComponentModel.Description("dddd, d MMMM yy HH:mm")]
    FullDateLong24h_yy,

    [System.ComponentModel.Description("dddd, MMMM d, yy h:mm tt")]
    FullDateLong12h_yy,

    [System.ComponentModel.Description("ddd, d MMM yy HH:mm")]
    FullDateShort24h_yy,

    [System.ComponentModel.Description("ddd, MMM d, yy h:mm tt")]
    FullDateShort12h_yy,
}
