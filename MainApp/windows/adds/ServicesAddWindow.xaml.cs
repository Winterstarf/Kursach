using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MainApp.windows.adds
{
    /// <summary>
    /// Interaction logic for ServicesAddWindow.xaml
    /// </summary>
    public partial class ServicesAddWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();

        public ServicesAddWindow()
        {
            InitializeComponent();

            var newServiceData = new NewServiceData();
            this.DataContext = newServiceData;

            var types = db_cont.service_types.ToList();
            newServiceData.TypeOptions = types;
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

                var newService = new medical_services
                {
                    mservice_name = newServiceData.Name,
                    mservice_description = newServiceData.Description,
                    mservice_icd = newServiceData.ICD,
                    mservice_price = Convert.ToDouble(newServiceData.Price),
                    extra_info = newServiceData.Extra,
                    id_type = newServiceData.SelectedType.id
                };
                db_cont.medical_services.AddObject(newService);
                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
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
