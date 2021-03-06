﻿using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StudySmarterFlashcards.Menus
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class SettingsPage : Page
  {
    public SettingsPage()
    {
      this.InitializeComponent();
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      if (DataContext is SettingsViewModel) {
        SettingsViewModel dataContext = DataContext as SettingsViewModel;
        dataContext.UpdateSettings();
      }
      base.OnNavigatedTo(e);
    }
  }
}
