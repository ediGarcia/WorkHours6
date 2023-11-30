using HelperExtensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class WorkedHoursDecimalSumConverter : IMultiValueConverter
{
    #region Convert
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        TimeSpan sum = TimeSpan.Zero;
        values.ForEach<TimeSpan>(_ => sum += _);

        return sum.TotalHours.ToString("0.00");
    }
    #endregion

    #region ConvertBack
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion
}