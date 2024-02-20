using MainApp.assets.models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace MainApp.windows
{
    /// <summary>
    /// Interaction logic for PatientsAddWindow.xaml
    /// </summary>
    public partial class PatientsAddWindow : Window
    {
        readonly BigBoarsEntities db_cont = new BigBoarsEntities();
        private int Manual;

        public PatientsAddWindow()
        {
            InitializeComponent();

            var newPatientData = new NewPatientData();
            this.DataContext = newPatientData;

            var genders = db_cont.Genders.ToList();
            newPatientData.GenderOptions = genders;
            var companies = db_cont.InsuranceCompanies.ToList();
            newPatientData.CompanyOptions = companies;

            InsCompDroplist_rb.IsChecked = true;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newPatientData = (NewPatientData)this.DataContext;
                int ActualIdInsuranceCompany;

                if (LastName_tb.Text == string.Empty || FirstName_tb.Text == string.Empty || Gender_cb.SelectedItem == null
                    || BirthDate_dp.SelectedDate == null || City_tb.Text == string.Empty 
                    || Street_tb.Text == string.Empty || HouseNumber_tb.Text == string.Empty || InsPolicy_tb.Text == string.Empty || !ulong.TryParse(InsPolicy_tb.Text, out ulong insPolicyRes) 
                    || InsPolicy_tb.Text.Length != 16 || (Manual == 0 && InsComp_cb.SelectedItem == null) || (Manual == 1 && InsComp_tb.Text == string.Empty) 
                    || Email_tb.Text == string.Empty || Phone_tb.Text == string.Empty || Passport_tb.Text == string.Empty || !uint.TryParse(Passport_tb.Text, out uint passportRes) || Passport_tb.Text.Length != 10)
                {
                    throw new Exception("Некоторые обязательные поля не указаны или содержат неправильный тип данных!");
                }

                if (Manual == 1)
                {
                    var newInsuranceCompany = new InsuranceCompanies
                    {
                        CompanyName = InsComp_tb.Text
                    };
                    db_cont.InsuranceCompanies.AddObject(newInsuranceCompany);
                    db_cont.SaveChanges();
                    ActualIdInsuranceCompany = newInsuranceCompany.id;
                }
                else
                {
                    ActualIdInsuranceCompany = newPatientData.SelectedCompany.id;
                }

                var newAddress = new Addresses
                {
                    City = newPatientData.City,
                    Street = newPatientData.Street,
                    HouseNumber = newPatientData.HouseNumber,
                    FlatNumber = newPatientData.FlatNumber
                };
                db_cont.Addresses.AddObject(newAddress);
                db_cont.SaveChanges();
                int newAddressId = newAddress.id;

                var newPatient = new Patients
                {
                    LastName = newPatientData.LastName,
                    FirstName = newPatientData.FirstName,
                    MiddleName = newPatientData.MiddleName,
                    idGender = newPatientData.SelectedGender.id,
                    BirthDate = (DateTime)BirthDate_dp.SelectedDate,
                    idAddress = newAddressId,
                    WorkPlace = newPatientData.WorkPlace
                };
                db_cont.Patients.AddObject(newPatient);
                db_cont.SaveChanges();
                int newPatientId = newPatient.id;

                var newEmail = new Emails
                {
                    Email = newPatientData.Email,
                    idPatient = newPatientId,
                };
                db_cont.Emails.AddObject(newEmail);
                db_cont.SaveChanges();

                var newPhone = new Phones
                {
                    Phone = newPatientData.Phone,
                    idPatient = newPatientId
                };
                db_cont.Phones.AddObject(newPhone);
                db_cont.SaveChanges();

                var newPassport = new Passports
                {
                    PassportNumber = newPatientData.SerialNumber,
                    idPatient = newPatientId
                };
                db_cont.Passports.AddObject(newPassport);
                db_cont.SaveChanges();
                int newPassportId = newPassport.id;

                var newInsurancePolicy = new InsurancePolicies
                {
                    idInsuranceCompany = ActualIdInsuranceCompany,
                    PolicyNumber = newPatientData.PolicyNumber,
                    ExpiryDate = DateTime.Parse("2100-01-01"),
                    idPassport = newPassportId
                };
                db_cont.InsurancePolicies.AddObject(newInsurancePolicy);
                db_cont.SaveChanges();
                int newInsurancePolicyId = newInsurancePolicy.id;

                int mn = Convert.ToInt32(db_cont.Medcards.Max(p => p.MedcardNumber)) + 1;
                var newMedcard = new Medcards
                {
                    MedcardNumber = Convert.ToString(mn),
                    idPolicy = newInsurancePolicyId,
                    DateIssued = DateTime.Now
                };
                db_cont.Medcards.AddObject(newMedcard);
                db_cont.SaveChanges();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void InsCompDroplist_rb_Checked(object sender, RoutedEventArgs e)
        {
            Manual = 0;
            InsComp_cb.Visibility = Visibility.Visible;
            InsComp_tb.Visibility = Visibility.Hidden;
        }

        private void InsCompManual_rb_Checked(object sender, RoutedEventArgs e)
        {
            Manual = 1;
            InsComp_cb.Visibility = Visibility.Hidden;
            InsComp_tb.Visibility = Visibility.Visible;
        }
    }

    public class NewPatientData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public Genders SelectedGender { get; set; }
        public List<Genders> GenderOptions { get; set; }
        public string BirthDate { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string FlatNumber { get; set; }
        public string WorkPlace { get; set; }
        public string PolicyNumber { get; set; }
        public InsuranceCompanies SelectedCompany { get; set; }
        public List<InsuranceCompanies> CompanyOptions { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SerialNumber { get; set; }
        public string MedcardNumber { get; set; }
        public string MedcardDateIssued { get; set; }
    }
}
