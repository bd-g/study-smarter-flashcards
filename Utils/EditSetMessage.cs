using System;
using System.Collections.Generic;
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
    public EditSetMessage(Guid setID, string name, string description, List<IndividualCardModel> editedCards, SetStatus setStatus)
    {
      SetID = setID;
      Name = name;
      Description = description;
      EditedCards = editedCards;
      SetStatus = setStatus;
    }
    #endregion

    #region Properties
    public Guid SetID { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<IndividualCardModel> EditedCards { get; private set; }
    public SetStatus SetStatus { get; private set; }
    #endregion
  }
}
