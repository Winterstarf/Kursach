using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MainApp.windows.adds
{
    public partial class ServicesAddWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();

        public ServicesAddWindow()
        {
            InitializeComponent();

            var newServiceData = new NewServiceData();
            this.DataContext = newServiceData;

            newServiceData.TypeOptions = db_cont.service_types.ToList();
        }

        private async void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newServiceData = (NewServiceData)this.DataContext;

                string name = Name_tb.Text.Trim();
                string description = Description_tb.Text.Trim();
                string price = Price_tb.Text.Trim();
                string icd = ICD_tb.Text.Trim();
                string extra = Extra_tb.Text.Trim();

                // Название и описание обязательны
                if (string.IsNullOrWhiteSpace(name))
                {
                    await App.ShowPopup("Название услуги обязательно", ValidationPopup, PopupText);
                    return;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    await App.ShowPopup("Описание услуги обязательно", ValidationPopup, PopupText);
                    return;
                }

                // Тип услуги обязателен
                if (newServiceData.SelectedType == null)
                {
                    await App.ShowPopup("Выберите тип услуги", ValidationPopup, PopupText);
                    return;
                }

                // Цена обязательна и должна быть числом
                if (!double.TryParse(price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedPrice))
                {
                    await App.ShowPopup("Введите корректную цену (например, 1200.50 или 800,75)", ValidationPopup, PopupText);
                    return;
                }

                // ICD — если заполнен, проверяем формат (буква + 2 цифры + точка + 1 цифра)
                if (!string.IsNullOrWhiteSpace(icd) && !Regex.IsMatch(icd, @"^[A-ZА-Я]{1}\d{2}\.\d$"))
                {
                    await App.ShowPopup("Код ICD должен быть в формате A00.0", ValidationPopup, PopupText);
                    return;
                }

                var newService = new medical_services
                {
                    mservice_name = name,
                    mservice_description = description,
                    mservice_icd = string.IsNullOrWhiteSpace(icd) ? null : icd,
                    mservice_price = parsedPrice,
                    extra_info = extra,
                    id_type = newServiceData.SelectedType.id
                };

                db_cont.medical_services.AddObject(newService);
                db_cont.SaveChanges();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                await App.ShowPopup($"Ошибка при сохранении: {ex.Message}", ValidationPopup, PopupText);
            }
        }
    }

    public class NewServiceData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ICD { get; set; }
        public service_types SelectedType { get; set; }
        public List<service_types> TypeOptions { get; set; }
        public string Price { get; set; }
        public string Extra { get; set; }
    }
}
