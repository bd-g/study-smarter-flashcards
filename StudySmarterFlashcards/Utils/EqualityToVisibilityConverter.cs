using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StudySmarterFlashcards.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace StudySmarterFlashcards.Utils
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
