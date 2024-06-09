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
    /// Interaction logic for ServicesEditWindow.xaml
    /// </summary>
    public partial class ServicesEditWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        private medical_services _selectedService;

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

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newServiceData = (NewServiceData)this.DataContext;

                if (Name_tb.Text == string.Empty || Description_tb.Text == string.Empty || Types_cb.SelectedItem == null
                || Price_tb.Text == string.Empty)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неверный тип данных");
                }

                // Обновление существующей услуги
                _selectedService.mservice_name = newServiceData.Name;
                _selectedService.mservice_description = newServiceData.Description;
                _selectedService.mservice_icd = newServiceData.ICD;
                _selectedService.mservice_price = Convert.ToDouble(newServiceData.Price);
                _selectedService.extra_info = newServiceData.Extra;
                _selectedService.id_type = newServiceData.SelectedType.id;

                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
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
