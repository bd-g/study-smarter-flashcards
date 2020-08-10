using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudySmarterFlashcards.Sets;
using Windows.Storage;

namespace StudySmarterFlashcards.Utils
{
  public static class LocalDataHandler
  {
    public static async Task<ObservableCollection<CardSetModel>> LoadAllSetsFromLocalMemory()
    {
      StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
      try {
        StorageFile file = await folder.GetFileAsync("localCardSets.json");
        string text = await FileIO.ReadTextAsync(file);
        ObservableCollection<CardSetModel> localCardSets = JsonConvert.DeserializeObject<ObservableCollection<CardSetModel>>(text);
        return localCardSets != null ? localCardSets : new ObservableCollection<CardSetModel>();
      } catch (FileNotFoundException) {
        return new ObservableCollection<CardSetModel>();
      }
    }

    public static async Task SaveAllSetsToLocalMemory(ObservableCollection<CardSetModel> allLocalSets)
    {
      StorageFile tmpFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("localCardSets.json", CreationCollisionOption.ReplaceExisting);
      await Windows.Storage.FileIO.WriteTextAsync(tmpFile, JsonConvert.SerializeObject(allLocalSets));
    }
  }
}
