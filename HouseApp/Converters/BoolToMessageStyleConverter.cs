using System.Globalization;

namespace HouseApp.Converters;

/// <summary>
/// Converts a boolean to different colors for chat message styling.
/// True = Current user message (accent colors), False = Other user message (neutral colors)
/// </summary>
public class BoolToMessageBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCurrentUser)
        {
            // Return hex color codes directly
            return isCurrentUser ? Color.FromArgb("#F3E8FF") : Color.FromArgb("#FFFFFF");
        }
        return Color.FromArgb("#FFFFFF");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean to different border colors for chat message styling.
/// </summary>
public class BoolToMessageBorderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCurrentUser)
        {
            // Return hex color codes directly
            return isCurrentUser ? Color.FromArgb("#D8B4FE") : Color.FromArgb("#E5E7EB");
        }
        return Color.FromArgb("#E5E7EB");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
