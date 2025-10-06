using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SergePlanner2
{
    public partial class Login : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=SergeB;Initial Catalog=SergePlanner;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
        public Login()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void loginBtn_Click(object sender, EventArgs e)
        {


        }

        

        private void usernameBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Register Register = new Register();
            Register.Show();
            this.Hide();
        }

        private void loginBtn_Click_1(object sender, EventArgs e)
        {


            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                string query = "SELECT * FROM [User] WHERE email=@email AND password=@pass";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", usernameBox.Text.Trim());
                cmd.Parameters.AddWithValue("@pass", passwordBox.Text.Trim());

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Close();
                    MessageBox.Show("Login successful!");
                    Planning planForm = new Planning(usernameBox.Text.Trim()); planForm.ShowDialog();
                    planForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid username or password!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during login: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }
    }
}
