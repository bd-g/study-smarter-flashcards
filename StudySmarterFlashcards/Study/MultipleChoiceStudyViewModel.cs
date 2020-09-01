using DataAccessLibrary.DataModels;
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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace StudySmarterFlashcards.Study
{  public enum GuessStatus
  {
    Unaffected,
    ShowAsCorrect,
    ShowAsFalse
  }
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
      MultipleChoiceItemClickedCommand = new RelayCommand<TappedRoutedEventArgs>(MultipleChoiceItemClickedAction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand GoToNextFlashcardCommand { get; private set; }
    public RelayCommand ShowMultipleChoiceInstructionsCommand { get; private set; }
    public RelayCommand<TappedRoutedEventArgs> MultipleChoiceItemClickedCommand { get; private set; }
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
    public ObservableCollection<Tuple<string, GuessStatus>> MultipleChoiceAnswers { get; private set; } = new ObservableCollection<Tuple<string, GuessStatus>>();
    public int NumAvailableAnswers { get; private set; } = 0;
    public bool IsGuessMade { 
      get
      {
        foreach (Tuple<string, GuessStatus> choicePair in MultipleChoiceAnswers) {
          if (choicePair.Item2 != GuessStatus.Unaffected) {
            return true;
          }
        }
        return false;
      } 
    }
    #endregion

    #region Public Methods
    public void KeyDownFunction(object sender, KeyEventArgs args)
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
        case Windows.System.VirtualKey.Number1:
        case Windows.System.VirtualKey.NumberPad1:
          MakeGuessAction(MultipleChoiceAnswers.Count > 0 ? MultipleChoiceAnswers[0] : null);
          break;
        case Windows.System.VirtualKey.Number2:
        case Windows.System.VirtualKey.NumberPad2:
          MakeGuessAction(MultipleChoiceAnswers.Count > 1 ? MultipleChoiceAnswers[1] : null);
          break;
        case Windows.System.VirtualKey.Number3:
        case Windows.System.VirtualKey.NumberPad3:
          MakeGuessAction(MultipleChoiceAnswers.Count > 2 ? MultipleChoiceAnswers[2] : null);
          break;
        case Windows.System.VirtualKey.Number4:
        case Windows.System.VirtualKey.NumberPad4:
          MakeGuessAction(MultipleChoiceAnswers.Count > 3 ? MultipleChoiceAnswers[3] : null);
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
      await InstructionsDialogService.ShowAsync(InstructionDialogType.MultipleChoiceStudyInstructions);
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
        MultipleChoiceAnswers.Add(new Tuple<string, GuessStatus>(FlashCardSet.FlashcardCollection[values[0]].Definition, GuessStatus.Unaffected));
      }
      if (values.Count > 1) {
        MultipleChoiceAnswers.Add(new Tuple<string, GuessStatus>(FlashCardSet.FlashcardCollection[values[1]].Definition, GuessStatus.Unaffected));
      }
      if (values.Count > 2) {
        MultipleChoiceAnswers.Add(new Tuple<string, GuessStatus>(FlashCardSet.FlashcardCollection[values[2]].Definition, GuessStatus.Unaffected));
      }

      NumAvailableAnswers = 1 + Math.Min(3, values.Count);
      MultipleChoiceAnswers.Insert(prRandom.Next(0, NumAvailableAnswers), new Tuple<string, GuessStatus>(CurrentFlashcard.Definition, GuessStatus.Unaffected));

      OnPropertyChanged("NumAvailableAnswers");
    }

    private void GoToNextFlashcard()
    {
      if (!IsGuessMade) {
        return;
      }
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

    private void MultipleChoiceItemClickedAction(TappedRoutedEventArgs args)
    {
      if (IsGuessMade) {
        return;
      }
      Tuple<string, GuessStatus> clickedItem = null;
      if (args.OriginalSource is TextBlock textBlock) {
        clickedItem = textBlock.DataContext as Tuple<string, GuessStatus>;
      } else if (args.OriginalSource is ListViewItemPresenter itemPresenter) {
        clickedItem = itemPresenter.Content as Tuple<string, GuessStatus>;
      }
      MakeGuessAction(clickedItem);
    }

    private void MakeGuessAction(Tuple<string, GuessStatus> choicePair)
    {
      if (choicePair != null) {
        for (int i = 0; i < NumAvailableAnswers; i++) {
          if (MultipleChoiceAnswers[i].Item1 == CurrentFlashcard.Definition) {
            MultipleChoiceAnswers[i] = new Tuple<string, GuessStatus>(CurrentFlashcard.Definition, GuessStatus.ShowAsCorrect);
          } else if (MultipleChoiceAnswers[i].Item1 == choicePair.Item1) {
            MultipleChoiceAnswers[i] = new Tuple<string, GuessStatus>(choicePair.Item1, GuessStatus.ShowAsFalse);
          }
        }
      }
    }

    private async void ShowMultipleChoiceInstructionsAction()
    {
      await InstructionsDialogService.ShowAsync(InstructionDialogType.MultipleChoiceStudyInstructions, true);
    }
    #endregion
  }
}
