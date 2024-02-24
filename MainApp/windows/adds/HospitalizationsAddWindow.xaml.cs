using MainApp.assets.models;
using System;
using System.Collections.Generic;
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

                if ((DateTime)HospDate_dp.SelectedDate < DateTime.UtcNow || (DateTime)HospDate_dp.SelectedDate > (DateTime)PlannedDehospDate_dp.SelectedDate || (DateTime)PlannedDehospDate_dp.SelectedDate < DateTime.UtcNow || (DateTime)HospDate_dp.SelectedDate == (DateTime)PlannedDehospDate_dp.SelectedDate)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неправильный тип данных!");
                }

                if (App_cb.SelectedItem == null || Goal_tb.Text == string.Empty || Blocks_cb.SelectedItem == null || Status_сb.SelectedItem == null || Bed_tb.Text == string.Empty || !uint.TryParse(Bed_tb.Text, out uint bedRes) || bedRes == 0 || bedRes > 999 || HospDate_dp.SelectedDate == null || PlannedDehospDate_dp.SelectedDate == null)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неправильный тип данных!");
                }

                if (Price_tb.Text == string.Empty)
                {
                    newHospData.Price = Convert.ToString(0);
                }
                else if (!float.TryParse(Price_tb.Text, out float priceRes) || priceRes < 0)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неправильный тип данных!");
                }

                int tc = Convert.ToInt32(db_cont.Hospitalizations.Max(p => p.TherapistCode)) + 1;
                var newHosp = new Hospitalizations
                {
                    idMedApp = newHospData.SelectedApp.id,
                    HospGoal = newHospData.Goal,
                    idHospBlock = newHospData.SelectedBlock.id,
                    HospPrice = Convert.ToDouble(newHospData.Price),
                    idHospStatus = newHospData.SelectedStatus.id,
                    HospDate = (DateTime)HospDate_dp.SelectedDate,
                    PlannedDehospDate = (DateTime)PlannedDehospDate_dp.SelectedDate,
                    BedNumber = newHospData.Bed,
                    TherapistCode = Convert.ToString(tc)
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
        public string Bed { get; set; }
    }
}
