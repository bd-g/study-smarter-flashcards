using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StudySmarterFlashcards.Utils;

namespace StudySmarterFlashcards.Sets
{
  public class IndividualCardModel
  {
    #region Constructors
    public IndividualCardModel(string term = "New Term", string definition = "New Definition")
    {
      Term = term;
      Definition = definition;
    }
    
    [JsonConstructor]
    public IndividualCardModel(string term, string definition, Guid cardID, bool isLearned, bool isArchived)
    {
      Term = term;
      Definition = definition;
      CardID = cardID;
      IsLearned = isLearned;
      IsArchived = isArchived;
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
    public IndividualCardModel Clone()
    {
      return new IndividualCardModel(this.Term, this.Definition, this.CardID, this.IsLearned, this.IsArchived);
    }

    public bool DeepEquals(object obj)
    {
      return obj is IndividualCardModel model &&
             Term == model.Term &&
             Definition == model.Definition &&
             IsLearned == model.IsLearned &&
             IsArchived == model.IsArchived;
    }
    public override bool Equals(object obj)
    {
      return obj is IndividualCardModel model &&
             CardID == model.CardID;
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
