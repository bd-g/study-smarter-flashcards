using System;
using System.IO;
using OfficeOpenXml;
using StudySmarterFlashcards.Sets;

namespace StudySmarterFlashcards.ImportTools
{
  public static class ImportFlashcardService
  {
    #region Public Methods
    public static CardSetModel ImportFromFile(string fileName)
    {
      if (fileName.Substring(Math.Max(0, fileName.Length - 5)).Equals(".xlsx")){
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        if (!File.Exists(fileName)) {
          throw new FileNotFoundException(string.Format("Could not locate the file {0}. Please verify the file exists at the given location.", fileName));
        }
        FileInfo fileInfo = new FileInfo(fileName);

        VerifyExcelFileIsParseable(fileInfo);



      }
      

      return null;
    }
    #endregion

    #region Private Methods
    private static void VerifyExcelFileIsParseable(FileInfo excelFileInfo)
    {
      using (ExcelPackage excelPackage = new ExcelPackage(excelFileInfo)) {
        foreach(ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets) {
          if (!worksheet.Cells["A1"].Value.ToString().Contains("Term", StringComparison.CurrentCultureIgnoreCase) || !worksheet.Cells["B1"].Value.ToString().Contains("Definition", StringComparison.CurrentCultureIgnoreCase)) {
            throw new NotSupportedException(string.Format("Excel file is not in a supported format. The worksheet {0} doesn't contain the proper headers. Please review formatting rules for importing excel files.", worksheet.Name));
          }
        }
      }
    }
    #endregion


  }
}
