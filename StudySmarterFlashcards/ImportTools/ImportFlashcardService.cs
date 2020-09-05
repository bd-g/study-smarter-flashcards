using DataAccessLibrary.DataModels;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace StudySmarterFlashcards.ImportTools
{
  public static class ImportFlashcardService
  {
    #region Public Methods
    public async static Task<List<CardSetModel>> ImportNewSetsFromFile(StorageFile storageFile, CancellationToken cancellationToken)
    {
      if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".xlsx") || storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 4)).Equals(".xls")) {
        ExcelEngine excelEngine = new ExcelEngine();
        IWorkbook workbook = await excelEngine.Excel.Workbooks.OpenAsync(await storageFile.OpenStreamForReadAsync());
        if (cancellationToken.IsCancellationRequested) {
          return null;
        }

        VerifyExcelFileIsParseable(workbook);
        List<CardSetModel> newCardSetModels = new List<CardSetModel>();
        foreach (IWorksheet worksheet in workbook.Worksheets) {
          CardSetModel newCardSetModel = ExcelWorksheetToCardSet(worksheet, cancellationToken);
          newCardSetModels.Add(newCardSetModel);
        }

        return newCardSetModels;
      } else if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".docx") || storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 4)).Equals(".doc")) {
        WordDocument wordDocument = new WordDocument(await storageFile.OpenStreamForReadAsync());

        VerifyWordFileIsParseable(wordDocument);
        List<CardSetModel> newCardSetModels = WordDocumentToCardSetModels(wordDocument).ToList();
        return newCardSetModels;
      }
      return null;
    }
    public async static Task<CardSetModel> ImportAddToExistingSet(StorageFile storageFile, CancellationToken cancellationToken, CardSetModel existingSet)
    {
      if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".xlsx") || storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 4)).Equals(".xls")) {
        ExcelEngine excelEngine = new ExcelEngine();
        IWorkbook workbook = await excelEngine.Excel.Workbooks.OpenAsync(await storageFile.OpenStreamForReadAsync());
        if (cancellationToken.IsCancellationRequested) {
          return existingSet;
        }

        VerifyExcelFileIsParseable(workbook, canContainMultipleWorksheets:false);
        existingSet = ExcelWorksheetToCardSet(workbook.Worksheets[0], cancellationToken, existingSet);        

        return existingSet;
      } else if (storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 5)).Equals(".docx") || storageFile.Name.Substring(Math.Max(0, storageFile.Name.Length - 4)).Equals(".doc")) {
        WordDocument wordDocument = new WordDocument(await storageFile.OpenStreamForReadAsync());

        VerifyWordFileIsParseable(wordDocument);
        return WordDocumentToCardSetModels(wordDocument, existingSet).ToList()[0];
      }
      return null;
    }
    #endregion

    #region Private Methods
    private static void VerifyExcelFileIsParseable(IWorkbook workbook, bool canContainMultipleWorksheets = true)
    {
      if (workbook.Worksheets.Count < 1) {
        throw new NotSupportedException("Excel file needs to contain at least one worksheet.");
      } else if (workbook.Worksheets.Count > 1 && !canContainMultipleWorksheets) {
        throw new NotSupportedException("Excel workbook contains multiple worksheets (tabs at the bottom). There can only be one worksheet in a workbook when importing an addition to an existing set.");
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
      foreach (WSection wSection in wordDocument.Sections) {
        int baseListDepth = 0;
        if (wSection.Paragraphs.Count > 0) {
          baseListDepth = wSection.Paragraphs[0].ListFormat.ListLevelNumber;
        }
        foreach (IWParagraph paragraph in wSection.Paragraphs) {
          if (paragraph.ListFormat.ListLevelNumber < baseListDepth && !string.IsNullOrWhiteSpace(paragraph.Text)) {
            throw new NotSupportedException("Word file is in invalid format. Make sure your topmost line (set name) is all the way to the left and the terms and definitions underneath it are indented properly. See settings page for more details on formatting word documents for import.");
          }
        }
      }
    }

    private static CardSetModel ExcelWorksheetToCardSet(IWorksheet worksheet, CancellationToken cancellationToken, CardSetModel cardSetToAddTo = null)
    {
      CardSetModel cardSetModel = cardSetToAddTo != null ? cardSetToAddTo : new CardSetModel();
      worksheet.UsedRangeIncludesFormatting = false;
      IRange usedRange = worksheet.UsedRange;
      int lastRow = usedRange.LastRow;
      bool thirdColumnIsStarred = worksheet.Range["C1"].Text.Contains("Star", StringComparison.CurrentCultureIgnoreCase);
      int indexOfFirstUnstarredCard = 0;

      for (int i = 2; i < lastRow + 1; i++) {
        if (cancellationToken.IsCancellationRequested) {
          return null;
        }
        string cardTerm = usedRange[i, 1].Value.Trim();
        string cardDefinition = usedRange[i, 2].Value.Trim();
        bool overrideStarredValue = bool.TryParse(usedRange[i, (thirdColumnIsStarred ? 3 : 4)].Value, out bool cardIsStarred);
        bool overrideLearnedValue = bool.TryParse(usedRange[i, (thirdColumnIsStarred ? 4 : 3)].Value, out bool cardIsLearned);

        if (overrideStarredValue && !cardIsStarred) {
          cardSetModel.AddCardToSet(cardTerm, cardDefinition, isLearned: overrideLearnedValue ? (bool?)cardIsLearned : null, isStarred: overrideStarredValue ? (bool?)cardIsStarred : null);
        } else {
          cardSetModel.AddCardToSet(cardTerm, cardDefinition, isLearned: overrideLearnedValue ? (bool?)cardIsLearned : null, isStarred: overrideStarredValue ? (bool?)cardIsStarred : null, indexOfFirstUnstarredCard);
          indexOfFirstUnstarredCard++;
        }
      }
      return cardSetModel;
    }

    private static IEnumerable<CardSetModel> WordDocumentToCardSetModels(WordDocument wordDocument, CardSetModel existingCardSet = null)
    {
      bool onlyCreateOneSet = existingCardSet != null;
      CardSetModel cardSetModel = existingCardSet != null ? existingCardSet :  null;
      foreach (WSection wSection in wordDocument.Sections) {
        int baseListDepth = 0;
        string tmpTerm = "";
        string tmpDefinition = "";
        if (wSection.Paragraphs.Count > 0) {
          baseListDepth = wSection.Paragraphs[0].ListFormat.ListLevelNumber;
        }
        if (onlyCreateOneSet) {
          baseListDepth--;
        }
        for (int i = 0; i < wSection.Paragraphs.Count; i++) {
          IWParagraph paragraph = wSection.Paragraphs[i];
          if (string.IsNullOrWhiteSpace(paragraph.Text)) {
            continue;
          }
          if (paragraph.ListFormat.ListLevelNumber == baseListDepth) {
            if (cardSetModel != null) {
              yield return cardSetModel;
            }
            cardSetModel = new CardSetModel(name: paragraph.Text);
          } else {
            int peekNextListDepth = (paragraph.NextSibling is IWParagraph) ? (paragraph.NextSibling as IWParagraph).ListFormat.ListLevelNumber : -1;
            if (paragraph.ListFormat.ListLevelNumber == baseListDepth + 1) {
              tmpTerm = paragraph.Text;
              if (peekNextListDepth < baseListDepth + 2) {
                int indexOfHyphen = Math.Max(tmpTerm.IndexOf('-'), tmpTerm.IndexOf((char)8211));
                if (indexOfHyphen > -1) {
                  tmpDefinition = (indexOfHyphen > -1 ? tmpTerm.Substring(indexOfHyphen) : "").Substring(1);
                  tmpTerm = tmpTerm.Substring(0, (indexOfHyphen > -1 ? indexOfHyphen - 1 : tmpTerm.Length));
                }
                cardSetModel.AddCardToSet(tmpTerm.Trim(), tmpDefinition.Trim());
                tmpTerm = "";
                tmpDefinition = "";
              }
            } else if (paragraph.ListFormat.ListLevelNumber > baseListDepth + 1) {
              tmpDefinition = tmpDefinition + (string.IsNullOrWhiteSpace(tmpDefinition) ? "" : "\n") + paragraph.Text;
              if (peekNextListDepth < baseListDepth + 2) {
                cardSetModel.AddCardToSet(tmpTerm.Trim(), tmpDefinition.Trim());
                tmpTerm = "";
                tmpDefinition = "";
              }
            }
          }
        }
      }
      if (cardSetModel != null) {
        yield return cardSetModel;
      }
    }
    #endregion
  }
}
