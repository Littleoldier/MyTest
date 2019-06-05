using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms_print
{
    public partial class JS_Unlock : Form
    {
        Form1 f;
        public JS_Unlock(Form1 form)
        {
            InitializeComponent();
            this.f = form;
        }
        

        private void sure_Click(object sender, EventArgs e)
        {
            if (this.Password.Text == "bobo") {
                this.reminder.Text = "";
                this.Close();
                f.ToUnlock.Enabled = false;
                f.Unlock_content();
            }
            else
            {
                this.reminder.Text = "密码错误，请重新输入";
                this.Password.Clear();
                this.Password.Focus();
            }
           
        }

        private void Password_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) {
                sure_Click(sender, e);
            }
        }
    }
}
