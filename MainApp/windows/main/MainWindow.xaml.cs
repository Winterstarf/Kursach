using MainApp.pages;
using System.Windows;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly PatientsPage pap;
        readonly HospitalizationsPage hop;
        readonly HADPage hap;

        public string CurrentUserName { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            pap = new PatientsPage();
            hop = new HospitalizationsPage();
            hap = new HADPage();
        }

        private void Patients_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage_tb.Text = "Пациенты";
            DG_frm.NavigationService.Navigate(pap);
        }

        private void Hospitalizations_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage_tb.Text = "Госпитализации";
            DG_frm.NavigationService.Navigate(hop);
        }

        private void HAD_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage_tb.Text = "Лечение и диагностика";
            DG_frm.NavigationService.Navigate(hap);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //i dont know why this works, dont add anything here
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
