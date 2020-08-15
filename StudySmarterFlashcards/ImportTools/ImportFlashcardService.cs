using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using StudySmarterFlashcards.Sets;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using Windows.Storage;

namespace StudySmarterFlashcards.ImportTools
{
  public static class ImportFlashcardService
  {
    #region Public Methods
    public async static Task<List<CardSetModel>> ImportFromFile(StorageFile storageFile, CancellationToken cancellationToken)
    {
      if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".xlsx") || storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 4)).Equals(".xls")) {
        ExcelEngine excelEngine = new ExcelEngine();
        IWorkbook workbook = await excelEngine.Excel.Workbooks.OpenAsync(await storageFile.OpenStreamForReadAsync());
        if (cancellationToken.IsCancellationRequested) {
          return null;
        }

        VerifyExcelFileIsParseable(workbook);
        List<CardSetModel> newCardSetModels = new List<CardSetModel>();
        foreach(IWorksheet worksheet in workbook.Worksheets) {
          CardSetModel newCardSetModel = new CardSetModel();
          worksheet.UsedRangeIncludesFormatting = false;
          IRange usedRange = worksheet.UsedRange;
          int lastRow = usedRange.LastRow;
          bool thirdColumnIsStarred = worksheet.Range["C1"].Text.Contains("Star", StringComparison.CurrentCultureIgnoreCase);
          int indexOfFirstUnstarredCard = 0;

          for (int i = 2; i < lastRow + 1; i++) {
            if (cancellationToken.IsCancellationRequested) {
              return null;
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
          newCardSetModels.Add(newCardSetModel);
        }

        return newCardSetModels;
      } else if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".docx") || storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 4)).Equals(".doc")) {
        WordDocument wordDocument = new WordDocument(await storageFile.OpenStreamForReadAsync());

        VerifyWordFileIsParseable(wordDocument);
        CardSetModel newCardSetModel = new CardSetModel();
        foreach (WSection wSection in wordDocument.Sections) {
          foreach (IWParagraph paragraph in wSection.Paragraphs) {

          }
          foreach (IWTable table in wSection.Tables) {

          }

        }
      }
      return null;
    }
    #endregion

    #region Private Methods
    private static void VerifyExcelFileIsParseable(IWorkbook workbook)
    {
      if (workbook.Worksheets.Count < 1) {
        throw new NotSupportedException("Excel file needs to contain at least one worksheet.");
      }
      foreach (IWorksheet worksheet in workbook.Worksheets) {
        if (!worksheet.Range["A1"].Value.Contains("Term", StringComparison.CurrentCultureIgnoreCase) || !worksheet.Range["B1"].Value.Contains("Definition", StringComparison.CurrentCultureIgnoreCase)) {
          throw new NotSupportedException(string.Format("Excel file is not in a supported format. The worksheet \"{0}\" doesn't contain the proper headers.\n\nMake sure the first column's topmost cell is labeled \"Terms\", the second column's topmost cell is labeled \"Definitions\", and the third and fourth columns (if included) are labeled \"Starred\" and \"Learned\" and values are only \"true\" or \"false\".\n\nSee settings page for more details, including pictures of valid formats.", worksheet.Name));
        }
      }
    }

    private static void VerifyWordFileIsParseable(WordDocument wordDocument)
    {
      if (wordDocument.Sections.Count < 1) {
        throw new NotSupportedException("Word file needs to have content to import");
      }
    }
    #endregion


  }
}
