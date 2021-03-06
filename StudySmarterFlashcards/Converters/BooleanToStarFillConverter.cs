﻿using System;
using Windows.UI.Xaml.Data;

namespace StudySmarterFlashcards.Converters
{
  public class BooleanToStarFillConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (bool)value ^ (parameter as string ?? string.Empty).Equals("Reverse") ?
            "\xE735" : "\xE734";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        ((string)value).Equals("\xE735") ^ (parameter as string ?? string.Empty).Equals("Reverse");
  }
}
