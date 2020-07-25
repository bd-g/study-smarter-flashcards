using System;
using System.Collections.Generic;
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
    }
    #endregion

    #region Properties
    public bool IsCreatingNewSet { get; private set; } = true;
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand CancelCommand { get; private set; }
    public RelayCommand SaveCommand { get; private set; }
    public RelayCommand AddCardCommand { get; private set; }
    public CardSetModel FlashCardSet { get; private set; } = new CardSetModel();
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
        IsCreatingNewSet = false;
      } else {
        FlashCardSet = new CardSetModel();
        IsCreatingNewSet = true;
      }
      OnPropertyChanged("FlashCardSet");
      OnPropertyChanged("IsCreatingNewSet");
    }

    private void CancelAction()
    {
      prNavigationService.GoBack();
    }

    private void AddCardAction()
    {
      FlashCardSet.AddCardToSet();
    }
    private void SaveAction()
    {
      Messenger.Default.Send(new AddSetMessage(FlashCardSet));
      Messenger.Default.Send(FlashCardSet);
      prNavigationService.NavigateTo("SetPage");
    }
    #endregion
  }
}
