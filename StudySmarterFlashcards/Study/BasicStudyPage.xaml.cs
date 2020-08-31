using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StudySmarterFlashcards.Study
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class BasicStudyPage : Page
  {
    public BasicStudyPage()
    {
      this.InitializeComponent();
    }
    private void AttachUniversalKeyHandler(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is BasicStudyViewModel viewModel) {
        Window.Current.CoreWindow.KeyUp += viewModel.KeyUpFunction;
      }
    }
    private void DetachUniversalKeyHandler(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is BasicStudyViewModel viewModel) {
        Window.Current.CoreWindow.KeyUp -= viewModel.KeyUpFunction;
      }
    }
  }
}
