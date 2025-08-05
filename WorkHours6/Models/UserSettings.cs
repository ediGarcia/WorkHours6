using System;
using WorkHours6.Services;

namespace WorkHours6.Models;

public class UserSettings
{
    #region Properties

    /// <summary>
    /// Gets or sets the first date considered for the balance.
    /// </summary>
    public DateTime BalanceStartDate { get; set; }

    /// <summary>
    /// Gets or sets whether the extra hours should be calculated.
    /// </summary>
    public bool CalculateExtraHours { get; set; }

    /// <summary>
    /// Gets or sets whether the current application show be always above other windows.
    /// </summary>
    public bool IsTopMost { get; set; } = true;

    /// <summary>
    /// Gets or sets the target balance.
    /// </summary>
    public TimeSpan TargetBalance { get; set; }

    /// <summary>
    /// Gets or sets the target balance deadline.
    /// </summary>
    public DateTime TargetBalanceDeadline { get; set; }

    #endregion

    public UserSettings()
    {
        UserSettingsDatabaseEntry entry = DataService.GetBalanceSettings();

        BalanceStartDate = entry.BalanceStartDate;
        CalculateExtraHours = entry.CalculateExtraHours;
        TargetBalance = entry.TargetBalance;
        TargetBalanceDeadline = entry.TargetBalanceDeadline;
    }
}
