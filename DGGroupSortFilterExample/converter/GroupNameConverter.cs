using System.Globalization;
using System.Windows.Data;

namespace DGGroupSortFilterExample.converter;

public class GroupNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return "Без названия";
            
        var str = value.ToString();
            
        // Преобразуем значения булевых полей
        if (bool.TryParse(str, out var boolValue))
        {
            return boolValue ? "✅ Выполнено" : "⏳ В работе";
        }

        // Преобразуем даты
        return DateTime.TryParse(str, out var dateValue) ? dateValue.ToString("dd.MM.yyyy") : str;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}