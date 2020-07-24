using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySmarterFlashcards.Sets
{
  public class CardSetModel
  {
    #region Fields
    private List<IndividualCardModel> prFlashcardCollection = new List<IndividualCardModel>();
    #endregion

    #region Constructors
    public CardSetModel(string name = "New Flashcard Set", string description = "")
    {
      Name = name;
      Description = description;
    }
    #endregion

    #region Properties
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid SetID { get; } = Guid.NewGuid();
    public List<IndividualCardModel> FlashcardCollection { 
      get 
      {
        return prFlashcardCollection;
      }
      set
      {
        prFlashcardCollection = value;
      }
    }
    public double LearningProgress
    {
      get
      {
        int numLearned = prFlashcardCollection.Count(x => x.IsLearned);
        return (double)numLearned/prFlashcardCollection.Count * 100;
      }
    }
    public int NumTimesReviewed { get; set; } = 0;
    public DateTime WhenCreated { get; } = DateTime.Now;
    public DateTime WhenLastReviewedUTC { get; set; } = DateTime.MinValue;
    public bool IsArchived { get; set; } = false;

    #endregion

    #region Public Methods
    public void AddCardToSet(string cardTerm = "", string cardDefinition = "")
    {
      prFlashcardCollection.Add(new IndividualCardModel(cardTerm, cardDefinition));
    }

    public void RemoveCardFromSet(Guid guidOfCardToRemove)
    {
      IndividualCardModel cardToDelete = prFlashcardCollection.Single(x => x.CardID.Equals(guidOfCardToRemove));
      prFlashcardCollection.Remove(cardToDelete);
    }

    public void EditCardInSet(Guid guidofCardToEdit, string newTerm = "", string newDefinition = "")
    {
      IndividualCardModel cardToEdit = prFlashcardCollection.Single(x => x.CardID.Equals(guidofCardToEdit));

      if (!String.IsNullOrWhiteSpace(newTerm)) {
        cardToEdit.Term = newTerm;
      }
      if (!String.IsNullOrWhiteSpace(newDefinition)) {
        cardToEdit.Definition = newDefinition;
      }
    }

    public void EditCardSwitchIsLearned(Guid guidofCardToEdit)
    {
      IndividualCardModel cardToEdit = prFlashcardCollection.Single(x => x.CardID.Equals(guidofCardToEdit));
      cardToEdit.IsLearned = !cardToEdit.IsLearned;
    }

    public void EditCardSwitchIsArchived(Guid guidofCardToEdit)
    {
      IndividualCardModel cardToEdit = prFlashcardCollection.Single(x => x.CardID.Equals(guidofCardToEdit));
      cardToEdit.IsArchived = !cardToEdit.IsArchived;
    }

    public override bool Equals(object obj)
    {
      return obj is CardSetModel model &&
             Name == model.Name &&
             Description == model.Description &&
             SetID.Equals(model.SetID) &&
             EqualityComparer<List<IndividualCardModel>>.Default.Equals(FlashcardCollection, model.FlashcardCollection) &&
             LearningProgress == model.LearningProgress &&
             NumTimesReviewed == model.NumTimesReviewed &&
             WhenCreated == model.WhenCreated &&
             WhenLastReviewedUTC == model.WhenLastReviewedUTC &&
             IsArchived == model.IsArchived;
    }

    public override int GetHashCode()
    {
      int hashCode = 2146425462;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
      hashCode = hashCode * -1521134295 + SetID.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<List<IndividualCardModel>>.Default.GetHashCode(FlashcardCollection);
      hashCode = hashCode * -1521134295 + LearningProgress.GetHashCode();
      hashCode = hashCode * -1521134295 + NumTimesReviewed.GetHashCode();
      hashCode = hashCode * -1521134295 + WhenCreated.GetHashCode();
      hashCode = hashCode * -1521134295 + WhenLastReviewedUTC.GetHashCode();
      hashCode = hashCode * -1521134295 + IsArchived.GetHashCode();
      return hashCode;
    }
    #endregion
  }
}
