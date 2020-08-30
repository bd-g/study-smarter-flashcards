using GalaSoft.MvvmLight.Messaging;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StudySmarterFlashcards.Study
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class FillBlankStudyPage : Page
  {
    #region Constructors
    public FillBlankStudyPage()
    {
      Messenger.Default.Register<bool>(this, "CharacterGuess", async (isGuessCorrect) => await MakeCharacterGuess(isGuessCorrect));
      Messenger.Default.Register<bool>(this, "FillBlankHint", async (revealedWholeWord) => await AnimateHint(revealedWholeWord));
      this.InitializeComponent();
    }
    #endregion

    #region Private Methods
    private async Task AnimateHint(bool revealedWholeWord)
    {
      if (!revealedWholeWord) {
        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SingleHintAnimation.BeginAsync());
      } else {
        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => RevealWordAnimation.BeginAsync());
      }
    }
    private async Task MakeCharacterGuess(bool isGuessCorrect)
    {
      if (!isGuessCorrect) {
        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => WrongGuessAnimation.BeginAsync());
      } else {
        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => CompleteAnswerAnimation.BeginAsync());
      }
    }
    private void AttachUniversalKeyHandler(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is FillBlankStudyViewModel) {
        Window.Current.CoreWindow.KeyDown += (this.DataContext as FillBlankStudyViewModel).KeyDownFunction;
        Window.Current.CoreWindow.CharacterReceived += (this.DataContext as FillBlankStudyViewModel).CharacterReceivedFunction;
      }
    }
    private void DetachUniversalKeyHandler(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is FillBlankStudyViewModel) {
        Window.Current.CoreWindow.KeyDown -= (this.DataContext as FillBlankStudyViewModel).KeyDownFunction;
        Window.Current.CoreWindow.CharacterReceived -= (this.DataContext as FillBlankStudyViewModel).CharacterReceivedFunction;
      }
    }
    private void HandleSpaceBarPress(object sender, KeyRoutedEventArgs e)
    {
      if (e.Key == VirtualKey.Space) {
        if (this.DataContext is FillBlankStudyViewModel) {
          (this.DataContext as FillBlankStudyViewModel).KeyDownFunction(e.Key, null);
        }
        e.Handled = true;
      } else if (e.Key == VirtualKey.Enter) {
        if (this.DataContext is FillBlankStudyViewModel) {
          (this.DataContext as FillBlankStudyViewModel).KeyDownFunction(e.Key, null);
        }
        e.Handled = true;
      }
    }
    #endregion
  }
}
