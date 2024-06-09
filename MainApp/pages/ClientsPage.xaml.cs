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
            string[] searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (searchWords.Length == 0)
            {
                DG_Clients.ItemsSource = db_cont.clients.ToList();
            }
            else
            {
                DG_Clients.ItemsSource = db_cont.clients.ToList()
                    .Where(x =>
                        searchWords.All(word =>
                            x.last_name.ToLower().Contains(word.ToLower()) ||
                            x.first_name.ToLower().Contains(word.ToLower()) ||
                            x.middle_name.ToLower().Contains(word.ToLower()) ||
                            x.phone_number.Contains(word)))
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

        private void Update_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Clients.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для изменения!");
                return;
            }
            else
            {
                var selectedData = (clients)DG_Clients.SelectedItem;
                ClientsEditWindow cew = new ClientsEditWindow(selectedData);

                cew.ShowDialog();

                DG_Clients.ItemsSource = db_cont.clients.ToList();
            }
        }

        private void DG_Clients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DG_Clients.SelectedItem = null;
        }
    }
}
