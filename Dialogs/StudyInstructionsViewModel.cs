using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace StudySmarterFlashcards.Dialogs
{
  class StudyInstructionsViewModel
  {
    #region Constructors
    public StudyInstructionsViewModel()
    {
      SaveSettingsAndCloseCommand = new RelayCommand(SaveSettingsAndCloseAction);
    }
    #endregion

    #region Properties
    public RelayCommand SaveSettingsAndCloseCommand { get; private set; }
    public bool DontShowAgain { get; set; }
    #endregion

    #region Private Methods
    private void SaveSettingsAndCloseAction()
    {
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowStudyInstructionsDialog"] = !DontShowAgain;
    }
    #endregion
  }
}
