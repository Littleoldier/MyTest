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
    public partial class CH_Unlock : Form
    {
        Color_Box cb;
        public CH_Unlock(Color_Box color)
        {
            InitializeComponent();
            this.cb = color;
        }

        private void sure_Click(object sender, EventArgs e)
        {
            if (this.Password.Text == "bobo")
            {
                this.Close();
                cb.ToUnlock.Enabled = false;
                cb.unlock_content();
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
            if (e.KeyChar == 13)
            {
                sure_Click(sender, e);
            }
        }

    }
}
