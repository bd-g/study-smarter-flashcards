using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System;
using System.Collections.ObjectModel;
using DataAccessLibrary.DataModels;

namespace DataAccessLibrary
{
  public static class DataAccess
  {
    private static readonly string prDBName = "localCardSets.db";
    public async static Task InitializeDatabase_UWP()
    {
      await ApplicationData.Current.LocalFolder.CreateFileAsync(prDBName, CreationCollisionOption.OpenIfExists);
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        string createFlashCardSetTableCommand =
          "CREATE TABLE IF NOT EXISTS FlashCardSet " +
          "(" +
            "RowID INT PRIMARY KEY, " +
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
            "RowID INT PRIMARY KEY, " +
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
    public static ObservableCollection<CardSetModel> ReadAllExistingData_UWP()
    {
      ObservableCollection<CardSetModel> allCardSets = new ObservableCollection<CardSetModel>();

      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();

        string selectAllSets =
          "SELECT" +
            "Name, " +
            "Description, " +
            "SetID, " +
            "NumTimesReviewed, " +
            "WhenCreated, " +
            "WhenLastReviewedUTC, " +
            "IsStarred" +
          "FROM" +
            "FlashCardSet";
        SqliteCommand selectAllSetsCommand = new SqliteCommand(selectAllSets, db);

        SqliteDataReader selectAllSetsReader = selectAllSetsCommand.ExecuteReader();
        while (selectAllSetsReader.Read()) {
          string name =                  selectAllSetsReader.GetString(0);
          string description =           selectAllSetsReader.GetString(1);
          Guid setID =                   selectAllSetsReader.GetGuid(2);
          int numTimesReviewed =         selectAllSetsReader.GetInt32(3);
          DateTime whenCreated =         selectAllSetsReader.GetDateTime(4);
          DateTime whenLastReviewedUTC = selectAllSetsReader.GetDateTime(5);
          bool isStarred =               selectAllSetsReader.GetBoolean(6);

          ObservableCollection<IndividualCardModel> flashcardCollection = new ObservableCollection<IndividualCardModel>();
          string selectAllAssociatedCards =
            "SELECT" +
              "Term, " +
              "Definition, " +
              "CardID, " +
              "IsLearned, " +
              "IsStarred, " +
            "FROM" +
              "IndividualFlashcard" +
            "WHERE" +
              "ParentSetID = @ParentSetID";
          SqliteCommand selectAllAssociatedCardsCommand = new SqliteCommand(selectAllAssociatedCards, db);
          selectAllAssociatedCardsCommand.Parameters.AddWithValue("@ParentSetID", setID);

          SqliteDataReader allAssociatedCardsReader = selectAllAssociatedCardsCommand.ExecuteReader();          
          while (allAssociatedCardsReader.Read()) {
            string term =           allAssociatedCardsReader.GetString(0);
            string definition =     allAssociatedCardsReader.GetString(1);
            Guid cardID =           allAssociatedCardsReader.GetGuid(2);
            bool isLearned =        allAssociatedCardsReader.GetBoolean(3);
            bool isCardStarred =    allAssociatedCardsReader.GetBoolean(4);
            flashcardCollection.Add(new IndividualCardModel(term, definition, cardID, isLearned, isStarred));
          }

          allCardSets.Add(new CardSetModel(name, description, flashcardCollection, numTimesReviewed, whenCreated, whenLastReviewedUTC, isStarred));
        }

        db.Close();
      }

      return allCardSets;
    }
    public static void AddNewCardSet_UWP(CardSetModel newCardSet)
    {
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
      {
        db.Open();
        SqliteCommand insertSetCommand = new SqliteCommand();
        insertSetCommand.Connection = db;

        insertSetCommand.CommandText =
          "INSERT INTO FlashCardSet VALUES " +
          "(" +
            "@Name, " +
            "@Description, " +
            "@SetID, " +
            "@NumTimesReviewed, " +
            "@WhenCreated, " +
            "@WhenLastReviewedUTC, " +
            "@IsStarred" +
          ");";
        insertSetCommand.Parameters.AddWithValue("@Name",                newCardSet.Name);
        insertSetCommand.Parameters.AddWithValue("@Description",         newCardSet.Description);
        insertSetCommand.Parameters.AddWithValue("@SetID",               newCardSet.SetID);
        insertSetCommand.Parameters.AddWithValue("@NumTimesReviewed",    newCardSet.NumTimesReviewed);
        insertSetCommand.Parameters.AddWithValue("@WhenCreated",         newCardSet.WhenCreated);
        insertSetCommand.Parameters.AddWithValue("@WhenLastReviewedUTC", newCardSet.WhenLastReviewedUTC);
        insertSetCommand.Parameters.AddWithValue("@IsStarred",           newCardSet.IsStarred);
        insertSetCommand.ExecuteReader();

        foreach(IndividualCardModel individualCard in newCardSet.FlashcardCollection) {
          AddNewCard_UWP(individualCard, newCardSet.SetID, db);
        }

        db.Close();
      }

    }
    public static void AddNewCard_UWP(IndividualCardModel newCard, Guid parentSetID, SqliteConnection db)
    {
      SqliteCommand insertCardCommand = new SqliteCommand();
      insertCardCommand.Connection = db;

      insertCardCommand.CommandText =
        "INSERT INTO IndividualFlashcard VALUES " +
        "(" +
          "@Term, " +
          "@Definition, " +
          "@CardID, " +
          "@ParentSetID, " +
          "@IsLearned, " +
          "@IsStarred, " +
        ");";

      insertCardCommand.Parameters.AddWithValue("@Term",        newCard.Term);
      insertCardCommand.Parameters.AddWithValue("@Definition",  newCard.Definition);
      insertCardCommand.Parameters.AddWithValue("@CardID",      newCard.CardID);
      insertCardCommand.Parameters.AddWithValue("@ParentSetID", parentSetID);
      insertCardCommand.Parameters.AddWithValue("@IsLearned",   newCard.IsLearned);
      insertCardCommand.Parameters.AddWithValue("@IsStarred",   newCard.IsStarred);
      insertCardCommand.ExecuteReader();
    }
  }
}
