using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using Seagull.BarTender.Print;
using System.Windows.Forms;
using Print_Message;
using Print.Message.Bll;
using ManuOrder.Param.BLL;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using System.Media;
using System.Threading;
using System.Diagnostics;
using DataRelative.Param.BLL;
using ExcelPrint.Param.Bll;
using ManuPrintRecord.Param.BLL;
using TestResult.Param.BLL;
using ManuFuselagePrintRecord.Param.BLL;


namespace WindowsForms_print
{
    public partial class Form1 : Form
    {
        public string GetName()
        {
            SignIn SI = new SignIn();
            return  SI.UserNamestr1;
        }
        //写日志函数
        public static void Log(string msg, Exception e)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory +"\\log\\"+ System.DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine("");
                writer.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + msg);
                writer.Flush();
                writer.Close();
            }
            catch
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\log\\" +System.DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                if (!Directory.Exists(path))
                {
                    File.Create(path).Close();
                }
                StreamWriter writer = File.AppendText(path);
                writer.WriteLine("");
                writer.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + msg + "错误：" + e.Message);
                writer.Flush();
                writer.Close();
            }
        }

        string outString;

        //音频文件
        SoundPlayer player = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "失败.wav");
        SoundPlayer player1 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "请先选择模板.wav");
        SoundPlayer player2 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "请选择制单号.wav");
        SoundPlayer player3 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "校验错误.wav");
        //SoundPlayer player4 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "绑定成功.wav");


        ManuFuselagePrintRecordParamBLL MFPRPB = new ManuFuselagePrintRecordParamBLL();

        List<ManuFuselagePrintRecordParam> mfprpb = new List<ManuFuselagePrintRecordParam>();

        DataRelativeSheetBLL DRSB = new DataRelativeSheetBLL();

        ManuExcelPrintParamBLL MEPPB = new ManuExcelPrintParamBLL();

        ManuPrintRecordParamBLL MPRPB = new ManuPrintRecordParamBLL();

        TestResultBLL TRB = new TestResultBLL();

        ManuOrderParamBLL MOPB = new ManuOrderParamBLL();

        PrintMessageBLL PMB = new PrintMessageBLL();

        InputExcelBLL IEB = new InputExcelBLL();

        List<Gps_ManuOrderParam> G_MOP = new List<Gps_ManuOrderParam>();

        List<PrintMessage> list = new List<PrintMessage>();

        PrintMessage PList = new PrintMessage();

        SortedDictionary<int, string> RelationFields = new SortedDictionary<int, string>();

        SortedDictionary<int, string> CheckFields = new SortedDictionary<int, string>();

        //用于记录打印复选框的选择：c1为客供SN，c2为不打印校验码，c3为不打印SN号
        int c1, c2, c3;

        //记录打印模板路径
        string lj = "";
        Messages messages;
        int waitout = 10000;

        //记录模板刷新次数
        int RefreshNum = 0;

        //记录模板打印份数
        int TN = 1;

        //记录SN号后缀位数
        int s;

        //获取订单数据
        int StartZhiDan = 0;

        //记录不打印校验码时IMEI的位数
        int ImeiDig;

        public static int QuitThreadFalge = 0;

        //模式标志位
        int ModeFalge = 0;

        string snstr = "";
        string simstr = "";
        string iccidstr = "";
        string macstr = "";
        string equistr = "";
        string vipstr = "";
        string batstr = "";
        string rfidstr = "";

        //记录打印时间
        string ProductTime = "";
        Engine btEngine = new Engine();
        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            int wid = Screen.PrimaryScreen.WorkingArea.Width;
            int hei = Screen.PrimaryScreen.WorkingArea.Height;
            this.Height = hei;
            this.tabControl1.Width = wid;
            this.tabPage2.Width = wid;
        }
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnvalue, int buffersize, string filepath);

        private string IniFilePath;

        public int QuitThreadFalge1 { get => QuitThreadFalge; set => QuitThreadFalge = value; }
        public int QuitThreadFalge2 { get => QuitThreadFalge; set => QuitThreadFalge = value; }

        //读取ini配置文件
        private void GetValue(string section, string key, out string value)
        {
            IniFilePath = AppDomain.CurrentDomain.BaseDirectory + "PrintVariable.ini";
            StringBuilder stringBuilder = new StringBuilder();
            GetPrivateProfileString(section, key, "", stringBuilder, 1024, IniFilePath);
            value = stringBuilder.ToString();
        }

        //程序加载时运行的函数
        public void Form1_Load(object sender, EventArgs e)
        {
            
            PrintDocument print = new PrintDocument();
            string sDefault = print.PrinterSettings.PrinterName;//默认打印机名
            this.Printer1.Text = sDefault;
            foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
            {
                Printer1.Items.Add(sPrint);
            }
            G_MOP = MOPB.SelectZhidanNumBLL();
            foreach (Gps_ManuOrderParam a in G_MOP)
            {
                this.CB_ZhiDan.Items.Add(a.ZhiDan);
            }
            string NowData = System.DateTime.Now.ToString("yyyy.MM.dd");
            this.ProductData.Text = NowData;
            //开启打印机引擎
            btEngine.Start();
            //bool isAlive = btEngine.IsAlive;
            //this.reminder.Text = isAlive.ToString();
            //if (!btEngine.IsAlive)
            //    btEngine.Start();
        }

        //更换数据库时调用
        public void refrezhidan()
        {
            this.CB_ZhiDan.Items.Clear();
            G_MOP = MOPB.SelectZhidanNumBLL();
            foreach (Gps_ManuOrderParam a in G_MOP)
            {
                this.CB_ZhiDan.Items.Add(a.ZhiDan);
            }
            DRSB.refeshConBLL();
            MEPPB.refeshConBLL();
            MPRPB.refeshConBLL();
            PMB.refeshConBLL();
            TRB.refeshConBLL();
        }

        //选择制单时引发的事件
        private void CB_ZhiDan_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.reminder.Text = "";
            this.IMEI_Start.Clear();
            this.PrintNum.Clear();
            this.ProductData.Clear();
            this.Re_IMEINum.Clear();
            this.ReImeiNum1.Clear();
            this.ReImeiNum2.Clear();

            this.Select_Template1.Clear();
            this.SoftModel.Clear();
            this.SN1_num.Clear();
            this.SN2_num.Clear();
            this.ProductNo.Clear();
            this.SoftwareVersion.Clear();
            this.IMEI_num1.Clear();
            this.IMEI_num2.Clear();
            this.IMEI_Present.Clear();
            this.SIM_num1.Clear();
            this.SIM_num2.Clear();
            this.BAT_num1.Clear();
            this.BAT_num2.Clear();
            this.VIP_num1.Clear();
            this.VIP_num2.Clear();
            this.Remake.Clear();
            this.IMEIRel.Clear();
            this.IMEI_Start.Clear();
            this.PrintNum.Clear();
            this.Re_IMEINum.Clear();
            this.ReImeiNum1.Clear();
            this.ReImeiNum2.Clear();

            this.StartZhiDan = 0;

            this.CheckIMEI2.Checked = false;
            this.CheckSIM.Checked = false;
            this.CheckBAT.Checked = false;
            this.CheckICCID.Checked = false;
            this.CheckMAC.Checked = false;
            this.CheckEquipment.Checked = false;
            this.CheckVIP.Checked = false;
            this.CheckRFID.Checked = false;

            this.RelationSN.Checked = false;
            this.RelationSIM.Checked = false;
            this.RelationICCID.Checked = false;
            this.RelationBAT.Checked = false;
            this.RelationMAC.Checked = false;
            this.RelationEquipment.Checked = false;
            this.RelationVIP.Checked = false;
            this.RelationRFID.Checked = false;

            this.PrintMode1.Checked = false;
            this.PrintMode2.Checked = false;

            this.SnFromCustomer.Checked = false;
            this.NoCheckCode.Checked = false;
            this.NoSn.Checked = false;
            this.Hexadecimal.Checked = false;
            this.SNHex.Checked = false;
            this.Re_Nocheckcode.Checked = false;
            this.RePrintOne.Checked = false;
            this.RePrintMore.Checked = false;
            this.Re_IMEINum.ReadOnly = true;
            this.ReImeiNum1.ReadOnly = true;
            this.ReImeiNum2.ReadOnly = true;
            string NowData = System.DateTime.Now.ToString("yyyy.MM.dd");
            this.ProductData.Text = NowData;
            string ZhidanNum = this.CB_ZhiDan.Text;
            G_MOP = MOPB.selectManuOrderParamByzhidanBLL(ZhidanNum);
 
            //string Presentsn = PMB.SelectPresentSnByZhidanBLL(this.CB_ZhiDan.Text);
            //if (Presentsn != "")
            //{
            //    char[] a = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            //    char[] b = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            //    string sn2_aft = Presentsn.TrimStart(a);
            //    string sn1_pre = Presentsn.TrimEnd(b);
            //    this.SN1_num.Text = sn1_pre + (long.Parse(sn2_aft) + 1).ToString().PadLeft(sn2_aft.Length, '0');
            //}
        }

        //点击制单号下拉框时发生
        private void CB_ZhiDan_DropDown(object sender, EventArgs e)
        {
            
        }

        //判断是否为纯数字
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

        //获取IMEI的校验位
        public string getimei15(string imei)
        {
            if (imei.Length == 14)
            {
                Char[] imeiChar = imei.ToCharArray();
                int resultInt = 0;
                for (int i = 0; i < imeiChar.Length; i++)
                {
                    int a = int.Parse(imeiChar[i].ToString());
                    i++;
                    int temp = int.Parse(imeiChar[i].ToString()) * 2;
                    int b = temp < 10 ? temp : temp - 9;
                    resultInt += a + b;
                }
                resultInt %= 10;
                resultInt = resultInt == 0 ? 0 : 10 - resultInt;
                return resultInt + "";
            }
            else { return ""; }
        }

        //判断是否为日期格式的函数
        public bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //鼠标移出生产日期框时引发的函数
        private void ProductData_MouseLeave(object sender, EventArgs e)
        {
            if (this.ProductData.Text != "")
            {
                if (!IsDate(this.ProductData.Text))
                {
                    player.Play();
                    this.ProductData.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
                }
            }
        }

        //光标离开生产日期框时引发的函数
        private void ProductData_Leave(object sender, EventArgs e)
        {
            if (this.ProductData.Text != "")
            {
                if (!IsDate(this.ProductData.Text))
                {
                    player.Play();
                    this.ProductData.Text = System.DateTime.Now.ToString("yyyy.MM.dd");
                }
            }
        }

        //判断是否有中文字符
        public static bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        //打开模板按钮函数
        private void Open_Template1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件|*.btw";
            ofd.ShowDialog();
            string path = ofd.FileName;
            this.Select_Template1.Text = path;
            lj = path;
        }

        //选择tabControl子页面
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabControl1.SelectedTab.Refresh();
            if (tabControl1.SelectedTab == tabPage2)
            {
                Color_Box CB = new Color_Box();
                CB.TopLevel = false;
                tabPage2.Controls.Add(CB);
                CB.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                CB.Show();
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                CheckRePrint CRP = new CheckRePrint();
                CRP.TopLevel = false;
                tabPage3.Controls.Add(CRP);
                CRP.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                CRP.Show();
            }
        }

        //关闭程序时引发事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Application.Exit();
                foreach (Process p in Process.GetProcessesByName("bartend"))
                {
                    if (!p.CloseMainWindow())
                    {
                        //p.CloseMainWindow();
                        p.Kill();
                    }
                }
                string path = AppDomain.CurrentDomain.BaseDirectory;
                if (Directory.Exists(path + "机身贴模板"))
                {
                    if (File.GetAttributes(path + "机身贴模板") == FileAttributes.Directory)
                    {
                        Directory.Delete(path + "机身贴模板", true);
                    }
                }
                if (Directory.Exists(path + "Excel模板"))
                {
                    if (File.GetAttributes(path + "Excel模板") == FileAttributes.Directory)
                    {
                        Directory.Delete(path + "Excel模板", true);
                    }
                }
                if (Directory.Exists(path + "彩盒贴模板"))
                {
                    if (File.GetAttributes(path + "彩盒贴模板") == FileAttributes.Directory)
                    {
                        Directory.Delete(path + "彩盒贴模板", true);
                    }
                }
                if (MessageBox.Show("是否退出系统？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                {
                    //e.Cancel = true;
                    return;
                }

                QuitThreadFalge = 1;
                //结束打印引擎
                btEngine.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex.Message);
            }
        }

        //将tabControl子页面的按钮文字横着显示
        private void tabControl2_DrawItem(object sender, DrawItemEventArgs e)
        {
            Rectangle tabArea = tabControl2.GetTabRect(e.Index);//主要是做个转换来获得TAB项的RECTANGELF 
            RectangleF tabTextArea = (RectangleF)(tabControl2.GetTabRect(e.Index));
            Graphics g = e.Graphics;
            StringFormat sf = new StringFormat();//封装文本布局信息 
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            Font font = this.tabControl2.Font;
            SolidBrush brush = new SolidBrush(Color.Black);//绘制边框的画笔 
            g.DrawString(((TabControl)(sender)).TabPages[e.Index].Text, font, brush, tabTextArea, sf);
        }

        //选择tabContro2子页面
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl2.SelectedTab == CheckAndDelete)
            {
                JST_CheckAndDelect JSTCAD = new JST_CheckAndDelect();
                JSTCAD.TopLevel = false;
                CheckAndDelete.Controls.Add(JSTCAD);
                JSTCAD.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                JSTCAD.Show();
            }
            else if (tabControl2.SelectedTab == ExcelToPrint)
            {
                PrintFromExcel PFE = new PrintFromExcel();
                PFE.TopLevel = false;
                ExcelToPrint.Controls.Add(PFE);
                PFE.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                PFE.Show();
                //MessageBox.Show("该功能暂建设中...");
            }
        }

        //调试打印
        private void Debug_print_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Select_Template1.Text != "")
                {
                    if(this.StartZhiDan == 0)
                    {
                        this.reminder.AppendText("请获取制单数据\r\n");
                        return;
                    }

                    LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                    //指定打印机名称
                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                    string imei15 = getimei15(this.IMEI_num1.Text);
                    //对模板相应字段进行赋值
                    GetValue("Information", "IMEI", out outString);
                    btFormat.SubStrings[outString].Value = this.IMEI_num1.Text + imei15;
                    GetValue("Information", "SN", out outString);
                    btFormat.SubStrings[outString].Value = this.SN1_num.Text;
                    GetValue("Information", "型号", out outString);
                    btFormat.SubStrings[outString].Value = this.SoftModel.Text;
                    GetValue("Information", "产品编码", out outString);
                    btFormat.SubStrings[outString].Value = this.ProductNo.Text;
                    GetValue("Information", "软件版本", out outString);
                    btFormat.SubStrings[outString].Value = this.SoftwareVersion.Text;
                    GetValue("Information", "SIM卡号", out outString);
                    btFormat.SubStrings[outString].Value = this.SIM_num1.Text;
                    GetValue("Information", "服务卡号", out outString);
                    btFormat.SubStrings[outString].Value = this.VIP_num1.Text;
                    GetValue("Information", "备注", out outString);
                    btFormat.SubStrings[outString].Value = this.Remake.Text;
                    GetValue("Information", "生产日期", out outString);
                    btFormat.SubStrings[outString].Value = this.ProductData.Text;
                    //打印份数,同序列打印的份数
                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                    btFormat.Print();
                    Form1.Log("调试打印了机身贴IMEI号为" + this.IMEI_num1.Text + "的制单", null);
                }
                else
                {
                    player1.Play();
                    this.reminder.AppendText("请先选择模板\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex.Message);
            }
        }

        //选择逐个打印引发的事件
        private void PrintOne_Click(object sender, EventArgs e)
        {
            if (this.PrintOne.Checked == true)
            {
                this.PrintMore.Checked = false;
                this.IMEI_Start.ReadOnly = false;

                this.CheckIMEI2.Enabled = true;
                this.CheckSIM.Enabled = true;
                this.CheckBAT.Enabled = true;
                this.CheckICCID.Enabled = true;
                this.CheckMAC.Enabled = true;
                this.CheckEquipment.Enabled = true;
                this.CheckVIP.Enabled = true;
                this.CheckRFID.Enabled = true;

                this.RelationSN.Enabled = true;
                this.RelationSIM.Enabled = true;
                this.RelationICCID.Enabled = true;
                this.RelationBAT.Enabled = true;
                this.RelationMAC.Enabled = true;
                this.RelationEquipment.Enabled = true;
                this.RelationVIP.Enabled = true;
                this.RelationRFID.Enabled = true;


                //逐个重打
                if (this.RePrintOne.Checked == true)
                {
                    this.RePrintOne.Checked = false;
                    this.Re_IMEINum.ReadOnly = true;
                    this.Re_IMEINum.Clear();

                }

                //批量重打
                if (this.RePrintMore.Checked == true)
                {
                    this.RePrintMore.Checked = false;
                    this.RePrintMore.Checked = false;
                    this.Re_IMEINum.ReadOnly = false;
                    this.ReImeiNum1.ReadOnly = true;
                    this.ReImeiNum2.ReadOnly = true;
                    this.ReImeiNum1.Clear();
                    this.ReImeiNum2.Clear();
                    this.Re_IMEINum.Focus();

                }

                this.PrintNum.ReadOnly = true;
                this.PrintNum.Clear();
                this.IMEI_Start.Focus();
            }
            else
            {
                this.PrintOne.Checked = true;
                this.IMEI_Start.Focus();
            }
        }

        //选择批量打印时引发的事件
        private void PrintMore_Click(object sender, EventArgs e)
        {
            if (this.PrintMore.Checked == true)
            {

                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;
                
                this.RelationSN.Enabled = false;
                this.RelationSIM.Enabled = false;
                this.RelationICCID.Enabled = false;
                this.RelationBAT.Enabled = false;
                this.RelationMAC.Enabled = false;
                this.RelationEquipment.Enabled = false;
                this.RelationVIP.Enabled = false;
                this.RelationRFID.Enabled = false;

                this.CheckIMEI2.Checked = false;
                this.CheckSIM.Checked = false;
                this.CheckBAT.Checked = false;
                this.CheckICCID.Checked = false;
                this.CheckMAC.Checked = false;
                this.CheckEquipment.Checked = false;
                this.CheckVIP.Checked = false;
                this.CheckRFID.Checked = false;

                this.RelationSN.Checked = false;
                this.RelationSIM.Checked = false;
                this.RelationICCID.Checked = false;
                this.RelationBAT.Checked = false;
                this.RelationMAC.Checked = false;
                this.RelationEquipment.Checked = false;
                this.RelationVIP.Checked = false;
                this.RelationRFID.Checked = false;

                if(this.RePrintOne.Checked == true)
                {
                    this.RePrintOne.Checked = false;
                    this.Re_IMEINum.ReadOnly = true;
                    this.Re_IMEINum.Clear();

                }

                //批量重打
                if (this.RePrintMore.Checked == true)
                {
                    this.RePrintMore.Checked = false;
                    this.ReImeiNum1.ReadOnly = true;
                    this.ReImeiNum2.ReadOnly = true;
                    this.ReImeiNum1.Clear();
                    this.ReImeiNum2.Clear();

                }

                this.PrintOne.Checked = false;
                this.PrintNum.ReadOnly = false;
                this.IMEI_Start.ReadOnly = true;
                this.IMEI_Start.Clear();
                this.PrintNum.Focus();
            }
            else
            {
                this.PrintMore.Checked = true;
                this.PrintNum.Focus();
            }
        }

        //选择逐个重打引发的事件
        private void RePrintOne_Click(object sender, EventArgs e)
        {
            if (this.RePrintOne.Checked == true)
            {
                this.CheckIMEI2.Enabled = true;
                this.CheckSIM.Enabled = true;
                this.CheckBAT.Enabled = true;
                this.CheckICCID.Enabled = true;
                this.CheckMAC.Enabled = true;
                this.CheckEquipment.Enabled = true;
                this.CheckVIP.Enabled = true;
                this.CheckRFID.Enabled = true;

                this.RelationSN.Enabled = true;
                this.RelationSIM.Enabled = true;
                this.RelationICCID.Enabled = true;
                this.RelationBAT.Enabled = true;
                this.RelationMAC.Enabled = true;
                this.RelationEquipment.Enabled = true;
                this.RelationVIP.Enabled = true;
                this.RelationRFID.Enabled = true;

                if(this.PrintMore.Checked == true)
                {
                    this.PrintMore.Checked = false;
                    this.PrintNum.ReadOnly = true;
                    this.PrintNum.Clear();
                    this.IMEI_Start.Focus();
                }
                
                if(this.PrintOne.Checked == true)
                {
                    this.PrintOne.Checked = false;
                    this.IMEI_Start.ReadOnly = true;
                    this.IMEI_Start.Clear();
                }

                this.RePrintMore.Checked = false;
                this.Re_IMEINum.ReadOnly = false;
                this.ReImeiNum1.ReadOnly = true;
                this.ReImeiNum2.ReadOnly = true;
                this.ReImeiNum1.Clear();
                this.ReImeiNum2.Clear();
                this.Re_IMEINum.Focus();
            }
            else
            {
                this.RePrintOne.Checked = true;
                this.Re_IMEINum.Focus();
            }
        }

        //选择批量重打引发的事件
        private void RePrintMore_Click(object sender, EventArgs e)
        {
            if (this.RePrintMore.Checked == true)
            {

                if(this.PrintOne.Checked == true)
                {
                    this.PrintOne.Checked = false;
                    this.IMEI_Start.Clear();
                    this.IMEI_Start.ReadOnly = true;
                }
                if(this.PrintMore.Checked == true)
                {
                    this.PrintMore.Checked = false;
                    this.PrintNum.Clear();
                    this.PrintNum.ReadOnly = true;
                }

                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;

                this.RelationSN.Enabled = false;
                this.RelationSIM.Enabled = false;
                this.RelationICCID.Enabled = false;
                this.RelationBAT.Enabled = false;
                this.RelationMAC.Enabled = false;
                this.RelationEquipment.Enabled = false;
                this.RelationVIP.Enabled = false;
                this.RelationRFID.Enabled = false;

                this.CheckIMEI2.Checked = false;
                this.CheckSIM.Checked = false;
                this.CheckBAT.Checked = false;
                this.CheckICCID.Checked = false;
                this.CheckMAC.Checked = false;
                this.CheckEquipment.Checked = false;
                this.CheckVIP.Checked = false;
                this.CheckRFID.Checked = false;

                this.RelationSN.Checked = false;
                this.RelationSIM.Checked = false;
                this.RelationICCID.Checked = false;
                this.RelationBAT.Checked = false;
                this.RelationMAC.Checked = false;
                this.RelationEquipment.Checked = false;
                this.RelationVIP.Checked = false;
                this.RelationRFID.Checked = false;

                this.RePrintOne.Checked = false;
                this.Re_IMEINum.ReadOnly = true;
                this.ReImeiNum1.ReadOnly = false;
                this.ReImeiNum2.ReadOnly = false;
                this.Re_IMEINum.Clear();
                this.ReImeiNum1.Focus();
            }
            else
            {
                this.RePrintMore.Checked = true;
                this.ReImeiNum1.Focus();

            }
        }

        //选择重打16进制引发的事件
        private void RePrintHex_Click(object sender, EventArgs e)
        {
            if(this.RePrintHex.Checked == false)
            {
                this.Re_Nocheckcode.Enabled = true;
                this.Re_Nocheckcode.Checked = false;
            }
            else
            {
                this.Re_Nocheckcode.Enabled = false;
                this.Re_Nocheckcode.Checked = true;
            }
        }

        //非十六进制批量打印
        private void PrintNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.UserShow.Text == "")
                {
                    this.reminder.AppendText("请先登录\r\n");
                    return;
                }

                if (this.CB_ZhiDan.Text == "")
                {
                    player2.Play();
                    this.reminder.AppendText("请先选择制单\r\n");
                    this.PrintNum.Clear();
                    this.PrintNum.Focus();
                    return;
                }
                if (this.Select_Template1.Text == "")
                {
                    player1.Play();
                    this.reminder.AppendText("请先选择模板\r\n");
                    this.PrintNum.Clear();
                    this.PrintNum.Focus();
                    return;
                }
                if (this.StartZhiDan == 0 )
                {
                    this.reminder.AppendText("请获取制单数据\r\n");
                    this.PrintNum.Clear();
                    this.PrintNum.Focus();
                    return;
                }
                if (this.ToLock.Enabled == true)
                {
                    this.reminder.AppendText("请锁定\r\n");
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.Focus();
                    return;
                }
                if (this.PrintNum.Text != "" && IsNumeric(this.PrintNum.Text))
                {
                    if (this.NoCheckCode.Checked == false)
                    {
                        long between;
                        if (this.IMEI_Present.Text == "")
                        {
                            between = long.Parse(this.IMEI_num2.Text) - long.Parse(this.IMEI_num1.Text) + 1;
                        }
                        else
                        {
                            between = long.Parse(this.IMEI_num2.Text) - long.Parse(this.IMEI_Present.Text);
                        }
                        if (int.Parse(this.PrintNum.Text) < 0 || int.Parse(this.PrintNum.Text) > between)
                        {
                            player.Play();
                            this.reminder.AppendText(this.PrintNum.Text + "超出范围\r\n");
                            this.PrintNum.Clear();
                            this.PrintNum.Focus();
                            return;
                        }
                    }
                    else
                    {
                        long Imei1Suf;
                        ImeiDig = this.IMEI_num1.Text.Length;
                        if (this.IMEI_Present.Text == "")
                        {
                            Imei1Suf = long.Parse(this.IMEI_num1.Text.Remove(0, ImeiDig - 5));
                        }
                        else
                        {
                            Imei1Suf = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                        }
                        long Imei2Suf = long.Parse(this.IMEI_num2.Text.Remove(0, ImeiDig - 5));
                        long between = Imei2Suf - Imei1Suf + 1;
                        if (int.Parse(this.PrintNum.Text) < 0 || int.Parse(this.PrintNum.Text) > between)
                        {
                            player.Play();
                            this.reminder.AppendText(this.PrintNum.Text + "超出范围\r\n");
                            this.PrintNum.Clear();
                            this.PrintNum.Focus();
                            return;
                        }
                    }
                }
                else if (this.PrintNum.Text == "")
                {
                    this.PrintNum.Focus();
                    return;
                }
                else
                {
                    player.Play();
                    this.reminder.AppendText("请输入数字\r\n");
                    this.PrintNum.Clear();
                    this.PrintNum.Focus();
                    return;
                }
                try
                {
                    if(!btEngine.IsAlive)
                    {
                        btEngine.Start();
                        Form1.Log("开启打印机发动机\r\n", null);
                    }
                    LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                    //对模板相应字段进行赋值
                    ValueToTemplate(btFormat);
                    //指定打印机名称
                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                    //打印份数,同序列打印的份数
                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                    switch (c1 + c2 + c3)
                    {
                        case 4:
                            {
                                long imei_begin;
                                string imei15, sn_aft;
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                }
                                else
                                {
                                    imei_begin = long.Parse(this.IMEI_num1.Text);
                                }
                                sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                imei15 = getimei15(imei_begin.ToString());
                                string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15((imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                list = PMB.CheckRangeIMEIBLL(imei_begin.ToString() + imei15, EndIMEI);
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI.Substring(0, 14) + "重号\r\n");
                                    }
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }
                                for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                {
                                    imei15 = getimei15(imei_begin.ToString());
                                    btFormat.SubStrings["IMEI"].Value = imei_begin.ToString() + imei15;
                                    //记录打印信息日志
                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                    PList.Claer();
                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                    PList.IMEI = imei_begin.ToString() + imei15;
                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                    PList.SN = "";
                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                    PList.SIM = "";
                                    PList.VIP = "";
                                    PList.BAT = "";
                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                    PList.Remark = this.Remake.Text.Trim();
                                    PList.JS_PrintTime = ProductTime;
                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                    PList.CH_PrintTime = "";
                                    PList.CH_TemplatePath1 = null;
                                    PList.CH_TemplatePath2 = null;
                                    PList.ICCID = "";
                                    PList.MAC = "";
                                    PList.Equipment = "";
                                    PList.JSUserName = this.UserShow.Text;
                                    if (PMB.InsertPrintMessageBLL(PList))
                                    {
                                        btFormat.Print();
                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                        imei_begin++;
                                    }
                                }
                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (imei_begin - 1).ToString()))
                                {
                                    this.IMEI_Present.Text = (imei_begin - 1).ToString();
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                }
                            }
                            break;
                        case 0:
                            {
                                long imei_begin;
                                string imei15, sn_bef, sn_aft, sn_laf;
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                }
                                else
                                {
                                    imei_begin = long.Parse(this.IMEI_num1.Text);
                                }
                                imei15 = getimei15(imei_begin.ToString());
                                string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15((imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                list = PMB.CheckRangeIMEIBLL(imei_begin.ToString() + imei15, EndIMEI);
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI.Substring(0, 14) + "重号\r\n");
                                    }
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }
                                if (this.SN1_num.Text != "")
                                {
                                    if (this.SNHex.Checked == false)
                                    {
                                        sn_bef = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn_aft = this.SN1_num.Text.Remove(0, this.SN1_num.Text.Length - s);
                                        sn_laf = this.SN2_num.Text.Remove(0, this.SN2_num.Text.Length - s);
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (int.Parse(sn_aft) < int.Parse(sn_laf))
                                            {
                                                imei15 = getimei15(imei_begin.ToString());
                                                btFormat.SubStrings["IMEI"].Value = imei_begin.ToString() + imei15;
                                                btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                //记录打印信息日志
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = imei_begin.ToString() + imei15;
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = sn_bef + sn_aft;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = "";
                                                PList.VIP = "";
                                                PList.BAT = "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = "";
                                                PList.MAC = "";
                                                PList.Equipment = "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    btFormat.Print();
                                                    //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                    imei_begin++;
                                                    sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (imei_begin - 1).ToString()))
                                        {
                                            this.SN1_num.Text = sn_bef + sn_aft;
                                            this.IMEI_Present.Text = (imei_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                    else
                                    {
                                        string SNHexNum= this.SN1_num.Text;
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (SNHexNum.CompareTo(SN2_num.Text)==-1)
                                            {
                                                imei15 = getimei15(imei_begin.ToString());
                                                btFormat.SubStrings["IMEI"].Value = imei_begin.ToString() + imei15;
                                                btFormat.SubStrings["SN"].Value = SNHexNum;
                                                //记录打印信息日志
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = imei_begin.ToString() + imei15;
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = SNHexNum;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = "";
                                                PList.VIP = "";
                                                PList.BAT = "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = "";
                                                PList.MAC = "";
                                                PList.Equipment = "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    btFormat.Print();
                                                    //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                    imei_begin++;
                                                    SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (imei_begin - 1).ToString()))
                                        {
                                            this.SN1_num.Text = SNHexNum;
                                            this.IMEI_Present.Text = (imei_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        imei15 = getimei15(imei_begin.ToString());
                                        btFormat.SubStrings["IMEI"].Value = imei_begin.ToString() + imei15;
                                        btFormat.SubStrings["SN"].Value = "";
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = imei_begin.ToString() + imei15;
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = "";
                                        PList.VIP = "";
                                        PList.BAT = "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.UserName = this.UserShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                            imei_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (imei_begin - 1).ToString()))
                                    {
                                        this.IMEI_Present.Text = (imei_begin - 1).ToString();
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                }
                            }
                            break;
                        case 1:
                            {
                                long imei_begin;
                                string imei15, sn_bef, sn_aft, sn_laf;
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                }
                                else
                                {
                                    imei_begin = long.Parse(this.IMEI_num1.Text);
                                }
                                imei15 = getimei15(imei_begin.ToString());
                                string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15((imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                list = PMB.CheckRangeIMEIBLL(imei_begin.ToString() + imei15, EndIMEI);
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI.Substring(0, 14) + "重号\r\n");
                                    }
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }
                                if (this.SN1_num.Text != "")
                                {
                                    if (this.SNHex.Checked == false)
                                    {
                                        sn_bef = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn_aft = this.SN1_num.Text.Remove(0, this.SN1_num.Text.Length - s);
                                        sn_laf = this.SN2_num.Text.Remove(0, this.SN2_num.Text.Length - s);
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (int.Parse(sn_aft) < int.Parse(sn_laf))
                                            {
                                                imei15 = getimei15(imei_begin.ToString());
                                                btFormat.SubStrings["IMEI"].Value = imei_begin.ToString() + imei15;
                                                if (!PMB.CheckSNBLL(sn_bef + sn_aft))
                                                {
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin.ToString() + imei15;
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    PList.SN = sn_bef + sn_aft;
                                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                    PList.SIM = "";
                                                    PList.VIP = "";
                                                    PList.BAT = "";
                                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                                    PList.Remark = this.Remake.Text.Trim();
                                                    PList.JS_PrintTime = ProductTime;
                                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                    PList.CH_PrintTime = "";
                                                    PList.CH_TemplatePath1 = null;
                                                    PList.CH_TemplatePath2 = null;
                                                    PList.ICCID = "";
                                                    PList.MAC = "";
                                                    PList.Equipment = "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin++;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    //player.Play();
                                                    sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    i--;
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (imei_begin - 1).ToString()))
                                        {
                                            this.SN1_num.Text = sn_bef + sn_aft;
                                            this.IMEI_Present.Text = (imei_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                    else
                                    {
                                        string SNHexNum = this.SN1_num.Text;
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (SNHexNum.CompareTo(SN2_num.Text) == 1)
                                            {
                                                imei15 = getimei15(imei_begin.ToString());
                                                btFormat.SubStrings["IMEI"].Value = imei_begin.ToString() + imei15;
                                                if (!PMB.CheckSNBLL(SNHexNum))
                                                {
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin.ToString() + imei15;
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    PList.SN = SNHexNum;
                                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                    PList.SIM = "";
                                                    PList.VIP = "";
                                                    PList.BAT = "";
                                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                                    PList.Remark = this.Remake.Text.Trim();
                                                    PList.JS_PrintTime = ProductTime;
                                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                    PList.CH_PrintTime = "";
                                                    PList.CH_TemplatePath1 = null;
                                                    PList.CH_TemplatePath2 = null;
                                                    PList.ICCID = "";
                                                    PList.MAC = "";
                                                    PList.Equipment = "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin++;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    //player.Play();
                                                    SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    i--;
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (imei_begin - 1).ToString()))
                                        {
                                            this.SN1_num.Text = SNHexNum;
                                            this.IMEI_Present.Text = (imei_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        imei15 = getimei15(imei_begin.ToString());
                                        btFormat.SubStrings["IMEI"].Value = imei_begin.ToString() + imei15;
                                        btFormat.SubStrings["SN"].Value = "";
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = imei_begin.ToString() + imei15;
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = "";
                                        PList.VIP = "";
                                        PList.BAT = "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.JSUserName = this.UserShow.Text;

                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                            imei_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (imei_begin - 1).ToString()))
                                    {
                                        this.IMEI_Present.Text = (imei_begin - 1).ToString();
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                }
                            }
                            break;
                        case 2:
                            {
                                long imei_begin;
                                string sn_bef, sn_aft, sn_laf;
                                string imei_begin_pre = this.IMEI_num1.Text.Substring(0, ImeiDig - 5);
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                }
                                else
                                {
                                    imei_begin = long.Parse(this.IMEI_num1.Text.Remove(0, ImeiDig - 5));
                                }
                                string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                list = PMB.CheckRangeIMEIBLL(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0'));
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI + "重号\r\n");
                                    }
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }
                                if (this.SN1_num.Text != "")
                                {
                                    if (this.SNHex.Checked == false)
                                    {
                                        sn_bef = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn_aft = this.SN1_num.Text.Remove(0, this.SN1_num.Text.Length - s);
                                        sn_laf = this.SN2_num.Text.Remove(0, this.SN2_num.Text.Length - s);
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (int.Parse(sn_aft) < int.Parse(sn_laf))
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                //记录打印信息日志
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = sn_bef + sn_aft;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = "";
                                                PList.VIP = "";
                                                PList.BAT = "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = "";
                                                PList.MAC = "";
                                                PList.Equipment = "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    btFormat.Print();
                                                    //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                    imei_begin++;
                                                    sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.SN1_num.Text = sn_bef + sn_aft;
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                    else
                                    {
                                        string SNHexNum = this.SN1_num.Text;
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (SNHexNum.CompareTo(SN2_num.Text) == -1)
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                btFormat.SubStrings["SN"].Value = SNHexNum;
                                                //记录打印信息日志
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = SNHexNum;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = "";
                                                PList.VIP = "";
                                                PList.BAT = "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = "";
                                                PList.MAC = "";
                                                PList.Equipment = "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    btFormat.Print();
                                                    //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                    imei_begin++;
                                                    SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.SN1_num.Text = SNHexNum;
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                        btFormat.SubStrings["SN"].Value = "";
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = "";
                                        PList.VIP = "";
                                        PList.BAT = "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.JSUserName = this.UserShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                    {
                                        this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                }
                            }
                            break;
                        case 3:
                            {
                                long imei_begin;
                                string sn_bef, sn_aft, sn_laf;
                                string imei_begin_pre = this.IMEI_num1.Text.Substring(0, ImeiDig - 5);
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                }
                                else
                                {
                                    imei_begin = long.Parse(this.IMEI_num1.Text.Remove(0, ImeiDig - 5));
                                }
                                string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                list = PMB.CheckRangeIMEIBLL(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0'));
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI + "重号\r\n");
                                    }
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }
                                if (this.SN1_num.Text != "")
                                {
                                    if (this.SNHex.Checked == false)
                                    {
                                        sn_bef = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn_aft = this.SN1_num.Text.Remove(0, this.SN1_num.Text.Length - s);
                                        sn_laf = this.SN2_num.Text.Remove(0, this.SN2_num.Text.Length - s);
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (int.Parse(sn_aft) < int.Parse(sn_laf))
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                if (!PMB.CheckSNBLL(sn_bef + sn_aft))
                                                {
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    PList.SN = sn_bef + sn_aft;
                                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                    PList.SIM = "";
                                                    PList.VIP = "";
                                                    PList.BAT = "";
                                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                                    PList.Remark = this.Remake.Text.Trim();
                                                    PList.JS_PrintTime = ProductTime;
                                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                    PList.CH_PrintTime = "";
                                                    PList.CH_TemplatePath1 = null;
                                                    PList.CH_TemplatePath2 = null;
                                                    PList.ICCID = "";
                                                    PList.MAC = "";
                                                    PList.Equipment = "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin++;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    //player.Play();
                                                    sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    i--;
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.SN1_num.Text = sn_bef + sn_aft;
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                    else
                                    {
                                        string SNHexNum = this.SN1_num.Text;
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            if (SNHexNum.CompareTo(SN2_num.Text) == -1)
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                if (!PMB.CheckSNBLL(SNHexNum))
                                                {
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    PList.SN = SNHexNum;
                                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                    PList.SIM = "";
                                                    PList.VIP = "";
                                                    PList.BAT = "";
                                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                                    PList.Remark = this.Remake.Text.Trim();
                                                    PList.JS_PrintTime = ProductTime;
                                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                    PList.CH_PrintTime = "";
                                                    PList.CH_TemplatePath1 = null;
                                                    PList.CH_TemplatePath2 = null;
                                                    PList.ICCID = "";
                                                    PList.MAC = "";
                                                    PList.Equipment = "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin++;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    //player.Play();
                                                    SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    i--;
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.SN1_num.Text = SNHexNum;
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                        btFormat.SubStrings["SN"].Value = "";
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = "";
                                        PList.VIP = "";
                                        PList.BAT = "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.JSUserName = this.UserShow.Text;

                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                    {
                                        this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                }
                            }
                            break;
                        case 6:
                            {
                                long imei_begin;
                                string sn_aft;
                                sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                string imei_begin_pre = this.IMEI_num1.Text.Substring(0, ImeiDig - 5);
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                }
                                else
                                {
                                    imei_begin = long.Parse(this.IMEI_num1.Text.Remove(0, ImeiDig - 5));
                                }
                                string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                list = PMB.CheckRangeIMEIBLL(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0'));
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI + "重号\r\n");
                                    }
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }
                                for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                {
                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                    //记录打印信息日志
                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                    PList.Claer();
                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                    PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                    PList.SN = "";
                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                    PList.SIM = "";
                                    PList.VIP = "";
                                    PList.BAT = "";
                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                    PList.Remark = this.Remake.Text.Trim();
                                    PList.JS_PrintTime = ProductTime;
                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                    PList.CH_PrintTime = "";
                                    PList.CH_TemplatePath1 = null;
                                    PList.CH_TemplatePath2 = null;
                                    PList.ICCID = "";
                                    PList.MAC = "";
                                    PList.Equipment = "";
                                    PList.JSUserName = this.UserShow.Text;
                                    if (PMB.InsertPrintMessageBLL(PList))
                                    {
                                        btFormat.Print();
                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                        imei_begin++;
                                    }
                                }
                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                {
                                    this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:" + ex.Message);
                }
            }
        }

        //十六进制批量打印
        private void HexPrintNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.UserShow.Text == "")
                {
                    this.reminder.AppendText("请先登录\r\n");
                    return;
                }
                if (this.CB_ZhiDan.Text == "")
                {
                    player2.Play();
                    this.reminder.AppendText("请先选择制单\r\n");
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.Focus();
                    return;
                }
                if (this.Select_Template1.Text == "")
                {
                    player1.Play();
                    this.reminder.AppendText("请先选择模板\r\n");
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.Focus();
                }
                if (this.StartZhiDan == 0)
                {
                    player1.Play();
                    this.reminder.AppendText("请获取制单数据\r\n");
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.Focus();
                    return;
                }
                if (this.ToLock.Enabled == true)
                {
                    this.reminder.AppendText("请锁定\r\n");
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.Focus();
                    return;
                }
                if (this.HexPrintNum.Text != "" && IsNumeric(this.HexPrintNum.Text))
                {
                    long HexBetween;
                    long HexNum1;
                    if (this.IMEI_Present.Text == "")
                    {
                        HexNum1 = Convert.ToInt64(this.IMEI_num1.Text, 16);
                    }
                    else
                    {
                        HexNum1 = Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16);
                    }
                    HexBetween = Convert.ToInt64(this.IMEI_num2.Text, 16) - HexNum1 + Convert.ToInt64("1", 16);
                    if (int.Parse(this.HexPrintNum.Text) < 0 || int.Parse(this.HexPrintNum.Text) > HexBetween)
                    {
                        player.Play();
                        this.reminder.AppendText(this.HexPrintNum.Text + "超出范围\r\n");
                        this.HexPrintNum.Clear();
                        this.HexPrintNum.Focus();
                        return;
                    }
                }
                else if (this.HexPrintNum.Text == "")
                {
                    this.HexPrintNum.Focus();
                    return;
                }
                else
                {
                    player.Play();
                    this.reminder.AppendText("请输入数字\r\n");
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.Focus();
                    return;
                }
                try
                {
                    if (!btEngine.IsAlive)
                    {
                        btEngine.Start();
                        Form1.Log("开启打印机发动机2\r\n", null);
                    }
                    LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                    //对模板相应字段进行赋值
                    ValueToTemplate(btFormat);
                    //指定打印机名称
                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                    //打印份数,同序列打印的份数
                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                    switch (c1 + c2 + c3)
                    {
                        case 2:
                            {
                                string imei_begin;
                                string sn_bef, sn_aft, sn_laf;
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = (Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                }
                                else
                                {
                                    imei_begin = this.IMEI_num1.Text;
                                }
                                long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                list = PMB.CheckRangeIMEIBLL(imei_begin, EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0'));
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI + "重号\r\n");
                                    }
                                    this.HexPrintNum.Clear();
                                    this.HexPrintNum.Focus();
                                    return;
                                }
                                if (this.SN1_num.Text != "")
                                {
                                    if (this.SNHex.Checked == false)
                                    {
                                        sn_bef = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn_aft = this.SN1_num.Text.Remove(0, this.SN1_num.Text.Length - s);
                                        sn_laf = this.SN2_num.Text.Remove(0, this.SN2_num.Text.Length - s);
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {
                                            if (int.Parse(sn_aft) < int.Parse(sn_laf))
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                //记录打印信息日志
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = imei_begin;
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = sn_bef + sn_aft;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = "";
                                                PList.VIP = "";
                                                PList.BAT = "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = "";
                                                PList.MAC = "";
                                                PList.Equipment = "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    btFormat.Print();
                                                    //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                    imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                    sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                        {
                                            this.SN1_num.Text = sn_bef + sn_aft;
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                    }
                                    else
                                    {
                                        string SNHexNum = this.SN1_num.Text;
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {
                                            if (SNHexNum.CompareTo(SN2_num.Text) == -1)
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                btFormat.SubStrings["SN"].Value = SNHexNum;
                                                //记录打印信息日志
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = imei_begin;
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = SNHexNum;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = "";
                                                PList.VIP = "";
                                                PList.BAT = "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = "";
                                                PList.MAC = "";
                                                PList.Equipment = "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    btFormat.Print();
                                                    //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                    imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                    SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                        {
                                            this.SN1_num.Text = SNHexNum;
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                    {
                                        btFormat.SubStrings["IMEI"].Value = imei_begin;
                                        btFormat.SubStrings["SN"].Value = "";
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = imei_begin;
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = "";
                                        PList.VIP = "";
                                        PList.BAT = "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.JSUserName = this.UserShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                    }
                                }
                            }
                            break;
                        case 3:
                            {
                                string imei_begin;
                                string sn_bef, sn_aft, sn_laf;
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = (Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                }
                                else
                                {
                                    imei_begin = this.IMEI_num1.Text;
                                }
                                long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                list = PMB.CheckRangeIMEIBLL(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0'));
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI + "重号\r\n");
                                    }
                                    this.HexPrintNum.Clear();
                                    this.HexPrintNum.Focus();
                                    return;
                                }
                                if (this.SN1_num.Text != "")
                                {
                                    if (this.SNHex.Checked == false)
                                    {
                                        sn_bef = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn_aft = this.SN1_num.Text.Remove(0, this.SN1_num.Text.Length - s);
                                        sn_laf = this.SN2_num.Text.Remove(0, this.SN2_num.Text.Length - s);
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {
                                            if (int.Parse(sn_aft) < int.Parse(sn_laf))
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                if (!PMB.CheckSNBLL(sn_bef + sn_aft))
                                                {
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin;
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    PList.SN = sn_bef + sn_aft;
                                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                    PList.SIM = "";
                                                    PList.VIP = "";
                                                    PList.BAT = "";
                                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                                    PList.Remark = this.Remake.Text.Trim();
                                                    PList.JS_PrintTime = ProductTime;
                                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                    PList.CH_PrintTime = "";
                                                    PList.CH_TemplatePath1 = null;
                                                    PList.CH_TemplatePath2 = null;
                                                    PList.ICCID = "";
                                                    PList.MAC = "";
                                                    PList.Equipment = "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    //player.Play();
                                                    sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    i--;
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                        {
                                            this.SN1_num.Text = sn_bef + sn_aft;
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                    }
                                    else
                                    {
                                        string SNHexNum = this.SN1_num.Text;
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {
                                            if (SNHexNum.CompareTo(SN2_num.Text) == -1)
                                            {
                                                btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                if (!PMB.CheckSNBLL(SNHexNum))
                                                {
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin;
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    PList.SN = SNHexNum;
                                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                    PList.SIM = "";
                                                    PList.VIP = "";
                                                    PList.BAT = "";
                                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                                    PList.Remark = this.Remake.Text.Trim();
                                                    PList.JS_PrintTime = ProductTime;
                                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                    PList.CH_PrintTime = "";
                                                    PList.CH_TemplatePath1 = null;
                                                    PList.CH_TemplatePath2 = null;
                                                    PList.ICCID = "";
                                                    PList.MAC = "";
                                                    PList.Equipment = "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    //player.Play();
                                                    SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    i--;
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("SN号不足");
                                                return;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                        {
                                            this.SN1_num.Text = SNHexNum;
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                    {
                                        btFormat.SubStrings["IMEI"].Value = imei_begin;
                                        btFormat.SubStrings["SN"].Value = "";
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = imei_begin;
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = "";
                                        PList.VIP = "";
                                        PList.BAT = "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.JSUserName = this.UserShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                    }
                                }
                            }
                            break;
                        case 6:
                            {
                                string imei_begin;
                                string sn_aft;
                                sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                if (this.IMEI_Present.Text != "")
                                {
                                    imei_begin = (Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                }
                                else
                                {
                                    imei_begin = this.IMEI_num1.Text;
                                }
                                long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                list = PMB.CheckRangeIMEIBLL(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0'));
                                if (list.Count > 0)
                                {
                                    foreach (PrintMessage a in list)
                                    {
                                        this.reminder.AppendText(a.IMEI + "重号\r\n");
                                    }
                                    this.HexPrintNum.Clear();
                                    this.HexPrintNum.Focus();
                                    return;
                                }
                                for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                {
                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                    //记录打印信息日志
                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                    PList.Claer();
                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                    PList.IMEI = imei_begin;
                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                    PList.SN = "";
                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                    PList.SIM = "";
                                    PList.VIP = "";
                                    PList.BAT = "";
                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                    PList.Remark = this.Remake.Text.Trim();
                                    PList.JS_PrintTime = ProductTime;
                                    PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                    PList.CH_PrintTime = "";
                                    PList.CH_TemplatePath1 = null;
                                    PList.CH_TemplatePath2 = null;
                                    PList.ICCID = "";
                                    PList.MAC = "";
                                    PList.Equipment = "";
                                    PList.JSUserName = this.UserShow.Text;
                                    if (PMB.InsertPrintMessageBLL(PList))
                                    {
                                        btFormat.Print();
                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                    }
                                }
                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                {
                                    this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                    this.HexPrintNum.Clear();
                                    this.HexPrintNum.Focus();
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:" + ex.Message);
                }
            }
        }

        //单个打印
        private void IMEI_Start_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.UserShow.Text == "")
                {
                    this.reminder.AppendText("请先登录\r\n");
                    return;
                }
                if (this.CB_ZhiDan.Text == "")
                {
                    player2.Play();
                    this.reminder.AppendText("请先选择制单\r\n");
                    this.IMEI_Start.Clear();
                    this.IMEI_Start.Focus();
                    return;
                }
                if (this.StartZhiDan == 0)
                {
                    this.reminder.AppendText("请获取制单数据\r\n");
                    this.IMEI_Start.Clear();
                    this.IMEI_Start.Focus();
                    return;
                }

                if (this.ToLock.Enabled == true)
                {
                    this.reminder.AppendText("请锁定\r\n");
                    this.IMEI_Start.Clear();
                    this.IMEI_Start.Focus();
                    return;
                }

                if (this.CB_ZhiDan.Text != "")
                {

                    if (this.NoCheckCode.Checked == false)
                    {
                        string imei14;
                        String imeiRes = "";
                        if (this.IMEI_Start.Text != "" && IsNumeric(this.IMEI_Start.Text) && this.IMEI_Start.Text.Length == 15)
                        {
                            imei14 = this.IMEI_Start.Text.Substring(0, 14);
                            long IMEI_Start = long.Parse(imei14);
                            if (IMEI_Start < long.Parse(this.IMEI_num1.Text))
                            {
                                player.Play();
                                this.reminder.AppendText(IMEI_Start + "IMEI不在范围内\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                            else if (IMEI_Start > long.Parse(this.IMEI_num2.Text))
                            {
                                player.Play();
                                this.reminder.AppendText(IMEI_Start + "IMEI不在范围内\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                            else
                            {
                                string imei15 = getimei15(imei14);
                                imeiRes = imei14 + imei15;
                                if (imeiRes != this.IMEI_Start.Text)
                                {
                                    player3.Play();
                                    this.reminder.AppendText(this.IMEI_Start.Text + "IMEI校验错误\r\n");
                                    this.IMEI_Start.Clear();
                                    this.IMEI_Start.Focus();
                                    return;
                                }
                            }
                        }
                        else if (this.IMEI_Start.Text == "")
                        {
                            player.Play();
                            this.reminder.AppendText("请输入IMEI\r\n");
                            this.IMEI_Start.Focus();
                            return;
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI格式错误\r\n");
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else
                    {

                        if (this.IMEI_Start.Text != "")
                        {
                            if (this.IMEI_Start.Text.Length != this.IMEI_num1.Text.Length)
                            {
                                this.reminder.AppendText("IMEI号位数与起始位数不一致\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                            if (this.IMEI_Start.Text.CompareTo(this.IMEI_num1.Text) == -1 || this.IMEI_Start.Text.CompareTo(this.IMEI_num2.Text) == 1)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI不在范围内\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                        }
                        else
                        {
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                }
                else
                {
                    player2.Play();
                    this.reminder.AppendText("请选择制单号\r\n");
                    this.IMEI_Start.Clear();
                    this.IMEI_Start.Focus();
                    return;
                }
                try
                {
                    if (this.Select_Template1.Text != "")
                    {
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        //指定打印机名称
                        btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                        //打印份数,同序列打印的份数
                        btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                        switch (c1 + c2 + c3)
                        {
                            //不打印SN号
                            case 4:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    if (a.IMEI2 == "")
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 == "")
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 == "")
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 == "")
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 == "")
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 == "")
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 == "")
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckRFID.Checked == true)
                                                {
                                                    if (a.RFID == "")
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (RelationFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                //if (this.RelationSN.Checked == true)
                                                //{
                                                //    if (a.IMEI2 != "")
                                                //    {
                                                //        btFormat.SubStrings["SN"].Value = a.IMEI2;
                                                //        snstr = a.IMEI2;
                                                //    }
                                                //    else
                                                //    {
                                                //        this.reminder.AppendText("SN号为空\r\n");
                                                //        player.Play();
                                                //        this.IMEI_Start.Clear();
                                                //        this.IMEI_Start.Focus();
                                                //        return;
                                                //    }
                                                //}

                                                if (this.RelationSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 != "")
                                                    {
                                                        btFormat.SubStrings["SIM"].Value = a.IMEI3;
                                                        simstr = a.IMEI3;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 != "")
                                                    {
                                                        btFormat.SubStrings["ICCID"].Value = a.IMEI4;
                                                        iccidstr = a.IMEI4;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 != "")
                                                    {
                                                        btFormat.SubStrings["MAC"].Value = a.IMEI6;
                                                        macstr = a.IMEI6;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 != "")
                                                    {
                                                        btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                                                        equistr = a.IMEI7;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }

                                                }

                                                if (this.RelationVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 != "")
                                                    {
                                                        btFormat.SubStrings["VIP"].Value = a.IMEI8;
                                                        vipstr = a.IMEI8;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 != "")
                                                    {
                                                        btFormat.SubStrings["BAT"].Value = a.IMEI9;
                                                        batstr = a.IMEI9;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationRFID.Checked == true)
                                                {
                                                    if (a.RFID != "")
                                                    {
                                                        btFormat.SubStrings["RFID"].Value = a.RFID;
                                                        rfidstr = a.RFID;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        //对模板相应字段进行赋值
                                        ValueToTemplate(btFormat);
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = simstr != "" ? simstr : "";
                                        PList.VIP = vipstr != "" ? vipstr : "";
                                        PList.BAT = batstr != "" ? batstr : "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.ICCID = iccidstr != "" ? iccidstr : "";
                                        PList.MAC = macstr != "" ? macstr : "";
                                        PList.Equipment = equistr != "" ? equistr : "";
                                        PList.RFID = rfidstr != "" ? rfidstr : "";
                                        PList.JSUserName = this.UserShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn1_suffix.ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                            {
                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                this.IMEI_Present.Text = imei_star14.ToString();
                                                this.IMEI_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //没有客供，打印校验码和SN号
                            case 0:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    if (a.IMEI2 == "")
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 == "")
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 == "")
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 == "")
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 == "")
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 == "")
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 == "")
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckRFID.Checked == true)
                                                {
                                                    if (a.RFID == "")
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (RelationFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.RelationSN.Checked == true)
                                                {
                                                    if (a.IMEI2 != "")
                                                    {
                                                        //btFormat.SubStrings["SN"].Value = a.IMEI2;
                                                        //snstr = a.IMEI2;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 != "")
                                                    {
                                                        btFormat.SubStrings["SIM"].Value = a.IMEI3;
                                                        simstr = a.IMEI3;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 != "")
                                                    {
                                                        btFormat.SubStrings["ICCID"].Value = a.IMEI4;
                                                        iccidstr = a.IMEI4;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 != "")
                                                    {
                                                        btFormat.SubStrings["MAC"].Value = a.IMEI6;
                                                        macstr = a.IMEI6;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 != "")
                                                    {
                                                        btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                                                        equistr = a.IMEI7;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }

                                                }

                                                if (this.RelationVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 != "")
                                                    {
                                                        btFormat.SubStrings["VIP"].Value = a.IMEI8;
                                                        vipstr = a.IMEI8;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 != "")
                                                    {
                                                        btFormat.SubStrings["BAT"].Value = a.IMEI9;
                                                        batstr = a.IMEI9;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationRFID.Checked == true)
                                                {
                                                    if (a.RFID != "")
                                                    {
                                                        btFormat.SubStrings["RFID"].Value = a.RFID;
                                                        rfidstr = a.RFID;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;

                                    if(this.RelationSN.Checked == false)
                                    {
                                        if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            if (this.SN1_num.Text != "")
                                            {
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = this.SN1_num.Text;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = simstr != "" ? simstr : "";
                                                PList.VIP = vipstr != "" ? vipstr : "";
                                                PList.BAT = batstr != "" ? batstr : "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = iccidstr != "" ? iccidstr : "";
                                                PList.MAC = macstr != "" ? macstr : "";
                                                PList.Equipment = equistr != "" ? equistr : "";
                                                PList.RFID = rfidstr != "" ? rfidstr : "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                    string sn2_suffix = SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        this.SN1_num.Text = sn1;
                                                        long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        this.IMEI_Present.Text = imei_star14.ToString();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                btFormat.SubStrings["SN"].Value = "";
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = "";
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = simstr != "" ? simstr : "";
                                                PList.VIP = vipstr != "" ? vipstr : "";
                                                PList.BAT = batstr != "" ? batstr : "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = iccidstr != "" ? iccidstr : "";
                                                PList.MAC = macstr != "" ? macstr : "";
                                                PList.Equipment = equistr != "" ? equistr : "";
                                                PList.RFID = rfidstr != "" ? rfidstr : "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        this.IMEI_Present.Text = imei_star14.ToString();
                                                    }
                                                }
                                            }
                                        }
                                        else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                        {
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                            foreach (PrintMessage a in list)
                                            {
                                                btFormat.SubStrings["SN"].Value = a.SN;
                                            }
                                            if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                            {
                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                this.IMEI_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            btFormat.SubStrings["SN"].Value = snstr;
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                            PList.SN = this.SN1_num.Text;
                                            PList.IMEIRel = this.IMEIRel.Text.Trim();
                                            PList.SIM = simstr != "" ? simstr : "";
                                            PList.VIP = vipstr != "" ? vipstr : "";
                                            PList.BAT = batstr != "" ? batstr : "";
                                            PList.SoftModel = this.SoftModel.Text.Trim();
                                            PList.Version = this.SoftwareVersion.Text.Trim();
                                            PList.Remark = this.Remake.Text.Trim();
                                            PList.JS_PrintTime = ProductTime;
                                            PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                            PList.CH_PrintTime = "";
                                            PList.CH_TemplatePath1 = null;
                                            PList.CH_TemplatePath2 = null;
                                            PList.ICCID = iccidstr != "" ? iccidstr : "";
                                            PList.MAC = macstr != "" ? macstr : "";
                                            PList.Equipment = equistr != "" ? equistr : "";
                                            PList.RFID = rfidstr != "" ? rfidstr : "";
                                            PList.JSUserName =this.UserShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                string sn1_prefix = snstr.Substring(0, snstr.Length - s);
                                                long sn1_suffix = long.Parse(snstr.Remove(0, (snstr.Length) - s));
                                                string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                string sn2_suffix = SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                    this.SN1_num.Text = sn1;
                                                    long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    this.IMEI_Present.Text = imei_star14.ToString();
                                                }
                                            }
                                        }
                                        else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                        {
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            btFormat.SubStrings["SN"].Value = snstr;
                                            if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                            {
                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                this.IMEI_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                   
                                }
                                break;

                            //客供SN
                            case 1:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    if (a.IMEI2 == "")
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 == "")
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 == "")
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 == "")
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 == "")
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 == "")
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 == "")
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckRFID.Checked == true)
                                                {
                                                    if (a.RFID == "")
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (RelationFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.RelationSN.Checked == true)
                                                {
                                                    if (a.IMEI2 != "")
                                                    {
                                                        btFormat.SubStrings["SN"].Value = a.IMEI2;
                                                        snstr = a.IMEI2;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 != "")
                                                    {
                                                        btFormat.SubStrings["SIM"].Value = a.IMEI3;
                                                        simstr = a.IMEI3;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 != "")
                                                    {
                                                        btFormat.SubStrings["ICCID"].Value = a.IMEI4;
                                                        iccidstr = a.IMEI4;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 != "")
                                                    {
                                                        btFormat.SubStrings["MAC"].Value = a.IMEI6;
                                                        macstr = a.IMEI6;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 != "")
                                                    {
                                                        btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                                                        equistr = a.IMEI7;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }

                                                }

                                                if (this.RelationVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 != "")
                                                    {
                                                        btFormat.SubStrings["VIP"].Value = a.IMEI8;
                                                        vipstr = a.IMEI8;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 != "")
                                                    {
                                                        btFormat.SubStrings["BAT"].Value = a.IMEI9;
                                                        batstr = a.IMEI9;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationRFID.Checked == true)
                                                {
                                                    if (a.RFID != "")
                                                    {
                                                        btFormat.SubStrings["RFID"].Value = a.RFID;
                                                        rfidstr = a.RFID;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        //对模板相应字段进行赋值
                                        ValueToTemplate(btFormat);
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        if (this.SN1_num.Text != "")
                                        {
                                            if (!PMB.CheckSNBLL(this.SN1_num.Text))
                                            {
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = this.SN1_num.Text;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = simstr != "" ? simstr : "";
                                                PList.VIP = vipstr != "" ? vipstr : "";
                                                PList.BAT = batstr != "" ? batstr : "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = iccidstr != "" ? iccidstr : "";
                                                PList.MAC = macstr != "" ? macstr : "";
                                                PList.Equipment = equistr != "" ? equistr : "";
                                                PList.RFID = rfidstr != "" ? rfidstr : "";
                                                PList.JSUserName =this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        this.SN1_num.Text = sn1;
                                                        long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        this.IMEI_Present.Text = imei_star14.ToString();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                this.IMEI_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = "";
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                            PList.SN = "";
                                            PList.IMEIRel = this.IMEIRel.Text.Trim();
                                            PList.SIM = simstr != "" ? simstr : "";
                                            PList.VIP = vipstr != "" ? vipstr : "";
                                            PList.BAT = batstr != "" ? batstr : "";
                                            PList.SoftModel = this.SoftModel.Text.Trim();
                                            PList.Version = this.SoftwareVersion.Text.Trim();
                                            PList.Remark = this.Remake.Text.Trim();
                                            PList.JS_PrintTime = ProductTime;
                                            PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                            PList.CH_PrintTime = "";
                                            PList.CH_TemplatePath1 = null;
                                            PList.CH_TemplatePath2 = null;
                                            PList.ICCID = iccidstr != "" ? iccidstr : "";
                                            PList.MAC = macstr != "" ? macstr : "";
                                            PList.Equipment = equistr != "" ? equistr : "";
                                            PList.RFID = rfidstr != "" ? rfidstr : "";
                                            PList.JSUserName = this.UserShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                    long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    this.IMEI_Present.Text = imei_star14.ToString();
                                                }
                                            }
                                        }
                                    }
                                    else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                    {
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                        foreach (PrintMessage a in list)
                                        {
                                            btFormat.SubStrings["SN"].Value = a.SN;
                                        }
                                        if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                        {
                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                            Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //不打印校验码
                            case 2:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    if (a.IMEI2 == "")
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 == "")
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 == "")
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 == "")
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 == "")
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 == "")
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 == "")
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckRFID.Checked == true)
                                                {
                                                    if (a.RFID == "")
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (RelationFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.RelationSN.Checked == true)
                                                {
                                                    if (a.IMEI2 != "")
                                                    {
                                                        btFormat.SubStrings["SN"].Value = a.IMEI2;
                                                        snstr = a.IMEI2;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 != "")
                                                    {
                                                        btFormat.SubStrings["SIM"].Value = a.IMEI3;
                                                        simstr = a.IMEI3;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 != "")
                                                    {
                                                        btFormat.SubStrings["ICCID"].Value = a.IMEI4;
                                                        iccidstr = a.IMEI4;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 != "")
                                                    {
                                                        btFormat.SubStrings["MAC"].Value = a.IMEI6;
                                                        macstr = a.IMEI6;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 != "")
                                                    {
                                                        btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                                                        equistr = a.IMEI7;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }

                                                }

                                                if (this.RelationVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 != "")
                                                    {
                                                        btFormat.SubStrings["VIP"].Value = a.IMEI8;
                                                        vipstr = a.IMEI8;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 != "")
                                                    {
                                                        btFormat.SubStrings["BAT"].Value = a.IMEI9;
                                                        batstr = a.IMEI9;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationRFID.Checked == true)
                                                {
                                                    if (a.RFID != "")
                                                    {
                                                        btFormat.SubStrings["RFID"].Value = a.RFID;
                                                        rfidstr = a.RFID;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        //对模板相应字段进行赋值
                                        ValueToTemplate(btFormat);
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        if (this.SN1_num.Text != "")
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                            PList.SN = this.SN1_num.Text;
                                            PList.IMEIRel = this.IMEIRel.Text.Trim();
                                            PList.SIM = simstr != "" ? simstr : "";
                                            PList.VIP = vipstr != "" ? vipstr : "";
                                            PList.BAT = batstr != "" ? batstr : "";
                                            PList.SoftModel = this.SoftModel.Text.Trim();
                                            PList.Version = this.SoftwareVersion.Text.Trim();
                                            PList.Remark = this.Remake.Text.Trim();
                                            PList.JS_PrintTime = ProductTime;
                                            PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                            PList.CH_PrintTime = "";
                                            PList.CH_TemplatePath1 = null;
                                            PList.CH_TemplatePath2 = null;
                                            PList.ICCID = iccidstr != "" ? iccidstr : "";
                                            PList.MAC = macstr != "" ? macstr : "";
                                            PList.Equipment = equistr != "" ? equistr : "";
                                            PList.RFID = rfidstr != "" ? rfidstr : "";
                                            PList.JSUserName =this.UserShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                string sn2_suffix = SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text) + 1).ToString()))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                    this.SN1_num.Text = sn1;
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = "";
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                            PList.SN = "";
                                            PList.IMEIRel = this.IMEIRel.Text.Trim();
                                            PList.SIM = simstr != "" ? simstr : "";
                                            PList.VIP = vipstr != "" ? vipstr : "";
                                            PList.BAT = batstr != "" ? batstr : "";
                                            PList.SoftModel = this.SoftModel.Text.Trim();
                                            PList.Version = this.SoftwareVersion.Text.Trim();
                                            PList.Remark = this.Remake.Text.Trim();
                                            PList.JS_PrintTime = ProductTime;
                                            PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                            PList.CH_PrintTime = "";
                                            PList.CH_TemplatePath1 = null;
                                            PList.CH_TemplatePath2 = null;
                                            PList.ICCID = iccidstr != "" ? iccidstr : "";
                                            PList.MAC = macstr != "" ? macstr : "";
                                            PList.Equipment = equistr != "" ? equistr : "";
                                            PList.RFID = rfidstr != "" ? rfidstr : "";
                                            PList.JSUserName = this.UserShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                string sn2_suffix;
                                                if (this.SN2_num.Text != "")
                                                {
                                                    sn2_suffix = SN2_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                                }
                                                else
                                                {
                                                    sn2_suffix = "";
                                                }
                                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (long.Parse(this.IMEI_Start.Text) + 1).ToString()))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                        }
                                    }
                                    else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                    {
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                        foreach (PrintMessage a in list)
                                        {
                                            btFormat.SubStrings["SN"].Value = a.SN;
                                        }
                                        if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                        {
                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                            Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //客供，不打印校验码
                            case 3:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    if (a.IMEI2 == "")
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 == "")
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 == "")
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 == "")
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 == "")
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 == "")
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 == "")
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckRFID.Checked == true)
                                                {
                                                    if (a.RFID == "")
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (RelationFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.RelationSN.Checked == true)
                                                {
                                                    if (a.IMEI2 != "")
                                                    {
                                                        btFormat.SubStrings["SN"].Value = a.IMEI2;
                                                        snstr = a.IMEI2;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 != "")
                                                    {
                                                        btFormat.SubStrings["SIM"].Value = a.IMEI3;
                                                        simstr = a.IMEI3;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 != "")
                                                    {
                                                        btFormat.SubStrings["ICCID"].Value = a.IMEI4;
                                                        iccidstr = a.IMEI4;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 != "")
                                                    {
                                                        btFormat.SubStrings["MAC"].Value = a.IMEI6;
                                                        macstr = a.IMEI6;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 != "")
                                                    {
                                                        btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                                                        equistr = a.IMEI7;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }

                                                }

                                                if (this.RelationVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 != "")
                                                    {
                                                        btFormat.SubStrings["VIP"].Value = a.IMEI8;
                                                        vipstr = a.IMEI8;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 != "")
                                                    {
                                                        btFormat.SubStrings["BAT"].Value = a.IMEI9;
                                                        batstr = a.IMEI9;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationRFID.Checked == true)
                                                {
                                                    if (a.RFID != "")
                                                    {
                                                        btFormat.SubStrings["RFID"].Value = a.RFID;
                                                        rfidstr = a.RFID;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        //对模板相应字段进行赋值
                                        ValueToTemplate(btFormat);
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        if (this.SN1_num.Text != "")
                                        {
                                            if (!PMB.CheckSNBLL(this.SN1_num.Text))
                                            {
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = this.SN1_num.Text;
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SIM = simstr != "" ? simstr : "";
                                                PList.VIP = vipstr != "" ? vipstr : "";
                                                PList.BAT = batstr != "" ? batstr : "";
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.ICCID = iccidstr != "" ? iccidstr : "";
                                                PList.MAC = macstr != "" ? macstr : "";
                                                PList.Equipment = equistr != "" ? equistr : "";
                                                PList.RFID = rfidstr != "" ? rfidstr : "";
                                                PList.JSUserName = this.UserShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text) + 1).ToString()))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        this.SN1_num.Text = sn1;
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                this.IMEI_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = "";
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                            PList.SN = "";
                                            PList.IMEIRel = this.IMEIRel.Text.Trim();
                                            PList.SIM = simstr != "" ? simstr : "";
                                            PList.VIP = vipstr != "" ? vipstr : "";
                                            PList.BAT = batstr != "" ? batstr : "";
                                            PList.SoftModel = this.SoftModel.Text.Trim();
                                            PList.Version = this.SoftwareVersion.Text.Trim();
                                            PList.Remark = this.Remake.Text.Trim();
                                            PList.JS_PrintTime = ProductTime;
                                            PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                            PList.CH_PrintTime = "";
                                            PList.CH_TemplatePath1 = null;
                                            PList.CH_TemplatePath2 = null;
                                            PList.ICCID = iccidstr != "" ? iccidstr : "";
                                            PList.MAC = macstr != "" ? macstr : "";
                                            PList.Equipment = equistr != "" ? equistr : "";
                                            PList.RFID = rfidstr != "" ? rfidstr : "";
                                            PList.JSUserName =this.UserShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (long.Parse(this.IMEI_Start.Text) + 1).ToString()))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                        }
                                    }
                                    else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                    {
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                        foreach (PrintMessage a in list)
                                        {
                                            btFormat.SubStrings["SN"].Value = a.SN;
                                        }
                                        if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                        {
                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                            Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //不打印校验码，不打印SN号
                            case 6:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    if (a.IMEI2 == "")
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 == "")
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 == "")
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 == "")
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 == "")
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 == "")
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 == "")
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.CheckRFID.Checked == true)
                                                {
                                                    if (a.RFID == "")
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (RelationFields.Count != 0)
                                    {
                                        List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                                        if (drs.Count == 1)
                                        {
                                            foreach (DataRelativeSheet a in drs)
                                            {
                                                if (this.RelationSN.Checked == true)
                                                {
                                                    if (a.IMEI2 != "")
                                                    {
                                                        //btFormat.SubStrings["SN"].Value = a.IMEI2;
                                                        //snstr = a.IMEI2;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SN号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationSIM.Checked == true)
                                                {
                                                    if (a.IMEI3 != "")
                                                    {
                                                        btFormat.SubStrings["SIM"].Value = a.IMEI3;
                                                        simstr = a.IMEI3;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("SIM号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationICCID.Checked == true)
                                                {
                                                    if (a.IMEI4 != "")
                                                    {
                                                        btFormat.SubStrings["ICCID"].Value = a.IMEI4;
                                                        iccidstr = a.IMEI4;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("ICCID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationMAC.Checked == true)
                                                {
                                                    if (a.IMEI6 != "")
                                                    {
                                                        btFormat.SubStrings["MAC"].Value = a.IMEI6;
                                                        macstr = a.IMEI6;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("MAC号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationEquipment.Checked == true)
                                                {
                                                    if (a.IMEI7 != "")
                                                    {
                                                        btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                                                        equistr = a.IMEI7;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("Equipment号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }

                                                }

                                                if (this.RelationVIP.Checked == true)
                                                {
                                                    if (a.IMEI8 != "")
                                                    {
                                                        btFormat.SubStrings["VIP"].Value = a.IMEI8;
                                                        vipstr = a.IMEI8;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("VIP号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationBAT.Checked == true)
                                                {
                                                    if (a.IMEI9 != "")
                                                    {
                                                        btFormat.SubStrings["BAT"].Value = a.IMEI9;
                                                        batstr = a.IMEI9;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("EBAT号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }

                                                if (this.RelationRFID.Checked == true)
                                                {
                                                    if (a.RFID != "")
                                                    {
                                                        btFormat.SubStrings["RFID"].Value = a.RFID;
                                                        rfidstr = a.RFID;
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText("RFID号为空\r\n");
                                                        player.Play();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    //对模板相应字段进行赋值
                                    ValueToTemplate(btFormat);
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        PList.SIM = simstr != "" ? simstr : "";
                                        PList.VIP = vipstr != "" ? vipstr : "";
                                        PList.BAT = batstr != "" ? batstr : "";
                                        PList.SoftModel = this.SoftModel.Text.Trim();
                                        PList.Version = this.SoftwareVersion.Text.Trim();
                                        PList.Remark = this.Remake.Text.Trim();
                                        PList.JS_PrintTime = ProductTime;
                                        PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                        PList.CH_PrintTime = "";
                                        PList.CH_TemplatePath1 = null;
                                        PList.CH_TemplatePath2 = null;
                                        PList.ICCID = iccidstr != "" ? iccidstr : "";
                                        PList.MAC = macstr != "" ? macstr : "";
                                        PList.Equipment = equistr != "" ? equistr : "";
                                        PList.RFID = rfidstr != "" ? rfidstr : "";
                                        PList.JSUserName =this.UserShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn1_suffix.ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text) + 1).ToString()))
                                            {
                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                this.IMEI_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        player1.Play();
                        this.reminder.AppendText("请选择模板\r\n");
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:" + ex.Message);
                }
            }
        }

        //公用模板复制函数
        private void ValueToTemplate(LabelFormatDocument btFormat)
        {
            //GetValue("Information", "型号", out outString);
            //btFormat.SubStrings[outString].Value = this.SoftModel.Text;
            GetValue("Information", "生产日期", out outString);
            btFormat.SubStrings[outString].Value = this.ProductData.Text;
            //GetValue("Information", "软件版本", out outString);
            //btFormat.SubStrings[outString].Value = this.SoftwareVersion.Text;
            //GetValue("Information", "备注", out outString);
            //btFormat.SubStrings[outString].Value = this.Remake.Text;
        }

        //逐个重打
        private void Re_IMEINum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.UserShow.Text == "")
                {
                    this.reminder.AppendText("请先登录\r\n");
                    return;
                }
                if (this.CB_ZhiDan.Text == "")
                {
                    player2.Play();
                    this.reminder.AppendText("请先选择制单\r\n");
                    this.Re_IMEINum.Clear();
                    this.Re_IMEINum.Focus();
                    return;
                }
                if (this.StartZhiDan == 0)
                {
                    this.reminder.AppendText("请获取制单数据\r\n");
                    this.Re_IMEINum.Clear();
                    this.Re_IMEINum.Focus();
                    return;
                }

                if (this.ToLock.Enabled == true)
                {
                    this.reminder.AppendText("请锁定\r\n");
                    this.Re_IMEINum.Clear();
                    this.Re_IMEINum.Focus();
                    return;
                }

                if (this.Re_IMEINum.Text != "")
                {
                    if (this.Re_Nocheckcode.Checked == false)
                    {
                        if (IsNumeric(this.Re_IMEINum.Text))
                        {
                            if (long.Parse(this.Re_IMEINum.Text.Substring(0, 14)) < long.Parse(this.IMEI_num1.Text) || long.Parse(this.Re_IMEINum.Text.Substring(0, 14)) > long.Parse(this.IMEI_num2.Text))
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI不在范围内\r\n");
                                this.Re_IMEINum.Clear();
                                this.Re_IMEINum.Focus();
                                return;
                            }
                            if (this.Re_IMEINum.Text.Length != 15)
                            {
                                player.Play();
                                this.reminder.AppendText("请输入15位IMEI\r\n");
                                this.Re_IMEINum.Clear();
                                this.Re_IMEINum.Focus();
                                return;
                            }
                            else
                            {
                                string imeiRes;
                                string imei14 = this.Re_IMEINum.Text.Substring(0, 14);
                                string imei15 = getimei15(imei14);
                                imeiRes = imei14 + imei15;
                                if (imeiRes != this.Re_IMEINum.Text)
                                {
                                    player3.Play();
                                    this.reminder.AppendText("校验错误\r\n");
                                    this.Re_IMEINum.Clear();
                                    this.Re_IMEINum.Focus();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI格式错误\r\n");
                            this.Re_IMEINum.Clear();
                            this.Re_IMEINum.Focus();
                            return;
                        }
                    }
                    else
                    {
                        if (this.Re_IMEINum.Text.Length != this.IMEI_num1.Text.Length)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.Re_IMEINum.Clear();
                            this.Re_IMEINum.Focus();
                            return;
                        }
                        if (this.Re_IMEINum.Text.CompareTo(this.IMEI_num1.Text) == -1 || this.Re_IMEINum.Text.CompareTo(this.IMEI_num2.Text) == 1)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.Re_IMEINum.Clear();
                            this.Re_IMEINum.Focus();
                            return;
                        }
                    }
                }
                else
                {
                    this.Re_IMEINum.Focus();
                    return;
                }
                try
                {
                    if (this.Select_Template1.Text != "")
                    {
                        string snstr = "";
                        string simstr = "";
                        string iccidstr = "";
                        string macstr = "";
                        string equistr = "";
                        string vipstr = "";
                        string batstr = "";
                        string rfidstr = "";

                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        //指定打印机名称
                        btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                        //对模板相应字段进行赋值
                        GetValue("Information", "生产日期", out outString);
                        btFormat.SubStrings[outString].Value = this.ProductData.Text;
                        //打印份数,同序列打印的份数
                        btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;

                        if (CheckFields.Count != 0)
                        {
                            List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                            if (drs.Count == 1)
                            {
                                foreach (DataRelativeSheet a in drs)
                                {
                                    if (this.CheckIMEI2.Checked == true)
                                    {
                                        if (a.IMEI2 == "")
                                        {
                                            this.reminder.AppendText("SN号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.CheckSIM.Checked == true)
                                    {
                                        if (a.IMEI3 == "")
                                        {
                                            this.reminder.AppendText("SIM号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.CheckICCID.Checked == true)
                                    {
                                        if (a.IMEI4 == "")
                                        {
                                            this.reminder.AppendText("ICCID号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.CheckMAC.Checked == true)
                                    {
                                        if (a.IMEI6 == "")
                                        {
                                            this.reminder.AppendText("MAC号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.CheckEquipment.Checked == true)
                                    {
                                        if (a.IMEI7 == "")
                                        {
                                            this.reminder.AppendText("Equipment号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.CheckVIP.Checked == true)
                                    {
                                        if (a.IMEI8 == "")
                                        {
                                            this.reminder.AppendText("VIP号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.CheckBAT.Checked == true)
                                    {
                                        if (a.IMEI9 == "")
                                        {
                                            this.reminder.AppendText("EBAT号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.CheckRFID.Checked == true)
                                    {
                                        if (a.RFID == "")
                                        {
                                            this.reminder.AppendText("RFID号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                player.Play();
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                        }

                        if (RelationFields.Count != 0)
                        {
                            List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(this.IMEI_Start.Text);
                            if (drs.Count == 1)
                            {
                                foreach (DataRelativeSheet a in drs)
                                {
                                    if (this.RelationSN.Checked == true)
                                    {
                                        if (a.IMEI2 != "")
                                        {
                                            btFormat.SubStrings["SN"].Value = a.IMEI2;
                                            snstr = a.IMEI2;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("SN号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.RelationSIM.Checked == true)
                                    {
                                        if (a.IMEI3 != "")
                                        {
                                            btFormat.SubStrings["SIM"].Value = a.IMEI3;
                                            simstr = a.IMEI3;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("SIM号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.RelationICCID.Checked == true)
                                    {
                                        if (a.IMEI4 != "")
                                        {
                                            btFormat.SubStrings["ICCID"].Value = a.IMEI4;
                                            iccidstr = a.IMEI4;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("ICCID号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.RelationMAC.Checked == true)
                                    {
                                        if (a.IMEI6 != "")
                                        {
                                            btFormat.SubStrings["MAC"].Value = a.IMEI6;
                                            macstr = a.IMEI6;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("MAC号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.RelationEquipment.Checked == true)
                                    {
                                        if (a.IMEI7 != "")
                                        {
                                            btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                                            equistr = a.IMEI7;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("Equipment号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }

                                    }

                                    if (this.RelationVIP.Checked == true)
                                    {
                                        if (a.IMEI8 != "")
                                        {
                                            btFormat.SubStrings["VIP"].Value = a.IMEI8;
                                            vipstr = a.IMEI8;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("VIP号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.RelationBAT.Checked == true)
                                    {
                                        if (a.IMEI9 != "")
                                        {
                                            btFormat.SubStrings["BAT"].Value = a.IMEI9;
                                            batstr = a.IMEI9;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("EBAT号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    if (this.RelationRFID.Checked == true)
                                    {
                                        if (a.RFID != "")
                                        {
                                            btFormat.SubStrings["RFID"].Value = a.RFID;
                                            rfidstr = a.RFID;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText("RFID号为空\r\n");
                                            player.Play();
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                                player.Play();
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                        }

                        btFormat.SubStrings["IMEI"].Value = this.Re_IMEINum.Text;
                        if (PMB.CheckReCHOrJSIMEIBLL(this.Re_IMEINum.Text, 1))
                        {
                            if (RelationSN.Checked == false)
                                btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(this.Re_IMEINum.Text);
                            else
                                btFormat.SubStrings["SN"].Value = snstr;
                           //更新打印信息到数据表
                           string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                            if (PMB.UpdateRePrintBLL(this.Re_IMEINum.Text, RE_PrintTime, 1, lj, lj))
                            {
                                btFormat.Print();
                                Form1.Log("重打了IMEI号为" + this.Re_IMEINum.Text + "的制单", null);
                                this.Re_IMEINum.Clear();
                                this.Re_IMEINum.Focus();
                            }
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText(this.Re_IMEINum.Text + "无记录\r\n");
                            this.Re_IMEINum.Clear();
                            this.Re_IMEINum.Focus();
                        }
                    }
                    else
                    {
                        player1.Play();
                        this.reminder.AppendText("请先选择模板\r\n");
                        this.Re_IMEINum.Clear();
                        this.Re_IMEINum.Focus();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:" + ex.Message);
                }
            }
        }

        //选择客供SN复选框引发的事件
        private void SnFromCustomer_Click(object sender, EventArgs e)
        {
            if (this.SnFromCustomer.Checked == true)
            {
                c1 = 1;
                if (this.NoSn.Checked == true)
                {
                    this.NoSn.Checked = false;
                    c3 = 0;
                }
            }
            else
            {
                c1 = 0;
            }
        }

        //选择无校验位复选框引发的事件
        private void NoCheckCode_Click(object sender, EventArgs e)
        {
            if (this.NoCheckCode.Checked == true)
            {
                c2 = 2;
            }
            else
            {
                c2 = 0;
                this.Hexadecimal.Checked = false;
                this.HexPrintNum.Visible = false;
            }
        }

        //选择不打印SN复选框引发的事件
        private void NoSn_Click(object sender, EventArgs e)
        {
            if (this.NoSn.Checked == true)
            {
                c3 = 4;
                if (this.SnFromCustomer.Checked == true)
                {
                    this.SnFromCustomer.Checked = false;
                    c1 = 0;
                }
            }
            else
            {
                c3 = 0;
            }
        }

        //选择16进制时触发的事件
        private void Hexadecimal_Click(object sender, EventArgs e)
        {
            if(this.Hexadecimal.Checked == true)
            {
                this.HexPrintNum.Visible = true;
                this.HexPrintNum.Focus();
                this.HexPrintNum.BringToFront();
                if (NoCheckCode.Checked == false)
                {
                    this.NoCheckCode.Checked = true;
                    c2 = 2;
                }
            }
            else
            {
                this.HexPrintNum.Visible = false;
            }
        }

        //批量重打IMEI起始位
        private void ReImeiNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.ReImeiNum1.Text != "")
                {
                    if (this.Select_Template1.Text == "")
                    {
                        player1.Play();
                        this.reminder.AppendText("请先选择模板\r\n");
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum1.Focus();
                        return;
                    }
                    if (this.CB_ZhiDan.Text == "")
                    {
                        player2.Play();
                        this.reminder.AppendText("请先选择制单\r\n");
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum1.Focus();
                        return;
                    }
                    if (this.StartZhiDan == 0)
                    {
                        this.reminder.AppendText("请获取制单数据\r\n");
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum1.Focus();
                        return;
                    }

                    if(this.ToLock.Enabled == true)
                    {
                        this.reminder.AppendText("请锁定\r\n");
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum1.Focus();
                        return;
                    }
                    if (this.Re_Nocheckcode.Checked == false)
                    {
                        if (IsNumeric(this.ReImeiNum1.Text))
                        {
                            if (this.ReImeiNum1.Text.Length != 15)
                            {
                                player.Play();
                                this.reminder.AppendText("请输入15位IMEI\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }
                            else
                            {
                                string imeiRes;
                                string imei14 = this.ReImeiNum1.Text.Substring(0, 14);
                                string imei15 = getimei15(imei14);
                                imeiRes = imei14 + imei15;
                                if (imeiRes != this.ReImeiNum1.Text)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI校验错误\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI格式错误\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                        if (long.Parse(this.ReImeiNum1.Text.Substring(0, 14)) < long.Parse(this.IMEI_num1.Text) || long.Parse(this.ReImeiNum1.Text.Substring(0, 14)) > long.Parse(this.IMEI_num2.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                    }
                    else
                    {
                        if (this.ReImeiNum1.Text.Length != this.IMEI_num1.Text.Length)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                        if (this.ReImeiNum1.Text.CompareTo(this.IMEI_num1.Text) == -1 || this.ReImeiNum1.Text.CompareTo(this.IMEI_num2.Text) == 1)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                    }
                    this.ReImeiNum2.Focus();
                }
            }
        }

        //批量重打IMEI终止位
        private void ReImeiNum2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.ReImeiNum2.Text != "")
                {
                    if (this.Re_Nocheckcode.Checked == false)
                    {
                        if (IsNumeric(this.ReImeiNum2.Text))
                        {
                            if (this.ReImeiNum2.Text.Length != 15)
                            {
                                player.Play();
                                this.reminder.AppendText("请输入15位IMEI\r\n");
                                this.ReImeiNum2.Clear();
                                this.ReImeiNum2.Focus();
                                return;
                            }
                            else
                            {
                                string imeiRes;
                                string imei14 = this.ReImeiNum2.Text.Substring(0, 14);
                                string imei15 = getimei15(imei14);
                                imeiRes = imei14 + imei15;
                                if (imeiRes != this.ReImeiNum2.Text)
                                {
                                    player3.Play();
                                    this.reminder.AppendText("IMEI校验错误\r\n");
                                    this.ReImeiNum2.Clear();
                                    this.ReImeiNum2.Focus();
                                    return;
                                }
                                else if (long.Parse(imei14) < long.Parse(this.ReImeiNum1.Text.Substring(0, 14)))
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI小于重打起始位\r\n");
                                    this.ReImeiNum2.Clear();
                                    this.ReImeiNum2.Focus();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI格式错误\r\n");
                            this.ReImeiNum2.Clear();
                            this.ReImeiNum2.Focus();
                            return;
                        }
                        if (long.Parse(this.ReImeiNum2.Text.Substring(0, 14)) < long.Parse(this.IMEI_num1.Text) || long.Parse(this.ReImeiNum2.Text.Substring(0, 14)) > long.Parse(this.IMEI_num2.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.ReImeiNum2.Clear();
                            this.ReImeiNum2.Focus();
                            return;
                        }
                    }
                    else
                    {
                        if (this.ReImeiNum2.Text.Length != this.IMEI_num1.Text.Length)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                        if (this.ReImeiNum2.Text.CompareTo(this.IMEI_num1.Text) == -1 || this.ReImeiNum2.Text.CompareTo(this.IMEI_num2.Text) == 1)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                    }
                }
                else
                {
                    player.Play();
                    this.reminder.AppendText("请输入重打终止位\r\n");
                    this.ReImeiNum2.Focus();
                    return;
                }
                try
                {
                    //制定模板
                    LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                    //指定打印机名称
                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                    //对模板相应字段进行赋值
                    GetValue("Information", "生产日期", out outString);
                    btFormat.SubStrings[outString].Value = this.ProductData.Text;
                    //打印份数,同序列打印的份数
                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;

                    if (this.Re_Nocheckcode.Checked == false)
                    {
                        long Num2Imei14 = long.Parse(this.ReImeiNum2.Text.Substring(0, 14));
                        int JSCount = PMB.CheckReJSRangeIMEIBLL(this.ReImeiNum1.Text, this.ReImeiNum2.Text);
                        int InputCount = int.Parse((Num2Imei14 - long.Parse(this.ReImeiNum1.Text.Substring(0, 14)) + 1).ToString());
                        if (JSCount != InputCount)
                        {
                            this.reminder.AppendText("部分无记录，无法全部重打\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                        for (long Num1Imei14 = long.Parse(this.ReImeiNum1.Text.Substring(0, 14)); Num1Imei14 <= Num2Imei14; Num1Imei14++)
                        {
                            string Num1Imei15 = getimei15(Num1Imei14.ToString());
                            btFormat.SubStrings["IMEI"].Value = Num1Imei14.ToString() + Num1Imei15.ToString();
                            btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(Num1Imei14.ToString() + Num1Imei15.ToString());
                            //更新打印信息到数据表
                            string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                            if (PMB.UpdateRePrintBLL(Num1Imei14.ToString() + Num1Imei15.ToString(), RE_PrintTime, 1, lj, lj))
                            {
                                btFormat.Print();
                                Form1.Log("批量重打了IMEI号为" + Num1Imei14.ToString() + Num1Imei15.ToString() + "的制单", null);
                            }
                        }
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum2.Clear();
                        this.ReImeiNum1.Focus();
                    }
                    else
                    {
                        if (this.RePrintHex.Checked == false)
                        {
                            int ReDig = this.IMEI_num2.Text.Length;
                            string RePre = this.ReImeiNum1.Text.Substring(0, ReDig - 5);
                            long Num2Imei14 = long.Parse(this.ReImeiNum2.Text.Remove(0, ReDig - 5));
                            int JSCount2 = PMB.CheckReJSRangeIMEIBLL(this.ReImeiNum1.Text, this.ReImeiNum2.Text);
                            int InputCount2 = int.Parse((Num2Imei14 - long.Parse(this.ReImeiNum1.Text.Remove(0, ReDig - 5)) + 1).ToString());
                            if (JSCount2 != InputCount2)
                            {
                                this.reminder.AppendText("部分无记录，无法全部重打\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }
                            for (long Num1Imei14 = long.Parse(this.ReImeiNum1.Text.Remove(0, ReDig - 5)); Num1Imei14 <= Num2Imei14; Num1Imei14++)
                            {
                                btFormat.SubStrings["IMEI"].Value = RePre + Num1Imei14.ToString();
                                btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(RePre + Num1Imei14.ToString());
                                //更新打印信息到数据表
                                string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                if (PMB.UpdateRePrintBLL(RePre + Num1Imei14.ToString(), RE_PrintTime, 1, lj, lj))
                                {
                                    btFormat.Print();
                                    Form1.Log("批量重打了IMEI号为" + RePre + Num1Imei14 + "的制单", null);
                                }
                            }
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImeiNum1.Focus();
                        }
                        else
                        {
                            int JSCount2 = PMB.CheckReJSRangeIMEIBLL(this.ReImeiNum1.Text, this.ReImeiNum2.Text);
                            long InputCount2 = Convert.ToInt64(this.ReImeiNum2.Text, 16) - Convert.ToInt64(this.ReImeiNum1.Text, 16) +Convert.ToInt64("1",16);
                            if (JSCount2 != InputCount2)
                            {
                                this.reminder.AppendText("部分无记录，无法全部重打\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }
                            for (long Num1Imei14 = Convert.ToInt64(this.ReImeiNum1.Text, 16); Num1Imei14 <= Convert.ToInt64(this.ReImeiNum2.Text, 16);)
                            {
                                btFormat.SubStrings["IMEI"].Value = Num1Imei14.ToString("X");
                                btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(Num1Imei14.ToString("X"));
                                //更新打印信息到数据表
                                string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                if (PMB.UpdateRePrintBLL(Num1Imei14.ToString("X"), RE_PrintTime, 1, lj, lj))
                                {
                                    Num1Imei14 = Convert.ToInt64(Num1Imei14.ToString("X"),16) + Convert.ToInt64("1", 16);
                                    btFormat.Print();
                                    Form1.Log("批量重打了IMEI号为" + Num1Imei14 + "的制单", null);
                                }
                            }
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImeiNum1.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:" + ex.Message);
                }
            }
        }

        //刷新制单
        private void Refresh_zhidan_Click(object sender, EventArgs e)
        {
            this.CB_ZhiDan.Items.Clear();
            G_MOP = MOPB.SelectZhidanNumBLL();
            foreach (Gps_ManuOrderParam a in G_MOP)
            {
                this.CB_ZhiDan.Items.Add(a.ZhiDan);
            }
            if (MOPB.CheckZhiDanBLL(this.CB_ZhiDan.Text))
            {
                CB_ZhiDan_SelectedIndexChanged(sender, e);
            }
            else
            {
                this.CB_ZhiDan.Text = "";
                this.Select_Template1.Clear();
                this.SoftModel.Clear();
                this.SN1_num.Clear();
                this.SN2_num.Clear();
                this.ProductNo.Clear();
                this.SoftwareVersion.Clear();
                this.IMEI_num1.Clear();
                this.IMEI_num2.Clear();
                this.IMEI_Present.Clear();
                this.SIM_num1.Clear();
                this.SIM_num2.Clear();
                this.BAT_num1.Clear();
                this.BAT_num2.Clear();
                this.VIP_num1.Clear();
                this.VIP_num2.Clear();
                this.Remake.Clear();
                this.IMEIRel.Clear();
                this.IMEI_Start.Clear();
                this.PrintNum.Clear();
                this.Re_IMEINum.Clear();
                this.ReImeiNum1.Clear();
                this.ReImeiNum2.Clear();
                this.SnFromCustomer.Checked = false;
                this.NoCheckCode.Checked = false;
                this.NoSn.Checked = false;
                this.Hexadecimal.Checked = false;
                this.Re_Nocheckcode.Checked = false;
                this.RePrintOne.Checked = false;
                this.RePrintMore.Checked = false;
                this.Re_IMEINum.ReadOnly = true;
                this.ReImeiNum1.ReadOnly = true;
                this.ReImeiNum2.ReadOnly = true;
            }
        }

        //解锁按钮，弹出输入密码的界面
        private void Unlock_Click(object sender, EventArgs e)
        {
            JS_Unlock ul = new JS_Unlock(this);
            ul.ShowDialog();
        }

        //解锁的内容
        public void Unlock_content()
        {
            this.Open_Template1.Enabled = true;
            this.Select_Template1.ReadOnly = false;
            this.Printer1.Enabled = true;
            this.CB_ZhiDan.Enabled = true;
            this.Open_file.Enabled = true;
            this.Debug_print.Enabled = true;
            this.Refresh_zhidan.Enabled = true;
            this.Refresh_template.Enabled = true;
            this.ToLock.Enabled = true;
            this.SqlConfig.Enabled = true;
            this.PrintOne.Enabled = true;
            this.PrintMore.Enabled = true;
            this.NoCheckCode.Enabled = true;
            this.SnFromCustomer.Enabled = true;
            this.NoSn.Enabled = true;
            this.Hexadecimal.Enabled = true;
            this.SNHex.Enabled = true;
            this.RePrintOne.Enabled = true;
            this.RePrintMore.Enabled = true;
            this.Re_Nocheckcode.Enabled = true;
            this.RePrintHex.Enabled = true;
            this.Get_ZhiDan_Data.Enabled = true;
            this.ProductData.ReadOnly = false;
            this.TemplateNum.ReadOnly = false;

            this.PrintMode1.Enabled = true;
            this.PrintMode2.Enabled = true;

            this.SiginIN.Enabled = true;
            this.QuitBt.Enabled = true;

            if (this.PrintOne.Checked == true)
            {
                this.CheckIMEI2.Enabled = true;
                this.CheckSIM.Enabled = true;
                this.CheckBAT.Enabled = true;
                this.CheckICCID.Enabled = true;
                this.CheckMAC.Enabled = true;
                this.CheckEquipment.Enabled = true;
                this.CheckVIP.Enabled = true;
                this.CheckRFID.Enabled = true;

                this.RelationSN.Enabled = true;
                this.RelationSIM.Enabled = true;
                this.RelationICCID.Enabled = true;
                this.RelationBAT.Enabled = true;
                this.RelationMAC.Enabled = true;
                this.RelationEquipment.Enabled = true;
                this.RelationVIP.Enabled = true;
                this.RelationRFID.Enabled = true;

                this.IMEI_Start.ReadOnly = false;
                this.IMEI_Start.Focus();
            }
            if (this.PrintMore.Checked == true)
            {
                this.PrintNum.ReadOnly = false;
                this.PrintNum.Focus();
            }
        }

        //锁定
        private void ToLock_Click(object sender, EventArgs e)
        {
            if(this.UserShow.Text == "")
            {
                this.reminder.AppendText("请先登录\r\n");
                return;
            }

            if (this.CB_ZhiDan.Text == "")
            {
                player.Play();
                this.reminder.AppendText("请先选择制单号\r\n");
                return;
            }
            if (this.Select_Template1.Text == "")
            {
                player.Play();
                this.reminder.AppendText("模板不能为空\r\n");
                return;
            }
            if(this.SoftModel.Text =="" && this.SIM_num1.Text == ""&& this.SIM_num2.Text == ""
                && this.SN1_num.Text == ""&& this.SN2_num.Text == ""&& this.BAT_num1.Text == ""
                && this.BAT_num1.Text == "" && this.ProductNo.Text == "" && this.BAT_num2.Text == ""
                && this.SoftwareVersion.Text == "" && this.VIP_num1.Text == "" && this.VIP_num2.Text == ""
                 && this.IMEI_num1.Text == "" && this.IMEI_num2.Text == "" && this.IMEIRel.Text == ""
                  && this.IMEI_Present.Text == "" && this.Remake.Text == "")
            {
                this.reminder.AppendText("请获取制单数据\r\n");
                return;
            }

            this.Open_Template1.Enabled = false;
            this.Select_Template1.ReadOnly = true;
            this.Printer1.Enabled = false;
            this.CB_ZhiDan.Enabled = false;
            this.Open_file.Enabled = false;
            this.Debug_print.Enabled = false;
            this.Refresh_template.Enabled = false;
            this.Refresh_zhidan.Enabled = false;
            this.ToLock.Enabled = false;
            this.SqlConfig.Enabled = false;
            this.PrintOne.Enabled = false;
            this.PrintMore.Enabled = false;
            this.NoCheckCode.Enabled = false;
            this.SnFromCustomer.Enabled = false;
            this.NoSn.Enabled = false;
            this.Hexadecimal.Enabled = false;
            this.SNHex.Enabled = false;
            this.RePrintOne.Enabled = false;
            this.RePrintMore.Enabled = false;
            this.Re_Nocheckcode.Enabled = false;
            this.RePrintHex.Enabled = false;
            this.Get_ZhiDan_Data.Enabled = false;

            this.CheckIMEI2.Enabled = false;
            this.CheckSIM.Enabled = false;
            this.CheckBAT.Enabled = false;
            this.CheckICCID.Enabled = false;
            this.CheckMAC.Enabled = false;
            this.CheckEquipment.Enabled = false;
            this.CheckVIP.Enabled = false;
            this.CheckRFID.Enabled = false;

            this.RelationSN.Enabled = false;
            this.RelationSIM.Enabled = false;
            this.RelationICCID.Enabled = false;
            this.RelationBAT.Enabled = false;
            this.RelationMAC.Enabled = false;
            this.RelationEquipment.Enabled = false;
            this.RelationVIP.Enabled = false;
            this.RelationRFID.Enabled = false;

            this.PrintMode1.Enabled = false;
            this.PrintMode2.Enabled = false;

            this.SiginIN.Enabled = false;
            this.QuitBt.Enabled = false;

            if (this.RePrintOne.Checked == true || this.RePrintMore.Checked == true)
            {
                this.IMEI_Start.ReadOnly = true;
                this.PrintNum.ReadOnly = true;
            }
            this.ProductData.ReadOnly = true;
            this.TemplateNum.ReadOnly = true;
            this.ToUnlock.Enabled = true;

            if (this.PrintOne.Checked == true)
                this.IMEI_Start.Focus();

            if (this.PrintMore.Checked == true)
                this.PrintNum.Focus();

            if (this.RePrintOne.Checked == true)
                this.Re_IMEINum.Focus();

            if (this.RePrintMore.Checked == true)
                this.ReImeiNum1.Focus();

            if (this.CB_ZhiDan.Text != "")
            {
                MOPB.UpdateJS_TemplatePathDAL(this.CB_ZhiDan.Text, lj);
            }
            //记录界面复选框
           MFPRPB.InsertPrintRecordParamBLL(this.CB_ZhiDan.Text, Convert.ToInt32(this.PrintOne.Checked), Convert.ToInt32(this.PrintMore.Checked), Convert.ToInt32(this.SnFromCustomer.Checked), Convert.ToInt32(this.NoCheckCode.Checked), Convert.ToInt32(this.NoSn.Checked), Convert.ToInt32(this.Hexadecimal.Checked), Convert.ToInt32(this.SNHex.Checked), Convert.ToInt32(this.RePrintOne.Checked), Convert.ToInt32(this.RePrintMore.Checked), Convert.ToInt32(this.Re_Nocheckcode.Checked), Convert.ToInt32(this.RePrintHex.Checked),
           Convert.ToInt32(this.RelationSN.Checked), Convert.ToInt32(this.RelationSIM.Checked), Convert.ToInt32(this.RelationBAT.Checked), Convert.ToInt32(this.RelationICCID.Checked), Convert.ToInt32(this.RelationMAC.Checked), Convert.ToInt32(this.RelationEquipment.Checked), Convert.ToInt32(this.RelationVIP.Checked), Convert.ToInt32(this.RelationRFID.Checked), Convert.ToInt32(this.CheckIMEI2.Checked), Convert.ToInt32(this.CheckSIM.Checked), Convert.ToInt32(this.CheckBAT.Checked)
            ,Convert.ToInt32(this.CheckICCID.Checked), Convert.ToInt32(this.CheckMAC.Checked), Convert.ToInt32(this.CheckEquipment.Checked), Convert.ToInt32(this.CheckVIP.Checked), Convert.ToInt32(this.CheckRFID.Checked), Convert.ToInt32(this.PrintMode1.Checked), Convert.ToInt32(this.PrintMode2.Checked));

        }

        //输入模板打印份数引发函数
        private void TemplateNum_TextChanged(object sender, EventArgs e)
        {
            if (this.TemplateNum.Text != "")
            {
                if (IsNumeric(this.TemplateNum.Text))
                {
                    TN = int.Parse(this.TemplateNum.Text);
                }
                else
                {
                    this.reminder.AppendText("请输入数字\r\n");
                    this.TemplateNum.Clear();
                    this.TemplateNum.Focus();
                }
            }
        }

        //光标离开模板打印份数引发函数
        private void TemplateNum_Leave(object sender, EventArgs e)
        {
            if (this.TemplateNum.Text == "")
            {
                this.TemplateNum.Text = 1.ToString();
            }
        }

        //打开模板函数
        private void Open_file_Click(object sender, EventArgs e)
        {
            if (this.Select_Template1.Text == "")
            {
                player1.Play();
            }
            else
            {
                string path = this.Select_Template1.Text;
                if (File.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    player.Play();
                }
            }
        }

        //获取制单号数据
        private void Get_ZhiDan_Data_Click(object sender, EventArgs e)
        {
            if (this.CB_ZhiDan.Text == "")
            {
                this.reminder.AppendText("请选择制单号\r\n");
                return;
            }
                
            foreach (Gps_ManuOrderParam b in G_MOP)
            {
                s = b.SN2.Length;
                this.SoftModel.Text = b.SoftModel;
                this.SN1_num.Text = b.SN1 + b.SN2;
                this.SN2_num.Text = b.SN1 + b.SN3;
                this.IMEI_Present.Text = b.IMEIPrints;
                this.ProductNo.Text = b.ProductNo;
                this.SoftwareVersion.Text = b.Version;
                this.IMEI_num1.Text = b.IMEIStart;
                this.IMEI_num2.Text = b.IMEIEnd;
                this.SIM_num1.Text = b.SIMStart;
                this.SIM_num2.Text = b.SIMEnd;
                this.BAT_num1.Text = b.BATStart;
                this.BAT_num2.Text = b.BATEnd;
                this.VIP_num1.Text = b.VIPStart;
                this.VIP_num2.Text = b.VIPEnd;
                this.IMEI_Present.Text = b.IMEIPrints;
                this.Select_Template1.Text = b.JST_template;
                lj = b.JST_template;
                if (b.Remark1 != "")
                {
                    string rem = b.Remark1;
                    string[] remark = rem.Split('：');
                    this.Remake.Text = remark[1];
                }
                else
                {
                    this.Remake.Text = b.Remark1;
                }
                if (int.Parse(b.IMEIRel) == 0)
                {
                    this.IMEIRel.Text = "无绑定";
                }
                else if (int.Parse(b.IMEIRel) == 1)
                {
                    this.IMEIRel.Text = "与SIM卡绑定";
                }
                else if (int.Parse(b.IMEIRel) == 2)
                {
                    this.IMEIRel.Text = "与SIM&BAT绑定";
                }
                else if (int.Parse(b.IMEIRel) == 3)
                {
                    this.IMEIRel.Text = "与SIM&VIP绑定";
                }
                else if (int.Parse(b.IMEIRel) == 4)
                {
                    this.IMEIRel.Text = "与BAT绑定";
                }
            }
            string PresentImei = PMB.SelectPresentImeiByZhidanBLL(this.CB_ZhiDan.Text);
            if (PresentImei.Length == 15 && this.IMEI_num1.Text.Length == 14)
            {
                string PresentImei1 = PresentImei.Substring(0, 14);
                this.IMEI_Present.Text = PresentImei1;
            }
            else
            {
                if (PresentImei != "")
                {
                    this.IMEI_Present.Text = PresentImei;
                }
            }

            mfprpb = MFPRPB.selectRecordParamByzhidanBLL(this.CB_ZhiDan.Text);
            foreach (ManuFuselagePrintRecordParam a in mfprpb)
            {
                if (mfprpb.Count != 0)
                {
                    this.PrintOne.Checked = Convert.ToBoolean(a.PrintOneByOne);
                    if(this.PrintOne.Checked == true)
                    {
                        //if (this.PrintMore.Checked == true)
                        //    this.PrintMore.Checked = false;
                        this.CheckIMEI2.Enabled = true;
                        this.CheckSIM.Enabled = true;
                        this.CheckBAT.Enabled = true;
                        this.CheckICCID.Enabled = true;
                        this.CheckMAC.Enabled = true;
                        this.CheckEquipment.Enabled = true;
                        this.CheckVIP.Enabled = true;
                        this.CheckRFID.Enabled = true;

                        this.RelationSN.Enabled = true;
                        this.RelationSIM.Enabled = true;
                        this.RelationICCID.Enabled = true;
                        this.RelationBAT.Enabled = true;
                        this.RelationMAC.Enabled = true;
                        this.RelationEquipment.Enabled = true;
                        this.RelationVIP.Enabled = true;
                        this.RelationRFID.Enabled = true;

                        this.PrintNum.ReadOnly = true;
                        this.Re_IMEINum.ReadOnly = true;
                        this.ReImeiNum1.ReadOnly = true;
                        this.ReImeiNum2.ReadOnly = true;
                        this.PrintNum.Clear();
                        this.Re_IMEINum.Clear();
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum2.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        this.IMEI_Start.ReadOnly = false;
                    }


                    this.PrintMore.Checked = Convert.ToBoolean(a.Pltplot);
                    if (this.PrintMore.Checked == true)
                    {
                        this.CheckIMEI2.Enabled = false;
                        this.CheckSIM.Enabled = false;
                        this.CheckBAT.Enabled = false;
                        this.CheckICCID.Enabled = false;
                        this.CheckMAC.Enabled = false;
                        this.CheckEquipment.Enabled = false;
                        this.CheckVIP.Enabled = false;
                        this.CheckRFID.Enabled = false;

                        this.RelationSN.Enabled = false;
                        this.RelationSIM.Enabled = false;
                        this.RelationICCID.Enabled = false;
                        this.RelationBAT.Enabled = false;
                        this.RelationMAC.Enabled = false;
                        this.RelationEquipment.Enabled = false;
                        this.RelationVIP.Enabled = false;
                        this.RelationRFID.Enabled = false;

                        this.Re_IMEINum.ReadOnly = true;
                        this.IMEI_Start.ReadOnly = true;
                        this.ReImeiNum1.ReadOnly = true;
                        this.ReImeiNum2.ReadOnly = true;
                        this.Re_IMEINum.Clear();
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum2.Clear();
                        this.IMEI_Start.Clear();

                        this.PrintNum.Clear();
                        this.PrintNum.Focus();
                        this.PrintNum.ReadOnly = false;
                    }
                    this.SnFromCustomer.Checked = Convert.ToBoolean(a.CustomerSupplySN);
                    this.NoCheckCode.Checked = Convert.ToBoolean(a.NoPrintCheckCode);
                    this.NoSn.Checked = Convert.ToBoolean(a.NoPrintingSN);
                    this.Hexadecimal.Checked = Convert.ToBoolean(a.IMEIHexadecimal);
                    this.SNHex.Checked = Convert.ToBoolean(a.SNHexadecimal);
                    this.RePrintOne.Checked = Convert.ToBoolean(a.ReplayOneByOne);
                    if (this.RePrintOne.Checked == true)
                    {
                        //if (this.PrintMore.Checked == true)
                        //    this.PrintMore.Checked = false;
                        this.CheckIMEI2.Enabled = true;
                        this.CheckSIM.Enabled = true;
                        this.CheckBAT.Enabled = true;
                        this.CheckICCID.Enabled = true;
                        this.CheckMAC.Enabled = true;
                        this.CheckEquipment.Enabled = true;
                        this.CheckVIP.Enabled = true;
                        this.CheckRFID.Enabled = true;

                        this.RelationSN.Enabled = true;
                        this.RelationSIM.Enabled = true;
                        this.RelationICCID.Enabled = true;
                        this.RelationBAT.Enabled = true;
                        this.RelationMAC.Enabled = true;
                        this.RelationEquipment.Enabled = true;
                        this.RelationVIP.Enabled = true;
                        this.RelationRFID.Enabled = true;

                        this.PrintNum.ReadOnly = true;
                        this.IMEI_Start.ReadOnly = true;
                        this.ReImeiNum1.ReadOnly = true;
                        this.ReImeiNum2.ReadOnly = true;
                        this.PrintNum.Clear();
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum2.Clear();
                        this.IMEI_Start.Clear();
                        this.Re_IMEINum.Clear();
                        this.Re_IMEINum.Focus();
                        this.Re_IMEINum.ReadOnly = false;
                    }

                    this.RePrintMore.Checked = Convert.ToBoolean(a.BattingInBatches);
                    if (this.RePrintMore.Checked == true)
                    {
                        //if (this.PrintMore.Checked == true)
                        //    this.PrintMore.Checked = false;
                        this.CheckIMEI2.Enabled = false;
                        this.CheckSIM.Enabled = false;
                        this.CheckBAT.Enabled = false;
                        this.CheckICCID.Enabled = false;
                        this.CheckMAC.Enabled = false;
                        this.CheckEquipment.Enabled = false;
                        this.CheckVIP.Enabled = false;
                        this.CheckRFID.Enabled = false;

                        this.RelationSN.Enabled = false;
                        this.RelationSIM.Enabled = false;
                        this.RelationICCID.Enabled = false;
                        this.RelationBAT.Enabled = false;
                        this.RelationMAC.Enabled = false;
                        this.RelationEquipment.Enabled = false;
                        this.RelationVIP.Enabled = false;
                        this.RelationRFID.Enabled = false;

                        this.Re_IMEINum.ReadOnly = true;
                        this.IMEI_Start.ReadOnly = true;
                        this.PrintNum.ReadOnly = true;
                        this.Re_IMEINum.Clear();
                        this.PrintNum.Clear();
                        this.IMEI_Start.Clear();

                        this.ReImeiNum1.Clear();
                        this.ReImeiNum2.Clear();
                        this.ReImeiNum1.Focus();
                        this.ReImeiNum1.ReadOnly = false;
                        this.ReImeiNum2.ReadOnly = false;
                    }

                    this.Re_Nocheckcode.Checked = Convert.ToBoolean(a.NoParityBit);
                    this.RePrintHex.Checked = Convert.ToBoolean(a.Hexadecimal);

                    this.RelationSN.Checked = Convert.ToBoolean(a.JSRelationSnMark);
                    this.RelationSIM.Checked = Convert.ToBoolean(a.JSRelationSimMark);
                    this.RelationBAT.Checked = Convert.ToBoolean(a.JSRelationBatMark);
                    this.RelationICCID.Checked = Convert.ToBoolean(a.JSRelationIccidMark);
                    this.RelationMAC.Checked = Convert.ToBoolean(a.JSRelationMacMark);
                    this.RelationEquipment.Checked = Convert.ToBoolean(a.JSRelationEquipmentMark);
                    this.RelationVIP.Checked = Convert.ToBoolean(a.JSRelationVipMark);
                    this.RelationRFID.Checked = Convert.ToBoolean(a.JSRelationRfidMark);
                    this.CheckIMEI2.Checked = Convert.ToBoolean(a.JSCheckSnMark);
                    this.CheckSIM.Checked = Convert.ToBoolean(a.JSCheckSimMark);
                    this.CheckBAT.Checked = Convert.ToBoolean(a.JSCheckBatMark);
                    this.CheckICCID.Checked = Convert.ToBoolean(a.JSCheckIccidMark);
                    this.CheckMAC.Checked = Convert.ToBoolean(a.JSCheckMacMark);
                    this.CheckEquipment.Checked = Convert.ToBoolean(a.JSCheckEquipmentMark);
                    this.CheckVIP.Checked = Convert.ToBoolean(a.JSCheckVipMark);
                    this.CheckRFID.Checked = Convert.ToBoolean(a.JSCheckRfidMark);
                    this.PrintMode1.Checked = Convert.ToBoolean(a.PrintMode1);
                    this.PrintMode2.Checked = Convert.ToBoolean(a.PrintMode2);

                    if(this.PrintOne.Checked == false && this.PrintMore.Checked == false && this.RePrintOne.Checked == false && this.RePrintMore.Checked == false)
                    {
                        this.PrintMore.Checked = true;
                        this.CheckIMEI2.Enabled = false;
                        this.CheckSIM.Enabled = false;
                        this.CheckBAT.Enabled = false;
                        this.CheckICCID.Enabled = false;
                        this.CheckMAC.Enabled = false;
                        this.CheckEquipment.Enabled = false;
                        this.CheckVIP.Enabled = false;
                        this.CheckRFID.Enabled = false;

                        this.RelationSN.Enabled = false;
                        this.RelationSIM.Enabled = false;
                        this.RelationICCID.Enabled = false;
                        this.RelationBAT.Enabled = false;
                        this.RelationMAC.Enabled = false;
                        this.RelationEquipment.Enabled = false;
                        this.RelationVIP.Enabled = false;
                        this.RelationRFID.Enabled = false;

                        this.Re_IMEINum.ReadOnly = true;
                        this.IMEI_Start.ReadOnly = true;
                        this.ReImeiNum1.ReadOnly = true;
                        this.ReImeiNum2.ReadOnly = true;
                        this.Re_IMEINum.Clear();
                        this.ReImeiNum1.Clear();
                        this.ReImeiNum2.Clear();
                        this.IMEI_Start.Clear();

                        this.PrintNum.Clear();
                        this.PrintNum.Focus();
                        this.PrintNum.ReadOnly = false;
                    }
                }
            }

            this.StartZhiDan = 1;

        }
        //打印模式1
        private void PrintMode1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.PrintMode1.Checked == false)
            {
                this.ModeFalge = 0;
            }
            else
            {
                if (this.PrintMode2.Checked == true)
                {
                    this.PrintMode2.Checked = false;
                }
                this.ModeFalge = 1;
            }
        }

        //打印模式2
        private void PrintMode2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.PrintMode2.Checked == false)
            {
                this.ModeFalge = 0;
            }
            else
            {
                if (this.PrintMode1.Checked == true)
                {
                    this.PrintMode1.Checked = false;
                }
                this.ModeFalge = 2;
            }
        }

        //关联SN
        private void RelationSN_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationSN.Checked == false)
            {
                RelationFields.Remove(0);
            }
            else
            {
                RelationFields[0] = "IMEI2";
            }
        }
        //关联SIM
        private void RelationSIM_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationSIM.Checked == false)
            {
                RelationFields.Remove(1);
            }
            else
            {
                RelationFields[1] = "IMEI3";
            }
        }

        //关联ICCID
        private void RelationICCID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationICCID.Checked == false)
            {
                RelationFields.Remove(2);
            }
            else
            {
                RelationFields[2] = "IMEI4";
            }
        }

        //关联MAC
        private void RelationMAC_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationMAC.Checked == false)
            {
                RelationFields.Remove(3);
            }
            else
            {
                RelationFields[3] = "IMEI6";
            }
        }
        //关联Equipment
        private void RelationEquipment_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationEquipment.Checked == false)
            {
                RelationFields.Remove(4);
            }
            else
            {
                RelationFields[4] = "IMEI7";
            }
        }
        //关联VIP
        private void RelationVIP_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationVIP.Checked == false)
            {
                RelationFields.Remove(5);
            }
            else
            {
                RelationFields[5] = "IMEI8";
            }
        }

        //关联BAT
        private void RelationBAT_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationBAT.Checked == false)
            {
                RelationFields.Remove(6);
            }
            else
            {
                RelationFields[6] = "IMEI9";
            }
        }


        //关联RFID
        private void RelationRFID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RelationRFID.Checked == false)
            {
                RelationFields.Remove(7);
            }
            else
            {
                RelationFields[7] = "IMEI13";
            }
        }
        //查SN
        private void CheckIMEI2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckIMEI2.Checked == false)
            {
                CheckFields.Remove(0);
            }
            else
            {
                CheckFields[0] = "IMEI2";
            }
        }
        //查SIM
        private void CheckSIM_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckSIM.Checked == false)
            {
                CheckFields.Remove(1);
            }
            else
            {
                CheckFields[1] = "IMEI3";
            }
        }
        //查ICCID
        private void CheckICCID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckICCID.Checked == false)
            {
                CheckFields.Remove(2);
            }
            else
            {
                CheckFields[2] = "IMEI4";
            }
        }

        //查MAC
        private void CheckMAC_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckMAC.Checked == false)
            {
                CheckFields.Remove(3);
            }
            else
            {
                CheckFields[3] = "IMEI6";
            }
        }

        //查Equipment
        private void CheckEquipment_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckEquipment.Checked == false)
            {
                CheckFields.Remove(4);
            }
            else
            {
                CheckFields[4] = "IMEI7";
            }

        }

        //查VIP
        private void CheckVIP_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckVIP.Checked == false)
            {
                CheckFields.Remove(5);
            }
            else
            {
                CheckFields[5] = "IMEI8";
            }
        }
        //查BAT
        private void CheckBAT_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckBAT.Checked == false)
            {
                CheckFields.Remove(6);
            }
            else
            {
                CheckFields[6] = "IMEI9";
            }
        }


        //查RFID
        private void CheckRFID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckRFID.Checked == false)
            {
                CheckFields.Remove(7);
            }
            else
            {
                CheckFields[7] = "IMEI13";
            }
        }


        //用户登录
        private void SiginIN_Click(object sender, EventArgs e)
        {
            SignIn sigin = new SignIn();
            sigin.ShowDialog();
            if (sigin.UserNamestr1 != "")
            {
                this.UserShow.Text = sigin.UserNamestr1;
            }
            if (sigin.Usertype1 != "")
            {
                this.UserTypeShow.Text = sigin.Usertype1;
            }
        }

        //退出
        private void QuitBt_Click(object sender, EventArgs e)
        {
           if( MessageBox.Show("是否退出当前账号？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                this.UserShow.Clear();
            }

        }



        //刷新模板函数
        private void Refresh_template_Click(object sender, EventArgs e)
        {
            if (this.Select_Template1.Text != "")
            {
                string path = this.Select_Template1.Text;
                string filename = Path.GetFileName(path);
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "机身贴模板"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\机身贴模板");
                }
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "机身贴模板\\" + RefreshNum))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\机身贴模板\\" + RefreshNum);
                }
                File.Copy(path, AppDomain.CurrentDomain.BaseDirectory + "\\机身贴模板\\" + RefreshNum + "\\" + filename, true);
                lj = AppDomain.CurrentDomain.BaseDirectory + "\\机身贴模板\\" + RefreshNum + "\\" + filename;
                this.reminder.AppendText("刷新模板成功\r\n");
                this.IMEI_Start.Focus();
                RefreshNum++;
            }
        }


        //打开数据库配置页面
        private void SqlConfig_Click(object sender, EventArgs e)
        {
            UpdateSqlConn US = new UpdateSqlConn(this);
            US.ShowDialog();
        }

    }
}
