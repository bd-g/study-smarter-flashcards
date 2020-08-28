using GalaSoft.MvvmLight.Command;
using StudySmarterFlashcards.Utils;
using System;
using System.Linq;

namespace StudySmarterFlashcards.Dialogs
{
  #region Outer Enums
  public enum ValidFileFormats
  {
    ExcelFile,
    WordFile,
    FutureSupportedFormats
  }
  #endregion
  class ValidFileFormatsViewModel : BaseViewModel
  {
    #region Constructors
    public ValidFileFormatsViewModel() : base(null)
    {
      GoToPreviousContentCommand = new RelayCommand(GoToPreviousContentAction);
      GoToNextContentCommand = new RelayCommand(GoToNextContentAction);
    }
    #endregion

    #region Properties
    public RelayCommand GoToPreviousContentCommand { get; private set; }
    public RelayCommand GoToNextContentCommand { get; private set; }
    public ValidFileFormats CurrentContent { get; set; }
    public bool HasPreviousContent { get; private set; } = false;
    public bool HasNextContent { get; private set; } = true;
    #endregion

    #region Private Methods
    private void GoToNextContentAction()
    {
      if (((int)CurrentContent) < Enum.GetValues(typeof(ValidFileFormats)).Cast<int>().Max()) {
        CurrentContent = (ValidFileFormats)((int)CurrentContent + 1);
        HasPreviousContent = true;
        HasNextContent = ((int)CurrentContent) < Enum.GetValues(typeof(ValidFileFormats)).Cast<int>().Max();

        OnPropertyChanged("CurrentContent");
        OnPropertyChanged("HasPreviousContent");
        OnPropertyChanged("HasNextContent");
      }
    }
    private void GoToPreviousContentAction()
    {
      if (((int)CurrentContent) > Enum.GetValues(typeof(ValidFileFormats)).Cast<int>().Min()) {
        CurrentContent = (ValidFileFormats)((int)CurrentContent - 1);
        HasPreviousContent = ((int)CurrentContent) > Enum.GetValues(typeof(ValidFileFormats)).Cast<int>().Min();
        HasNextContent = true;

        OnPropertyChanged("CurrentContent");
        OnPropertyChanged("HasPreviousContent");
        OnPropertyChanged("HasNextContent");
      }
    }
    #endregion
  }
}
