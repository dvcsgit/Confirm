using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Utility.Models;

namespace Utility
{
    public class ExcelHelper : IDisposable
    {
        private ExcelExportModel Model;

        private IWorkbook Workbook;

        #region WorkBook Style
        private IFont HeaderFont;
        private IFont CellFont;

        private ICellStyle HeaderStyle;
        private ICellStyle CellStyle;
        #endregion

        public ExcelHelper(string FileName, Define.EnumExcelVersion ExcelVersion)
        {
            if (ExcelVersion == Define.EnumExcelVersion._2003)
            {
                Workbook = new HSSFWorkbook();
            }

            if (ExcelVersion == Define.EnumExcelVersion._2007)
            {
                Workbook = new XSSFWorkbook();
            }

            Init();

            Model = new ExcelExportModel(FileName, ExcelVersion);
        }

        private void Init()
        {
            #region Header Style
            HeaderFont = Workbook.CreateFont();
            HeaderFont.Color = HSSFColor.Black.Index;
            HeaderFont.Boldweight = (short)FontBoldWeight.Bold;
            HeaderFont.FontHeightInPoints = 12;

            HeaderStyle = Workbook.CreateCellStyle();
            HeaderStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
            HeaderStyle.FillPattern = FillPattern.SolidForeground;
            HeaderStyle.BorderTop = BorderStyle.Thin;
            HeaderStyle.BorderBottom = BorderStyle.Thin;
            HeaderStyle.BorderLeft = BorderStyle.Thin;
            HeaderStyle.BorderRight = BorderStyle.Thin;
            HeaderStyle.SetFont(HeaderFont);
            #endregion

            #region Cell Style
            CellFont = Workbook.CreateFont();
            CellFont.Color = HSSFColor.Black.Index;
            CellFont.Boldweight = (short)FontBoldWeight.Normal;
            CellFont.FontHeightInPoints = 12;

            CellStyle = Workbook.CreateCellStyle();
            CellStyle.BorderTop = BorderStyle.Thin;
            CellStyle.BorderBottom = BorderStyle.Thin;
            CellStyle.BorderLeft = BorderStyle.Thin;
            CellStyle.BorderRight = BorderStyle.Thin;
            CellStyle.SetFont(CellFont);
            #endregion
        }

        public void CreateSheet<T>(IEnumerable<T> DataList)
        {
            CreateSheet("sheet" + (Workbook.NumberOfSheets + 1).ToString(), DataList);
        }

        public void CreateSheet<T>(string SheetName, Dictionary<string, ExcelDisplayItem> TitleListMap, IEnumerable<T> DataList)
        {
            var worksheet = Workbook.CreateSheet(SheetName);

            #region Header
            var row = worksheet.CreateRow(0);

            int cellIndex = 0;

            foreach (var item in TitleListMap)
            {
                var cell = row.CreateCell(cellIndex);

                var title = item.Value;

                cell.CellStyle = HeaderStyle;

                cell.SetCellValue(title.Name);

                cellIndex++;
            }
            #endregion

            #region Cell Data
            InsertDataValues(worksheet, TitleListMap, DataList);
            #endregion

            #region Adjust Size
            for (int i = 0; i < TitleListMap.Count; i++)
            {
                if (TitleListMap.ElementAt(i).Key != "PhotoList")
                    worksheet.AutoSizeColumn(i);
            }
            #endregion
        }

        public void CreateSheet<T>(string SheetName, IEnumerable<T> DataList)
        {
            var datatype = typeof(T);

            var titleListMap = GetPropertyDisplayNamesMap(datatype);

            CreateSheet(SheetName, titleListMap, DataList);
        }

        public ExcelExportModel Export()
        {
            using (var fs = File.OpenWrite(Model.FullFileName))
            {
                Workbook.Write(fs);

                fs.Close();
            }

            byte[] buff = null;

            using (var fs = File.OpenRead(Model.FullFileName))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    long numBytes = new FileInfo(Model.FullFileName).Length;

                    buff = br.ReadBytes((int)numBytes);

                    br.Close();
                }

                fs.Close();
            }

            Model.Data = buff;

            return Model;
        }

        private Dictionary<string, ExcelDisplayItem> GetPropertyDisplayNamesMap(Type Type)
        {
            var titleListMap = new Dictionary<string, ExcelDisplayItem>();

            var propertyInfos = Type.GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                var titleName = GetDisplayName(propertyInfo);

                if (string.IsNullOrEmpty(titleName))
                {
                    continue;
                }

                var cellType = CellType.String;

                var excelType = propertyInfo.GetCustomAttributes(typeof(ExcelDataTypeAttribute), true);

                if (excelType.Length == 1)
                {
                    var checkType = (ExcelDataTypeAttribute)excelType.FirstOrDefault();

                    if (checkType.DataType == DataType.Currency)
                    {
                        cellType = CellType.Numeric;
                    }
                    else if (checkType.DataType == DataType.DateTime)
                    {
                        cellType = CellType.Formula;
                    }
                }

                titleListMap.Add(propertyInfo.Name, new ExcelDisplayItem { Name = titleName, CellType = cellType });
            }

            return titleListMap;
        }

        private string GetDisplayName(MemberInfo MemberInfo)
        {
            var titleName = string.Empty;

            var attribute = MemberInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault();

            if (attribute != null)
            {
                titleName = (attribute as DisplayNameAttribute).DisplayName;

                if (titleName.Contains("@"))
                {
                    titleName = string.Empty;
                }
            }
            else
            {
                attribute = MemberInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

                if (attribute != null)
                {
                    titleName = (attribute as DisplayAttribute).Name;
                }
                else
                {
                    titleName = string.Empty;
                }

            }

            return titleName;
        }

        private void InsertDataValues<T>(ISheet Worksheet, Dictionary<string, ExcelDisplayItem> TitleListMap, IEnumerable<T> DataList)
        {
            var numericFormat = Workbook.CreateDataFormat();

            var numericCellStyle = Workbook.CreateCellStyle();
            numericCellStyle.BorderTop = BorderStyle.Thin;
            numericCellStyle.BorderBottom = BorderStyle.Thin;
            numericCellStyle.BorderLeft = BorderStyle.Thin;
            numericCellStyle.BorderRight = BorderStyle.Thin;
            numericCellStyle.DataFormat = numericFormat.GetFormat("#,##0.000");

            var dateFormat = Workbook.CreateDataFormat();

            var dateCellStyle = Workbook.CreateCellStyle();
            dateCellStyle.BorderTop = BorderStyle.Thin;
            dateCellStyle.BorderBottom = BorderStyle.Thin;
            dateCellStyle.BorderLeft = BorderStyle.Thin;
            dateCellStyle.BorderRight = BorderStyle.Thin;
            dateCellStyle.DataFormat = dateFormat.GetFormat("yyyy/m/d");

            //Insert data values
            for (int i = 1; i < DataList.Count() + 1; i++)
            {
                var tmpRow = Worksheet.CreateRow(i);

                var valueList = GetPropertyValues(DataList.ElementAt(i - 1), TitleListMap);

                for (int j = 0; j < valueList.Count; j++)
                {
                    var rowCell = tmpRow.CreateCell(j);

                    var tempValue = valueList[j].Name;

                    switch (valueList[j].CellType)
                    {
                        case CellType.Numeric:

                            if (string.IsNullOrEmpty(tempValue))
                            {
                                rowCell.SetCellValue("");
                            }
                            else
                            {
                                var value = 0.00;

                                var flag = double.TryParse(tempValue.Replace(",", string.Empty), out value);

                                if (flag)
                                {
                                    rowCell.SetCellValue(value);
                                }
                                else
                                {
                                    rowCell.SetCellValue(tempValue);
                                }
                            }

                            rowCell.CellStyle = numericCellStyle;

                            break;
                        case CellType.Formula:
                            if (!string.IsNullOrEmpty(tempValue))
                            {
                                DateTime dateValue;

                                var flag = DateTime.TryParseExact(tempValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue);

                                if (flag)
                                {
                                    rowCell.SetCellValue(dateValue);
                                }
                                else
                                {
                                    rowCell.SetCellValue(tempValue);
                                }
                            }
                            else
                            {
                                rowCell.SetCellValue(tempValue);
                            }

                            rowCell.CellStyle = dateCellStyle;
                            break;
                        default:
                            if (valueList[j].IsPhotoItem)
                            {
                                //照片
                                rowCell.CellStyle = this.CellStyle;

                                if (!string.IsNullOrEmpty(tempValue))
                                {
                                    try
                                    {
                                        var photoList = tempValue.Split(',').ToList();
                                                                                
                                        var patriarch = Worksheet.CreateDrawingPatriarch();

                                        int colIdx = j; 

                                        foreach (var photo in photoList)
                                        {
                                            Worksheet.SetColumnWidth(colIdx, 30 * 256);

                                            var photoPath = Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, photo);

                                            byte[] bytes = File.ReadAllBytes(photoPath);

                                            int pictureIndex = this.Workbook.AddPicture(bytes, PictureType.JPEG);
                                       
                                            if (Model.ContentType == Define.ExcelContentType_2003)
                                            {
                                                var anchor = new HSSFClientAnchor(0, 0, 0, 0, colIdx, i, colIdx + 1, i + 1) { AnchorType = AnchorType.MoveAndResize }; 
                                                
                                                var picture = patriarch.CreatePicture(anchor, pictureIndex);

                                                var dimension = picture.GetImageDimension();

                                                var ratio = dimension.Height / dimension.Width;

                                                tmpRow.HeightInPoints = Worksheet.GetColumnWidthInPixels(colIdx) * ratio;
                                            }

                                            if (Model.ContentType == Define.ExcelContentType_2007)
                                            {
                                                var anchor = new XSSFClientAnchor(0, 0, 0, 0, colIdx, i, colIdx + 1, i + 1) { AnchorType = AnchorType.MoveAndResize };
                                                
                                                var picture = patriarch.CreatePicture(anchor, pictureIndex);

                                                var dimension = picture.GetImageDimension();

                                                var ratio = dimension.Height / dimension.Width;

                                                tmpRow.HeightInPoints = Worksheet.GetColumnWidthInPixels(colIdx) * ratio;
                                                
                                            }
                                            colIdx++;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log(MethodBase.GetCurrentMethod(), ex);
                                    }
                                }
                            }
                            else
                            {
                                rowCell.SetCellValue(tempValue);
                                rowCell.CellStyle = this.CellStyle;
                            }
                            break;
                    }
                }
            }
        }

        private List<RowItem> GetPropertyValues<T>(T Data, Dictionary<string, ExcelDisplayItem> ColumnMap)
        {
            var propertyValues = new List<RowItem>();

            var sourceData = Data.GetType();

            foreach (var item in ColumnMap)
            {
                var propertyValue = sourceData.GetProperty(item.Key).GetValue(Data, null);

                //判斷是不是PhotoList
                bool isPhotoList = propertyValue != null && sourceData.GetProperty(item.Key).Name == "PhotoList" ? true : false;

                if (isPhotoList)
                {
                    var photoList = propertyValue as List<string>;

                    if (photoList.Count > 0)
                    {
                        var rowValue = "";

                        foreach(var photo in photoList)
                        {
                            rowValue += photo + ",";
                        }

                        rowValue = rowValue.Trim(',');

                        propertyValues.Add(new RowItem { Name = rowValue, CellType = item.Value.CellType, IsPhotoItem = true });
                    }
                    else
                    {
                        var rowValue = string.Empty;
                        propertyValues.Add(new RowItem { Name = rowValue, CellType = item.Value.CellType, IsPhotoItem = false });
                    }
                }
                else
                {
                    var rowValue = propertyValue != null ? propertyValue.ToString() : "";

                    propertyValues.Add(new RowItem { Name = rowValue, CellType = item.Value.CellType, IsPhotoItem = false });
                }
            }

            return propertyValues;
        }

        public class ExcelDisplayItem
        {
            public string Name { get; set; }

            public CellType CellType { get; set; }
        }

        private class RowItem
        {
            public string Name { get; set; }

            public CellType CellType { get; set; }

            public bool IsPhotoItem { get; set; }
        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //...
                }
            }

            _disposed = true;
        }

        ~ExcelHelper()
        {
            Dispose(false);
        }

        #endregion

        public static string GetCellValue(ICell Cell)
        {
            if (Cell != null)
            {
                try
                {
                    switch (Cell.CellType)
                    {
                        case CellType.String:
                            return Cell.StringCellValue;
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(Cell))
                            {
                                return Cell.DateCellValue.ToString();
                            }
                            else
                            {
                                return Cell.NumericCellValue.ToString();
                            }
                        case CellType.Formula:
                            if (Cell.CachedFormulaResultType == CellType.String)
                                return Cell.StringCellValue.Trim();
                            else if (Cell.CachedFormulaResultType == CellType.Numeric)
                                return Cell.NumericCellValue.ToString().Trim();
                            else
                                return string.Empty;
                        default:
                            return string.Empty;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetCellValue(CellValue CellValue)
        {
            if (CellValue != null)
            {
                try
                {
                    switch (CellValue.CellType)
                    {
                        case CellType.String:
                            return CellValue.StringValue;
                        case CellType.Numeric:
                            return Math.Round(CellValue.NumberValue, 5).ToString("f5");
                        default:
                            return string.Empty;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
