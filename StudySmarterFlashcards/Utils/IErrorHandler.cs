using System;

namespace StudySmarterFlashcards.Utils
{
  public interface IErrorHandler
  {
    void HandleError(Exception ex);
  }

}
