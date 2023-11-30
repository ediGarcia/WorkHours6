using System.Windows;

namespace WorkHours6.Controls;

/// <summary>
/// Interaction logic for CloseButton.xaml
/// </summary>
public partial class CloseButton
{
    public CloseButton() =>
        InitializeComponent();

    #region Events

    #region BtnCloseButton_OnClick
    /// <summary>
    /// Closes the parent <see cref="Window"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnCloseButton_OnClick(object sender, RoutedEventArgs e) =>
        Window.GetWindow(this)?.Close();
    #endregion

    #endregion
}