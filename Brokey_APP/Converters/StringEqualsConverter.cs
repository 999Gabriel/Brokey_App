using System.Globalization;

namespace Brokey_APP.Converters;

// Vergleicht einen gebundenen String mit dem ConverterParameter und liefert true/false zurück
// (z. B. um in XAML je nach Zustand/Auswahl ein Element ein- oder auszublenden).
public class StringEqualsConverter : IValueConverter
{
    // Liefert true, wenn der gebundene Wert (Groß-/Kleinschreibung egal) dem Parameter entspricht.
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    // Rückkonvertierung nicht unterstützt – der Converter wird nur in eine Richtung verwendet.
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
