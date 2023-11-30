using System.ComponentModel;
using System.Windows;
using WorkHours6.Services;

namespace WorkHours6;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
        AdjustWindowPosition();

        SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
    }

    #region Events

    #region SettingsWindow_Closing
    /// <summary>
    /// Saves data before closing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SettingsWindow_Closing(object? sender, CancelEventArgs e) =>
        DataService.SaveBalanceSettings(
            SettingsService.UserSettings.BalanceStartDate,
            SettingsService.UserSettings.TargetBalance,
            SettingsService.UserSettings.TargetBalanceDeadline,
            SettingsService.UserSettings.CalculateExtraHours);
    #endregion

    #region BtnOk_Click
    /// <summary>
    /// Closes the window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnOk_Click(object sender, RoutedEventArgs e) =>
        Close();
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

    #endregion

    #region Private Methods

    #region AdjustWindowPosition
    /// <summary>
    /// Positions the window in the right bottom corner of the screen.
    /// </summary>
    private void AdjustWindowPosition()
    {
        Left = SystemParameters.WorkArea.Width - Width;
        Top = SystemParameters.WorkArea.Height - SettingsService.WindowHeight - Height;
    }
    #endregion

    #endregion
}