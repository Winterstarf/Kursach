using MainApp.assets.models;
using MainApp.windows.adds;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            if (string.IsNullOrEmpty(searchText)) DG_Services.ItemsSource = db_cont.medical_services.ToList();
            else
            {
                DG_Services.ItemsSource = db_cont.medical_services.ToList()
                    .Where(x =>
                        x.mservice_name.Contains(searchText))
                    .ToList();
            }
        }

        private void Del_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Services.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для удаления!");
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
        }
    }
}
