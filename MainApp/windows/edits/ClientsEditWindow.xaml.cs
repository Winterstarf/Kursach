using MainApp.assets.models;
using System;
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
using System.Windows.Shapes;

namespace MainApp.windows.edits
{
    /// <summary>
    /// Interaction logic for ClientsEditWindow.xaml
    /// </summary>
    public partial class ClientsEditWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        private clients _selectedClient;

        public ClientsEditWindow(clients client)
        {
            InitializeComponent();

            _selectedClient = client;

            var newClientData = new NewClientData
            {
                LastName = _selectedClient.last_name,
                FirstName = _selectedClient.first_name,
                MiddleName = _selectedClient.middle_name,
                Phone = _selectedClient.phone_number,
                Email = _selectedClient.email,
                Passport = _selectedClient.passport,
                GenderOptions = db_cont.genders.ToList(),
                SelectedGender = db_cont.genders.FirstOrDefault(g => g.id == _selectedClient.id_gender)
            };

            this.DataContext = newClientData;
            BirthDate_dp.SelectedDate = _selectedClient.birth_date;
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

                // Обновление существующего клиента
                _selectedClient.last_name = newClientData.LastName;
                _selectedClient.first_name = newClientData.FirstName;
                _selectedClient.middle_name = newClientData.MiddleName;
                _selectedClient.id_gender = newClientData.SelectedGender.id;
                _selectedClient.birth_date = (DateTime)BirthDate_dp.SelectedDate;
                _selectedClient.phone_number = newClientData.Phone;
                _selectedClient.email = newClientData.Email;
                _selectedClient.passport = newClientData.Passport;

                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
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
}
