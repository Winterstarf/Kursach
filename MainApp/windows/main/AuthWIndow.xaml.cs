using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = Convert.ToString(UsernameTB.Text);
            string password = Convert.ToString(PassPB.Password);

            //DESKTOP-QLMK9N or 201-04\SQLEXPRESS
            SqlConnection con = new SqlConnection(@"Data Source=201-04\SQLEXPRESS;Initial Catalog=BigBoars;Integrated Security=SSPI");
            con.Open();

            SqlCommand cmd = new SqlCommand("select * from Doctors where Username='" + username + "' and Passwrd='" + password + "'", con)
            {
                CommandType = CommandType.Text
            };

            SqlDataAdapter adapter = new SqlDataAdapter
            {
                SelectCommand = cmd
            };
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                MainWindow m = new MainWindow();
                m.CurrentUserName = username.ToString();
                m.CurrentDoctor_tb.Text = m.CurrentUserName.ToString();
                m.Show();
                this.Close();
            }
            else if ((username == "" || username == null) && (password == "" || password == null)) MessageBox.Show("Логин и пароль не введены!");
            else if (username == "" || username == null) MessageBox.Show("Логин не введён!");
            else if (password == "" || password == null) MessageBox.Show("Пароль не введён!");
            else MessageBox.Show("Аккаунт не найден!");

            con.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("НЕЧЕГО ОТМЕНЯТЬ!!!!!!!!!!");
        }
    }
}
