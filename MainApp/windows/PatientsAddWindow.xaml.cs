using MainApp.assets.models;
using MainApp.pages;
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
    /// Interaction logic for PatientsAddWindow.xaml
    /// </summary>
    public partial class PatientsAddWindow : Window
    {
        BigBoarsEntities db_cont = new BigBoarsEntities();
        public PatientsAddWindow()
        {
            InitializeComponent();

            var newPatientData = new NewPatientData();
            this.DataContext = newPatientData;

            var genders = db_cont.Genders.ToList();
            newPatientData.GenderOptions = genders;
            var companies = db_cont.InsuranceCompanies.ToList();
            newPatientData.CompanyOptions = companies;
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            var newPatientData = (NewPatientData)this.DataContext;
            CultureInfo us = new CultureInfo("en-US");

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
                BirthDate = DateTime.ParseExact(newPatientData.BirthDate, "yyyy-MM-dd", us),
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
                idInsuranceCompany = newPatientData.SelectedCompany.id,
                PolicyNumber = newPatientData.PolicyNumber,
                ExpiryDate = DateTime.ParseExact("2100-01-01", "yyyy-MM-dd", us),
                idPassport = newPassportId
            };
            db_cont.InsurancePolicies.AddObject(newInsurancePolicy);
            db_cont.SaveChanges();

            int newInsurancePolicyId = newInsurancePolicy.id;

            Random rnd = new Random();
            int mn = rnd.Next(100000, 999999);
            DateTime di = DateTime.Now;

            var newMedcard = new Medcards
            {
                MedcardNumber = Convert.ToString(mn),
                idPolicy = newInsurancePolicyId,
                DateIssued = di
            };
            db_cont.Medcards.AddObject(newMedcard);
            db_cont.SaveChanges();
            this.Close();
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
        public string idAddress { get; set; }
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
