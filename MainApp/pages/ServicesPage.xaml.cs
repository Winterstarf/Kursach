using MainApp;
using MainApp.assets.models;
using MainApp.windows.adds;
using MainApp.windows.edits;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainApp.pages
{
    /// <summary>
    /// Interaction logic for ServicesPage.xaml
    /// </summary>
    public partial class ServicesPage : Page
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();

        public ServicesPage()
        {
            InitializeComponent();
            DG_Services.ItemsSource = db_cont.medical_services.ToList();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = Search_tb.Text.Trim();
            string[] searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (searchWords.Length == 0)
            {
                DG_Services.ItemsSource = db_cont.medical_services.ToList();
            }
            else
            {
                DG_Services.ItemsSource = db_cont.medical_services.ToList()
                    .Where(x =>
                        searchWords.All(word =>
                            (x.mservice_name != null && x.mservice_name.ToLower().Contains(word.ToLower())) ||
                            (x.mservice_icd != null && x.mservice_icd.ToLower().Contains(word.ToLower()))
                        )
                    ).ToList();
            }
        }

        private void Del_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Services.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для удаления");
                return;
            }
            else
            {
                var selectedData = (dynamic)DG_Services.SelectedItem;
                medical_services selectedRow = selectedData;

                MessageBoxResult res = MessageBox.Show("Подтвердите удаление", "Удаление строки", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    db_cont.DeleteObject(selectedRow);
                    db_cont.SaveChanges();

                    DG_Services.ItemsSource = db_cont.medical_services.ToList();
                }
            }
        }

        private void Add_btn_Click(object sender, RoutedEventArgs e)
        {
            var window = new ServicesAddWindow();
            window.ShowDialog();

            DG_Services.ItemsSource = db_cont.medical_services.ToList();
        }

        private void Refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            DG_Services.ItemsSource = db_cont.medical_services.ToList();
            DG_Services.SelectedItem = null;
        }

        private void Update_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Services.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для изменения");
                return;
            }
            else
            {
                var selectedData = (medical_services)DG_Services.SelectedItem; // Преобразование напрямую в medical_services
                ServicesEditWindow sew = new ServicesEditWindow(selectedData); // Передача услуги в конструктор

                sew.ShowDialog();

                DG_Services.ItemsSource = db_cont.medical_services.ToList();
            }
        }

        private void DG_Clients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DG_Services.SelectedItem = null;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.IsDoctor || App.IsLaborant)
            {
                Update_btn.IsEnabled = false;
                Del_btn.IsEnabled = false;
                Add_btn.IsEnabled = false;
            }
        }
    }
}
