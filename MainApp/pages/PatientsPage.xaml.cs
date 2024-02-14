using MainApp.assets.models;
using MainApp.windows;
using System.Data.Common.CommandTrees.ExpressionBuilder;
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
        readonly BigBoarsEntities db_cont = new BigBoarsEntities();

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
            var q = from medcard in db_cont.Medcards
                    join email in db_cont.Emails on medcard.InsurancePolicies.Passports.Patients.id equals email.idPatient into emailGroup
                    join phone in db_cont.Phones on medcard.InsurancePolicies.Passports.Patients.id equals phone.idPatient into phoneGroup
                    select new
                    {
                        Medcard = medcard,
                        Emails = emailGroup,
                        Phones = phoneGroup,
                    };

            if (string.IsNullOrEmpty(searchText)) DG_Patients.ItemsSource = q.ToList();
            else
            {
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

        private void Del_btn_Click(object sender, RoutedEventArgs e)
        {
            if (DG_Patients.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для удаления!");
                return;
            }
            else
            {
                var selectedData = (dynamic)DG_Patients.SelectedItem;
                Medcards selectedRow = selectedData.Medcard;

                MessageBoxResult res = MessageBox.Show("Подтвердите удаление", "Удаление строки", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    db_cont.DeleteObject(selectedRow);
                    db_cont.SaveChanges();

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
            }
        }

        private void Add_btn_Click(object sender, RoutedEventArgs e)
        {
            var win = new PatientsAddWindow();
            win.ShowDialog();

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

        private void Refresh_btn_Click(object sender, RoutedEventArgs e)
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
    }
}
