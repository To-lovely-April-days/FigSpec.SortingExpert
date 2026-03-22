using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Globalization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace FigSpec.SortingExpert.Tools
{
    public class ExcelHelper
    {
        #region 私有方法

        /// <summary>
        /// 获取要保存的文件名称（含完整路径）
        /// </summary>
        /// <returns></returns>
        public static string GetSaveFilePath(string fileName = null, string filter = "")
        {
            SaveFileDialog saveFileDig = new SaveFileDialog();
            if (string.IsNullOrEmpty(filter))
                filter = "Excel Office97-2003(*.xls)|.xls|Excel Office2007(*.xlsx)|*.xlsx";
            saveFileDig.Filter = filter;
            saveFileDig.FilterIndex = 0;
            saveFileDig.OverwritePrompt = true;
            //saveFileDig.InitialDirectory = DesktopDirectory;
            saveFileDig.FileName = fileName;
            string filePath = null;
            if (saveFileDig.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDig.FileName;
            }

            return filePath;
        }

        public static string GetOpenFilePath(string filter, string initialDirectory, out string mess)
        {
            mess = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (string.IsNullOrEmpty(filter))
                filter = "Excel(*xls*)|*.xls*|Excel Office97-2003(*.xls)|.xls|Excel Office2007(*.xlsx)|*.xlsx";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择文件".ToMultiLanguage();
            openFileDialog.Filter = filter; //设置要选择的文件的类型
            if (!string.IsNullOrEmpty(initialDirectory))
                openFileDialog.InitialDirectory = initialDirectory; 
            string filePath = null;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }

            if (FileIsOpen(filePath) && !string.IsNullOrEmpty(filePath))
            {
                mess = "文件被占用".ToMultiLanguage();
                return null;
            }

            return filePath;
        }

        public static string[] GetOpenFilePaths(string filter, string initialDirectory)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (string.IsNullOrEmpty(filter))
                filter = "Excel(*xls*)|*.xls*|Excel Office97-2003(*.xls)|.xls|Excel Office2007(*.xlsx)|*.xlsx";
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "请选择文件".ToMultiLanguage(); 
            openFileDialog.Filter = filter; //设置要选择的文件的类型
            if (!string.IsNullOrEmpty(initialDirectory))
                openFileDialog.InitialDirectory = initialDirectory; // GlobalSettings.ApplyInfo.CalibrationResultPath;
            string[] filePath = null;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileNames;
            }
            return filePath;
        }


        /// <summary>
        /// 获取要导入的文件名称（含完整路径）
        /// </summary>
        /// <returns></returns>
        public static string GetOpenFilePath(string filter = null)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (string.IsNullOrEmpty(filter))
                filter = "Excel(*xls*)|*.xls*|Excel Office97-2003(*.xls)|.xls|Excel Office2007(*.xlsx)|*.xlsx";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择文件".ToMultiLanguage();
            openFileDialog.Filter = filter; //设置要选择的文件的类型

            string filePath = null;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }

            if (FileIsOpen(filePath))
            {
                return null;
            }

            return filePath;
        }

        /// <summary>
        /// 判断是否为兼容模式
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool GetIsCompatible(string filePath)
        {
            return filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
        }



        /// <summary>
        /// 创建工作薄
        /// </summary>
        /// <param name="isCompatible"></param>
        /// <returns></returns>
        private static IWorkbook CreateWorkbook(bool isCompatible)
        {
            if (isCompatible)
            {
                return new HSSFWorkbook();
            }
            else
            {
                return new XSSFWorkbook();
            }
        }

        /// <summary>
        /// 创建工作薄(依据文件流)
        /// </summary>
        /// <param name="isCompatible"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static IWorkbook CreateWorkbook(bool isCompatible, dynamic stream)
        {
            if (isCompatible)
            {
                return new HSSFWorkbook(stream);
            }
            else
            {
                return new XSSFWorkbook(stream);
            }
        }

        /// <summary>
        /// 创建表格头单元格
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private static ICellStyle GetCellStyle(IWorkbook workbook)
        {
            ICellStyle style = workbook.CreateCellStyle();
            style.FillPattern = FillPattern.SolidForeground;
            style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;

            return style;
        }


        /// <summary>
        /// 从工作表中生成DataTable
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="headerRowIndex"></param>
        /// <returns></returns>
        private static DataTable GetDataTableFromSheet(ISheet sheet, int headerRowIndex)
        {
            DataTable table = new DataTable();
            table.TableName = sheet.SheetName; //设置表格名称

            IRow headerRow = sheet.GetRow(headerRowIndex);
            if (headerRow == null)
                return null;
            int cellCount = headerRow.LastCellNum;

            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                var sss = headerRow.GetCell(i);

                if (headerRow.GetCell(i) == null || headerRow.GetCell(i).ToString().Trim() == "")
                {
                    // 如果遇到第一个空列，则不再继续向后读取
                    cellCount = i + 1;
                    break;
                }
                DataColumn column = new DataColumn(headerRow.GetCell(i).ToString());
                table.Columns.Add(column);
            }

            for (int i = (headerRowIndex + 1); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);

                if (row != null && row.Cells.Count > 0 && !string.IsNullOrEmpty(row.Cells[0].ToString()))
                {
                    DataRow dataRow = table.NewRow();

                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {
                        if (row.GetCell(j) != null)
                        {
                            dataRow[j] = getCellValueByCell(row.GetCell(j));
                        }
                    }

                    table.Rows.Add(dataRow);
                }
            }

            return table;
        }

        /// <summary>
        /// 获取单元格各类型值，返回字符串类型
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static String getCellValueByCell(ICell cell)
        {
            if (string.IsNullOrEmpty(cell.ToString()))
            {
                return string.Empty;
            }

            switch (cell.CellType)
            {
                case CellType.String: //字符串类型
                    return cell.StringCellValue.Trim();
                case CellType.Boolean:  //布尔类型
                    return cell.BooleanCellValue.ToString();
                case CellType.Numeric: //数值类型
                    return cell.NumericCellValue.ToString();
                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.String: //字符串类型
                            return cell.StringCellValue.Trim();
                        case CellType.Boolean:  //布尔类型
                            return cell.BooleanCellValue.ToString();
                        case CellType.Numeric: //数值类型
                            return cell.NumericCellValue.ToString();
                        default:
                            return cell.ToString();
                    }
                default:
                    return cell.ToString();
            }
        }

        #endregion

        #region 公共导出方法

        /// <summary>
        /// 由DataSet导出Excel
        /// </summary>
        /// <param name="sourceDs">要导出数据的DataTable集合</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>Excel工作表</returns>
        public static string ExportToExcel(DataSet sourceDs, string filter = "", string fileName = null, string filePath = null)
        {

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetSaveFilePath(fileName, filter);
            }

            if (string.IsNullOrEmpty(filePath)) return "找不到文件路径".ToMultiLanguage(); ;

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception)
                {

                    return "文件已被其他程序打开".ToMultiLanguage(); ;
                }
            }

            bool isCompatible = GetIsCompatible(filePath);

            IWorkbook workbook = CreateWorkbook(isCompatible);
            ICellStyle cellStyle = GetCellStyle(workbook);

            for (int i = 0; i < sourceDs.Tables.Count; i++)
            {
                DataTable table = sourceDs.Tables[i];
                string sheetName = string.IsNullOrEmpty(table.TableName) ? ("Sheet" + i) : table.TableName;
                ISheet sheet = workbook.CreateSheet(sheetName);
                IRow headerRow = sheet.CreateRow(0);
                // handling header.
                foreach (DataColumn column in table.Columns)
                {
                    ICell cell = headerRow.CreateCell(column.Ordinal);
                    cell.SetCellValue(column.ColumnName);
                    cell.CellStyle = cellStyle;
                }

                // handling value.
                int rowIndex = 1;

                foreach (DataRow row in table.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in table.Columns)
                    {

                        if (column.DataType.IsValueType)
                        {
                            if (row[column] == DBNull.Value)
                                continue;
                            dataRow.CreateCell(column.Ordinal).SetCellValue(Convert.ToDouble(row[column]));
                        }
                        else
                        {
                            dataRow.CreateCell(column.Ordinal).SetCellValue((row[column] ?? "").ToString());
                        }
                    }

                    rowIndex++;
                }
            }

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            workbook.Write(fs);
            fs.Dispose();
            workbook = null;

            return null;

        }


        /// <summary>
        /// 由DataTable导出Excel
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="sourceTable">要导出数据的DataTable</param>
        /// <returns>Excel工作表</returns>
        public static string ExportToExcel(DataTable sourceTable, string filter = "", string fileName = null, string sheetName = "Sheet", string filePath = null)
        {


            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetSaveFilePath(fileName, filter);
            }

            if (string.IsNullOrEmpty(filePath))
                return "导出失败".ToMultiLanguage(); ;

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception)
                {

                    return "文件已被其他程序打开".ToMultiLanguage(); ;
                }
            }
            bool isCompatible = GetIsCompatible(filePath);

            IWorkbook workbook = CreateWorkbook(isCompatible);
            ICellStyle cellStyle = GetCellStyle(workbook);

            ISheet sheet = workbook.CreateSheet(sheetName);
            IRow headerRow = sheet.CreateRow(0);
            // handling header.
            foreach (DataColumn column in sourceTable.Columns)
            {
                ICell headerCell = headerRow.CreateCell(column.Ordinal);
                headerCell.SetCellValue(column.ColumnName);
                headerCell.CellStyle = cellStyle;
            }

            // handling value.
            int rowIndex = 1;

            foreach (DataRow row in sourceTable.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);

                foreach (DataColumn column in sourceTable.Columns)
                {
                    if (column.DataType.IsValueType)
                    {
                        dataRow.CreateCell(column.Ordinal).SetCellValue(Convert.ToDouble(row[column]));
                    }
                    else
                    {
                        dataRow.CreateCell(column.Ordinal).SetCellValue((row[column] ?? "").ToString());
                    }
                }

                rowIndex++;
            }
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            workbook.Write(fs);
            fs.Dispose();

            sheet = null;
            headerRow = null;
            workbook = null;

            return null;
        }



        /// <summary>
        /// 由List导出Excel
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="data">在导出的List</param>
        /// <param name="sheetName">sheet名称</param>
        /// <returns></returns>
        public static string ExportToExcel<T>(List<T> data, IList<KeyValuePair<string, string>> headerNameList, string sheetName = "Sheet", string filePath = null) where T : class
        {
            if (data.Count <= 0) return null;

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetSaveFilePath();
            }

            if (string.IsNullOrEmpty(filePath)) return null;

            bool isCompatible = GetIsCompatible(filePath);

            IWorkbook workbook = CreateWorkbook(isCompatible);
            ICellStyle cellStyle = GetCellStyle(workbook);
            ISheet sheet = workbook.CreateSheet(sheetName);
            IRow headerRow = sheet.CreateRow(0);

            for (int i = 0; i < headerNameList.Count; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellValue(headerNameList[i].Value);
                cell.CellStyle = cellStyle;
            }

            Type t = typeof(T);
            int rowIndex = 1;
            foreach (T item in data)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                for (int n = 0; n < headerNameList.Count; n++)
                {
                    object pValue = t.GetProperty(headerNameList[n].Key).GetValue(item, null);
                    dataRow.CreateCell(n).SetCellValue((pValue ?? "").ToString());
                }
                rowIndex++;
            }
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            workbook.Write(fs);
            fs.Dispose();

            sheet = null;
            headerRow = null;
            workbook = null;

            return filePath;
        }

        /// <summary>
        /// 由DataGridView导出
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="sheetName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ExportToExcel(DataGridView grid, string sheetName = "Sheet", string filePath = null)
        {
            if (grid.Rows.Count <= 0) return null;

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetSaveFilePath();
            }

            if (string.IsNullOrEmpty(filePath)) return null;

            bool isCompatible = GetIsCompatible(filePath);

            IWorkbook workbook = CreateWorkbook(isCompatible);
            ICellStyle cellStyle = GetCellStyle(workbook);
            ISheet sheet = workbook.CreateSheet(sheetName);

            IRow headerRow = sheet.CreateRow(0);

            for (int i = 0; i < grid.Columns.Count; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellValue(grid.Columns[i].Name);
                cell.CellStyle = cellStyle;
            }

            int rowIndex = 1;
            foreach (DataGridViewRow row in grid.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                for (int n = 0; n < grid.Columns.Count; n++)
                {
                    dataRow.CreateCell(n).SetCellValue((row.Cells[n].Value ?? "").ToString());
                }
                rowIndex++;
            }

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            workbook.Write(fs);
            fs.Dispose();

            sheet = null;
            headerRow = null;
            workbook = null;

            return filePath;
        }

        public static string ExportToExcel(List<string> columns, object[][] datas, bool isLengthwise = false, string sheetName = "Sheet", string filePath = null)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception)
                {

                    return "文件已被其他程序打开".ToMultiLanguage(); ;
                }
            }

            bool isCompatible = GetIsCompatible(filePath);

            IWorkbook workbook = CreateWorkbook(isCompatible);


            ISheet sheet = workbook.CreateSheet(sheetName);

            if (isLengthwise)
            {
                //纵向导出
                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    IRow dataRow = sheet.CreateRow(colIndex);
                    dataRow.CreateCell(0).SetCellValue(columns[colIndex]);
                    for (int rowIndex = 0; rowIndex < datas.Length; rowIndex++)
                    {
                        object value = datas[rowIndex][colIndex];
                        ICell cell = dataRow.CreateCell(rowIndex + 1);
                        SetCellValue(cell, value);
                    }
                }
            }
            else
            {
                ICellStyle cellStyle = GetCellStyle(workbook);
                IRow headerRow = sheet.CreateRow(0);
                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    headerRow.CreateCell(colIndex).SetCellValue(columns[colIndex]);
                }
                for (int rowIndex = 0; rowIndex < datas.Length; rowIndex++)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex + 1);
                    for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                    {
                        ICell cell = dataRow.CreateCell(colIndex);
                        object value = datas[rowIndex][colIndex];
                        SetCellValue(cell, value);
                    }
                }
            }

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            workbook.Write(fs);
            fs.Dispose();

            sheet = null;
            workbook = null;
            return null;
        }

        private static void SetCellValue(ICell cell, object value)
        {
            if (value is null)
                return;
            Type valueType = value.GetType();
            if (valueType == typeof(DateTime))
            {
                cell.SetCellValue(Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss fff"));
            }
            else if (valueType.IsValueType && valueType != typeof(bool) && valueType != typeof(char))
            {
                cell.SetCellValue(Convert.ToDouble(value));
            }
            else if (valueType == typeof(bool))
            {
                cell.SetCellValue(Convert.ToBoolean(value));
            }
            else
            {
                cell.SetCellValue(Convert.ToString(value));
            }
        }
        #endregion

        #region 公共导入方法

        /// <summary>
        /// 读取反射率文件
        /// </summary>
        /// <param name="filePath">反射率文件路径</param>
        /// <returns>波长数据和反射率数组</returns>
        public static (float[], float[]) ReadExcelColumns(string filePath)
        {
            DataTable dt = ImportFromExcel(string.Empty, 0, filePath);
            List<float> column1Data = new List<float>();
            List<float> column2Data = new List<float>();
            foreach (DataRow row in dt.Rows)
            {
                column1Data.Add(Convert.ToSingle(row[0]));
                column2Data.Add(Convert.ToSingle(row[1]) / 100f);
            }


            return (column1Data.ToArray(), column2Data.ToArray());
        }


        /// <summary>
        /// 由Excel导入DataTable
        /// </summary>
        /// <param name="excelFileStream">Excel文件流</param>
        /// <param name="sheetName">Excel工作表名称</param>
        /// <param name="headerRowIndex">Excel表头行索引</param>
        /// <param name="isCompatible">是否为兼容模式</param>
        /// <returns>DataTable</returns>
        public static DataTable ImportFromExcel(Stream excelFileStream, string sheetName, int headerRowIndex, bool isCompatible)
        {
            IWorkbook workbook = CreateWorkbook(isCompatible, excelFileStream);
            ISheet sheet = null;
            int sheetIndex = 0;
            if (string.IsNullOrEmpty(sheetName) || int.TryParse(sheetName, out sheetIndex))
            {
                sheet = workbook.GetSheetAt(sheetIndex);
            }
            else
            {
                sheet = workbook.GetSheet(sheetName);
            }

            DataTable table = GetDataTableFromSheet(sheet, headerRowIndex);

            excelFileStream.Close();
            workbook = null;
            sheet = null;
            return table;
        }

        /// <summary>
        /// 由Excel导入DataTable
        /// </summary>
        /// <param name="excelFilePath">Excel文件路径，为物理路径。</param>
        /// <param name="sheetName">Excel工作表名称</param>
        /// <param name="headerRowIndex">Excel表头行索引</param>
        /// <returns>DataTable</returns>
        public static DataTable ImportFromExcel(string sheetName, int headerRowIndex, string excelFilePath = null)
        {
            if (string.IsNullOrEmpty(excelFilePath))
            {
                excelFilePath = GetOpenFilePath();
            }

            if (string.IsNullOrEmpty(excelFilePath)) return null;

            using (FileStream stream = System.IO.File.OpenRead(excelFilePath))
            {
                bool isCompatible = GetIsCompatible(excelFilePath);
                return ImportFromExcel(stream, sheetName, headerRowIndex, isCompatible);
            }
        }

        /// <summary>
        /// 由Excel导入DataSet，如果有多个工作表，则导入多个DataTable
        /// </summary>
        /// <param name="excelFileStream">Excel文件流</param>
        /// <param name="headerRowIndex">Excel表头行索引</param>
        /// <param name="isCompatible">是否为兼容模式</param>
        /// <returns>DataSet</returns>
        public static DataSet ImportFromExcel(Stream excelFileStream, int headerRowIndex, bool isCompatible)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook = CreateWorkbook(isCompatible, excelFileStream);
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                DataTable table = GetDataTableFromSheet(sheet, headerRowIndex);
                if (table == null)
                    continue;
                ds.Tables.Add(table);
            }

            excelFileStream.Close();
            workbook = null;

            return ds;
        }

        /// <summary>
        /// 由Excel导入DataSet，如果有多个工作表，则导入多个DataTable
        /// </summary>
        /// <param name="excelFilePath">Excel文件路径，为物理路径。</param>
        /// <param name="headerRowIndex">Excel表头行索引</param>
        /// <returns>DataSet</returns>
        public static DataSet ImportFromExcel(int headerRowIndex, string excelFilePath = null)
        {
            if (string.IsNullOrEmpty(excelFilePath))
            {
                excelFilePath = GetOpenFilePath();
            }

            if (string.IsNullOrEmpty(excelFilePath)) return null;

            using (FileStream stream = System.IO.File.OpenRead(excelFilePath))
            {
                bool isCompatible = GetIsCompatible(excelFilePath);
                return ImportFromExcel(stream, headerRowIndex, isCompatible);
            }
        }

        #endregion

        #region 公共转换方法

        /// <summary>
        /// 将Excel的列索引转换为列名，列索引从0开始，列名从A开始。如第0列为A，第1列为B...
        /// </summary>
        /// <param name="index">列索引</param>
        /// <returns>列名，如第0列为A，第1列为B...</returns>
        public static string ConvertColumnIndexToColumnName(int index)
        {
            index = index + 1;
            int system = 26;
            char[] digArray = new char[100];
            int i = 0;
            while (index > 0)
            {
                int mod = index % system;
                if (mod == 0) mod = system;
                digArray[i++] = (char)(mod - 1 + 'A');
                index = (index - 1) / 26;
            }
            StringBuilder sb = new StringBuilder(i);
            for (int j = i - 1; j >= 0; j--)
            {
                sb.Append(digArray[j]);
            }
            return sb.ToString();
        }


        /// <summary>
        /// 转化日期
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public static DateTime ConvertDate(object date)
        {
            string dtStr = (date ?? "").ToString();

            DateTime dt = new DateTime();

            if (DateTime.TryParse(dtStr, out dt))
            {
                return dt;
            }

            try
            {
                string spStr = "";
                if (dtStr.Contains("-"))
                {
                    spStr = "-";
                }
                else if (dtStr.Contains("/"))
                {
                    spStr = "/";
                }
                string[] time = dtStr.Split(spStr.ToCharArray());
                int year = Convert.ToInt32(time[2]);
                int month = Convert.ToInt32(time[0]);
                int day = Convert.ToInt32(time[1]);
                string years = Convert.ToString(year);
                string months = Convert.ToString(month);
                string days = Convert.ToString(day);
                if (months.Length == 4)
                {
                    dt = Convert.ToDateTime(date);
                }
                else
                {
                    string rq = "";
                    if (years.Length == 1)
                    {
                        years = "0" + years;
                    }
                    if (months.Length == 1)
                    {
                        months = "0" + months;
                    }
                    if (days.Length == 1)
                    {
                        days = "0" + days;
                    }
                    rq = "20" + years + "-" + months + "-" + days;
                    dt = Convert.ToDateTime(rq);
                }
            }
            catch
            {
                throw new Exception("日期格式不正确，转换日期失败！".ToMultiLanguage());
            }
            return dt;
        }

        /// <summary>
        /// 转化数字
        /// </summary>
        /// <param name="d">数字字符串</param>
        /// <returns></returns>
        public static decimal ConvertDecimal(object d)
        {
            string dStr = (d ?? "").ToString();
            decimal result = 0;
            if (decimal.TryParse(dStr, out result))
            {
                return result;
            }
            else
            {
                throw new Exception("数字格式不正确，转换数字失败！".ToMultiLanguage());
            }

        }


        [DllImport("kernel32.dll")]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);
        private const int OF_READWRITE = 2;
        private const int OF_SHARE_DENY_NONE = 0x40;
        private static readonly IntPtr HFILE_ERROR = new IntPtr(-1);
        public static bool FileIsOpen(string fileFullName)
        {
            if (!File.Exists(fileFullName))
            {
                return true;
            }
            IntPtr handle = _lopen(fileFullName, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (handle == HFILE_ERROR)
            {
                return true;
            }
            CloseHandle(handle);
            return false;
        }

        #endregion

        #region
        public static string CheckTitle(string[] titles, DataTable dt)
        {
            if (dt.Columns.Count != titles.Length)
            {
                return string.Format("工作表{0}列数与模版不匹配".ToMultiLanguage(), dt.TableName);
            }
            for (int i = 0; i < titles.Length; i++)
            {

                if (titles[i].ToLower() != dt.Columns[i].ToString().ToLower().Trim())
                {
                    return "\"" + dt.TableName + "\"" + string.Format("工作表中的“{0}”列无法识别".ToMultiLanguage(), dt.Columns[i]);
                }
            }
            return null;
        }

        #endregion
    }
}
