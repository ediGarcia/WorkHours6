using HelperExtensions;
using HelperMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkHours6.Models;

namespace WorkHours6.Services;

public static class WorkTimeService
{
    #region Properties

    /// <summary>
    /// The work hours model.
    /// </summary>
    public static WorkTime WorkTime { get; }

    #endregion

    static WorkTimeService()
    {
        try
        {
            WorkTime = new(DataService.GetTimeEntry(DateTime.Today));
        }
        catch
        {
            WorkTime = new(new(DateTime.Today));
        }

        WorkTime.TimerStateChanged += (_, _) => SaveData();
    }

    #region Public Methods

    #region UpdateCreditedHours
    /// <summary>
    /// Updates the credited hours for the current day.
    /// </summary>
    public static void UpdateCreditedHours()
    {
        try
        {
            TimeDatabaseEntry databaseEntry = DataService.GetTimeEntry(DateTime.Today);

            if (databaseEntry.CreditedHours != WorkTime.CreditedHours)
                WorkTime.CreditedHours = databaseEntry.CreditedHours;
        }
        catch { }
    }
    #endregion

    #region UpdateCurrentBalance
    /// <summary>
    /// Updates the current balance.
    /// </summary>
    /// <returns></returns>
    public static void UpdateCurrentBalance()
    {
        TimeSpan balance = TimeSpan.Zero;
        UserSettings userSettings = SettingsService.UserSettings;

        if (userSettings.BalanceStartDate < DateTime.Today)
        {
            List<TimeDatabaseEntry> entries =
                DataService.GetTimeEntries(userSettings.BalanceStartDate, DateTime.Today.AddDays(-1));
            TimeSpan eightHours = TimeSpan.FromHours(8);

            balance =
                DateTimeMethods.RoundToMinutes(
                    entries
                        .Sum(_ => _.WorkedTime + _.CreditedHours - eightHours));
        }

        WorkTime.Balance = balance;
    }
    #endregion

    #region UpdateExtraHours
    /// <summary>
    /// Updates the current needed extra hours.
    /// </summary>
    public static void UpdateExtraHours()
    {
        TimeSpan extraHours = TimeSpan.Zero;
        UserSettings userSettings = SettingsService.UserSettings;

        if (userSettings.CalculateExtraHours
            && WorkTime.Balance != userSettings.TargetBalance
            && userSettings.TargetBalanceDeadline >= DateTime.Today)
        {
            TimeSpan eightHours = TimeSpan.FromHours(8);
            int workDaysCount = DataService
                .GetTimeEntries(DateTime.Today.AddDays(1), userSettings.TargetBalanceDeadline)
                .Count(_ => _.CreditedHours < eightHours);

            extraHours =
                DateTimeMethods.RoundToMinutes(
                    (userSettings.TargetBalance - WorkTime.Balance) / Int32.Max(workDaysCount, 1));
        }

        WorkTime.ExtraHours = extraHours;
    }
    #endregion

    #endregion

    #region Private Methods

    #region SaveData
    /// <summary>
    /// Saves the time data to the database.
    /// </summary>
    private static void SaveData() =>
        DataService.SaveTimeData(WorkTime.CurrentDate, WorkTime.WorkedHours, WorkTime.CreditedHours, WorkTime.LastStateChangeTime, WorkTime.IsTimerRunning);
    #endregion

    #endregion
}
