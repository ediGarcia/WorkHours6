using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class IsSpecialDateConverter : IValueConverter
{
    #region Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (DateTime)value is { Day: 1, Month: 5 } or { Day: 12, Month: 12 };
    #endregion

    #region ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion
}