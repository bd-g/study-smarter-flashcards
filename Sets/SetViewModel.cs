using System;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Utils;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace StudySmarterFlashcards.Sets
{
  public class SetViewModel : BaseViewModel
  {
    #region Constructors
    public SetViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "SetView", cardSetModel => InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      EditCommand = new RelayCommand(EditAction);
      BasicStudyCommand = new RelayCommand(BasicStudyAction);
      ResizeColumnWidthCommand = new RelayCommand<SizeChangedEventArgs>(ResizeColumnWidthFunction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand EditCommand { get; private set; }
    public RelayCommand BasicStudyCommand { get; private set; }
    public CardSetModel FlashCardSet { get; private set; } = new CardSetModel();
    public RelayCommand<SizeChangedEventArgs> ResizeColumnWidthCommand { get; private set; }
    public int SetColumnWidth { get; private set; } = 100;
    #endregion

    #region Private Methods
    private void NavigateHomeAction()
    {
      prNavigationService.NavigateTo("MainMenuPage");
    }

    private void InitializeSetPage(CardSetModel cardSetModel)
    {
      if (cardSetModel != null) {
        FlashCardSet = cardSetModel;
      } else {
        FlashCardSet = new CardSetModel();
        Messenger.Default.Send(FlashCardSet, "AddSet");
      }
      OnPropertyChanged("FlashCardSet");
    }
    private void ResizeColumnWidthFunction(SizeChangedEventArgs args)
    {
      if (args.NewSize.Width - 75 > 800) {
        SetColumnWidth = (int)Math.Floor(((args.NewSize.Width - 75) / 3));
      } else if (args.NewSize.Width - 50 > 400) {
        SetColumnWidth = (int)Math.Floor(((args.NewSize.Width - 50) / 2));
      } else {
        SetColumnWidth = (int)Math.Floor(args.NewSize.Width - 25);
      }
      OnPropertyChanged("SetColumnWidth");
    }

    private void EditAction()
    {
      prNavigationService.NavigateTo("EditSetPage");
      Messenger.Default.Send(FlashCardSet, "EditSetView");
    }

    private async void BasicStudyAction()
    {
      if (FlashCardSet.FlashcardCollection.Count == 0) {
        await new MessageDialog("You can't study an empty flashcard set! Add some cards to study.").ShowAsync();
        return;
      }
      bool containsStarredCard = false;
      foreach (IndividualCardModel individualCard in FlashCardSet.FlashcardCollection) {
        if (individualCard.IsStarred) {
          containsStarredCard = true;
          break;
        }
      }
      if (!containsStarredCard) {
        await new MessageDialog("There are no starred cards in this set. Star some cards to study them, and leave any cards you don't want to study yet unstarred.").ShowAsync();
        return;
      }

      prNavigationService.NavigateTo("BasicStudyPage");
      Messenger.Default.Send(FlashCardSet, "StudyView");
      FlashCardSet.RegisterNewReviewSession();
    }
    #endregion
  }
}
