using MainApp.assets.models;
using System.Globalization;
using System;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;

namespace MainApp.windows.adds
{
    /// <summary>
    /// Interaction logic for FulfillmentsAddWindow.xaml
    /// </summary>
    public partial class FulfillmentsAddWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();

        public FulfillmentsAddWindow()
        {
            InitializeComponent();

            var newFulfillmentData = new NewFulfillmentData();
            this.DataContext = newFulfillmentData;

            var clients = db_cont.clients.ToList();
            newFulfillmentData.ClientOptions = clients;
            var medical_services = db_cont.medical_services.ToList();
            newFulfillmentData.ServiceOptions = medical_services;
            var statuses = db_cont.statuses.ToList();
            newFulfillmentData.StatusOptions = statuses;
            var staff = db_cont.staff.ToList();
            newFulfillmentData.StaffOptions = staff;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newFulfillmentData = (NewFulfillmentData)this.DataContext;

                if (Client_сb.SelectedItem == null || Service_сb.SelectedItem == null
                    || Status_cb.SelectedItem == null || Staff_сb.SelectedItem == null)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неверный тип данных");
                }

                var newFulfillment = new clients_services
                {
                    id_client = newFulfillmentData.SelectedClient.id,
                    id_service = newFulfillmentData.SelectedService.id,
                    date_asked = (DateTime)DatePaid_dp.SelectedDate,
                    date_made = (DateTime)DateMade_dp.SelectedDate,
                    id_status = newFulfillmentData.SelectedStatus.id,
                    id_staff = newFulfillmentData.SelectedStaff.id
                };
                db_cont.clients_services.AddObject(newFulfillment);
                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
    }

    public class NewFulfillmentData
    {
        public clients SelectedClient { get; set; }
        public List<clients> ClientOptions { get; set; }
        public medical_services SelectedService { get; set; }
        public List<medical_services> ServiceOptions { get; set; }
        public statuses SelectedStatus { get; set; }
        public List<statuses> StatusOptions { get; set; }
        public staff SelectedStaff { get; set; }
        public List<staff> StaffOptions { get; set; }
    }
}
