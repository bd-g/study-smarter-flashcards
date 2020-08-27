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
    BasicStudyInstructions,
    FillBlankStudyInstructions,
    MainInstructions,
    ValidFileFormats
  }
  #endregion
  public static class InstructionsDialogService
  {
    public static async Task<ContentDialogResult> ShowAsync(InstructionDialogType dialogType, bool overrideSettings = false)
    {
      switch (dialogType) {
        case InstructionDialogType.BasicStudyInstructions:
          bool? showStudyInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowBasicStudyInstructionsDialog"] as bool?;
          if (showStudyInstructions != false || overrideSettings) {
            return await new BasicStudyInstructionsDialog().ShowAsync();
          } else {
            return ContentDialogResult.None;
          }
        case InstructionDialogType.FillBlankStudyInstructions:
          bool? showFillBlankInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowFillBlankStudyInstructionsDialog"] as bool?;
          if (showFillBlankInstructions != false || overrideSettings) {
            return await new FillBlankStudyInstructionsDialog().ShowAsync();
          } else {
            return ContentDialogResult.None;
          }
        case InstructionDialogType.MainInstructions:
          bool? showMainInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMainInstructionsDialog"] as bool?;
          if (showMainInstructions != false || overrideSettings) {
            return await new MainInstructionsDialog().ShowAsync();
          } else {
            return ContentDialogResult.None;
          }
        case InstructionDialogType.ValidFileFormats:
          return await new ValidFileFormatsDialog().ShowAsync();
        default:
          return ContentDialogResult.None;
      }
    }
  }
}
