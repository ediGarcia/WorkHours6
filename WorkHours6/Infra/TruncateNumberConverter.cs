using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkHours6.Infra;

public class TruncateNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (int)Math.Floor((double)value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
