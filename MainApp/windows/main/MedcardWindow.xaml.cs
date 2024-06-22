using MainApp.assets.models;
using System.Linq;
using System.Windows;

namespace MainApp.windows.main
{
    /// <summary>
    /// Interaction logic for MedcardWindow.xaml
    /// </summary>
    public partial class MedcardWindow : Window
    {
        readonly HelixDBEntities db_cont = new HelixDBEntities();
        readonly private clients _selectedClient;

        public MedcardWindow(clients client)
        {
            InitializeComponent();

            _selectedClient = client;

            var ClientData = new ClientData
            {
                Clientid = _selectedClient.id.ToString(),
                Lastname = _selectedClient.last_name,
                Firstname = _selectedClient.first_name,
                Middlename = _selectedClient.middle_name,
                Gender = _selectedClient.genders.gender_name,
                Phone = _selectedClient.phone_number,
                Email = _selectedClient.email,
                Passport = _selectedClient.passport,
                Card_number = _selectedClient.card_number.ToString(),
                Card_balance = _selectedClient.card_balance.ToString()
            };
            this.DataContext = ClientData;
        }
    }

    public class ClientData
    {
        public string Clientid { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Passport { get; set; }
        public string Card_number { get; set; }
        public string Card_balance { get; set; }
    }
}
