using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySmarterFlashcards.Utils
{
  public class NotifyPropertyChanged : INotifyPropertyChanged
  {
    public virtual event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
