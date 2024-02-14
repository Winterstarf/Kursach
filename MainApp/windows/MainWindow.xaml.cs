using MainApp.pages;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using WpfAnimatedGif;

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

        private async void SecretSurprise_btn_Click(object sender, RoutedEventArgs e)
        {
            Secret_img.Visibility = Visibility.Visible;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new System.Uri(@"\assets\images\hog.gif", UriKind.Relative);
            image.EndInit();

            ImageBehavior.SetAnimatedSource(Secret_img, image);
            ImageBehavior.SetRepeatBehavior(Secret_img, new RepeatBehavior(1));

            await Task.Delay(8000);

            Secret_img.Visibility = Visibility.Hidden;
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
