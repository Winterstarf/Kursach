using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MainApp.windows.adds
{
    /// <summary>
    /// Interaction logic for ClientsAddWindow.xaml
    /// </summary>
    public partial class ClientsAddWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();

        public ClientsAddWindow()
        {
            InitializeComponent();

            var newClientData = new NewClientData();
            this.DataContext = newClientData;

            var genders = db_cont.genders.ToList();
            newClientData.GenderOptions = genders;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newClientData = (NewClientData)this.DataContext;

                if (LastName_tb.Text == string.Empty || FirstName_tb.Text == string.Empty || Gender_cb.SelectedItem == null
                    || BirthDate_dp.SelectedDate == null || Phone_tb.Text == string.Empty || Email_tb.Text == string.Empty
                    || Passport_tb.Text == string.Empty || Passport_tb.Text.Length != 10)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неверный тип данных");
                }

                int newcard = Convert.ToInt32(db_cont.clients.Max(x => x.card_number)) + 1;
                var newClient = new clients
                {
                    last_name = newClientData.LastName,
                    first_name = newClientData.FirstName,
                    middle_name = newClientData.MiddleName,
                    id_gender = newClientData.SelectedGender.id,
                    birth_date = (DateTime)BirthDate_dp.SelectedDate,
                    phone_number = newClientData.Phone,
                    email = newClientData.Email,
                    passport = newClientData.Passport,
                    card_number = newcard,
                    card_balance = 0
                };
                db_cont.clients.AddObject(newClient);
                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
    }

    public class NewClientData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public genders SelectedGender { get; set; }
        public List<genders> GenderOptions { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Passport { get; set; }
    }
}
