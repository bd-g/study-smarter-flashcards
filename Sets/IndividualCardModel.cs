using System;
using System.Collections.Generic;
using StudySmarterFlashcards.Utils;

namespace StudySmarterFlashcards.Sets
{
  class IndividualCardModel
  {
    #region Constructors
    public IndividualCardModel(string term = "", string definition = "")
    {
      Term = term;
      Definition = definition;
    }
    #endregion

    #region Properties
    public string Term { get; set; }
    public string Definition { get; set; }
    public Guid CardID { get; } = Guid.NewGuid();
    public bool IsLearned { get; set; } = false;
    public bool IsArchived { get; set; } = false;
    #endregion

    #region Public Methods
    public override bool Equals(object obj)
    {
      return obj is IndividualCardModel model &&
             Term == model.Term &&
             Definition == model.Definition &&
             CardID.Equals(model.CardID) &&
             IsLearned == model.IsLearned &&
             IsArchived == model.IsArchived;
    }

    public override int GetHashCode()
    {
      int hashCode = 849365176;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Term);
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Definition);
      hashCode = hashCode * -1521134295 + CardID.GetHashCode();
      hashCode = hashCode * -1521134295 + IsLearned.GetHashCode();
      hashCode = hashCode * -1521134295 + IsArchived.GetHashCode();
      return hashCode;
    }
    #endregion
  }
}
