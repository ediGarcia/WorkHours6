using System;
using System.ComponentModel;
using System.Windows;
using WorkHours6.Services;

namespace WorkHours6;

/// <summary>
/// Interaction logic for TimeSheetWindow.xaml
/// </summary>
public partial class TimeSheetWindow
{
    public TimeSheetWindow() =>
        InitializeComponent();

    #region Events

    #region TimeSheetWindow_OnClosing
    /// <summary>
    /// Saves the edited data before closing the window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeSheetWindow_OnClosing(object? sender, CancelEventArgs e) =>
        DataService.SaveTimeData(TemCalendar.EditedEntries);
    #endregion

    #region BtnGoToToday_OnClick
    /// <summary>
    /// Switches the calendar to the current month.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnGoToToday_OnClick(object sender, RoutedEventArgs e)
    {
        TemCalendar.Year = DateTime.Today.Year;
        TemCalendar.Month = DateTime.Today.Month;
    }
    #endregion

    #region BtnOk_OnClick
    /// <summary>
    /// Closes the window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnOk_OnClick(object sender, RoutedEventArgs e) =>
        Close();
    #endregion

    #region TemCalendar_MonthChanging
    /// <summary>
    /// Saves the edited time data when the month is changing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TemCalendar_MonthChanging(object? sender, EventArgs e) =>
        DataService.SaveTimeData(TemCalendar.EditedEntries);
    #endregion

    #endregion
}