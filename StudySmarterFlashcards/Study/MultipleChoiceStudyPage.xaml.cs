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
  public sealed partial class MultipleChoiceStudyPage : Page
  {
    #region Constructors
    public MultipleChoiceStudyPage()
    {
      this.InitializeComponent();
    }

    private void AttachUniversalKeyHandler(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is MultipleChoiceStudyViewModel viewModel) {
        Window.Current.CoreWindow.KeyDown += viewModel.KeyDownFunction;
      }
    }
    private void DetachUniversalKeyHandler(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is MultipleChoiceStudyViewModel viewModel) {
        Window.Current.CoreWindow.KeyDown -= viewModel.KeyDownFunction;
      }
    }
    #endregion
  }
}
