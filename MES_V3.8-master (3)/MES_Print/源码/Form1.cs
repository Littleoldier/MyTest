﻿using System;
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
using UserAccount.Pri_Bll;
using LPrintMarkData.Param.Pri_BLL;
using System.Globalization;
using System.ComponentModel;
using System.Data;



namespace WindowsForms_print
{
    public partial class Form1 : Form
    {

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

        //彩盒
        Color_Box CB = new Color_Box();
        PrintFromExcel PFE = new PrintFromExcel();

        //账号
        LUserAccountBLL luab = new LUserAccountBLL();

        ManuFuselagePrintRecordParamBLL MFPRPB = new ManuFuselagePrintRecordParamBLL();

        List<ManuFuselagePrintRecordParam> mfprpb = new List<ManuFuselagePrintRecordParam>();

        DataRelativeSheetBLL DRSB = new DataRelativeSheetBLL();

        ManuExcelPrintParamBLL MEPPB = new ManuExcelPrintParamBLL();

        LPrintMarkDataBLL LPMDB = new LPrintMarkDataBLL();

        ManuPrintRecordParamBLL MPRPB = new ManuPrintRecordParamBLL();

        TestResultBLL TRB = new TestResultBLL();

        ManuOrderParamBLL MOPB = new ManuOrderParamBLL();

        PrintMessageBLL PMB = new PrintMessageBLL();

        InputExcelBLL IEB = new InputExcelBLL();

        List<Gps_ManuOrderParam> G_MOP = new List<Gps_ManuOrderParam>();
        List<Gps_ManuOrderParam> G_OMOP = new List<Gps_ManuOrderParam>();

        List<PrintMessage> list = new List<PrintMessage>();

        PrintMessage PList = new PrintMessage();

        DataRelativeSheet Drs = new DataRelativeSheet();

        SortedDictionary<int, string> RelationFields = new SortedDictionary<int, string>();

        SortedDictionary<int, string> CheckFields = new SortedDictionary<int, string>();

        //拼接查询字段
        string FindField;

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


        //锁定标志位
        public static int recordLuck = 0;
        public static int recordUpdateUI = 0;

        //工位标志
        public static int WorkInt = 0;
        public static int FuselageStation = 1;
        public static int ColourBoxStation = 2;
        public static int ExcelStation = 3;
        public static int QueryAndDeleteStation = 4;
        public static int RetypingQuerieslStation = 5;


        //制单记录
        public static string jSZhidanStr = "";
        public static string cHZhidanStr = "";


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
        string IMEI2str = "";
        
        //装取 分割出的 IMEI起始位 终止位
        string SlipIMEIStart;
        string SlipIMEIEnd;
        
        //记录打印时间
        string ProductTime = "";
        Engine btEngine = new Engine();
        
        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            int wid = Screen.PrimaryScreen.WorkingArea.Width;
            int hei = Screen.PrimaryScreen.WorkingArea.Height;
            //this.Height = hei;
            //this.tabControl1.Width = wid;
            //this.tabPage2.Width = wid;
            //this.ExcelToPrint.Width = wid;

            Rectangle rect = new Rectangle();
            rect = Screen.GetWorkingArea(this);


            this.tabControl1.Width = rect.Width;
            this.tabPage2.Width = rect.Width;
            this.ExcelToPrint.Width = rect.Width;

            this.tabControl1.Height = rect.Height;
            this.tabPage2.Height = rect.Height;
            this.ExcelToPrint.Height = rect.Height;

        }
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnvalue, int buffersize, string filepath);

        private string IniFilePath;

        public int QuitThreadFalge1 { get => QuitThreadFalge; set => QuitThreadFalge = value; }
        public int QuitThreadFalge2 { get => QuitThreadFalge; set => QuitThreadFalge = value; }
        public int RecordLuck { get => recordLuck; set => recordLuck = value; }
        public int RecordUpdateUI { get => recordUpdateUI; set => recordUpdateUI = value; }
        public static string JSZhidanStr { get => jSZhidanStr; set => jSZhidanStr = value; }
        public static string CHZhidanStr { get => cHZhidanStr; set => cHZhidanStr = value; }

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
            foreach (Process p in Process.GetProcessesByName("bartend"))
            {
                if (!p.CloseMainWindow())
                {
                    //p.CloseMainWindow();
                    p.Kill();
                }
            }

            tabControl1.SelectedIndex = 0;
            tabControl1.Show();
            PrintDocument print = new PrintDocument();
            string sDefault = print.PrinterSettings.PrinterName;//默认打印机名
            this.Printer1.Text = sDefault;
            foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
            {
                Printer1.Items.Add(sPrint);
            }
            G_MOP.Clear();
            G_MOP = MOPB.SelectZhidanNumBLL();
            foreach (Gps_ManuOrderParam a in G_MOP)
            {
                this.CB_ZhiDan.Items.Add(a.ZhiDan);
            }
            string NowData = System.DateTime.Now.ToString("yyyy.MM.dd");
            this.ProductData.Text = NowData;
            //开启打印机引擎
            btEngine.Start();

            WorkInt = FuselageStation;

        }

        //更换数据库时调用
        public void refrezhidan()
        {
            this.CB_ZhiDan.Items.Clear();
            G_MOP.Clear();
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
            luab.refeshConBLL();
        }

        //选择制单时引发的事件
        private void CB_ZhiDan_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.reminder.Text = "";
            ClreaUIInformation();
            
            string NowData = System.DateTime.Now.ToString("yyyy.MM.dd");
            this.ProductData.Text = NowData;
            string ZhidanNum = this.CB_ZhiDan.Text;
            //G_OMOP = MOPB.selectManuOrderParamByzhidanBLL(ZhidanNum);
 
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
            if(Form1.recordLuck == 1)
            {
                if(WorkInt == FuselageStation)
                {
                    Form1.recordLuck = 0;
                    tabControl1.SelectedIndex = 0;
                    tabControl1.Show();
                    Form1.recordLuck = 1;
                    MessageBox.Show("请解除锁定", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (WorkInt == ColourBoxStation)
                {
                    Form1.recordLuck = 0;
                    tabControl1.SelectedTab = tabPage2;
                    tabControl1.Show();
                    Form1.recordLuck = 1;
                    MessageBox.Show("请解除锁定", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                if (WorkInt == RetypingQuerieslStation)
                {
                    Form1.recordLuck = 0;
                    tabControl1.SelectedTab = tabPage3;
                    tabControl1.Show();
                    Form1.recordLuck = 1;
                    MessageBox.Show("请解除锁定", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                }
            }

            tabControl1.SelectedTab.Refresh();
            if (tabControl1.SelectedTab == tabPage2)
            {
                CB.TopLevel = false;
                tabPage2.Controls.Add(CB);
                CB.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                CB.Show();

                if(Form1.recordUpdateUI == 0 && Form1.cHZhidanStr != "")
                {
                    CB.UpdateCHUIdata();
                }

                WorkInt = ColourBoxStation;
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                CheckRePrint CRP = new CheckRePrint();
                CRP.TopLevel = false;
                tabPage3.Controls.Add(CRP);
                CRP.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                CRP.Show();

                WorkInt = RetypingQuerieslStation;
            }
            else if (tabControl1.SelectedIndex == 0)
            {
                PrintDocument print = new PrintDocument();
                string sDefault = print.PrinterSettings.PrinterName;//默认打印机名
                this.Printer1.Text = sDefault;
                foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
                {
                    Printer1.Items.Add(sPrint);
                }
                this.CB_ZhiDan.Items.Clear();
                G_MOP.Clear();
                G_MOP = MOPB.SelectZhidanNumBLL();
                foreach (Gps_ManuOrderParam a in G_MOP)
                {
                    this.CB_ZhiDan.Items.Add(a.ZhiDan);
                }
                string NowData = System.DateTime.Now.ToString("yyyy.MM.dd");
                this.ProductData.Text = NowData;
                //开启打印机引擎
                btEngine.Start();

                if (Form1.recordUpdateUI == 0 && Form1.jSZhidanStr != "")
                {
                    this.UpdateUIdata();
                }

                WorkInt = FuselageStation;
            }
        }

        //关闭程序时引发事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if ((MessageBox.Show("是否退出系统？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                //Application.Exit();
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
            if (Form1.recordLuck == 1)
            {
                if (WorkInt == FuselageStation)
                {
                    Form1.recordLuck = 0;
                    tabControl2.SelectedTab = tabPage4;
                    tabControl2.Show();
                    Form1.recordLuck = 1;
                    MessageBox.Show("请解除锁定", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (WorkInt == QueryAndDeleteStation)
                {
                    Form1.recordLuck = 0;
                    tabControl2.SelectedTab = CheckAndDelete;
                    tabControl2.Show();
                    Form1.recordLuck = 1;
                    MessageBox.Show("请解除锁定", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                if (WorkInt == ExcelStation)
                {
                    Form1.recordLuck = 0;
                    tabControl2.SelectedTab = ExcelToPrint;
                    tabControl2.Show();
                    Form1.recordLuck = 1;
                    MessageBox.Show("请解除锁定", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                }

            }
           

            if (tabControl2.SelectedTab == CheckAndDelete)
            {
                JST_CheckAndDelect JSTCAD = new JST_CheckAndDelect();
                JSTCAD.TopLevel = false;
                CheckAndDelete.Controls.Add(JSTCAD);
                JSTCAD.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                JSTCAD.Show();

                WorkInt = RetypingQuerieslStation;

            }
            else if (tabControl2.SelectedTab == ExcelToPrint)
            {
                PFE.TopLevel = false;
                ExcelToPrint.Controls.Add(PFE);
                PFE.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                PFE.Show();
                PFE.StartIpStation();
                //MessageBox.Show("该功能暂建设中...");

                WorkInt = ExcelStation;

            }
            else if (tabControl2.SelectedTab == tabPage4)
            {
                PrintDocument print = new PrintDocument();
                string sDefault = print.PrinterSettings.PrinterName;//默认打印机名
                this.Printer1.Text = sDefault;
                foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
                {
                    Printer1.Items.Add(sPrint);
                }
                this.CB_ZhiDan.Items.Clear();
                G_MOP.Clear();
                G_MOP = MOPB.SelectZhidanNumBLL();
                foreach (Gps_ManuOrderParam a in G_MOP)
                {
                    this.CB_ZhiDan.Items.Add(a.ZhiDan);
                }
                string NowData = System.DateTime.Now.ToString("yyyy.MM.dd");
                this.ProductData.Text = NowData;
                //开启打印机引擎
                btEngine.Start();


                if (Form1.recordUpdateUI == 0 && Form1.jSZhidanStr != "")
                {
                    this.UpdateUIdata();
                }

                WorkInt = FuselageStation;

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
                    ClearTemplate1ToVlue(btFormat);
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
                this.CheckIMEI14.Enabled = true;

                //rfid
                this.RFID_Start.ReadOnly = true;
                this.RFID_Check.Checked = false;


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
                    if (this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
                    {
                        this.ReImei2Num1.ReadOnly = true;
                        this.ReImei2Num2.ReadOnly = true;
                        this.ReImei2Num1.Clear();
                        this.ReImei2Num2.Clear();
                    }
                    this.RePrintMore.Checked = false;
                    this.Re_IMEINum.ReadOnly = true;
                    this.ReImeiNum1.ReadOnly = true;
                    this.ReImeiNum2.ReadOnly = true;
                    this.ReImeiNum1.Clear();
                    this.ReImeiNum2.Clear();
                    this.Re_IMEINum.Focus();

                }

                //IMEI16进制
                if(this.Hexadecimal.Checked == true)
                {
                    this.Hexadecimal.Checked = false;
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.ReadOnly = true;
                }


                this.InseIMEI2.Enabled = true;

                if(this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
                {
                    this.InseIMEI2.Checked = true;
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
                this.CheckIMEI2.Checked = false;
                this.CheckSIM.Checked = false;
                this.CheckBAT.Checked = false;
                this.CheckICCID.Checked = false;
                this.CheckMAC.Checked = false;
                this.CheckEquipment.Checked = false;
                this.CheckVIP.Checked = false;
                this.CheckRFID.Checked = false;
                this.CheckIMEI14.Checked = false;
                
                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;
                this.CheckIMEI14.Enabled = false;

                //rfid
                this.RFID_Start.ReadOnly = true;
                this.RFID_Check.Checked = false;

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
                    if (this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
                    {
                        this.ReImei2Num1.ReadOnly = true;
                        this.ReImei2Num2.ReadOnly = true;
                        this.ReImei2Num1.Clear();
                        this.ReImei2Num2.Clear();
                    }
                    this.RePrintMore.Checked = false;
                    this.ReImeiNum1.ReadOnly = true;
                    this.ReImeiNum2.ReadOnly = true;
                    this.ReImeiNum1.Clear();
                    this.ReImeiNum2.Clear();

                }

                //IMEI十六进制打印
                if(this.Hexadecimal.Checked == true)
                {
                    this.Hexadecimal.Checked = false;
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.ReadOnly = true;

                }

                this.InseIMEI2.Enabled = false;
                this.InseIMEI2.Checked = false;
                this.PrintOne.Checked = false;
                this.PrintNum.ReadOnly = false;
                this.IMEI_Start.ReadOnly = true;
                this.IMEI2_Start.ReadOnly = true;
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
                this.CheckIMEI14.Enabled = true;

                //rfid
                this.RFID_Start.ReadOnly = true;
                this.RFID_Check.Checked = false;

                //批量打印
                if (this.PrintMore.Checked == true)
                {
                    this.PrintMore.Checked = false;
                    this.PrintNum.ReadOnly = true;
                    this.PrintNum.Clear();
                    this.IMEI_Start.Focus();
                }
                
                //逐个打印
                if(this.PrintOne.Checked == true)
                {
                    this.InseIMEI2.Enabled = false;
                    this.InseIMEI2.Checked = false;
                    this.PrintOne.Checked = false;
                    this.IMEI_Start.ReadOnly = true;
                    this.IMEI2_Start.ReadOnly = true;
                    this.IMEI_Start.Clear();
                }

                //批量重打
                if(this.RePrintMore.Checked == true)
                {
                    if (this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
                    {
                        this.ReImei2Num1.ReadOnly = true;
                        this.ReImei2Num2.ReadOnly = true;
                        this.ReImei2Num1.Clear();
                        this.ReImei2Num2.Clear();
                    }
                }


                //IMEI16进制
                if (this.Hexadecimal.Checked == true)
                {
                    this.Hexadecimal.Checked = false;
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.ReadOnly = true;
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
                //rfid
                this.RFID_Start.ReadOnly = true;
                this.RFID_Check.Checked = false;

                //逐个打印
                if (this.PrintOne.Checked == true)
                {
                    this.InseIMEI2.Enabled = false;
                    this.InseIMEI2.Checked = false;
                    this.PrintOne.Checked = false;
                    this.IMEI_Start.Clear();
                    this.IMEI_Start.ReadOnly = true;
                    this.IMEI2_Start.ReadOnly = true;
                 
                }

                //批量打印
                if(this.PrintMore.Checked == true)
                {
                    this.PrintMore.Checked = false;
                    this.PrintNum.Clear();
                    this.PrintNum.ReadOnly = true;
                }

                //打印模式 1/2
                if (this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
                {
                    this.ReImei2Num1.ReadOnly = false;
                    this.ReImei2Num2.ReadOnly = false;
                    this.ReImei2Num1.Clear();
                    this.ReImei2Num2.Clear();
                }

                //IMEI16进制
                if (this.Hexadecimal.Checked == true)
                {
                    this.Hexadecimal.Checked = false;
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.ReadOnly = true;
                }
                this.CheckIMEI2.Checked = false;
                this.CheckSIM.Checked = false;
                this.CheckBAT.Checked = false;
                this.CheckICCID.Checked = false;
                this.CheckMAC.Checked = false;
                this.CheckEquipment.Checked = false;
                this.CheckVIP.Checked = false;
                this.CheckRFID.Checked = false;
                this.CheckIMEI14.Checked = false;

                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;
                this.CheckIMEI14.Enabled = false;

                
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

                //rfid
                this.RFID_Start.ReadOnly = true;
                this.RFID_Check.Checked = false;

                if (this.RePrintMore.Checked == false)
                {
                    //逐个打印
                    if (this.PrintOne.Checked == true)
                    {
                        this.InseIMEI2.Enabled = false;
                        this.InseIMEI2.Checked = false;
                        this.PrintOne.Checked = false;
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.ReadOnly = true;
                        this.IMEI2_Start.ReadOnly = true;
                    }

                    //批量打印
                    if (this.PrintMore.Checked == true)
                    {
                        this.PrintMore.Checked = false;
                        this.PrintNum.Clear();
                        this.PrintNum.ReadOnly = true;
                    }

                    //打印模式 1/2
                    if (this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
                    {
                        this.ReImei2Num1.ReadOnly = false;
                        this.ReImei2Num2.ReadOnly = false;
                        this.ReImei2Num1.Clear();
                        this.ReImei2Num2.Clear();
                    }

                    //IMEI16进制
                    if (this.Hexadecimal.Checked == true)
                    {
                        this.Hexadecimal.Checked = false;
                        this.HexPrintNum.Clear();
                        this.HexPrintNum.ReadOnly = true;
                    }
                    this.CheckIMEI2.Checked = false;
                    this.CheckSIM.Checked = false;
                    this.CheckBAT.Checked = false;
                    this.CheckICCID.Checked = false;
                    this.CheckMAC.Checked = false;
                    this.CheckEquipment.Checked = false;
                    this.CheckVIP.Checked = false;
                    this.CheckRFID.Checked = false;
                    this.CheckIMEI14.Checked = false;

                    this.CheckIMEI2.Enabled = false;
                    this.CheckSIM.Enabled = false;
                    this.CheckBAT.Enabled = false;
                    this.CheckICCID.Enabled = false;
                    this.CheckMAC.Enabled = false;
                    this.CheckEquipment.Enabled = false;
                    this.CheckVIP.Enabled = false;
                    this.CheckRFID.Enabled = false;
                    this.CheckIMEI14.Enabled = false;


                    this.RePrintOne.Checked = false;
                    this.Re_IMEINum.ReadOnly = true;
                    this.ReImeiNum1.ReadOnly = false;
                    this.ReImeiNum2.ReadOnly = false;
                    this.Re_IMEINum.Clear();
                    this.ReImeiNum1.Focus();

                    this.RePrintMore.Checked = true;
                }
            }
        }

        //非十六进制批量打印
        private void PrintNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //检查操作设置
                if(checkInformation())
                {
                    this.PrintNum.Clear();
                    this.PrintNum.Focus();
                    return;
                }
               
                if(this.ModeFalge == 0)
                {
                    try
                    {
                        if (this.PrintNum.Text != "" && IsNumeric(this.PrintNum.Text))
                        {
                            if (this.NoCheckCode.Checked == false)
                            {
                                
                                long between;
                                if (this.IMEI_Present.Text == "")
                                {
                                    between = long.Parse(SlipIMEIEnd) - long.Parse(SlipIMEIStart) + 1;
                                }
                                else
                                {
                                    between = long.Parse(SlipIMEIEnd) - long.Parse(this.IMEI_Present.Text);
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
                                ImeiDig = SlipIMEIStart.Length;
                                if (this.IMEI_Present.Text == "")
                                {
                                    Imei1Suf = int.Parse(SlipIMEIStart.Remove(0, ImeiDig - 5));
                                }
                                else
                                {
                                    Imei1Suf = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                }
                                long Imei2Suf = long.Parse(SlipIMEIEnd.Remove(0, ImeiDig - 5));
                                long between = 0;
                                between = Imei2Suf - Imei1Suf + 1;
                                if (long.Parse(this.PrintNum.Text) < 0 || long.Parse(this.PrintNum.Text) > between)
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
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                    long imei_begin = 0;
                                    string imei15 = "", sn_aft = "";
                                    string begin0 = "";
                                    
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI_Present.Text);

                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(SlipIMEIStart);
                                        begin0 = GetLength0(imei_begin, SlipIMEIStart);
                                    }

                                    sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());

                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        imei15 = getimei15(begin0 + imei_begin.ToString());
                                        btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                        PList.IMEIStart = SlipIMEIStart;
                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                            imei_begin++;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "插入失败\r\n");
                                            imei_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 1).ToString()))
                                    {
                                        this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                }
                                break;
                            case 0:
                                {
                                    long imei_begin;
                                    string imei15, sn_bef, sn_aft, sn_laf;
                                    string begin0;
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI_Present.Text);

                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(SlipIMEIStart);
                                        begin0 = GetLength0(imei_begin, SlipIMEIStart);

                                    }
                                    imei15 = getimei15(begin0+ imei_begin.ToString());
                                    string EndIMEI = begin0+ (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());

                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin++;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "插入失败\r\n");
                                                        imei_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin++;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "插入失败\r\n");
                                                        imei_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), begin0 + (imei_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei15 = getimei15(begin0 + imei_begin.ToString());
                                            btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                            btFormat.SubStrings["SN"].Value = "";
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                            PList.IMEIStart = SlipIMEIStart;
                                            PList.IMEIEnd = SlipIMEIEnd;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                imei_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "插入失败\r\n");
                                                imei_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", begin0 + (imei_begin - 1).ToString()))
                                        {
                                            this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 1:
                                {
                                    long imei_begin;
                                    string imei15, sn_bef, sn_aft, sn_laf;
                                    string begin0;
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI_Present.Text);

                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(SlipIMEIStart);
                                        begin0 = GetLength0(imei_begin, SlipIMEIStart);

                                    }
                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    if (!PMB.CheckSNBLL(sn_bef + sn_aft))
                                                    {
                                                        btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                        //记录打印信息日志
                                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                            imei_begin++;
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "插入失败\r\n");
                                                            imei_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    if (!PMB.CheckSNBLL(SNHexNum))
                                                    {
                                                        btFormat.SubStrings["SN"].Value = SNHexNum;
                                                        //记录打印信息日志
                                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                            imei_begin++;
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "插入失败\r\n");
                                                            imei_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), begin0 + (imei_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei15 = getimei15(begin0 + imei_begin.ToString());
                                            btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                            btFormat.SubStrings["SN"].Value = "";
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                            PList.IMEIStart = SlipIMEIStart;
                                            PList.IMEIEnd = SlipIMEIEnd;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                imei_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "插入失败\r\n");
                                                imei_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", begin0 + (imei_begin - 1).ToString()))
                                        {
                                            this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 2:
                                {
                                    long imei_begin;
                                    string sn_bef, sn_aft, sn_laf;
                                    string imei_begin_pre = SlipIMEIStart.Substring(0, ImeiDig - 5);
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(SlipIMEIStart.Remove(0, ImeiDig - 5));
                                    }

                                    string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                    
                                 
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin++;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                        imei_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
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
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin++;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                        imei_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
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
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
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
                                            PList.IMEIStart = SlipIMEIStart;
                                            PList.IMEIEnd = SlipIMEIEnd;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                imei_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }

                                break;
                            case 3:
                                {
                                    long imei_begin;
                                    string sn_bef, sn_aft, sn_laf;
                                    string imei_begin_pre = SlipIMEIStart.Substring(0, ImeiDig - 5);
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(SlipIMEIStart.Remove(0, ImeiDig - 5));
                                    }
                                    string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                  
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
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
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin++;
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                            imei_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
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
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
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
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin++;
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                            imei_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
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
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
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
                                            PList.IMEIStart = SlipIMEIStart;
                                            PList.IMEIEnd = SlipIMEIEnd;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                imei_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 6:
                                {
                                    long imei_begin;
                                    string sn_aft;
                                    sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                    string imei_begin_pre = SlipIMEIStart.Substring(0, ImeiDig - 5);
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(SlipIMEIStart.Remove(0, ImeiDig - 5));
                                    }
                                    string EndIMEI = (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString();

                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
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
                                        PList.IMEIStart = SlipIMEIStart;
                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin++;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                            imei_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0')))
                                    {
                                        this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
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
                else if(this.ModeFalge == 1)
                {
                    try
                    {
                        if (this.PrintNum.Text != "" && IsNumeric(this.PrintNum.Text))
                        {
                            if(this.IMEI_num1.Text == this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("起始位相等\r\n");
                                this.PrintNum.Clear();
                                this.PrintNum.Focus();
                                return;
                            }
                            if (this.IMEI_num2.Text == this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("终止位相等\r\n");
                                this.PrintNum.Clear();
                                this.PrintNum.Focus();
                                return;
                            }
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
                                    this.reminder.AppendText(this.PrintNum.Text + "IMEI1超出范围\r\n");
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }

                                long between2;
                                if (this.IMEI2_Present.Text == "")
                                {
                                    between2 = long.Parse(this.IMEI2_num2.Text) - long.Parse(this.IMEI2_num1.Text) + 1;
                                }
                                else
                                {
                                    between2 = long.Parse(this.IMEI2_num2.Text) - long.Parse(this.IMEI2_Present.Text);
                                }
                                if (int.Parse(this.PrintNum.Text) < 0 || int.Parse(this.PrintNum.Text) > between2)
                                {
                                    player.Play();
                                    this.reminder.AppendText(this.PrintNum.Text + "IMEI2超出范围\r\n");
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

                                long Imei1Suf2;
                                ImeiDig = this.IMEI2_num1.Text.Length;
                                if (this.IMEI2_Present.Text == "")
                                {
                                    Imei1Suf2 = long.Parse(this.IMEI2_num1.Text.Remove(0, ImeiDig - 5));
                                }
                                else
                                {
                                    Imei1Suf2 = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                }
                                long Imei2Suf2 = long.Parse(this.IMEI2_num2.Text.Remove(0, ImeiDig - 5));
                                long between2 = Imei2Suf2 - Imei1Suf2 + 1;
                                if (int.Parse(this.PrintNum.Text) < 0 || int.Parse(this.PrintNum.Text) > between2)
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

                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                    long imei_begin, imei2_begin;
                                    string imei15, sn_aft ,imei2_15;
                                    string begin0;
                                    string begin2;
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI_Present.Text);
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text);
                                        begin0 = GetLength0(imei_begin, this.IMEI_num1.Text);

                                    }
                                    sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                   

                                    imei15 = getimei15(begin0+imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin0+(imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                  
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                        begin2 = GetLength0(imei2_begin, this.IMEI2_Present.Text);
                                    }
                                    else
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_num1.Text);
                                        begin2 = GetLength0(imei2_begin, this.IMEI2_num1.Text);

                                    }
                                    if (imei2_begin.ToString().Length != 14)
                                    {
                                        this.reminder.AppendText("IMEI2长度不为14位\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                    string EndIMEI2 = begin2 + (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin2 + (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(begin2 + imei2_begin.ToString() + imei2_15, EndIMEI2);
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2.Substring(0, 14) + "重号\r\n");
                                        }
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        imei15 = getimei15(begin0 + imei_begin.ToString());
                                        imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                        btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                        btFormat.SubStrings["IMEI2"].Value = begin2 + imei2_begin.ToString() + imei2_15;
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
                                        PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                        PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                        PList.IMEI2 = begin2 + imei2_begin.ToString() + imei2_15;
                                        PList.IMEI2Start = this.IMEI2_num1.Text.Trim();
                                        PList.IMEI2End = this.IMEI2_num2.Text.Trim();
                                        PList.SN = "";
                                        PList.IMEIRel = this.IMEIRel.Text.Trim();
                                        //PList.IMEI2Rel = this.IMEI2Rel.Text.Trim();
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            Drs.Claer();
                                            Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                            Drs.IMEI2 = "";
                                            Drs.IMEI14 = begin2 + imei2_begin.ToString() + imei2_15;
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);

                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                            imei_begin++;
                                            imei2_begin++;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与"+ begin2 + imei2_begin.ToString() + imei2_15+"插入失败\r\n");
                                            imei_begin++;
                                            imei2_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 1).ToString(), begin2 + (imei2_begin - 1).ToString()))
                                    {
                                        this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                        this.IMEI2_Present.Text = begin2 + (imei2_begin - 1).ToString();
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                }
                                break;
                            case 0:
                                {
                                    long imei_begin;
                                    string imei15, sn_bef, sn_aft, sn_laf;
                                    string begin0;
                                    if (this.IMEI_Present.Text != "")
                                    {

                                        imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI_Present.Text);
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text);
                                        begin0 = GetLength0(imei_begin, this.IMEI_num1.Text);
                                    }
                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                    

                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin;
                                    string imei2_15;
                                    string begin2;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                        begin2 = GetLength0(imei2_begin, this.IMEI_Present.Text);

                                    }
                                    else
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_num1.Text);
                                        begin2 = GetLength0(imei2_begin, this.IMEI_Present.Text);

                                    }
                                    imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                    string EndIMEI2 = begin2 + (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin2 + (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(begin2 + imei2_begin.ToString() + imei2_15, EndIMEI2);
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2.Substring(0, 14) + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin2 + imei2_begin.ToString() + imei2_15;
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = begin2 + imei2_begin.ToString() + imei2_15;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                        Drs.IMEI2 = sn_bef + sn_aft;
                                                        Drs.IMEI14 = begin2 + imei2_begin.ToString() + imei2_15;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin++;
                                                        imei2_begin++;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin2 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                        imei_begin++;
                                                        imei2_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 1).ToString(), begin2 + (imei2_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.IMEI2_Present.Text = begin2 + (imei2_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin2 + imei2_begin.ToString() + imei2_15;
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = begin2 + imei2_begin.ToString() + imei2_15;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                        Drs.IMEI2 = SNHexNum;
                                                        Drs.IMEI14 = imei2_begin.ToString() + imei2_15;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin++;
                                                        imei2_begin++;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin2 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                        imei_begin++;
                                                        imei2_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), begin0 + (imei_begin - 1).ToString(), begin2 + (imei2_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.IMEI2_Present.Text = begin2 + (imei2_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei15 = getimei15(begin0 + imei_begin.ToString());
                                            imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                            btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                            btFormat.SubStrings["IMEI2"].Value = begin2 + imei2_begin.ToString() + imei2_15;
                                            btFormat.SubStrings["SN"].Value = "";
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = begin2 + imei2_begin.ToString() + imei2_15;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = begin2 + imei2_begin.ToString() + imei2_15;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin2 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", begin0 + (imei_begin - 1).ToString(), begin2 + (imei2_begin - 1).ToString()))
                                        {
                                            this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                            this.IMEI2_Present.Text = begin2 + (imei2_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 1:
                                {
                                    long imei_begin;
                                    string imei15, sn_bef, sn_aft, sn_laf;
                                    string begin0;
                                    if (this.IMEI_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI_Present.Text);
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text);
                                        begin0 = GetLength0(imei_begin, this.IMEI_num1.Text);

                                    }


                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin0 + (imei_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                    
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin;
                                    string imei2_15;
                                    string begin2;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                        begin2 = GetLength0(imei2_begin, this.IMEI_Present.Text);

                                    }
                                    else
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_num1.Text);
                                        begin2 = GetLength0(imei2_begin, this.IMEI2_num1.Text);

                                    }
                                    if (imei2_begin.ToString().Length != 14)
                                    {
                                        this.reminder.AppendText("IMEI2长度不为14位\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                    string EndIMEI2 = begin2 + (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15(begin2 + (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(begin2 + imei2_begin.ToString() + imei2_15, EndIMEI2);
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2.Substring(0, 14) + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin2 + imei2_begin.ToString() + imei2_15;
                                                    if (!PMB.CheckSNBLL(sn_bef + sn_aft))
                                                    {
                                                        btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                        //记录打印信息日志
                                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = begin2 + imei2_begin.ToString() + imei2_15;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                            Drs.IMEI2 = sn_bef + sn_aft;
                                                            Drs.IMEI14 = begin2 + imei2_begin.ToString() + imei2_15;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                            imei_begin++;
                                                            imei2_begin++;
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin2 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                            imei_begin++;
                                                            imei2_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 1).ToString(), begin2 + (imei2_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.IMEI2_Present.Text = begin2 + (imei2_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin2 + imei2_begin.ToString() + imei2_15;
                                                    if (!PMB.CheckSNBLL(SNHexNum))
                                                    {
                                                        btFormat.SubStrings["SN"].Value = SNHexNum;
                                                        //记录打印信息日志
                                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = begin2 + imei2_begin.ToString() + imei2_15;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                            Drs.IMEI2 = SNHexNum;
                                                            Drs.IMEI14 = begin2 + imei2_begin.ToString() + imei2_15;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                            imei_begin++;
                                                            imei2_begin++;
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin2 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                            imei_begin++;
                                                            imei2_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), begin0 + (imei_begin - 1).ToString(), begin2 + (imei2_begin - 1).ToString()))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                                this.IMEI2_Present.Text = begin2 + (imei2_begin - 1).ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei15 = getimei15(begin0 + imei_begin.ToString());
                                            imei2_15 = getimei15(begin2 + imei2_begin.ToString());
                                            btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                            btFormat.SubStrings["IMEI2"].Value = begin2 + imei2_begin.ToString() + imei2_15;
                                            btFormat.SubStrings["SN"].Value = "";
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = begin2 + imei2_begin.ToString() + imei2_15;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = begin2 + imei2_begin.ToString() + imei2_15;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin2 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", begin0 + (imei_begin - 1).ToString(), begin2 + (imei2_begin - 1).ToString()))
                                        {
                                            this.IMEI_Present.Text = begin0 + (imei_begin - 1).ToString();
                                            this.IMEI2_Present.Text = begin2 + (imei2_begin - 1).ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                    
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin;
                                    string imei2_begin_pre = this.IMEI2_num1.Text.Substring(0, ImeiDig - 5);
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_num1.Text.Remove(0, ImeiDig - 5));
                                    }


                                    string EndIMEI2 = (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0'), imei2_begin_pre + EndIMEI2.PadLeft(5, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                        Drs.IMEI2 = sn_bef + sn_aft;
                                                        Drs.IMEI14 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin++;
                                                        imei2_begin++;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                        imei_begin++;
                                                        imei2_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                        Drs.IMEI2 = SNHexNum;
                                                        Drs.IMEI14 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin++;
                                                        imei2_begin++;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                        imei_begin++;
                                                        imei2_begin++;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                            btFormat.SubStrings["IMEI2"].Value = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.IMEI2_Present.Text = imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                  
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin;
                                    string imei2_begin_pre = this.IMEI2_num1.Text.Substring(0, ImeiDig - 5);
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_num1.Text.Remove(0, ImeiDig - 5));
                                    }
                                    if (imei2_begin.ToString().Length != 14)
                                    {
                                        this.reminder.AppendText("IMEI2长度不为14位\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    string EndIMEI2 = (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0'), imei2_begin_pre + EndIMEI2.PadLeft(5, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                            Drs.IMEI2 = sn_bef + sn_aft;
                                                            Drs.IMEI14 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin++;
                                                            imei2_begin++;
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                            imei_begin++;
                                                            imei2_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                            Drs.IMEI2 = SNHexNum;
                                                            Drs.IMEI14 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin++;
                                                            imei2_begin++;
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                            imei_begin++;
                                                            imei2_begin++;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                            btFormat.SubStrings["IMEI2"].Value = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                imei_begin++;
                                                imei2_begin++;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0')))
                                        {
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.IMEI2_Present.Text = imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                   
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin;
                                    string imei2_begin_pre = this.IMEI2_num1.Text.Substring(0, ImeiDig - 5);
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei2_begin = long.Parse(this.IMEI2_num1.Text.Remove(0, ImeiDig - 5));
                                    }
                                    string EndIMEI2 = (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0'), imei2_begin_pre + EndIMEI2.PadLeft(5, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
                                        }
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                        btFormat.SubStrings["IMEI2"].Value = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        PList.IMEI2 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            Drs.Claer();
                                            Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                            Drs.IMEI2 = "";
                                            Drs.IMEI14 = imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);

                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin++;
                                            imei2_begin++;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei2_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                            imei_begin++;
                                            imei2_begin++;
                                        }
                                    }
                                    if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0')))
                                    {
                                        this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                        this.IMEI2_Present.Text = imei2_begin_pre + (imei2_begin - 1).ToString().PadLeft(5, '0');
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
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
                else if( this.ModeFalge == 2)
                {
                    try
                    {
                        if (this.PrintNum.Text != "" && IsNumeric(this.PrintNum.Text))
                        {
                            //检查两个起始结束范围是否相等
                            if(this.IMEI_num1.Text != this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI1-IMEI2起始位不等\r\n");
                                this.PrintNum.Clear();
                                this.PrintNum.Focus();
                                return;

                            }
                            if (this.IMEI_num2.Text != this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI1-IMEI2终止位不等\r\n");
                                this.PrintNum.Clear();
                                this.PrintNum.Focus();
                                return;

                            }

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
                                if ((int.Parse(this.PrintNum.Text)*2) < 0 || (int.Parse(this.PrintNum.Text)*2) > between)
                                {
                                    player.Play();
                                    this.reminder.AppendText(this.PrintNum.Text + "IMEI1超出范围\r\n");
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }

                                //long between2;
                                //if (this.IMEI2_Present.Text == "")
                                //{
                                //    between2 = long.Parse(this.IMEI2_num2.Text) - long.Parse(this.IMEI2_num1.Text) + 1;
                                //}
                                //else
                                //{
                                //    between2 = long.Parse(this.IMEI2_num2.Text) - long.Parse(this.IMEI2_Present.Text);
                                //}
                                //if (int.Parse(this.PrintNum.Text) < 0 || int.Parse(this.PrintNum.Text) > between2)
                                //{
                                //    player.Play();
                                //    this.reminder.AppendText(this.PrintNum.Text + "IMEI2超出范围\r\n");
                                //    this.PrintNum.Clear();
                                //    this.PrintNum.Focus();
                                //    return;
                                //}
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
                                if ((int.Parse(this.PrintNum.Text)*2) < 0 || (int.Parse(this.PrintNum.Text)*2) > between)
                                {
                                    player.Play();
                                    this.reminder.AppendText(this.PrintNum.Text + "超出范围\r\n");
                                    this.PrintNum.Clear();
                                    this.PrintNum.Focus();
                                    return;
                                }

                                //long Imei1Suf2;
                                //ImeiDig = this.IMEI2_num1.Text.Length;
                                //if (this.IMEI2_Present.Text == "")
                                //{
                                //    Imei1Suf2 = long.Parse(this.IMEI2_num1.Text.Remove(0, ImeiDig - 5));
                                //}
                                //else
                                //{
                                //    Imei1Suf2 = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                //}
                                //long Imei2Suf2 = long.Parse(this.IMEI2_num2.Text.Remove(0, ImeiDig - 5));
                                //long between2 = Imei2Suf2 - Imei1Suf2 + 1;
                                //if (int.Parse(this.PrintNum.Text) < 0 || int.Parse(this.PrintNum.Text) > between2)
                                //{
                                //    player.Play();
                                //    this.reminder.AppendText(this.PrintNum.Text + "超出范围\r\n");
                                //    this.PrintNum.Clear();
                                //    this.PrintNum.Focus();
                                //    return;
                                //}
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

                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                    long imei_begin, imei2_begin;
                                    string imei15, sn_aft, imei2_15;
                                    string begin0;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        
                                        imei_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI2_Present.Text);
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text);
                                        begin0 = GetLength0(imei_begin, this.IMEI_num1.Text);

                                    }
                                    sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString() + getimei15(begin0 + (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString());
                                    

                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    //if (this.IMEI2_Present.Text != "")
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                    //}
                                    //else
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_num1.Text);
                                    //}

                                    //imei2_15 = getimei15(imei2_begin.ToString());
                                    //string EndIMEI2 = (imei2_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString() + getimei15((imei2_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString());
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(begin0 + imei_begin.ToString() + imei15, EndIMEI);
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2.Substring(0, 14) + "重号\r\n");
                                        }
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    imei2_begin = 0;
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        imei2_begin = imei_begin + 1;
                                        imei15 = getimei15(begin0 + imei_begin.ToString());
                                        imei2_15 = getimei15(begin0 + imei2_begin.ToString());
                                        btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                        btFormat.SubStrings["IMEI2"].Value = begin0 + imei2_begin.ToString() + imei2_15;
                                        //记录打印信息日志
                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        PList.Claer();
                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        PList.IMEI2 = begin0 + imei2_begin.ToString() + imei2_15;
                                        PList.IMEI2Start = this.IMEI2_num1.Text.Trim();
                                        PList.IMEI2End = this.IMEI2_num2.Text.Trim();
                                        //PList.IMEI2Rel = this.IMEI2Rel.Text.Trim();
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            Drs.Claer();
                                            Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                            Drs.IMEI2 = "";
                                            Drs.IMEI14 = begin0 + imei2_begin.ToString() + imei2_15;
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);

                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                            imei_begin = imei_begin+2;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15+"与" + begin0 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                            imei_begin = imei_begin + 2;
                                        }
                                    }
                                    if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 2).ToString(), begin0 + imei2_begin .ToString()))
                                    {
                                        this.IMEI_Present.Text = begin0 + (imei_begin - 2).ToString();
                                        this.IMEI2_Present.Text = begin0 + imei2_begin.ToString();
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                }
                                break;
                            case 0:
                                {
                                    long imei_begin;
                                    string imei15, sn_bef, sn_aft, sn_laf;
                                    string begin0 ;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI2_Present.Text);
                                        
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text);
                                        begin0 = GetLength0(imei_begin, this.IMEI_num1.Text);
                                        
                                    }
                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString() + getimei15(begin0 + (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString());
                                  
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin = 0;
                                    string imei2_15;
                                    //if (this.IMEI2_Present.Text != "")
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                    //}
                                    //else
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_num1.Text);
                                    //}
                                    //imei2_15 = getimei15(imei2_begin.ToString());
                                    //string EndIMEI2 = (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15((imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                    //list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(begin0 + imei_begin.ToString() + imei15, EndIMEI);
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2.Substring(0, 14) + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin0 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin0 + imei2_begin.ToString() + imei2_15;
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = begin0 + imei2_begin.ToString() + imei2_15;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;

                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                        Drs.IMEI2 = sn_bef + sn_aft;
                                                        Drs.IMEI14 = begin0 + imei2_begin.ToString() + imei2_15;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin = imei_begin + 2;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin0 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                        imei_begin = imei_begin + 2;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 2).ToString(), begin0 + imei2_begin.ToString()))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 2).ToString();
                                                this.IMEI2_Present.Text = begin0 + imei2_begin.ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin0 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin0 + imei2_begin.ToString() + imei2_15;
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = begin0 + imei2_begin.ToString() + imei2_15;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                        Drs.IMEI2 = SNHexNum;
                                                        Drs.IMEI14 = begin0 + imei2_begin.ToString() + imei2_15;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                        imei_begin = imei_begin + 2;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin0 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                        imei_begin = imei_begin + 2;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), begin0 + (imei_begin - 2).ToString(), begin0 + imei2_begin .ToString()))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 2).ToString();
                                                this.IMEI2_Present.Text = begin0 + imei2_begin .ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei2_begin = imei_begin + 1;
                                            imei15 = getimei15(begin0 + imei_begin.ToString());
                                            imei2_15 = getimei15(begin0 + imei2_begin.ToString());
                                            btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                            btFormat.SubStrings["IMEI2"].Value = begin0 + imei2_begin.ToString() + imei2_15;
                                            btFormat.SubStrings["SN"].Value = "";
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = begin0 + imei2_begin.ToString() + imei2_15;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = begin0 + imei2_begin.ToString() + imei2_15;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                imei_begin = imei_begin + 2;

                                            }
                                            else
                                            {
                                                this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin0 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                imei_begin = imei_begin + 2;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", begin0 + (imei_begin - 2).ToString(), begin0 + imei2_begin.ToString()))
                                        {
                                            this.IMEI_Present.Text = begin0 + (imei_begin - 2).ToString();
                                            this.IMEI2_Present.Text = begin0 + imei2_begin.ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 1:
                                {
                                    long imei_begin;
                                    string imei15, sn_bef, sn_aft, sn_laf;
                                    string begin0 ;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                        begin0 = GetLength0(imei_begin, this.IMEI2_Present.Text);
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text);
                                        begin0 = GetLength0(imei_begin, this.IMEI_num1.Text);

                                    }

                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    string EndIMEI = begin0 + (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString() + getimei15(begin0 + (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString());
                                  
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(begin0 + imei_begin.ToString() + imei15, EndIMEI))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin = 0;
                                    string imei2_15;
                                    //if (this.IMEI2_Present.Text != "")
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_Present.Text) + 1;
                                    //}
                                    //else
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_num1.Text);
                                    //}
                                    //imei2_15 = getimei15(imei2_begin.ToString());
                                    //string EndIMEI2 = (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString() + getimei15((imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString());
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(begin0 + imei_begin.ToString() + imei15, EndIMEI);
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2.Substring(0, 14) + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin0 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin0 + imei2_begin.ToString() + imei2_15;
                                                    if (!PMB.CheckSNBLL(sn_bef + sn_aft))
                                                    {
                                                        btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                        //记录打印信息日志
                                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = begin0 + imei2_begin.ToString() + imei2_15;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                            Drs.IMEI2 = sn_bef + sn_aft; ;
                                                            Drs.IMEI14 = begin0 + imei2_begin.ToString() + imei2_15;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                            imei_begin = imei_begin + 2;
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin0 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                            imei_begin = imei_begin + 2;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, begin0 + (imei_begin - 2).ToString(), begin0 + imei2_begin .ToString()))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 2).ToString();
                                                this.IMEI2_Present.Text = begin0 + imei2_begin.ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                                    imei2_15 = getimei15(begin0 + imei2_begin.ToString());
                                                    btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                                    btFormat.SubStrings["IMEI2"].Value = begin0 + imei2_begin.ToString() + imei2_15;
                                                    if (!PMB.CheckSNBLL(SNHexNum))
                                                    {
                                                        btFormat.SubStrings["SN"].Value = SNHexNum;
                                                        //记录打印信息日志
                                                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = begin0 + imei2_begin.ToString() + imei2_15;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {

                                                            Drs.Claer();
                                                            Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                            Drs.IMEI2 = SNHexNum;
                                                            Drs.IMEI14 = begin0 + imei2_begin.ToString() + imei2_15;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                            imei_begin = imei_begin + 2;
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin0 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                            imei_begin = imei_begin + 2;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), begin0 + (imei_begin - 2).ToString(), begin0 + imei2_begin .ToString()))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = begin0 + (imei_begin - 2).ToString();
                                                this.IMEI2_Present.Text = begin0 + imei2_begin.ToString();
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei2_begin = imei_begin + 1;
                                            imei15 = getimei15(begin0 + imei_begin.ToString());
                                            imei2_15 = getimei15(begin0 + imei2_begin.ToString());
                                            btFormat.SubStrings["IMEI"].Value = begin0 + imei_begin.ToString() + imei15;
                                            btFormat.SubStrings["IMEI2"].Value = begin0 + imei2_begin.ToString() + imei2_15;
                                            btFormat.SubStrings["SN"].Value = "";
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = begin0 + imei_begin.ToString() + imei15;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = begin0 + imei2_begin.ToString() + imei2_15;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = begin0 + imei_begin.ToString() + imei15;
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = begin0 + imei2_begin.ToString() + imei2_15;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + imei15 + "的制单", null);
                                                imei_begin = imei_begin + 2;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(begin0 + imei_begin.ToString() + imei15 + "与" + begin0 + imei2_begin.ToString() + imei2_15 + "插入失败\r\n");
                                                imei_begin = imei_begin + 2;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", begin0 + (imei_begin - 2).ToString(), begin0 + imei2_begin.ToString()))
                                        {
                                            this.IMEI_Present.Text = begin0 + (imei_begin - 2).ToString();
                                            this.IMEI2_Present.Text = begin0 + imei2_begin.ToString();
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 2:
                                {
                                    long imei_begin;
                                    string sn_bef, sn_aft, sn_laf;
                                    string imei_begin_pre = this.IMEI_num1.Text.Substring(0, ImeiDig - 5);
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text.Remove(0, ImeiDig - 5));
                                    }
                                    string EndIMEI = (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString();
                                 

                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin = 0;
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                        Drs.IMEI2 = sn_bef + sn_aft ;
                                                        Drs.IMEI14 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = imei_begin + 2;
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                        imei_begin = imei_begin + 2;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei_begin_pre + imei2_begin .ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                        Drs.IMEI2 = SNHexNum;
                                                        Drs.IMEI14 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = imei_begin + 2;
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                        imei_begin = imei_begin + 2;
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0'), imei_begin_pre + imei2_begin .ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei2_begin = imei_begin + 1;
                                            btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                            btFormat.SubStrings["IMEI2"].Value = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = imei_begin + 2;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                imei_begin = imei_begin + 2;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0'), imei_begin_pre + imei2_begin .ToString().PadLeft(5, '0')))
                                        {
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 1).ToString().PadLeft(5, '0');
                                            this.IMEI2_Present.Text = imei_begin_pre + imei2_begin .ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 3:
                                {
                                    long imei_begin;
                                    string sn_bef, sn_aft, sn_laf;
                                    string imei_begin_pre = this.IMEI_num1.Text.Substring(0, ImeiDig - 5);
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text.Remove(0, ImeiDig - 5));
                                    }
                                    if (imei_begin.ToString().Length != 14)
                                    {
                                        this.reminder.AppendText("IMEI长度不为14位\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    string EndIMEI = (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString();
                                  
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    long imei2_begin = 0;
                                    //string imei2_begin_pre = this.IMEI2_num1.Text.Substring(0, ImeiDig - 5);
                                    //if (this.IMEI2_Present.Text != "")
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    //}
                                    //else
                                    //{
                                    //    imei2_begin = long.Parse(this.IMEI2_num1.Text.Remove(0, ImeiDig - 5));
                                    //}
                                    //string EndIMEI2 = (imei2_begin + int.Parse(this.PrintNum.Text) - 1).ToString();
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                            Drs.IMEI2 = sn_bef + sn_aft;
                                                            Drs.IMEI14 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = imei_begin + 2;
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                            imei_begin = imei_begin + 2;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0'), imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei2_begin = imei_begin + 1;
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                    btFormat.SubStrings["IMEI2"].Value = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                            Drs.IMEI2 = SNHexNum;
                                                            Drs.IMEI14 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = imei_begin + 2;
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                            imei_begin = imei_begin + 2;
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0'), imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0');
                                                this.IMEI2_Present.Text = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                        {
                                            imei2_begin = imei_begin + 1;
                                            btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                            btFormat.SubStrings["IMEI2"].Value = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = imei_begin + 2;
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                                imei_begin = imei_begin + 2;
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0'), imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0')))
                                        {
                                            this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0');
                                            this.IMEI2_Present.Text = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = long.Parse(this.IMEI2_Present.Text.Remove(0, ImeiDig - 5)) + 1;
                                    }
                                    else
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text.Remove(0, ImeiDig - 5));
                                    }
                                    string EndIMEI = (imei_begin + (int.Parse(this.PrintNum.Text)*2) - 1).ToString();
                                    

                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0')))
                                    {
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }

                                    long imei2_begin = 0;
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0'), imei_begin_pre + EndIMEI.PadLeft(5, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
                                        }
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
                                    }
                                    for (int i = 0; i < int.Parse(this.PrintNum.Text); i++)
                                    {
                                        imei2_begin = imei_begin + 1;
                                        btFormat.SubStrings["IMEI"].Value = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                        btFormat.SubStrings["IMEI2"].Value = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        PList.IMEI2 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            Drs.Claer();
                                            Drs.IMEI1 = imei_begin_pre + imei_begin.ToString().PadLeft(5, '0');
                                            Drs.IMEI2 = "";
                                            Drs.IMEI14 = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);

                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin = imei_begin + 2;
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(imei_begin_pre + imei_begin.ToString().PadLeft(5, '0') + "与" + imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0') + "插入失败\r\n");
                                            imei_begin = imei_begin + 2;
                                        }
                                    }
                                    if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0'), imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0')))
                                    {
                                        this.IMEI_Present.Text = imei_begin_pre + (imei_begin - 2).ToString().PadLeft(5, '0');
                                        this.IMEI2_Present.Text = imei_begin_pre + imei2_begin.ToString().PadLeft(5, '0');
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
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
        }

        //十六进制批量打印
        private void HexPrintNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //检查操作设置
                if (checkInformation())
                {
                    this.HexPrintNum.Clear();
                    this.HexPrintNum.Focus();
                    return;
                }
                
                if(this.ModeFalge == 0)
                {
                    try
                    {
                        if (this.HexPrintNum.Text != "" && IsNumeric(this.HexPrintNum.Text))
                        {
                            long HexBetween;
                            long HexNum1;
                            if (this.IMEI_Present.Text == "")
                            {
                                HexNum1 = Convert.ToInt64(SlipIMEIStart, 16);
                            }
                            else
                            {
                                HexNum1 = Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16);
                            }
                            HexBetween = Convert.ToInt64(SlipIMEIEnd, 16) - HexNum1 + Convert.ToInt64("1", 16);
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
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                        imei_begin = (Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                    }
                                    else
                                    {
                                        imei_begin = SlipIMEIStart;
                                    }
                                    long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin, EndIMEI.ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                    {
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["SN"].Value = sn_bef + sn_aft;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin;
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin + "插入失败\r\n");
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1|| SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["SN"].Value = SNHexNum;
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = imei_begin;
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin + "插入失败\r\n");
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
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
                                            PList.IMEIStart = SlipIMEIStart;
                                            PList.IMEIEnd = SlipIMEIEnd;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin + "插入失败\r\n");
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                        {
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                        imei_begin = (Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                    }
                                    else
                                    {
                                        imei_begin = SlipIMEIStart;
                                    }
                                    long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                    {
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
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
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin + "插入失败\r\n");
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                            {

                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
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
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin + "插入失败\r\n");
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
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
                                            PList.IMEIStart = SlipIMEIStart;
                                            PList.IMEIEnd = SlipIMEIEnd;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin + "插入失败\r\n");
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                            }
                                        }
                                        if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                        {
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                        imei_begin = (Convert.ToInt64(this.IMEI_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                    }
                                    else
                                    {
                                        imei_begin = SlipIMEIStart;
                                    }
                                    long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                   
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                    {
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
                                        PList.IMEIStart = SlipIMEIStart;
                                        PList.IMEIEnd = SlipIMEIEnd;
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(imei_begin + "插入失败\r\n");
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                        }
                                    }
                                    if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0')))
                                    {
                                        this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(SlipIMEIStart.Length, '0');
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
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
                if(this.ModeFalge == 1)
                {
                    try
                    {
                        if (this.HexPrintNum.Text != "" && IsNumeric(this.HexPrintNum.Text))
                        {
                            if (this.IMEI_num1.Text == this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("起始位相等\r\n");
                                this.HexPrintNum.Clear();
                                this.HexPrintNum.Focus();
                                return;
                            }
                            if (this.IMEI_num2.Text == this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("终止位相等\r\n");
                                this.HexPrintNum.Clear();
                                this.HexPrintNum.Focus();
                                return;
                            }

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

                            long HexBetween_2;
                            long HexNum1_2;
                            if (this.IMEI2_Present.Text == "")
                            {
                                HexNum1_2 = Convert.ToInt64(this.IMEI2_num1.Text, 16);
                            }
                            else
                            {
                                HexNum1_2 = Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16);
                            }
                            HexBetween_2 = Convert.ToInt64(this.IMEI2_num2.Text, 16) - HexNum1 + Convert.ToInt64("1", 16);
                            if (int.Parse(this.HexPrintNum.Text) < 0 || int.Parse(this.HexPrintNum.Text) > HexBetween_2)
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


                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                   
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin, EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }

                                    string imei2_begin;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                    }
                                    else
                                    {
                                        imei2_begin = this.IMEI2_num1.Text;
                                    }
                                    long EndIMEI2 = Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei2_begin, EndIMEI.ToString("X").PadLeft(IMEI2_num1.Text.Length, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei2_begin;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin;
                                                        Drs.IMEI2 = sn_bef + sn_aft;
                                                        Drs.IMEI14 = imei2_begin;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin + "与"+ imei2_begin + "插入失败\r\n");
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei2_begin;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin;
                                                        Drs.IMEI2 = SNHexNum;
                                                        Drs.IMEI14 = imei2_begin;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {
                                            btFormat.SubStrings["IMEI"].Value = imei_begin;
                                            btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei2_begin;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin;
                                                Drs.IMEI2 ="";
                                                Drs.IMEI14 = imei2_begin;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0')))
                                        {
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.IMEI2_Present.Text = (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                   
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }

                                    string imei2_begin;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                    }
                                    else
                                    {
                                        imei2_begin = this.IMEI_num1.Text;
                                    }
                                    long EndIMEI2 = Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei2_begin.ToString(), EndIMEI2.ToString("X").PadLeft(IMEI2_num1.Text.Length, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei2_begin;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin;
                                                            Drs.IMEI2 = sn_bef + sn_aft;
                                                            Drs.IMEI14 = imei2_begin;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                            imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                            imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0')))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei2_begin;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin;
                                                            Drs.IMEI2 = SNHexNum;
                                                            Drs.IMEI14 = imei2_begin;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                            imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                            imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0')))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {
                                            btFormat.SubStrings["IMEI"].Value = imei_begin;
                                            btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei2_begin;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin;
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = imei2_begin;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0')))
                                        {
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.IMEI2_Present.Text = (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
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
                                   
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }

                                    string imei2_begin;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei2_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                    }
                                    else
                                    {
                                        imei2_begin = this.IMEI2_num1.Text;
                                    }
                                    long EndIMEI2 = Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei2_begin.ToString(), EndIMEI2.ToString("X").PadLeft(IMEI2_num1.Text.Length, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
                                        }
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }
                                    for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                    {
                                        btFormat.SubStrings["IMEI"].Value = imei_begin;
                                        btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        PList.IMEI2 = imei2_begin;
                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            Drs.Claer();
                                            Drs.IMEI1 = imei_begin;
                                            Drs.IMEI2 = "";
                                            Drs.IMEI14 = imei2_begin;
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);

                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            imei2_begin = (Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                        }
                                    }
                                    if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0')))
                                    {
                                        this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        this.IMEI2_Present.Text = (Convert.ToInt64(imei2_begin, 16) - Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
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
                if(this.ModeFalge == 2)
                {
                    try
                    {
                        if (this.HexPrintNum.Text != "" && IsNumeric(this.HexPrintNum.Text))
                        {
                            if (this.IMEI_num1.Text != this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("起始位不相等\r\n");
                                this.HexPrintNum.Clear();
                                this.HexPrintNum.Focus();
                                return;
                            }
                            if (this.IMEI_num2.Text != this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("终止位不相等\r\n");
                                this.HexPrintNum.Clear();
                                this.HexPrintNum.Focus();
                                return;
                            }

                            //检查两个起始结束范围是否相等
                            if (this.IMEI_num1.Text != this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI1-IMEI2起始位不等\r\n");
                                this.PrintNum.Clear();
                                this.PrintNum.Focus();
                                return;

                            }
                            if (this.IMEI_num2.Text != this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI1-IMEI2终止位不等\r\n");
                                this.PrintNum.Clear();
                                this.PrintNum.Focus();
                                return;

                            }

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
                            if ((int.Parse(this.HexPrintNum.Text)*2) < 0 || (int.Parse(this.HexPrintNum.Text)*2) > HexBetween)
                            {
                                player.Play();
                                this.reminder.AppendText(this.HexPrintNum.Text + "超出范围\r\n");
                                this.HexPrintNum.Clear();
                                this.HexPrintNum.Focus();
                                return;
                            }

                            long HexBetween_2;
                            long HexNum1_2;
                            if (this.IMEI2_Present.Text == "")
                            {
                                HexNum1_2 = Convert.ToInt64(this.IMEI2_num1.Text, 16);
                            }
                            else
                            {
                                HexNum1_2 = Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16);
                            }
                            HexBetween_2 = Convert.ToInt64(this.IMEI2_num2.Text, 16) - HexNum1 + Convert.ToInt64("1", 16);
                            if ((int.Parse(this.HexPrintNum.Text)*2) < 0 || (int.Parse(this.HexPrintNum.Text)*2) > HexBetween_2)
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

                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                    }
                                    else
                                    {
                                        imei_begin = this.IMEI_num1.Text;
                                    }
                                    long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64((int.Parse(this.HexPrintNum.Text)*2).ToString(), 16) - Convert.ToInt64("1", 16);
                                    
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin, EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }

                                    string imei2_begin = "";
                                    //if (this.IMEI_Present.Text != "")
                                    //{
                                    //    imei2_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                    //}
                                    //else
                                    //{
                                    //    imei2_begin = this.IMEI2_num1.Text;
                                    //}
                                    //long EndIMEI2 = Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei_begin, EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei2_begin =  (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei2_begin;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin;
                                                        Drs.IMEI2 = sn_bef + sn_aft;
                                                        Drs.IMEI14 = imei2_begin;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), imei2_begin))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = imei2_begin;
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei2_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = imei2_begin;
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_begin;
                                                        Drs.IMEI2 = SNHexNum;
                                                        Drs.IMEI14 = imei2_begin;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);

                                                        btFormat.Print();
                                                        //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                        SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    }
                                                    else
                                                    {
                                                        this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                        imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                    }
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), imei2_begin))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = imei2_begin;
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {

                                            imei2_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            btFormat.SubStrings["IMEI"].Value = imei_begin;
                                            btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei2_begin;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin;
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = imei2_begin;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            }
                                            else
                                            {
                                                this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), imei2_begin))
                                        {
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.IMEI2_Present.Text = imei2_begin;
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 3:
                                {
                                    string imei_begin;
                                    string sn_bef, sn_aft, sn_laf;
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                    }
                                    else
                                    {
                                        imei_begin = this.IMEI_num1.Text;
                                    }
                                    long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64((int.Parse(this.HexPrintNum.Text)*2).ToString(), 16) - Convert.ToInt64("1", 16);
                                   
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }
                                    string imei2_begin = "";
                                    //if (this.IMEI2_Present.Text != "")
                                    //{
                                    //    imei2_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                    //}
                                    //else
                                    //{
                                    //    imei2_begin = this.IMEI_num1.Text;
                                    //}
                                    //long EndIMEI2 = Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
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
                                                if (int.Parse(sn_aft) <= int.Parse(sn_laf))
                                                {
                                                    imei2_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei2_begin;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin;
                                                            Drs.IMEI2 = sn_bef + sn_aft;
                                                            Drs.IMEI14 = imei2_begin;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                            sn_aft = (int.Parse(sn_aft) + 1).ToString().PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), imei2_begin))
                                            {
                                                this.SN1_num.Text = sn_bef + sn_aft;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = imei2_begin;
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            string SNHexNum = this.SN1_num.Text;
                                            string SNHexNum2 = this.SN2_num.Text;
                                            SNHexNum = Convert.ToInt64(SNHexNum, 16).ToString("X");
                                            SNHexNum2 = Convert.ToInt64(SNHexNum2, 16).ToString("X");
                                            for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                            {
                                                if (SNHexNum.CompareTo(SNHexNum2) == -1 || SNHexNum.CompareTo(SNHexNum2) == 0)
                                                {
                                                    imei2_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                                    btFormat.SubStrings["IMEI"].Value = imei_begin;
                                                    btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        PList.IMEI2 = imei2_begin;
                                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Drs.Claer();
                                                            Drs.IMEI1 = imei_begin;
                                                            Drs.IMEI2 = SNHexNum;
                                                            Drs.IMEI14 = imei2_begin;
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);

                                                            btFormat.Print();
                                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                            SNHexNum = (Convert.ToInt64(SNHexNum, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        }
                                                        else
                                                        {
                                                            this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
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
                                                    this.reminder.AppendText("SN号不足\r\n");
                                                    return;
                                                }
                                            }
                                            if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, SNHexNum.Remove(0, this.SN1_num.Text.Length - s), (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), imei2_begin))
                                            {
                                                this.SN1_num.Text = SNHexNum;
                                                this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                this.IMEI2_Present.Text = imei2_begin;
                                                this.HexPrintNum.Clear();
                                                this.HexPrintNum.Focus();
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText("更新制单sn号失败\r\n");
                                                this.PrintNum.Clear();
                                                this.PrintNum.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                        {
                                            imei2_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                            btFormat.SubStrings["IMEI"].Value = imei_begin;
                                            btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = imei2_begin;
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                Drs.Claer();
                                                Drs.IMEI1 = imei_begin;
                                                Drs.IMEI2 = "";
                                                Drs.IMEI14 = imei2_begin;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);

                                                btFormat.Print();
                                                //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                                imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                                
                                            }
                                        }
                                        if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, "", (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), imei2_begin))
                                        {
                                            this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                            this.IMEI2_Present.Text = imei2_begin;
                                            this.HexPrintNum.Clear();
                                            this.HexPrintNum.Focus();
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText("更新制单sn号失败\r\n");
                                            this.PrintNum.Clear();
                                            this.PrintNum.Focus();
                                            return;
                                        }
                                    }
                                }
                                break;
                            case 6:
                                {
                                    string imei_begin;
                                    string sn_aft;
                                    sn_aft = SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                    if (this.IMEI2_Present.Text != "")
                                    {
                                        imei_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                    }
                                    else
                                    {
                                        imei_begin = this.IMEI_num1.Text;
                                    }
                                    long EndIMEI = Convert.ToInt64(imei_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                 
                                    //批量打印查询打印表和镭雕打印表IMEI号是否重号
                                    if (Check_MP_LP_Print(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0')))
                                    {
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }

                                    string imei2_begin = "";
                                    //if (this.IMEI_Present.Text != "")
                                    //{
                                    //    imei2_begin = (Convert.ToInt64(this.IMEI2_Present.Text, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                    //}
                                    //else
                                    //{
                                    //    imei2_begin = this.IMEI2_num1.Text;
                                    //}
                                    //long EndIMEI2 = Convert.ToInt64(imei2_begin, 16) + Convert.ToInt64(this.HexPrintNum.Text, 16) - Convert.ToInt64("1", 16);
                                    list.Clear();
                                    list = PMB.CheckRangeIMEI_2BLL(imei_begin.ToString(), EndIMEI.ToString("X").PadLeft(IMEI_num1.Text.Length, '0'));
                                    if (list.Count > 0)
                                    {
                                        foreach (PrintMessage a in list)
                                        {
                                            this.reminder.AppendText(a.IMEI2 + "重号\r\n");
                                        }
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                        return;
                                    }
                                    for (int i = 0; i < int.Parse(this.HexPrintNum.Text); i++)
                                    {

                                        imei2_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(IMEI2_num1.Text.Length, '0');
                                        btFormat.SubStrings["IMEI"].Value = imei_begin;
                                        btFormat.SubStrings["IMEI2"].Value = imei2_begin;
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
                                        PList.JSUserDes = this.UserDesShow.Text;
                                        PList.IMEI2 = imei2_begin;
                                        PList.IMEI2Start = this.IMEI2_num1.Text;
                                        PList.IMEI2End = this.IMEI2_num2.Text;
                                        //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                        if (PMB.InsertPrintMessageBLL(PList))
                                        {
                                            Drs.Claer();
                                            Drs.IMEI1 = imei_begin;
                                            Drs.IMEI2 = "";
                                            Drs.IMEI14 = imei2_begin;
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);

                                            btFormat.Print();
                                            //Form1.Log("批量打印了IMEI号为" + imei_begin + "的制单", null);
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        }
                                        else
                                        {
                                            this.reminder.AppendText(imei_begin + "与" + imei2_begin + "插入失败\r\n");
                                            imei_begin = (Convert.ToInt64(imei_begin, 16) + Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        }
                                    }
                                    if (MOPB.UpdateIMEI2SNnumberBLL(this.CB_ZhiDan.Text, sn_aft, (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0'), imei2_begin))
                                    {
                                        this.IMEI_Present.Text = (Convert.ToInt64(imei_begin, 16) - Convert.ToInt64("2", 16)).ToString("X").PadLeft(IMEI_num1.Text.Length, '0');
                                        this.IMEI2_Present.Text = imei2_begin;
                                        this.HexPrintNum.Clear();
                                        this.HexPrintNum.Focus();
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新制单sn号失败\r\n");
                                        this.PrintNum.Clear();
                                        this.PrintNum.Focus();
                                        return;
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
        }

        public string SplicingCheckSQLStr(string FieldNumber)
        {
            string[] FindFieldstr = FindField.Split(',');
            string Sqlstr = "IMEI1 = '" + FieldNumber + "' OR ";
            string De = "= '";
            string Or = "' OR ";

            for (int i = 0; i < FindFieldstr.Count() - 1; i++)
            {
                Sqlstr += FindFieldstr[i] + De + FieldNumber + Or;
            }

            return Sqlstr = Sqlstr.Substring(0, Sqlstr.Length - 3);
        }

        //逐个打印
        private void IMEI_Start_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //检查操作设置
                if (checkInformation())
                {
                    this.IMEI_Start.Clear();
                    this.IMEI_Start.Focus();
                    return;
                }
                
                //分割字符串
                this.IMEI_Start.Text = SustringPos(this.IMEI_Start.Text);

                string strField = DRSB.SelectIMEIFieldBLL(SplicingCheckSQLStr(this.IMEI_Start.Text));
                

                if (strField != "")
                {
                    this.IMEI_Start.Text = strField;
                }
                
                if(this.InseIMEI2.Checked == false)
                {
                    if (this.ModeFalge == 0)
                    {
                        try
                        {
                            if (this.CB_ZhiDan.Text != "")
                            {
                                if (this.NoCheckCode.Checked == false)
                                {
                                    string imei14;
                                    string imeiRes = "";
                                    if (this.IMEI_Start.Text != "" && IsNumeric(this.IMEI_Start.Text) && this.IMEI_Start.Text.Length == 15)
                                    {
                                        imei14 = this.IMEI_Start.Text.Substring(0, 14);
                                        long IMEI_Startlong = long.Parse(imei14);
                                        if (IMEI_Startlong < long.Parse(SlipIMEIStart) || IMEI_Startlong > long.Parse(SlipIMEIEnd) )
                                        {
                                           
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI_Start.Text + "IMEI不在范围内\r\n");
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
                                        if (this.IMEI_Start.Text.Length != SlipIMEIStart.Length)
                                        {
                                            this.reminder.AppendText("IMEI号位数与起始位数不一致\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                            
                                        }
                                        if (this.IMEI_Start.Text.CompareTo(SlipIMEIStart) == -1 || this.IMEI_Start.Text.CompareTo(SlipIMEIEnd) == 1)
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
                            if (this.Select_Template1.Text != "")
                            {
                                LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                                ClearTemplate1ToVlue(btFormat);
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
                                               if(!CheckFieldsChoice(this.IMEI_Start.Text, 4,1,0, btFormat))
                                               { 
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                               }
                                            }

                                            if(!LPMDB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
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
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        //long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                        //if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn1_suffix.ToString().PadLeft(s, '0'), (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                                        //{
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        //long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                        //this.IMEI_Present.Text = imei_star14.ToString();
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                        //}
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
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI_Start.Text + "镭雕表重号\r\n");
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
                                                if (!CheckFieldsChoice(this.IMEI_Start.Text,0, 1,0,btFormat))
                                                {
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                                }
                                            }

                                            if (!LPMDB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                                if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    //对模板相应字段进行赋值
                                                    ValueToTemplate(btFormat);
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");

                                                    if (this.SN1_num.Text != "")
                                                    {
                                                        if (this.CheckIMEI2.Checked == false)
                                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
                                                        if (this.CheckIMEI2.Checked == true)
                                                            PList.SN = snstr;
                                                        else
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
                                                        PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                        PList.JSUserName = this.UserShow.Text;
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            if (this.CheckIMEI2.Checked == false)
                                                            {
                                                                if (this.SNHex.Checked == false)
                                                                {
                                                                    string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                                    string sn2_suffix = this.SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                                    this.SN1_num.Text = sn1;
                                                                }
                                                                else
                                                                {
                                                                    string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                                    string Hex = this.SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                                                    string sn_16str = (Convert.ToInt64(Hex, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                                    string sn1 = sn1_prefix + sn_16str;
                                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_16str);
                                                                    this.SN1_num.Text = sn1;
                                                                }

                                                            }
                                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                            Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                            this.IMEI_Start.Clear();
                                                            this.IMEI_Start.Focus();

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (this.CheckIMEI2.Checked == false)
                                                            btFormat.SubStrings["SN"].Value = "";
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
                                                        if (this.CheckIMEI2.Checked == true)
                                                            PList.SN = snstr;
                                                        else
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
                                                        PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                        PList.JSUserName = this.UserShow.Text;
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                            Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                            this.IMEI_Start.Clear();
                                                            this.IMEI_Start.Focus();

                                                        }
                                                    }

                                                }
                                                else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                                {
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = snstr;

                                                    }
                                                    else
                                                    {
                                                        list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                        foreach (PrintMessage a in list)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = a.SN;
                                                        }

                                                    }
                                                    if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                    }
                                                    else
                                                    {
                                                        player.Play();
                                                        this.reminder.AppendText("更新打印失败\r\n");
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
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI_Start.Text + "镭雕表重号\r\n");
                                                this.IMEI_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        break;

                                    //客供SN
                                    case 1:
                                        {
                                            if (CheckFields.Count != 0)
                                            {
                                                if (!CheckFieldsChoice(this.IMEI_Start.Text, 1, 1,0,btFormat))
                                                {
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                                }
                                            }

                                            if (!LPMDB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
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
                                                            if (this.CheckIMEI2.Checked == true)
                                                            {
                                                                btFormat.SubStrings["SN"].Value = snstr;
                                                            }
                                                            else
                                                            {
                                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                            }
                                                            PList.Claer();
                                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                                            PList.IMEIStart = SlipIMEIStart;
                                                            PList.IMEIEnd = SlipIMEIEnd;
                                                            if (this.CheckIMEI2.Checked == true)
                                                            {
                                                                PList.SN = snstr;
                                                            }
                                                            else
                                                            {
                                                                PList.SN = this.SN1_num.Text;
                                                            }
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
                                                            PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                            PList.JSUserName = this.UserShow.Text;
                                                            PList.JSUserDes = this.UserDesShow.Text;
                                                            if (PMB.InsertPrintMessageBLL(PList))
                                                            {
                                                                if (this.CheckIMEI2.Checked == false)
                                                                {
                                                                    string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                                    this.SN1_num.Text = sn1;

                                                                }
                                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                                Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                                this.IMEI_Start.Clear();
                                                                this.IMEI_Start.Focus();
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
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = snstr;

                                                        }
                                                        else
                                                        {
                                                            btFormat.SubStrings["SN"].Value = "";
                                                        }
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            PList.SN = snstr;
                                                        }
                                                        else
                                                        {
                                                            PList.SN = "";
                                                        }
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
                                                        PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                        PList.JSUserName = this.UserShow.Text;
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            //if (MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, "", (long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1).ToString()))
                                                            //{
                                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                            Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                            //long imei_star14 = long.Parse(this.IMEI_Start.Text.Substring(0, 14)) + 1;
                                                            //this.IMEI_Present.Text = imei_star14.ToString();
                                                            this.IMEI_Start.Clear();
                                                            this.IMEI_Start.Focus();

                                                            //}
                                                        }
                                                    }
                                                }
                                                else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                                {
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = snstr;
                                                    }
                                                    else
                                                    {
                                                        list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                        foreach (PrintMessage a in list)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = a.SN;
                                                        }
                                                    }

                                                    if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                    }
                                                    else
                                                    {
                                                        player.Play();
                                                        this.reminder.AppendText("更新打印失败\r\n");
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
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI_Start.Text + "镭雕表重号\r\n");
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
                                                if (!CheckFieldsChoice(this.IMEI_Start.Text, 2,1, 0,btFormat))
                                                {
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                                }
                                            }

                                            if (!LPMDB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                                if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    //对模板相应字段进行赋值
                                                    ValueToTemplate(btFormat);
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    if (this.SN1_num.Text != "")
                                                    {
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = snstr;
                                                        }
                                                        else
                                                        {
                                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                        }
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            PList.SN = snstr;
                                                        }
                                                        else
                                                        {
                                                            PList.SN = this.SN1_num.Text;
                                                        }
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
                                                        PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                        PList.JSUserName = this.UserShow.Text;
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            if (this.CheckIMEI2.Checked == false)
                                                            {
                                                                //string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                                //long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                                //string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                                //string sn2_suffix = SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                                //MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                                //this.SN1_num.Text = sn1;
                                                                if (this.SNHex.Checked == false)
                                                                {
                                                                    string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                                    string sn2_suffix = this.SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                                    this.SN1_num.Text = sn1;
                                                                }
                                                                else
                                                                {
                                                                    string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                                    string Hex = this.SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                                                    string sn_16str = (Convert.ToInt64(Hex, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                                    string sn1 = sn1_prefix + sn_16str;
                                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_16str);
                                                                    this.SN1_num.Text = sn1;
                                                                }
                                                            }
                                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                            Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                            //this.IMEI_Present.Text = (long.Parse(this.IMEI_Start.Text) + 1).ToString();
                                                            this.IMEI_Start.Clear();
                                                            this.IMEI_Start.Focus();

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = snstr;
                                                        }
                                                        else
                                                        {
                                                            btFormat.SubStrings["SN"].Value = "";
                                                        }
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            PList.SN = snstr;
                                                        }
                                                        else
                                                        {
                                                            PList.SN = "";
                                                        }
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
                                                        PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                        PList.JSUserName = this.UserShow.Text;
                                                        PList.JSUserDes = this.UserDesShow.Text;
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
                                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                            Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                            this.IMEI_Start.Clear();
                                                            this.IMEI_Start.Focus();
                                                        }
                                                    }
                                                }
                                                else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                                {
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = snstr;
                                                    }
                                                    else
                                                    {
                                                        list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                        foreach (PrintMessage a in list)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = a.SN;
                                                        }
                                                    }
                                                    if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                    }
                                                    else
                                                    {
                                                        player.Play();
                                                        this.reminder.AppendText("更新打印失败\r\n");
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
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI_Start.Text + "镭雕表重号\r\n");
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
                                                if (!CheckFieldsChoice(this.IMEI_Start.Text, 3, 1,0,btFormat))
                                                {
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                                }
                                            }

                                            if (!LPMDB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
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
                                                            if (this.CheckIMEI2.Checked == true)
                                                            {
                                                                btFormat.SubStrings["SN"].Value = snstr;

                                                            }
                                                            else
                                                            {
                                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                            }
                                                            PList.Claer();
                                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                                            PList.IMEIStart = SlipIMEIStart;
                                                            PList.IMEIEnd = SlipIMEIEnd;
                                                            if (this.CheckIMEI2.Checked == true)
                                                            {
                                                                PList.SN = snstr;
                                                            }
                                                            else
                                                            {
                                                                PList.SN = this.SN1_num.Text;
                                                            }
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
                                                            PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                            PList.JSUserName = this.UserShow.Text;
                                                            PList.JSUserDes = this.UserDesShow.Text;
                                                            if (PMB.InsertPrintMessageBLL(PList))
                                                            {

                                                                if (this.CheckIMEI2.Checked == false)
                                                                {
                                                                    string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                                    this.SN1_num.Text = sn1;
                                                                }

                                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                                Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                                this.IMEI_Start.Clear();
                                                                this.IMEI_Start.Focus();

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
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = snstr;

                                                        }
                                                        else
                                                        {
                                                            btFormat.SubStrings["SN"].Value = "";

                                                        }
                                                        PList.Claer();
                                                        PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                        PList.IMEI = this.IMEI_Start.Text.Trim();
                                                        PList.IMEIStart = SlipIMEIStart;
                                                        PList.IMEIEnd = SlipIMEIEnd;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            PList.SN = snstr;

                                                        }
                                                        else
                                                        {
                                                            PList.SN = "";

                                                        }
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
                                                        PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                        PList.JSUserName = this.UserShow.Text;
                                                        PList.JSUserDes = this.UserDesShow.Text;
                                                        if (PMB.InsertPrintMessageBLL(PList))
                                                        {
                                                            Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                            Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                            this.IMEI_Start.Clear();
                                                            this.IMEI_Start.Focus();
                                                        }
                                                    }
                                                }
                                                else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                                {
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = snstr;

                                                    }
                                                    else
                                                    {
                                                        list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                        foreach (PrintMessage a in list)
                                                        {
                                                            btFormat.SubStrings["SN"].Value = a.SN;
                                                        }
                                                    }

                                                    if (MOPB.UpdateJSmesBLL(this.IMEI_Start.Text, ProductTime, lj))
                                                    {
                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了机身贴IMEI号为" + this.IMEI_Start.Text + "的制单", null);
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI_Start.Focus();
                                                    }
                                                    else
                                                    {
                                                        player.Play();
                                                        this.reminder.AppendText("更新打印失败\r\n");
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
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI_Start.Text + "镭雕表重号\r\n");
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
                                                if (!CheckFieldsChoice(this.IMEI_Start.Text, 6, 1,0,btFormat))
                                                {
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                                }
                                            }
                                            
                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);

                                            if (!LPMDB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                                if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    //记录打印信息日志
                                                    ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = this.IMEI_Start.Text.Trim();
                                                    PList.IMEIStart = SlipIMEIStart;
                                                    PList.IMEIEnd = SlipIMEIEnd;
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
                                                    PList.IMEI2 = IMEI2str != "" ? IMEI2str : "";
                                                    PList.JSUserName = this.UserShow.Text;
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {

                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + "的制单", null);
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
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI_Start.Text + "镭雕表重号\r\n");
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
               


                // 打印模式1 || 打印模式2
                if(this.ModeFalge == 1 || this.ModeFalge == 2 ||  this.InseIMEI2.Checked == true)
                {
                    try
                    {

                        if(this.ModeFalge == 1)
                        {
                            if (this.IMEI_num1.Text == this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("起始位相等\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                            if (this.IMEI_num2.Text == this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("终止位相等\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                        }

                        if (this.ModeFalge == 2)
                        {
                            if (this.IMEI_num1.Text != this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("起始位不相等\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                            if (this.IMEI_num2.Text != this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("终止位不相等\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                        }

                        if (this.NoCheckCode.Checked == false)
                        {
                            string imei14;
                            string imeiRes = "";
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
                        
                        if (this.Select_Template1.Text != "")
                        {
                            
                            //查询镭雕IMEI
                            if (!LPMDB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                {
                                    this.IMEI2_Start.Focus();
                                    return;
                                }
                                else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                {
                                    this.IMEI2_Start.Focus();
                                    return;
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                    this.IMEI_Start.Clear();
                                    this.IMEI_Start.Focus();
                                    return;
                                }
                            }
                            else
                            {
                                player.Play();
                                this.reminder.AppendText(this.IMEI_Start.Text + "镭雕表重号\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
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
        }


        //IMEI2单个打印
        private void IMEI2_Start_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                if (this.IMEI_Start.Text == "")
                {
                    player.Play();
                    this.reminder.AppendText("IMEI不能为空\r\n");
                    this.IMEI2_Start.Clear();
                    this.IMEI_Start.Focus();
                    return;
                }

                try
                {
                    if (this.CB_ZhiDan.Text != "")
                    {
                        //分割字符串
                        this.IMEI2_Start.Text = SustringPos(this.IMEI2_Start.Text);

                        if (this.NoCheckCode.Checked == false)
                        {
                            string imei14;
                            string imeiRes = "";
                            if (this.IMEI2_Start.Text != "" && IsNumeric(this.IMEI2_Start.Text) && this.IMEI2_Start.Text.Length == 15)
                            {
                                imei14 = this.IMEI2_Start.Text.Substring(0, 14);
                                long IMEI2_Startlo = long.Parse(imei14);
                                if (IMEI2_Startlo < long.Parse(this.IMEI2_num1.Text))
                                {
                                    player.Play();
                                    this.reminder.AppendText(this.IMEI2_Start + "IMEI2不在范围内\r\n");
                                    this.IMEI2_Start.Clear();
                                    this.IMEI2_Start.Focus();
                                    return;
                                }
                                else if (IMEI2_Startlo > long.Parse(this.IMEI2_num2.Text))
                                {
                                    player.Play();
                                    this.reminder.AppendText(this.IMEI2_Start + "IMEI2不在范围内\r\n");
                                    this.IMEI2_Start.Clear();
                                    this.IMEI2_Start.Focus();
                                    return;
                                }
                                else
                                {
                                    string imei15 = getimei15(imei14);
                                    imeiRes = imei14 + imei15;
                                    if (imeiRes != this.IMEI2_Start.Text)
                                    {
                                        player3.Play();
                                        this.reminder.AppendText(this.IMEI2_Start.Text + "IMEI2校验错误\r\n");
                                        this.IMEI2_Start.Clear();
                                        this.IMEI2_Start.Focus();
                                        return;
                                    }
                                }
                            }
                            else if (this.IMEI_Start.Text == "")
                            {
                                player.Play();
                                this.reminder.AppendText("请输入IMEI2\r\n");
                                this.IMEI2_Start.Focus();
                                return;
                            }
                            else
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2格式错误\r\n");
                                this.IMEI2_Start.Clear();
                                this.IMEI2_Start.Focus();
                                return;
                            }
                        }
                        else
                        {

                            if (this.IMEI2_Start.Text != "")
                            {
                                if (this.IMEI2_Start.Text.Length != this.IMEI2_num1.Text.Length)
                                {
                                    this.reminder.AppendText("IMEI2号位数与起始位数不一致\r\n");
                                    this.IMEI2_Start.Clear();
                                    this.IMEI2_Start.Focus();
                                    return;
                                }
                                if (this.IMEI2_Start.Text.CompareTo(this.IMEI2_num1.Text) == -1 || this.IMEI2_Start.Text.CompareTo(this.IMEI2_num2.Text) == 1)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI2不在范围内\r\n");
                                    this.IMEI2_Start.Clear();
                                    this.IMEI2_Start.Focus();
                                    return;
                                }
                            }
                            else
                            {
                                this.IMEI2_Start.Focus();
                                return;
                            }
                        }
                    }
                    else
                    {
                        player2.Play();
                        this.reminder.AppendText("请选择制单号\r\n");
                        this.IMEI2_Start.Clear();
                        this.IMEI2_Start.Focus();
                        return;
                    }
                    if (this.Select_Template1.Text != "")
                    {
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                        if (!CheckFieldsChoice(this.IMEI_Start.Text, 4, 2, 0,btFormat))
                                        {
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    btFormat.SubStrings["IMEI2"].Value = this.IMEI2_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;

                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                }
                                                else
                                                {
                                                    //记录关联数据信息到关联表
                                                    Drs.Claer();
                                                    Drs.IMEI1 = this.IMEI_Start.Text;
                                                    Drs.IMEI2 = "";
                                                    Drs.IMEI3 = simstr != "" ? simstr : "";
                                                    Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                    Drs.IMEI5 = "";
                                                    Drs.IMEI6 = macstr != "" ? macstr : "";
                                                    Drs.IMEI7 = equistr != "" ? equistr : "";
                                                    Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                    Drs.IMEI9 = batstr != "" ? batstr : "";
                                                    Drs.IMEI10 = "";
                                                    Drs.IMEI11 = "";
                                                    Drs.IMEI12 = "";
                                                    Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                    Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                    Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                    Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    DRSB.InsertRelativeSheetBLL(Drs);
                                                }

                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                this.IMEI_Start.Clear();
                                                this.IMEI2_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                          
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI2_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //没有客供，打印校验码和SN号
                            case 0:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        if (!CheckFieldsChoice(this.IMEI_Start.Text, 0, 2, 0,btFormat))
                                        {
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    btFormat.SubStrings["IMEI2"].Value = this.IMEI2_Start.Text;

                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))
                                        {
                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            if (this.CheckIMEI2.Checked == true)
                                            {
                                                btFormat.SubStrings["SN"].Value = snstr;

                                            }
                                            else
                                            {
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;

                                            }
                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = this.IMEI_Start.Text.Trim();
                                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                            if (this.CheckIMEI2.Checked == true)
                                            {
                                                PList.SN = snstr;

                                            }
                                            else
                                            {
                                                PList.SN = this.SN1_num.Text;

                                            }
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
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {

                                                if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                }
                                                else
                                                {
                                                    //记录关联数据信息到关联表
                                                    Drs.Claer();
                                                    Drs.IMEI1 = this.IMEI_Start.Text;
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        Drs.IMEI2 = snstr;
                                                    }
                                                    else
                                                    {
                                                        Drs.IMEI2 = this.SN1_num.Text;
                                                    }
                                                    Drs.IMEI3 = simstr != "" ? simstr : "";
                                                    Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                    Drs.IMEI5 = "";
                                                    Drs.IMEI6 = macstr != "" ? macstr : "";
                                                    Drs.IMEI7 = equistr != "" ? equistr : "";
                                                    Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                    Drs.IMEI9 = batstr != "" ? batstr : "";
                                                    Drs.IMEI10 = "";
                                                    Drs.IMEI11 = "";
                                                    Drs.IMEI12 = "";
                                                    Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                    Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                    Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                    Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    DRSB.InsertRelativeSheetBLL(Drs);
                                                }


                                                if (this.CheckIMEI2.Checked == false)
                                                {
                                                    string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    long sn1_suffix = long.Parse(this.SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                    string sn2_suffix = SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                    this.SN1_num.Text = sn1;

                                                }
                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                this.IMEI_Start.Clear();
                                                this.IMEI2_Start.Clear();
                                                this.IMEI_Start.Focus();

                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
    
                                    }
                                    else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                    {
                                        if(!PMB.CheckJSIMEI2BLL(this.IMEI2_Start.Text))
                                        {
                                            if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))
                                            {
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                    foreach (PrintMessage a in list)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = a.SN;
                                                    }
                                                }

                                                if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                }

                                                if (MOPB.UpdateJSmesIMEI2BLL(this.IMEI_Start.Text, this.IMEI2_Start.Text, ProductTime, this.UserShow.Text, this.UserDesShow.Text, this.IMEI2_num1.Text, this.IMEI2_num2.Text, lj))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("更新打印失败\r\n");
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                                this.IMEI_Start.Clear();
                                                this.IMEI2_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }


                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI_Start.Text + "已绑定\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI2_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }

                                }
                                break;

                            //客供SN
                            case 1:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        if (!CheckFieldsChoice(this.IMEI_Start.Text, 1, 2, 0,btFormat))
                                        {
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }

                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    btFormat.SubStrings["IMEI2"].Value = this.IMEI2_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))
                                        {
                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            if (this.SN1_num.Text != "")
                                            {
                                                if (!PMB.CheckSNBLL(this.SN1_num.Text))
                                                {
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = snstr;

                                                    }
                                                    else
                                                    {
                                                        btFormat.SubStrings["SN"].Value = this.SN1_num.Text;

                                                    }
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = this.IMEI_Start.Text.Trim();
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        PList.SN = snstr;

                                                    }
                                                    else
                                                    {
                                                        PList.SN = this.SN1_num.Text;

                                                    }
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                        {
                                                            DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                        }
                                                        else
                                                        {
                                                            //记录关联数据信息到关联表
                                                            Drs.Claer();
                                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                                            if (this.CheckIMEI2.Checked == true)
                                                            {
                                                                Drs.IMEI2 = snstr;
                                                            }
                                                            else
                                                            {
                                                                Drs.IMEI2 = this.SN1_num.Text;
                                                            }
                                                            Drs.IMEI3 = simstr != "" ? simstr : "";
                                                            Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                            Drs.IMEI5 = "";
                                                            Drs.IMEI6 = macstr != "" ? macstr : "";
                                                            Drs.IMEI7 = equistr != "" ? equistr : "";
                                                            Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                            Drs.IMEI9 = batstr != "" ? batstr : "";
                                                            Drs.IMEI10 = "";
                                                            Drs.IMEI11 = "";
                                                            Drs.IMEI12 = "";
                                                            Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                            Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);
                                                        }

                                                        if (this.CheckIMEI2.Checked == false)
                                                        {
                                                            string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                            long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                            string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                            MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                            this.SN1_num.Text = sn1;

                                                        }

                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI2_Start.Clear();
                                                        this.IMEI_Start.Focus();
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
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                            else
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    btFormat.SubStrings["SN"].Value = "";

                                                }
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    PList.SN = snstr;
                                                }
                                                else
                                                {
                                                    PList.SN = "";

                                                }

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
                                                PList.JSUserDes = this.UserDesShow.Text;
                                                PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                                PList.IMEI2Start = this.IMEI2_num1.Text;
                                                PList.IMEI2End = this.IMEI2_num2.Text;
                                                //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                    {
                                                        DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                    }
                                                    else
                                                    {
                                                        //记录关联数据信息到关联表
                                                        Drs.Claer();
                                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            Drs.IMEI2 = snstr;
                                                        }
                                                        else
                                                        {
                                                            Drs.IMEI2 = "";
                                                        }
                                                        Drs.IMEI3 = simstr != "" ? simstr : "";
                                                        Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                        Drs.IMEI5 = "";
                                                        Drs.IMEI6 = macstr != "" ? macstr : "";
                                                        Drs.IMEI7 = equistr != "" ? equistr : "";
                                                        Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                        Drs.IMEI9 = batstr != "" ? batstr : "";
                                                        Drs.IMEI10 = "";
                                                        Drs.IMEI11 = "";
                                                        Drs.IMEI12 = "";
                                                        Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                        Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);
                                                    }

                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }

                                    }
                                    else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 1))
                                    {
                                        if (!PMB.CheckJSIMEI2BLL(this.IMEI2_Start.Text)) //查绑定
                                        {
                                            if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))//查重号
                                            {
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                    foreach (PrintMessage a in list)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = a.SN;
                                                    }
                                                }

                                                if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                }

                                                if (MOPB.UpdateJSmesIMEI2BLL(this.IMEI_Start.Text, this.IMEI2_Start.Text, ProductTime, this.UserShow.Text, this.UserDesShow.Text, this.IMEI2_num1.Text, this.IMEI2_num2.Text, lj))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("更新打印失败\r\n");
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                                this.IMEI_Start.Clear();
                                                this.IMEI2_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                            
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI_Start.Text + "已绑定\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }

                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI2_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //不打印校验码
                            case 2:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        if (!CheckFieldsChoice(this.IMEI_Start.Text, 2, 2, 0,btFormat))
                                        {
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }
                                    
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    btFormat.SubStrings["IMEI2"].Value = this.IMEI2_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))
                                        {
                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            if (this.SN1_num.Text != "")
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    btFormat.SubStrings["SN"].Value = this.SN1_num.Text;

                                                }
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    PList.SN = snstr;
                                                }
                                                else
                                                {
                                                    PList.SN = this.SN1_num.Text;

                                                }
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
                                                PList.JSUserDes = this.UserDesShow.Text;
                                                PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                                PList.IMEI2Start = this.IMEI2_num1.Text;
                                                PList.IMEI2End = this.IMEI2_num2.Text;
                                                //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                    {
                                                        DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                    }
                                                    else
                                                    {
                                                        //记录关联数据信息到关联表
                                                        Drs.Claer();
                                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            Drs.IMEI2 = snstr;
                                                        }
                                                        else
                                                        {
                                                            Drs.IMEI2 = this.SN1_num.Text;
                                                        }
                                                        Drs.IMEI3 = simstr != "" ? simstr : "";
                                                        Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                        Drs.IMEI5 = "";
                                                        Drs.IMEI6 = macstr != "" ? macstr : "";
                                                        Drs.IMEI7 = equistr != "" ? equistr : "";
                                                        Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                        Drs.IMEI9 = batstr != "" ? batstr : "";
                                                        Drs.IMEI10 = "";
                                                        Drs.IMEI11 = "";
                                                        Drs.IMEI12 = "";
                                                        Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                        Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);
                                                    }

                                                    if (this.CheckIMEI2.Checked == false)
                                                    {
                                                        string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                        long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                        string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                        string sn2_suffix = SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                        MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                        this.SN1_num.Text = sn1;

                                                    }

                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                            else
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    btFormat.SubStrings["SN"].Value = "";

                                                }
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    PList.SN = snstr;

                                                }
                                                else
                                                {
                                                    PList.SN = "";

                                                }
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
                                                PList.JSUserDes = this.UserDesShow.Text;
                                                PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                                PList.IMEI2Start = this.IMEI2_num1.Text;
                                                PList.IMEI2End = this.IMEI2_num2.Text;
                                                //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                    {
                                                        DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                    }
                                                    else
                                                    {
                                                        //记录关联数据信息到关联表
                                                        Drs.Claer();
                                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            Drs.IMEI2 = snstr;
                                                        }
                                                        else
                                                        {
                                                            Drs.IMEI2 = "";
                                                        }
                                                        Drs.IMEI3 = simstr != "" ? simstr : "";
                                                        Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                        Drs.IMEI5 = "";
                                                        Drs.IMEI6 = macstr != "" ? macstr : "";
                                                        Drs.IMEI7 = equistr != "" ? equistr : "";
                                                        Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                        Drs.IMEI9 = batstr != "" ? batstr : "";
                                                        Drs.IMEI10 = "";
                                                        Drs.IMEI11 = "";
                                                        Drs.IMEI12 = "";
                                                        Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                        Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);
                                                    }

                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else if (PMB.CheckCHOrJSIMEI2BLL(this.IMEI_Start.Text, this.IMEI2_Start.Text, 1))
                                    {

                                        if (!PMB.CheckJSIMEI2BLL(this.IMEI2_Start.Text))
                                        {
                                            if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))//查重号
                                            {
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                    foreach (PrintMessage a in list)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = a.SN;
                                                    }
                                                }

                                                if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                }

                                                if (MOPB.UpdateJSmesIMEI2BLL(this.IMEI_Start.Text, this.IMEI2_Start.Text, ProductTime, this.UserShow.Text, this.UserDesShow.Text, this.IMEI2_num1.Text, this.IMEI2_num2.Text, lj))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("更新打印失败\r\n");
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                                this.IMEI_Start.Clear();
                                                this.IMEI2_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }


                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI_Start.Text + "已绑定\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI2_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //客供，不打印校验码
                            case 3:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        if (!CheckFieldsChoice(this.IMEI_Start.Text, 3, 2,0, btFormat))
                                        {
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    btFormat.SubStrings["IMEI2"].Value = this.IMEI2_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))
                                        {
                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            if (this.SN1_num.Text != "")
                                            {
                                                if (!PMB.CheckSNBLL(this.SN1_num.Text))
                                                {
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = snstr;

                                                    }
                                                    else
                                                    {
                                                        btFormat.SubStrings["SN"].Value = this.SN1_num.Text;

                                                    }
                                                    PList.Claer();
                                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                    PList.IMEI = this.IMEI_Start.Text.Trim();
                                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        PList.SN = snstr;

                                                    }
                                                    else
                                                    {
                                                        PList.SN = this.SN1_num.Text;

                                                    }
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
                                                    PList.JSUserDes = this.UserDesShow.Text;
                                                    PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                                    PList.IMEI2Start = this.IMEI2_num1.Text;
                                                    PList.IMEI2End = this.IMEI2_num2.Text;
                                                    //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                    if (PMB.InsertPrintMessageBLL(PList))
                                                    {
                                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                        {
                                                            DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                        }
                                                        else
                                                        {
                                                            //记录关联数据信息到关联表
                                                            Drs.Claer();
                                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                                            if (this.CheckIMEI2.Checked == true)
                                                            {
                                                                Drs.IMEI2 = snstr;
                                                            }
                                                            else
                                                            {
                                                                Drs.IMEI2 = this.SN1_num.Text;
                                                            }
                                                            Drs.IMEI3 = simstr != "" ? simstr : "";
                                                            Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                            Drs.IMEI5 = "";
                                                            Drs.IMEI6 = macstr != "" ? macstr : "";
                                                            Drs.IMEI7 = equistr != "" ? equistr : "";
                                                            Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                            Drs.IMEI9 = batstr != "" ? batstr : "";
                                                            Drs.IMEI10 = "";
                                                            Drs.IMEI11 = "";
                                                            Drs.IMEI12 = "";
                                                            Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                            Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                            DRSB.InsertRelativeSheetBLL(Drs);
                                                        }

                                                        if (this.CheckIMEI2.Checked == false)
                                                        {
                                                            string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                            long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                            string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                            MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                            this.SN1_num.Text = sn1;
                                                        }

                                                        Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                        Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                        this.IMEI_Start.Clear();
                                                        this.IMEI2_Start.Clear();
                                                        this.IMEI_Start.Focus();


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
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                            else
                                            {
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    btFormat.SubStrings["SN"].Value = "";

                                                }
                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    PList.SN = snstr;

                                                }
                                                else
                                                {
                                                    PList.SN = "";

                                                }
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
                                                PList.JSUserDes = this.UserDesShow.Text;
                                                PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                                PList.IMEI2Start = this.IMEI2_num1.Text;
                                                PList.IMEI2End = this.IMEI2_num2.Text;
                                                //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                    {
                                                        DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                    }
                                                    else
                                                    {
                                                        //记录关联数据信息到关联表
                                                        Drs.Claer();
                                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                                        if (this.CheckIMEI2.Checked == true)
                                                        {
                                                            Drs.IMEI2 = snstr;
                                                        }
                                                        else
                                                        {
                                                            Drs.IMEI2 = "";
                                                        }
                                                        Drs.IMEI3 = simstr != "" ? simstr : "";
                                                        Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                        Drs.IMEI5 = "";
                                                        Drs.IMEI6 = macstr != "" ? macstr : "";
                                                        Drs.IMEI7 = equistr != "" ? equistr : "";
                                                        Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                        Drs.IMEI9 = batstr != "" ? batstr : "";
                                                        Drs.IMEI10 = "";
                                                        Drs.IMEI11 = "";
                                                        Drs.IMEI12 = "";
                                                        Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                        Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);
                                                    }


                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else if (PMB.CheckCHOrJSIMEI2BLL(this.IMEI_Start.Text, this.IMEI2_Start.Text, 1))
                                    {
                                        if (!PMB.CheckJSIMEI2BLL(this.IMEI2_Start.Text))
                                        {

                                            if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))//查重号
                                            {
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                if (this.CheckIMEI2.Checked == true)
                                                {
                                                    btFormat.SubStrings["SN"].Value = snstr;

                                                }
                                                else
                                                {
                                                    list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                                    foreach (PrintMessage a in list)
                                                    {
                                                        btFormat.SubStrings["SN"].Value = a.SN;
                                                    }
                                                }

                                                if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                }

                                                if (MOPB.UpdateJSmesIMEI2BLL(this.IMEI_Start.Text, this.IMEI2_Start.Text, ProductTime, this.UserShow.Text, this.UserDesShow.Text, this.IMEI2_num1.Text, this.IMEI2_num2.Text, lj))
                                                {
                                                    Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                                else
                                                {
                                                    player.Play();
                                                    this.reminder.AppendText("更新打印失败\r\n");
                                                    this.IMEI_Start.Clear();
                                                    this.IMEI2_Start.Clear();
                                                    this.IMEI_Start.Focus();
                                                }
                                            }
                                            else
                                            {
                                                player.Play();
                                                this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                                this.IMEI_Start.Clear();
                                                this.IMEI2_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }


                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI_Start.Text + "已绑定\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }

                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI2_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                break;

                            //不打印校验码，不打印SN号
                            case 6:
                                {
                                    if (CheckFields.Count != 0)
                                    {
                                        if (!CheckFieldsChoice(this.IMEI_Start.Text, 6, 2, 0,btFormat))
                                        {
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    
                                    }
                                    //对模板相应字段进行赋值
                                    ValueToTemplate(btFormat);
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    btFormat.SubStrings["IMEI2"].Value = this.IMEI2_Start.Text;
                                    if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        if (!PMB.CheckIMEI2BLL(this.IMEI2_Start.Text))
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
                                            PList.JSUserName = this.UserShow.Text;
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            PList.IMEI2 = this.IMEI2_Start.Text.Trim();
                                            PList.IMEI2Start = this.IMEI2_num1.Text;
                                            PList.IMEI2End = this.IMEI2_num2.Text;
                                            //PList.IMEI2Rel = this.IMEI2Rel.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                                {
                                                    DRSB.UpdateIMEI14DAL(this.IMEI_Start.Text, this.IMEI2_Start.Text);
                                                }
                                                else
                                                {
                                                    //记录关联数据信息到关联表
                                                    Drs.Claer();
                                                    Drs.IMEI1 = this.IMEI_Start.Text;
                                                    if (this.CheckIMEI2.Checked == true)
                                                    {
                                                        Drs.IMEI2 = snstr;
                                                    }
                                                    else
                                                    {
                                                        Drs.IMEI2 = "";
                                                    }
                                                    Drs.IMEI3 = simstr != "" ? simstr : "";
                                                    Drs.IMEI4 = iccidstr != "" ? iccidstr : "";
                                                    Drs.IMEI5 = "";
                                                    Drs.IMEI6 = macstr != "" ? macstr : "";
                                                    Drs.IMEI7 = equistr != "" ? equistr : "";
                                                    Drs.IMEI8 = vipstr != "" ? vipstr : "";
                                                    Drs.IMEI9 = batstr != "" ? batstr : "";
                                                    Drs.IMEI10 = "";
                                                    Drs.IMEI11 = "";
                                                    Drs.IMEI12 = "";
                                                    Drs.RFID = rfidstr != "" ? rfidstr : "";
                                                    Drs.IMEI14 = this.IMEI2_Start.Text.Trim();
                                                    Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                    Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    DRSB.InsertRelativeSheetBLL(Drs);
                                                }


                                                Result nResult1 = btFormat.Print("标签打印软件", waitout, out messages);
                                                Form1.Log("打印了IMEI号为" + this.IMEI_Start.Text + ", IMEI2号为" + this.IMEI2_Start.Text + "的制单", null);
                                                this.IMEI_Start.Clear();
                                                this.IMEI2_Start.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            player.Play();
                                            this.reminder.AppendText(this.IMEI2_Start.Text + "重号\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI2_Start.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI2_Start.Clear();
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
                        this.IMEI2_Start.Clear();
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
                //检查操作设置
                if (checkInformation())
                {
                    this.Re_IMEINum.Clear();
                    this.Re_IMEINum.Focus();
                    return;
                }

                //分割字符串
                this.Re_IMEINum.Text = SustringPos(this.Re_IMEINum.Text);

                string strField = DRSB.SelectIMEIFieldBLL(SplicingCheckSQLStr(this.Re_IMEINum.Text));

                if(strField != "")
                {
                    this.Re_IMEINum.Text = strField;
                }

                if (this.ModeFalge == 0)
                {
                    try
                    {
                        if (this.Re_IMEINum.Text != "")
                        {
                            if (this.Re_Nocheckcode.Checked == false)
                            {
                                if (IsNumeric(this.Re_IMEINum.Text))
                                {
                                    if (long.Parse(this.Re_IMEINum.Text.Substring(0, 14)) < long.Parse(SlipIMEIStart) || long.Parse(this.Re_IMEINum.Text.Substring(0, 14)) > long.Parse(SlipIMEIEnd))
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
                                if (this.Re_IMEINum.Text.Length !=SlipIMEIStart.Length)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.Re_IMEINum.Clear();
                                    this.Re_IMEINum.Focus();
                                    return;
                                   
                                }
                                if (this.Re_IMEINum.Text.CompareTo(SlipIMEIStart) == -1 || this.Re_IMEINum.Text.CompareTo(SlipIMEIEnd) == 1)
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
                        if (this.Select_Template1.Text != "")
                        {
                            LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                            ClearTemplate1ToVlue(btFormat);
                            //指定打印机名称
                            btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                            //对模板相应字段进行赋值
                            GetValue("Information", "生产日期", out outString);
                            btFormat.SubStrings[outString].Value = this.ProductData.Text;
                            //打印份数,同序列打印的份数
                            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;

                            if (CheckFields.Count != 0)
                            {
                                if (!CheckFieldsChoice(this.Re_IMEINum.Text, 0, 1, 0,btFormat))
                                {
                                    this.Re_IMEINum.Clear();
                                    this.Re_IMEINum.Focus();
                                    return;
                                }
                            }
                            
                            btFormat.SubStrings["IMEI"].Value = this.Re_IMEINum.Text;
                            if (PMB.CheckReCHOrJSIMEIBLL(this.Re_IMEINum.Text, 1))
                            {
                                if (snstr == "")
                                    btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(this.Re_IMEINum.Text);

                                //更新打印信息到数据表
                                string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                if (PMB.UpdateRePrintBLL(this.Re_IMEINum.Text, RE_PrintTime, 1, lj, lj))
                                {
                                    btFormat.Print();
                                    Form1.Log("重打了IMEI号为" + this.Re_IMEINum.Text + "的制单", null);
                                    this.Re_IMEINum.Clear();
                                    this.Re_IMEINum.Focus();
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText("更新打印失败\r\n");
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

                //打印模式
                if (this.ModeFalge == 1 || this.ModeFalge == 2)
                {
                    try
                    {
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
                        if (this.Select_Template1.Text != "")
                        {
                            LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                            ClearTemplate1ToVlue(btFormat);
                            //指定打印机名称
                            btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                            //对模板相应字段进行赋值
                            GetValue("Information", "生产日期", out outString);
                            btFormat.SubStrings[outString].Value = this.ProductData.Text;
                            //打印份数,同序列打印的份数
                            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;

                            if (CheckFields.Count != 0)
                            {
                                if (!CheckFieldsChoice(this.Re_IMEINum.Text, 0, 1, 1,btFormat))
                                {
                                    this.Re_IMEINum.Clear();
                                    this.Re_IMEINum.Focus();
                                    return;
                                }
                            }

                            btFormat.SubStrings["IMEI"].Value = this.Re_IMEINum.Text;
                            if (PMB.CheckReCHOrJSIMEI2BLL(this.Re_IMEINum.Text, 1))
                            {
                                btFormat.SubStrings["IMEI2"].Value = PMB.SelectIMEI2ByIMEIBLL(this.Re_IMEINum.Text);

                                if (snstr == "")
                                    btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(this.Re_IMEINum.Text);
                                //btFormat.SubStrings["SN"].Value = snstr;

                                //更新打印信息到数据表
                                string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                if (PMB.UpdateRePrintBLL(this.Re_IMEINum.Text, RE_PrintTime, 1, lj, lj))
                                {
                                    btFormat.Print();
                                    Form1.Log("重打了IMEI号为" + this.Re_IMEINum.Text + "的制单", null);
                                    this.Re_IMEINum.Clear();
                                    this.Re_IMEINum.Focus();
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText("更新打印失败\r\n");
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

                if(this.Hexadecimal.Checked == true)
                {
                    this.Hexadecimal.Checked = false;
                    //this.HexPrintNum.Visible = false;
                    this.HexPrintNum.ReadOnly = true;
                }

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

                if(this.SNHex.Checked == true)
                {
                    this.SNHex.Checked = false;
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
                this.CheckIMEI2.Checked = false;
                this.CheckSIM.Checked = false;
                this.CheckBAT.Checked = false;
                this.CheckICCID.Checked = false;
                this.CheckMAC.Checked = false;
                this.CheckEquipment.Checked = false;
                this.CheckVIP.Checked = false;
                this.CheckRFID.Checked = false;
                
                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;

                //rfid
                this.RFID_Start.ReadOnly = true;
                this.RFID_Check.Checked = false;

                //逐个打印
                if (this.PrintOne.Checked == true)
                {
                    this.InseIMEI2.Enabled = false;
                    this.InseIMEI2.Checked = false;
                    this.PrintOne.Checked = false;
                    this.IMEI_Start.ReadOnly = true;
                    this.IMEI2_Start.ReadOnly = true;
                    this.IMEI_Start.Clear();
                }   
                //批量打印
                if(this.PrintMore.Checked == true)
                {
                    this.PrintMore.Checked = false;
                    this.PrintNum.Clear();
                    this.PrintNum.ReadOnly = true;
                }
                
                //逐个重打印
                if(this.RePrintOne.Checked == true)
                {
                    this.RePrintOne.Checked = false;
                    this.Re_IMEINum.ReadOnly = true;
                    this.Re_IMEINum.Clear();
                }               
                //批量重打印
                if(this.RePrintMore.Checked == true)
                {
                    if (this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
                    {
                        this.ReImei2Num1.ReadOnly = true;
                        this.ReImei2Num2.ReadOnly = true;
                        this.ReImei2Num1.Clear();
                        this.ReImei2Num2.Clear();
                    }
                    this.RePrintMore.Checked = false;
                    this.Re_IMEINum.ReadOnly = true;
                    this.ReImeiNum1.ReadOnly = true;
                    this.ReImeiNum2.ReadOnly = true;
                    this.ReImeiNum1.Clear();
                    this.ReImeiNum2.Clear();
                    this.Re_IMEINum.Focus();
                }



                //this.HexPrintNum.Visible = true;
                this.HexPrintNum.ReadOnly = false;
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
                //this.HexPrintNum.Visible = false;
                this.HexPrintNum.ReadOnly = true;
            }
        }

        //批量重打IMEI起始位
        private void ReImeiNum1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //检查操作设置
                if (checkInformation())
                {
                    this.ReImeiNum1.Clear();
                    this.ReImeiNum1.Focus();
                    return;
                }
                if (this.ReImeiNum1.Text != "")
                {
                    //分割字符串
                    this.ReImeiNum1.Text = SustringPos(this.ReImeiNum1.Text);

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
                        if (long.Parse(this.ReImeiNum1.Text.Substring(0, 14)) < long.Parse(SlipIMEIStart) || long.Parse(this.ReImeiNum1.Text.Substring(0, 14)) > long.Parse(SlipIMEIEnd))
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
                        if (this.ReImeiNum1.Text.Length != SlipIMEIStart.Length)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI不在范围内\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }
                        if (this.ReImeiNum1.Text.CompareTo(SlipIMEIStart) == -1 || this.ReImeiNum1.Text.CompareTo(SlipIMEIEnd) == 1)
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
               if(this.ModeFalge == 0)
                {
                    try
                    {
                        if(this.ReImeiNum1.Text == "")
                        {
                            player.Play();
                            this.reminder.AppendText("请输入重打起始位\r\n");
                            this.ReImeiNum1.Focus();
                            return;
                        }

                        if (this.ReImeiNum2.Text != "")
                        {
                            //分割字符串
                            this.ReImeiNum2.Text = SustringPos(this.ReImeiNum2.Text);

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
                                if (long.Parse(this.ReImeiNum2.Text.Substring(0, 14)) < long.Parse(SlipIMEIStart) || long.Parse(this.ReImeiNum2.Text.Substring(0, 14)) > long.Parse(SlipIMEIEnd))
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
                                if (this.ReImeiNum2.Text.Length != SlipIMEIStart.Length)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }
                                if (this.ReImeiNum2.Text.CompareTo(SlipIMEIStart) == -1 || this.ReImeiNum2.Text.CompareTo(SlipIMEIEnd) == 1)
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

                        //制定模板
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                            string begin0 = "";
                            for (long Num1Imei14 = long.Parse(this.ReImeiNum1.Text.Substring(0, 14)); Num1Imei14 <= Num2Imei14; Num1Imei14++)
                            {
                                begin0 = GetLength0(Num1Imei14, SlipIMEIStart);
                                string Num1Imei15 = getimei15(begin0 + Num1Imei14.ToString());
                                btFormat.SubStrings["IMEI"].Value = begin0 + Num1Imei14.ToString() + Num1Imei15.ToString();
                                btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(begin0 + Num1Imei14.ToString() + Num1Imei15.ToString());
                                //更新打印信息到数据表
                                string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                if (PMB.UpdateRePrintBLL(begin0 + Num1Imei14.ToString() + Num1Imei15.ToString(), RE_PrintTime, 1, lj, lj))
                                {
                                    btFormat.Print();
                                    Form1.Log("批量重打了IMEI号为" + begin0 + Num1Imei14.ToString() + Num1Imei15.ToString() + "的制单", null);
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText("更新打印失败\r\n");
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
                                int ReDig = SlipIMEIEnd.Length;
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
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新打印失败\r\n");
                                    }
                                }
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImeiNum1.Focus();
                            }
                            else
                            {
                                int JSCount2 = PMB.CheckReJSRangeIMEIBLL(this.ReImeiNum1.Text, this.ReImeiNum2.Text);
                                long InputCount2 = Convert.ToInt64(this.ReImeiNum2.Text, 16) - Convert.ToInt64(this.ReImeiNum1.Text, 16) + Convert.ToInt64("1", 16);
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
                                        Num1Imei14 = Convert.ToInt64(Num1Imei14.ToString("X"), 16) + Convert.ToInt64("1", 16);
                                        btFormat.Print();
                                        Form1.Log("批量重打了IMEI号为" + Num1Imei14 + "的制单", null);
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新打印失败\r\n");
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

               if(this.ModeFalge == 1 || this.ModeFalge == 2)
                {
                    try
                    {
                        if (this.ReImeiNum2.Text != "")
                        {
                            //分割字符串
                            this.ReImeiNum2.Text = SustringPos(this.ReImeiNum2.Text);

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

                        this.ReImei2Num1.Focus();
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception:" + ex.Message);
                    }
                }
                
            }
        }

        //批量重打IMEI2起始位
        private void ReImei2Num1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {

                if (this.ModeFalge == 1)
                {
                    if (this.ReImei2Num1.Text != "")
                    {
                        //分割字符串
                        this.ReImei2Num1.Text = SustringPos(this.ReImei2Num1.Text);

                        if (this.IMEI_num1.Text == this.IMEI2_num1.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("起始位相等\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImei2Num1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }


                        if (this.IMEI_num2.Text == this.IMEI2_num2.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("终止位相等\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImei2Num1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }

                        if (this.Re_Nocheckcode.Checked == false)
                        {
                            if (IsNumeric(this.ReImei2Num1.Text))
                            {
                                if (this.ReImei2Num1.Text.Length != 15)
                                {
                                    player.Play();
                                    this.reminder.AppendText("请输入15位IMEI2\r\n");
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num1.Focus();
                                    return;
                                }
                                else
                                {
                                    string imeiRes;
                                    string imei14 = this.ReImei2Num1.Text.Substring(0, 14);
                                    string imei15 = getimei15(imei14);
                                    imeiRes = imei14 + imei15;
                                    if (imeiRes != this.ReImei2Num1.Text)
                                    {
                                        player.Play();
                                        this.reminder.AppendText("IMEI2校验错误\r\n");
                                        this.ReImei2Num1.Clear();
                                        this.ReImei2Num1.Focus();
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2格式错误\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                            if (long.Parse(this.ReImei2Num1.Text.Substring(0, 14)) < long.Parse(this.IMEI2_num1.Text) || long.Parse(this.ReImei2Num1.Text.Substring(0, 14)) > long.Parse(this.IMEI2_num2.Text))
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2不在范围内\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                        }
                        else
                        {
                            if (this.ReImei2Num1.Text.Length != this.IMEI2_num1.Text.Length)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2不在范围内\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                            if (this.ReImei2Num1.Text.CompareTo(this.IMEI2_num1.Text) == -1 || this.ReImei2Num1.Text.CompareTo(this.IMEI2_num2.Text) == 1)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2不在范围内\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                        }


                        this.ReImei2Num2.Focus();
                    }
                }

                if (this.ModeFalge == 2)
                {
                    if (this.ReImei2Num1.Text != "")
                    {
                        //分割字符串
                        this.ReImei2Num1.Text = SustringPos(this.ReImei2Num1.Text);

                        if (this.IMEI_num1.Text != this.IMEI2_num1.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("起始位不相等\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImei2Num1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }

                        if (this.IMEI_num2.Text != this.IMEI2_num2.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("终止位不相等\r\n");
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImei2Num1.Clear();
                            this.ReImeiNum1.Focus();
                            return;
                        }

                        if (this.Re_Nocheckcode.Checked == false)
                        {
                            if (IsNumeric(this.ReImei2Num1.Text))
                            {
                                if (this.ReImei2Num1.Text.Length != 15)
                                {
                                    player.Play();
                                    this.reminder.AppendText("请输入15位IMEI2\r\n");
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num1.Focus();
                                    return;
                                }
                                else
                                {
                                    string imeiRes;
                                    string imei14 = this.ReImei2Num1.Text.Substring(0, 14);
                                    string imei15 = getimei15(imei14);
                                    imeiRes = imei14 + imei15;
                                    if (imeiRes != this.ReImei2Num1.Text)
                                    {
                                        player.Play();
                                        this.reminder.AppendText("IMEI2校验错误\r\n");
                                        this.ReImei2Num1.Clear();
                                        this.ReImei2Num1.Focus();
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2格式错误\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                            if (long.Parse(this.ReImei2Num1.Text.Substring(0, 14)) < long.Parse(this.IMEI2_num1.Text) || long.Parse(this.ReImei2Num1.Text.Substring(0, 14)) > long.Parse(this.IMEI2_num2.Text))
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2不在范围内\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                        }
                        else
                        {
                            if (this.ReImei2Num1.Text.Length != this.IMEI2_num1.Text.Length)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2不在范围内\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                            if (this.ReImei2Num1.Text.CompareTo(this.IMEI2_num1.Text) == -1 || this.ReImei2Num1.Text.CompareTo(this.IMEI2_num2.Text) == 1)
                            {
                                player.Play();
                                this.reminder.AppendText("IMEI2不在范围内\r\n");
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num1.Focus();
                                return;
                            }
                        }


                        this.ReImei2Num2.Focus();
                    }
                }
            }

        }

        //批量重打IMEI2终止位
        private void ReImei2Num2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.ModeFalge == 1)
                {
                    try
                    {
                        if (this.ReImei2Num2.Text != "")
                        {
                            //分割字符串
                            this.ReImei2Num2.Text = SustringPos(this.ReImei2Num2.Text);

                            if (this.IMEI_num1.Text == this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("起始位相等\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }


                            if (this.IMEI_num2.Text == this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("终止位相等\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }

                            if (this.Re_Nocheckcode.Checked == false)
                            {
                                if (IsNumeric(this.ReImei2Num2.Text))
                                {
                                    if (this.ReImei2Num2.Text.Length != 15)
                                    {
                                        player.Play();
                                        this.reminder.AppendText("请输入15位IMEI\r\n");
                                        this.ReImei2Num2.Clear();
                                        this.ReImei2Num2.Focus();
                                        return;
                                    }
                                    else
                                    {
                                        string imeiRes;
                                        string imei14 = this.ReImei2Num2.Text.Substring(0, 14);
                                        string imei15 = getimei15(imei14);
                                        imeiRes = imei14 + imei15;
                                        if (imeiRes != this.ReImei2Num2.Text)
                                        {
                                            player3.Play();
                                            this.reminder.AppendText("IMEI校验错误\r\n");
                                            this.ReImei2Num2.Clear();
                                            this.ReImei2Num2.Focus();
                                            return;
                                        }
                                        else if (long.Parse(imei14) < long.Parse(this.ReImei2Num1.Text.Substring(0, 14)))
                                        {
                                            player.Play();
                                            this.reminder.AppendText("IMEI小于重打起始位\r\n");
                                            this.ReImei2Num2.Clear();
                                            this.ReImei2Num2.Focus();
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI格式错误\r\n");
                                    this.ReImei2Num2.Clear();
                                    this.ReImei2Num2.Focus();
                                    return;
                                }
                                if (long.Parse(this.ReImei2Num2.Text.Substring(0, 14)) < long.Parse(this.IMEI2_num1.Text) || long.Parse(this.ReImei2Num2.Text.Substring(0, 14)) > long.Parse(this.IMEI2_num2.Text))
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.ReImei2Num2.Clear();
                                    this.ReImei2Num2.Focus();
                                    return;
                                }
                            }
                            else
                            {
                                if (this.ReImei2Num2.Text.Length != this.IMEI2_num1.Text.Length)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num1.Focus();
                                    return;
                                }
                                if (this.ReImei2Num2.Text.CompareTo(this.IMEI2_num1.Text) == -1 || this.ReImei2Num2.Text.CompareTo(this.IMEI2_num2.Text) == 1)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num1.Focus();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText("请输入重打终止位\r\n");
                            this.ReImei2Num2.Focus();
                            return;
                        }

                        //制定模板
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                                this.reminder.AppendText("IMEI1 部分无记录，无法全部重打\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }

                            long Num2Imei214 = long.Parse(this.ReImei2Num2.Text.Substring(0, 14));
                            int JSCount2 = PMB.CheckReJSRangeIMEI2BLL(this.ReImei2Num1.Text, this.ReImei2Num2.Text);
                            int InputCount2 = int.Parse((Num2Imei214 - long.Parse(this.ReImei2Num1.Text.Substring(0, 14)) + 1).ToString());
                            if (JSCount2 != InputCount2)
                            {
                                this.reminder.AppendText("IMEI2 部分无记录，无法全部重打\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }

                            string begin0 = "";
                            string begin1 = "";
                            long Num1Imei214 = long.Parse(this.ReImei2Num1.Text.Substring(0, 14));
                            for (long Num1Imei14 = long.Parse(this.ReImeiNum1.Text.Substring(0, 14)); Num1Imei14 <= Num2Imei14; Num1Imei14++)
                            {
                                begin0 = GetLength0(Num1Imei14, this.IMEI_num1.Text);
                                begin1 = GetLength0(Num1Imei214, this.IMEI_num1.Text);
                                string Num1Imei15 = getimei15(begin0 + Num1Imei14.ToString());
                                string Num1Imei215 = getimei15(begin1 + Num1Imei214.ToString());
                                btFormat.SubStrings["IMEI"].Value = begin0 + Num1Imei14.ToString() + Num1Imei15.ToString();
                                btFormat.SubStrings["IMEI2"].Value = begin1 + Num1Imei214.ToString() + Num1Imei215.ToString();
                                btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(begin0 + Num1Imei14.ToString() + Num1Imei15.ToString());
                                //更新打印信息到数据表
                                string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                if (PMB.UpdateRePrintBLL(begin0 + Num1Imei14.ToString() + Num1Imei15.ToString(), RE_PrintTime, 1, lj, lj))
                                {
                                    Num1Imei214++;

                                    btFormat.Print();
                                    Form1.Log("批量重打了IMEI号为" + begin0 + Num1Imei14.ToString() + Num1Imei15.ToString() + "的制单", null);
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText("更新打印失败\r\n");
                                }
                            }
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImei2Num1.Clear();
                            this.ReImei2Num2.Clear();
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
                                    this.reminder.AppendText("IMEI1部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                int ReDig2 = this.IMEI2_num2.Text.Length;
                                string RePre2 = this.ReImei2Num1.Text.Substring(0, ReDig2 - 5);
                                long Num2Imei214 = long.Parse(this.ReImei2Num2.Text.Remove(0, ReDig2 - 5));
                                int JSCount2_2 = PMB.CheckReJSRangeIMEI2BLL(this.ReImei2Num1.Text, this.ReImei2Num2.Text);
                                int InputCount2_2 = int.Parse((Num2Imei214 - long.Parse(this.ReImei2Num1.Text.Remove(0, ReDig2 - 5)) + 1).ToString());
                                if (JSCount2_2 != InputCount2_2)
                                {
                                    this.reminder.AppendText("IMEI2部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                long Num1Imei214 = long.Parse(this.ReImei2Num1.Text.Remove(0, ReDig2 - 5));
                                for (long Num1Imei14 = long.Parse(this.ReImeiNum1.Text.Remove(0, ReDig - 5)); Num1Imei14 <= Num2Imei14; Num1Imei14++)
                                {
                                    btFormat.SubStrings["IMEI"].Value = RePre + Num1Imei14.ToString();
                                    btFormat.SubStrings["IMEI2"].Value = RePre2 + Num1Imei214.ToString();
                                    btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(RePre + Num1Imei14.ToString());
                                    //更新打印信息到数据表
                                    string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                    if (PMB.UpdateRePrintBLL(RePre + Num1Imei14.ToString(), RE_PrintTime, 1, lj, lj))
                                    {
                                        Num1Imei214++;

                                        btFormat.Print();
                                        Form1.Log("批量重打了IMEI号为" + RePre + Num1Imei14 + "的制单", null);
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新打印失败\r\n");
                                    }
                                }
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                            }
                            else
                            {
                                int JSCount2 = PMB.CheckReJSRangeIMEIBLL(this.ReImeiNum1.Text, this.ReImeiNum2.Text);
                                long InputCount2 = Convert.ToInt64(this.ReImeiNum2.Text, 16) - Convert.ToInt64(this.ReImeiNum1.Text, 16) + Convert.ToInt64("1", 16);
                                if (JSCount2 != InputCount2)
                                {
                                    this.reminder.AppendText("部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                int JSCount2_2 = PMB.CheckReJSRangeIMEI2BLL(this.ReImei2Num1.Text, this.ReImei2Num2.Text);
                                long InputCount2_2 = Convert.ToInt64(this.ReImeiNum2.Text, 16) - Convert.ToInt64(this.ReImeiNum1.Text, 16) + Convert.ToInt64("1", 16);
                                if (JSCount2_2 != InputCount2_2)
                                {
                                    this.reminder.AppendText("部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                long Num1Imei214 = Convert.ToInt64(this.ReImei2Num1.Text, 16);
                                for (long Num1Imei14 = Convert.ToInt64(this.ReImeiNum1.Text, 16); Num1Imei14 <= Convert.ToInt64(this.ReImeiNum2.Text, 16);)
                                {
                                    btFormat.SubStrings["IMEI"].Value = Num1Imei14.ToString("X");
                                    btFormat.SubStrings["IMEI2"].Value = Num1Imei214.ToString("X");
                                    btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(Num1Imei14.ToString("X"));
                                    //更新打印信息到数据表
                                    string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                    if (PMB.UpdateRePrintBLL(Num1Imei14.ToString("X"), RE_PrintTime, 1, lj, lj))
                                    {
                                        Num1Imei14 = Convert.ToInt64(Num1Imei14.ToString("X"), 16) + Convert.ToInt64("1", 16);
                                        Num1Imei214 = Convert.ToInt64(Num1Imei214.ToString("X"), 16) + Convert.ToInt64("1", 16);
                                        btFormat.Print();
                                        Form1.Log("批量重打了IMEI号为" + Num1Imei14 + "的制单", null);
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新打印失败\r\n");
                                    }
                                }
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception:" + ex.Message);
                    }
                }

                if (this.ModeFalge == 2)
                {
                    try
                    {
                        if (this.ReImei2Num2.Text != "")
                        {
                            //分割字符串
                            this.ReImei2Num2.Text = SustringPos(this.ReImei2Num2.Text);

                            if (this.IMEI_num1.Text != this.IMEI2_num1.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("起始位不相等\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }

                            if (this.IMEI_num2.Text != this.IMEI2_num2.Text)
                            {
                                player.Play();
                                this.reminder.AppendText("终止位不相等\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }

                            if (this.Re_Nocheckcode.Checked == false)
                            {
                                if (IsNumeric(this.ReImei2Num2.Text))
                                {
                                    if (this.ReImei2Num2.Text.Length != 15)
                                    {
                                        player.Play();
                                        this.reminder.AppendText("请输入15位IMEI\r\n");
                                        this.ReImei2Num2.Clear();
                                        this.ReImei2Num2.Focus();
                                        return;
                                    }
                                    else
                                    {
                                        string imeiRes;
                                        string imei14 = this.ReImei2Num2.Text.Substring(0, 14);
                                        string imei15 = getimei15(imei14);
                                        imeiRes = imei14 + imei15;
                                        if (imeiRes != this.ReImei2Num2.Text)
                                        {
                                            player3.Play();
                                            this.reminder.AppendText("IMEI校验错误\r\n");
                                            this.ReImei2Num2.Clear();
                                            this.ReImei2Num2.Focus();
                                            return;
                                        }
                                        else if (long.Parse(imei14) < long.Parse(this.ReImei2Num1.Text.Substring(0, 14)))
                                        {
                                            player.Play();
                                            this.reminder.AppendText("IMEI小于重打起始位\r\n");
                                            this.ReImei2Num2.Clear();
                                            this.ReImei2Num2.Focus();
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI格式错误\r\n");
                                    this.ReImei2Num2.Clear();
                                    this.ReImei2Num2.Focus();
                                    return;
                                }
                                if (long.Parse(this.ReImei2Num2.Text.Substring(0, 14)) < long.Parse(this.IMEI2_num1.Text) || long.Parse(this.ReImei2Num2.Text.Substring(0, 14)) > long.Parse(this.IMEI2_num2.Text))
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.ReImei2Num2.Clear();
                                    this.ReImei2Num2.Focus();
                                    return;
                                }
                            }
                            else
                            {
                                if (this.ReImei2Num2.Text.Length != this.IMEI2_num1.Text.Length)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num1.Focus();
                                    return;
                                }
                                if (this.ReImei2Num2.Text.CompareTo(this.IMEI2_num1.Text) == -1 || this.ReImei2Num2.Text.CompareTo(this.IMEI2_num2.Text) == 1)
                                {
                                    player.Play();
                                    this.reminder.AppendText("IMEI不在范围内\r\n");
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num1.Focus();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText("请输入重打终止位\r\n");
                            this.ReImei2Num2.Focus();
                            return;
                        }

                        //制定模板
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ClearTemplate1ToVlue(btFormat);
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
                            if ((JSCount*2) != InputCount)
                            {
                                this.reminder.AppendText("IMEI1 部分无记录，无法全部重打\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }

                            long Num2Imei214 = long.Parse(this.ReImei2Num2.Text.Substring(0, 14));
                            int JSCount2 = PMB.CheckReJSRangeIMEI2BLL(this.ReImei2Num1.Text, this.ReImei2Num2.Text);
                            int InputCount2 = int.Parse((Num2Imei214 - long.Parse(this.ReImei2Num1.Text.Substring(0, 14)) + 1).ToString());
                            if ((JSCount2 *2)!= InputCount2)
                            {
                                this.reminder.AppendText("IMEI2 部分无记录，无法全部重打\r\n");
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                                return;
                            }

                            long Num1Imei214 = 0;
                            string begin0 = "";
                            string begin1 = "";
                            for (long Num1Imei14 = long.Parse(this.ReImeiNum1.Text.Substring(0, 14)); Num1Imei14 <= Num2Imei14;)
                            {
                                begin0 = GetLength0(Num1Imei14, this.IMEI_num1.Text);
                                string Num1Imei15 = getimei15(begin0 + Num1Imei14.ToString());
                                Num1Imei214 = Num1Imei14 + 1;
                                begin1 = GetLength0(Num1Imei214, this.IMEI_num1.Text);
                                string Num1Imei215 = getimei15(begin1 + Num1Imei214.ToString());
                                btFormat.SubStrings["IMEI"].Value = begin0 + Num1Imei14.ToString() + Num1Imei15.ToString();
                                btFormat.SubStrings["IMEI2"].Value = begin1 + Num1Imei214.ToString() + Num1Imei215.ToString();
                                btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(begin0 + Num1Imei14.ToString() + Num1Imei15.ToString());
                                //更新打印信息到数据表
                                string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                if (PMB.UpdateRePrintBLL(begin0 + Num1Imei14.ToString() + Num1Imei15.ToString(), RE_PrintTime, 1, lj, lj))
                                {
                                    Num1Imei14 = Num1Imei14+2;

                                    btFormat.Print();
                                    Form1.Log("批量重打了IMEI号为" + begin0 + Num1Imei14.ToString() + Num1Imei15.ToString() + "的制单", null);
                                }
                                else
                                {
                                    player.Play();
                                    this.reminder.AppendText("更新打印失败\r\n");
                                }
                            }
                            this.ReImeiNum1.Clear();
                            this.ReImeiNum2.Clear();
                            this.ReImei2Num1.Clear();
                            this.ReImei2Num2.Clear();
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
                                    this.reminder.AppendText("IMEI1部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                int ReDig2 = this.IMEI2_num2.Text.Length;
                                string RePre2 = this.ReImei2Num1.Text.Substring(0, ReDig2 - 5);
                                long Num2Imei214 = long.Parse(this.ReImei2Num2.Text.Remove(0, ReDig2 - 5));
                                int JSCount2_2 = PMB.CheckReJSRangeIMEI2BLL(this.ReImei2Num1.Text, this.ReImei2Num2.Text);
                                int InputCount2_2 = int.Parse((Num2Imei214 - long.Parse(this.ReImei2Num1.Text.Remove(0, ReDig2 - 5)) + 1).ToString());
                                if (JSCount2_2 != InputCount2_2)
                                {
                                    this.reminder.AppendText("IMEI2部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                long Num1Imei214 = 0;
                                for (long Num1Imei14 = long.Parse(this.ReImeiNum1.Text.Remove(0, ReDig - 5)); Num1Imei14 <= Num2Imei14; )
                                {
                                    Num1Imei214 = Num1Imei14 + 1;
                                    btFormat.SubStrings["IMEI"].Value = RePre + Num1Imei14.ToString();
                                    btFormat.SubStrings["IMEI2"].Value = RePre2 + Num1Imei214.ToString();
                                    btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(RePre + Num1Imei14.ToString());
                                    //更新打印信息到数据表
                                    string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                    if (PMB.UpdateRePrintBLL(RePre + Num1Imei14.ToString(), RE_PrintTime, 1, lj, lj))
                                    {
                                        Num1Imei14 = Num1Imei14 + 2;

                                        btFormat.Print();
                                        Form1.Log("批量重打了IMEI号为" + RePre + Num1Imei14 + "的制单", null);
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新打印失败\r\n");
                                    }
                                }
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
                                this.ReImeiNum1.Focus();
                            }
                            else
                            {
                                int JSCount2 = PMB.CheckReJSRangeIMEIBLL(this.ReImeiNum1.Text, this.ReImeiNum2.Text);
                                long InputCount2 = Convert.ToInt64(this.ReImeiNum2.Text, 16) - Convert.ToInt64(this.ReImeiNum1.Text, 16) + Convert.ToInt64("1", 16);
                                if (JSCount2 != InputCount2)
                                {
                                    this.reminder.AppendText("部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                int JSCount2_2 = PMB.CheckReJSRangeIMEI2BLL(this.ReImei2Num1.Text, this.ReImei2Num2.Text);
                                long InputCount2_2 = Convert.ToInt64(this.ReImeiNum2.Text, 16) - Convert.ToInt64(this.ReImeiNum1.Text, 16) + Convert.ToInt64("1", 16);
                                if (JSCount2_2 != InputCount2_2)
                                {
                                    this.reminder.AppendText("部分无记录，无法全部重打\r\n");
                                    this.ReImeiNum1.Clear();
                                    this.ReImeiNum2.Clear();
                                    this.ReImei2Num1.Clear();
                                    this.ReImei2Num2.Clear();
                                    this.ReImeiNum1.Focus();
                                    return;
                                }

                                long Num1Imei214 = 0;
                                for (long Num1Imei14 = Convert.ToInt64(this.ReImeiNum1.Text, 16); Num1Imei14 <= Convert.ToInt64(this.ReImeiNum2.Text, 16);)
                                {
                                    Num1Imei214 = Convert.ToInt64(Num1Imei14.ToString("X"), 16) + Convert.ToInt64("1", 16);
                                    btFormat.SubStrings["IMEI"].Value = Num1Imei14.ToString("X");
                                    btFormat.SubStrings["IMEI2"].Value = Num1Imei214.ToString("X");
                                    btFormat.SubStrings["SN"].Value = PMB.SelectOnlySnByIMEIBLL(Num1Imei14.ToString("X"));
                                    //更新打印信息到数据表
                                    string RE_PrintTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                                    if (PMB.UpdateRePrintBLL(Num1Imei14.ToString("X"), RE_PrintTime, 1, lj, lj))
                                    {
                                        Num1Imei14 = Convert.ToInt64(Num1Imei14.ToString("X"), 16) + Convert.ToInt64("2", 16);
                                        btFormat.Print();
                                        Form1.Log("批量重打了IMEI号为" + Num1Imei14 + "的制单", null);
                                    }
                                    else
                                    {
                                        player.Play();
                                        this.reminder.AppendText("更新打印失败\r\n");
                                    }
                                }
                                this.ReImeiNum1.Clear();
                                this.ReImeiNum2.Clear();
                                this.ReImei2Num1.Clear();
                                this.ReImei2Num2.Clear();
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
        }


        //刷新制单
        private void Refresh_zhidan_Click(object sender, EventArgs e)
        {
            this.IMEI_Start.Clear();
            this.IMEI2_Start.Clear();
            this.PrintNum.Clear();
            this.Re_IMEINum.Clear();
            this.ReImeiNum1.Clear();
            this.ReImeiNum2.Clear();
            this.ReImei2Num1.Clear();
            this.ReImei2Num2.Clear();
            this.HexPrintNum.Clear();

            this.IMEI_Start.ReadOnly = true;
            this.IMEI2_Start.ReadOnly = true;
            this.PrintNum.ReadOnly = true;
            this.Re_IMEINum.ReadOnly = true;
            this.ReImeiNum1.ReadOnly = true;
            this.ReImeiNum2.ReadOnly = true;
            this.ReImei2Num1.ReadOnly = true;
            this.ReImei2Num2.ReadOnly = true;
            this.HexPrintNum.ReadOnly = true;

            this.CB_ZhiDan.Items.Clear();
            G_MOP.Clear();
            G_MOP = MOPB.SelectZhidanNumBLL();
            foreach (Gps_ManuOrderParam a in G_MOP)
            {
                this.CB_ZhiDan.Items.Add(a.ZhiDan);
            }
            if (MOPB.CheckZhiDanBLL(this.CB_ZhiDan.Text))
            {
                if(this.StartZhiDan == 1)
                {
                    if (this.CB_ZhiDan.Text == "")
                    {
                        this.reminder.AppendText("请选择制单号\r\n");
                        return;
                    }
                    GetZhidanInformation(this.CB_ZhiDan.Text);
                }
                else
                {
                    ClreaUIInformation();
                }

            }
            else
            {
                this.CB_ZhiDan.Text = "";
                ClreaUIInformation();
            }

            IMEIComboxAdditme();
            
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
            this.IMEInumCOBx.Enabled = true;
            this.RFID_Check.Enabled = true;

            if(RePrintHex.Checked == true)
            {
                this.Re_Nocheckcode.Enabled = false;
            }
            else
            {
                this.Re_Nocheckcode.Enabled = true;
            }
            
            this.RePrintHex.Enabled = true;
            this.Get_ZhiDan_Data.Enabled = true;
            this.ProductData.ReadOnly = false;
            this.TemplateNum.ReadOnly = false;

            this.PrintMode1.Enabled = true;
            this.PrintMode2.Enabled = true;

            this.SiginIN.Enabled = true;
            this.QuitBt.Enabled = true;

            //前后缀可写
            this.IMEI_Prefix.ReadOnly = false;
            this.IMEI_Suffix.ReadOnly = false;

            if (this.PrintOne.Checked == true)
            {
                this.InseIMEI2.Enabled = true;

            }

            if (this.PrintOne.Checked == true || this.RePrintOne.Checked == true)
            {
                this.CheckIMEI2.Enabled = true;
                this.CheckSIM.Enabled = true;
                this.CheckBAT.Enabled = true;
                this.CheckICCID.Enabled = true;
                this.CheckMAC.Enabled = true;
                this.CheckEquipment.Enabled = true;
                this.CheckVIP.Enabled = true;
                this.CheckRFID.Enabled = true;
                this.CheckIMEI14.Enabled = true;

                this.IMEI_Start.ReadOnly = false;
                this.IMEI_Start.Focus();
            }
            if (this.PrintMore.Checked == true)
            {
                this.PrintNum.ReadOnly = false;
                this.PrintNum.Focus();
            }

            Form1.recordLuck = 0;
            Form1.recordUpdateUI = 0;
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
            if (this.PrintOne.Checked == false && this.PrintMore.Checked == false && this.RePrintOne.Checked == false && this.RePrintMore.Checked == false && this.Hexadecimal.Checked == false && this.RFID_Check.Checked == false)
            {
                player.Play();
                this.reminder.AppendText("请选则打印方式\r\n");
                return;
            }

            int itme = 0;
            string[] IMEI = this.IMEInumCOBx.Text.Split(',');
            foreach (var startend in IMEI)
            {
                if (itme == 0)
                {
                    SlipIMEIStart = startend;
                    itme++;
                }
                else
                {
                    SlipIMEIEnd = startend;
                    itme = 0;
                }
            }
            
            int Rel = 0;
            if(this.IMEI_Present.Text != "")
            {
                Rel =  IMEIHexOrIrregular(this.IMEI_Present.Text);

            }else
            {
                Rel = IMEIHexOrIrregular(SlipIMEIStart);
            }

            //十六进制提示
            if(Rel == 2 && this.Hexadecimal.Checked == false && this.RePrintHex.Checked == false)
            {
                player.Play();
                this.reminder.AppendText("此制单IMEI为十六进制打印\r\n");
                return;
            }   
            //不规则提示
            if(Rel == 3 && this.PrintOne.Checked == false)
            {
                player.Play();
                this.reminder.AppendText("此制单IMEI为逐个打印\r\n");
                return;
            }

            if(this.RePrintMore.Checked == false && this.RePrintOne.Checked == false)
            {
                if (Rel == 0)
                {
                    if (this.IMEI_Present.Text != "")
                    {
                        if (long.Parse(this.IMEI_Present.Text) < long.Parse(SlipIMEIStart) || long.Parse(this.IMEI_Present.Text) > long.Parse(SlipIMEIEnd))
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI打印位不在范围内\r\n");
                            return;
                        }
                    }
                }
                else if (Rel == 2)
                {
                    if (this.IMEI_Present.Text != "")
                    {
                        long PreIMEI = Convert.ToInt64(this.IMEI_Present.Text, 16);
                        long IMEInum1 = Convert.ToInt64(SlipIMEIStart, 16);
                        long IMEInum2 = Convert.ToInt64(SlipIMEIEnd, 16);

                        if (PreIMEI < IMEInum1 || PreIMEI > IMEInum2)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI打印位不在范围内\r\n");
                            return;
                        }
                    }
                }
            }
            
            int Rel2 = 0;
            if(this.IMEI_Present.Text != "")
            {
                Rel2 =  IMEIHexOrIrregular(this.IMEI2_Present.Text);

            }else
            {
                Rel2 = IMEIHexOrIrregular(this.IMEI2_num1.Text);
            }

            //十六进制提示
            if(Rel2 == 2 && this.Hexadecimal.Checked == false && this.RePrintHex.Checked == false)
            {
                player.Play();
                this.reminder.AppendText("此制单IMEI2为十六进制打印\r\n");
                return;
            }   
            //不规则提示
            if(Rel2 == 3 && this.PrintOne.Checked == false)
            {
                player.Play();
                this.reminder.AppendText("此制单IMEI2为逐个打印\r\n");
                return;
            }

            if(this.RePrintMore.Checked == false && this.RePrintOne.Checked == false)
            {
                 if (Rel2 == 0)
                {
                    if (this.IMEI2_Present.Text != "")
                    {
                        if (long.Parse(this.IMEI2_Present.Text) < long.Parse(this.IMEI2_num1.Text) || long.Parse(this.IMEI2_Present.Text) > long.Parse(this.IMEI2_num2.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI2打印位不在范围内\r\n");
                            return;
                        }
                    }
                }
                else if (Rel2 == 2)
                {
                    if (this.IMEI2_Present.Text != "")
                    {
                         long PreIMEI2 =  Convert.ToInt64(this.IMEI2_Present.Text, 16);
                         long IMEI2num1 =  Convert.ToInt64(this.IMEI2_num1.Text, 16);
                         long IMEI2num2 =  Convert.ToInt64(this.IMEI2_num2.Text, 16);

                        if (PreIMEI2 < IMEI2num1 || PreIMEI2 > IMEI2num2)
                        {
                            player.Play();
                            this.reminder.AppendText("IMEI2打印位不在范围内\r\n");
                            return;
                        }
                    }
                }
            }
            
            FindField = "";

            //拼接查询字段
            if (this.CheckIMEI2.Checked == true)
            {
                FindField += "IMEI2 ,";
            }
            if (this.CheckSIM.Checked == true)
            {
                FindField += "IMEI3 ,";
            }
            if (this.CheckVIP.Checked == true)
            {
                FindField += "IMEI8 ,";
            }
            if (this.CheckBAT.Checked == true)
            {
                FindField += "IMEI9 ,";
            }
            if (this.CheckICCID.Checked == true)
            {
                FindField += "IMEI4 ,";
            }
            if (this.CheckMAC.Checked == true)
            {
                FindField += "IMEI6 ,";
            }
            if (this.CheckEquipment.Checked == true)
            {
                FindField += "IMEI7 ,";
            }
            if (this.CheckRFID.Checked == true)
            {
                FindField += "IMEI13 ,";
            }
            if (this.CheckIMEI2.Checked == true)
            {
                FindField += "IMEI14 ,";
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
            this.CheckIMEI14.Enabled = false;
            this.InseIMEI2.Enabled = false;
            this.RFID_Check.Enabled = false;
            
            this.PrintMode1.Enabled = false;
            this.PrintMode2.Enabled = false;

            //前后缀只读
            this.IMEI_Prefix.ReadOnly = true;
            this.IMEI_Suffix.ReadOnly = true;

            this.IMEInumCOBx.Enabled = false;

            //禁用登录退出
            this.SiginIN.Enabled = false;
            this.QuitBt.Enabled = false;
            
            //this.IMEI2Rel.ReadOnly = true;

            if (this.RePrintOne.Checked == true || this.RePrintMore.Checked == true)
            {
                this.IMEI_Start.ReadOnly = true;
                this.PrintNum.ReadOnly = true;
            }
            this.ProductData.ReadOnly = true;
            this.TemplateNum.ReadOnly = true;
            this.ToUnlock.Enabled = true;

            

            if (this.CB_ZhiDan.Text != "")
            {
                MOPB.UpdateJS_TemplatePathDAL(this.CB_ZhiDan.Text, this.Select_Template1.Text);
            }
            //记录界面复选框
           MFPRPB.InsertPrintRecordParamBLL(this.CB_ZhiDan.Text, Convert.ToInt32(this.PrintOne.Checked), Convert.ToInt32(this.PrintMore.Checked), Convert.ToInt32(this.SnFromCustomer.Checked), Convert.ToInt32(this.NoCheckCode.Checked), Convert.ToInt32(this.NoSn.Checked), Convert.ToInt32(this.Hexadecimal.Checked), Convert.ToInt32(this.SNHex.Checked), Convert.ToInt32(this.RePrintOne.Checked), Convert.ToInt32(this.RePrintMore.Checked), Convert.ToInt32(this.Re_Nocheckcode.Checked), Convert.ToInt32(this.RePrintHex.Checked),
               Convert.ToInt32(this.CheckIMEI2.Checked), Convert.ToInt32(this.CheckSIM.Checked), Convert.ToInt32(this.CheckBAT.Checked) ,Convert.ToInt32(this.CheckICCID.Checked), Convert.ToInt32(this.CheckMAC.Checked), Convert.ToInt32(this.CheckEquipment.Checked), Convert.ToInt32(this.CheckVIP.Checked), Convert.ToInt32(this.CheckRFID.Checked),
               Convert.ToInt32(this.PrintMode1.Checked), Convert.ToInt32(this.PrintMode2.Checked), Convert.ToInt32(this.CheckIMEI14.Checked), Convert.ToInt32(this.InseIMEI2.Checked),Convert.ToInt32(this.RFID_Check.Checked),this.IMEI_Prefix.Text,this.IMEI_Suffix.Text);

            if (this.IMEI_Start.ReadOnly == false)
                this.IMEI_Start.Focus();

            if (this.PrintNum.ReadOnly == false)
                this.PrintNum.Focus();

            if (this.Re_IMEINum.ReadOnly == false)
                this.Re_IMEINum.Focus();

            if (this.ReImeiNum1.ReadOnly == false)
                this.ReImeiNum1.Focus();

            if (this.HexPrintNum.ReadOnly == false)
                this.HexPrintNum.Focus();

            if (this.RFID_Start.ReadOnly == false)
                this.RFID_Start.Focus();

            Form1.recordLuck = 1;
            Form1.recordUpdateUI = 1;
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
            try
            {
                if (this.CB_ZhiDan.Text == "")
                {
                    this.reminder.AppendText("请选择制单号\r\n");
                    return;
                }

                GetZhidanInformation(this.CB_ZhiDan.Text);

                IMEIComboxAdditme();

                //前后缀可写
                this.IMEI_Prefix.ReadOnly = false;
                this.IMEI_Suffix.ReadOnly = false;

                this.StartZhiDan = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex.Message);
            }
            
        }
        //打印模式1
        private void PrintMode1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.PrintMode1.Checked == false)
            {
                this.ModeFalge = 0;
                
                if (RePrintMore.Checked == true)
                {
                    this.ReImei2Num1.ReadOnly = true;
                    this.ReImei2Num2.ReadOnly = true;
                }
                
            }
            else
            {
                if (this.PrintMode2.Checked == true)
                {
                    this.PrintMode2.Checked = false;
                }

                if (RePrintMore.Checked == true)
                {
                    this.ReImei2Num1.ReadOnly = false;
                    this.ReImei2Num2.ReadOnly = false;
                }

                if (this.PrintOne.Checked == true)
                {
                    this.InseIMEI2.Checked = true;
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
                
                if (RePrintMore.Checked == true)
                {
                    this.ReImei2Num1.ReadOnly = true;
                    this.ReImei2Num2.ReadOnly = true;
                }

            }
            else
            {
                if (this.PrintMode1.Checked == true)
                {
                    this.PrintMode1.Checked = false;
                }

                if (RePrintMore.Checked == true)
                {
                    this.ReImei2Num1.ReadOnly = false;
                    this.ReImei2Num2.ReadOnly = false;
                }

                if (this.PrintOne.Checked == true)
                {
                    this.InseIMEI2.Checked = true;
                }
                this.ModeFalge = 2;
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
                CheckFields.Remove(1);
            else
                CheckFields[1] = "IMEI3";
        }
        //查ICCID
        private void CheckICCID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckICCID.Checked == false)
                CheckFields.Remove(2);
            else
                CheckFields[2] = "IMEI4";
        }

        //查MAC
        private void CheckMAC_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckMAC.Checked == false)
                CheckFields.Remove(3);
            else
                CheckFields[3] = "IMEI6";
        }

        //查Equipment
        private void CheckEquipment_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckEquipment.Checked == false)
                CheckFields.Remove(4);
            else
                CheckFields[4] = "IMEI7";

        }

        //查VIP
        private void CheckVIP_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckVIP.Checked == false)
                CheckFields.Remove(5);
            else
                CheckFields[5] = "IMEI8";
        }
        //查BAT
        private void CheckBAT_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckBAT.Checked == false)
                CheckFields.Remove(6);
            else
                CheckFields[6] = "IMEI9";
        }


        //查RFID
        private void CheckRFID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckRFID.Checked == false)
                CheckFields.Remove(7);
            else
                CheckFields[7] = "IMEI13";
        }


        private void CheckIMEI14_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckRFID.Checked == false)
                CheckFields.Remove(8);
            else
                CheckFields[8] = "IMEI14";
        }

        //打开IMEI2扫入框
        private void InseIMEI2_CheckedChanged(object sender, EventArgs e)
        {

            if (this.InseIMEI2.Checked == false)
                this.IMEI2_Start.ReadOnly = true;
            else
                this.IMEI2_Start.ReadOnly = false;
            

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
            if (sigin.UserDes1 != "")
            {
                this.UserDesShow.Text = sigin.UserDes1;
            }
        }

        //退出
        private void QuitBt_Click(object sender, EventArgs e)
        {
            if(this.UserShow.Text != "")
            {
                if (MessageBox.Show("是否退出当前账号？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    this.UserShow.Clear();
                    this.UserDesShow.Clear();
                }

            }

        }

        //选择SN号十六进制事件
        private void SNHex_CheckedChanged(object sender, EventArgs e)
        {
            if(this.SNHex.Checked == true)
            {
                //不打印sn号为真改假
                if(this.NoSn.Checked == true)
                {
                    this.NoSn.Checked = false;
                    c3 = 0;
                }
            }
        }
        

        //刷新模板函数
        private void Refresh_template_Click(object sender, EventArgs e)
        {
            if (this.Select_Template1.Text != "")
            {
                foreach (Process p in Process.GetProcessesByName("bartend"))
                {
                    if (!p.CloseMainWindow())
                    {
                        p.Kill();
                    }
                }
                btEngine.Stop();
                lj = this.Select_Template1.Text;
                btEngine.Start();
                this.reminder.AppendText("刷新模板成功\r\n");
            }

        }

        //打开数据库配置页面
        private void SqlConfig_Click(object sender, EventArgs e)
        {
            UpdateSqlConn US = new UpdateSqlConn(this);
            US.ShowDialog();
        }


        //清空打印模板赋值
        public void ClearTemplate1ToVlue(LabelFormatDocument btFormat)
        {
            btFormat.SubStrings["SIM"].Value = "";
            btFormat.SubStrings["VIP"].Value = "";
            btFormat.SubStrings["BAT"].Value = "";
            btFormat.SubStrings["ICCID"].Value = "";
            btFormat.SubStrings["MAC"].Value = "";
            btFormat.SubStrings["Equipment"].Value = "";
            btFormat.SubStrings["SN"].Value = "";
            btFormat.SubStrings["RFID"].Value = "";
            btFormat.SubStrings["IMEI2"].Value = "";
        }

        private void CB_ZhiDan_DropDown(object sender, EventArgs e)
        {

        }

        //清空关联表赋值
        public void ClearToVlue()
        {
             snstr = "";
             simstr = "";
             iccidstr = "";
             macstr = "";
             equistr = "";
             vipstr = "";
             batstr = "";
             rfidstr = "";
             IMEI2str = "";
        }

        private void CB_ZhiDan_TextChanged(object sender, EventArgs e)
        {
            string str = this.CB_ZhiDan.Text;
            Form1.jSZhidanStr = str;
            if(str == "")
            {
                this.reminder.Text = "";
                ClreaUIInformation();
            }
        }

        //获取--补0
        public string GetLength0(long imeilong,string imeistr)
        {
            string str = "";
            string str0 = "";

            if (imeilong.ToString().Length != imeistr.Length)
            {
                for (int i = 1; i < imeistr.Length; i++)
                {
                    str = imeistr.Substring(0, i);
                    str = str.Substring(str.Length - 1, 1);
                    if (str == "0")
                    {
                        str0 += "0";
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return str0 ;
        }

        //扫入RFID
        private void RFID_Start_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if(checkInformation())
                {
                    this.RFID_Start.Clear();
                    this.RFID_Start.Focus();
                    return;
                }
                
                try
                {
                    //查rfid位数
                    if (this.Rfid_digit.Text == "")
                    {
                        this.reminder.AppendText("RFID位数为空\r\n");
                        this.RFID_Start.Clear();
                        this.RFID_Start.Focus();
                        return;
                    }
                    else
                    {
                        //分割字符串
                        this.RFID_Start.Text = SustringPos(this.RFID_Start.Text);

                        //对比前缀
                        int RFID_prefix_width = this.Rfid_prefix.Text.Length;
                        string RFID_prefix = this.RFID_Start.Text.Substring(0, RFID_prefix_width);
                        if (RFID_prefix != this.Rfid_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("RFID号前缀错误\r\n");
                            this.RFID_Start.Clear();
                            this.RFID_Start.Focus();
                            return;
                        }

                        //查范围
                        if (int.Parse(this.Rfid_digit.Text) == this.RFID_Start.Text.Length)
                        {
                            if(this.Rfid_num1.Text.CompareTo(this.RFID_Start.Text) == 1 || this.Rfid_num2.Text.CompareTo(this.RFID_Start.Text) == -1)
                            {
                                this.reminder.AppendText("RFID不在范围内\r\n");
                                player.Play();
                                this.RFID_Start.Clear();
                                this.RFID_Start.Focus();
                                return;
                            }
                            
                            //查rfid是否存在关联表
                            if (!DRSB.CheckRFIDBLL(this.RFID_Start.Text))
                            {
                                //检查sn号是否为空
                                if (this.SN1_num.Text != "")
                                {
                                    if (this.SN2_num.Text == "")
                                    {
                                        this.reminder.AppendText("SN终止位为空\r\n");
                                        player.Play();
                                        this.RFID_Start.Clear();
                                        this.RFID_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    this.reminder.AppendText("SN起始位为空\r\n");
                                    player.Play();
                                    this.RFID_Start.Clear();
                                    this.RFID_Start.Focus();
                                    return;
                                }

                                if(this.IMEI_num1.Text == "")
                                {
                                    this.reminder.AppendText("IMEI起始位为空\r\n");
                                    player.Play();
                                    this.RFID_Start.Clear();
                                    this.RFID_Start.Focus();
                                    return;
                                }
                                else
                                {
                                    if (this.IMEI_num2.Text == "")
                                    {
                                        this.reminder.AppendText("IMEI终止位为空\r\n");
                                        player.Play();
                                        this.RFID_Start.Clear();
                                        this.RFID_Start.Focus();
                                        return;
                                    }
                                    else
                                    {
                                        if (this.IMEI_num1.Text.Length != this.IMEI_num2.Text.Length)
                                        {
                                            this.reminder.AppendText("IMEI位数不符合\r\n");
                                            player.Play();
                                            this.RFID_Start.Clear();
                                            this.RFID_Start.Focus();
                                            return;
                                        }

                                        if(this.IMEI_Present.Text != "")
                                        {
                                            if (this.IMEI_num1.Text.Length != this.IMEI_Present.Text.Length)
                                            {
                                                this.reminder.AppendText("IMEI位数不符合\r\n");
                                                player.Play();
                                                this.RFID_Start.Clear();
                                                this.RFID_Start.Focus();
                                                return;
                                            }
                                        }
                                    }
                                }
                                long imei_begin;
                                string begin0 = "", imei15, imei_start;
                                
                                //Rfid检查IMEI号
                                if (this.IMEI_Present.Text == "")
                                {
                                    if (long.Parse(this.IMEI_num1.Text) < long.Parse(this.IMEI_num2.Text))
                                    {
                                        imei_begin = long.Parse(this.IMEI_num1.Text);
                                        begin0 = GetLength0(imei_begin, this.IMEI_num1.Text);
                                        imei15 = getimei15(begin0 + imei_begin.ToString());

                                        if(this.NoCheckCode.Checked == true)
                                            imei_start = begin0 + imei_begin;
                                        else
                                            imei_start = begin0 + imei_begin + imei15;

                                       
                                        //查询imei在关联表中是否存在rfid --存在则报错
                                       if ( DRSB.CheckIMEIExitRFIDBLL(imei_start))
                                       {
                                            this.reminder.AppendText("IMEI已绑定RFID\r\n");
                                            player.Play();
                                            this.RFID_Start.Clear();
                                            this.RFID_Start.Focus();
                                            return;
                                       }
                                       else
                                       {
                                            LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                                            ClearTemplate1ToVlue(btFormat);
                                            //指定打印机名称
                                            btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                            //打印份数,同序列打印的份数
                                            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;

                                            btFormat.SubStrings["IMEI"].Value = imei_start;
                                            btFormat.SubStrings["RFID"].Value = this.RFID_Start.Text.Trim();
                                            if (this.NoSn.Checked == false)
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;

                                            if (!PMB.CheckIMEIBLL(imei_start))
                                            {
                                                //对模板相应字段进行赋值
                                                ValueToTemplate(btFormat);
                                                //记录打印信息日志
                                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");

                                                PList.Claer();
                                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                                PList.IMEI = imei_start;
                                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                                PList.SN = this.SN1_num.Text;
                                                PList.SIM = "";
                                                PList.VIP = "";
                                                PList.BAT = "";
                                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                                PList.SoftModel = this.SoftModel.Text.Trim();
                                                PList.Version = this.SoftwareVersion.Text.Trim();
                                                PList.Remark = this.Remake.Text.Trim();
                                                PList.JS_PrintTime = ProductTime;
                                                PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                                PList.CH_PrintTime = "";
                                                PList.CH_TemplatePath1 = null;
                                                PList.CH_TemplatePath2 = null;
                                                PList.RFID = this.RFID_Start.Text;
                                                PList.ICCID = "";
                                                PList.MAC = "";
                                                PList.Equipment = "";
                                                PList.JSUserName = this.UserShow.Text;
                                                PList.JSUserDes = this.UserDesShow.Text;
                                                if (PMB.InsertPrintMessageBLL(PList))
                                                {
                                                    if (DRSB.CheckIMEIBLL(imei_start))
                                                    {
                                                        DRSB.UpdateSN_RFIDDAL(imei_start, this.SN1_num.Text, this.RFID_Start.Text);
                                                    }
                                                    else
                                                    {
                                                        //记录关联数据信息到关联表
                                                        Drs.Claer();
                                                        Drs.IMEI1 = imei_start;
                                                        Drs.IMEI2 = this.SN1_num.Text;

                                                        Drs.RFID = this.RFID_Start.Text;
                                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                        DRSB.InsertRelativeSheetBLL(Drs);
                                                    }
                                                    if (this.SNHex.Checked == false)
                                                    {
                                                        string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                        long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                        string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                        string sn2_suffix = this.SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                        MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'), begin0 + (imei_begin).ToString());
                                                        this.SN1_num.Text = sn1;
                                                    }
                                                    else
                                                    {
                                                        string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                        string Hex = this.SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                                        string sn_16str = (Convert.ToInt64(Hex, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                        string sn1 = sn1_prefix + sn_16str;
                                                        MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_16str, begin0 + (imei_begin).ToString());
                                                        this.SN1_num.Text = sn1;
                                                    }

                                                    this.IMEI_Present.Text = begin0 + imei_begin.ToString();

                                                    btFormat.Print();
                                                    Form1.Log("打印了IMEI号为" + this.IMEI_num1.Text + "，RFID号为 " + this.RFID_Start.Text + "的制单", null);
                                                    this.RFID_Start.Clear();
                                                    this.RFID_Start.Focus();

                                                }
                                                else
                                                {
                                                    this.reminder.AppendText("上传数据失败\r\n");
                                                    player.Play();
                                                    this.RFID_Start.Clear();
                                                    this.RFID_Start.Focus();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                this.reminder.AppendText("IMEI已存在打印表\r\n");
                                                player.Play();
                                                this.RFID_Start.Clear();
                                                this.RFID_Start.Focus();
                                                return;
                                            }
                                        }
                                       

                                    }
                                    else
                                    {
                                        this.reminder.AppendText("IMEI超出范围\r\n");
                                        player.Play();
                                        this.RFID_Start.Clear();
                                        this.RFID_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    if ((long.Parse(this.IMEI_Present.Text) < long.Parse(this.IMEI_num1.Text)) || (long.Parse(this.IMEI_Present.Text) >= long.Parse(this.IMEI_num2.Text)))
                                    {
                                        this.reminder.AppendText("IMEI不在范围内\r\n");
                                        player.Play();
                                        this.RFID_Start.Clear();
                                        this.RFID_Start.Focus();
                                        return;
                                    }

                                    imei_begin = long.Parse(this.IMEI_Present.Text) + 1;
                                    begin0 = GetLength0(imei_begin, this.IMEI_Present.Text);
                                    imei15 = getimei15(begin0 + imei_begin.ToString());
                                    if (this.NoCheckCode.Checked == true)
                                        imei_start = begin0 + imei_begin;
                                    else
                                        imei_start = begin0 + imei_begin + imei15;


                                    //查询imei在关联表中是否存在rfid --存在则报错
                                    if (DRSB.CheckIMEIExitRFIDBLL(imei_start))
                                    {
                                        this.reminder.AppendText("IMEI已绑定RFID\r\n");
                                        player.Play();
                                        this.RFID_Start.Clear();
                                        this.RFID_Start.Focus();
                                        return;
                                    }
                                    else
                                    {
                                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                                        ClearTemplate1ToVlue(btFormat);
                                        //指定打印机名称
                                        btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                        //打印份数,同序列打印的份数
                                        btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;

                                        if (!PMB.CheckIMEIBLL(imei_start))
                                        {
                                            btFormat.SubStrings["IMEI"].Value = imei_start;
                                            btFormat.SubStrings["RFID"].Value = this.RFID_Start.Text.Trim();
                                            if (this.NoSn.Checked == false)
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;

                                            //对模板相应字段进行赋值
                                            ValueToTemplate(btFormat);
                                            //记录打印信息日志
                                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");

                                            PList.Claer();
                                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                            PList.IMEI = imei_start;
                                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                            PList.SN = this.SN1_num.Text;
                                            PList.IMEIRel = this.IMEIRel.Text.Trim();
                                            PList.SoftModel = this.SoftModel.Text.Trim();
                                            PList.Version = this.SoftwareVersion.Text.Trim();
                                            PList.Remark = this.Remake.Text.Trim();
                                            PList.JS_PrintTime = ProductTime;
                                            PList.JS_TemplatePath = this.Select_Template1.Text.Trim();
                                            PList.CH_PrintTime = "";
                                            PList.CH_TemplatePath1 = null;
                                            PList.CH_TemplatePath2 = null;
                                            PList.RFID = this.RFID_Start.Text;
                                            PList.JSUserName = this.UserShow.Text;
                                            PList.JSUserDes = this.UserDesShow.Text;
                                            if (PMB.InsertPrintMessageBLL(PList))
                                            {
                                                if (DRSB.CheckIMEIBLL(imei_start))
                                                {
                                                    DRSB.UpdateSN_RFIDDAL(imei_start, this.SN1_num.Text, this.RFID_Start.Text);
                                                }
                                                else
                                                {
                                                    //记录关联数据信息到关联表
                                                    Drs.Claer();
                                                    Drs.IMEI1 = imei_start;
                                                    Drs.IMEI2 = this.SN1_num.Text;

                                                    Drs.RFID = this.RFID_Start.Text;
                                                    Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                    Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                    DRSB.InsertRelativeSheetBLL(Drs);
                                                }

                                                if (this.SNHex.Checked == false)
                                                {
                                                    string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                    string sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                    string sn2_suffix = this.SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'), begin0 + (imei_begin).ToString());
                                                    this.SN1_num.Text = sn1;
                                                }
                                                else
                                                {
                                                    string sn1_prefix = this.SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    string Hex = this.SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s);
                                                    string sn_16str = (Convert.ToInt64(Hex, 16) + Convert.ToInt64("1", 16)).ToString("X").PadLeft(s, '0');
                                                    string sn1 = sn1_prefix + sn_16str;
                                                    MOPB.UpdateSNnumberBLL(this.CB_ZhiDan.Text, sn_16str, begin0 + (imei_begin).ToString());
                                                    this.SN1_num.Text = sn1;
                                                }

                                                this.IMEI_Present.Text = begin0 + imei_begin.ToString();

                                                btFormat.Print();
                                                Form1.Log("打印了IMEI号为" + this.IMEI_Present.Text + "，RFID号为 " + this.RFID_Start.Text + "的制单", null);
                                                this.RFID_Start.Clear();
                                                this.RFID_Start.Focus();

                                            }
                                            else
                                            {
                                                this.reminder.AppendText("上传数据失败\r\n");
                                                player.Play();
                                                this.RFID_Start.Clear();
                                                this.RFID_Start.Focus();
                                                return;
                                            }

                                        }
                                        else
                                        {
                                            this.reminder.AppendText("IMEI已存在打印表\r\n");
                                            player.Play();
                                            this.RFID_Start.Clear();
                                            this.RFID_Start.Focus();
                                            return;
                                        }
                                    }
                                    
                                }
                            }
                            else
                            {
                                this.reminder.AppendText("RFID已存在关联表\r\n");
                                player.Play();
                                this.RFID_Start.Clear();
                                this.RFID_Start.Focus();
                                return;
                            }
                        }
                        else
                        {
                            this.reminder.AppendText("RFID位数不符\r\n");
                            player.Play();
                            this.RFID_Start.Clear();
                            this.RFID_Start.Focus();
                            return;
                        }
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Exception:" + ex.Message);
                }
                
            }
        }


        //IMEI十六进制与不规则查询
        public int IMEIHexOrIrregular(string Strimei)
        {
            int HexOrIrre = 0;
            int NO_HexOrIrre = 0;
            foreach(char c in  Strimei)
            {
                if (char.IsNumber(c))
                {
                    continue;

                }
                else
                {
                    //判断是大小（a~f）十六进制范围
                    if( ( (int)c < 71 && (int)c > 64 ) || ((int)c < 103 && (int)c > 96))
                    {
                        HexOrIrre = 1;
                    }
                    //不规则字符
                    else
                    {
                        NO_HexOrIrre = 1;
                    }
                }
            }

            if (NO_HexOrIrre == 1) //不规则字符
                return 3;

            if (HexOrIrre == 1) //十六进制
                return 2;

              return HexOrIrre; //全数字
            
        }

        //更新机身UI
        public void UpdateUIdata()
        {
            this.CB_ZhiDan.Items.Clear();
            G_MOP.Clear();
            G_MOP = MOPB.SelectZhidanNumBLL();
            foreach (Gps_ManuOrderParam a in G_MOP)
            {
                this.CB_ZhiDan.Items.Add(a.ZhiDan);
            }
            if (MOPB.CheckZhiDanBLL(Form1.jSZhidanStr))
            {
                if (this.StartZhiDan == 1)
                {
                    if (this.CB_ZhiDan.Text == "")
                    {
                        this.reminder.AppendText("请选择制单号\r\n");
                        return;
                    }
                    GetZhidanInformation(Form1.jSZhidanStr);
                }
                else
                {
                    ClreaUIInformation();
                }
            }
            else
            {
                this.CB_ZhiDan.Text = "";
                ClreaUIInformation();
            }

            IMEIComboxAdditme();
        }


        public void IMEIComboxAdditme()
        {
            this.IMEInumCOBx.Items.Clear();
            this.IMEInumCOBx.Items.Add(this.IMEI_num1.Text + "," + this.IMEI_num2.Text);
            if (this.IMEI_Range.Text != "")
            {
                string chatstr = this.IMEI_Range.Text.Substring(this.IMEI_Range.Text.Length - 1, 1);
                if (chatstr == ";")
                {
                    //分割IMEI号段范围
                    InitIMEIArry();
                }
                else
                {
                    player.Play();
                    this.reminder.AppendText("IMEI多号段格式不正确\r\n");
                    return;
                }
            }

            this.IMEInumCOBx.SelectedIndex = 0;
        }


        public void ClreaUIInformation()
        {
            this.IMEI_Start.Clear();
            this.IMEI2_Start.Clear();
            this.HexPrintNum.Clear();
            this.PrintNum.Clear();
            this.ProductData.Clear();
            this.Re_IMEINum.Clear();
            this.ReImeiNum1.Clear();
            this.ReImeiNum2.Clear();
            this.ReImei2Num1.Clear();
            this.ReImei2Num2.Clear();
            this.RFID_Start.Clear();

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

            this.IMEI2_num1.Clear();
            this.IMEI2_num2.Clear();
            this.IMEI2_Present.Clear();

            this.Rfid_digit.Clear();
            this.Rfid_prefix.Clear();
            this.Rfid_num1.Clear();
            this.Rfid_num2.Clear();

            this.IMEI_Prefix.Clear();
            this.IMEI_Suffix.Clear();

            this.IMEInumCOBx.Items.Clear();
            this.IMEInumCOBx.Text = "";

            this.StartZhiDan = 0;

            this.CheckIMEI2.Checked = false;
            this.CheckSIM.Checked = false;
            this.CheckBAT.Checked = false;
            this.CheckICCID.Checked = false;
            this.CheckMAC.Checked = false;
            this.CheckEquipment.Checked = false;
            this.CheckVIP.Checked = false;
            this.CheckRFID.Checked = false;
            this.CheckIMEI14.Checked = false;


            this.CheckIMEI2.Enabled = false;
            this.CheckSIM.Enabled = false;
            this.CheckBAT.Enabled = false;
            this.CheckICCID.Enabled = false;
            this.CheckMAC.Enabled = false;
            this.CheckEquipment.Enabled = false;
            this.CheckVIP.Enabled = false;
            this.CheckRFID.Enabled = false;
            this.CheckIMEI14.Enabled = false;

            this.PrintMode1.Checked = false;
            this.PrintMode2.Checked = false;
            
            this.InseIMEI2.Checked = false;//绑定IMEI2
            this.InseIMEI2.Enabled = false;
            this.IMEI2_Start.ReadOnly = true;

            this.PrintOne.Checked = false;//逐个
            this.PrintMore.Checked = false;//批量
            this.RePrintOne.Checked = false;//重打逐个打印
            this.RePrintMore.Checked = false;//重打批量打印
            this.SnFromCustomer.Checked = false;//客供
            this.NoCheckCode.Checked = false;//不打印校验码
            this.Re_Nocheckcode.Checked = false;//重打不打印校验码
            this.NoSn.Checked = false;//不打印SN
            this.Hexadecimal.Checked = false;//十六进制
            this.SNHex.Checked = false;//SN十六进制
            this.RFID_Check.Checked = false;//rfid

            this.PrintNum.ReadOnly = true;
            this.HexPrintNum.ReadOnly = true;
            this.IMEI_Start.ReadOnly = true;
            this.IMEI2_Start.ReadOnly = true;
            this.Re_IMEINum.ReadOnly = true;
            this.ReImeiNum1.ReadOnly = true;
            this.ReImeiNum2.ReadOnly = true;
            this.ReImei2Num1.ReadOnly = true;
            this.ReImei2Num2.ReadOnly = true;
            this.RFID_Start.ReadOnly = true;
        }

        //批量打印查询打印表和镭雕打印表IMEI号是否重号
        public bool Check_MP_LP_Print(string IMEI1,string IMEI2)
        {
            list.Clear();
            list = PMB.CheckRangeIMEIBLL(IMEI1, IMEI2);
            if (list.Count > 0)
            {
                foreach (PrintMessage a in list)
                {
                    this.reminder.AppendText(a.IMEI + "重号\r\n");
                }
                
                return true;
            }

            list.Clear();
            list = LPMDB.CheckRangeIMEIBLL(IMEI1, IMEI2);
            if (list.Count > 0)
            {
                foreach (PrintMessage a in list)
                {
                    this.reminder.AppendText(a.IMEI + "重号\r\n");
                }
                return true;
            }

            return false;
        }

        //自动获取周期
        private int GetWeekOfYear(DateTime dt)
        {
            GregorianCalendar gc = new GregorianCalendar();
            return gc.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }


        //分割字符串
        public string SustringPos(string str)
        {

            if(this.IMEI_Prefix.Text !="" && this.IMEI_Suffix.Text !="")
            {
                if (str.Contains(this.IMEI_Prefix.Text) && str.Contains(this.IMEI_Suffix.Text))
                {
                    str = str.Replace(this.IMEI_Prefix.Text, "");
                    str = str.Replace(this.IMEI_Suffix.Text, "");
                    return str;
                }
                else
                {
                    return str;
                }
            }
            else
            {
                return str;
            }
            
        }

        //查找 ， ; 数量
        public int FindCharCount(string str, string charstr)
        {
            string strlen = str.Replace(charstr, "");
            return (str.Length - strlen.Length);

        }

        public void InitIMEIArry()
        {
            string[] RangeIMEI = this.IMEI_Range.Text.Split(';');
            foreach (var range in RangeIMEI)
            {
                if (range == "")
                    break;

                this.IMEInumCOBx.Items.Add(range);
                
            }

        }

        public bool checkInformation()
        {
            if (this.UserShow.Text == "")
            {
                this.reminder.AppendText("请先登录\r\n");
                return true;
            }
            if (this.CB_ZhiDan.Text == "")
            {
                player2.Play();
                this.reminder.AppendText("请先选择制单\r\n");
                return true;
            }
            if (this.Select_Template1.Text == "")
            {
                player1.Play();
                this.reminder.AppendText("请先选择模板\r\n");
                return true;
            }
            if (this.StartZhiDan == 0)
            {
                this.reminder.AppendText("请获取制单数据\r\n");
                return true;
            }
            if (this.ToLock.Enabled == true)
            {
                this.reminder.AppendText("请锁定\r\n");
                return true;
            }

            return false;
        }

        //选择RFID打印事件
        private void RFID_Check_CheckedChanged(object sender, EventArgs e)
        {
            if (this.RFID_Check.Checked == true)
            {
                this.Re_Nocheckcode.Checked = false;
                this.CheckIMEI2.Checked = false;
                this.CheckSIM.Checked = false;
                this.CheckBAT.Checked = false;
                this.CheckICCID.Checked = false;
                this.CheckMAC.Checked = false;
                this.CheckEquipment.Checked = false;
                this.CheckVIP.Checked = false;
                this.CheckRFID.Checked = false;
                this.CheckIMEI14.Checked = false;
                this.InseIMEI2.Checked = false;
                
                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;
                this.CheckIMEI14.Enabled = false;
                this.InseIMEI2.Enabled = false;


                this.PrintOne.Checked = false;
                this.PrintMore.Checked = false;
                this.Hexadecimal.Checked = false;
                this.RePrintOne.Checked = false;
                this.RePrintMore.Checked = false;
                this.RePrintHex.Checked = false;
                this.PrintMode1.Checked = false;
                this.PrintMode2.Checked = false;

                
                this.IMEI_Start.ReadOnly = true;
                this.IMEI2_Start.ReadOnly = true;
                this.PrintNum.ReadOnly = true;
                this.HexPrintNum.ReadOnly = true;
                this.Re_IMEINum.ReadOnly = true;
                this.ReImeiNum1.ReadOnly = true;
                this.ReImeiNum2.ReadOnly = true;
                this.ReImei2Num1.ReadOnly = true;
                this.ReImei2Num2.ReadOnly = true;
                this.RFID_Start.ReadOnly = true;

                this.IMEI_Start.Clear();
                this.IMEI2_Start.Clear();
                this.PrintNum.Clear();
                this.HexPrintNum.Clear();
                this.Re_IMEINum.Clear();
                this.ReImeiNum1.Clear();
                this.ReImeiNum2.Clear();
                this.ReImei2Num1.Clear();
                this.ReImei2Num2.Clear();
                this.RFID_Start.Clear();

                this.RFID_Start.ReadOnly = false;
                this.RFID_Start.Focus();
                
            }
            else
            {
                this.RFID_Start.ReadOnly = true;
            }

        }

        //获取制单信息
        public void GetZhidanInformation(string zhidan)
        {
            //获取状态0、1、2制单数据
            Gps_ManuOrderParam b = MOPB.selectManuOrderParamByzhidanllBLL(zhidan);

            if (b.status == 0)
            {
                if (b.Week == -1)
                {
                    this.Week_count.Text = "";
                }
                else
                {
                    //不为空则更新为最新的周期
                    DateTime dt = System.DateTime.Now;
                    int dateInt = GetWeekOfYear(dt);
                    if (0 < dateInt && dateInt < 10)
                        this.Week_count.Text = "0" + dateInt.ToString();
                    else
                        this.Week_count.Text = dateInt.ToString();
                }
            }
            else
            {
                if (b.Week == -1)
                {
                    this.Week_count.Text = "";
                }
                else
                {
                    if (b.Week > 0 && b.Week < 9)
                        this.Week_count.Text = "0" + b.Week.ToString().Trim();
                    else
                        this.Week_count.Text = b.Week.ToString().Trim();

                }
            }
            s = b.SN2.Length;
            this.SoftModel.Text = b.SoftModel;
            this.SN1_num.Text = b.SN1 + this.Week_count.Text + b.SN2;
            this.SN2_num.Text = b.SN1 + this.Week_count.Text + b.SN3;
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
            this.IMEI2_num1.Text = b.IMEI2Start;
            this.IMEI2_num2.Text = b.IMEI2End;
            this.IMEI2_Present.Text = b.IMEI2Prints;
            this.Rfid_num1.Text = b.RFIDStart;
            this.Rfid_num2.Text = b.RFIDEnd;
            this.Rfid_digit.Text = b.RFID_digits;
            this.Rfid_prefix.Text = b.RFID_prefix;
            this.IMEI_Range.Text = b.IMEIRangeNum;
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

            string PresentImei = PMB.SelectPresentImeiByZhidanBLL(zhidan);
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

            string PresentImei2 = PMB.SelectPresentImei2ByZhidanBLL(zhidan);
            if (PresentImei2.Length == 15 && this.IMEI_num1.Text.Length == 14)
            {
                string PresentImei1 = PresentImei2.Substring(0, 14);
                this.IMEI2_Present.Text = PresentImei1;
            }
            else
            {
                if (PresentImei2 != "")
                {
                    this.IMEI2_Present.Text = PresentImei2;
                }
            }

            //操作记录
            ManuFuselagePrintRecordParam a = MFPRPB.selectRecordParamByzhidanBLL(zhidan);
            this.PrintOne.Checked = Convert.ToBoolean(a.PrintOneByOne);
            if (this.PrintOne.Checked == true)
            {
                this.InseIMEI2.Enabled = true;
                this.CheckIMEI2.Enabled = true;
                this.CheckSIM.Enabled = true;
                this.CheckBAT.Enabled = true;
                this.CheckICCID.Enabled = true;
                this.CheckMAC.Enabled = true;
                this.CheckEquipment.Enabled = true;
                this.CheckVIP.Enabled = true;
                this.CheckRFID.Enabled = true;
                this.CheckIMEI14.Enabled = true;

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
                this.CheckIMEI14.Enabled = false;


                this.Re_IMEINum.ReadOnly = true;
                this.IMEI_Start.ReadOnly = true;
                this.ReImeiNum1.ReadOnly = true;
                this.ReImeiNum2.ReadOnly = true;
                this.Re_IMEINum.Clear();
                this.ReImeiNum1.Clear();
                this.ReImeiNum2.Clear();
                this.IMEI_Start.Clear();

                this.InseIMEI2.Enabled = false;
                this.PrintNum.Clear();
                this.PrintNum.Focus();
                this.PrintNum.ReadOnly = false;
            }
            this.SnFromCustomer.Checked = Convert.ToBoolean(a.CustomerSupplySN);
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

            this.NoCheckCode.Checked = Convert.ToBoolean(a.NoPrintCheckCode);
            if (this.NoCheckCode.Checked == true)
            {
                c2 = 2;
            }
            else
            {
                c2 = 0;
                this.Hexadecimal.Checked = false;
            }
            this.NoSn.Checked = Convert.ToBoolean(a.NoPrintingSN);
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

            this.Hexadecimal.Checked = Convert.ToBoolean(a.IMEIHexadecimal);
            if (this.Hexadecimal.Checked == true)
            {
                this.HexPrintNum.ReadOnly = false;
                this.HexPrintNum.Focus();
                this.HexPrintNum.BringToFront();
                if (NoCheckCode.Checked == false)
                {
                    this.NoCheckCode.Checked = true;
                    c2 = 2;
                }

            }
            this.SNHex.Checked = Convert.ToBoolean(a.SNHexadecimal);
            this.RePrintOne.Checked = Convert.ToBoolean(a.ReplayOneByOne);
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
                this.CheckIMEI14.Enabled = true;

                this.InseIMEI2.Enabled = false;

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
                this.InseIMEI2.Enabled = false;
                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;
                this.CheckIMEI14.Enabled = false;
                
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
            this.CheckIMEI14.Checked = Convert.ToBoolean(a.JSCheckIMEI2Mark);
            this.InseIMEI2.Checked = Convert.ToBoolean(a.JSCheckInseIMEI2Mark);
            this.RFID_Check.Checked = Convert.ToBoolean(a.RfidMark);

            //获取IMEI前后缀
            this.IMEI_Prefix.Text = a.IMEI_Prefix;
            this.IMEI_Suffix.Text = a.IMEI_Suffix;

            if (this.PrintMode1.Checked == true || this.PrintMode2.Checked == true)
            {
                if (this.PrintOne.Checked == true)
                {
                    this.IMEI2_Start.ReadOnly = false;
                }
            }

            if (this.PrintOne.Checked == false && this.PrintMore.Checked == false && this.RePrintOne.Checked == false && this.RePrintMore.Checked == false && this.Hexadecimal.Checked == false&& this.RFID_Check.Checked == false)
            {
                this.InseIMEI2.Enabled = false;
                this.PrintMore.Checked = true;
                this.CheckIMEI2.Enabled = false;
                this.CheckSIM.Enabled = false;
                this.CheckBAT.Enabled = false;
                this.CheckICCID.Enabled = false;
                this.CheckMAC.Enabled = false;
                this.CheckEquipment.Enabled = false;
                this.CheckVIP.Enabled = false;
                this.CheckRFID.Enabled = false;
                this.CheckIMEI14.Enabled = false;

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


        //逐个打印查询关联字段
        public bool CheckFieldsChoice(string IMEI,int Choice ,int mode , int reset, LabelFormatDocument btFormat)
        {
            ClearToVlue();
            List<DataRelativeSheet> drs = DRSB.GetCheckIMEIBLL(IMEI);
            if (drs.Count == 1)
            {
                foreach (DataRelativeSheet a in drs)
                {
                    if (this.CheckIMEI2.Checked == true)
                    {
                        if (a.IMEI2 != "")
                        {
                            if(Choice == 4 || Choice == 6)
                            {}
                            else
                            {
                                btFormat.SubStrings["SN"].Value = a.IMEI2;
                                snstr = a.IMEI2;
                            }

                        }
                        else
                        {
                            this.reminder.AppendText("SN号为空\r\n");
                            player.Play();
                            return false;
                        }
                    }

                    if (this.CheckSIM.Checked == true)
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
                            return false;
                        }
                    }

                    if (this.CheckICCID.Checked == true)
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
                            return false;
                        }
                    }

                    if (this.CheckMAC.Checked == true)
                    {
                        if (a.IMEI6 != "")
                        {
                            btFormat.SubStrings["MAC"].Value = a.IMEI6;
                            macstr = a.IMEI6;
                        }
                        else
                        {
                            this.reminder.AppendText("蓝牙号为空\r\n");
                            player.Play();
                            return false;
                        }
                    }

                    if (this.CheckEquipment.Checked == true)
                    {
                        if (a.IMEI7 != "")
                        {
                            btFormat.SubStrings["Equipment"].Value = a.IMEI7;
                            equistr = a.IMEI7;
                        }
                        else
                        {
                            this.reminder.AppendText("设备号为空\r\n");
                            player.Play();
                            return false;
                        }

                    }

                    if (this.CheckVIP.Checked == true)
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
                            return false;
                        }
                    }

                    if (this.CheckBAT.Checked == true)
                    {
                        if (a.IMEI9 != "")
                        {
                            btFormat.SubStrings["BAT"].Value = a.IMEI9;
                            batstr = a.IMEI9;
                        }
                        else
                        {
                            this.reminder.AppendText("BAT号为空\r\n");
                            player.Play();
                            return false;
                        }
                    }

                    if (this.CheckRFID.Checked == true)
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
                            return false;
                        }
                    }

                    if(reset == 0)
                    {
                        if (this.CheckIMEI14.Checked == true)
                        {
                            if (a.IMEI14 != "")
                            {
                                if (mode == 2)
                                {

                                }
                                else if (mode == 1)
                                {
                                    btFormat.SubStrings["IMEI2"].Value = a.IMEI14;
                                    IMEI2str = a.IMEI14;
                                }

                            }
                            else
                            {
                                this.reminder.AppendText("IMEI2为空\r\n");
                                player.Play();
                                return false;
                            }
                        }
                    }
                    
                }
                return true;
            }
            else
            {
                this.reminder.AppendText(this.IMEI_Start.Text + "无关联\r\n");
                player.Play();
                return false;
            }
        }
    }
}
