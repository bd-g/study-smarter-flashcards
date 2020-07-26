using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudySmarterFlashcards.Sets
{
  public class CardSetModel 
  {
    #region Fields
    protected static readonly NLog.Logger prNLogLogger = NLog.LogManager.GetCurrentClassLogger();
    #endregion


    #region Constructors
    public CardSetModel(string name = "New Flashcard Set", string description = "New Description")
    {
      Name = name;
      Description = description;
      FlashcardCollection = new ObservableCollection<IndividualCardModel>();
    }
    #endregion

    #region Properties
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid SetID { get; } = Guid.NewGuid();
    public ObservableCollection<IndividualCardModel> FlashcardCollection { get; private set; } 
    public double LearningProgress
    {
      get
      {
        int numLearned = FlashcardCollection.Count(x => x.IsLearned);
        return (double)numLearned/FlashcardCollection.Count * 100;
      }
    }
    public int NumTimesReviewed { get; private set; } = 0;
    public DateTime WhenCreated { get; } = DateTime.Now;
    public DateTime WhenLastReviewedUTC { get; private set; } = DateTime.MinValue;
    public bool IsArchived { get; set; } = false;

    #endregion

    #region Public Methods
    public void AddCardToSet(string cardTerm = "", string cardDefinition = "")
    {
      FlashcardCollection.Add(new IndividualCardModel(cardTerm, cardDefinition));
    }

    public void RemoveCardFromSet(Guid guidOfCardToRemove)
    {
      IndividualCardModel cardToRemove = FindCardInCollection(guidOfCardToRemove);
      FlashcardCollection.Remove(cardToRemove);
    }

    public void EditCardInSet(Guid guidofCardToEdit, string newTerm = "", string newDefinition = "")
    {
      IndividualCardModel cardToEdit = FindCardInCollection(guidofCardToEdit);

      if (!String.IsNullOrWhiteSpace(newTerm)) {
        cardToEdit.Term = newTerm;
      }
      if (!String.IsNullOrWhiteSpace(newDefinition)) {
        cardToEdit.Definition = newDefinition;
      }
    }

    public void EditCardSwitchIsLearned(Guid guidofCardToEdit)
    {
      IndividualCardModel cardToEdit = FindCardInCollection(guidofCardToEdit);
      cardToEdit.IsLearned = !cardToEdit.IsLearned;
    }

    public void EditCardSwitchIsArchived(Guid guidofCardToEdit)
    {
      IndividualCardModel cardToEdit = FindCardInCollection(guidofCardToEdit);
      cardToEdit.IsArchived = !cardToEdit.IsArchived;
    }

    public void IncrementNumTimesReviewed()
    {
      NumTimesReviewed++;
    }

    public void UpdatedLastReviewedTime()
    {
      WhenLastReviewedUTC = DateTime.UtcNow;
    }

    public override bool Equals(object obj)
    {
      return obj is CardSetModel model &&
             Name == model.Name &&
             Description == model.Description &&
             EqualityComparer<ObservableCollection<IndividualCardModel>>.Default.Equals(FlashcardCollection, model.FlashcardCollection) &&
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
      hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<IndividualCardModel>>.Default.GetHashCode(FlashcardCollection);
      hashCode = hashCode * -1521134295 + LearningProgress.GetHashCode();
      hashCode = hashCode * -1521134295 + NumTimesReviewed.GetHashCode();
      hashCode = hashCode * -1521134295 + WhenCreated.GetHashCode();
      hashCode = hashCode * -1521134295 + WhenLastReviewedUTC.GetHashCode();
      hashCode = hashCode * -1521134295 + IsArchived.GetHashCode();
      return hashCode;
    }
    #endregion

    #region Private Methods
    private IndividualCardModel FindCardInCollection(Guid guidOfCardToFind)
    {
      IndividualCardModel cardToFind = null;
      foreach (IndividualCardModel card in FlashcardCollection) {
        if (card.CardID.Equals(guidOfCardToFind)) {
          cardToFind = card;
          break;
        }
      }

      if (cardToFind == null) {
        throw new ArgumentException(string.Format("Could not find card {0} in set", guidOfCardToFind));
      }

      return cardToFind;
    }
    #endregion
  }
}
