using System;
using System.ComponentModel;
using WorkHours6.Services;

namespace WorkHours6.Models;

public class WorkTime : INotifyPropertyChanged
{
    #region Custom Events

    /// <summary>
    /// Notifies when a property's value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

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
    /// Gets the expected exit time for the day.
    /// </summary>
    public DateTime ExpectedExitTime => DateTime.Now + ExpectedWorkHours - WorkedHours;

    /// <summary>
    /// Gets the expected work hours.
    /// </summary>
    public TimeSpan ExpectedWorkHours => WorkTimeService.EightHours - CreditedHours + ExtraHours;

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
    public bool IsTimerRunning
    {
        get => _isTimerRunning;
        set
        {
            _isTimerRunning = value;
            PropertyChanged?.Invoke(this, new(nameof(IsTimerRunning)));
        }
    }

    /// <summary>
    /// Gets the last time the timer state has changed.
    /// </summary>
    public DateTime LastStateChangeTime
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
    public TimeSpan WorkedHours
    {
        get => _workedHours;
        set
        {
            _workedHours = value;
            PropertyChanged?.Invoke(this, new(nameof(WorkedHours)));
            PropertyChanged?.Invoke(this, new(nameof(ExpectedExitTime)));
        }
    }

    #endregion

    private TimeSpan _balance;
    private TimeSpan _creditedHours;
    private TimeSpan _extraHours;
    private bool _isTimerRunning;
    private DateTime _lastStateTimeChanged = DateTime.Today;
    private TimeSpan _workedHours;
}
