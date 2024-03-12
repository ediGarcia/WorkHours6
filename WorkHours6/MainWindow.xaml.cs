using System;
using System.ComponentModel;
using System.Windows;
using WorkHours6.Services;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace WorkHours6;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    #region Custom Events

    /// <summary>
    /// Notifies when the value of a property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets whether the time sheet window is opened.
    /// </summary>
    public bool IsSettingsWindowOpened
    {
        get => _isSettingsWindowOpened;
        private set
        {
            _isSettingsWindowOpened = value;
            PropertyChanged?.Invoke(this, new(nameof(IsSettingsWindowOpened)));
        }
    }

    /// <summary>
    /// Gets whether the time sheet window is opened.
    /// </summary>
    public bool IsTimeSheetOpened
    {
        get => _isTimeSheetOpened;
        private set
        {
            _isTimeSheetOpened = value;
            PropertyChanged?.Invoke(this, new(nameof(IsTimeSheetOpened)));
        }
    }

    #endregion

    private bool _isSettingsWindowOpened;
    private bool _isTimeSheetOpened;
    private SettingsWindow _settingsWindow;
    private TimeSheetWindow _timeSheetWindow;

    public MainWindow()
    {
        InitializeComponent();
        AdjustWindowPosition();
        UpdateBalanceAndExtraHours();

        SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged; //Keeps window in the bottom left corner.
    }

    #region Events

    #region BtnClosePopup_OnClick
    /// <summary>
    /// Closes the popup.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnClosePopup_OnClick(object sender, RoutedEventArgs e) =>
        PopSetTime.IsOpen = false;
    #endregion

    #region BtnOkPopup_OnClick
    /// <summary>
    /// Start/Stops the timer at the specified moment and closes the popup.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnOkPopup_OnClick(object sender, RoutedEventArgs e)
    {
        WorkTimeService.WorkTime.ToggleTimer(DateTime.Today + TpkTimePicker.SelectedTime);
        PopSetTime.IsOpen = false;
    }
    #endregion

    #region BtnSettings_OnClick
    /// <summary>
    /// Opens the Settings window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
    {
        if (IsSettingsWindowOpened)
            _settingsWindow.Focus();
        else
        {
            IsSettingsWindowOpened = true;

            _settingsWindow = new();
            _settingsWindow.Closed += SettingsWindow_Closed;
            _settingsWindow.Show();
        }
    }
    #endregion

    #region BtnTimeSheet_OnClick
    /// <summary>
    /// Shows the time sheet window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnTimeSheet_OnClick(object sender, RoutedEventArgs e)
    {
        if (IsTimeSheetOpened)
            _timeSheetWindow.Focus();
        else
        {
            _timeSheetWindow = new();
            _timeSheetWindow.Closed += TimeSheetWindow_Closed;
            _timeSheetWindow.Show();

            IsTimeSheetOpened = true;
        }
    }
    #endregion

    #region MainWindow_Closing
    /// <summary>
    /// Closes all windows, when the current window is closed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _settingsWindow?.Close();
        _timeSheetWindow?.Close();
    }
    #endregion

    #region MniResetTime_OnClick
    /// <summary>
    /// Resets the current timer.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MniResetTime_OnClick(object sender, RoutedEventArgs e) =>
        WorkTimeService.WorkTime.ResetTimer();
    #endregion

    #region MniSetTime_OnClick
    /// <summary>
    /// Opens the popup.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MniSetTime_OnClick(object sender, RoutedEventArgs e)
    {
        TpkTimePicker.Maximum = DateTime.Now.TimeOfDay;
        PopSetTime.IsOpen = true;
        TpkTimePicker.Focus();
    }
    #endregion

    #region TimeSheetWindow_Closed
    /// <summary>
    /// Cleans the time sheet window data and updates the balance.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeSheetWindow_Closed(object? sender, EventArgs e)
    {
        _timeSheetWindow.Closed -= TimeSheetWindow_Closed;
        _timeSheetWindow = null;

        IsTimeSheetOpened = false;
        WorkTimeService.UpdateCreditedHours();
        UpdateBalanceAndExtraHours();
    }
    #endregion

    #region SettingsWindow_Closed
    /// <summary>
    /// Cleans the settings window data and updates the balance.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SettingsWindow_Closed(object? sender, EventArgs e)
    {
        _settingsWindow.Closed -= SettingsWindow_Closed;
        _settingsWindow = null;

        IsSettingsWindowOpened = false;
        UpdateBalanceAndExtraHours();
    }
    #endregion

    #region SystemParameters_StaticPropertyChanged
    /// <summary>
    /// Adjusts the window position if the screen size changes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SystemParameters_StaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.WorkArea))
            AdjustWindowPosition();
    }
    #endregion

    #region TimeSpanProgressBar_OnClick
    /// <summary>
    /// Toggles the timer.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeSpanProgressBar_Click(object? sender, EventArgs e) =>
        WorkTimeService.WorkTime.ToggleTimer();
    #endregion

    #endregion

    #region Private Methods

    #region AdjustWindowPosition
    /// <summary>
    /// Positions the window in the right bottom corner of the screen.
    /// </summary>
    private void AdjustWindowPosition()
    {
        Left = SystemParameters.WorkArea.Width - Width;
        Top = SystemParameters.WorkArea.Height - Height;
    }
    #endregion

    #region UpdateBalance
    /// <summary>
    /// Updates the balance and the extra hours' information.
    /// </summary>
    private void UpdateBalanceAndExtraHours()
    {
        WorkTimeService.UpdateCurrentBalance();
        WorkTimeService.UpdateExtraHours();
    }
    #endregion

    #endregion
}
