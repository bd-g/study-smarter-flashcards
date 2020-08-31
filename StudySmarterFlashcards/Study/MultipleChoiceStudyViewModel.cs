﻿using DataAccessLibrary.DataModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using StudySmarterFlashcards.Dialogs;
using StudySmarterFlashcards.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace StudySmarterFlashcards.Study
{
  public class MultipleChoiceStudyViewModel : BaseViewModel
  {
    #region Fields
    private static readonly Random prRandom = new Random();
    private static readonly object myLocker = new object();
    private static bool prCanUseKeyDown = true;
    #endregion

    #region Constructors
    public MultipleChoiceStudyViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "MultipleChoiceStudyView", async (cardSetModel) => await InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      BackCommand = new RelayCommand(BackAction);
      GoToNextFlashcardCommand = new RelayCommand(GoToNextFlashcard);
      ShowMultipleChoiceInstructionsCommand = new RelayCommand(ShowMultipleChoiceInstructionsAction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand GoToNextFlashcardCommand { get; private set; }
    public RelayCommand ShowMultipleChoiceInstructionsCommand { get; private set; }
    public CardSetModel FlashCardSet { get; private set; }
    private int CurrentFlashcardIndex { get; set; }
    private int IndexOfFirstUnstarredCard { get; set; }
    public IndividualCardModel CurrentFlashcard
    {
      get
      {
        return FlashCardSet.FlashcardCollection[CurrentFlashcardIndex];
      }
    }
    public ObservableCollection<string> MultipleChoiceAnswers { get; private set; } = new ObservableCollection<string>();
    public int NumAvailableAnswers { get; private set; } = 0;
    #endregion

    #region Public Methods
    public void KeyDownFunction(object sender, KeyEventArgs args)
    {
      lock (myLocker) {
        if (!prCanUseKeyDown) {
          return;
        }
      }
      if (args != null) {
        switch (args.VirtualKey) {
          case Windows.System.VirtualKey.Right:
            GoToNextFlashcard();
            break;
        }
      } else if (sender is VirtualKey) {
        VirtualKey virtualKey = (VirtualKey)sender;
        switch (virtualKey) {
          case Windows.System.VirtualKey.Enter:
            break;
          case Windows.System.VirtualKey.Space:
            break;
        }
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
        IndexOfFirstUnstarredCard = cardSetModel.FlashcardCollection.Count;
        for (int i = 0; i < cardSetModel.FlashcardCollection.Count; i++) {
          if (!cardSetModel.FlashcardCollection[i].IsStarred) {
            IndexOfFirstUnstarredCard = i;
            break;
          }
        }

        PopulateQuizOptions();
      } else {
        throw new ArgumentNullException("Can't send null set to study page");
      }
      OnPropertyChanged("FlashCardSet");
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");

      lock (myLocker) {
        prCanUseKeyDown = false;
      }
      //await InstructionsDialogService.ShowAsync(InstructionDialogType.MainInstructions);
      lock (myLocker) {
        prCanUseKeyDown = true;
      }
    }

    private void BackAction()
    {
      prNavigationService.GoBack();
    }

    private void PopulateQuizOptions()
    {
      MultipleChoiceAnswers.Clear();
      var values = Enumerable.Range(0, IndexOfFirstUnstarredCard).OrderBy(x => prRandom.Next()).ToList();
      values.Remove(CurrentFlashcardIndex);

      if (values.Count > 0) {
        MultipleChoiceAnswers.Add(FlashCardSet.FlashcardCollection[values[0]].Definition);
      }
      if (values.Count > 1) {
        MultipleChoiceAnswers.Add(FlashCardSet.FlashcardCollection[values[1]].Definition);
      }
      if (values.Count > 2) {
        MultipleChoiceAnswers.Add(FlashCardSet.FlashcardCollection[values[2]].Definition);
      }

      NumAvailableAnswers = 1 + Math.Min(3, values.Count);
      MultipleChoiceAnswers.Insert(prRandom.Next(0, NumAvailableAnswers), CurrentFlashcard.Definition);

      OnPropertyChanged("NumAvailableAnswers");
    }

    private void GoToNextFlashcard()
    {
      //if (IsWordIncomplete) {
      //  return;
      //}
      int currentIndex = CurrentFlashcardIndex;

      int nextIndex = prRandom.Next(0, IndexOfFirstUnstarredCard);
      if (currentIndex != nextIndex) {
        CurrentFlashcardIndex = nextIndex;
      } else if (nextIndex > 0) {
        CurrentFlashcardIndex = nextIndex - 1;
      } else {
        CurrentFlashcardIndex = IndexOfFirstUnstarredCard - 1;
      }

      PopulateQuizOptions();

      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");
    }

    private async void ShowMultipleChoiceInstructionsAction()
    {
      //await InstructionsDialogService.ShowAsync(InstructionDialogType.MainInstructions, true);
    }
    #endregion
  }
}
