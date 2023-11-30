using HelperExtensions;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
#pragma warning disable CS8602

namespace WorkHours6.Controls;

/// <summary>
/// Interaction logic for TimePicker.xaml
/// </summary>
public partial class TimePicker
{
    #region Properties

    /// <summary>
    /// Gets and sets the maximum time.
    /// </summary>
    [Category("Behavior")]
    [DefaultValue(typeof(TimeSpan), "MaxValue")]
    [Description("Gets and sets the maximum time.")]
    public TimeSpan Maximum
    {
        get => (TimeSpan)GetValue(MaximumProperty);
        set
        {
            SetValue(MaximumProperty, value);
            UpdateLimits();
        }
    }

    /// <summary>
    /// Gets and sets the minimum time.
    /// </summary>
    [Category("Behavior")]
    [DefaultValue(typeof(TimeSpan), "MinValue")]
    [Description("Gets and sets the minimum time.")]
    public TimeSpan Minimum
    {
        get => (TimeSpan)GetValue(MinimumProperty);
        set
        {
            SetValue(MinimumProperty, value);
            UpdateLimits();
        }
    }

    /// <summary>
    /// Gets and sets the selected time.
    /// </summary>
    [Category("Behavior")]
    [Description("Gets and sets the selected time.")]
    public TimeSpan SelectedTime
    {
        get => (TimeSpan)GetValue(SelectedTimeProperty);
        set
        {
            SetValue(SelectedTimeProperty, value);
            FitValueIntoLimits();
        }
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(TimeSpan),
            typeof(TimePicker),
            new FrameworkPropertyMetadata(
                TimeSpan.MaxValue,
                FrameworkPropertyMetadataOptions.AffectsRender,
                LimitsChangedCallback));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(TimeSpan),
            typeof(TimePicker),
            new FrameworkPropertyMetadata(
                TimeSpan.MinValue,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan),
            typeof(TimePicker),
            new FrameworkPropertyMetadata(
                TimeSpan.Zero,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                SelectedValueChangedCallback));

    #endregion

    private bool _timeChangedByTheUser; //Indicates whether the value was changed by the user.

    public TimePicker() =>
        InitializeComponent();

    #region Events

    #region TimePicker_OnGotFocus
    /// <summary>
    /// Sets the default focus os the user control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimePicker_OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (!TxbMinutes.IsFocused)
        {
            TxbHours.Focus();
            TxbHours.SelectAll();
        }
    }
    #endregion

    #region TxbHours_OnLostFocus
    /// <summary>
    /// Updates the selected time when the hours text box focus is lost.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbHours_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (_timeChangedByTheUser && Int32.TryParse(TxbHours.Text, out int hours))
            SelectedTime = new(hours, SelectedTime.Minutes, 0);

        _timeChangedByTheUser = false;
    }
    #endregion

    #region TxbHours_OnPreviewKeyDown
    /// <summary>
    /// Manages the direction keys for the hours text box.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbHours_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Right:
                if (TxbHours.SelectionStart == TxbHours.Text.Length)
                {
                    TxbMinutes.Focus();
                    TxbMinutes.SelectAll();
                    e.Handled = true;
                }
                break;

            case Key.Up:
                SelectedTime = SelectedTime.AddHours(1);
                break;

            case Key.Down:
                SelectedTime = SelectedTime.AddHours(-1);
                break;
        }
    }
    #endregion

    #region TxbMinutes_OnLostFocus
    /// <summary>
    /// Updates the selected time when the minutes text box focus is lost.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbMinutes_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (_timeChangedByTheUser && Int32.TryParse(TxbMinutes.Text, out int minutes))
            SelectedTime = new TimeSpan(SelectedTime.Hours, minutes, 0);

        _timeChangedByTheUser = false;
    }
    #endregion

    #region TxbMinutes_OnPreviewKeyDown
    /// <summary>
    /// Manages the direction keys for the minutes text box.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbMinutes_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                if (TxbMinutes.SelectionStart == 0)
                {
                    TxbHours.Focus();
                    TxbHours.SelectAll();
                    e.Handled = true;
                }
                break;

            case Key.Up:
                SelectedTime = SelectedTime.AddMinutes(1);
                break;

            case Key.Down:
                SelectedTime = SelectedTime.AddMinutes(-1);
                break;
        }
    }
    #endregion

    #region TxbTime_OnPreviewTextInput
    /// <summary>
    /// Prevents any non-numeric character in the time textboxes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbTime_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        _timeChangedByTheUser = Char.IsNumber(e.Text[0]);
        e.Handled = !_timeChangedByTheUser;
    }
    #endregion

    #endregion

    #region Private Methods

    #region FitValueIntoLimits
    /// <summary>
    /// Fits the selected value into the Minimum and Maximum limits.
    /// </summary>
    private void FitValueIntoLimits()
    {
        if (SelectedTime > Maximum)
            SelectedTime = Maximum;

        else if (SelectedTime < Minimum)
            SelectedTime = Minimum;

        else if (SelectedTime < TimeSpan.Zero)
            SelectedTime = TimeSpan.Zero;
    }
    #endregion

    #region UpdateLimits
    /// <summary>
    /// Updates the selected value according to the limits.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void UpdateLimits()
    {
        if (Maximum < Minimum)
            throw new InvalidOperationException("The Maximum value must be greater than the Minimum value.");

        FitValueIntoLimits();
    }
    #endregion

    #endregion

    #region Static Private Methods

    #region LimitsChangedCallback
    /// <summary>
    /// Validates the limits and updates the selected value if needed.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void LimitsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as TimePicker).UpdateLimits();
    #endregion

    #region SelectedValueChangedCallback
    /// <summary>
    /// Fits the selected value into the limits if necessary.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void SelectedValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as TimePicker).FitValueIntoLimits();
    #endregion

    #endregion
}