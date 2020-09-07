using DataAccessLibrary.DataModels;
using DataAccessLibrary.Utils;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DataAccessLibrary
{
  public static class DataAccess
  {
    #region Fields
    private static readonly string prDBName = "localCardSets.db";
    #endregion

    #region Database Initialization
    public async static Task InitializeDatabase_UWP()
    {
      await ApplicationData.Current.LocalFolder.CreateFileAsync(prDBName, CreationCollisionOption.OpenIfExists);
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        string createFlashCardSetTableCommand =
          "CREATE TABLE IF NOT EXISTS FlashCardSet " +
          "(" +
            "RowID INTEGER PRIMARY KEY, " +
            "Name NVARCHAR(30) NOT NULL, " +
            "Description NVARCHAR(150) NOT NULL, " +
            "SetID UNIQUEIDENTIFIER NOT NULL UNIQUE, " +
            "NumTimesReviewed INT NOT NULL, " +
            "WhenCreated DATETIME NOT NULL, " +
            "WhenLastReviewedUTC DATETIME NOT NULL, " +
            "IsStarred BOOLEAN NOT NULL" +
          ")";
        string createIndividualFlashcardTableCommand =
          "CREATE TABLE IF NOT EXISTS IndividualFlashcard " +
          "(" +
            "RowID INTEGER PRIMARY KEY, " +
            "Term NVARCHAR(200) NOT NULL, " +
            "Definition NVARCHAR(2000) NOT NULL, " +
            "CardID UNIQUEIDENTIFIER NOT NULL UNIQUE, " +
            "ParentSetID UNIQUEIDENTIFIER NOT NULL, " +
            "IsLearned BOOLEAN NOT NULL, " +
            "IsStarred BOOLEAN NOT NULL, " +
            "FOREIGN KEY(ParentSetID) REFERENCES FlashCardSet(SetID)" +
          ")";

        SqliteCommand createSetTable = new SqliteCommand(createFlashCardSetTableCommand, db);
        createSetTable.ExecuteReader();
        SqliteCommand createCardTable = new SqliteCommand(createIndividualFlashcardTableCommand, db);
        createCardTable.ExecuteReader();
        db.Close();
      }
    }
    #endregion

    #region Select Methods
    public static ObservableCollection<CardSetModel> ReadAllExistingData_UWP()
    {
      ObservableCollection<CardSetModel> allCardSets = new ObservableCollection<CardSetModel>();

      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();

        string selectAllSets =
          "SELECT " +
            "Name, " +
            "Description, " +
            "SetID, " +
            "NumTimesReviewed, " +
            "WhenCreated, " +
            "WhenLastReviewedUTC, " +
            "IsStarred " +
          "FROM " +
            "FlashCardSet " +
          "ORDER BY " +
            "(CASE WHEN IsStarred THEN 1 ELSE 2 END) ASC, " +
            "WhenLastReviewedUTC DESC";
        SqliteCommand selectAllSetsCommand = new SqliteCommand(selectAllSets, db);

        SqliteDataReader selectAllSetsReader = selectAllSetsCommand.ExecuteReader();
        while (selectAllSetsReader.Read()) {
          string name = selectAllSetsReader.GetString(0);
          string description = selectAllSetsReader.GetString(1);
          Guid setID = selectAllSetsReader.GetGuid(2);
          int numTimesReviewed = selectAllSetsReader.GetInt32(3);
          DateTime whenCreated = selectAllSetsReader.GetDateTime(4);
          DateTime whenLastReviewedUTC = selectAllSetsReader.GetDateTime(5);
          bool isStarred = selectAllSetsReader.GetBoolean(6);

          ObservableCollection<IndividualCardModel> flashcardCollection = new ObservableCollection<IndividualCardModel>();
          string selectAllAssociatedCards =
            "SELECT " +
              "Term, " +
              "Definition, " +
              "CardID, " +
              "IsLearned, " +
              "IsStarred " +
            "FROM " +
              "IndividualFlashcard " +
            "WHERE " +
              "ParentSetID = @ParentSetID " +
            "ORDER BY " +
              "(CASE WHEN IsStarred THEN 1 ELSE 2 END) ASC, " +
              "RowID ASC";
          SqliteCommand selectAllAssociatedCardsCommand = new SqliteCommand(selectAllAssociatedCards, db);
          selectAllAssociatedCardsCommand.Parameters.AddWithValue("@ParentSetID", setID);

          SqliteDataReader allAssociatedCardsReader = selectAllAssociatedCardsCommand.ExecuteReader();
          while (allAssociatedCardsReader.Read()) {
            string term = allAssociatedCardsReader.GetString(0);
            string definition = allAssociatedCardsReader.GetString(1);
            Guid cardID = allAssociatedCardsReader.GetGuid(2);
            bool isLearned = allAssociatedCardsReader.GetBoolean(3);
            bool isCardStarred = allAssociatedCardsReader.GetBoolean(4);
            flashcardCollection.Add(new IndividualCardModel(term, definition, cardID, isLearned, isCardStarred));
          }

          allCardSets.Add(new CardSetModel(name, description, setID, flashcardCollection, numTimesReviewed, whenCreated, whenLastReviewedUTC, isStarred));
        }

        db.Close();
      }

      return allCardSets;
    }
    #endregion

    #region Insert Methods
    public static void AddNewCardSet_UWP(CardSetModel newCardSet)
    {
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        SqliteCommand insertSetCommand = new SqliteCommand();
        insertSetCommand.Connection = db;

        insertSetCommand.CommandText =
          "INSERT INTO FlashCardSet VALUES " +
          "(" +
            "NULL," +
            "@Name, " +
            "@Description, " +
            "@SetID, " +
            "@NumTimesReviewed, " +
            "@WhenCreated, " +
            "@WhenLastReviewedUTC, " +
            "@IsStarred" +
          ");";
        insertSetCommand.Parameters.AddWithValue("@Name", newCardSet.Name);
        insertSetCommand.Parameters.AddWithValue("@Description", newCardSet.Description);
        insertSetCommand.Parameters.AddWithValue("@SetID", newCardSet.SetID);
        insertSetCommand.Parameters.AddWithValue("@NumTimesReviewed", newCardSet.NumTimesReviewed);
        insertSetCommand.Parameters.AddWithValue("@WhenCreated", newCardSet.WhenCreated);
        insertSetCommand.Parameters.AddWithValue("@WhenLastReviewedUTC", newCardSet.WhenLastReviewedUTC);
        insertSetCommand.Parameters.AddWithValue("@IsStarred", newCardSet.IsStarred);
        insertSetCommand.ExecuteReader();

        foreach (IndividualCardModel individualCard in newCardSet.FlashcardCollection) {
          AddNewCard(individualCard, newCardSet.SetID, db);
        }

        db.Close();
      }

    }
    private static void AddNewCard(IndividualCardModel newCard, Guid parentSetID, SqliteConnection db)
    {
      SqliteCommand insertCardCommand = new SqliteCommand();
      insertCardCommand.Connection = db;

      insertCardCommand.CommandText =
        "INSERT INTO IndividualFlashcard VALUES " +
        "(" +
          "NULL," +
          "@Term, " +
          "@Definition, " +
          "@CardID, " +
          "@ParentSetID, " +
          "@IsLearned, " +
          "@IsStarred" +
        ");";

      insertCardCommand.Parameters.AddWithValue("@Term", newCard.Term);
      insertCardCommand.Parameters.AddWithValue("@Definition", newCard.Definition);
      insertCardCommand.Parameters.AddWithValue("@CardID", newCard.CardID);
      insertCardCommand.Parameters.AddWithValue("@ParentSetID", parentSetID);
      insertCardCommand.Parameters.AddWithValue("@IsLearned", newCard.IsLearned);
      insertCardCommand.Parameters.AddWithValue("@IsStarred", newCard.IsStarred);
      insertCardCommand.ExecuteReader();
    }
    #endregion

    #region Update Methods
    public static void EditCardSet_UWP(CardSetModel editedCardSet)
    {
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        SqliteCommand updateSetCommand = new SqliteCommand();
        updateSetCommand.Connection = db;

        updateSetCommand.CommandText =
          "UPDATE FlashCardSet " +
          "SET " +
            "Name = @Name, " +
            "Description = @Description, " +
            "NumTimesReviewed = @NumTimesReviewed, " +
            "WhenCreated = @WhenCreated, " +
            "WhenLastReviewedUTC = @WhenLastReviewedUTC, " +
            "IsStarred = @IsStarred " +
          "WHERE " +
            "SetID = @SetID;";
          
        updateSetCommand.Parameters.AddWithValue("@Name", editedCardSet.Name);
        updateSetCommand.Parameters.AddWithValue("@Description", editedCardSet.Description);
        updateSetCommand.Parameters.AddWithValue("@NumTimesReviewed", editedCardSet.NumTimesReviewed);
        updateSetCommand.Parameters.AddWithValue("@WhenCreated", editedCardSet.WhenCreated);
        updateSetCommand.Parameters.AddWithValue("@WhenLastReviewedUTC", editedCardSet.WhenLastReviewedUTC);
        updateSetCommand.Parameters.AddWithValue("@IsStarred", editedCardSet.IsStarred);

        updateSetCommand.Parameters.AddWithValue("@SetID", editedCardSet.SetID);
        updateSetCommand.ExecuteReader();

        List<Guid> allCardIDs = new List<Guid>();
        foreach (IndividualCardModel individualCard in editedCardSet.FlashcardCollection) {
          allCardIDs.Add(individualCard.CardID);

          SqliteCommand queryCardCommand = new SqliteCommand();
          queryCardCommand.Connection = db;
          queryCardCommand.CommandText = "SELECT COUNT(*) FROM IndividualFlashcard WHERE CardID = @CardID;";
          queryCardCommand.Parameters.AddWithValue("@CardID", individualCard.CardID);
          int count = Convert.ToInt32(queryCardCommand.ExecuteScalar());
          if (count == 0) {
            AddNewCard(individualCard, editedCardSet.SetID, db);
          } else {
            EditFlashcard(individualCard, db);
          }
        }

        SqliteCommand deleteOldCardsCommand = new SqliteCommand();
        deleteOldCardsCommand.Connection = db;
        deleteOldCardsCommand.CommandText =
          "DELETE FROM " +
            "IndividualFlashcard " +
          "WHERE " +
            "ParentSetID = @ParentSetID AND " +
            "CardID NOT IN (";
        for (int i = 0; i < allCardIDs.Count; i++) {
          deleteOldCardsCommand.CommandText += "@CardID" + i + ",";
        }
        deleteOldCardsCommand.CommandText = deleteOldCardsCommand.CommandText.TrimEnd(',');
        deleteOldCardsCommand.CommandText += ");";

        deleteOldCardsCommand.Parameters.AddWithValue("@ParentSetID", editedCardSet.SetID);
        for(int i = 0; i < allCardIDs.Count; i++) {
          deleteOldCardsCommand.Parameters.AddWithValue("@CardID" + i, allCardIDs[i]).SqliteType = SqliteType.Text;
        }

        deleteOldCardsCommand.ExecuteReader();

        db.Close();
      }

    }
    public static void ArchiveCardSet_UWP(Guid setID, bool newArchiveStatus)
    {
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        SqliteCommand archiveSetCommand = new SqliteCommand();
        archiveSetCommand.Connection = db;

        archiveSetCommand.CommandText =
          "UPDATE FlashCardSet " +
          "SET " +
            "IsStarred = @IsStarred " +
          "WHERE " +
            "SetID = @SetID;";

        archiveSetCommand.Parameters.AddWithValue("@IsStarred", newArchiveStatus);
        archiveSetCommand.Parameters.AddWithValue("@SetID", setID);
        archiveSetCommand.ExecuteReader();

        db.Close();
      }

    }
    public static void DeleteCardSet_UWP(CardSetModel deletedCardSet)
    {
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        SqliteCommand deleteAssociatedCardsCommand = new SqliteCommand();
        deleteAssociatedCardsCommand.Connection = db;

        deleteAssociatedCardsCommand.CommandText =
          "DELETE FROM " +
            "IndividualFlashcard " +
          "WHERE " +
            "ParentSetID = @SetID;";

        deleteAssociatedCardsCommand.Parameters.AddWithValue("@SetID", deletedCardSet.SetID);
        deleteAssociatedCardsCommand.ExecuteReader();

        SqliteCommand deleteSetCommand = new SqliteCommand();
        deleteSetCommand.Connection = db;

        deleteSetCommand.CommandText =
          "DELETE FROM " +
            "FlashCardSet " +
          "WHERE " +
            "SetID = @SetID;";

        deleteSetCommand.Parameters.AddWithValue("@SetID", deletedCardSet.SetID);
        deleteSetCommand.ExecuteReader();

        db.Close();
      }

    }
    public static void EditFlashcardIsLearned_UWP(Guid cardID, bool isLearned)
    {
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        SqliteCommand editCardCommand = new SqliteCommand();
        editCardCommand.Connection = db;

        editCardCommand.CommandText =
          "UPDATE IndividualFlashcard " +
          "SET " +
            "IsLearned = @IsLearned " +
          "WHERE " +
            "CardID = @CardID;";

        editCardCommand.Parameters.AddWithValue("@CardID", cardID);
        editCardCommand.Parameters.AddWithValue("@IsLearned", isLearned);
        editCardCommand.ExecuteReader();

        db.Close();
      }
    }
    public static void EditCardSetRegisterNewReviewSession_UWP(CardSetModel reviewedCardSet)
    {
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        SqliteCommand editCardCommand = new SqliteCommand();
        editCardCommand.Connection = db;

        editCardCommand.CommandText =
          "UPDATE FlashCardSet " +
          "SET " +
            "NumTimesReviewed = @NumTimesReviewed, " +
            "WhenLastReviewedUTC = @WhenLastReviewedUTC " +
          "WHERE " +
            "SetID = @SetID;";

        editCardCommand.Parameters.AddWithValue("@NumTimesReviewed", reviewedCardSet.NumTimesReviewed);
        editCardCommand.Parameters.AddWithValue("@WhenLastReviewedUTC", reviewedCardSet.WhenLastReviewedUTC);
        editCardCommand.Parameters.AddWithValue("@SetID", reviewedCardSet.SetID);
        editCardCommand.ExecuteReader();

        db.Close();
      }
    }
    private static void EditFlashcard(IndividualCardModel editedCard, SqliteConnection db)
    {
      SqliteCommand editCardCommand = new SqliteCommand();
      editCardCommand.Connection = db;

      editCardCommand.CommandText =
        "UPDATE IndividualFlashcard " +
        "SET " +
          "Term = @Term, " +
          "Definition = @Definition, " +
          "IsLearned = @IsLearned, " +
          "IsStarred = @IsStarred " +
        "WHERE " +
          "CardID = @CardID;";

      editCardCommand.Parameters.AddWithValue("@Term", editedCard.Term);
      editCardCommand.Parameters.AddWithValue("@Definition", editedCard.Definition);
      editCardCommand.Parameters.AddWithValue("@CardID", editedCard.CardID);
      editCardCommand.Parameters.AddWithValue("@IsLearned", editedCard.IsLearned);
      editCardCommand.Parameters.AddWithValue("@IsStarred", editedCard.IsStarred);
      editCardCommand.ExecuteReader();
    }
    #endregion
  }
}
