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
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace StudySmarterFlashcards.Sets
{
  public class EditSetViewModel : BaseViewModel
  {
    #region Constructors
    public EditSetViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, "EditSetView", cardSetModel => InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      CancelCommand = new RelayCommand(CancelAction);
      SaveCommand = new RelayCommand(SaveAction);
      AddCardCommand = new RelayCommand(AddCardAction);
      ArchiveCardCommand = new RelayCommand<IndividualCardModel>(ArchiveCardFunction);
      DeleteCardCommand = new RelayCommand<IndividualCardModel>(DeleteCardFunction);
      ResizeColumnWidthCommand = new RelayCommand<SizeChangedEventArgs>(ResizeColumnWidthFunction);
    }
    #endregion

    #region Properties
    public bool IsCreatingNewSet { get; private set; } = true;
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand CancelCommand { get; private set; }
    public RelayCommand SaveCommand { get; private set; }
    public RelayCommand AddCardCommand { get; private set; }
    public RelayCommand<IndividualCardModel> ArchiveCardCommand { get; private set; }
    public RelayCommand<IndividualCardModel> DeleteCardCommand { get; private set; }
    public CardSetModel OriginalFlashCardSet { get; private set; } = new CardSetModel();
    public CardSetModel TempFlashCardSet { get; private set; } = new CardSetModel();
    public string TempName
    {
      get {
        return TempFlashCardSet.Name;
      }
      set {
        TempFlashCardSet.Name = value;
        OnPropertyChanged("TempName");
        OnPropertyChanged("TempNameLength");
        OnPropertyChanged("TempNameLengthColor");
      }
    }
    public string TempNameLength
    {
      get {
        return TempName.Length + "/30";
      }
    }
    public Brush TempNameLengthColor
    {
      get {
        return TempName.Length <= 30 ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red);
      }
    }
    public string TempDescription
    {
      get {
        return TempFlashCardSet.Description;
      }
      set {
        TempFlashCardSet.Description = value;
        OnPropertyChanged("TempDescription");
        OnPropertyChanged("TempDescriptionLength");
        OnPropertyChanged("TempDescriptionLengthColor");
      }
    }
    public string TempDescriptionLength
    {
      get {
        return TempFlashCardSet.Description.Length + "/150";
      }
    }
    public Brush TempDescriptionLengthColor
    {
      get {
        return TempFlashCardSet.Description.Length <= 150 ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red);
      }
    }
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
      MessageDialog messageDialog = new MessageDialog("Are you sure you want to continue without saving?");
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
      int indexOfFirstArchivedCard = -1;
      for (int i = 0; i < TempFlashCardSet.FlashcardCollection.Count; i++) {
        if (TempFlashCardSet.FlashcardCollection[i].IsArchived) {
          indexOfFirstArchivedCard = i;
          break;
        }
      }
      TempFlashCardSet.AddCardToSet(indexToAddAt:indexOfFirstArchivedCard);
    }
    private void ArchiveCardFunction(IndividualCardModel cardToArchive)
    {
      if (cardToArchive.IsArchived) {
        for (int i = 0; i < TempFlashCardSet.FlashcardCollection.Count; i++) {
          if (TempFlashCardSet.FlashcardCollection[i].IsArchived) {
            TempFlashCardSet.FlashcardCollection.Move(TempFlashCardSet.FlashcardCollection.IndexOf(cardToArchive), i);
            break;
          }
        }
        cardToArchive.IsArchived = false;
      } else {
        TempFlashCardSet.FlashcardCollection.Move(TempFlashCardSet.FlashcardCollection.IndexOf(cardToArchive), TempFlashCardSet.FlashcardCollection.Count - 1);
        cardToArchive.IsArchived = true;
      }      
    }

    private void DeleteCardFunction(IndividualCardModel cardlToDelete)
    {
      TempFlashCardSet.RemoveCardFromSet(cardlToDelete);  
    }
    private async void SaveAction()
    {
      if (OriginalFlashCardSet != null) {
        if (!PerformValidation()) {
          await new MessageDialog("Set name and\\or description are too long.").ShowAsync();
          return;
        }
        OriginalFlashCardSet.Name = TempFlashCardSet.Name;
        OriginalFlashCardSet.Description = TempFlashCardSet.Description;
        OriginalFlashCardSet.IsArchived = TempFlashCardSet.IsArchived;

        foreach (IndividualCardModel editedCard in TempFlashCardSet.FlashcardCollection) {
          int indexOfOriginal = OriginalFlashCardSet.FlashcardCollection.IndexOf(editedCard);
          if (indexOfOriginal > -1) {
            IndividualCardModel originalCard = OriginalFlashCardSet.FlashcardCollection[indexOfOriginal];
            if (originalCard.CardID.Equals(editedCard.CardID)) {
              originalCard.IsLearned = editedCard.IsLearned;
              originalCard.IsArchived = editedCard.IsArchived;
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
      } else {
        OriginalFlashCardSet = TempFlashCardSet;
      }

      prNavigationService.NavigateTo("SetPage");
      Messenger.Default.Send(OriginalFlashCardSet, "SetView");
      Messenger.Default.Send(OriginalFlashCardSet, "EditSet");
    }

    private bool PerformValidation()
    {
      if (TempFlashCardSet.Name.Length > 30 || TempFlashCardSet.Description.Length > 150) {
        return false;
      }
      return true;
    }
    #endregion
  }
}
