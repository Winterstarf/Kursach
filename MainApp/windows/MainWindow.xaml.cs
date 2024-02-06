using System.Windows;
using System.Windows.Navigation;

namespace MainApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            AuthWindow a = new AuthWindow();
            this.Owner = a;
        }

        private void Patients_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hospitalizations_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HAD_btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
