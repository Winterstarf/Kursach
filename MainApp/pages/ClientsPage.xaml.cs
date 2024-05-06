using MainApp.assets.models;
using MainApp.windows.adds;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MainApp.pages
{
    /// <summary>
    /// Interaction logic for ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();

        public ClientsPage()
        {
            InitializeComponent();

            DG_Clients.ItemsSource = db_cont.clients.ToList();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = Search_tb.Text.Trim();

            if (string.IsNullOrEmpty(searchText)) DG_Clients.ItemsSource = db_cont.clients.ToList();
            else
            {
                DG_Clients.ItemsSource = db_cont.clients.ToList()
                    .Where(x =>
                        x.last_name.Contains(searchText) ||
                        x.first_name.Contains(searchText) ||
                        x.middle_name.Contains(searchText) ||
                        x.phone_number.Contains(searchText))
                    .ToList();
            }
        }

        private void Del_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Clients.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для удаления!");
                return;
            }
            else
            {
                var selectedData = (dynamic)DG_Clients.SelectedItem;
                clients selectedRow = selectedData;

                MessageBoxResult res = MessageBox.Show("Подтвердите удаление", "Удаление строки", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    db_cont.DeleteObject(selectedRow);
                    db_cont.SaveChanges();

                    DG_Clients.ItemsSource = db_cont.clients.ToList();
                }
            }
        }

        private void Add_btn_Click(object sender, RoutedEventArgs e)
        {
            var window = new ClientsAddWindow();
            window.ShowDialog();

            DG_Clients.ItemsSource = db_cont.clients.ToList();
        }

        private void Refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            DG_Clients.ItemsSource = db_cont.clients.ToList();
        }
    }
}
