using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using StudySmarterFlashcards.Sets;
using Syncfusion.XlsIO;
using Windows.Storage;

namespace StudySmarterFlashcards.ImportTools
{
  public static class ImportFlashcardService
  {
    #region Public Methods
    public async static Task<CardSetModel> ImportFromFile(StorageFile storageFile)
    {
      if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".xlsx")){
        ExcelEngine excelEngine = new ExcelEngine();
        IWorkbook workbook = await excelEngine.Excel.Workbooks.OpenAsync(await storageFile.OpenStreamForReadAsync());

        VerifyExcelFileIsParseable(workbook);

        CardSetModel newCardSetModel = new CardSetModel();
        IWorksheet worksheet = workbook.Worksheets[0];
        worksheet.UsedRangeIncludesFormatting = false;
        IRange usedRange = worksheet.UsedRange;
        int lastRow = usedRange.LastRow;

        for (int i = 2; i < lastRow + 1; i++) {
          string cardTerm = usedRange[i, 1].Text;
          string cardDefinition = usedRange[i, 2].Text;
          bool overrideStarredValue = bool.TryParse(usedRange[i, 3].Text, out bool cardIsStarred);
          bool overrideLearnedValue = bool.TryParse(usedRange[i, 4].Text, out bool cardIsLearned);

          newCardSetModel.AddCardToSet(cardTerm, cardDefinition, isLearned: overrideLearnedValue ? (bool?)cardIsLearned : null, isStarred:overrideStarredValue ? (bool?)cardIsStarred : null);
        }

        return newCardSetModel;
      }


      return null;
    }
    #endregion

    #region Private Methods
    private static void VerifyExcelFileIsParseable(IWorkbook workbook)
    {
      if(workbook.Worksheets.Count > 0) {
        IWorksheet worksheet = workbook.Worksheets[0];
        if (!worksheet.Range["A1"].Text.Contains("Term", StringComparison.CurrentCultureIgnoreCase) || !worksheet.Range["B1"].Text.Contains("Definition", StringComparison.CurrentCultureIgnoreCase)) {
          throw new NotSupportedException(string.Format("Excel file is not in a supported format. The worksheet {0} doesn't contain the proper headers. Please review formatting rules for importing excel files.", worksheet.Name));
        }
      } else {
        throw new NotSupportedException("Excel file needs to contain at least one worksheet.");
      }
    }
    #endregion


  }
}
