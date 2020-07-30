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
using Windows.UI.Xaml.Controls;

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
      GoToSetCommand = new RelayCommand<ItemClickEventArgs>(GoToSetAction);
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
    public RelayCommand<ItemClickEventArgs> GoToSetCommand { get; private set; }

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

    private void GoToSetAction(ItemClickEventArgs args)
    {
      CardSetModel cardSetClicked = args.ClickedItem as CardSetModel;
      prNavigationService.NavigateTo("SetPage");
      Messenger.Default.Send<CardSetModel>(cardSetClicked);
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
      
      if (!CardSets.Contains(cardSetModelEdited)) {
        CardSets.Add(cardSetModelEdited);
      }

      await LocalDataHandler.SaveAllSetsToLocalMemory(CardSets);
    }
    #endregion
  }
}
