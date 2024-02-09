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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainApp.pages
{
    /// <summary>
    /// Interaction logic for HospitalizationsPage.xaml
    /// </summary>
    public partial class HospitalizationsPage : Page
    {
        BigBoarsEntities db_cont = new BigBoarsEntities();
        public HospitalizationsPage()
        {
            InitializeComponent();
            DG_Hosps.ItemsSource = db_cont.Hospitalizations.ToList();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = Search_tb.Text.Trim();

            if (string.IsNullOrEmpty(searchText)) DG_Hosps.ItemsSource = db_cont.Hospitalizations.ToList();
            else
            {
                DG_Hosps.ItemsSource = db_cont.Hospitalizations
                    .Where(hosps =>
                        hosps.MedAppointments.Medcards.MedcardNumber.Contains(searchText) ||
                        hosps.MedAppointments.Medcards.InsurancePolicies.Passports.Patients.LastName.Contains(searchText) ||
                        hosps.MedAppointments.Medcards.InsurancePolicies.Passports.Patients.FirstName.Contains(searchText) ||
                        hosps.MedAppointments.Medcards.InsurancePolicies.Passports.Patients.MiddleName.Contains(searchText) ||
                        hosps.TherapistCode.Contains(searchText))
                    .ToList();
            }
        }
    }
}
