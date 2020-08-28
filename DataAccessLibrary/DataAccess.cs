using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System;

namespace DataAccessLibrary
{
  public static class DataAccess
  {
    private static readonly string prDBName = "localCardSets.db";
    public async static Task InitializeDatabase()
    {
      await ApplicationData.Current.LocalFolder.CreateFileAsync(prDBName, CreationCollisionOption.OpenIfExists);
      string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, prDBName);
      using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}")) {
        db.Open();
        String tableCommand =
          "CREATE TABLE IF NOT EXISTS FlashCardSets " +
          "(" +
            "Primary_Key INTEGER PRIMARY KEY, " +
            "Text_Entry NVARCHAR(2048) NULL" +
          ")";
        SqliteCommand createTable = new SqliteCommand(tableCommand, db);
        createTable.ExecuteReader();
      }
    }
  }
}
