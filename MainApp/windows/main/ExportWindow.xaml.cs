using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Data.SqlClient;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Dynamic;
using MainApp.classes;

namespace MainApp.windows.main
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
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
            int totalRows = await Exporter.LoadDataAsync(export_page_name, null).ContinueWith(t => t.Result.Count);
            TotalRow_tb.Text = totalRows.ToString();
        }

        private async void Export_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RowCount_tb.Text, out int rowCount) || rowCount <= 0)
            {
                MessageBox.Show("Введите корректное число строк");
                return;
            }
            string path = ShowSaveFileDialog();
            if (string.IsNullOrWhiteSpace(path)) return;
            await Exporter.ExportAsync(export_type, export_page_name, rowCount, path);
            Close();
        }

        private async void ExportAll_btn_Click(object sender, RoutedEventArgs e)
        {
            string path = ShowSaveFileDialog();
            if (string.IsNullOrWhiteSpace(path)) return;
            await Exporter.ExportAsync(export_type, export_page_name, null, path);
            Close();
        }

        private string ShowSaveFileDialog()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Экспорт_{export_page_name}_{(export_type == "pdf" ? "PDF" : "Excel")}_{(isExportAll ? "all" : RowCount_tb.Text)}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.{export_type}",
                Filter = export_type == "pdf" ? "PDF files (*.pdf)|*.pdf" : "Excel files (*.xlsx)|*.xlsx"
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }
}
