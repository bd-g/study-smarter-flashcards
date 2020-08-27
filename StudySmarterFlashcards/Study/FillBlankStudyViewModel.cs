using System;
using System.Collections.Generic;
using System.Linq;
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
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
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
    private static bool prUsedHint = false;
    #endregion

    #region Constructors
    public FillBlankStudyViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "FillBlankStudyView", async (cardSetModel) => await InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      BackCommand = new RelayCommand(BackAction);
      GoToNextFlashcardCommand = new RelayCommand(GoToNextFlashcard);
      UseHintCommand = new RelayCommand(UseHintAction);
      RevealEntireWordCommand = new RelayCommand(RevealEntireWordAction);
      AdjustColumnSpanCommand = new RelayCommand<SizeChangedEventArgs>(AdjustColumnSpanAction);
      ShowFillBlankInstructionsCommand = new RelayCommand(ShowFillBlankInstructionsAction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand GoToNextFlashcardCommand { get; private set; }
    public RelayCommand UseHintCommand { get; private set; }
    public RelayCommand RevealEntireWordCommand { get; private set; }
    public RelayCommand ShowFillBlankInstructionsCommand { get; private set; }
    public RelayCommand<SizeChangedEventArgs> AdjustColumnSpanCommand { get; private set; }
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
          return " _";
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
          StringBuilder sb = new StringBuilder();
          for (int i = NumCharsGuessed + 1; i < CurrentFlashcard.Term.Length; i++) {
            if (char.IsLetterOrDigit(CurrentFlashcard.Term[i])) {
              sb.Append((char)160);
              sb.Append('_');
            } else if (char.IsWhiteSpace(CurrentFlashcard.Term[i])) {
              sb.Append((char)160); 
              sb.Append(CurrentFlashcard.Term[i]);
              sb.Append((char)160);
            } else {
              sb.Append(CurrentFlashcard.Term[i]);
            }
          }
          return sb.ToString();
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
    public int ColumnSpanLength { get; private set; } = 1;
    public int ColumnNumber { get; private set; } = 5;
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
            if (IsWordIncomplete) {
              UseHintAction();
            } else {
              GoToNextFlashcard();
            }
            break;
          default:
            string unicode = args.VirtualKey.KeyCodeToUnicode();
            if (!string.IsNullOrWhiteSpace(unicode)) {
              AttemptLetterGuess(unicode[0]);
            }
            break;
        }
      } else if (sender is VirtualKey) {
        VirtualKey virtualKey = (VirtualKey)sender;
        switch (virtualKey) {
          case Windows.System.VirtualKey.Enter:
            RevealEntireWordAction();
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
      await InstructionsDialogService.ShowAsync(InstructionDialogType.FillBlankStudyInstructions);
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
      if (IsWordIncomplete) {
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

      prUsedHint = false;
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
        RevealNextLetter();
      } else {
        Messenger.Default.Send(false, "CharacterGuess");
      }
    }

    private void UseHintAction()
    {
      prUsedHint = true;
      RevealNextLetter();
      Messenger.Default.Send(false, "FillBlankHint");
    }

    private void RevealEntireWordAction()
    {
      if (IsWordIncomplete) {
        prUsedHint = true;
        NumCharsGuessed = CurrentFlashcard.Term.Length;
        Messenger.Default.Send(true, "FillBlankHint");
      }
    }

    private void RevealNextLetter()
    {
      NumCharsGuessed++;
      while (IsWordIncomplete && !char.IsLetterOrDigit(CurrentFlashcard.Term[NumCharsGuessed])) {
        NumCharsGuessed++;
      }
      if (!IsWordIncomplete && !prUsedHint) {
        Messenger.Default.Send(true, "CharacterGuess");
      }
    }

    private void AdjustColumnSpanAction(SizeChangedEventArgs args)
    {
      if (args.NewSize.Width < 500) {
        ColumnSpanLength = 3;
        ColumnNumber = 4;
      } else if (args.NewSize.Width < 1000) {
        ColumnSpanLength = 2;
        ColumnNumber = 4;
      } else {
        ColumnSpanLength = 1;
        ColumnNumber = 5;
      }
      OnPropertyChanged("ColumnSpanLength");
      OnPropertyChanged("ColumnNumber");
    }

    private async void ShowFillBlankInstructionsAction()
    {
      await InstructionsDialogService.ShowAsync(InstructionDialogType.FillBlankStudyInstructions, true);
    }
    #endregion
  }
}
