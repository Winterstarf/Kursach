using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MainApp.windows
{
    /// <summary>
    /// Interaction logic for HADDocsAddWindow.xaml
    /// </summary>
    public partial class HADDocsAddWindow : Window
    {
        readonly BigBoarsEntities db_cont = new BigBoarsEntities();
        readonly NewDocData n = new NewDocData();
        readonly public int[] DocIds = new int[3];
        readonly public string[] Infos = new string[3];
        
        public HADDocsAddWindow()
        {
            InitializeComponent();

            this.DataContext = n;

            var docs = db_cont.Doctors.ToList();
            n.DoctorOptions = docs;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PropertyInfo[] p = typeof(NewDocData).GetProperties();

                for (int i = 0; i < DocIds.Length; i++)
                {
                    Doctors selectedDoc = (Doctors)p[i].GetValue(n);
                    if (selectedDoc != null)
                    {
                        DocIds[i] = selectedDoc.id;
                        TextBox infoTextBox = (TextBox)this.FindName($"Info{i + 1}_tb");

                        if (infoTextBox != null && !string.IsNullOrEmpty(infoTextBox.Text)) Infos[i] = infoTextBox.Text;
                        else
                        {
                            Infos[i] = string.Empty;
                            throw new Exception("Некоторые обязательные поля не указаны или содержат неправильный тип данных!");
                        } 
                    }
                    else
                    {
                        DocIds[i] = 0;
                        Infos[i] = string.Empty;
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }

    public class NewDocData
    {
        public Doctors SelectedDoctor1 { get; set; }
        public Doctors SelectedDoctor2 { get; set; }
        public Doctors SelectedDoctor3 { get; set; }
        public List<Doctors> DoctorOptions { get; set; }
    }
}
