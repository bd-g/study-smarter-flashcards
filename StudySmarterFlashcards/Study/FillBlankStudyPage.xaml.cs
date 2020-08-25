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
using Windows.UI;
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
      this.InitializeComponent();
    }
    #endregion

    #region Private Methods
    private async Task MakeCharacterGuess(bool isGuessCorrect)
    {
      if (!isGuessCorrect) {
        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => WrongAnswer.BeginAsync());
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
    #endregion
  }
}
