using MainApp.assets.models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MainApp.pages
{
    /// <summary>
    /// Interaction logic for PatientsPage.xaml
    /// </summary>
    public partial class PatientsPage : Page
    {
        BigBoarsEntities db_cont = new BigBoarsEntities();
        public PatientsPage()
        {
            InitializeComponent();

            var q = from medcard in db_cont.Medcards
                    join email in db_cont.Emails on medcard.InsurancePolicies.Passports.Patients.id equals email.idPatient into emailGroup
                    join phone in db_cont.Phones on medcard.InsurancePolicies.Passports.Patients.id equals phone.idPatient into phoneGroup
                    select new
                    {
                        Medcard = medcard,
                        Emails = emailGroup,
                        Phones = phoneGroup,
                    };

            DG_Patients.ItemsSource = q.ToList();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = Search_tb.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                var q = from medcard in db_cont.Medcards
                        join email in db_cont.Emails on medcard.InsurancePolicies.Passports.Patients.id equals email.idPatient into emailGroup
                        join phone in db_cont.Phones on medcard.InsurancePolicies.Passports.Patients.id equals phone.idPatient into phoneGroup
                        select new
                        {
                            Medcard = medcard,
                            Emails = emailGroup,
                            Phones = phoneGroup,
                        };

                DG_Patients.ItemsSource = q.ToList();
            }
            else
            {
                var q = from medcard in db_cont.Medcards
                        join email in db_cont.Emails on medcard.InsurancePolicies.Passports.Patients.id equals email.idPatient into emailGroup
                        join phone in db_cont.Phones on medcard.InsurancePolicies.Passports.Patients.id equals phone.idPatient into phoneGroup
                        select new
                        {
                            Medcard = medcard,
                            Emails = emailGroup,
                            Phones = phoneGroup,
                        };

                DG_Patients.ItemsSource = q
                    .Where(medcard =>
                        medcard.Medcard.InsurancePolicies.PolicyNumber.Contains(searchText) ||
                        medcard.Medcard.MedcardNumber.Contains(searchText) ||
                        medcard.Medcard.InsurancePolicies.Passports.Patients.LastName.Contains(searchText) ||
                        medcard.Medcard.InsurancePolicies.Passports.Patients.FirstName.Contains(searchText) ||
                        medcard.Medcard.InsurancePolicies.Passports.Patients.MiddleName.Contains(searchText))
                    .ToList();
            }
        }
    }
}
