using System;
using System.IO;
using System.Threading;
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
    public async static Task<CardSetModel> ImportFromFile(StorageFile storageFile, CancellationToken cancellationToken)
    {
      if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".xlsx")){
        ExcelEngine excelEngine = new ExcelEngine();
        IWorkbook workbook = await excelEngine.Excel.Workbooks.OpenAsync(await storageFile.OpenStreamForReadAsync());
        if (cancellationToken.IsCancellationRequested) {
          return null;
        }

        VerifyExcelFileIsParseable(workbook);

        CardSetModel newCardSetModel = new CardSetModel();
        IWorksheet worksheet = workbook.Worksheets[0];
        worksheet.UsedRangeIncludesFormatting = false;
        IRange usedRange = worksheet.UsedRange;
        int lastRow = usedRange.LastRow;
        bool thirdColumnIsStarred = worksheet.Range["C1"].Text.Contains("Star", StringComparison.CurrentCultureIgnoreCase);
        int indexOfFirstUnstarredCard = 0;

        for (int i = 2; i < lastRow + 1; i++) {
          if (cancellationToken.IsCancellationRequested) {
            return newCardSetModel;
          }
          string cardTerm = usedRange[i, 1].Value;
          string cardDefinition = usedRange[i, 2].Value;
          bool overrideStarredValue = bool.TryParse(usedRange[i, (thirdColumnIsStarred ? 3 : 4)].Value, out bool cardIsStarred);
          bool overrideLearnedValue = bool.TryParse(usedRange[i, (thirdColumnIsStarred ? 4 : 3)].Value, out bool cardIsLearned);

          if (overrideStarredValue && !cardIsStarred) {
            newCardSetModel.AddCardToSet(cardTerm, cardDefinition, isLearned: overrideLearnedValue ? (bool?)cardIsLearned : null, isStarred: overrideStarredValue ? (bool?)cardIsStarred : null);
          } else {
            newCardSetModel.AddCardToSet(cardTerm, cardDefinition, isLearned: overrideLearnedValue ? (bool?)cardIsLearned : null, isStarred: overrideStarredValue ? (bool?)cardIsStarred : null, indexOfFirstUnstarredCard);
            indexOfFirstUnstarredCard++;
          }

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
        if (!worksheet.Range["A1"].Value.Contains("Term", StringComparison.CurrentCultureIgnoreCase) || !worksheet.Range["B1"].Value.Contains("Definition", StringComparison.CurrentCultureIgnoreCase)) {
          throw new NotSupportedException(string.Format("Excel file is not in a supported format. The worksheet {0} doesn't contain the proper headers. Please review formatting rules for importing excel files.", worksheet.Name));
        }
      } else {
        throw new NotSupportedException("Excel file needs to contain at least one worksheet.");
      }
    }
    #endregion


  }
}
