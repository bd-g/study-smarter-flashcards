using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Utils;

namespace StudySmarterFlashcards.Menus
{
  public class SettingsViewModel : BaseViewModel
  {
    #region Constructors
    public SettingsViewModel(INavigationService navigationService) : base(navigationService)
    {
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      LaunchFeedbackHubCommand = new RelayCommand(LaunchFeedbackHubAction);
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand LaunchFeedbackHubCommand { get; private set; }
    public bool IsFeedbackHubSupported {
      get {
        return Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported();
      } 
    }
    #endregion

    #region Private Methods
    private void NavigateHomeAction()
    {
      prNavigationService.NavigateTo("MainMenuPage");
    }

    private async void LaunchFeedbackHubAction()
    {
      var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
      await launcher.LaunchAsync();
    }
    #endregion  
  }
}
