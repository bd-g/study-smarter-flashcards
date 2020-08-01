using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySmarterFlashcards.Utils
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Windows.UI.Xaml.Data;

  namespace StudySmarterFlashcards.Utils
  {
    public class StringFormatConverter : IValueConverter
    {
      public string StringFormat { get; set; }
      public bool ConvertToLocalTime { get; set; }

      public object Convert(object value, Type targetType, object parameter, string language)
      {

        if (!String.IsNullOrEmpty(StringFormat)) {
          if (value is DateTime && ((DateTime)value).Equals(DateTime.MinValue)) {
            return String.Format(StringFormat, "Never");
          } else {
            if (ConvertToLocalTime) {
              DateTime localDateTime = ((DateTime)value).ToLocalTime();
              return String.Format(StringFormat, localDateTime);
            }
            return String.Format(StringFormat, value);
          }
        }
        return value;
      }

      public object ConvertBack(object value, Type targetType, object parameter, string language)
      {
        throw new NotImplementedException();
      }
    }
  }
}
