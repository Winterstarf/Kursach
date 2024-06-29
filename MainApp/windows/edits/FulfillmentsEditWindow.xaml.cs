using MainApp.assets.models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using MainApp.windows.main;
using MainApp.windows.adds;

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

            // Update the Services ListBox
            Services_lb.SelectedItems.Clear();
            foreach (var service in editFulfillmentData.SelectedServices)
            {
                Services_lb.SelectedItems.Add(service);
            }
        }

        private void Services_lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = Services_lb.SelectedItems.Cast<medical_services>().ToList();
            if (selectedItems.Count > 5)
            {
                // Remove the last selected item if the count exceeds 5
                foreach (var item in e.AddedItems)
                {
                    Services_lb.SelectedItems.Remove(item);
                }

                MessageBox.Show("Нельзя выбрать более чем 5 услуг");
            }
            else
            {
                editFulfillmentData.SelectedServices = selectedItems;
            }
        }

        private void SearchService_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchService_tb.Text.ToLower();
            editFulfillmentData.FilteredServiceOptions = editFulfillmentData.ServiceOptions
                .Where(s => (s.mservice_name?.ToLower().Contains(query) ?? false) || (s.mservice_icd?.ToLower().Contains(query) ?? false))
                .ToList();
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editFulfillmentData = (EditFulfillmentData)this.DataContext;

                if (editFulfillmentData.SelectedClient == null || !editFulfillmentData.SelectedServices.Any()
                    || editFulfillmentData.SelectedStatus == null || editFulfillmentData.SelectedStaff == null || DatePaid_dp.SelectedDate == null)
                {
                    throw new Exception("Некоторые поля не заполнены или заполнены неверными данными");
                }

                if (editFulfillmentData.SelectedStatus.id != 2 && DateMade_dp.SelectedDate != null)
                {
                    DateMade_dp.SelectedDate = null;
                    throw new Exception("Нельзя указывать дату выполнения при статусе Выполняется или Отменено");
                }

                // Delete existing services for the order
                var existingServices = db_cont.clients_services.Where(cs => cs.id_order == orderId).ToList();
                foreach (var existingService in existingServices)
                {
                    db_cont.clients_services.DeleteObject(existingService);
                }
                db_cont.SaveChanges();

                // Add updated services for the order
                foreach (var selectedService in editFulfillmentData.SelectedServices)
                {
                    clients_services newService = new clients_services
                    {
                        id_client = editFulfillmentData.SelectedClient.id,
                        id_service = selectedService.id,
                        id_order = orderId,
                        date_asked = DatePaid_dp.SelectedDate.Value,
                        date_made = DateMade_dp.SelectedDate,
                        id_status = editFulfillmentData.SelectedStatus.id,
                        id_staff = editFulfillmentData.SelectedStaff.id
                    };
                    db_cont.clients_services.AddObject(newService);
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
