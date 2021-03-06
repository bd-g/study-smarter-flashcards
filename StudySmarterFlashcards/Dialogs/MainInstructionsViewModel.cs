﻿using GalaSoft.MvvmLight.Command;
using StudySmarterFlashcards.Utils;
using System;
using System.Linq;

namespace StudySmarterFlashcards.Dialogs
{
  #region Outer Enums
  public enum MainInstructionsContent
  {
    Welcome,
    MainMenu,
    ImportInstructions,
    SetPage,
    EditSetPage
  }
  #endregion
  class MainInstructionsViewModel : BaseViewModel
  {
    #region Fields
    private bool dontShowAgain = false;
    #endregion

    #region Constructors
    public MainInstructionsViewModel() : base(null)
    {
      SaveSettingsAndCloseCommand = new RelayCommand(SaveSettingsAndCloseAction);
      GoToPreviousContentCommand = new RelayCommand(GoToPreviousContentAction);
      GoToNextContentCommand = new RelayCommand(GoToNextContentAction);
      bool? showMainInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMainInstructionsDialog"] as bool?;
      dontShowAgain = showMainInstructions == false ? true : false;
    }
    #endregion

    #region Properties
    public RelayCommand SaveSettingsAndCloseCommand { get; private set; }
    public RelayCommand GoToPreviousContentCommand { get; private set; }
    public RelayCommand GoToNextContentCommand { get; private set; }
    public MainInstructionsContent CurrentContent { get; set; }
    public bool HasPreviousContent { get; private set; } = false;
    public bool HasNextContent { get; private set; } = true;
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
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMainInstructionsDialog"] = !DontShowAgain;
    }

    private void GoToNextContentAction()
    {
      if (((int)CurrentContent) < Enum.GetValues(typeof(MainInstructionsContent)).Cast<int>().Max()) {
        CurrentContent = (MainInstructionsContent)((int)CurrentContent + 1);
        HasPreviousContent = true;
        HasNextContent = ((int)CurrentContent) < Enum.GetValues(typeof(MainInstructionsContent)).Cast<int>().Max();

        OnPropertyChanged("CurrentContent");
        OnPropertyChanged("HasPreviousContent");
        OnPropertyChanged("HasNextContent");
      }
    }
    private void GoToPreviousContentAction()
    {
      if (((int)CurrentContent) > Enum.GetValues(typeof(MainInstructionsContent)).Cast<int>().Min()) {
        CurrentContent = (MainInstructionsContent)((int)CurrentContent - 1);
        HasPreviousContent = ((int)CurrentContent) > Enum.GetValues(typeof(MainInstructionsContent)).Cast<int>().Min();
        HasNextContent = true;

        OnPropertyChanged("CurrentContent");
        OnPropertyChanged("HasPreviousContent");
        OnPropertyChanged("HasNextContent");
      }
    }
    #endregion
  }
}
