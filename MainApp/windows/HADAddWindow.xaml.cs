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
    /// Interaction logic for HADAddWindow.xaml
    /// </summary>
    public partial class HADAddWindow : Window
    {
        readonly BigBoarsEntities db_cont = new BigBoarsEntities();
        readonly HADDrugsAddWindow drugs;
        readonly HADProcsAddWindow procs;
        readonly HADDocsAddWindow doctors;

        public HADAddWindow()
        {
            InitializeComponent();

            var newHADData = new NewHADData();
            this.DataContext = newHADData;

            drugs = new HADDrugsAddWindow();
            procs = new HADProcsAddWindow();
            doctors = new HADDocsAddWindow();

            var apps = db_cont.MedAppointments.ToList();
            newHADData.AppOptions = apps;
            var docs = db_cont.Doctors.ToList();
            newHADData.DoctorOptions = docs;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newHADData = (NewHADData)this.DataContext;

                if (App_cb.SelectedItem == null || Anamnez_tb.Text == string.Empty || Symptomatics_tb.Text == string.Empty 
                    || Recs_tb.Text == string.Empty || Doctor_сb.SelectedItem == null)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неправильный тип данных!");
                }

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
                int newIdHAD = newHAD.id;

                for (int i = 0; i < 10; i++)
                {
                    if (drugs.DrugIds[i] != 0)
                    {
                        var newHADDrug = new HAD_Drugs
                        {
                            idHAD = newIdHAD,
                            idDrug = drugs.DrugIds[i],
                            Doze = drugs.DrugDozes[i],
                            ConsumingFormat = drugs.DrugFormats[i]
                        };
                        db_cont.HAD_Drugs.AddObject(newHADDrug);
                        db_cont.SaveChanges();
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    if (procs.ProcIds[i] != 0)
                    {
                        var newHADProc = new HAD_Procedures
                        {
                            idHAD = newIdHAD,
                            idProc = procs.ProcIds[i]
                        };
                        db_cont.HAD_Procedures.AddObject(newHADProc);
                        db_cont.SaveChanges();
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    if (doctors.DocIds[i] != 0)
                    {
                        var newHADDoc = new HAD_NextDoctors
                        {
                            idHAD = newIdHAD,
                            idDoctor = doctors.DocIds[i],
                            ExtraInfo = doctors.Infos[i]
                        };
                        db_cont.HAD_NextDoctors.AddObject(newHADDoc);
                        db_cont.SaveChanges();
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void App_cb_DropDownClosed(object sender, EventArgs e)
        {
            var newHADData = (NewHADData)this.DataContext;
            if (((ComboBox)sender).SelectedItem != null)
            {
                var q = from app in db_cont.MedAppointments
                        where app.id == newHADData.SelectedApp.id
                        select app.Medcards.InsurancePolicies.Passports.Patients.LastName + " " + app.Medcards.InsurancePolicies.Passports.Patients.FirstName + " с диагнозом: " + app.Diagnose;

                CurrentPat_tbk.Text = $"Текущий пациент: {q.Single()}";
            }
        }

        private void DrugsAdd_btn_Click(object sender, RoutedEventArgs e)
        {
            drugs.ShowDialog();
        }

        private void ProcsAdd_btn_Click(object sender, RoutedEventArgs e)
        {
            procs.ShowDialog();
        }

        private void DocsAdd_btn_Click(object sender, RoutedEventArgs e)
        {
            doctors.ShowDialog();
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
