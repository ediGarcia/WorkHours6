using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WorkHours6.Models;
using WorkHours6.Services;
#pragma warning disable CS8618
#pragma warning disable CS8602

namespace WorkHours6.Controls;

/// <summary>
/// Interaction logic for TimeEditorCalendar.xaml
/// </summary>
public partial class TimeEditorMonth
{
    #region Custom Events

    /// <summary>
    /// Notifies whenever the current selected month is changing.
    /// </summary>
    public event EventHandler MonthChanging;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the entries that have been edited.
    /// </summary>
    public TimeDatabaseEntry[] EditedEntries => _entries.Where((entry, i) =>
            entry.WorkedTime != _originalEntries[i].WorkedTime
            || entry.CreditedHours != _originalEntries[i].CreditedHours)
        .ToArray();

    /// <summary>
    /// Gets or sets currently selected month.
    /// </summary>
    [Category("Appearance")]
    [Description("Gets or sets currently selected month.")]
    public int Month
    {
        get => (int)GetValue(MonthProperty);
        set
        {
            MonthChanging?.Invoke(this, EventArgs.Empty);
            SetValue(MonthProperty, value);
            UpdateMonth();
        }
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
    /// Gets or sets currently selected year.
    /// </summary>
    [Category("Appearance")]
    [Description("Gets or sets currently selected year.")]
    public int Year
    {
        get => (int)GetValue(YearProperty);
        set
        {
            MonthChanging?.Invoke(this, EventArgs.Empty);
            SetValue(YearProperty, value);
            UpdateMonth();
        }
    }

    public static readonly DependencyProperty MonthProperty = DependencyProperty.Register(
        nameof(Month),
        typeof(int),
        typeof(TimeEditorMonth),
        new FrameworkPropertyMetadata(
            DateTime.Today.Month,
            FrameworkPropertyMetadataOptions.AffectsRender,
            UpdateMonthStatic));

    public static readonly DependencyProperty ShowDecimalValuesProperty = DependencyProperty.Register(
        nameof(ShowDecimalValues),
        typeof(bool),
        typeof(TimeEditorMonth),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty YearProperty = DependencyProperty.Register(
        nameof(Year),
        typeof(int),
        typeof(TimeEditorMonth),
        new FrameworkPropertyMetadata(
            DateTime.Today.Year,
            FrameworkPropertyMetadataOptions.AffectsRender,
            UpdateMonthStatic));

    #endregion

    private List<TimeDatabaseEntry> _entries;
    private List<TimeDatabaseEntry> _originalEntries;

    public TimeEditorMonth()
    {
        InitializeComponent();
        UpdateMonth();
    }

    #region Events

    #region BrdNextMonth_OnMouseLeftButtonUp
    /// <summary>
    /// Shows the next month's data.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BrdNextMonth_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        MonthChanging?.Invoke(this, EventArgs.Empty);

        if (Month == 12)
        {
            Month = 1;
            Year++;
        }
        else
            Month++;
    }
    #endregion

    #region BrdPreviousMonth_OnMouseLeftButtonUp
    /// <summary>
    /// Shows the previous month data.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BrdPreviousMonth_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        MonthChanging?.Invoke(this, EventArgs.Empty);

        if (Month == 1)
        {
            Month = 12;
            Year--;
        }
        else
            Month--;
    }
    #endregion

    #endregion

    #region Private Methods

    #region UpdateMonth
    /// <summary>
    /// Updates the weeks' data.
    /// </summary>
    private void UpdateMonth()
    {
        TxbMonth.Text = Month switch
        {
            1 => $"January {Year:0000}",
            2 => $"February {Year:0000}",
            3 => $"March {Year:0000}",
            4 => $"April {Year:0000}",
            5 => $"May {Year:0000}",
            6 => $"June {Year:0000}",
            7 => $"July {Year:0000}",
            8 => $"August {Year:0000}",
            9 => $"September {Year:0000}",
            10 => $"October {Year:0000}",
            11 => $"November {Year:0000}",
            12 => $"December {Year:0000}",
            _ => "December 1991"
        };

        _entries = DataService.GetMonthReport(Month, Year);
        _originalEntries = _entries.Select(_ => _.Clone()).ToList();

        TmwFirst.Sunday = _entries[0];
        TmwFirst.Monday = _entries[1];
        TmwFirst.Tuesday = _entries[2];
        TmwFirst.Wednesday = _entries[3];
        TmwFirst.Thursday = _entries[4];
        TmwFirst.Friday = _entries[5];
        TmwFirst.Saturday = _entries[6];

        TmwSecond.Sunday = _entries[7];
        TmwSecond.Monday = _entries[8];
        TmwSecond.Tuesday = _entries[9];
        TmwSecond.Wednesday = _entries[10];
        TmwSecond.Thursday = _entries[11];
        TmwSecond.Friday = _entries[12];
        TmwSecond.Saturday = _entries[13];

        TmwThird.Sunday = _entries[14];
        TmwThird.Monday = _entries[15];
        TmwThird.Tuesday = _entries[16];
        TmwThird.Wednesday = _entries[17];
        TmwThird.Thursday = _entries[18];
        TmwThird.Friday = _entries[19];
        TmwThird.Saturday = _entries[20];

        TmwFourth.Sunday = _entries[21];
        TmwFourth.Monday = _entries[22];
        TmwFourth.Tuesday = _entries[23];
        TmwFourth.Wednesday = _entries[24];
        TmwFourth.Thursday = _entries[25];
        TmwFourth.Friday = _entries[26];
        TmwFourth.Saturday = _entries[27];

        TmwFifth.Sunday = _entries[28];
        TmwFifth.Monday = _entries[29];
        TmwFifth.Tuesday = _entries[30];
        TmwFifth.Wednesday = _entries[31];
        TmwFifth.Thursday = _entries[32];
        TmwFifth.Friday = _entries[33];
        TmwFifth.Saturday = _entries[34];

        if (_entries.Count > 35)
        {
            TmwSixth.Sunday = _entries[35];
            TmwSixth.Monday = _entries[36];
            TmwSixth.Tuesday = _entries[37];
            TmwSixth.Wednesday = _entries[38];
            TmwSixth.Thursday = _entries[39];
            TmwSixth.Friday = _entries[40];
            TmwSixth.Saturday = _entries[41];
            TmwSixth.Visibility = Visibility.Visible;
        }
        else
            TmwSixth.Visibility = Visibility.Collapsed;
    }
    #endregion

    #endregion

    #region Private Static Methods

    #region UpdateMonthStatic

    /// <summary>
    /// Updates the weeks' data.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="args"></param>
    private static void UpdateMonthStatic(DependencyObject o, DependencyPropertyChangedEventArgs args) =>
        (o as TimeEditorMonth).UpdateMonth();

    #endregion

    #endregion
}