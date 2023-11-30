using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class CurrentDateCompareConverter : IValueConverter
{
    #region Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (DateTime)value == DateTime.Today;
    #endregion

    #region ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion
}