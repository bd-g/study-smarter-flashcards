using GalaSoft.MvvmLight.Command;
using StudySmarterFlashcards.Utils;

namespace StudySmarterFlashcards.Dialogs
{
  class MultipleChoiceStudyInstructionsViewModel : BaseViewModel
  {
    #region Fields
    private bool dontShowAgain = false;
    #endregion

    #region Constructors
    public MultipleChoiceStudyInstructionsViewModel() : base(null)
    {
      SaveSettingsAndCloseCommand = new RelayCommand(SaveSettingsAndCloseAction);
      bool? showFillBlankInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMultipleChoiceStudyInstructionsDialog"] as bool?;
      dontShowAgain = showFillBlankInstructions == false ? true : false;
    }
    #endregion

    #region Properties
    public RelayCommand SaveSettingsAndCloseCommand { get; private set; }
    public bool DontShowAgain
    {
      get
      {
        return dontShowAgain;
      }
      set
      {
        dontShowAgain = value;
        OnPropertyChanged();
      }
    }
    #endregion

    #region Private Methods
    private void SaveSettingsAndCloseAction()
    {
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMultipleChoiceStudyInstructionsDialog"] = !DontShowAgain;
    }
    #endregion
  }
}
