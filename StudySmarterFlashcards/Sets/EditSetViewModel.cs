using DataAccessLibrary.DataModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace StudySmarterFlashcards.Sets
{
  public class EditSetViewModel : BaseViewModel
  {
    #region Constants
    private readonly int prMaxTermLength = 30;
    private readonly int prMaxDescriptionLength = 150;
    #endregion

    #region Fields
    private int prIndexOfLastImportSaved = -1;
    #endregion

    #region Constructors
    public EditSetViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "EditSetView", cardSetModel => InitializeSetPage(cardSetModel));
      Messenger.Default.Register<Tuple<CardSetModel, CardSetModel>> (this, "EditSetView", cardSetModelPair => InitializeSetPage(cardSetModelPair.Item1, cardSetModelPair.Item2));
      Messenger.Default.Register<List<CardSetModel>>(this, "EditSetView", cardSetModels => InitializeSetPage(cardSetModels));

      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      CancelCommand = new RelayCommand(CancelAction);
      NextImportedSetWithoutSavingCommand = new RelayCommand(NextImportedSetWithoutSavingAction);
      SaveCommand = new RelayCommand(async () => await SaveAction());
      AddCardCommand = new RelayCommand(AddCardAction);
      StarCardCommand = new RelayCommand<IndividualCardModel>(StarCardFunction);
      DeleteCardCommand = new RelayCommand<IndividualCardModel>(DeleteCardFunction);
      ResizeColumnWidthCommand = new RelayCommand<SizeChangedEventArgs>(ResizeColumnWidthFunction);
    }
    #endregion

    #region Properties
    public bool IsCreatingNewSet { get; private set; } = true;
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand CancelCommand { get; private set; }
    public RelayCommand NextImportedSetWithoutSavingCommand { get; private set; }
    public RelayCommand SaveCommand { get; private set; }
    public RelayCommand AddCardCommand { get; private set; }
    public RelayCommand<IndividualCardModel> StarCardCommand { get; private set; }
    public RelayCommand<IndividualCardModel> DeleteCardCommand { get; private set; }
    public CardSetModel OriginalFlashCardSet { get; private set; } = new CardSetModel();
    public CardSetModel TempFlashCardSet { get; private set; } = new CardSetModel();
    public List<CardSetModel> ImportedFlashcardSets { get; private set; } = null;
    public bool HasMultipleSetsToEdit { get; private set; }
    public int NumImportedSets
    {
      get
      {
        return ImportedFlashcardSets == null ? -1 : ImportedFlashcardSets.Count;
      }
    }
    public int IndexOfImportedSet { get; private set; }
    public int IndexOfImportedSetDisplay
    {
      get
      {
        return IndexOfImportedSet + 1;
      }
    }
    public string TempName
    {
      get
      {
        return TempFlashCardSet.Name;
      }
      set
      {
        TempFlashCardSet.Name = value;
        OnPropertyChanged("TempName");
        OnPropertyChanged("TempNameLength");
        OnPropertyChanged("TempNameLengthColor");
      }
    }
    public string TempNameLength
    {
      get
      {
        return TempName.Length + "/" + prMaxTermLength;
      }
    }
    public Brush TempNameLengthColor
    {
      get
      {
        return TempName.Length <= prMaxTermLength ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red);
      }
    }
    public string TempDescription
    {
      get
      {
        return TempFlashCardSet.Description;
      }
      set
      {
        TempFlashCardSet.Description = value;
        OnPropertyChanged("TempDescription");
        OnPropertyChanged("TempDescriptionLength");
        OnPropertyChanged("TempDescriptionLengthColor");
      }
    }
    public string TempDescriptionLength
    {
      get
      {
        return TempFlashCardSet.Description.Length + "/" + prMaxDescriptionLength;
      }
    }
    public Brush TempDescriptionLengthColor
    {
      get
      {
        return TempFlashCardSet.Description.Length <= prMaxDescriptionLength ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red);
      }
    }
    public RelayCommand<SizeChangedEventArgs> ResizeColumnWidthCommand { get; private set; }
    public int SetColumnWidth { get; private set; } = 100;
    #endregion

    #region Private Methods
    private async void NavigateHomeAction()
    {
      MessageDialog messageDialog = new MessageDialog("Would you like to save before exiting?");
      messageDialog.Commands.Add(new UICommand("Yes", null));
      messageDialog.Commands.Add(new UICommand("No", null));
      messageDialog.Commands.Add(new UICommand("Cancel", null));
      messageDialog.DefaultCommandIndex = 0;
      messageDialog.CancelCommandIndex = 2;
      IUICommand cmdResult = await messageDialog.ShowAsync();
      if (cmdResult.Label == "Yes") {
        if (await SaveAction(goingHomeAfter: true)) {
          prNavigationService.NavigateTo("MainMenuPage");
        }
      } else if (cmdResult.Label == "No") {
        prNavigationService.NavigateTo("MainMenuPage");
        TempFlashCardSet = null;
      }
    }

    private void InitializeSetPage(CardSetModel cardSetModel, CardSetModel updatedCardSetModel = null)
    {
      if (cardSetModel != null) {
        OriginalFlashCardSet = cardSetModel;
        TempFlashCardSet = updatedCardSetModel != null ? updatedCardSetModel : OriginalFlashCardSet.Clone();
        IsCreatingNewSet = false;
      } else {
        OriginalFlashCardSet = null;
        TempFlashCardSet = new CardSetModel();
        IsCreatingNewSet = true;
      }
      HasMultipleSetsToEdit = false;
      ImportedFlashcardSets = null;
      IndexOfImportedSet = -1;
      OnPropertyChanged("HasMultipleSetsToEdit");
      OnPropertyChanged("ImportedFlashcardSets");
      OnPropertyChanged("IndexOfImportedSet");
      OnPropertyChanged("IndexOfImportedSetDisplay");
      OnPropertyChanged("OriginalFlashCardSet");
      OnPropertyChanged("TempFlashCardSet");
      OnPropertyChanged("IsCreatingNewSet");
    }
    private void InitializeSetPage(List<CardSetModel> cardSetModels)
    {
      if (cardSetModels != null && cardSetModels.Count > 0) {
        OriginalFlashCardSet = null;
        ImportedFlashcardSets = cardSetModels;
        TempFlashCardSet = cardSetModels[0];
        IndexOfImportedSet = 0;
        IsCreatingNewSet = true;
        HasMultipleSetsToEdit = true;
        prIndexOfLastImportSaved = -1;
      } else {
        OriginalFlashCardSet = null;
        ImportedFlashcardSets = null;
        TempFlashCardSet = new CardSetModel();
        IsCreatingNewSet = true;
        HasMultipleSetsToEdit = false;
      }
      OnPropertyChanged("HasMultipleSetsToEdit");
      OnPropertyChanged("ImportedFlashcardSets");
      OnPropertyChanged("NumImportedSets");
      OnPropertyChanged("IndexOfImportedSet");
      OnPropertyChanged("IndexOfImportedSetDisplay");
      OnPropertyChanged("OriginalFlashCardSet");
      OnPropertyChanged("TempFlashCardSet");
      OnPropertyChanged("IsCreatingNewSet");
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

    private async void CancelAction()
    {
      MessageDialog messageDialog = new MessageDialog("Are you sure you want to exit without saving?");
      messageDialog.Commands.Add(new UICommand("Yes", null));
      messageDialog.Commands.Add(new UICommand("No", null));
      messageDialog.DefaultCommandIndex = 0;
      messageDialog.CancelCommandIndex = 1;
      IUICommand cmdResult = await messageDialog.ShowAsync();
      if (cmdResult.Label == "Yes") {
        prNavigationService.GoBack();
        TempFlashCardSet = null;
      }
    }

    private void AddCardAction()
    {
      int indexOfFirstUnstarredCard = -1;
      for (int i = 0; i < TempFlashCardSet.FlashcardCollection.Count; i++) {
        if (!TempFlashCardSet.FlashcardCollection[i].IsStarred) {
          indexOfFirstUnstarredCard = i;
          break;
        }
      }
      TempFlashCardSet.AddCardToSet(indexToAddAt: indexOfFirstUnstarredCard);
    }
    private void StarCardFunction(IndividualCardModel cardToStar)
    {
      if (!cardToStar.IsStarred) {
        for (int i = 0; i < TempFlashCardSet.FlashcardCollection.Count; i++) {
          if (!TempFlashCardSet.FlashcardCollection[i].IsStarred) {
            TempFlashCardSet.FlashcardCollection.Move(TempFlashCardSet.FlashcardCollection.IndexOf(cardToStar), i);
            break;
          }
        }
        cardToStar.IsStarred = true;
      } else {
        TempFlashCardSet.FlashcardCollection.Move(TempFlashCardSet.FlashcardCollection.IndexOf(cardToStar), TempFlashCardSet.FlashcardCollection.Count - 1);
        cardToStar.IsStarred = false;
      }
    }

    private void DeleteCardFunction(IndividualCardModel cardlToDelete)
    {
      TempFlashCardSet.RemoveCardFromSet(cardlToDelete);
    }
    private async Task<bool> SaveAction(bool goingHomeAfter = false)
    {
      if (OriginalFlashCardSet != null) {
        if (!PerformValidation()) {
          await new MessageDialog("Set name and\\or description are too long.").ShowAsync();
          return false;
        }
        OriginalFlashCardSet.Name = TempFlashCardSet.Name;
        OriginalFlashCardSet.Description = TempFlashCardSet.Description;
        OriginalFlashCardSet.IsStarred = TempFlashCardSet.IsStarred;

        foreach (IndividualCardModel editedCard in TempFlashCardSet.FlashcardCollection) {
          int indexOfOriginal = OriginalFlashCardSet.FlashcardCollection.IndexOf(editedCard);
          if (indexOfOriginal > -1) {
            IndividualCardModel originalCard = OriginalFlashCardSet.FlashcardCollection[indexOfOriginal];
            if (originalCard.CardID.Equals(editedCard.CardID)) {
              originalCard.IsLearned = editedCard.IsLearned;
              originalCard.IsStarred = editedCard.IsStarred;
              originalCard.Term = editedCard.Term;
              originalCard.Definition = editedCard.Definition;
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

        for (int i = 0; i < TempFlashCardSet.FlashcardCollection.Count; i++) {
          int indexInOriginal = OriginalFlashCardSet.FlashcardCollection.IndexOf(TempFlashCardSet.FlashcardCollection[i]);
          if (indexInOriginal > -1) {
            OriginalFlashCardSet.FlashcardCollection.Move(indexInOriginal, i);
          }
        }
      } else if (ImportedFlashcardSets == null) {
        if (!PerformValidation()) {
          await new MessageDialog("Set name and\\or description are too long.").ShowAsync();
          return false;
        }
        OriginalFlashCardSet = TempFlashCardSet;
      } else {
        return await NextImportedSetAction(true, goingHomeAfter);
      }

      prNavigationService.NavigateTo("SetPage");
      Messenger.Default.Send(OriginalFlashCardSet, "SetView");
      Messenger.Default.Send(OriginalFlashCardSet, "EditSet");
      return true;
    }

    private async Task<bool> NextImportedSetAction(bool saveBeforeProceeding, bool goingHomeAfter = false)
    {
      if (saveBeforeProceeding) {
        if (!PerformValidation()) {
          await new MessageDialog("Set name and\\or description are too long.").ShowAsync();
          return false;
        }
        Messenger.Default.Send(TempFlashCardSet, "EditSet");
        prIndexOfLastImportSaved = ImportedFlashcardSets.IndexOf(TempFlashCardSet);
      }

      if (goingHomeAfter) {
        return true;
      }

      if (++IndexOfImportedSet < ImportedFlashcardSets.Count) {
        TempFlashCardSet = ImportedFlashcardSets[IndexOfImportedSet];
        TempName = TempFlashCardSet.Name;
        TempDescription = TempFlashCardSet.Description;
        OnPropertyChanged("IndexOfImportedSet");
        OnPropertyChanged("IndexOfImportedSetDisplay");
        OnPropertyChanged("TempFlashCardSet");
      } else {
        if (prIndexOfLastImportSaved > -1) {
          prNavigationService.NavigateTo("SetPage");
          Messenger.Default.Send(ImportedFlashcardSets[prIndexOfLastImportSaved], "SetView");
        } else {
          prNavigationService.NavigateTo("MainMenuPage");
        }
        ImportedFlashcardSets = null;
      }
      return true;
    }

    private async void NextImportedSetWithoutSavingAction()
    {
      MessageDialog messageDialog = new MessageDialog("Do you want to save before proceeding?");
      messageDialog.Commands.Add(new UICommand("Yes", null));
      messageDialog.Commands.Add(new UICommand("No", null));
      messageDialog.Commands.Add(new UICommand("Cancel", null));
      messageDialog.DefaultCommandIndex = 0;
      messageDialog.CancelCommandIndex = 2;
      IUICommand cmdResult = await messageDialog.ShowAsync();
      if (cmdResult.Label == "Yes") {
        await NextImportedSetAction(true);
      } else if (cmdResult.Label == "No") {
        await NextImportedSetAction(false);
      }
    }

    private bool PerformValidation()
    {
      if (TempFlashCardSet.Name.Length > prMaxTermLength || TempFlashCardSet.Description.Length > prMaxDescriptionLength) {
        return false;
      }
      return true;
    }
    #endregion
  }
}
