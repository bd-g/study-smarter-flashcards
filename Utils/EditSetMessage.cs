using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudySmarterFlashcards.Sets;

namespace StudySmarterFlashcards.Utils
{
  #region Outer Enums
  public enum SetStatus
  {
    Active,
    Archived,
    Deleted
  }
  #endregion  

  public class EditSetMessage
  {
    #region Constructors
    public EditSetMessage(CardSetModel editedSet)
    {
      EditedSet = editedSet;
    }
    #endregion

    #region Properties
    public CardSetModel EditedSet { get; private set;}
    #endregion
  }
}
