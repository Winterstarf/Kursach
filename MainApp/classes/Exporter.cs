using ClosedXML.Excel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;

namespace MainApp.classes
{
    public static class Exporter
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["HelixDB"].ConnectionString;
        // гыча v3
        public static async Task<List<ExpandoObject>> LoadDataAsync(string exportPageName, int? limit)
        {
            var data = new List<ExpandoObject>();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string sql = null;

                if (exportPageName == "Клиенты")
                {
                    sql = @"SELECT " + (limit.HasValue ? "TOP(@limit) " : "") +
                          "c.id, c.last_name, c.first_name, c.middle_name, c.phone_number, g.gender_name, c.email, c.card_number, c.card_balance " +
                          "FROM clients c LEFT JOIN genders g ON g.id = c.id_gender";
                }
                else if (exportPageName == "Услуги")
                {
                    sql = @"SELECT " + (limit.HasValue ? "TOP(@limit) " : "") +
                          "ms.id, ms.mservice_name, ms.mservice_icd, ms.mservice_price, ms.mservice_description, st.stype_name, ms.extra_info " +
                          "FROM medical_services ms LEFT JOIN service_types st ON st.id = ms.id_type";
                }
                else if (exportPageName == "Архив заказов")
                {
                    sql = $@"
                            WITH UniqueOrders AS (
                                SELECT DISTINCT id_order 
                                FROM clients_services 
                                WHERE id_status = 1
                                {(limit.HasValue ? "ORDER BY id_order OFFSET 0 ROWS FETCH NEXT @limit ROWS ONLY" : "")}
                            )
                            SELECT
                                cs.id_order,
                                cs.id_client,
                                cl.last_name,
                                cl.first_name,
                                cl.middle_name,
                                st.status_name,
                                ms.mservice_name,
                                ms.mservice_price,
                                ms.mservice_icd,
                                ms.mservice_description,
                                MIN(cs.total_price) AS total
                            FROM clients_services cs
                            JOIN UniqueOrders uo ON cs.id_order = uo.id_order
                            LEFT JOIN clients cl ON cl.id = cs.id_client
                            LEFT JOIN statuses st ON st.id = cs.id_status
                            JOIN medical_services ms ON ms.id = cs.id_service
                            WHERE cs.id_status = 1
                            GROUP BY
                                cs.id_order, cs.id_client,
                                cl.last_name, cl.first_name, cl.middle_name,
                                st.status_name,
                                ms.mservice_name, ms.mservice_price, ms.mservice_icd, ms.mservice_description
                            ORDER BY cs.id_order";
                }

                if (sql == null)
                    return data;

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (limit.HasValue)
                        cmd.Parameters.AddWithValue("@limit", limit.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            IDictionary<string, object> expando = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                                expando[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            data.Add((ExpandoObject)expando);
                        }
                    }
                }
            }
            return data;
        }

        public static async Task ExportAsync(string exportType, string exportPageName, int? limit, string path)
        {
            var data = await LoadDataAsync(exportPageName, limit);
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта");
                return;
            }
            if (exportType == "pdf")
                ExportToPdf(data, exportPageName, path);
            else
                ExportToXlsx(data, exportPageName, path);

            MessageBox.Show("Экспорт завершён");
        }

        public static async Task ExportSelectedByIdsAsync(IEnumerable<int> selectedIds, string exportType, string exportPageName)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                MessageBox.Show("Нет выбранных строк для экспорта");
                return;
            }
            var data = await LoadSelectedDataAsync(selectedIds, exportPageName);
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта");
                return;
            }
            var dlg = new SaveFileDialog
            {
                FileName = $"Экспорт_{exportPageName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.{exportType}",
                Filter = exportType == "pdf" ? "PDF files (*.pdf)|*.pdf" : "Excel files (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() != true)
                return;
            if (exportType == "pdf") ExportToPdf(data, exportPageName, dlg.FileName);
            else ExportToXlsx(data, exportPageName, dlg.FileName);
            MessageBox.Show("Экспорт выбранных завершён");
        }

        private static async Task<List<ExpandoObject>> LoadSelectedDataAsync(IEnumerable<int> ids, string exportPageName)
        {
            return await LoadDataAsync(exportPageName, null)
                .ContinueWith(t => t.Result.Where(o => {
                    var dict = (IDictionary<string, object>)o;
                    return ids.Contains(Convert.ToInt32(dict[exportPageName == "Архив заказов" ? "id_order" : "id"]));
                }).ToList());
        }

        private static void ExportToXlsx(IEnumerable<ExpandoObject> data, string sheetName, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            int row = 1;
            var first = data.First();
            var headers = ((IDictionary<string, object>)first).Keys.ToList();

            for (int col = 1; col <= headers.Count; col++)
            {
                var key = headers[col - 1];
                ws.Cell(row, col).Value = HeaderTranslations.ContainsKey(key) ? HeaderTranslations[key] : key;
            }

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

        private static void ExportToPdf(IEnumerable<ExpandoObject> data, string title, string path)
        {
            string arialPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            BaseFont bf = BaseFont.CreateFont(arialPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var cellFont = new Font(bf, 10);
            var titleFont = new Font(bf, 14, Font.BOLD);

            var doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            doc.Open();
            doc.Add(new Paragraph(title, titleFont));

            var dict0 = (IDictionary<string, object>)data.First();
            var table = new PdfPTable(dict0.Count) { WidthPercentage = 100 };

            foreach (var key in dict0.Keys)
            {
                string displayName = HeaderTranslations.ContainsKey(key) ? HeaderTranslations[key] : key;
                table.AddCell(new PdfPCell(new Phrase(displayName, cellFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }

            foreach (var item in data)
                foreach (var val in ((IDictionary<string, object>)item).Values)
                    table.AddCell(new Phrase(val?.ToString() ?? "", cellFont));
            doc.Add(table);
            doc.Close();
        }

        private static readonly Dictionary<string, string> HeaderTranslations = new Dictionary<string, string>
        {
            // Клиенты
            ["id"] = "ID",
            ["last_name"] = "Фамилия",
            ["first_name"] = "Имя",
            ["middle_name"] = "Отчество",
            ["phone_number"] = "Телефон",
            ["gender_name"] = "Пол",
            ["email"] = "Email",
            ["card_number"] = "Номер карты",
            ["card_balance"] = "Баланс карты",

            // Услуги
            ["mservice_name"] = "Услуга",
            ["mservice_icd"] = "Код МКБ",
            ["mservice_price"] = "Цена",
            ["mservice_description"] = "Описание",
            ["stype_name"] = "Тип",
            ["extra_info"] = "Дополнительно",

            // Архив заказов
            ["id_order"] = "Номер заказа",
            ["id_client"] = "ID клиента",
            ["status_name"] = "Статус",
            ["total"] = "Итого"
        };

    }
}
