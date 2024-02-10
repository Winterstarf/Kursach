using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace MainApp.windows
{
    /// <summary>
    /// Interaction logic for HospitalizationsAddWindow.xaml
    /// </summary>
    public partial class HospitalizationsAddWindow : Window
    {
        BigBoarsEntities db_cont = new BigBoarsEntities();
        public HospitalizationsAddWindow()
        {
            InitializeComponent();

            var newHospitalizationData = new NewHospitalizationData();
            this.DataContext = newHospitalizationData;

            var apps = db_cont.MedAppointments.ToList();
            newHospitalizationData.AppOptions = apps;
            var blocks = db_cont.HospBlocks.ToList();
            newHospitalizationData.BlockOptions = blocks;
            var statuses = db_cont.HospStatus.ToList();
            newHospitalizationData.StatusOptions = statuses;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            var newHospData = (NewHospitalizationData)this.DataContext;
            CultureInfo us = new CultureInfo("en-US");

            var newHosp = new Hospitalizations
            {
                idMedApp = newHospData.SelectedApp.id,
                HospGoal = newHospData.Goal,
                idHospBlock = newHospData.SelectedBlock.id,
                HospPrice = Convert.ToDouble(newHospData.Price),
                idHospStatus = newHospData.SelectedStatus.id,
                HospDate = DateTime.ParseExact(newHospData.HospDate, "yyyy-MM-dd", us),
                PlannedDehospDate = DateTime.ParseExact(newHospData.PlannedDehospDate, "yyyy-MM-dd", us),
                BedNumber = newHospData.Bed,
                TherapistCode = newHospData.TherapistCode
            };
            db_cont.Hospitalizations.AddObject(newHosp);
            db_cont.SaveChanges();
            this.Close();
        }
    }

    public class NewHospitalizationData
    {
        public MedAppointments SelectedApp { get; set; }
        public List<MedAppointments> AppOptions { get; set; }
        public string Goal { get; set; }
        public string Price { get; set; }
        public HospBlocks SelectedBlock { get; set; }
        public List<HospBlocks> BlockOptions { get; set; }
        public HospStatus SelectedStatus { get; set; }
        public List<HospStatus> StatusOptions { get; set; }
        public string TherapistCode { get; set; }
        public string Bed { get; set; }
        public string HospDate { get; set; }
        public string PlannedDehospDate { get; set; }
    }
}
