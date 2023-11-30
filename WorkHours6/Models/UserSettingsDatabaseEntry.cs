using System;

namespace WorkHours6.Models;

public class UserSettingsDatabaseEntry
{
    #region Properties

    /// <summary>
    /// Gets or sets the first date considered for the balance.
    /// </summary>
    public DateTime BalanceStartDate { get; set; } = new(DateTime.Today.Year, 1, 1);

    /// <summary>
    /// Gets or sets whether the extra hours should be calculated.
    /// </summary>
    public bool CalculateExtraHours { get; set; }

    /// <summary>
    /// Gets or sets the number that identifies the database entry.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the target balance.
    /// </summary>
    public TimeSpan TargetBalance { get; set; }

    /// <summary>
    /// Gets or sets the target balance deadline.
    /// </summary>
    public DateTime TargetBalanceDeadline { get; set; } = DateTime.Today;

    #endregion
}