using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class ExtraHoursToTextConverter : IValueConverter
{
    #region Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is TimeSpan { TotalMinutes: >= 1 or <= -1 } extraHours
            ? $"({(extraHours < TimeSpan.Zero ? "-" : "+")}{Math.Abs((int)extraHours.TotalHours):00}:{Math.Abs(extraHours.Minutes):00})"
            : String.Empty;
    #endregion

    #region ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion
}