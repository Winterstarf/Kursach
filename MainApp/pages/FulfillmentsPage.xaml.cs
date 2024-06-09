using MainApp.assets.models;
using MainApp.windows.adds;
using MainApp.windows.edits;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainApp.pages
{
    /// <summary>
    /// Interaction logic for FulfillmentsPage.xaml
    /// </summary>
    public partial class FulfillmentsPage : Page
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();

        public FulfillmentsPage()
        {
            InitializeComponent();

            DG_Fulfillments.ItemsSource = db_cont.clients_services.ToList();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = Search_tb.Text.Trim();

            if (string.IsNullOrEmpty(searchText)) DG_Fulfillments.ItemsSource = db_cont.clients_services.ToList();
            else
            {
                if (int.TryParse(searchText, out int fulfillmentId))
                {
                    DG_Fulfillments.ItemsSource = db_cont.clients_services.ToList()
                    .Where(x =>
                        x.id == fulfillmentId)
                    .ToList();
                }
            }
        }

        private void Del_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Fulfillments.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для удаления!");
                return;
            }
            else
            {
                var selectedData = (dynamic)DG_Fulfillments.SelectedItem;
                clients_services selectedRow = selectedData;

                MessageBoxResult res = MessageBox.Show("Подтвердите удаление", "Удаление строки", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    db_cont.DeleteObject(selectedRow);
                    db_cont.SaveChanges();

                    DG_Fulfillments.ItemsSource = db_cont.clients_services.ToList();
                }
            }
        }

        private void Add_btn_Click(object sender, RoutedEventArgs e)
        {
            var window = new FulfillmentsAddWindow();
            window.ShowDialog();

            DG_Fulfillments.ItemsSource = db_cont.clients_services.ToList();
        }

        private void Refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            DG_Fulfillments.ItemsSource = db_cont.clients_services.ToList();
        }

        private void Update_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Fulfillments.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для изменения!");
                return;
            }
            else
            {
                var selectedData = (clients_services)DG_Fulfillments.SelectedItem;
                FulfillmentsEditWindow few = new FulfillmentsEditWindow(selectedData);

                few.ShowDialog();

                DG_Fulfillments.ItemsSource = db_cont.clients_services.ToList();
            }
        }

        private void DG_Clients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DG_Fulfillments.SelectedItem = null;
        }
    }
}
