using MainApp.assets.models;
using MainApp.windows.adds;
using MainApp.windows.edits;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MainApp.windows.main
{
    public partial class MedcardWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        readonly private clients _selectedClient;

        public MedcardWindow(clients client)
        {
            InitializeComponent();

            _selectedClient = client;

            var ClientData = new ClientData
            {
                Clientid = _selectedClient.id.ToString(),
                Lastname = _selectedClient.last_name,
                Firstname = _selectedClient.first_name,
                Middlename = _selectedClient.middle_name,
                Gender = _selectedClient.genders.gender_name,
                Phone = _selectedClient.phone_number,
                Email = _selectedClient.email,
                Passport = _selectedClient.passport,
                Card_number = _selectedClient.card_number.ToString(),
                Card_balance = _selectedClient.card_balance.ToString()
            };
            this.DataContext = ClientData;

            LoadOrders();
            LoadStatuses();
        }

        private void LoadOrders()
        {
            // Retrieve orders for the selected client
            var ordersData = db_cont.clients_services
                               .Where(cs => cs.id_client == _selectedClient.id)
                               .GroupBy(cs => cs.id_order)
                               .ToList()
                               .Select(g => new Order
                               {
                                   OrderId = (int)g.Key,
                                   OrderSummary = $"Заказ {g.Key} - {g.Count()} услуг(и)",
                                   Services = g.ToList()
                               }).ToList();

            OrdersListBox.ItemsSource = ordersData;
        }

        private void LoadStatuses()
        {
            var statuses = db_cont.statuses.ToList();
            StatusComboBox.ItemsSource = statuses;
        }

        private void OrdersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersListBox.SelectedItem != null)
            {
                sepa_sep.Visibility = Visibility.Visible;

                var selectedOrder = OrdersListBox.SelectedItem as Order;
                var services = selectedOrder.Services.Select(s => new ServiceDetail
                {
                    mservice_name = s.medical_services.mservice_name,
                    mservice_icd = s.medical_services.mservice_icd,
                    mservice_description = s.medical_services.mservice_description,
                    mservice_price = (decimal)s.medical_services.mservice_price
                }).ToList();

                OrderDetailsItemsControl.ItemsSource = services;

                var total = services.Sum(s => s.mservice_price);
                TotalPriceTextBlock.Text = $"Итог: {total:C}";

                // Bind the selected status
                StatusComboBox.SelectedValue = selectedOrder.Status;
            }
            else
            {
                sepa_sep.Visibility = Visibility.Hidden;
                OrderDetailsItemsControl.ItemsSource = null;
                TotalPriceTextBlock.Text = string.Empty;

                StatusComboBox.SelectedValue = null; // Clear ComboBox selection
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersListBox.SelectedItem != null && StatusComboBox.SelectedValue != null)
            {
                var selectedOrder = OrdersListBox.SelectedItem as Order;
                var newStatusId = (int)StatusComboBox.SelectedValue;

                // Update the status in the database
                var orderToUpdate = db_cont.clients_services.FirstOrDefault(o => o.id == selectedOrder.OrderId);
                if (orderToUpdate != null)
                {
                    orderToUpdate.id_status = newStatusId;
                    db_cont.SaveChanges();
                }
            }
        }



        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var addOrderWindow = new FulfillmentsAddWindow(_selectedClient);
            addOrderWindow.ShowDialog();
            LoadOrders();
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedItem == null)
            {
                MessageBox.Show("Не выбран заказ для удаления");
                return;
            }

            var selectedOrder = OrdersListBox.SelectedItem as Order;
            if (selectedOrder != null)
            {
                MessageBoxResult res = MessageBox.Show("Подтвердите удаление", "Удаление заказа", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
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

        private void EditOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedItem == null)
            {
                MessageBox.Show("Не выбран заказ для изменения");
                return;
            }
            else
            {
                var selectedOrder = OrdersListBox.SelectedItem as Order;
                FulfillmentsEditWindow few = new FulfillmentsEditWindow(selectedOrder);
                few.ShowDialog();

                LoadOrders();
            }
        }

        private void OrdersListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OrdersListBox.SelectedItem = null;
        }
    }

    public class ClientData
    {
        public string Clientid { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Passport { get; set; }
        public string Card_number { get; set; }
        public string Card_balance { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public string OrderSummary { get; set; }
        public List<clients_services> Services { get; set; }
        public string Status { get; set; }
    }

    public class ServiceDetail
    {
        public string mservice_name { get; set; }
        public string mservice_icd { get; set; }
        public string mservice_description { get; set; }
        public decimal mservice_price { get; set; }
    }
}
