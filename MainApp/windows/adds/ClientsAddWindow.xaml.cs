using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MainApp.windows.adds
{
    public partial class ClientsAddWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        private NewClientData newClientData;

        public ClientsAddWindow()
        {
            InitializeComponent();

            newClientData = new NewClientData();
            this.DataContext = newClientData;

            var genders = db_cont.genders.ToList();
            newClientData.GenderOptions = genders;
        }

        private async void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? birthDate = BirthDate_dp.SelectedDate;
                bool consentGiven = Consent_cb.IsChecked == true;

                // Consent check
                if (!consentGiven)
                {
                    await App.ShowPopup("Необходимо согласие на обработку данных", ValidationPopup, PopupText);
                    return;
                }

                // Required fields
                if (string.IsNullOrWhiteSpace(newClientData.LastName) || string.IsNullOrWhiteSpace(newClientData.FirstName))
                {
                    await App.ShowPopup("Имя и фамилия обязательны", ValidationPopup, PopupText);
                    return;
                }

                // Passport: only digits, exactly 10
                if (string.IsNullOrWhiteSpace(newClientData.Passport) || !Regex.IsMatch(newClientData.Passport, @"^\d{10}$"))
                {
                    await App.ShowPopup("Паспорт должен содержать только 10 цифр", ValidationPopup, PopupText);
                    return;
                }

                // Phone: international format +10–15 digits
                if (string.IsNullOrWhiteSpace(newClientData.Phone) || !Regex.IsMatch(newClientData.Phone, @"^\+\d{10,15}$"))
                {
                    await App.ShowPopup("Номер телефона должен быть в формате +1234567890", ValidationPopup, PopupText);
                    return;
                }

                // Email: must contain @ and valid domain part
                if (string.IsNullOrWhiteSpace(newClientData.Email) || !Regex.IsMatch(newClientData.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    await App.ShowPopup("Некорректный email", ValidationPopup, PopupText);
                    return;
                }

                // Birthdate: must not be null or in the future/current year
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

                bool passportExists = db_cont.clients.Any(c => c.passport == newClientData.Passport);
                if (passportExists)
                {
                    await App.ShowPopup("Клиент с таким паспортом уже существует", ValidationPopup, PopupText);
                    return;
                }

                // Generate card number
                int newcard = Convert.ToInt32(db_cont.clients.Max(x => x.card_number)) + 1;

                var newClient = new clients
                {
                    last_name = newClientData.LastName,
                    first_name = newClientData.FirstName,
                    middle_name = newClientData.MiddleName,
                    id_gender = newClientData.SelectedGender?.id ?? 0,
                    birth_date = birthDate.Value,
                    phone_number = newClientData.Phone,
                    email = newClientData.Email,
                    passport = newClientData.Passport,
                    card_number = newcard,
                    card_balance = 0
                };

                db_cont.clients.AddObject(newClient);
                db_cont.SaveChanges();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                await App.ShowPopup($"Ошибка: {ex.Message}", ValidationPopup, PopupText);
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
