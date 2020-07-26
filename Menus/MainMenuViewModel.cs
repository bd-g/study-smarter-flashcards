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
    #region Constructors
    public MainMenuViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<EditSetMessage>(this, async editSetMessage => await ReceiveEditSetMessage(editSetMessage));
      AddSetCommand = new RelayCommand(AddSetAction);
      EditSetCommand = new RelayCommand<CardSetModel>(EditSetAction);
      ArchiveSetCommand = new RelayCommand<CardSetModel>(ArchiveSetAction);
      DeleteSetCommand = new RelayCommand<CardSetModel>(DeleteSetAction);
      GoToSetCommand = new RelayCommand<CardSetModel>(GoToSetAction);
      NumSetsLoaded = new NotifyTaskCompletion<string>(this.LoadStartingData());
    }
    #endregion

    #region Properties
    public NotifyTaskCompletion<string> NumSetsLoaded { get; private set; }
    public ObservableCollection<CardSetModel> CardSets { get; private set; }
    public RelayCommand AddSetCommand { get; private set; }
    public RelayCommand<CardSetModel> EditSetCommand { get; private set; }
    public RelayCommand<CardSetModel> ArchiveSetCommand { get; private set; }
    public RelayCommand<CardSetModel> DeleteSetCommand { get; private set; }
    public RelayCommand<CardSetModel> GoToSetCommand { get; private set; }

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

    private void GoToSetAction(CardSetModel cardSetModelToOpen)
    {
      prNavigationService.NavigateTo("SetPage");
      Messenger.Default.Send<CardSetModel>(cardSetModelToOpen);
    }

    private void EditSetAction(CardSetModel cardSetModelToEdit)
    {
      prNavigationService.NavigateTo("EditSetPage");
      Messenger.Default.Send<CardSetModel>(cardSetModelToEdit);
    }


    private void ArchiveSetAction(CardSetModel cardSetModelToArchive)
    {
      cardSetModelToArchive.IsArchived = !cardSetModelToArchive.IsArchived;
    }


    private void DeleteSetAction(CardSetModel cardSetModelToDelete)
    {
      CardSets.Remove(cardSetModelToDelete);
    }


    private async Task ReceiveEditSetMessage(EditSetMessage editSetMessage)
    {
      CardSetModel cardSetModelEdited = editSetMessage.EditedSet;
      CardSetModel cardSetModelToEdit = null;
      foreach (CardSetModel cardSetModel in CardSets) {
        if (cardSetModel.SetID.Equals(cardSetModelEdited.SetID)) {
          cardSetModelToEdit = cardSetModel;
        }
      }

      if (cardSetModelToEdit == null) {
        CardSets.Add(editSetMessage.EditedSet);
      } else {
        cardSetModelToEdit.Name = cardSetModelEdited.Name;
        cardSetModelToEdit.Description = cardSetModelEdited.Description;
        cardSetModelToEdit.IsArchived = cardSetModelToEdit.IsArchived;

        ObservableCollection<IndividualCardModel> flashcardsToEdit = cardSetModelToEdit.FlashcardCollection;

        foreach (IndividualCardModel editedCard in cardSetModelEdited.FlashcardCollection) {
          for (int i = 0; i < flashcardsToEdit.Count; i++) {
            IndividualCardModel originalCard = flashcardsToEdit[i];
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

      await LocalDataHandler.SaveAllSetsToLocalMemory(CardSets);
    }
    #endregion
  }
}
