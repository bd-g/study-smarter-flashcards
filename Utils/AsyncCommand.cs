using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StudySmarterFlashcards.Utils
{
  #region Interfaces
  public interface IAsyncCommand : ICommand
  {
    Task ExecuteAsync();
    bool CanExecute();
  }
  #endregion

  #region Classes
  public class AsyncCommand : IAsyncCommand
  {
    #region Fields
    private bool prIsExecuting;
    private readonly Func<Task> prExecute;
    private readonly Func<bool> prCanExecute;
    private readonly IErrorHandler prErrorHandler;
    #endregion

    #region Events
    public event EventHandler CanExecuteChanged;
    #endregion

    #region Constructors
    public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null, IErrorHandler errorHandler = null)
    {
      prExecute = execute;
      prCanExecute = canExecute;
      prErrorHandler = errorHandler;
    }
    #endregion

    #region Public Methods
    public bool CanExecute()
    {
      return !prIsExecuting && (prCanExecute?.Invoke() ?? true);
    }

    public async Task ExecuteAsync()
    {
      if (CanExecute()) {
        try {
          prIsExecuting = true;
          await prExecute();
        } finally {
          prIsExecuting = false;
        }
      }

      RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Explicit Implementations
    bool ICommand.CanExecute(object parameter)
    {
      return CanExecute();
    }

    void ICommand.Execute(object parameter)
    {
      ExecuteAsync().FireAndForgetSafeAsync(prErrorHandler);
    }
    #endregion
  }
  #endregion
}
