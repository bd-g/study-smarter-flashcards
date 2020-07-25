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
  public class SetViewModel : BaseViewModel
  {
    #region Constructors
    public SetViewModel(INavigationService navigationService) : base(navigationService)
    {
      Messenger.Default.Register<CardSetModel>(this, cardSetModel => InitializeSetPage(cardSetModel));
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      EditCommand = new RelayCommand(EditAction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand EditCommand { get; private set; }
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
      } else {
        FlashCardSet = new CardSetModel();
        Messenger.Default.Send<AddSetMessage>(new AddSetMessage(FlashCardSet));
      }
      OnPropertyChanged("FlashCardSet");
    }

    private void EditAction()
    {
      prNavigationService.NavigateTo("EditSetViewModel");
      Messenger.Default.Send(FlashCardSet);
    }
    #endregion
  }
}
