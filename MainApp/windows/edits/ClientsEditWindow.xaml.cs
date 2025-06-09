using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MainApp.windows.edits
{
    public partial class ClientsEditWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        readonly private clients _selectedClient;
        private NewClientData newClientData;

        public ClientsEditWindow(clients client)
        {
            InitializeComponent();

            _selectedClient = client;

            newClientData = new NewClientData
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

        private async void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? birthDate = BirthDate_dp.SelectedDate;

                // Валидации
                if (string.IsNullOrWhiteSpace(newClientData.LastName) || string.IsNullOrWhiteSpace(newClientData.FirstName))
                {
                    await App.ShowPopup("Имя и фамилия обязательны", ValidationPopup, PopupText);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newClientData.Passport) || !Regex.IsMatch(newClientData.Passport, @"^\d{10}$"))
                {
                    await App.ShowPopup("Паспорт должен содержать только 10 цифр", ValidationPopup, PopupText);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newClientData.Phone) || !Regex.IsMatch(newClientData.Phone, @"^\+\d{10,15}$"))
                {
                    await App.ShowPopup("Номер телефона должен быть в формате +1234567890", ValidationPopup, PopupText);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newClientData.Email) || !Regex.IsMatch(newClientData.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    await App.ShowPopup("Некорректный email", ValidationPopup, PopupText);
                    return;
                }

                if (birthDate == null)
                {
                    await App.ShowPopup("Укажите дату рождения", ValidationPopup, PopupText);
                    return;
                }

                if (birthDate.Value.Year >= DateTime.Now.Year || birthDate > DateTime.Now)
                {
                    await App.ShowPopup("Дата рождения некорректна (только до текущего года)", ValidationPopup, PopupText);
                    return;
                }

                bool passportExists = db_cont.clients.Any(c => c.passport == newClientData.Passport && c.id != _selectedClient.id);
                if (passportExists)
                {
                    await App.ShowPopup("Клиент с таким паспортом уже существует", ValidationPopup, PopupText);
                    return;
                }

                // Обновление клиента
                _selectedClient.last_name = newClientData.LastName;
                _selectedClient.first_name = newClientData.FirstName;
                _selectedClient.middle_name = newClientData.MiddleName;
                _selectedClient.id_gender = newClientData.SelectedGender.id;
                _selectedClient.birth_date = birthDate.Value;
                _selectedClient.phone_number = newClientData.Phone;
                _selectedClient.email = newClientData.Email;
                _selectedClient.passport = newClientData.Passport;

                db_cont.SaveChanges();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                await App.ShowPopup($"Ошибка: {ex.Message}", ValidationPopup, PopupText);
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
