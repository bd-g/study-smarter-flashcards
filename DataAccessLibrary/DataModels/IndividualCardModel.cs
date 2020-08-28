using System;
using System.Collections.Generic;

namespace DataAccessLibrary.DataModels
{
  public class IndividualCardModel
  {
    #region Constructors
    public IndividualCardModel(string term = "New Term", string definition = "New Definition", bool? isLearned = false, bool? isStarred = true)
    {
      Term = term;
      Definition = definition;
      IsLearned = isLearned == null ? false : (bool)isLearned;
      IsStarred = isStarred == null ? true : (bool)isStarred;
    }
    public IndividualCardModel(string term, string definition, Guid cardID, bool isLearned, bool isStarred)
    {
      Term = term;
      Definition = definition;
      CardID = cardID;
      IsLearned = isLearned;
      IsStarred = isStarred;
    }
    #endregion

    #region Properties
    public string Term { get; set; }
    public string Definition { get; set; }
    public Guid CardID { get; } = Guid.NewGuid();
    public bool IsLearned { get; set; }
    public bool IsStarred { get; set; }
    #endregion

    #region Public Methods
    public IndividualCardModel Clone()
    {
      return new IndividualCardModel(this.Term, this.Definition, this.CardID, this.IsLearned, this.IsStarred);
    }

    public bool DeepEquals(object obj)
    {
      return obj is IndividualCardModel model &&
             Term == model.Term &&
             Definition == model.Definition &&
             IsLearned == model.IsLearned &&
             IsStarred == model.IsStarred;
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
      hashCode = hashCode * -1521134295 + IsStarred.GetHashCode();
      return hashCode;
    }
    #endregion
  }
}
