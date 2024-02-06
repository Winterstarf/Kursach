using MainApp.classes;
using System;
using System.Data;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.SqlClient;
using System.Threading;
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

            SqlConnection con = new SqlConnection(@"Data Source=201-04\SQLEXPRESS;Initial Catalog=BigBoars;Integrated Security=SSPI");
            con.Open();

            SqlCommand cmd = new SqlCommand("select * from Staff where Username='" + username + "' and StaffPass='" + password + "'", con);
            cmd.CommandType = CommandType.Text;

            object result = cmd.ExecuteScalar();

            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmd;
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                this.Close();
                MainWindow m = new MainWindow();
                
            }
            else
            {
                MessageBox.Show("Не существует такого логина/пароля!");
            }

            con.Close();
        }
    }
}