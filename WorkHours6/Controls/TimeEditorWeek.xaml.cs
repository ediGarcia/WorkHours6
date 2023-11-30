using System;
using System.ComponentModel;
using System.Windows;
using WorkHours6.Models;
#pragma warning disable CS8602

namespace WorkHours6.Controls;

/// <summary>
/// Interaction logic for TimeEditorWeek.xaml
/// </summary>
public partial class TimeEditorWeek
{
    #region Properties

    /// <summary>
    /// Gets or sets the current month.
    /// </summary>
    [Category("Behaviour")]
    [DefaultValue(5)]
    [Description("Gets or sets the current month.")]
    public int CurrentMonth
    {
        get => (int)GetValue(CurrentMonthProperty);
        set
        {
            SetValue(CurrentMonthProperty, value);
            ValidateCurrentMonth();
        }
    }

    /// <summary>
    /// Gets or sets the friday's time data.
    /// </summary>
    [Category("Behaviour")]
    [Description("Gets or sets the friday's time data.")]
    public TimeDatabaseEntry Friday
    {
        get => (TimeDatabaseEntry)GetValue(FridayProperty);
        set => SetValue(FridayProperty, value);
    }

    /// <summary>
    /// Gets or sets the monday's time data.
    /// </summary>
    [Category("Behaviour")]
    [Description("Gets or sets the monday's time data.")]
    public TimeDatabaseEntry Monday
    {
        get => (TimeDatabaseEntry)GetValue(MondayProperty);
        set => SetValue(MondayProperty, value);
    }

    /// <summary>
    /// Gets or sets the saturday's time data.
    /// </summary>
    [Category("Behaviour")]
    [Description("Gets or sets the saturday's time data.")]
    public TimeDatabaseEntry Saturday
    {
        get => (TimeDatabaseEntry)GetValue(SaturdayProperty);
        set => SetValue(SaturdayProperty, value);
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

    /// <summary>
    /// Gets or sets the sunday's time data.
    /// </summary>
    [Category("Behaviour")]
    [Description("Gets or sets the sunday's time data.")]
    public TimeDatabaseEntry Sunday
    {
        get => (TimeDatabaseEntry)GetValue(SundayProperty);
        set => SetValue(SundayProperty, value);
    }

    /// <summary>
    /// Gets or sets the thursday's time data.
    /// </summary>
    [Category("Behaviour")]
    [Description("Gets or sets the thursday's time data.")]
    public TimeDatabaseEntry Thursday
    {
        get => (TimeDatabaseEntry)GetValue(ThursdayProperty);
        set => SetValue(ThursdayProperty, value);
    }

    /// <summary>
    /// Gets or sets the tuesday's time data.
    /// </summary>
    [Category("Behaviour")]
    [Description("Gets or sets the tuesday's time data.")]
    public TimeDatabaseEntry Tuesday
    {
        get => (TimeDatabaseEntry)GetValue(TuesdayProperty);
        set => SetValue(TuesdayProperty, value);
    }

    /// <summary>
    /// Gets or sets the wednesday's time data.
    /// </summary>
    [Category("Behaviour")]
    [Description("Gets or sets the wednesday's time data.")]
    public TimeDatabaseEntry Wednesday
    {
        get => (TimeDatabaseEntry)GetValue(WednesdayProperty);
        set => SetValue(WednesdayProperty, value);
    }

    public static readonly DependencyProperty CurrentMonthProperty =
        DependencyProperty.Register(
            nameof(CurrentMonth),
            typeof(int),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                DateTime.Today.Month,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (o, _) => (o as TimeEditorWeek).ValidateCurrentMonth()));

    public static readonly DependencyProperty FridayProperty =
        DependencyProperty.Register(
            nameof(Friday),
            typeof(TimeDatabaseEntry),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                new TimeDatabaseEntry(DateTime.Today),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MondayProperty =
        DependencyProperty.Register(
            nameof(Monday),
            typeof(TimeDatabaseEntry),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                new TimeDatabaseEntry(DateTime.Today),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SaturdayProperty =
        DependencyProperty.Register(
            nameof(Saturday),
            typeof(TimeDatabaseEntry),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                new TimeDatabaseEntry(DateTime.Today),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowDecimalValuesProperty = DependencyProperty.Register(
        nameof(ShowDecimalValues),
        typeof(bool),
        typeof(TimeEditorWeek),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SundayProperty =
        DependencyProperty.Register(
            nameof(Sunday),
            typeof(TimeDatabaseEntry),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                new TimeDatabaseEntry(DateTime.Today),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThursdayProperty =
        DependencyProperty.Register(
            nameof(Thursday),
            typeof(TimeDatabaseEntry),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                new TimeDatabaseEntry(DateTime.Today),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TuesdayProperty =
        DependencyProperty.Register(
            nameof(Tuesday),
            typeof(TimeDatabaseEntry),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                new TimeDatabaseEntry(DateTime.Today),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty WednesdayProperty =
        DependencyProperty.Register(
            nameof(Wednesday),
            typeof(TimeDatabaseEntry),
            typeof(TimeEditorWeek),
            new FrameworkPropertyMetadata(
                new TimeDatabaseEntry(DateTime.Today),
                FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion

    public TimeEditorWeek() =>
        InitializeComponent();

    #region Private Methods

    #region ValidateCurrentMonth
    /// <summary>
    /// Updates the opacity of each day according to the current month.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void ValidateCurrentMonth()
    {
        if (CurrentMonth is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(CurrentMonth),
                "The current month value must be between 1 and 12.");
    }
    #endregion

    #endregion
}