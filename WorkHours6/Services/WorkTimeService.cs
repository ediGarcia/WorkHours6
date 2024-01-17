using HelperExtensions;
using HelperMethods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WorkHours6.Models;

namespace WorkHours6.Services;

public static class WorkTimeService
{
    #region Enums

    /// <summary>
    /// Sets the next Stopwatch action.
    /// </summary>
    private enum StopwatchAction
    {
        Run,
        Stop,
        Reset
    }

    #endregion

    #region Properties

    /// <summary>
    /// The work hours model.
    /// </summary>
    public static WorkTime WorkTime { get; } = new();

    #endregion

    public static readonly TimeSpan EightHours = TimeSpan.FromHours(8);
    // ReSharper disable once NotAccessedField.Local
    private static readonly Timer NotificationTimer; // Prevents the timer from being collected by the GC.
    private static readonly Stopwatch Stopwatch = new();

    private static TimeSpan _additionalWorkTime;
    private static DateTime _currentDate = DateTime.Today;

    static WorkTimeService()
    {
        try
        {
            TimeDatabaseEntry databaseEntry = DataService.GetTimeEntry(DateTime.Today);

            _additionalWorkTime = databaseEntry.WorkedTime;
            WorkTime.CreditedHours = databaseEntry.CreditedHours;

            if (databaseEntry.LastStartTime.HasValue)
                ToggleTimerWithoutSaving(databaseEntry.LastStartTime);
        }
        catch { }

        NotificationTimer = new(NotificationTimer_Elapsed, null, 0, 1000);
    }

    #region Events

    #region NotificationTimer_Elapsed
    /// <summary>
    /// Notifies time changes every one second.
    /// </summary>
    /// <param name="state"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void NotificationTimer_Elapsed(object? state)
    {
        WorkTime.WorkedHours = Stopwatch.Elapsed + _additionalWorkTime;

        if (_currentDate != DateTime.Today)
        {
            if (WorkTime.IsTimerRunning)
                ToggleTimer();

            _currentDate = DateTime.Today;
            ResetTimer();
        }
    }
    #endregion

    #endregion

    #region Public Methods

    #region ResetTimer
    /// <summary>
    /// Resets the time for the current day.
    /// </summary>
    public static void ResetTimer()
    {
        ChangeStopwatchState(StopwatchAction.Reset);
        _additionalWorkTime = TimeSpan.Zero;
        WorkTime.LastStateChangeTime = DateTime.Today;

        SaveData();
    }
    #endregion

    #region ToggleTimer
    /// <summary>
    /// Alternates the timer state.
    /// </summary>
    /// <param name="time"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void ToggleTimer(DateTime? time = null)
    {
        ToggleTimerWithoutSaving(time);
        SaveData();
    }
    #endregion

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

            balance =
                DateTimeMethods.RoundToMinutes(
                    entries
                        .Sum(_ => _.WorkedTime + _.CreditedHours - EightHours));
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
            int workDaysCount = DataService
                .GetTimeEntries(DateTime.Today.AddDays(1), userSettings.TargetBalanceDeadline)
                .Count(_ => _.CreditedHours < EightHours);

            extraHours =
                DateTimeMethods.RoundToMinutes(
                    (userSettings.TargetBalance - WorkTime.Balance) / Int32.Max(workDaysCount, 1));
        }

        WorkTime.ExtraHours = extraHours;
    }
    #endregion

    #endregion

    #region Private Methods

    #region ChangeStopwatchState
    /// <summary>
    /// Changes the stopwatch state.
    /// </summary>
    /// <param name="action"></param>
    private static void ChangeStopwatchState(StopwatchAction action)
    {
        switch (action)
        {
            case StopwatchAction.Run:
                Stopwatch.Start();
                break;

            case StopwatchAction.Stop:
                Stopwatch.Stop();
                break;

            case StopwatchAction.Reset:
                Stopwatch.Reset();
                break;
        }

        WorkTime.IsTimerRunning = Stopwatch.IsRunning;
    }
    #endregion

    #region SaveData
    /// <summary>
    /// Saves the time data to the database.
    /// </summary>
    private static void SaveData() =>
        DataService.SaveTimeData(_currentDate, WorkTime.WorkedHours, WorkTime.CreditedHours, WorkTime.LastStateChangeTime, WorkTime.IsTimerRunning);
    #endregion

    #region ToggleTimerWithoutSaving
    /// <summary>
    /// Toggles the timer without saving the updated data to the database.
    /// </summary>
    /// <param name="time"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void ToggleTimerWithoutSaving(DateTime? time = null)
    {
        TimeSpan timeBalance = TimeSpan.Zero;

        if (time.HasValue)
        {
            if (time.Value.Date != DateTime.Today)
                throw new ArgumentException("The specified time does not match the current date.", nameof(time));

            if (time > DateTime.Now)
                throw new ArgumentException("The specified time must be in the past.", nameof(time));

            if (time < WorkTime.LastStateChangeTime)
                throw new ArgumentException($"The specified time cannot be set before {WorkTime.LastStateChangeTime:t}.");

            timeBalance = DateTimeMethods.RoundToSeconds(DateTime.Now - time.Value);
        }

        if (WorkTime.IsTimerRunning)
        {
            ChangeStopwatchState(StopwatchAction.Stop);
            _additionalWorkTime -= timeBalance;
        }
        else
        {
            ChangeStopwatchState(StopwatchAction.Run);
            _additionalWorkTime += timeBalance;
        }

        WorkTime.LastStateChangeTime = time ?? DateTime.Now;
    }
    #endregion

    #endregion
}
