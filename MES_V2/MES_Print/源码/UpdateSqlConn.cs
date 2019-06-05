using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.Text.RegularExpressions;

namespace WindowsForms_print
{
    public partial class UpdateSqlConn : Form
    {
        Form1 f;
        public UpdateSqlConn(Form1 form)
        {
            InitializeComponent();
            this.f = form;
            GetConfig();
        }

        private static readonly string conStr1 = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        static string server;
        static string uid;
        static string pwd;
        string conStr;

        public void GetConfig()
        {
            server = ConfigurationManager.AppSettings["server"];
            uid = ConfigurationManager.AppSettings["uid"];
            pwd = ConfigurationManager.AppSettings["pwd"];
            this.CIp.Text = server;
            this.CUser.Text = uid;
            this.CPassword.Text = pwd;
        }

        //测试连接并保存按钮
        private void TextConn_Click(object sender, EventArgs e)
        {
            if (!IsValidIp(this.CIp.Text))
            {
                MessageBox.Show("IP格式不正确，请重新输入");
                this.CIp.Focus();
                return;
            }
            conStr = "Data Source = " + this.CIp.Text + "," + this.Cport.Text + "; Initial Catalog = GPSTest; User ID = " + CUser.Text + "; PassWord=" + CPassword.Text + "";
            SqlConnection conn1 = new SqlConnection(conStr);
            try
            {
                this.TextConn.Enabled = false;
                conn1.Open();
                SaveCon_Click(sender, e);
            }
            catch
            {
                MessageBox.Show("连接失败");
                this.TextConn.Enabled = true;
            }
            finally
            {
                conn1.Close();
            }
        }

        public static bool IsValidIp(string strIn)
        {
            Regex validipregex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
            return (strIn != "" && validipregex.IsMatch(strIn.Trim())) ? true : false;
        }

        //保存数据库配置
        private void SaveCon_Click(object sender, EventArgs e)
        {
            conStr = "Data Source = " + this.CIp.Text + "," + this.Cport.Text + "; Initial Catalog = GPSTest; User ID = " + CUser.Text + "; PassWord=" + CPassword.Text + "";
            UpdateConnectionStringsConfig("conn1", conStr);
            f.refrezhidan();
        }

        //将新配置更新到配置文件里
        public void UpdateConnectionStringsConfig(string newName, string newConString)
        {
            //指定config文件读取
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            config.AppSettings.Settings["server"].Value = this.CIp.Text;
            config.AppSettings.Settings["uid"].Value = this.CUser.Text;
            config.AppSettings.Settings["pwd"].Value = this.CPassword.Text;

            bool exist = false; //记录该连接串是否已经存在  
            //如果要更改的连接串已经存在  
            if (config.ConnectionStrings.ConnectionStrings[newName] != null)
            {
                exist = true;
            }
            // 如果连接串已存在，首先删除它  
            if (exist)
            {
                config.ConnectionStrings.ConnectionStrings.Remove(newName);
            }
            //新建一个连接字符串实例  
            ConnectionStringSettings mySettings = new ConnectionStringSettings(newName, newConString);
            // 将新的连接串添加到配置文件中.  
            config.ConnectionStrings.ConnectionStrings.Add(mySettings);
            // 保存对配置文件所作的更改  
            config.Save(ConfigurationSaveMode.Modified);
            // 强制重新载入配置文件的ConnectionStrings配置节  
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("appSettings");
            MessageBox.Show("保存成功");
            this.TextConn.Enabled = true;
        }

        private void cansel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
