using HelperExtensions;
using System;

namespace WorkHours6.Models;

public class TimeDatabaseEntry
{
    #region Properties

    /// <summary>
    /// Gets and sets the credited time of the current time entry.
    /// </summary>
    public TimeSpan CreditedHours { get; set; }

    /// <summary>
    /// Gets or sets the date of the current time entry.
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// Gets and sets the ID of the current time entry.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets and sets the last update time of the current time entry.
    /// </summary>
    public DateTime? LastStartTime { get; set; }

    /// <summary>
    /// Gets and ste the stored worked hours of the current time entry.
    /// </summary>
    public TimeSpan WorkedTime { get; set; }

    #endregion

    public TimeDatabaseEntry(DateTime date)
    {
        Date = date.Date;

        if (Date.IsWeekend())
            CreditedHours = TimeSpan.FromHours(8);
    }

    #region Public Methods

    #region Clone
    /// <summary>
    /// Creates a new instance of <see cref="TimeDatabaseEntry"/> containing the same values of the current instance.
    /// </summary>
    /// <returns></returns>
    public TimeDatabaseEntry Clone() =>
        new(Date)
        {
            CreditedHours = CreditedHours,
            Id = Id,
            LastStartTime = LastStartTime,
            WorkedTime = WorkedTime
        };
    #endregion

    #endregion
}
