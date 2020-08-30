using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace StudySmarterFlashcards.Converters
{
  public class EqualityToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      return value.ToString().Equals(parameter.ToString()) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
