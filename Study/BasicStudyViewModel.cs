using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Sets;
using StudySmarterFlashcards.Utils;
using Windows.UI.Text.Core;
using Windows.UI.Xaml.Input;

namespace StudySmarterFlashcards.Study
{
  public class BasicStudyViewModel : BaseViewModel
  {
    #region Fields
    private static readonly Random prRandom = new Random();
    #endregion

    #region Constructors
    public BasicStudyViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "StudyView", cardSetModel => InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      BackCommand = new RelayCommand(BackAction);
      GoToNextFlashcardCommand = new RelayCommand(GoToNextFlashcard);
      GoToPreviousFlashcardCommand = new RelayCommand(GoToPreviousFlashcard);
      FlipFlashcardCommand = new RelayCommand(FlipFlashcardAction);
      SwitchShuffleModeCommand = new RelayCommand(SwitchShuffleModeAction);
      KeyDownCommand = new RelayCommand<KeyRoutedEventArgs>(KeyDownFunction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand GoToNextFlashcardCommand { get; private set; }
    public RelayCommand GoToPreviousFlashcardCommand { get; private set; }
    public RelayCommand FlipFlashcardCommand { get; private set; }
    public RelayCommand SwitchShuffleModeCommand { get; private set; }
    public RelayCommand<KeyRoutedEventArgs> KeyDownCommand { get; private set; }
    public CardSetModel FlashCardSet { get; private set; }
    public int CurrentFlashcardIndex { get; private set; }
    private int IndexOfFirstUnstarredCard { get; set; }
    public bool IsShowingTerm { get; private set; } = true;
    public string CurrentSideShowing
    {
      get
      {
        return IsShowingTerm ? CurrentFlashcard.Term : CurrentFlashcard.Definition;
      }
    }

    public IndividualCardModel CurrentFlashcard {
      get {
        return FlashCardSet.FlashcardCollection[CurrentFlashcardIndex];
      }
    }
    public Stack<int> PreviousFlashcardIndexes { get; private set; }
    public bool HasPreviousFlashcards {
      get
      {
        return PreviousFlashcardIndexes.Count > 0;
      } 
    }
    public bool IsShuffleMode { get; set; } = true;
    #endregion

    #region Private Methods
    private void NavigateHomeAction()
    {
      prNavigationService.NavigateTo("MainMenuPage");
    }

    private void InitializeSetPage(CardSetModel cardSetModel)
    {
      if (cardSetModel != null && cardSetModel.FlashcardCollection.Count > 0) {
        FlashCardSet = cardSetModel;
        CurrentFlashcardIndex = 0;
        PreviousFlashcardIndexes = new Stack<int>();
        IndexOfFirstUnstarredCard = cardSetModel.FlashcardCollection.Count;
        for (int i = 0; i < cardSetModel.FlashcardCollection.Count; i++) {
          if (!cardSetModel.FlashcardCollection[i].IsStarred) {
            IndexOfFirstUnstarredCard = i;
          }
        }
      } else {
        throw new ArgumentNullException("Can't send null set to study page");
      }
      OnPropertyChanged("FlashCardSet");
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");
      OnPropertyChanged("HasPreviousFlashcards");
    }

    private void BackAction()
    {
      prNavigationService.GoBack();
    }

    private void GoToPreviousFlashcard()
    {
      if (PreviousFlashcardIndexes.Count > 0) {
        CurrentFlashcardIndex = PreviousFlashcardIndexes.Pop();
      }
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

    private void KeyDownFunction(KeyRoutedEventArgs args)
    {
      switch(args.Key) {
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
          SwitchCardIsLearned();
          break;
      }
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

    private void SwitchCardIsLearned()
    {
      CurrentFlashcard.IsLearned = !CurrentFlashcard.IsLearned;
      OnPropertyChanged("CurrentFlashcard");
    }
    #endregion
  }
}
