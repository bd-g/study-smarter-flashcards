using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StudySmarterFlashcards.Study
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class FillBlankStudyPage : Page
  {
    #region Fields
    private static readonly object prLocker = new object();
    #endregion

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
      }
    }
    private void DetachUniversalKeyHandler(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is FillBlankStudyViewModel) {
        Window.Current.CoreWindow.KeyDown -= (this.DataContext as FillBlankStudyViewModel).KeyDownFunction;
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
