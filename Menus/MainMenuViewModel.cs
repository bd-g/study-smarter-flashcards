using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Sets;
using StudySmarterFlashcards.Utils;

namespace StudySmarterFlashcards.Menus
{
  public class MainMenuViewModel : BaseViewModel
  {
    #region Fields
    public RelayCommand AddSetCommand { get; private set; }
    #endregion
    #region Constructors
    public MainMenuViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<EditSetMessage>(this, editSetMessage => ReceiveEditSetMessage(editSetMessage));
      Messenger.Default.Register<AddSetMessage>(this, addSetMessage => ReceiveAddSetMessage(addSetMessage));
      AddSetCommand = new RelayCommand(AddSetAction);
      NumSetsLoaded = new NotifyTaskCompletion<string>(this.LoadStartingData());
    }
    #endregion

    #region Properties
    public NotifyTaskCompletion<string> NumSetsLoaded { get; private set; }

    public ObservableCollection<CardSetModel> CardSets { get; private set; }
    #endregion

    #region Private Methods
    private async Task<string> LoadStartingData()
    {
      CardSets = await LocalDataHandler.LoadAllSetsFromLocalMemory();
      OnPropertyChanged("CardSets");
      return CardSets.Count + " sets loaded successfully";
    }
    private void AddSetAction()
    {
      prNavigationService.NavigateTo("EditSetPage");
      Messenger.Default.Send<CardSetModel>(null);
    }

    private void ReceiveAddSetMessage(AddSetMessage addSetMessage)
    {
      CardSets.Add(addSetMessage.NewCardSetModel);
    }

    private void ReceiveEditSetMessage(EditSetMessage editSetMessage)
    {
      CardSetModel cardSetModelToEdit = null;
      foreach (CardSetModel cardSetModel in CardSets) {
        if (cardSetModel.SetID.Equals(editSetMessage.SetID)) {
          cardSetModelToEdit = cardSetModel;
        }
      }

      if (cardSetModelToEdit == null) {
        throw new ArgumentException(string.Format("Could not find selected set {0}.", editSetMessage.SetID));
      }

      if (editSetMessage.Name != null) {
        cardSetModelToEdit.Name = editSetMessage.Name;
      }
      if (editSetMessage.Description != null) {

        cardSetModelToEdit.Description = editSetMessage.Description;
      }

      switch (editSetMessage.SetStatus) {
        case SetStatus.Active:
          cardSetModelToEdit.IsArchived = false;
          break;
        case SetStatus.Archived:
          cardSetModelToEdit.IsArchived = true;
          break;
        case SetStatus.Deleted:
          if (!CardSets.Remove(cardSetModelToEdit)) {
            throw new ArgumentException(string.Format("Could not delete selected set {0}.", editSetMessage.SetID));
          }
          return;
      }

      ObservableCollection<IndividualCardModel> flashcards = cardSetModelToEdit.FlashcardCollection; 

      foreach(IndividualCardModel editedCard in editSetMessage.EditedCards) {
        for (int i = 0; i < flashcards.Count; i++) {
          IndividualCardModel originalCard = flashcards[i];
          if (originalCard.CardID.Equals(editedCard.CardID)) {
            if (originalCard.IsLearned != editedCard.IsLearned) {
              cardSetModelToEdit.EditCardSwitchIsLearned(originalCard.CardID);
            }
            if (originalCard.IsArchived != editedCard.IsArchived) {
              cardSetModelToEdit.EditCardSwitchIsArchived(originalCard.CardID);
            }
            if (originalCard.Term != editedCard.Term || originalCard.Definition != editedCard.Definition) {
              cardSetModelToEdit.EditCardInSet(originalCard.CardID, editedCard.Term, editedCard.Definition);
            }
          }
        }
      }
    }
    #endregion
  }
}
