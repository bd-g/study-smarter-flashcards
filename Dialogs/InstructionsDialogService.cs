using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace StudySmarterFlashcards.Dialogs
{
  #region Outer Enums
  public enum InstructionDialogType
  {
    StudyInstructions
  }
  #endregion
  public static class InstructionsDialogService
  {
    public static async Task<ContentDialogResult> ShowAsync(InstructionDialogType dialogType)
    {
      switch (dialogType) {
        case InstructionDialogType.StudyInstructions:
          bool? showStudyInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowStudyInstructionsDialog"] as bool?;
          if (showStudyInstructions != false) {
            return await new StudyInstructionsDialog().ShowAsync();
          } else {
            return ContentDialogResult.None;
          }
        default:
          return ContentDialogResult.None;
      }
    }
  }
}
