using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UserAccount.Pri_Bll;

namespace WindowsForms_print
{
    public partial class SignIn : Form
    {

        LUserAccountBLL luab = new LUserAccountBLL();

        string Usertype = "";
        string UserNamestr = "";

        public string Usertype1 { get => Usertype; set => Usertype = value; }
        public string UserNamestr1 { get => UserNamestr; set => UserNamestr = value; }

        public SignIn()
        {
            InitializeComponent();
            this.UserName.Focus();
        }
        private void UserName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                if (this.UserName.Text != "")
                {
                    this.Password.Focus();
                }
            }
        }

        private void Password_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                if (this.UserName.Text == "")
                {
                    MessageBox.Show("账号不能为空!", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (this.Password.Text != "")
                {
                    this.DetermineBt.Focus();
                }
            }

        }

        //登录
        private void DetermineBt_Click(object sender, EventArgs e)
        {
            if(this.UserName.Text =="" || this.Password.Text == "")
            {
                MessageBox.Show("账号或密码不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.UserName.Clear();
                this.Password.Clear();
                this.UserName.Focus();
                return;
            }

            //查数据库的账号密码
            if (luab.CheckUserNamePassword(this.UserName.Text, this.Password.Text) == 1)
            {
                Usertype = luab.GetUserType(this.UserName.Text, this.Password.Text);
                //if (Usertype != "")
                //{
                    UserNamestr = this.UserName.Text;
                    this.Close();
                //}
                //else
                //{
                //    MessageBox.Show("账号无权限！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    this.UserName.Clear();
                //    this.Password.Clear();
                //    this.UserName.Focus();
                //    return;
                //}
             }
            else
            {
                MessageBox.Show("账号或密码不正确！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.UserName.Clear();
                this.Password.Clear();
                this.UserName.Focus();
                return;
            }

        }

        //取消
        private void CancelBt_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
