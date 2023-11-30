using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class CompareIntegersConverter : IMultiValueConverter
{
    #region Public Methods

    #region Convert
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        (int)values[0] == (int)values[1];
    #endregion

    #region ConvertBack
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
    #endregion

    #endregion
}