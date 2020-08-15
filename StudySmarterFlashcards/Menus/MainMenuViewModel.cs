using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Dialogs;
using StudySmarterFlashcards.Sets;
using StudySmarterFlashcards.Utils;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StudySmarterFlashcards.Menus
{
  public class MainMenuViewModel : BaseViewModel
  {
    #region Constructors
    public MainMenuViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "AddSet", async addedCardSet => await ReceiveEditSetMessage(addedCardSet));
      Messenger.Default.Register<CardSetModel>(this, "EditSet", async editedCardSet => await ReceiveEditSetMessage(editedCardSet));
      AddEmptySetCommand = new RelayCommand(AddEmptySetAction);
      ImportSetFromFileCommand = new RelayCommand(ImportSetFromFileAction);
      EditSetCommand = new RelayCommand<CardSetModel>(EditSetAction);
      ArchiveSetCommand = new RelayCommand<CardSetModel>(ArchiveSetFunction);
      DeleteSetCommand = new RelayCommand<CardSetModel>(DeleteSetAction);
      GoToSetCommand = new RelayCommand<ItemClickEventArgs>(GoToSetAction);
      NumSetsLoaded = new NotifyTaskCompletion<string>(this.LoadStartingData());
      GoToSettingsCommand = new RelayCommand(GoToSettingsAction);
      ResizeColumnWidthCommand = new RelayCommand<SizeChangedEventArgs>(ResizeColumnWidthFunction);
      InstructionsDialogService.ShowAsync(InstructionDialogType.MainInstructions).FireAndForgetSafeAsync(this);
    }
    #endregion

    #region Properties
    public NotifyTaskCompletion<string> NumSetsLoaded { get; private set; }
    public ObservableCollection<CardSetModel> CardSets { get; private set; }
    public RelayCommand AddEmptySetCommand { get; private set; }
    public RelayCommand ImportSetFromFileCommand { get; private set; }
    public RelayCommand<CardSetModel> EditSetCommand { get; private set; }
    public RelayCommand<CardSetModel> ArchiveSetCommand { get; private set; }
    public RelayCommand<CardSetModel> DeleteSetCommand { get; private set; }
    public RelayCommand GoToSettingsCommand { get; private set; }
    public RelayCommand<ItemClickEventArgs> GoToSetCommand { get; private set; }
    public RelayCommand<SizeChangedEventArgs> ResizeColumnWidthCommand { get; private set; }
    public int SetColumnWidth { get; private set; } = 100;
    #endregion

    #region Private Methods
    private async Task<string> LoadStartingData()
    {
      CardSets = await LocalDataHandler.LoadAllSetsFromLocalMemory();
      OnPropertyChanged("CardSets");
      return CardSets.Count + " set(s) loaded successfully";
    }
    private void AddEmptySetAction()
    {
      prNavigationService.NavigateTo("EditSetPage");
      Messenger.Default.Send<CardSetModel>(null, "EditSetView");
    }
    private void ImportSetFromFileAction()
    {
      prNavigationService.NavigateTo("EditSetPage");
      Messenger.Default.Send<CardSetModel>(null, "EditSetView");
    }

    private void GoToSetAction(ItemClickEventArgs args)
    {
      CardSetModel cardSetClicked = args.ClickedItem as CardSetModel;
      prNavigationService.NavigateTo("SetPage");
      Messenger.Default.Send(cardSetClicked, "SetView");
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

    private void GoToSettingsAction()
    {
      prNavigationService.NavigateTo("SettingsPage");
    }

    private void EditSetAction(CardSetModel cardSetModelToEdit)
    {
      prNavigationService.NavigateTo("EditSetPage");
      Messenger.Default.Send(cardSetModelToEdit, "EditSetView");
    }
    
    private async void ArchiveSetFunction(CardSetModel cardSetModelToArchive)
    {
      if (cardSetModelToArchive.IsStarred) {
        for (int i = 0; i < CardSets.Count; i++) {
          if (CardSets[i].IsStarred) {
            CardSets.Move(CardSets.IndexOf(cardSetModelToArchive), i);
            break;
          }
        }
        cardSetModelToArchive.IsStarred = false;
      } else {
        CardSets.Move(CardSets.IndexOf(cardSetModelToArchive), CardSets.Count - 1);
        cardSetModelToArchive.IsStarred = true;
      }
      await LocalDataHandler.SaveAllSetsToLocalMemory(CardSets);
    }


    private async void DeleteSetAction(CardSetModel cardSetModelToDelete)
    {
      MessageDialog messageDialog = new MessageDialog("Are you sure you want to permanently delete this set?");
      messageDialog.Commands.Add(new UICommand("Yes", null));
      messageDialog.Commands.Add(new UICommand("No", null));
      messageDialog.DefaultCommandIndex = 0;
      messageDialog.CancelCommandIndex = 1;
      IUICommand cmdResult = await messageDialog.ShowAsync();
      if (cmdResult.Label == "Yes") {
        if (CardSets.Remove(cardSetModelToDelete)) {
          await LocalDataHandler.SaveAllSetsToLocalMemory(CardSets);
        }
      }       
    }

    private async Task ReceiveEditSetMessage(CardSetModel editedSet)
    {
      if (!CardSets.Contains(editedSet)) {
        CardSets.Add(editedSet);
      }

      await LocalDataHandler.SaveAllSetsToLocalMemory(CardSets);
    }
    #endregion
  }
}
