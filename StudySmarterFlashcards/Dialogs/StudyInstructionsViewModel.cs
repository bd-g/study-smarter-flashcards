using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using StudySmarterFlashcards.Utils;

namespace StudySmarterFlashcards.Dialogs
{
  class StudyInstructionsViewModel : BaseViewModel
  {
    #region Fields
    private bool dontShowAgain = false;
    #endregion
    #region Constructors
    public StudyInstructionsViewModel() : base(null)
    {
      SaveSettingsAndCloseCommand = new RelayCommand(SaveSettingsAndCloseAction);
    }
    #endregion

    #region Properties
    public RelayCommand SaveSettingsAndCloseCommand { get; private set; }
    public bool DontShowAgain { 
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
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowStudyInstructionsDialog"] = !DontShowAgain;
    }
    #endregion
  }
}
