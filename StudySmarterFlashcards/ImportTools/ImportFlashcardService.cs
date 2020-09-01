using DataAccessLibrary.DataModels;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        foreach (IWorksheet worksheet in workbook.Worksheets) {
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
            string cardTerm = usedRange[i, 1].Value.Trim();
            string cardDefinition = usedRange[i, 2].Value.Trim();
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
        List<CardSetModel> newCardSetModels = new List<CardSetModel>();
        foreach (WSection wSection in wordDocument.Sections) {
          int baseListDepth = 0;
          string tmpTerm = "";
          string tmpDefinition = "";
          if (wSection.Paragraphs.Count > 0) {
            baseListDepth = wSection.Paragraphs[0].ListFormat.ListLevelNumber;
          }
          for (int i = 0; i < wSection.Paragraphs.Count; i++) {
            IWParagraph paragraph = wSection.Paragraphs[i];
            if (string.IsNullOrWhiteSpace(paragraph.Text)) {
              continue;
            }
            if (paragraph.ListFormat.ListLevelNumber == baseListDepth) {
              newCardSetModels.Add(new CardSetModel(name: paragraph.Text));
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
                  newCardSetModels[newCardSetModels.Count - 1].AddCardToSet(tmpTerm.Trim(), tmpDefinition.Trim());
                  tmpTerm = "";
                  tmpDefinition = "";
                }
              } else if (paragraph.ListFormat.ListLevelNumber > baseListDepth + 1) {
                tmpDefinition = tmpDefinition + (string.IsNullOrWhiteSpace(tmpDefinition) ? "" : "\n") + paragraph.Text;
                if (peekNextListDepth < baseListDepth + 2) {
                  newCardSetModels[newCardSetModels.Count - 1].AddCardToSet(tmpTerm.Trim(), tmpDefinition.Trim());
                  tmpTerm = "";
                  tmpDefinition = "";
                }
              }
            }
          }
        }
        return newCardSetModels;
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
    #endregion
  }
}
