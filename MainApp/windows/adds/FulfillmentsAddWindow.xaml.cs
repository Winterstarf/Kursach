using MainApp.assets.models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using MainApp.windows.edits;
using MainApp.windows.main;
using System.Windows.Documents;
using System.Threading.Tasks;

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
            newFulfillmentData.SelectedStatus = statuses.FirstOrDefault(st => st.id == 3);

            var staff = db_cont.staff.ToList();
            newFulfillmentData.StaffOptions = staff;
            newFulfillmentData.SelectedStaff = staff.FirstOrDefault(s => s.id == App.CurrentUserId);
            Staff_сb.SelectedItem = newFulfillmentData.SelectedStaff;

            DatePaid_dp.SelectedDate = DateTime.Now;
        }

        private void Services_lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Services_lb.SelectedItems != null)
            {
                var selectedItems = Services_lb.SelectedItems.Cast<medical_services>().ToList();
                if (selectedItems.Count > 10)
                {
                    // Remove the last selected item if the count exceeds 10
                    foreach (var item in e.AddedItems)
                    {
                        Services_lb.SelectedItems.Remove(item);
                    }

                    MessageBox.Show("Нельзя выбрать более чем 10 услуг");
                }
                else
                {
                    newFulfillmentData.SelectedServices = selectedItems;
                }
            }
            else newFulfillmentData.SelectedServices = null;
        }

        private void SearchService_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchService_tb.Text.ToLower();

            // Preserve the currently selected services
            var selectedServices = newFulfillmentData.SelectedServices ?? new List<medical_services>();

            // Filter services based on the search query
            var filteredServices = newFulfillmentData.ServiceOptions
                .Where(s => (s.mservice_name?.ToLower().Contains(query) ?? false) || (s.mservice_icd?.ToLower().Contains(query) ?? false))
                .ToList();

            // Ensure selected services remain in the filtered list
            foreach (var selectedService in selectedServices)
            {
                if (!filteredServices.Contains(selectedService))
                {
                    filteredServices.Add(selectedService);
                }
            }

            // Update the filtered service options
            newFulfillmentData.FilteredServiceOptions = filteredServices;

            // Restore the selection in the ListBox
            foreach (var selectedService in selectedServices)
            {
                if (!Services_lb.SelectedItems.Contains(selectedService))
                {
                    Services_lb.SelectedItems.Add(selectedService);
                }
            }
        }

        private async void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newFulfillmentData = (NewFulfillmentData)this.DataContext;

                // Проверка обязательных полей
                if (newFulfillmentData.SelectedClient == null)
                {
                    await App.ShowPopup("Клиент не выбран", ValidationPopup, PopupText);
                    return;
                }

                if (newFulfillmentData.SelectedStatus == null)
                {
                    await App.ShowPopup("Статус не выбран", ValidationPopup, PopupText);
                    return;
                }

                if (newFulfillmentData.SelectedStaff == null)
                {
                    await App.ShowPopup("Ответственный за выполнение не выбран", ValidationPopup, PopupText);
                    return;
                }

                // Проверка id выбранного сотрудника
                var allowedStaffIds = new HashSet<int> { 1, 2, 5, 6, 7, 8, 9, 10, 11 };
                if (!allowedStaffIds.Contains(newFulfillmentData.SelectedStaff.id))
                {
                    await App.ShowPopup("Выбранный ответственный не входит в список допустимых", ValidationPopup, PopupText);
                    return;
                }

                // Статус и дата выполнения
                if (newFulfillmentData.SelectedStatus.id != 1 && DateMade_dp.SelectedDate != null)
                {
                    DateMade_dp.SelectedDate = null;
                    await App.ShowPopup("Нельзя указывать дату выполнения при статусе Выполняется или Отменено", ValidationPopup, PopupText);
                    return;
                }

                // Проверка даты оплаты
                if (DatePaid_dp.SelectedDate == null)
                {
                    await App.ShowPopup("Дата оплаты не была выбрана", ValidationPopup, PopupText);
                    return;
                }

                if (DatePaid_dp.SelectedDate > DateTime.Today)
                {
                    await App.ShowPopup("Дата оплаты не может быть позже сегодняшнего дня", ValidationPopup, PopupText);
                    return;
                }

                // Проверка выбранных услуг
                if (newFulfillmentData.SelectedServices == null || newFulfillmentData.SelectedServices.Count == 0)
                {
                    await App.ShowPopup("Не выбрано ни одной медицинской услуги", ValidationPopup, PopupText);
                    return;
                }

                if (newFulfillmentData.SelectedServices.Count > 10)
                {
                    await App.ShowPopup("Нельзя выбрать более 10 услуг", ValidationPopup, PopupText);
                    return;
                }

                // Генерация id заказа
                var newOrderId = db_cont.clients_services.Any()
                    ? db_cont.clients_services.Max(cs => cs.id_order) + 1
                    : 1;

                DateTime selDate = (DateTime)DatePaid_dp.SelectedDate;
                DateTime combinedDateTime = selDate + DateTime.Now.TimeOfDay;
                decimal total = (decimal)newFulfillmentData.SelectedServices.Sum(s => s.mservice_price);
                bool isFirst = true;

                foreach (var selectedService in newFulfillmentData.SelectedServices)
                {
                    var newFulfillment = new clients_services
                    {
                        id_client = newFulfillmentData.SelectedClient.id,
                        id_service = selectedService.id,
                        date_asked = combinedDateTime,
                        date_made = DateMade_dp.SelectedDate,
                        id_status = newFulfillmentData.SelectedStatus.id,
                        id_staff = newFulfillmentData.SelectedStaff.id,
                        id_order = newOrderId,
                        total_price = isFirst ? total : (decimal?)null
                    };
                    db_cont.clients_services.AddObject(newFulfillment);
                    isFirst = false;
                }

                db_cont.SaveChanges();
                this.Close();
            }
            catch (Exception ex)
            {
                await App.ShowPopup($"Ошибка: {ex.Message}", ValidationPopup, PopupText);
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
