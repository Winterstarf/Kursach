using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Data.SqlClient;
using System.Data.Common;
using Dapper;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Dynamic;

namespace MainApp.windows.main
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly string connectionString = "Data Source=DESKTOP-QLMK9N;Initial Catalog=HelixDB;Integrated Security=SSPI";

        public string export_type;
        public string export_page_name;
        private bool isExportAll = false;

        public ExportWindow()
        {
            InitializeComponent();
            Loaded += ExportWindow_Loaded;
        }

        private async void ExportWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ExportInfo_tb.Text = $"Экспорт: {export_page_name} → .{export_type}";
            try
            {
                int totalRows = 0;

                if (export_page_name.Trim() == "Клиенты")
                    totalRows = await GetTableRowCountAsync("clients");
                else if (export_page_name.Trim() == "Услуги")
                    totalRows = await GetTableRowCountAsync("medical_services");
                else if (export_page_name.Trim() == "Архив заказов")
                    totalRows = await GetTableRowCountAsync("clients_services");

                TotalRow_tb.Text = totalRows.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении количества строк: {ex.Message}");
            }
        }

        private async Task<int> GetTableRowCountAsync(string tableName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                SqlCommand command;

                if (tableName == "clients_services")
                {
                    command = new SqlCommand("SELECT COUNT(DISTINCT id_order) FROM clients_services WHERE id_status = 1", connection);
                }
                else if (tableName == "clients_services_full")
                {
                    command = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", connection);
                }
                else
                {
                    command = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", connection);
                }

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        private async void Export_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RowCount_tb.Text, out int rowCount) || rowCount <= 0)
            {
                MessageBox.Show("Введите корректное число строк.");
                return;
            }

            int maxRows;
            string tableName;

            if (export_page_name.Trim() == "Клиенты")
                tableName = "clients";
            else if (export_page_name.Trim() == "Услуги")
                tableName = "medical_services";
            else if (export_page_name.Trim() == "Архив заказов")
                tableName = "clients_services";
            else
            {
                MessageBox.Show("Неизвестная страница для экспорта.");
                return;
            }

            try
            {
                maxRows = await GetTableRowCountAsync(tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении количества строк: " + ex.Message);
                return;
            }

            // if trying to export more orders than exists
            if (rowCount > maxRows)
            {
                string message = export_page_name.Contains("Архив заказов")
                    ? $"Вы пытаетесь экспортировать {rowCount} заказов, но в базе только {maxRows} уникальных заказов (по id_order)."
                    : $"Вы пытаетесь экспортировать {rowCount} строк, но в базе только {maxRows}.";

                MessageBox.Show(message);
                return;
            }

            // for archive use loaddataasync with clients_services
            var data = await LoadDataAsync(export_page_name.Contains("Архив") ? rowCount : (int?)rowCount);
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            string path = ShowSaveFileDialog();
            if (string.IsNullOrWhiteSpace(path))
                return;

            ExportData(data, export_type, path);
        }

        private async void ExportAll_btn_Click(object sender, RoutedEventArgs e)
        {
            isExportAll = true;

            var data = await LoadDataAsync(null);
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            string path = ShowSaveFileDialog();
            if (string.IsNullOrWhiteSpace(path))
                return;

            ExportData(data, export_type, path);

            isExportAll = false;
        }

        private string ShowSaveFileDialog()
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            string rowCountText = isExportAll ? "_all" : RowCount_tb.Text;

            string fileName = $"Экспорт_{export_page_name}_{(export_type == "pdf" ? "PDF" : "Excel")}_{rowCountText}_{currentDate}";

            var dialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = export_type == "pdf" ? "PDF files (*.pdf)|*.pdf" : "Excel files (*.xlsx)|*.xlsx"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private async Task<List<ExpandoObject>> LoadDataAsync(int? limit)
        {
            var data = new List<ExpandoObject>();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string sql = null;

                if (export_page_name.Contains("Клиенты"))
                {
                    sql = @"SELECT TOP(@limit) c.id, c.last_name, c.first_name, c.middle_name, c.phone_number, g.gender_name, c.email, c.card_number, c.card_balance
                    FROM clients c
                    LEFT JOIN genders g ON g.id = c.id_gender";
                }
                else if (export_page_name.Contains("Услуги"))
                {
                    sql = @"SELECT TOP(@limit) ms.id, ms.mservice_name, ms.mservice_icd, ms.mservice_price, ms.mservice_description, st.stype_name, ms.extra_info
                    FROM medical_services ms
                    LEFT JOIN service_types st ON st.id = ms.id_type";
                }
                else if (export_page_name.Contains("Архив заказов"))
                {
                    sql = @"
                    WITH UniqueOrders AS (
                        SELECT id_order
                        FROM clients_services
                        WHERE id_status = 1
                        GROUP BY id_order
                        ORDER BY id_order
                        OFFSET 0 ROWS FETCH NEXT @limit ROWS ONLY
                    )
                    SELECT
                        cs.id_order                     AS id_order,
                        cs.id_client                    AS id_client,
                        cl.last_name                    AS last_name,
                        cl.first_name                   AS first_name,
                        cl.middle_name                  AS middle_name,
                        st.status_name                  AS status_name,
                        ms.mservice_name                AS mservice_name,
                        ms.mservice_price               AS mservice_price,
                        ms.mservice_icd                 AS mservice_icd,
                        ms.mservice_description         AS mservice_description,
                        totals.total                    AS total
                    FROM clients_services cs
                    JOIN UniqueOrders uo     ON cs.id_order  = uo.id_order
                    LEFT JOIN clients cl      ON cl.id        = cs.id_client
                    LEFT JOIN statuses st     ON st.id        = cs.id_status
                    JOIN medical_services ms  ON ms.id        = cs.id_service
                    CROSS APPLY (
                        SELECT SUM(ms2.mservice_price) AS total
                        FROM clients_services cs2
                        JOIN medical_services ms2 ON cs2.id_service = ms2.id
                        WHERE cs2.id_order = cs.id_order
                    ) totals
                    ORDER BY cs.id_order, cs.id_service;
                    ";
                }

                if (sql == null)
                    return null;

                if (!limit.HasValue)
                    sql = sql.Replace("TOP(@limit)", "");

                var cmd = new SqlCommand(sql, conn);

                if (limit.HasValue)
                    cmd.Parameters.AddWithValue("@limit", limit.Value);

                var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    IDictionary<string, object> expando = new ExpandoObject();
                    for (int i = 0; i < reader.FieldCount; i++)
                        expando[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    data.Add((ExpandoObject)expando);
                }
            }

            return data;
        }

        private void ExportData(IEnumerable<ExpandoObject> data, string type, string path)
        {
            if (type == "pdf")
                ExportToPdf(data, "Экспорт", path);
            else
                ExportToXlsx(data, "Экспорт", path);
        }

        private void ExportToXlsx(IEnumerable<ExpandoObject> data, string sheetName, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            int row = 1;
            var first = data.First();
            var headers = ((IDictionary<string, object>)first).Keys.ToList();

            for (int col = 1; col <= headers.Count; col++)
                ws.Cell(row, col).Value = headers[col - 1];

            foreach (var item in data)
            {
                row++;
                var dict = (IDictionary<string, object>)item;
                int col = 1;
                foreach (var val in dict.Values)
                    ws.Cell(row, col++).Value = val?.ToString();
            }

            wb.SaveAs(path);
        }

        private void ExportToPdf(IEnumerable<ExpandoObject> data, string title, string path)
        {
            string arialPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                "arial.ttf"
            );

            // create basefont
            BaseFont bf = BaseFont.CreateFont(arialPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            // fonts
            var cellFont = new iTextSharp.text.Font(bf, 10, Font.NORMAL);
            var titleFont = new iTextSharp.text.Font(bf, 14, Font.BOLD);

            // doc init
            var doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            doc.Open();

            doc.Add(new iTextSharp.text.Paragraph(title, titleFont));

            var dict0 = (IDictionary<string, object>)data.First();
            var table = new PdfPTable(dict0.Count)
            {
                WidthPercentage = 100
            };

            // headers
            foreach (var key in dict0.Keys)
            {
                table.AddCell(new PdfPCell(new Phrase(key, cellFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }

            // Данные
            foreach (var item in data)
            {
                foreach (var val in ((IDictionary<string, object>)item).Values)
                {
                    table.AddCell(new Phrase(val?.ToString() ?? "", cellFont));
                }
            }

            doc.Add(table);
            doc.Close();
        }
    }
}
