using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MainApp.windows.edits
{
    /// <summary>
    /// Interaction logic for ServicesEditWindow.xaml
    /// </summary>
    public partial class ServicesEditWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        readonly private medical_services _selectedService;

        public ServicesEditWindow(medical_services service)
        {
            InitializeComponent();

            _selectedService = service;

            var newServiceData = new NewServiceData
            {
                Name = _selectedService.mservice_name,
                Description = _selectedService.mservice_description,
                ICD = _selectedService.mservice_icd,
                Price = _selectedService.mservice_price.ToString(),
                Extra = _selectedService.extra_info,
                TypeOptions = db_cont.service_types.ToList(),
                SelectedType = db_cont.service_types.FirstOrDefault(t => t.id == _selectedService.id_type)
            };

            this.DataContext = newServiceData;
        }

        private async void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newServiceData = (NewServiceData)this.DataContext;

                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(newServiceData.Name))
                {
                    await App.ShowPopup("Название услуги обязательно для заполнения", ValidationPopup, PopupText);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newServiceData.Description))
                {
                    await App.ShowPopup("Описание услуги обязательно для заполнения", ValidationPopup, PopupText);
                    return;
                }

                if (newServiceData.SelectedType == null)
                {
                    await App.ShowPopup("Необходимо выбрать тип услуги", ValidationPopup, PopupText);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newServiceData.Price))
                {
                    await App.ShowPopup("Цена услуги обязательна для заполнения", ValidationPopup, PopupText);
                    return;
                }

                // Проверка формата цены (число с ',' или '.')
                if (!double.TryParse(newServiceData.Price.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedPrice))
                {
                    await App.ShowPopup("Введите корректную цену (например, 1200.50 или 800,75)", ValidationPopup, PopupText);
                    return;
                }

                // Если ICD заполнен — проверить формат (например, только цифры и буквы)
                if (!string.IsNullOrWhiteSpace(newServiceData.ICD))
                {
                    if (!Regex.IsMatch(newServiceData.ICD, @"^[A-Za-z0-9]+$"))
                    {
                        await App.ShowPopup("ICD может содержать только буквы и цифры", ValidationPopup, PopupText);
                        return;
                    }
                }

                // Обновление существующей услуги
                _selectedService.mservice_name = newServiceData.Name;
                _selectedService.mservice_description = newServiceData.Description;
                _selectedService.mservice_icd = newServiceData.ICD;
                _selectedService.mservice_price = parsedPrice;
                _selectedService.extra_info = newServiceData.Extra;
                _selectedService.id_type = newServiceData.SelectedType.id;

                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                await App.ShowPopup($"Ошибка: {ex.Message}", ValidationPopup, PopupText);
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
}
