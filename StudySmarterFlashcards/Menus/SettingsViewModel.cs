using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Dialogs;
using StudySmarterFlashcards.Utils;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StudySmarterFlashcards.Menus
{
  public class SettingsViewModel : BaseViewModel
  {
    #region Fields
    private bool prShowFillBlankStudyInstructions;
    private bool prShowBasicStudyInstructions;
    private bool prShowMainInstructions;
    private bool prShowMultipleChoiceInstructions;
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
      ToggleFillBlankStudyInstructionsCommand = new RelayCommand<RoutedEventArgs>(ToggleFillBlankInstructionsFunction);
      ToggleMultipleChoiceStudyInstructionsCommand = new RelayCommand<RoutedEventArgs>(ToggleMultipleChoiceStudyInstructionsFunction);
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
    public RelayCommand<RoutedEventArgs> ToggleFillBlankStudyInstructionsCommand { get; private set; }
    public RelayCommand<RoutedEventArgs> ToggleMultipleChoiceStudyInstructionsCommand { get; private set; }
    public bool IsFeedbackHubSupported
    {
      get
      {
        return false; // Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported();
      }
    }
    public bool ShowStudyInstructions
    {
      get
      {
        return prShowBasicStudyInstructions;
      }
      private set
      {
        if (value != prShowBasicStudyInstructions) {
          prShowBasicStudyInstructions = value;
          OnPropertyChanged();
        }
      }
    }
    public bool ShowFillBlankInstructions
    {
      get
      {
        return prShowFillBlankStudyInstructions;
      }
      private set
      {
        if (value != prShowFillBlankStudyInstructions) {
          prShowFillBlankStudyInstructions = value;
          OnPropertyChanged();
        }
      }
    }
    public bool ShowMultipleChoiceInstructions
    {
      get
      {
        return prShowMultipleChoiceInstructions;
      }
      private set
      {
        if (value != prShowMultipleChoiceInstructions) {
          prShowMultipleChoiceInstructions = value;
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
      bool? showStudyInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowBasicStudyInstructionsDialog"] as bool?;
      ShowStudyInstructions = showStudyInstructions == false ? false : true;
      bool? showMainInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMainInstructionsDialog"] as bool?;
      ShowMainInstructions = showMainInstructions == false ? false : true;
      bool? showFillBlankStudyInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowFillBlankStudyInstructionsDialog"] as bool?;
      ShowFillBlankInstructions = showFillBlankStudyInstructions == false ? false : true;
      bool? showMultipleChoiceStudyInstructions = Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMultipleChoiceStudyInstructionsDialog"] as bool?;
      ShowMultipleChoiceInstructions = showMultipleChoiceStudyInstructions == false ? false : true;
    }
    #endregion

    #region Private Methods
    private void NavigateHomeAction()
    {
      prNavigationService.NavigateTo("MainMenuPage");
    }

    private async void LaunchFeedbackHubAction()
    {
      await new MessageDialog("Feedback Hub is not currently supported. Please submit feedback to the email listed above.").ShowAsync();
      //var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
      //await launcher.LaunchAsync();
    }

    private void ToggleStudyInstructionsFunction(RoutedEventArgs args)
    {
      ToggleSwitch toggleSwitch = args.OriginalSource as ToggleSwitch;
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowBasicStudyInstructionsDialog"] = toggleSwitch.IsOn;
      ShowStudyInstructions = toggleSwitch.IsOn;
    }
    private void ToggleMainInstructionsFunction(RoutedEventArgs args)
    {
      ToggleSwitch toggleSwitch = args.OriginalSource as ToggleSwitch;
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMainInstructionsDialog"] = toggleSwitch.IsOn;
      ShowMainInstructions = toggleSwitch.IsOn;
    }
    private void ToggleFillBlankInstructionsFunction(RoutedEventArgs args)
    {
      ToggleSwitch toggleSwitch = args.OriginalSource as ToggleSwitch;
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowFillBlankStudyInstructionsDialog"] = toggleSwitch.IsOn;
      ShowFillBlankInstructions = toggleSwitch.IsOn;
    }
    private void ToggleMultipleChoiceStudyInstructionsFunction(RoutedEventArgs args)
    {
      ToggleSwitch toggleSwitch = args.OriginalSource as ToggleSwitch;
      Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowMultipleChoiceStudyInstructionsDialog"] = toggleSwitch.IsOn;
      ShowMultipleChoiceInstructions = toggleSwitch.IsOn;
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
