using HelperMethods;
using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class BalanceToTextConverter : IValueConverter
{
    #region Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeSpan balance)
            return "+00:00";

        balance = DateTimeMethods.RoundToMinutes(balance);
        return $"{(balance < TimeSpan.Zero ? "-" : "+")}{Math.Abs((int)balance.TotalHours):00}:{Math.Abs(balance.Minutes):00}";
    }
    #endregion

    #region ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion
}