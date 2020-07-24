using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudySmarterFlashcards.Sets;
using StudySmarterFlashcards.Utils;

namespace StudySmarterFlashcards.Menus
{
  class MainMenuViewModel : NotifyPropertyChanged, IErrorHandler
  {
    #region Fields
    private static readonly NLog.Logger prNLogLogger = NLog.LogManager.GetCurrentClassLogger();
    #endregion

    #region Constructors
    public MainMenuViewModel()
    {
      NumSetsLoaded = new NotifyTaskCompletion<string>(this.LoadStartingData());
    }
    #endregion

    #region Properties
    public NotifyTaskCompletion<string> NumSetsLoaded { get; private set; }

    public ObservableCollection<CardSetModel> CardSets { get; set; }
    #endregion

    #region Public Methods
    public void HandleError(Exception ex)
    {
      prNLogLogger.Error(ex);
    }
    #endregion

    #region Private Methods
    private async Task<string> LoadStartingData()
    {
      CardSets = await LocalDataHandler.LoadAllSetsFromLocalMemory();
      return CardSets.Count + " sets loaded successfully";
    }
    #endregion
  }
}
