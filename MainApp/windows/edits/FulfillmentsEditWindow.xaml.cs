using MainApp.assets.models;
using MainApp.windows.adds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MainApp.windows.edits
{
    /// <summary>
    /// Interaction logic for FulfillmentsEditWindow.xaml
    /// </summary>
    public partial class FulfillmentsEditWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        private clients_services _selectedFulfillment;

        public FulfillmentsEditWindow(clients_services selectedFulfillment)
        {
            InitializeComponent();

            _selectedFulfillment = selectedFulfillment;

            var newFulfillmentData = new NewFulfillmentData();

            // Заполнение данными из переданного объекта
            newFulfillmentData.SelectedClient = db_cont.clients.FirstOrDefault(c => c.id == _selectedFulfillment.id_client);
            newFulfillmentData.SelectedService = db_cont.medical_services.FirstOrDefault(m => m.id == _selectedFulfillment.id_service);
            newFulfillmentData.SelectedStatus = db_cont.statuses.FirstOrDefault(ss => ss.id == _selectedFulfillment.id_status);
            newFulfillmentData.SelectedStaff = db_cont.staff.FirstOrDefault(st => st.id == _selectedFulfillment.id_staff);
            newFulfillmentData.DateAsked = selectedFulfillment.date_asked.Date;
            newFulfillmentData.DateMade = selectedFulfillment.date_made ?? DateTime.MinValue;

            // Устанавливаем значения по умолчанию для DatePicker
            DatePaid_dp.SelectedDate = newFulfillmentData.DateAsked;
            DateMade_dp.SelectedDate = newFulfillmentData.DateMade;

            // Заполнение списков
            newFulfillmentData.ClientOptions = db_cont.clients.ToList();
            newFulfillmentData.ServiceOptions = db_cont.medical_services.ToList();
            newFulfillmentData.StatusOptions = db_cont.statuses.ToList();
            newFulfillmentData.StaffOptions = db_cont.staff.ToList();

            // Инициализация фильтрованного списка услуг
            newFulfillmentData.FilteredServiceOptions = new List<medical_services>(newFulfillmentData.ServiceOptions);

            // Обработка ранее выбранных услуг
            var previouslySelectedServices = new List<medical_services>
            {
                newFulfillmentData.SelectedService
            };

            // Если услуг больше 5, берем только первые 5
            previouslySelectedServices = previouslySelectedServices.Take(5).ToList();

            // Устанавливаем выбранные услуги
            newFulfillmentData.SelectedServices = previouslySelectedServices;

            // Перемещаем выбранные услуги наверх списка
            newFulfillmentData.FilteredServiceOptions = previouslySelectedServices
                .Concat(newFulfillmentData.ServiceOptions.Except(previouslySelectedServices))
                .ToList();

            this.DataContext = newFulfillmentData;

            // Устанавливаем выбранные элементы в ListBox
            foreach (var service in previouslySelectedServices)
            {
                Services_lb.SelectedItems.Add(service);
            }

            Services_lb.Loaded += (s, e) =>
            {
                foreach (var service in previouslySelectedServices)
                {
                    var listBoxItem = (ListBoxItem)Services_lb.ItemContainerGenerator.ContainerFromItem(service);
                    if (listBoxItem != null)
                    {
                        listBoxItem.Style = (Style)FindResource("PreviouslySelectedStyle");
                    }
                }
            };

            Services_lb.SelectionChanged += (s, e) =>
            {
                foreach (var service in e.AddedItems)
                {
                    var listBoxItem = (ListBoxItem)Services_lb.ItemContainerGenerator.ContainerFromItem(service);
                    if (listBoxItem != null)
                    {
                        listBoxItem.Style = (Style)FindResource("PreviouslySelectedStyle");
                    }
                }

                foreach (var service in e.RemovedItems)
                {
                    var listBoxItem = (ListBoxItem)Services_lb.ItemContainerGenerator.ContainerFromItem(service);
                    if (listBoxItem != null)
                    {
                        listBoxItem.Style = null;
                    }
                }
            };
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newFulfillmentData = (NewFulfillmentData)this.DataContext;

                if (Client_сb.SelectedItem == null || Services_lb.SelectedItems.Count == 0
                    || Status_cb.SelectedItem == null || Staff_сb.SelectedItem == null)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неверный тип данных");
                }

                // Обновление данных выбранного объекта
                _selectedFulfillment.id_client = newFulfillmentData.SelectedClient.id;
                _selectedFulfillment.id_service = newFulfillmentData.SelectedService.id;
                _selectedFulfillment.date_asked = newFulfillmentData.DateAsked;
                _selectedFulfillment.date_made = newFulfillmentData.DateMade;
                _selectedFulfillment.id_status = newFulfillmentData.SelectedStatus.id;
                _selectedFulfillment.id_staff = newFulfillmentData.SelectedStaff.id;

                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void SearchService_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var newFulfillmentData = (NewFulfillmentData)this.DataContext;
            var searchText = SearchService_tb.Text.ToLower();

            newFulfillmentData.FilteredServiceOptions = newFulfillmentData.ServiceOptions
                .Where(s => s.mservice_name.ToLower().Contains(searchText))
                .ToList();

            Services_lb.ItemsSource = newFulfillmentData.FilteredServiceOptions;
        }

        private void Services_lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Services_lb.SelectedItems.Count > 5)
            {
                // Удаляем последний добавленный элемент, если количество выбранных превышает 5
                var lastAddedItem = e.AddedItems[e.AddedItems.Count - 1];
                Services_lb.SelectedItems.Remove(lastAddedItem);
                MessageBox.Show("Вы не можете выбрать более 5 услуг.");
            }
            else
            {
                var newFulfillmentData = (NewFulfillmentData)this.DataContext;
                newFulfillmentData.SelectedServices = Services_lb.SelectedItems.Cast<medical_services>().ToList();
            }
        }
    }

    public class NewFulfillmentData
    {
        public clients SelectedClient { get; set; }
        public List<clients> ClientOptions { get; set; }
        public medical_services SelectedService { get; set; }
        public List<medical_services> ServiceOptions { get; set; }
        public List<medical_services> FilteredServiceOptions { get; set; }
        public statuses SelectedStatus { get; set; }
        public List<statuses> StatusOptions { get; set; }
        public staff SelectedStaff { get; set; }
        public List<staff> StaffOptions { get; set; }
        public DateTime DateAsked { get; set; }
        public DateTime DateMade { get; set; }
        public List<medical_services> SelectedServices { get; set; }
    }
}
