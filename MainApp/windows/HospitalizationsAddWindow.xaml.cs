using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MainApp.windows
{
    /// <summary>
    /// Interaction logic for HospitalizationsAddWindow.xaml
    /// </summary>
    public partial class HospitalizationsAddWindow : Window
    {
        readonly BigBoarsEntities db_cont = new BigBoarsEntities();
        readonly CultureInfo us = new CultureInfo("en-US");

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
            try
            {
                var newHospData = (NewHospitalizationData)this.DataContext;

                if (App_cb.SelectedItem == null || Goal_tb.Text == string.Empty || Price_tb.Text == string.Empty || !float.TryParse(Price_tb.Text, out float priceRes)
                    || priceRes < 0 || Blocks_cb.SelectedItem == null || Status_сb.SelectedItem == null || TherapistCode_tb.Text == string.Empty || !uint.TryParse(TherapistCode_tb.Text, out uint therapistCodeRes)
                    || therapistCodeRes == 0 || therapistCodeRes > 999999 || Bed_tb.Text == string.Empty || !uint.TryParse(Bed_tb.Text, out uint bedRes) || bedRes == 0 || bedRes > 999
                    || HospDate_tb.Text == string.Empty || !DateTime.TryParseExact(HospDate_tb.Text, "yyyy-MM-dd", us, DateTimeStyles.None, out DateTime hospDateRes)
                    || PlannedDehospDate_tb.Text == string.Empty || !DateTime.TryParseExact(PlannedDehospDate_tb.Text, "yyyy-MM-dd", us, DateTimeStyles.None, out DateTime plannedDehospDateRes))
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неправильный тип данных!");
                }

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
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void App_cb_DropDownClosed(object sender, EventArgs e)
        {
            var newHospitalizationData = (NewHospitalizationData)this.DataContext;
            if (((ComboBox)sender).SelectedItem != null)
            {
                var q = from hosp in db_cont.MedAppointments
                        where hosp.id == newHospitalizationData.SelectedApp.id
                        select hosp.Medcards.InsurancePolicies.Passports.Patients.LastName + " " + hosp.Medcards.InsurancePolicies.Passports.Patients.FirstName + " с диагнозом: " + hosp.Diagnose;

                CurrentPat_tbk.Text = $"Текущий пациент: {q.Single()}";
            }
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
