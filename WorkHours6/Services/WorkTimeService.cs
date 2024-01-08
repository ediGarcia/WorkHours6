using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using HelperMethods;
using WorkHours6.Models;

namespace WorkHours6.Services;

/// <summary>
/// Service that manages the worked time.
/// </summary>
public class WorkTimeService : INotifyPropertyChanged
{
    #region Custom Events

    /// <summary>
    /// Notifies when a property's value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the credited work hours for the current day.
    /// </summary>
    public TimeSpan CreditedHours
    {
        get => _creditedHours;
        set
        {
            _creditedHours = value;

            PropertyChanged?.Invoke(this, new(nameof(CreditedHours)));
            PropertyChanged?.Invoke(this, new(nameof(ExpectedWorkHours)));
            PropertyChanged?.Invoke(this, new(nameof(ExpectedExitTime)));
        }
    }

    /// <summary>
    /// Gets the expected exit time for the day.
    /// </summary>
    public DateTime ExpectedExitTime => DateTime.Now + ExpectedWorkHours - WorkedHours;

    /// <summary>
    /// Gets the expected work hours.
    /// </summary>
    public TimeSpan ExpectedWorkHours => EightHours - CreditedHours + ExtraHours;

    /// <summary>
    /// Gets or sets the calculated extra hours.
    /// </summary>
    public TimeSpan ExtraHours
    {
        get => _extraHours;
        set
        {
            _extraHours = value;

            PropertyChanged?.Invoke(this, new(nameof(ExtraHours)));
            PropertyChanged?.Invoke(this, new(nameof(ExpectedWorkHours)));
            PropertyChanged?.Invoke(this, new(nameof(ExpectedExitTime)));
        }
    }

    /// <summary>
    /// Indicates whether the current timer is running.
    /// </summary>
    public bool IsTimerRunning => _stopwatch.IsRunning;

    /// <summary>
    /// Gets the last time the timer state has changed.
    /// </summary>
    public DateTime LastStateChangeTime
    {
        get => _lastStateTimeChanged;
        private set
        {
            _lastStateTimeChanged = value;
            PropertyChanged?.Invoke(this, new(nameof(LastStateChangeTime)));
        }
    }

    /// <summary>
    /// Gets the worked hours for the day.
    /// </summary>
    public TimeSpan WorkedHours => _stopwatch.Elapsed + _additionalWorkTime;

    #endregion

    public static readonly TimeSpan EightHours = TimeSpan.FromHours(8);

    private TimeSpan _additionalWorkTime;
    private static DateTime _currentDate = DateTime.Today;
    private TimeSpan _creditedHours;
    private TimeSpan _extraHours;
    private DateTime _lastStateTimeChanged = DateTime.Today;
    // ReSharper disable once NotAccessedField.Local
    private readonly Timer _notificationTimer; // Prevents the timer from being collected by the GC.
    private readonly Stopwatch _stopwatch = new();

    public WorkTimeService()
    {
        try
        {
            TimeDatabaseEntry databaseEntry = DataService.GetTimeEntry(DateTime.Today);

            _additionalWorkTime = databaseEntry.WorkedTime;
            _creditedHours = databaseEntry.CreditedHours;

            if (databaseEntry.LastStartTime.HasValue)
                ToggleTimer(databaseEntry.LastStartTime);
        }
        catch { }

        _notificationTimer = new(NotificationTimer_Elapsed, null, 0, 1000);
    }

    #region Events

    #region NotificationTimer_Elapsed
    /// <summary>
    /// Notifies time changes every one second.
    /// </summary>
    /// <param name="state"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void NotificationTimer_Elapsed(object? state)
    {
        PropertyChanged?.Invoke(this, new(nameof(ExpectedExitTime)));
        PropertyChanged?.Invoke(this, new(nameof(WorkedHours)));

        if (_currentDate != DateTime.Today)
        {
            bool isTimerRunning = IsTimerRunning;

            SaveData();
            _currentDate = DateTime.Today;
            ResetTimer();

            if (isTimerRunning)
                ToggleTimer();
        }
    }
    #endregion

    #endregion

    #region Public Methods

    #region ResetTimer
    /// <summary>
    /// Resets the time for the current day.
    /// </summary>
    public void ResetTimer()
    {
        _stopwatch.Reset();
        _additionalWorkTime = TimeSpan.Zero;
        LastStateChangeTime = DateTime.Today;
        PropertyChanged?.Invoke(this, new(nameof(IsTimerRunning)));

        SaveData();
    }
    #endregion

    #region ToggleTimer
    /// <summary>
    /// Alternates the timer state.
    /// </summary>
    /// <param name="time"></param>
    /// <exception cref="ArgumentException"></exception>
    public void ToggleTimer(DateTime? time = null)
    {
        TimeSpan timeBalance = TimeSpan.Zero;

        if (time.HasValue)
        {
            if (time.Value.Date != DateTime.Today)
                throw new ArgumentException("The specified time does not match the current date.", nameof(time));

            if (time > DateTime.Now)
                throw new ArgumentException("The specified time must be in the past.", nameof(time));

            if (time < LastStateChangeTime)
                throw new ArgumentException($"The specified time cannot be set before {LastStateChangeTime:t}.");

            timeBalance = DateTimeMethods.RoundToSeconds(DateTime.Now - time.Value);
        }

        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
            _additionalWorkTime -= timeBalance;
        }
        else
        {
            _stopwatch.Start();
            _additionalWorkTime += timeBalance;
        }

        SaveData();
        LastStateChangeTime = time ?? DateTime.Now;
        PropertyChanged?.Invoke(this, new(nameof(IsTimerRunning)));
    }
    #endregion

    #region UpdateCreditedHours
    /// <summary>
    /// Updates the credited hours for the current day.
    /// </summary>
    public void UpdateCreditedHours()
    {
        try
        {
            TimeDatabaseEntry databaseEntry = DataService.GetTimeEntry(DateTime.Today);

            if (databaseEntry.CreditedHours != CreditedHours)
                CreditedHours = databaseEntry.CreditedHours;
        }
        catch { }
    }
    #endregion

    #endregion

    #region Private Methods

    #region SaveData
    /// <summary>
    /// Saves the time data to the database.
    /// </summary>
    private void SaveData() =>
        DataService.SaveTimeData(_currentDate, WorkedHours, CreditedHours, IsTimerRunning);
    #endregion

    #endregion
}
