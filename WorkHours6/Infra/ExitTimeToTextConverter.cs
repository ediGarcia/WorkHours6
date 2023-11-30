using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class ExitTimeToTextConverter : IMultiValueConverter
{
    #region Convert
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (!values.Any() || values[0] is not DateTime || values[1] is not TimeSpan)
            return "Exit time: --:--";

        DateTime expectedExitTime = (DateTime)values[0];
        TimeSpan extraHours = (TimeSpan)values[1];

        return
            $"Exit time: {expectedExitTime:HH:mm}{(extraHours != TimeSpan.Zero ? $" ({(extraHours > TimeSpan.Zero ? "+" : "-")}{Math.Abs((int)extraHours.TotalHours):00}:{Math.Abs(extraHours.Minutes):00})" : "")}";
    }
    #endregion

    #region ConvertBack
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion
}