using MainApp.assets.models;
using MainApp.windows;
using System.Linq;
using System.Windows;
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

        private void Del_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DG_HADs.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана строка для удаления!");
                return;
            }
            else
            {
                var selectedData = (dynamic)DG_HADs.SelectedItem;

                MessageBoxResult res = MessageBox.Show("Подтвердите удаление", "Удаление строки", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (res == MessageBoxResult.Yes)
                {
                    db_cont.DeleteObject(selectedData);
                    db_cont.SaveChanges();

                    DG_HADs.ItemsSource = db_cont.HealingAndDiagnostics.ToList();
                }
            }
        }

        private void Add_btn_Click(object sender, RoutedEventArgs e)
        {
            var win = new HADAddWindow();
            win.ShowDialog();
            DG_HADs.ItemsSource = db_cont.HealingAndDiagnostics.ToList();
        }

        private void Refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            DG_HADs.ItemsSource = db_cont.HealingAndDiagnostics.ToList();
        }
    }
}
