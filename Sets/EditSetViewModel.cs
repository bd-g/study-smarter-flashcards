using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Utils;
using Windows.UI.Xaml.Navigation;

namespace StudySmarterFlashcards.Sets
{
  public class EditSetViewModel : BaseViewModel
  {
    #region Constructors
    public EditSetViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, cardSetModel => InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      CancelCommand = new RelayCommand(CancelAction);
      SaveCommand = new RelayCommand(SaveAction);
      AddCardCommand = new RelayCommand(AddCardAction);
      DeleteCardCommand = new RelayCommand<IndividualCardModel>(DeleteCardFunction);
    }
    #endregion

    #region Properties
    public bool IsCreatingNewSet { get; private set; } = true;
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand CancelCommand { get; private set; }
    public RelayCommand SaveCommand { get; private set; }
    public RelayCommand AddCardCommand { get; private set; }
    public RelayCommand<IndividualCardModel> DeleteCardCommand { get; private set; }
    public CardSetModel OriginalFlashCardSet { get; private set; } = new CardSetModel();
    public CardSetModel TempFlashCardSet { get; private set; } = new CardSetModel();
    #endregion

    #region Private Methods
    private void NavigateHomeAction()
    {
      prNavigationService.NavigateTo("MainMenuPage");
    }

    private void InitializeSetPage(CardSetModel cardSetModel)
    {
      if (cardSetModel != null) {
        OriginalFlashCardSet = cardSetModel;
        TempFlashCardSet = OriginalFlashCardSet.Clone();
        IsCreatingNewSet = false;
      } else {
        OriginalFlashCardSet = null;
        TempFlashCardSet = new CardSetModel();
        IsCreatingNewSet = true;
      }
      OnPropertyChanged("OriginalFlashCardSet");
      OnPropertyChanged("TempFlashCardSet");
      OnPropertyChanged("IsCreatingNewSet");
    }

    private void CancelAction()
    {
      prNavigationService.GoBack();
      TempFlashCardSet = null;
    }

    private void AddCardAction()
    {
      TempFlashCardSet.AddCardToSet();
    }
    private void DeleteCardFunction(IndividualCardModel cardToDelete)
    {
      TempFlashCardSet.RemoveCardFromSet(cardToDelete.CardID);
    }
    private void SaveAction()
    {
      if (OriginalFlashCardSet != null) {
        OriginalFlashCardSet.Name = TempFlashCardSet.Name;
        OriginalFlashCardSet.Description = TempFlashCardSet.Description;
        OriginalFlashCardSet.IsArchived = TempFlashCardSet.IsArchived;

        foreach (IndividualCardModel editedCard in TempFlashCardSet.FlashcardCollection) {
          int indexOfOriginal = OriginalFlashCardSet.FlashcardCollection.IndexOf(editedCard);
          if (indexOfOriginal > -1) {
            IndividualCardModel originalCard = OriginalFlashCardSet.FlashcardCollection[indexOfOriginal];
            if (originalCard.CardID.Equals(editedCard.CardID)) {
              if (originalCard.IsLearned != editedCard.IsLearned) {
                OriginalFlashCardSet.EditCardSwitchIsLearned(originalCard.CardID);
              }
              if (originalCard.IsArchived != editedCard.IsArchived) {
                OriginalFlashCardSet.EditCardSwitchIsArchived(originalCard.CardID);
              }
              if (originalCard.Term != editedCard.Term || originalCard.Definition != editedCard.Definition) {
                OriginalFlashCardSet.EditCardInSet(originalCard.CardID, editedCard.Term, editedCard.Definition);
              }
            }
          } else {
            OriginalFlashCardSet.FlashcardCollection.Add(editedCard);
          }
        }

        for (int i = 0; i < OriginalFlashCardSet.FlashcardCollection.Count; i++) {
          IndividualCardModel originalCard = OriginalFlashCardSet.FlashcardCollection[i];
          if (!TempFlashCardSet.FlashcardCollection.Contains(originalCard)) {
            OriginalFlashCardSet.FlashcardCollection.Remove(originalCard);
            i--;
          }
        }
      } else {
        OriginalFlashCardSet = TempFlashCardSet;
      }

      prNavigationService.NavigateTo("SetPage");
      Messenger.Default.Send(OriginalFlashCardSet);
      Messenger.Default.Send(new EditSetMessage(OriginalFlashCardSet));
    }
    #endregion
  }
}
