using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Dialogs;
using StudySmarterFlashcards.Sets;
using StudySmarterFlashcards.Utils;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;

namespace StudySmarterFlashcards.Study
{
  public class FillBlankStudyViewModel : BaseViewModel
  {
    #region Fields
    private static readonly Random prRandom = new Random();
    private int prNumCharsGuessed = 0;
    private static readonly object myLocker = new object();
    private static bool prCanUseKeyDown = true;
    #endregion

    #region Constructors
    public FillBlankStudyViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "FillBlankStudyView", async (cardSetModel) => await InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      BackCommand = new RelayCommand(BackAction);
      GoToNextFlashcardCommand = new RelayCommand(GoToNextFlashcard);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand GoToNextFlashcardCommand { get; private set; }
    public CardSetModel FlashCardSet { get; private set; }
    public int CurrentFlashcardIndex { get; private set; }
    public int NumCharsGuessed
    {
      get
      {
        return prNumCharsGuessed;
      }
      private set
      {
        if (prNumCharsGuessed != value) {
          prNumCharsGuessed = value;
          OnPropertyChanged();
          OnPropertyChanged("IncompleteWord");
          OnPropertyChanged("EmptySpacesOne");
          OnPropertyChanged("EmptySpacesTwoOrMore");
          OnPropertyChanged("IsWordIncomplete");
        }
      }
    }
    public string IncompleteWord {
      get
      {
        return CurrentFlashcard.Term.Substring(0, Math.Min(NumCharsGuessed, CurrentFlashcard.Term.Length));
      } 
    }
    public string EmptySpacesOne
    {
      get
      {
        if (NumCharsGuessed < CurrentFlashcard.Term.Length) {
          return "_";
        } else {
          return "";
        }
      }
    }
     public string EmptySpacesTwoOrMore
    {
      get
      {
        if (NumCharsGuessed < CurrentFlashcard.Term.Length - 1) {
          return (char)160 + ("_" + (char)160).Repeat(CurrentFlashcard.Term.Length - NumCharsGuessed - 1);
        } else {
          return "";
        }
      }
    }
    public bool IsWordIncomplete
    {
      get
      {
        return NumCharsGuessed < CurrentFlashcard.Term.Length;
      }
    }
    private int IndexOfFirstUnstarredCard { get; set; }
    public IndividualCardModel CurrentFlashcard
    {
      get
      {
        return FlashCardSet.FlashcardCollection[CurrentFlashcardIndex];
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
        default:
          string unicode = args.VirtualKey.KeyCodeToUnicode();
          if (!string.IsNullOrWhiteSpace(unicode)) {
            AttemptLetterGuess(unicode[0]);
          }
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
          }
        }
        NumCharsGuessed = 0;
      } else {
        throw new ArgumentNullException("Can't send null set to study page");
      }      
      OnPropertyChanged("FlashCardSet");
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");

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

    private void GoToNextFlashcard()
    {
      int currentIndex = CurrentFlashcardIndex;

      int nextIndex = prRandom.Next(0, IndexOfFirstUnstarredCard);
      if (currentIndex != nextIndex) {
        CurrentFlashcardIndex = nextIndex;
      } else if (nextIndex > 0) {
        CurrentFlashcardIndex = nextIndex - 1;
      } else {
        CurrentFlashcardIndex = IndexOfFirstUnstarredCard - 1;
      }

      NumCharsGuessed = 0;
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");
    }

    private void AttemptLetterGuess(char charGuessed)
    {
      if (NumCharsGuessed == CurrentFlashcard.Term.Length) {
        return;
      }

      if (char.ToUpperInvariant(charGuessed) == char.ToUpperInvariant(CurrentFlashcard.Term[NumCharsGuessed])) {
        NumCharsGuessed++;
        if (!IsWordIncomplete) {
          Messenger.Default.Send(true, "CharacterGuess");
        }
      } else {
        Messenger.Default.Send(false, "CharacterGuess");
      }
    }
    #endregion
  }
}
