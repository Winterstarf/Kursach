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
    /// Interaction logic for HADAddWindow.xaml
    /// </summary>
    public partial class HADAddWindow : Window
    {
        BigBoarsEntities db_cont = new BigBoarsEntities();
        public HADAddWindow()
        {
            InitializeComponent();

            var newHADData = new NewHADData();
            this.DataContext = newHADData;

            var apps = db_cont.MedAppointments.ToList();
            newHADData.AppOptions = apps;
            var docs = db_cont.Doctors.ToList();
            newHADData.DoctorOptions = docs;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            var newHADData = (NewHADData)this.DataContext;
            CultureInfo us = new CultureInfo("en-US");

            var newHAD = new HealingAndDiagnostics
            {
                idMedApp = newHADData.SelectedApp.id,
                AnamnezHarvest = newHADData.Anamnez,
                Symptomatics = newHADData.Symptomatics,
                DoctorRecommendations = newHADData.Recs,
                idDoctor = newHADData.SelectedDoctor.id
            };
            db_cont.HealingAndDiagnostics.AddObject(newHAD);
            db_cont.SaveChanges();
            this.Close();
        }
    }

    public class NewHADData
    {
        public MedAppointments SelectedApp { get; set; }
        public List<MedAppointments> AppOptions { get; set; }
        public string Anamnez { get; set; }
        public string Symptomatics { get; set; }
        public string Recs { get; set; }
        public Doctors SelectedDoctor { get; set; }
        public List<Doctors> DoctorOptions { get; set; }
    }
}
