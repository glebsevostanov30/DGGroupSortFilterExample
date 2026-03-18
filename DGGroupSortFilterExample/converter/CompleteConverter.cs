using System.Globalization;
using System.Windows.Data;

namespace DGGroupSortFilterExample.converter;

[ValueConversion(typeof(bool), typeof(string))]
public class CompleteConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool complete)
        {
            return complete ? "✅ Выполнено" : "⏳ В работе";
        }
        return value?.ToString() ?? "Неизвестно";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string strComplete)
        {
            return strComplete.Contains("Выполнено");
        }
        return false;
    }
}