using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

    public CardSetModel(string name, string description,
                        ObservableCollection<IndividualCardModel> flashcardCollection,
                        int numTimesReviewed, DateTime whenCreated, DateTime whenLastReviewedUTC,
                        bool isStarred) : this(name, description)
    {
      SetID = Guid.Empty;
      FlashcardCollection = flashcardCollection;
      NumTimesReviewed = numTimesReviewed;
      WhenCreated = whenCreated;
      WhenLastReviewedUTC = whenLastReviewedUTC;
      IsStarred = isStarred;
    }

    [JsonConstructor]
    public CardSetModel(string name, string description, Guid setID, ObservableCollection<IndividualCardModel> flashcardCollection, int numTimesReviewed, DateTime whenCreated, DateTime whenLastReviewedUTC, bool IsStarred) : this(name, description)
    {
      SetID = setID;
      FlashcardCollection = flashcardCollection;
      NumTimesReviewed = numTimesReviewed;
      WhenCreated = whenCreated;
      WhenLastReviewedUTC = whenLastReviewedUTC;
      IsStarred = IsStarred;
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
    public bool IsStarred { get; set; } = true;

    #endregion

    #region Public Methods
    public CardSetModel Clone()
    {
      ObservableCollection<IndividualCardModel> clonedCards = new ObservableCollection<IndividualCardModel>();
      foreach(IndividualCardModel originalCard in FlashcardCollection) {
        clonedCards.Add(originalCard.Clone());
      }
      return new CardSetModel(this.Name, this.Description, clonedCards, this.NumTimesReviewed, this.WhenCreated,
                              this.WhenLastReviewedUTC, this.IsStarred);
    }

    public void AddCardToSet(string cardTerm = "New Term", string cardDefinition = "New Definition", int indexToAddAt = -1)
    {
      if (indexToAddAt >= 0 && indexToAddAt <= FlashcardCollection.Count) {
        FlashcardCollection.Insert(indexToAddAt, new IndividualCardModel(cardTerm, cardDefinition));
      } else {
        FlashcardCollection.Add(new IndividualCardModel(cardTerm, cardDefinition));
      }
    }

    public void RemoveCardFromSet(IndividualCardModel cardToRemove)
    {
      FlashcardCollection.Remove(cardToRemove);
    }

    public void RegisterNewReviewSession()
    {
      NumTimesReviewed++;
      WhenLastReviewedUTC = DateTime.UtcNow;
    }

    public override bool Equals(object obj)
    {
      return obj is CardSetModel model &&
             SetID == model.SetID;
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
      hashCode = hashCode * -1521134295 + IsStarred.GetHashCode();
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
