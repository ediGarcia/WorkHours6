using HelperMethods;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using WorkHours6.Services;

#pragma warning disable CS8618

namespace WorkHours6.Models;

public class WorkTime : INotifyPropertyChanged
{
    #region Custom Events

    /// <summary>
    /// Notifies when a property's value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifies whether the times state is changed.
    /// </summary>
    public event EventHandler TimerStateChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets and sets the current work time balance.
    /// </summary>
    public TimeSpan Balance
    {
        get => _balance;
        set
        {
            _balance = value;
            PropertyChanged?.Invoke(this, new(nameof(Balance)));
        }
    }

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
    /// Retrieves the current work date.
    /// </summary>
    public DateTime CurrentDate
    {
        get => _currentDate;
        private set
        {
            _currentDate = value;
            PropertyChanged?.Invoke(this, new(nameof(CurrentDate)));
        }
    }

    /// <summary>
    /// Gets the expected exit time for the day.
    /// </summary>
    public DateTime ExpectedExitTime => DateTime.Now + ExpectedWorkHours - WorkedHours;

    /// <summary>
    /// Gets the expected work hours.
    /// </summary>
    public TimeSpan ExpectedWorkHours => _eightHours - CreditedHours + ExtraHours;

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
    public DateTime? LastStateChangeTime
    {
        get => _lastStateTimeChanged;
        set
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

    private TimeSpan _additionalWorkTime;
    private TimeSpan _balance;
    private TimeSpan _creditedHours;
    private DateTime _currentDate = DateTime.Today;
    private TimeSpan _extraHours;
    private readonly TimeSpan _eightHours = TimeSpan.FromHours(8);
    private DateTime? _lastStateTimeChanged = DateTime.Today;
    // ReSharper disable once NotAccessedField.Local
    private readonly Timer _notificationTimer; // Prevents the timer from being collected by the GC.
    private readonly Stopwatch _stopwatch = new();

    public WorkTime(TimeDatabaseEntry entry)
    {
        _additionalWorkTime = entry.WorkedTime;
        CreditedHours = entry.CreditedHours;

        if (entry.IsTimerEnabled)
            ToggleTimer(entry.LastStartTime);
        else
            LastStateChangeTime = entry.LastStartTime;

        _notificationTimer = new(NotificationTimer_Elapsed, null, 0, 1000);
    }

    #region Event

    #region NotificationTimer_Elapsed
    /// <summary>
    /// Notifies time changes every one second.
    /// </summary>
    /// <param name="state"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void NotificationTimer_Elapsed(object? state)
    {
        PropertyChanged?.Invoke(this, new(nameof(WorkedHours)));
        PropertyChanged?.Invoke(this, new(nameof(ExpectedExitTime)));

        if (CurrentDate != DateTime.Today)
        {
            if (IsTimerRunning)
                ToggleTimer();

            CurrentDate = DateTime.Today;
            ResetTimer();

            WorkTimeService.UpdateCurrentBalance();
            WorkTimeService.UpdateExtraHours();
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

        NotifyTimerState();
    }
    #endregion

    #region ToggleTimer
    /// <summary>
    /// Toggles the timer.
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
                throw new ArgumentException($"The specified time cannot be set before {LastStateChangeTime:t}.", nameof(time));

            timeBalance = DateTimeMethods.RoundToSeconds(DateTime.Now) - time.Value;
        }

        LastStateChangeTime = time ?? DateTime.Now;

        if (IsTimerRunning)
        {
            _stopwatch.Stop();
            _additionalWorkTime -= timeBalance;
            NotifyTimerState();
        }
        else
        {
            _stopwatch.Start();
            NotifyTimerState();
            _additionalWorkTime += timeBalance;
        }
    }
    #endregion

    #endregion

    #region Private Method

    #region NotifyTimerState
    /// <summary>
    /// Notifies the timer state changes.
    /// </summary>
    private void NotifyTimerState()
    {
        PropertyChanged?.Invoke(this, new(nameof(IsTimerRunning)));
        TimerStateChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #endregion
}
