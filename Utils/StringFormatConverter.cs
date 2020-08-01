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
    public string StringFormatNormalized
    {
      get
      {
        return StringFormat.Replace("\\", "");
      }
    }
    public bool ConvertToLocalTime { get; set; }
    public string Title { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {

      if (!String.IsNullOrEmpty(StringFormat)) {
        if (value is DateTime && ((DateTime)value).Equals(DateTime.MinValue)) {
          return String.Format(StringFormatNormalized, Title, "Never");
        } else {
          if (ConvertToLocalTime) {
            DateTime localDateTime = ((DateTime)value).ToLocalTime();
            return String.Format(StringFormatNormalized, Title, localDateTime);
          }
          return String.Format(StringFormatNormalized, Title, value);
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

