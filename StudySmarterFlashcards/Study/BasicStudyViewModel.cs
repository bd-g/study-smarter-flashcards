﻿using DataAccessLibrary;
using DataAccessLibrary.DataModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Dialogs;
using StudySmarterFlashcards.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace StudySmarterFlashcards.Study
{
  public class BasicStudyViewModel : BaseViewModel
  {
    #region Fields
    private static readonly Random prRandom = new Random();
    private static readonly object myLocker = new object();
    private static bool prCanUseKeyDown = true;
    #endregion

    #region Constructors
    public BasicStudyViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "StudyView", async (cardSetModel) => await InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      BackCommand = new RelayCommand(BackAction);
      GoToNextFlashcardCommand = new RelayCommand(GoToNextFlashcard);
      GoToPreviousFlashcardCommand = new RelayCommand(GoToPreviousFlashcard);
      FlipFlashcardCommand = new RelayCommand(FlipFlashcardAction);
      SwitchShuffleModeCommand = new RelayCommand(SwitchShuffleModeAction);
      IsLearnedChangedCommand = new RelayCommand<RoutedEventArgs>(IsLearnedChangedAction);
      MouseDownOnCardCommand = new RelayCommand<TappedRoutedEventArgs>(MouseDownOnCardFunction);
      ShowInstructionsCommand = new RelayCommand(ShowInstructionsAction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand ShowInstructionsCommand { get; private set; }
    public RelayCommand GoToNextFlashcardCommand { get; private set; }
    public RelayCommand GoToPreviousFlashcardCommand { get; private set; }
    public RelayCommand FlipFlashcardCommand { get; private set; }
    public RelayCommand SwitchShuffleModeCommand { get; private set; }
    public RelayCommand<RoutedEventArgs> IsLearnedChangedCommand { get; private set; }
    public RelayCommand<TappedRoutedEventArgs> MouseDownOnCardCommand { get; private set; }
    public CardSetModel FlashCardSet { get; private set; }
    private int CurrentFlashcardIndex { get; set; }
    private int IndexOfFirstUnstarredCard { get; set; }
    public bool IsShowingTerm { get; private set; } = true;
    public bool IsCurrentFlashcardLearned
    {
      get
      {
        return CurrentFlashcard.IsLearned;
      }
      set { }
    }
    public string CurrentSideShowing
    {
      get
      {
        return IsShowingTerm ? CurrentFlashcard.Term : CurrentFlashcard.Definition;
      }
    }
    public IndividualCardModel CurrentFlashcard
    {
      get
      {
        return FlashCardSet.FlashcardCollection[CurrentFlashcardIndex];
      }
    }
    public Stack<int> PreviousFlashcardIndexes { get; private set; }
    public bool HasPreviousFlashcards
    {
      get
      {
        return PreviousFlashcardIndexes.Count > 0;
      }
    }
    public bool IsShuffleMode { get; set; } = true;
    #endregion

    #region Public Methods
    public void KeyUpFunction(object sender, KeyEventArgs args)
    {
      lock (myLocker) {
        if (!prCanUseKeyDown) {
          return;
        }
      }
      switch (args.VirtualKey) {
        case Windows.System.VirtualKey.Right:
          GoToNextFlashcard();
          break;
        case Windows.System.VirtualKey.Left:
          GoToPreviousFlashcard();
          break;
        case Windows.System.VirtualKey.Up:
        case Windows.System.VirtualKey.Down:
          FlipFlashcardAction();
          break;
        case Windows.System.VirtualKey.S:
          SwitchShuffleModeAction();
          break;
        case Windows.System.VirtualKey.L:
          IsLearnedChangedAction(null);
          break;
      }
    }
    #endregion

    #region Private Methods
    private void NavigateHomeAction()
    {
      prNavigationService.NavigateTo("MainMenuPage");
    }

    private async Task InitializeSetPage(CardSetModel cardSetModel)
    {
      if (cardSetModel != null && cardSetModel.FlashcardCollection.Count > 0) {
        FlashCardSet = cardSetModel;
        CurrentFlashcardIndex = 0;
        PreviousFlashcardIndexes = new Stack<int>();
        IndexOfFirstUnstarredCard = cardSetModel.FlashcardCollection.Count;
        for (int i = 0; i < cardSetModel.FlashcardCollection.Count; i++) {
          if (!cardSetModel.FlashcardCollection[i].IsStarred) {
            IndexOfFirstUnstarredCard = i;
            break;
          }
        }
        IsShuffleMode = true;
      } else {
        throw new ArgumentNullException("Can't send null set to study page");
      }
      OnPropertyChanged("IsShuffleMode");
      OnPropertyChanged("FlashCardSet");
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");
      OnPropertyChanged("HasPreviousFlashcards");

      lock (myLocker) {
        prCanUseKeyDown = false;
      }
      await InstructionsDialogService.ShowAsync(InstructionDialogType.BasicStudyInstructions);
      lock (myLocker) {
        prCanUseKeyDown = true;
      }
    }

    private void BackAction()
    {
      prNavigationService.GoBack();
    }

    private async void ShowInstructionsAction()
    {
      await InstructionsDialogService.ShowAsync(InstructionDialogType.BasicStudyInstructions, true);
    }

    private void GoToPreviousFlashcard()
    {
      if (PreviousFlashcardIndexes.Count > 0) {
        CurrentFlashcardIndex = PreviousFlashcardIndexes.Pop();
      }
      IsShowingTerm = true;
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");
      OnPropertyChanged("HasPreviousFlashcards");
      OnPropertyChanged("CurrentSideShowing");
    }

    private void GoToNextFlashcard()
    {
      int currentIndex = CurrentFlashcardIndex;
      if (IsShuffleMode) {
        int nextIndex = prRandom.Next(0, IndexOfFirstUnstarredCard);
        if (currentIndex != nextIndex) {
          CurrentFlashcardIndex = nextIndex;
        } else if (nextIndex > 0) {
          CurrentFlashcardIndex = nextIndex - 1;
        } else {
          CurrentFlashcardIndex = IndexOfFirstUnstarredCard - 1;
        }
      } else {
        CurrentFlashcardIndex = (currentIndex + 1) % IndexOfFirstUnstarredCard;
      }
      IsShowingTerm = true;
      PreviousFlashcardIndexes.Push(currentIndex);
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");
      OnPropertyChanged("HasPreviousFlashcards");
      OnPropertyChanged("CurrentSideShowing");
    }
    private void FlipFlashcardAction()
    {
      IsShowingTerm = !IsShowingTerm;
      OnPropertyChanged("IsShowingTerm");
      OnPropertyChanged("CurrentSideShowing");
    }

    private void SwitchShuffleModeAction()
    {
      IsShuffleMode = !IsShuffleMode;
      OnPropertyChanged("IsShuffleMode");
    }

    private void IsLearnedChangedAction(RoutedEventArgs args)
    {
      if (args != null) {
        if (args.OriginalSource is CheckBox checkbox) {
          CurrentFlashcard.IsLearned = checkbox.IsChecked == true ? true : false;
        }
      } else {
        CurrentFlashcard.IsLearned = !CurrentFlashcard.IsLearned;
      }
      OnPropertyChanged("IsCurrentFlashcardLearned");
      DataAccess.EditFlashcardIsLearned_UWP(CurrentFlashcard.CardID, CurrentFlashcard.IsLearned);
    }

    private void MouseDownOnCardFunction(TappedRoutedEventArgs args)
    {
      FlipFlashcardAction();
      var tmp = FocusManager.GetFocusedElement();
    }
    #endregion
  }
}
