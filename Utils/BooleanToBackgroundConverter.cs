using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace StudySmarterFlashcards.Utils
{
  public class BooleanToBackgroundConverter : IValueConverter
  {
    public bool AccentColor { get; set; }
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if ((bool)value == false) {
        {
          return AccentColor ? new SolidColorBrush((Color)Application.Current.Resources["SystemAltHighColor"])
                             : new SolidColorBrush((Color)Application.Current.Resources["SystemBaseMediumLowColor"]);
        }
      }
      return AccentColor ? new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorLight1"])
                         : new SolidColorBrush((Color)Application.Current.Resources["SystemAltHighColor"]);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }

  }
}
