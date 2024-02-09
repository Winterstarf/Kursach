using MainApp.assets.models;
using System.Linq;
using System.Windows.Controls;

namespace MainApp.pages
{
    /// <summary>
    /// Interaction logic for HADPage.xaml
    /// </summary>
    public partial class HADPage : Page
    {
        BigBoarsEntities db_cont = new BigBoarsEntities();
        public HADPage()
        {
            InitializeComponent();

            DG_HADs.ItemsSource = db_cont.HealingAndDiagnostics.ToList();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = Search_tb.Text.Trim();

            if (string.IsNullOrEmpty(searchText)) DG_HADs.ItemsSource = db_cont.HealingAndDiagnostics.ToList();
            else
            {
                DG_HADs.ItemsSource = db_cont.HealingAndDiagnostics
                    .Where(hads =>
                        hads.MedAppointments.Medcards.MedcardNumber.Contains(searchText) ||
                        hads.MedAppointments.Medcards.InsurancePolicies.Passports.Patients.LastName.Contains(searchText) ||
                        hads.MedAppointments.Medcards.InsurancePolicies.Passports.Patients.FirstName.Contains(searchText) ||
                        hads.MedAppointments.Medcards.InsurancePolicies.Passports.Patients.MiddleName.Contains(searchText))
                    .ToList();
            }
        }
    }
}
