using MainApp.assets.models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace MainApp.windows.adds
{
    public partial class FulfillmentsAddWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        readonly private NewFulfillmentData newFulfillmentData;

        public FulfillmentsAddWindow(clients selectedClient)
        {
            InitializeComponent();

            newFulfillmentData = new NewFulfillmentData
            {
                SelectedClient = selectedClient
            };
            this.DataContext = newFulfillmentData;

            var clients = db_cont.clients.ToList();
            newFulfillmentData.ClientOptions = clients;
            var medical_services = db_cont.medical_services.ToList();
            newFulfillmentData.ServiceOptions = medical_services;
            newFulfillmentData.FilteredServiceOptions = medical_services;
            var statuses = db_cont.statuses.ToList();
            newFulfillmentData.StatusOptions = statuses;
            var staff = db_cont.staff.ToList();
            newFulfillmentData.StaffOptions = staff;
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
                newFulfillmentData.SelectedServices = selectedItems;
            }
        }

        private void SearchService_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchService_tb.Text.ToLower();
            newFulfillmentData.FilteredServiceOptions = newFulfillmentData.ServiceOptions
                .Where(s => (s.mservice_name?.ToLower().Contains(query) ?? false) || (s.mservice_icd?.ToLower().Contains(query) ?? false))
                .ToList();
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newFulfillmentData = (NewFulfillmentData)this.DataContext;

                if (newFulfillmentData.SelectedClient == null || !newFulfillmentData.SelectedServices.Any()
                    || newFulfillmentData.SelectedStatus == null || newFulfillmentData.SelectedStaff == null || DatePaid_dp.SelectedDate == null)
                {
                    throw new Exception("Некоторые поля не заполнены или заполнены неверными данными");
                }

                if (newFulfillmentData.SelectedStatus.id != 2 && DateMade_dp.SelectedDate != null)
                {
                    DateMade_dp.SelectedDate = null;
                    throw new Exception("Нельзя указывать дату выполнения при статусе Выполняется или Отменено");
                }

                var newOrderId = db_cont.clients_services.Max(cs => cs.id_order) + 1;

                foreach (var selectedService in newFulfillmentData.SelectedServices)
                {
                    var newFulfillment = new clients_services
                    {
                        id_client = newFulfillmentData.SelectedClient.id,
                        id_service = selectedService.id,
                        date_asked = (DateTime)DatePaid_dp.SelectedDate,
                        date_made = DateMade_dp.SelectedDate,
                        id_status = newFulfillmentData.SelectedStatus.id,
                        id_staff = newFulfillmentData.SelectedStaff.id,
                        id_order = newOrderId
                    };
                    db_cont.clients_services.AddObject(newFulfillment);
                }

                db_cont.SaveChanges();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
    }

    public class NewFulfillmentData : INotifyPropertyChanged
    {
        private List<clients> _clientOptions;
        private clients _selectedClient;
        private List<medical_services> _serviceOptions;
        private List<medical_services> _filteredServiceOptions;
        private List<medical_services> _selectedServices;
        private List<statuses> _statusOptions;
        private statuses _selectedStatus;
        private List<staff> _staffOptions;
        private staff _selectedStaff;

        public List<clients> ClientOptions
        {
            get => _clientOptions;
            set
            {
                _clientOptions = value;
                OnPropertyChanged(nameof(ClientOptions));
            }
        }

        public clients SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                OnPropertyChanged(nameof(SelectedClientFullName)); // Trigger update for display
            }
        }

        public string SelectedClientFullName
        {
            get
            {
                if (_selectedClient == null)
                {
                    return string.Empty;
                }
                return $"{_selectedClient.last_name} {_selectedClient.first_name} {_selectedClient.middle_name}";
            }
        }

        public List<medical_services> ServiceOptions
        {
            get => _serviceOptions;
            set
            {
                _serviceOptions = value;
                OnPropertyChanged(nameof(ServiceOptions));
            }
        }

        public List<medical_services> FilteredServiceOptions
        {
            get => _filteredServiceOptions;
            set
            {
                _filteredServiceOptions = value;
                OnPropertyChanged(nameof(FilteredServiceOptions));
            }
        }

        public List<medical_services> SelectedServices
        {
            get => _selectedServices;
            set
            {
                _selectedServices = value;
                OnPropertyChanged(nameof(SelectedServices));
            }
        }

        public List<statuses> StatusOptions
        {
            get => _statusOptions;
            set
            {
                _statusOptions = value;
                OnPropertyChanged(nameof(StatusOptions));
            }
        }

        public statuses SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }

        public List<staff> StaffOptions
        {
            get => _staffOptions;
            set
            {
                _staffOptions = value;
                OnPropertyChanged(nameof(StaffOptions));
            }
        }

        public staff SelectedStaff
        {
            get => _selectedStaff;
            set
            {
                _selectedStaff = value;
                OnPropertyChanged(nameof(SelectedStaff));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
