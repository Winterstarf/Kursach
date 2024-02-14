using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

namespace MainApp.windows
{
    /// <summary>
    /// Interaction logic for HADDrugsAddWindow.xaml
    /// </summary>
    public partial class HADDrugsAddWindow : Window
    {
        BigBoarsEntities db_cont = new BigBoarsEntities();
        public int[] DrugIds = new int[10];
        public string[] DrugDozes = new string[10];
        public string[] DrugFormats = new string[10];
        NewDrugsData n = new NewDrugsData();
        public HADDrugsAddWindow()
        {
            InitializeComponent();

            this.DataContext = n;

            var drugs = db_cont.Drugs.ToList();
            n.DrugOptions = drugs;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            PropertyInfo[] p = typeof(NewDrugsData).GetProperties();
            for (int i = 0; i < DrugIds.Length; i++)
            {
                Drugs selectedDrug = (Drugs)p[i].GetValue(n);
                if (selectedDrug != null)
                {
                    DrugIds[i] = selectedDrug.id;
                    TextBox dozeTextBox = (TextBox)this.FindName($"Doze{i + 1}_tb");
                    TextBox formatTextBox = (TextBox)this.FindName($"Format{i + 1}_tb");
                    if (dozeTextBox != null && formatTextBox != null &&
                        !string.IsNullOrEmpty(dozeTextBox.Text) && !string.IsNullOrEmpty(formatTextBox.Text))
                    {
                        DrugDozes[i] = dozeTextBox.Text;
                        DrugFormats[i] = formatTextBox.Text;
                    }
                    else
                    {
                        DrugDozes[i] = string.Empty;
                        DrugFormats[i] = string.Empty;
                    }
                }
                else
                {
                    DrugIds[i] = 0;
                    DrugDozes[i] = string.Empty;
                    DrugFormats[i] = string.Empty;
                }
            }

            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }

    public class NewDrugsData
    {
        public Drugs SelectedDrug1 { get; set; }
        public Drugs SelectedDrug2 { get; set; }
        public Drugs SelectedDrug3 { get; set; }
        public Drugs SelectedDrug4 { get; set; }
        public Drugs SelectedDrug5 { get; set; }
        public Drugs SelectedDrug6 { get; set; }
        public Drugs SelectedDrug7 { get; set; }
        public Drugs SelectedDrug8 { get; set; }
        public Drugs SelectedDrug9 { get; set; }
        public Drugs SelectedDrug10 { get; set; }
        public List<Drugs> DrugOptions { get; set; }
    }
}
