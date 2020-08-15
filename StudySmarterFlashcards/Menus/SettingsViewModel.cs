using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Dialogs;
using StudySmarterFlashcards.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace StudySmarterFlashcards.Menus
{
  public class SettingsViewModel : BaseViewModel
  {
    #region Fields
    private bool prShowStudyInstructions;
    private bool prShowMainInstructions;
    #endregion

    #region Constructors
    public SettingsViewModel(INavigationService navigationService) : base(navigationService)
    {
      NavigateHomeCommand = new RelayCommand(NavigateHomeAction);
      LaunchFeedbackHubCommand = new RelayCommand(LaunchFeedbackHubAction);
      ShowMainInstructionsCommand = new RelayCommand(ShowMainInstructionsAction);
      ShowValidFileFormatsCommand = new RelayCommand(ShowValidFileFormatsAction);
      ToggleStudyInstructionsCommand = new RelayCommand<RoutedEventArgs>(ToggleStudyInstructionsFunction);
      ToggleMainInstructionsCommand = new RelayCommand<RoutedEventArgs>(ToggleMainInstructionsFunction);
      UpdateSettings();
    }
    #endregion

    #region Properties
    public RelayCommand NavigateHomeCommand { get; private set; }
    public RelayCommand LaunchFeedbackHubCommand { get; private set; }
    public RelayCommand ShowMainInstructionsCommand { get; private set; }
    public RelayCommand ShowValidFileFormatsCommand { get; private set; }
    public RelayCommand<RoutedEventArgs> ToggleStudyInstructionsCommand { get; private set; }
    public RelayCommand<RoutedEventArgs> ToggleMainInstructionsCommand { get; private set; }
    public bool IsFeedbackHubSupported {
      get {
        return Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported();
      } 
    }
    public bool ShowStudyInstructions
    {
      get
      {
        return prShowStudyInstructions;
      }
      private set
      {
        if (value != prShowStudyInstructions) {
          prShowStudyInstructions = value;
          OnPropertyChanged();
        }
      }
    }
    public bool ShowMainInstructions
    {
      get
      {
        return prShowMainInstructions;
      }
      private set
      {
        if (value != prShowMainInstructions) {
          prShowMainInstructions = value;
          OnPropertyChanged();
        }
      }
    }
    #endregion

    #region Public Methods
    public void UpdateSettings()
    {
      bool? showStudyInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowStudyInstructionsDialog"] as bool?;
      ShowStudyInstructions = showStudyInstructions == true ? true : false;
      bool? showMainInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMainInstructionsDialog"] as bool?;
      ShowMainInstructions = showMainInstructions == true ? true : false;
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

    private void ToggleStudyInstructionsFunction(RoutedEventArgs args)
    {
      ToggleSwitch toggleSwitch = args.OriginalSource as ToggleSwitch;
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowStudyInstructionsDialog"] = toggleSwitch.IsOn;
      ShowStudyInstructions = toggleSwitch.IsOn;
    }
    private void ToggleMainInstructionsFunction(RoutedEventArgs args)
    {
      ToggleSwitch toggleSwitch = args.OriginalSource as ToggleSwitch;
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMainInstructionsDialog"] = toggleSwitch.IsOn;
      ShowMainInstructions = toggleSwitch.IsOn;
    }

    private async void ShowMainInstructionsAction()
    {
      await InstructionsDialogService.ShowAsync(InstructionDialogType.MainInstructions, true);
      UpdateSettings();
    }

    private async void ShowValidFileFormatsAction()
    {
      await InstructionsDialogService.ShowAsync(InstructionDialogType.ValidFileFormats);
    }
    #endregion
  }
}
