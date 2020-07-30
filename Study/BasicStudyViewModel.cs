using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Sets;
using StudySmarterFlashcards.Utils;
using Windows.UI.Text.Core;

namespace StudySmarterFlashcards.Study
{
  public class BasicStudyViewModel : BaseViewModel
  {
    #region Fields
    private static readonly Random prRandom = new Random();
    private bool prIsShuffleMode = true;
    #endregion
    #region Constructors
    public BasicStudyViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, cardSetModel => InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      BackCommand = new RelayCommand(BackAction);
      GoToNextFlashcardCommand = new RelayCommand(GoToNextFlashcard);
      GoToPreviousFlashcardCommand = new RelayCommand(GoToPreviousFlashcard);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand GoToNextFlashcardCommand { get; private set; }
    public RelayCommand GoToPreviousFlashcardCommand { get; private set; }
    public CardSetModel FlashCardSet { get; private set; }
    public int CurrentFlashcardIndex { get; private set; }
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
    public bool IsShuffleMode
    {
      get
      {
        return prIsShuffleMode;
      }
      set
      {
        prIsShuffleMode = value;
        OnPropertyChanged();
      }
    }
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
    }

    private void GoToNextFlashcard()
    {
      int currentIndex = CurrentFlashcardIndex; 
      if (IsShuffleMode) {
        int nextIndex = prRandom.Next(0, FlashCardSet.FlashcardCollection.Count);
        CurrentFlashcardIndex = currentIndex != nextIndex ? nextIndex : (nextIndex + 1) % FlashCardSet.FlashcardCollection.Count;
      } else {
        CurrentFlashcardIndex = (currentIndex + 1) % FlashCardSet.FlashcardCollection.Count;
      }
      PreviousFlashcardIndexes.Push(currentIndex);
      OnPropertyChanged("CurrentFlashcardIndex");
      OnPropertyChanged("CurrentFlashcard");
      OnPropertyChanged("HasPreviousFlashcards");
    }
    #endregion
  }
}
