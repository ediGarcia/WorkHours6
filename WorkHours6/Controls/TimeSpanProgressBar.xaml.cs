using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WorkHours6.Services;
#pragma warning disable CS8618
#pragma warning disable CS8602

namespace WorkHours6.Controls;

/// <summary>
/// Interaction logic for CustomProgressBar.xaml
/// </summary>
public partial class TimeSpanProgressBar
{
    #region Custom Events

    /// <summary>
    /// Notifies when the control is clicked.
    /// </summary>
    public event EventHandler Click;

    #endregion

    #region Properties

    /// <summary>
    /// Gets and sets the calculate balance.
    /// </summary>
    [Category("Appearance")]
    [Description("Gets and sets the calculate balance.")]
    public TimeSpan Balance
    {
        get => (TimeSpan)GetValue(BalanceProperty);
        set => SetValue(BalanceProperty, value);
    }

    /// <summary>
    /// Gets and sets the current time.
    /// </summary>
    [Category("Appearance")]
    [Description("Gets and sets the current time.")]
    public TimeSpan CurrentTime
    {
        get => (TimeSpan)GetValue(CurrentTimeProperty);
        set
        {
            SetValue(CurrentTimeProperty, value);
            UpdatePercentage();
        }
    }

    /// <summary>
    /// Gets or sets the expected exit time for the current day.
    /// </summary>
    [Category("Appearance")]
    [Description("Gets or sets the expected exit time for the current day.")]
    public DateTime ExpectedExitTime
    {
        get => (DateTime)GetValue(ExpectedExitTimeProperty);
        set => SetValue(ExpectedExitTimeProperty, value);
    }

    /// <summary>
    /// Gets and sets the needed extra hours.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(typeof(TimeSpan), "00:00:00")]
    [Description("Gets and sets the needed extra hours.")]
    public TimeSpan ExtraHours
    {
        get => (TimeSpan)GetValue(ExtraHoursProperty);
        set => SetValue(ExtraHoursProperty, value);
    }

    /// <summary>
    /// Gets and sets the brush that describes the background of the progress bar.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(typeof(Brushes), "Black")]
    [Description("Gets and sets the brush that describes the background of the progress bar.")]
    public Brush ProgressBackground
    {
        get => (Brush)GetValue(ProgressBackgroundProperty);
        set => SetValue(ProgressBackgroundProperty, value);
    }

    /// <summary>
    /// Gets and sets the brush that describes the foreground of the progress bar.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(typeof(Brushes), "White")]
    [Description("Gets and sets the brush that describes the foreground of the progress bar.")]
    public Brush ProgressForeground
    {
        get => (Brush)GetValue(ProgressForegroundProperty);
        set => SetValue(ProgressForegroundProperty, value);
    }

    /// <summary>
    /// Gets and sets the tool-tip object that is displayed for this element in the user interface (UI).
    /// </summary>
    [Category("Appearance")]
    [Description("Gets and sets the tool-tip object that is displayed for this element in the user interface (UI).")]
    public new object ToolTip
    {
        get => GetValue(ToolTipProperty);
        set => SetValue(ToolTipProperty, value);
    }

    /// <summary>
    /// Gets and sets the total expected time.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(typeof(TimeSpan), "Zero")]
    [Description("Gets and sets the total expected time.")]
    public TimeSpan TotalTime
    {
        get => (TimeSpan)GetValue(TotalTimeProperty);
        set
        {
            SetValue(TotalTimeProperty, value);
            UpdatePercentage();
        }
    }

    public static readonly DependencyProperty BalanceProperty = DependencyProperty.Register(
        nameof(Balance),
        typeof(TimeSpan),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            TimeSpan.Zero,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CurrentTimeProperty = DependencyProperty.Register(
        nameof(CurrentTime),
        typeof(TimeSpan),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            TimeSpan.Zero,
            FrameworkPropertyMetadataOptions.AffectsRender,
            TimeChangedCallback));

    public static readonly DependencyProperty ExpectedExitTimeProperty = DependencyProperty.Register(
        nameof(ExpectedExitTime),
        typeof(DateTime),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            DateTime.Today,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ExtraHoursProperty = DependencyProperty.Register(
        nameof(ExtraHours),
        typeof(TimeSpan),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            TimeSpan.Zero,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ProgressBackgroundProperty = DependencyProperty.Register(
        nameof(ProgressBackground),
        typeof(Brush),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            Brushes.Black,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ProgressForegroundProperty = DependencyProperty.Register(
        nameof(ProgressForeground),
        typeof(Brush),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            Brushes.White,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public new static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(
        nameof(ToolTip),
        typeof(object),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TotalTimeProperty = DependencyProperty.Register(
        nameof(TotalTime),
        typeof(TimeSpan),
        typeof(TimeSpanProgressBar),
        new FrameworkPropertyMetadata(
            TimeSpan.Zero,
            FrameworkPropertyMetadataOptions.AffectsRender,
            TimeChangedCallback));

    #endregion

    public TimeSpanProgressBar() =>
        InitializeComponent();

    #region Events

    #region TimeLabel_OnClick
    /// <summary>
    /// Notifies that the time label was clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeLabel_Click(object? sender, EventArgs e) =>
        Click?.Invoke(this, e);
    #endregion
    
    #region TxbBalance_OnMouseLeftButtonUp
    /// <summary>
    /// Shows the expected exit time.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbBalance_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        TxbExitTime.Visibility = Visibility.Visible;
        TxbBalance.Visibility = Visibility.Collapsed;
    }
    #endregion

    #region TxbExitTime_OnMouseLeftButtonUp
    /// <summary>
    /// Shows the work hours balance.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TxbExitTime_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        TxbBalance.Visibility = Visibility.Visible;
        TxbExitTime.Visibility = Visibility.Collapsed;
    }
    #endregion

    #endregion

    #region Private Methods

    #region UpdatePercentage
    /// <summary>
    /// Updates the progress bar.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void UpdatePercentage() =>
        BrdProgress.Width = CurrentTime >= TotalTime
            ? SettingsService.WindowWidth
            : SettingsService.WindowWidth * CurrentTime.TotalSeconds / TotalTime.TotalSeconds;
    #endregion

    #endregion

    #region Private Static Methods

    #region PercentageChangedCallback
    /// <summary>
    /// Updates the progress bar.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void TimeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as TimeSpanProgressBar).UpdatePercentage();
    #endregion

    #endregion
}
