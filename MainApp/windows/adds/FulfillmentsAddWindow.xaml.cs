using MainApp.assets.models;
using System.Globalization;
using System;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace MainApp.windows.adds
{
    /// <summary>
    /// Interaction logic for FulfillmentsAddWindow.xaml
    /// </summary>
    public partial class FulfillmentsAddWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        private NewFulfillmentData newFulfillmentData;

        public FulfillmentsAddWindow()
        {
            InitializeComponent();

            newFulfillmentData = new NewFulfillmentData();
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
                // Удаляем последний выбранный элемент, если количество превышает 5
                foreach (var item in e.AddedItems)
                {
                    Services_lb.SelectedItems.Remove(item);
                }

                MessageBox.Show("Вы не можете выбрать более 5 услуг");
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
                .Where(s => s.mservice_name.ToLower().Contains(query))
                .ToList();
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newFulfillmentData = (NewFulfillmentData)this.DataContext;

                if (Client_сb.SelectedItem == null || !newFulfillmentData.SelectedServices.Any()
                    || Status_cb.SelectedItem == null || Staff_сb.SelectedItem == null)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неверный тип данных");
                }

                foreach (var selectedService in newFulfillmentData.SelectedServices)
                {
                    var newFulfillment = new clients_services
                    {
                        id_client = newFulfillmentData.SelectedClient.id,
                        id_service = selectedService.id,
                        date_asked = (DateTime)DatePaid_dp.SelectedDate,
                        date_made = DateMade_dp.SelectedDate,
                        id_status = newFulfillmentData.SelectedStatus.id,
                        id_staff = newFulfillmentData.SelectedStaff.id
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
