using StudySmarterFlashcards.Study;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace StudySmarterFlashcards.Converters
{
  public class GuessStatusToObjectConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if ((parameter as string) == "string") {
        if (value is GuessStatus guessStatus) {
          switch (guessStatus) {
            case GuessStatus.Unaffected:
              return "\uEA3A";
            case GuessStatus.ShowAsCorrect:
              return "\uEA3B";
            case GuessStatus.ShowAsFalse:
              return "\uEA39";
          }
        }
        return string.Empty;
      } else if ((parameter as string) == "foreground") {
        if (value is GuessStatus guessStatus) {
          switch (guessStatus) {
            case GuessStatus.Unaffected:
              return new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
            case GuessStatus.ShowAsCorrect:
              return new SolidColorBrush(Colors.Green);
            case GuessStatus.ShowAsFalse:
              return new SolidColorBrush(Colors.Red);
          }
        }
        return new SolidColorBrush(Colors.Black);
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
