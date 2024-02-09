using MainApp.assets.models;
using MainApp.pages;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MainApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PatientsPage pap;
        HospitalizationsPage hop;
        HADPage hap;

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
            
        }
    }
}
