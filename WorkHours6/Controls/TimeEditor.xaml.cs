using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HelperExtensions;
using WorkHours6.Models;

namespace WorkHours6.Controls;

/// <summary>
/// Interaction logic for TimeEditor.xaml
/// </summary>
public partial class TimeEditor
{
    #region Properties

    /// <inheritdoc cref="FrameworkElement"/>
    [Category("Appearance")]
    [DefaultValue(typeof(Brushes), "Transparent")]
    [Description("Gets or sets the brush that describes the background of the control.")]
    public new Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the data of the control.
    /// </summary>
    [Category("Appearance")]
    [Description("Gets or sets the data of the control.")]
    public TimeDatabaseEntry Data
    {
        get => (TimeDatabaseEntry)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the time picker control is enabled.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the time picker control is enabled.")]
    public bool IsTimePickerEnabled
    {
        get => (bool)GetValue(IsTimePickerEnabledProperty);
        set => SetValue(IsTimePickerEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the time picker control is enabled.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the control should sho the decimal representation of the worked hours.")]
    public bool ShowDecimalValues
    {
        get => (bool)GetValue(ShowDecimalValuesProperty);
        set => SetValue(ShowDecimalValuesProperty, value);
    }

    public new static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
        nameof(Background),
        typeof(Brush),
        typeof(TimeEditor),
        new FrameworkPropertyMetadata(
            Brushes.Transparent,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data),
        typeof(TimeDatabaseEntry),
        typeof(TimeEditor),
        new FrameworkPropertyMetadata(
            new TimeDatabaseEntry(DateTime.Today),
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsTimePickerEnabledProperty = DependencyProperty.Register(
        nameof(IsTimePickerEnabled),
        typeof(bool),
        typeof(TimeEditor),
        new FrameworkPropertyMetadata(
            true,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowDecimalValuesProperty = DependencyProperty.Register(
        nameof(ShowDecimalValues),
        typeof(bool),
        typeof(TimeEditor),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion

    public TimeEditor() => 
        InitializeComponent();

    #region Events

    #region BtnCreditedTime_OnClick
    /// <summary>
    /// Closes the Credited Time pop up.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnCreditedTime_OnClick(object sender, RoutedEventArgs e) =>
        PopCreditedTime.IsOpen = false;
    #endregion

    #region RdbDayOff_OnChecked
    /// <summary>
    /// Sets the credited time to 8 hours.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RdbDayOff_OnChecked(object sender, RoutedEventArgs e) =>
        TpkCreditedTime.SelectedTime = TimeSpan.FromHours(8);
    #endregion

    #region RdbNone_OnChecked
    /// <summary>
    /// Sets the credited time to 0 hours.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RdbNone_OnChecked(object sender, RoutedEventArgs e) =>
        TpkCreditedTime.SelectedTime = TimeSpan.Zero;
    #endregion

    #region RdbPartTime_OnChecked
    /// <summary>
    /// Sets the credited time to 4 hours.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RdbPartTime_OnChecked(object sender, RoutedEventArgs e) =>
        TpkCreditedTime.SelectedTime = TimeSpan.FromHours(4);
    #endregion

    #region TxbCreditedTime_OnMouseLeftButtonUp
    /// <summary>
    /// Opens the Credited Time pop up.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbCreditedTime_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        RdbNone.IsChecked = Data.CreditedHours == TimeSpan.Zero;
        RdbPartTime.IsChecked = Data.CreditedHours == TimeSpan.FromHours(4);
        RdbDayOff.IsChecked = Data.CreditedHours == TimeSpan.FromHours(8);
        RdbCustomTime.IsChecked = RdbNone.IsChecked.IsFalse()
                                  && RdbPartTime.IsChecked.IsFalse()
                                  && RdbDayOff.IsChecked.IsFalse();

        PopCreditedTime.IsOpen = true;
    }
    #endregion

    #endregion
}