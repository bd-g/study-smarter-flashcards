using GalaSoft.MvvmLight.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudySmarterFlashcards.Utils
{
  public class BaseViewModel : IErrorHandler, INotifyPropertyChanged
  {
    #region Fields
    protected static readonly NLog.Logger prNLogLogger = NLog.LogManager.GetCurrentClassLogger();
    protected readonly INavigationService prNavigationService;
    #endregion

    #region Constructors
    protected BaseViewModel(INavigationService navigationService)
    {
      prNavigationService = navigationService;
    }
    #endregion

    #region Events
    public virtual event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Public Methods
    public void HandleError(Exception ex)
    {
      prNLogLogger.Error(ex);
    }
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
  }
}
