using HelperMethods;
using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class ExitTimeToTextConverter : IValueConverter
{
    #region Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is DateTime exitTime ? DateTimeMethods.RoundToMinutes(exitTime).ToString("HH:mm") : "--:--";
    #endregion

    #region ConvertBack
    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion
}