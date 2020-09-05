using DataAccessLibrary;
using DataAccessLibrary.DataModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Dialogs;
using StudySmarterFlashcards.ImportTools;
using StudySmarterFlashcards.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
      AddToSetWithImportCommand = new RelayCommand(ImportSetFromFileAction);
      StudyCommand = new RelayCommand<string>(StudyAction);
      ResizeColumnWidthCommand = new RelayCommand<SizeChangedEventArgs>(ResizeColumnWidthFunction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand EditCommand { get; private set; }
    public RelayCommand AddToSetWithImportCommand { get; private set; }
    public RelayCommand<string> StudyCommand { get; private set; }
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

    private async void ImportSetFromFileAction()
    {
      FileOpenPicker openPicker = new FileOpenPicker();
      openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
      openPicker.FileTypeFilter.Add(".xlsx");
      openPicker.FileTypeFilter.Add(".xls");
      openPicker.FileTypeFilter.Add(".docx");
      openPicker.FileTypeFilter.Add(".doc");

      StorageFile file = await openPicker.PickSingleFileAsync();
      if (file != null) {
        try {
          CancellationTokenSource cancelSource = new CancellationTokenSource();
          Task<ContentDialogResult> loadingScreenTask = new LoadingDialog().ShowAsync().AsTask(cancelSource.Token);
          Task<CardSetModel> importingTask = ImportFlashcardService.ImportAddToExistingSet(file, cancelSource.Token, FlashCardSet.Clone());

          Task firstToFinish = await Task.WhenAny(loadingScreenTask, importingTask);
          cancelSource.Cancel();
          if (firstToFinish == importingTask) {
            CardSetModel updatedCardSetModel = await importingTask;
            prNavigationService.NavigateTo("EditSetPage");
            Messenger.Default.Send(new Tuple<CardSetModel, CardSetModel>(FlashCardSet, updatedCardSetModel), "EditSetView");
          }
        } catch (Exception ex) {
          await new MessageDialog(ex.Message).ShowAsync();
        }
      }
    }

    private async void StudyAction(string studyMode)
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

      string studyPage = null;
      string studyView = null;
      if (studyMode == "BasicStudyMode") {
        studyPage = "BasicStudyPage";
        studyView = "StudyView";
      } else if (studyMode == "FillBlankStudyMode") {
        studyPage = "FillBlankStudyPage";
        studyView = "FillBlankStudyView";
      } else if (studyMode == "MultipleChoiceStudyMode") {
        studyPage = "MultipleChoiceStudyPage";
        studyView = "MultipleChoiceStudyView";
      } else {
        return;
      }

      prNavigationService.NavigateTo(studyPage);
      Messenger.Default.Send(FlashCardSet, studyView);
      FlashCardSet.RegisterNewReviewSession();
      DataAccess.EditCardSetRegisterNewReviewSession_UWP(FlashCardSet);
    }
    #endregion
  }
}
