using MainApp.assets.models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using MainApp.windows.main;
using MainApp.windows.adds;
using DocumentFormat.OpenXml.Drawing.Charts;
using MainApp.windows.edits;
using Order = MainApp.windows.main.Order;

namespace MainApp.windows.edits
{
    public partial class FulfillmentsEditWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        readonly private EditFulfillmentData editFulfillmentData;
        private readonly int orderId;

        public FulfillmentsEditWindow(Order selectedOrder)
        {
            InitializeComponent();

            orderId = selectedOrder.OrderId;

            editFulfillmentData = new EditFulfillmentData();
            this.DataContext = editFulfillmentData;

            var clients = db_cont.clients.ToList();
            editFulfillmentData.ClientOptions = clients;

            var medical_services = db_cont.medical_services.ToList();
            editFulfillmentData.ServiceOptions = medical_services;
            editFulfillmentData.FilteredServiceOptions = medical_services;

            var statuses = db_cont.statuses.ToList();
            editFulfillmentData.StatusOptions = statuses;

            var staff = db_cont.staff.ToList();
            editFulfillmentData.StaffOptions = staff;

            LoadOrderDetails(selectedOrder);
        }

        private void LoadOrderDetails(Order selectedOrder)
        {
            var orderServices = selectedOrder.Services;

            // Extract the IDs needed for the query
            var serviceIds = orderServices.Select(os => os.id_service).ToList();
            var clientId = orderServices.First().id_client;
            var statusId = orderServices.First().id_status;
            var staffId = orderServices.First().id_staff;

            // Retrieve the related entities using their IDs
            editFulfillmentData.SelectedClient = db_cont.clients.FirstOrDefault(c => c.id == clientId);
            editFulfillmentData.SelectedServices = db_cont.medical_services.Where(ms => serviceIds.Contains(ms.id)).ToList();
            editFulfillmentData.SelectedStatus = db_cont.statuses.FirstOrDefault(s => s.id == statusId);
            editFulfillmentData.SelectedStaff = db_cont.staff.FirstOrDefault(st => st.id == staffId);

            // Set the date fields using properties
            editFulfillmentData.SelectedDatePaid = orderServices.First().date_asked;
            editFulfillmentData.SelectedDateMade = orderServices.First().date_made;

            DatePaid_dp.SelectedDate = orderServices.First().date_asked;
            var matched_services = orderServices.FirstOrDefault(sms => sms.id_status == 1 && sms.date_made != null);
            if (matched_services != null) DateMade_dp.SelectedDate = matched_services.date_made;

            // Update the Services ListBox
            Services_lb.SelectedItems.Clear();
            foreach (var service in editFulfillmentData.SelectedServices)
            {
                Services_lb.SelectedItems.Add(service);
            }

            if (editFulfillmentData.SelectedStatus.id == 1)
            {
                Services_lb.IsEnabled = false;
                SearchService_tb.IsEnabled = false;
                DateMade_dp.IsEnabled = false;
            }

            if (editFulfillmentData.SelectedStatus.id == 3)
            {
                Services_lb.IsEnabled = false;
                SearchService_tb.IsEnabled = false;
            }
        }

        private void Services_lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = Services_lb.SelectedItems.Cast<medical_services>().ToList();
            if (selectedItems.Count > 10)
            {
                // Remove the last selected item if the count exceeds 
                foreach (var item in e.AddedItems)
                {
                    Services_lb.SelectedItems.Remove(item);
                }

                MessageBox.Show("Нельзя выбрать более чем 10 услуг");
            }
            else
            {
                editFulfillmentData.SelectedServices = selectedItems;
            }
        }

        private void SearchService_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchService_tb.Text.ToLower();

            // Preserve selected services
            var selectedServices = editFulfillmentData.SelectedServices;

            // Filter services based on the search query
            editFulfillmentData.FilteredServiceOptions = editFulfillmentData.ServiceOptions
                .Where(s => (s.mservice_name?.ToLower().Contains(query) ?? false) || (s.mservice_icd?.ToLower().Contains(query) ?? false)
                            || selectedServices.Contains(s)) // Keep selected services in the filtered list
                .ToList();

            // Reapply selected services in the ListBox
            foreach (var service in selectedServices)
            {
                if (!Services_lb.SelectedItems.Contains(service))
                {
                    Services_lb.SelectedItems.Add(service);
                }
            }
        }

        public void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editFulfillmentData = (EditFulfillmentData)this.DataContext;
                var existingServices = db_cont.clients_services.Where(cs => cs.id_order == orderId).ToList();

                var originalStatus = existingServices.First().id_status;
                var newStatus = editFulfillmentData.SelectedStatus.id;

                if (editFulfillmentData.SelectedClient == null || editFulfillmentData.SelectedStatus == null || editFulfillmentData.SelectedStaff == null)
                {
                    throw new Exception("Некоторые поля не заполнены или заполнены неверными данными");
                }

                if (newStatus != 1 && originalStatus != 1 && DateMade_dp.SelectedDate != null)
                {
                    throw new Exception("Нельзя указывать дату выполнения при статусе Выполняется или Отменено");
                }

                if (DatePaid_dp.SelectedDate != null && DatePaid_dp.SelectedDate > DateTime.Today)
                {
                    throw new Exception("Дата оплаты не может быть позже сегодняшнего дня");
                }

                if (!editFulfillmentData.SelectedServices.Any() || Services_lb.SelectedItems == null)
                {
                    throw new Exception("Не выбрано ни одной медицинской услуги");
                }

                double totalPrice = editFulfillmentData.SelectedServices.Sum(s => s.mservice_price);
                double bonus = Math.Round(totalPrice * 0.15, 2);

                bool applyBonus = false;
                bool revokeBonus = false;

                loyalty_transactions loyaltyTx = null;
                var client = editFulfillmentData.SelectedClient;

                if (originalStatus == 1 && newStatus == 3)
                {
                    throw new Exception("Невозможно изменить статус на В процессе, так как заказ уже выполнен.");
                }

                if (originalStatus == 3 && newStatus == 2)
                {
                    var consent = MessageBox.Show("Вы отменяете заказ досрочно, баллы не начисляются и деньги не возвращаются. Продолжить?", "Подтверждение отмены досрочно", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (consent != MessageBoxResult.Yes)
                    {
                        editFulfillmentData.SelectedStatus = db_cont.statuses.First(s => s.id == originalStatus);
                        return;
                    }

                    loyaltyTx = null;
                    foreach (var existingService in existingServices)
                    {
                        existingService.date_cancelled = DateTime.Now;
                        existingService.date_made = null;
                    }
                }

                if (originalStatus == 1 && newStatus == 2)
                {
                    throw new Exception("Невозможно отменить заказ досрочно, так как заказ уже выполнен.");
                }

                if (originalStatus == 1 && newStatus == 5)
                {
                    var consent = MessageBox.Show("Вы отменяете заказ с возвратом, баллы будут списаны и деньги возвращены. Продолжить?", "Подтверждение отмены с возвратом", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (consent != MessageBoxResult.Yes)
                    {
                        editFulfillmentData.SelectedStatus = db_cont.statuses.First(s => s.id == originalStatus);
                        return;
                    }

                    revokeBonus = true;
                    client.card_balance = (client.card_balance ?? 0) - bonus;

                    loyaltyTx = new loyalty_transactions
                    {
                        client_id = client.id,
                        datetime = DateTime.Now,
                        action_type = 2,
                        amount = -bonus,
                        balance_after = client.card_balance ?? 0,
                        x_source = $"Отмена с возвратом заказа #{orderId}"
                    };

                    foreach (var existingService in existingServices)
                    {
                        existingService.date_cancelled = DateTime.Now;
                    }
                }

                if (originalStatus == 3 && newStatus == 1)
                {
                    var consent = MessageBox.Show("Вы подтверждаете выполнение заказа. Будут начислены баллы. Продолжить?", "Подтверждение выполнения", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (consent != MessageBoxResult.Yes)
                    {
                        editFulfillmentData.SelectedStatus = db_cont.statuses.First(s => s.id == originalStatus);
                        return;
                    }

                    applyBonus = true;
                    client.card_balance = (client.card_balance ?? 0) + bonus;

                    loyaltyTx = new loyalty_transactions
                    {
                        client_id = client.id,
                        datetime = DateTime.Now,
                        action_type = 1,
                        amount = bonus,
                        balance_after = client.card_balance ?? 0,
                        x_source = $"Зачисление баллов по выполнению заказа #{orderId}"
                    };
                }

                if (originalStatus == 1 && newStatus == 1)
                {
                    foreach (var existingService in existingServices)
                    {
                        if (existingService.date_made == null)
                        {
                            if (DateMade_dp.SelectedDate == null)
                                throw new Exception("Не выбрана дата выполнения для статуса Выполнено");

                            existingService.date_made = DateMade_dp.SelectedDate?.Add(DateTime.Now.TimeOfDay);
                        }

                        existingService.date_cancelled = null;
                    }
                }

                if (originalStatus == 3 && newStatus == 5)
                {
                    throw new Exception("Невозможно отменить заказ с возвратом, так как заказ еще в процессе.");
                }

                // Переносим дату выполнения из существующей записи, если заказ уже был выполнен
                DateTime? combinedDateTime = null;
                if (originalStatus == 1)
                {
                    combinedDateTime = existingServices.FirstOrDefault()?.date_made;
                }
                else if (newStatus == 1)
                {
                    // Впервые устанавливаем дату выполнения
                    if (DateMade_dp.SelectedDate == null)
                        throw new Exception("Не выбрана дата выполнения для нового выполнения заказа");

                    combinedDateTime = DateMade_dp.SelectedDate?.Add(DateTime.Now.TimeOfDay);
                }

                foreach (var existingService in existingServices)
                {
                    db_cont.clients_services.DeleteObject(existingService);
                }

                foreach (var selectedService in editFulfillmentData.SelectedServices)
                {
                    db_cont.clients_services.AddObject(new clients_services
                    {
                        id_client = client.id,
                        id_service = selectedService.id,
                        id_order = orderId,
                        date_asked = (DateTime)editFulfillmentData.SelectedDatePaid,
                        date_made = combinedDateTime,
                        id_status = newStatus,
                        id_staff = editFulfillmentData.SelectedStaff.id,
                        date_cancelled = (newStatus == 2 || newStatus == 5) ? DateTime.Now : (DateTime?)null
                    });
                }

                if (applyBonus || revokeBonus)
                {
                    db_cont.loyalty_transactions.AddObject(loyaltyTx);
                }

                db_cont.SaveChanges();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }

    public class EditFulfillmentData : INotifyPropertyChanged
    {
        private List<clients> clientOptions;
        private clients selectedClient;
        private List<medical_services> serviceOptions;
        private List<medical_services> filteredServiceOptions;
        private List<medical_services> selectedServices;
        private List<statuses> statusOptions;
        private statuses selectedStatus;
        private List<staff> staffOptions;
        private staff selectedStaff;
        private DateTime? selectedDatePaid;
        private DateTime? selectedDateMade;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<clients> ClientOptions
        {
            get { return clientOptions; }
            set { clientOptions = value; OnPropertyChanged(nameof(ClientOptions)); }
        }

        public clients SelectedClient
        {
            get { return selectedClient; }
            set
            {
                selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                OnPropertyChanged(nameof(SelectedClientFullName));
            }
        }

        public string SelectedClientFullName
        {
            get { return selectedClient != null ? $"{selectedClient.last_name} {selectedClient.first_name} {selectedClient.middle_name}" : string.Empty; }
        }

        public List<medical_services> ServiceOptions
        {
            get { return serviceOptions; }
            set { serviceOptions = value; OnPropertyChanged(nameof(ServiceOptions)); }
        }

        public List<medical_services> FilteredServiceOptions
        {
            get { return filteredServiceOptions; }
            set { filteredServiceOptions = value; OnPropertyChanged(nameof(FilteredServiceOptions)); }
        }

        public List<medical_services> SelectedServices
        {
            get { return selectedServices; }
            set { selectedServices = value; OnPropertyChanged(nameof(SelectedServices)); }
        }

        public List<statuses> StatusOptions
        {
            get { return statusOptions; }
            set { statusOptions = value; OnPropertyChanged(nameof(StatusOptions)); }
        }

        public statuses SelectedStatus
        {
            get { return selectedStatus; }
            set { selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        public List<staff> StaffOptions
        {
            get { return staffOptions; }
            set { staffOptions = value; OnPropertyChanged(nameof(StaffOptions)); }
        }

        public staff SelectedStaff
        {
            get { return selectedStaff; }
            set { selectedStaff = value; OnPropertyChanged(nameof(SelectedStaff)); }
        }

        public DateTime? SelectedDatePaid
        {
            get { return selectedDatePaid; }
            set { selectedDatePaid = value; OnPropertyChanged(nameof(SelectedDatePaid)); }
        }

        public DateTime? SelectedDateMade
        {
            get { return selectedDateMade; }
            set { selectedDateMade = value; OnPropertyChanged(nameof(SelectedDateMade)); }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
