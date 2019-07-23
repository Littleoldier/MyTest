using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Print_Message;
using Print.Message.Bll;

namespace WindowsForms_print
{
    public partial class JST_CheckAndDelect : Form
    {

        PrintMessageBLL PMB = new PrintMessageBLL();
        List<PrintMessage> list = new List<PrintMessage>();

        public JST_CheckAndDelect()
        {
            InitializeComponent();
            int wid = Screen.PrimaryScreen.WorkingArea.Width;
            this.Width = wid;
            this.dataGridView1.Width = wid;
            this.SnOrImei.Text = "请输入SN号或IMEI号";

            this.JST_DY.Items.Add("SIM");
            this.JST_DY.Items.Add("VIP");
            this.JST_DY.Items.Add("BAT");
            this.JST_DY.Items.Add("SN");
            this.JST_DY.Items.Add("ICCID");
            this.JST_DY.Items.Add("MAC");
            this.JST_DY.Items.Add("Equipment");
        }

        private void SnOrImei_Click(object sender, EventArgs e)
        {
            if (this.SnOrImei.Text == "请输入SN号或IMEI号")
            {
                this.SnOrImei.Text = string.Empty;
            }
        }

        private void SnOrImei_Leave(object sender, EventArgs e)
        {
            if (this.SnOrImei.Text == "") {
                this.SnOrImei.Text = "请输入SN号或IMEI号";
            }
        }

        private void JST_check_Click(object sender, EventArgs e)
        {
            if (this.SnOrImei.Text == "请输入SN号或IMEI号" ||this.SnOrImei.Text=="")
            {
                MessageBox.Show("请先输入IMEI号或SN号");
                this.SnOrImei.Text = "请输入SN号或IMEI号";
                this.JST_ID.Text = "";
                this.JST_ID.Items.Clear();
                this.dataGridView1.Rows.Clear();
            }
            else
            {
                this.dataGridView1.Rows.Clear();
                list = PMB.SelectPrintMesBySNOrIMEIBLL(this.SnOrImei.Text);
                if (list.Count == 0)
                {
                    MessageBox.Show("查找不到该打印记录");
                    this.SnOrImei.Text = "请输入SN号或IMEI号";
                    this.JST_ID.Text = "";
                    this.JST_ID.Items.Clear();
                    this.JST_DY.Text = "";
                    this.dataGridView1.Rows.Clear();
                }
                else
                {
                    foreach (PrintMessage a in list)
                    {
                        this.dataGridView1.Rows.Add(a.ID,a.Zhidan, a.IMEI, a.SN, a.SIM, a.VIP, a.BAT, a.ICCID,a.MAC,a.Equipment,a.IMEIRel, a.JS_PrintTime,a.SoftModel, a.JS_TemplatePath);
                        this.JST_ID.Text = a.ID.ToString();
                        this.JST_ID.Items.Clear();
                        this.JST_ID.Items.Add(a.ID);
                    }
                }
            }
        }

        static bool IsNumeric(string s)
        {
            double v;
            if (double.TryParse(s, out v))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //private void DeleteId_Leave(object sender, EventArgs e)
        //{
        //    if (this.DeleteId.Text == "")
        //    {
        //        this.DeleteId.Text = "请输入要删除的ID";
        //    }
        //    else {
        //        if (IsNumeric(this.DeleteId.Text))
        //        {
        //            if (long.Parse(this.DeleteId.Text) < 0)
        //            {
        //                MessageBox.Show("删除ID不能小于0");
        //                this.DeleteId.Clear();
        //                this.DeleteId.Text = "请输入要删除的ID";
        //            }
        //            else if (long.Parse(this.DeleteId.Text) > 2147483647)
        //            {
        //                MessageBox.Show("删除ID超出范围");
        //                this.DeleteId.Clear();
        //                this.DeleteId.Text = "请输入要删除的ID";
        //            }
        //        }
        //        else {
        //            MessageBox.Show("ID只能为数字，请重新输入");
        //            this.DeleteId.Clear();
        //            this.DeleteId.Text = "请输入要删除的ID";
        //        }
        //    }
        //}

        private void JST_delete_Click(object sender, EventArgs e)
        {
            if (this.JST_ID.Text=="")
            {

                MessageBox.Show("请先选择ID");
                this.JST_ID.Focus();
            }
            else
            {
                if (this.JST_DY.Text == "") {

                    MessageBox.Show("请先选择要删除的字段");
                }
                else
                {
                    PMB.DeletePrintMessageBLL(int.Parse(this.JST_ID.Text), this.JST_DY.Text);
                    this.dataGridView1.Rows.Clear();
                    list = PMB.SelectPrintMesBySNOrIMEIBLL(this.SnOrImei.Text);
                    foreach (PrintMessage a in list)
                    {
                        this.dataGridView1.Rows.Add(a.ID, a.Zhidan, a.IMEI, a.SN, a.SIM, a.VIP, a.BAT, a.ICCID, a.MAC, a.Equipment, a.IMEIRel, a.JS_PrintTime, a.SoftModel, a.JS_TemplatePath);
                    }
                    MessageBox.Show("删除成功");
                }
            }
        }

    }
}
