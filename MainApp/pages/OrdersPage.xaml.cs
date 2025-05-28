using MainApp.assets.models;
using MainApp.windows.main;
using System;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainApp.pages
{
    /// <summary>
    /// Interaction logic for OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        internal HelixDBEntities db_cont = new HelixDBEntities();

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        public List<OrderDisplay> SearchOrders(string searchText)
        {
            string[] searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredServices = db_cont.clients_services
                .Where(cs => cs.id_status == 1) // filtering by completed only
                .ToList();

            var groupedOrders = filteredServices
                .GroupBy(cs => cs.id_order)
                .Select(g =>
                {
                    var firstService = g.FirstOrDefault();
                    if (firstService == null) return null;

                    var client = firstService.clients;
                    var status = db_cont.statuses.FirstOrDefault(s => s.id == firstService.id_status);

                    return new OrderDisplay
                    {
                        OrderId = g.Key,
                        ClientFullName = $"{client.last_name} {client.first_name} {client.middle_name}".Trim(),
                        StatusName = status != null ? status.status_name : "Неизвестный статус",
                        TotalPrice = g.Sum(cs => cs.medical_services.mservice_price),
                        Services = g.ToList()
                    };
                })
                .Where(o => o != null)
                .ToList();

            if (searchWords.Length == 0)
            {
                return groupedOrders;
            }

            return groupedOrders
                .Where(order => searchWords.All(word =>
                    order.OrderId.ToString().ToLower().Contains(word.ToLower()) ||
                    (order.ClientFullName != null && order.ClientFullName.ToLower().Contains(word.ToLower()))))
                .ToList();
        }

        public void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = Search_tb.Text.Trim();
            DG_Orders.ItemsSource = SearchOrders(searchText);
        }

        private void Del_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Orders.SelectedItem == null)
            {
                MessageBox.Show("Не выбран заказ для удаления");
                return;
            }

            var selectedOrder = DG_Orders.SelectedItem as OrderDisplay;
            if (selectedOrder != null)
            {
                MessageBoxResult res = MessageBox.Show("Подтвердите удаление архивного заказа", "Удаление заказа", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    var orderServices = db_cont.clients_services.Where(cs => cs.id_order == selectedOrder.OrderId).ToList();
                    foreach (var service in orderServices)
                    {
                        db_cont.clients_services.DeleteObject(service);
                    }
                    db_cont.SaveChanges();
                    LoadOrders();
                }
            }
        }

        private void Refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void DG_Orders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DG_Orders.SelectedItem = null;
        }

        private void LoadOrders()
        {
            var orders = db_cont.clients_services
                .Where(cs => cs.id_status == 1) //means completed
                .GroupBy(cs => cs.id_order)
                .ToList()
                .Select(g =>
                {
                    var firstService = g.FirstOrDefault();
                    if (firstService == null) return null;

                    var client = firstService.clients;
                    var status = db_cont.statuses.FirstOrDefault(s => s.id == firstService.id_status);

                    return new OrderDisplay
                    {
                        OrderId = g.Key,
                        ClientFullName = $"{client.last_name} {client.first_name} {client.middle_name}".Trim(),
                        StatusName = status != null ? status.status_name : "Неизвестный статус",
                        TotalPrice = g.Sum(cs => cs.medical_services.mservice_price),
                        Services = g.ToList()
                    };
                })
                .Where(o => o != null)
                .ToList();

            DG_Orders.ItemsSource = orders;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.IsDoctor || App.IsLaborant)
            {
                Del_btn.IsEnabled = false;
            }
        }
    }

    public class OrderDisplay
    {
        public int OrderId { get; set; }
        public string ClientFullName { get; set; }
        public string StatusName { get; set; }
        public double TotalPrice { get; set; }
        public List<clients_services> Services { get; set; }
    }
}
