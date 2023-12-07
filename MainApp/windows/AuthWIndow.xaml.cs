using MainApp.classes;
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

        private void NumTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (NumTB.Text != string.Empty)
                {
                    string username = Convert.ToString(NumTB.Text);

                    SqlConnection con = new SqlConnection(@"Data Source=201-04\SQLEXPRESS;Initial Catalog=Neva;Integrated Security=SSPI");
                    con.Open();

                    SqlCommand cmd = new SqlCommand("select * from Staff where Username='" + username + "'", con);
                    cmd.CommandType = CommandType.Text;

                    object result = cmd.ExecuteScalar();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = cmd;
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);

                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        PassPB.IsEnabled = true;

                        Methods m = new Methods();
                        m.username = username;
                    }
                    else
                    {
                        MessageBox.Show("Не существует такого логина!");
                    }

                    con.Close();
                }
                else
                {
                    MessageBox.Show("Введите логин!");
                }
            }    
        }

        private void PassPB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (PassPB.Password != string.Empty)
                {
                    Methods m = new Methods();
                    string username = m.username;
                    string password = Convert.ToString(PassPB.Password);
                    string code;

                    SqlConnection con = new SqlConnection(@"Data Source=201-04\SQLEXPRESS;Initial Catalog=Neva;Integrated Security=SSPI");
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
                        m.code = m.GenerateCode();
                        MessageBox.Show(m.code);
                       
                    }
                    else
                    {
                        MessageBox.Show("Не существует такого пароля!");
                    }

                    con.Close();
                }
                else
                {
                    MessageBox.Show("Введите пароль!");
                }
            }
        }
    }
}