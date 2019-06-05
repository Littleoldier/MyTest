using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Seagull.BarTender.Print;
using System.Windows.Forms;
using Print_Message;
using Print.Message.Bll;
using ManuOrder.Param.BLL;
using TestResult.Param.BLL;
using DataRelative.Param.BLL;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using System.Media;
using System.Threading;
using System.Linq;
using ManuPrintRecord.Param.BLL;

namespace WindowsForms_print
{
    public partial class Color_Box : Form
    {

        public static void Log(string msg, Exception e)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + System.DateTime.Now.ToString("yyyy-MM-dd")+".log";
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
                string path = AppDomain.CurrentDomain.BaseDirectory + System.DateTime.Now.ToString("yyyy-MM-dd") + ".log";
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

        SoundPlayer player = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "失败.wav");
        SoundPlayer player1 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "请先选择模板.wav");
        SoundPlayer player2 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "请选择制单号.wav");
        SoundPlayer player3 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "校验错误.wav");
        SoundPlayer player4 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "成功.wav");
        SoundPlayer player5 = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "前测试位漏测.wav");

        string outString;
        
        ManuOrderParamBLL MOPB = new ManuOrderParamBLL();

        PrintMessageBLL PMB = new PrintMessageBLL();

        InputExcelBLL IEB = new InputExcelBLL();

        TestResultBLL TRB = new TestResultBLL();

        DataRelativeSheetBLL DRSB = new DataRelativeSheetBLL();

        ManuPrintRecordParamBLL MPRPB = new ManuPrintRecordParamBLL();

        List<Gps_ManuOrderParam> G_MOP = new List<Gps_ManuOrderParam>();

        List<PrintMessage> list = new List<PrintMessage>();

        PrintMessage PList = new PrintMessage();

        List<DataRelativeSheet> drs = new List<DataRelativeSheet>();

        List<DataRelativeSheet> LDrs = new List<DataRelativeSheet>();

        DataRelativeSheet Drs = new DataRelativeSheet();

        List<ManuPrintRecordParam> mprp = new List<ManuPrintRecordParam>();

        Engine btEngine = new Engine();
        LabelFormatDocument btFormat;
        

        //使用字典容器和数组存储关联关系
        SortedDictionary<int,string> AssociatedFields = new SortedDictionary<int, string>();
        int[] SortDictio = new int[6];
        int slog = 1;

        //查功能，查耦合，查写好，查参数等用到的参数
        string MissingSql = "";
        string MissingIMEI = "";
        int checklog =0; //是否触发查询工位标志位

        //lj为打印模板路径
        string lj = "";
        string lj2 = "";
        //设置打印模板份数
        int TN1 = 1;
        int TN2 = 1;

        //dj为打印机路径
        string dj = "";
        string dj2 = "";
        //打印时间
        string ProductTime = "";
        //SN参数
        string sn1_prefix;
        long sn1_suffix;
        string sn1 ;
        string ASS_sn;
        int Sn_mark = 0;

        //记录模板刷新次数
        int RefreshNum = 0;

        //记录SN号后缀位数
        int s;
        //记录制单号
        string G_zhidan;

        //拼接查询字段
        string FindField;

        //记录关联表返回值IMEI
        string Gl_IMEI;

        //控制线程的变量
        int xc;
        int xc2 = 0;
        //记录IMEI等，主要用于双模板线程使用
        string DZSN,IMEI,GLBSN,SIM,VIP,BAT,ICCID,MAC,Equipment,RFID;

        //调试打印双模板线程
        Thread thread2;
        //bool tl2;

        //查询字段容器
        SortedDictionary<int, string> CheckFields = new SortedDictionary<int, string>();

        //绑定累数
        int BindingCountS;
        int BindingCountF;
        int UpInt1 , UpInt2 ;

        public Color_Box()
        {
            InitializeComponent();
            int wid = Screen.PrimaryScreen.WorkingArea.Width;
            this.Width = wid;
            CheckForIllegalCrossThreadCalls = false;
        }

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnvalue, int buffersize, string filepath);


        private string IniFilePath;
        private void GetValue(string section, string key, out string value)
        {
            IniFilePath = AppDomain.CurrentDomain.BaseDirectory + "PrintVariable.ini";
            StringBuilder stringBuilder = new StringBuilder();
            GetPrivateProfileString(section, key, "", stringBuilder, 1024, IniFilePath);
            value = stringBuilder.ToString();
        }

        private void Color_Box_Load(object sender, EventArgs e)
        {
            PrintDocument print = new PrintDocument();
            string sDefault = print.PrinterSettings.PrinterName;//默认打印机名
            this.Printer2.Text = sDefault;
            foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
            {
                Printer2.Items.Add(sPrint);
            }
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
            this.PrintDate.Text = NowData;
            //开启打印机引擎
            btEngine.Start();

            thread2 = new Thread(new ThreadStart(PrintTemplate2));
            if (thread2.ThreadState != ThreadState.Running)
            {
                //启动线程
                thread2.Start();
            }
        }

        //选择模板1
        private void open_template1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件|*.btw";
            ofd.ShowDialog();
            string path = ofd.FileName;
            this.Select_Template1.Text = path;
            lj = path;
        }

        //选择模板2
        private void open_template2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件|*.btw";
            ofd.ShowDialog();
            string path = ofd.FileName;
            this.Select_Template2.Text = path;
            lj2 = path;

        }

        private void CB_ZhiDan_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAll();
            string NowData = System.DateTime.Now.ToString("yyyy.MM.dd");
            this.PrintDate.Text = NowData;
            string ZhidanNum = this.CB_ZhiDan.Text;
            Gps_ManuOrderParam b = MOPB.selectManuOrderParamByzhidanBLL(ZhidanNum,0);

            s = b.SN2.Length;
            this.SoftModel.Text = b.SoftModel;
            this.SN1_num.Text = b.SN1 + b.SN2;
            this.SN2_num.Text = b.SN1 + b.SN3;
            this.ProductDate.Text = b.ProductDate;
            this.Color.Text = b.Color;
            this.Weight.Text = b.Weight;
            this.ProductNo.Text = b.ProductNo;
            this.SoftwareVersion.Text = b.Version;
            this.IMEI_num1.Text = b.IMEIStart;
            this.IMEI_num2.Text = b.IMEIEnd;
            this.SIM_num1.Text = b.SIMStart;
            this.SIM_num2.Text = b.SIMEnd;
            this.SIM_digits.Text = b.SIM_digits;
            this.SIM_prefix.Text = b.SIM_prefix;
            this.BAT_num1.Text = b.BATStart;
            this.BAT_num2.Text = b.BATEnd;
            this.BAT_digits.Text = b.BAT_digits;
            this.BAT_prefix.Text = b.BAT_prefix;
            this.VIP_num1.Text = b.VIPStart;
            this.VIP_num2.Text = b.VIPEnd;
            this.VIP_digits.Text = b.VIP_digits;
            this.VIP_prefix.Text = b.VIP_prefix;
            this.ICCID_digits.Text = b.ICCID_digits;
            this.ICCID_prefix.Text = b.ICCID_prefix;
            this.MAC_digits.Text = b.MAC_digits;
            this.MAC_prefix.Text = b.MAC_prefix;
            this.Equipment_digits.Text = b.Equipment_digits;
            this.Equipment_prefix.Text = b.Equipment_prefix;
            this.RFID_Num1.Text = b.RFIDStart;
            this.RFID_Num2.Text = b.RFIDEnd;
            this.RFID_digits.Text = b.RFID_digits;
            this.RFID_prefix.Text = b.RFID_prefix;
            this.Select_Template1.Text = b.CHT_template1;
            lj = b.CHT_template1;
            this.Select_Template2.Text = b.CHT_template2;
            lj2 = b.CHT_template2;
            this.PrintDate.Text = b.ProductDate;
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
            if (b.status == 0)
            {
                this.SN1_num.ReadOnly = false;
                this.SN2_num.ReadOnly = false;
                this.ProductDate.ReadOnly = false;
                this.VIP_num1.ReadOnly = false;
                this.VIP_num2.ReadOnly = false;
                this.VIP_digits.ReadOnly = false;
                this.VIP_prefix.ReadOnly = false;
                this.BAT_num1.ReadOnly = false;
                this.BAT_num2.ReadOnly = false;
                this.BAT_digits.ReadOnly = false;
                this.BAT_prefix.ReadOnly = false;
                this.SIM_num1.ReadOnly = false;
                this.SIM_num2.ReadOnly = false;
                this.SIM_digits.ReadOnly = false;
                this.SIM_prefix.ReadOnly = false;
                this.ICCID_digits.ReadOnly = false;
                this.ICCID_prefix.ReadOnly = false;
                this.MAC_digits.ReadOnly = false;
                this.MAC_prefix.ReadOnly = false;
                this.Equipment_digits.ReadOnly = false;
                this.Equipment_prefix.ReadOnly = false;
                this.updata_inline.Visible = true;
            }
            else
            {
                this.SN1_num.ReadOnly = true;
                this.SN2_num.ReadOnly = true;
                this.ProductDate.ReadOnly = true;
                this.VIP_num1.ReadOnly = true;
                this.VIP_num2.ReadOnly = true;
                this.VIP_digits.ReadOnly = true;
                this.VIP_prefix.ReadOnly = true;
                this.BAT_num1.ReadOnly = true;
                this.BAT_num2.ReadOnly = true;
                this.BAT_digits.ReadOnly = true;
                this.BAT_prefix.ReadOnly = true;
                this.SIM_num1.ReadOnly = true;
                this.SIM_num2.ReadOnly = true;
                this.SIM_digits.ReadOnly = true;
                this.SIM_prefix.ReadOnly = true;
                this.ICCID_digits.ReadOnly = true;
                this.ICCID_prefix.ReadOnly = true;
                this.MAC_digits.ReadOnly = true;
                this.MAC_prefix.ReadOnly = true;
                this.Equipment_digits.ReadOnly = true;
                this.Equipment_prefix.ReadOnly = true;
                this.updata_inline.Visible = false;
            }
            
            //根据第一次订单打印成功做记忆功能
            mprp = MPRPB.selectRecordParamByzhidanBLL(this.CB_ZhiDan.Text);
            foreach (ManuPrintRecordParam a in mprp)
            {
                if (mprp.Count != 0)
                {
                    this.choose_sim.Checked = Convert.ToBoolean(a.SimMark);
                    this.choose_vip.Checked = Convert.ToBoolean(a.VipMark);
                    this.choose_bat.Checked = Convert.ToBoolean(a.BatMark);
                    this.choose_iccid.Checked = Convert.ToBoolean(a.IccidMark);
                    this.choose_mac.Checked = Convert.ToBoolean(a.MacMark);
                    this.choose_Equipment.Checked = Convert.ToBoolean(a.EquipmentMark);
                    this.choose_RFID.Checked = Convert.ToBoolean(a.RfidMark);/**/
                    this.NoCheckCode.Checked = Convert.ToBoolean(a.NoCheckMark);
                    this.NoPaper.Checked = Convert.ToBoolean(a.NoPaperMark);
                    this.UpdataSimByImei.Checked = Convert.ToBoolean(a.UpdataSimMark);
                    this.UpdateIMEIBySim.Checked = Convert.ToBoolean(a.UpdateIMEIMark);
                    this.AutoTest.Checked = Convert.ToBoolean(a.AutoTestMark);
                    this.Couple.Checked = Convert.ToBoolean(a.CoupleMark);
                    this.WriteImei.Checked = Convert.ToBoolean(a.WriteImeiMark);
                    this.ParamDownload.Checked = Convert.ToBoolean(a.ParamDownloadMark);
                    this.GPS.Checked = Convert.ToBoolean(a.GPSMark);
                    this.Tempalte1Num.Text = a.TemPlate1Num.ToString();
                    TN1 = int.Parse(this.Tempalte1Num.Text);
                    this.Tempalte2Num.Text = a.TemPlate2Num.ToString();
                    TN2 = int.Parse(this.Tempalte2Num.Text);

                    this.CheckSN.Checked = Convert.ToBoolean(a.CHCheckSnMark);
                    this.CheckSIM.Checked = Convert.ToBoolean(a.CHCheckSimMark);
                    this.CheckBAT.Checked = Convert.ToBoolean(a.CHCheckBatMark);
                    this.CheckICCID.Checked = Convert.ToBoolean(a.CHCheckIccidMark);
                    this.CheckMAC.Checked = Convert.ToBoolean(a.CHCheckMacMark);
                    this.CheckEquipment.Checked = Convert.ToBoolean(a.CHCheckEquipmentMark);
                    this.CheckVIP.Checked = Convert.ToBoolean(a.CHCheckVipMark);
                    this.CheckRFID.Checked = Convert.ToBoolean(a.CHCheckRfidMark);

                }
            }

        }

        private void ClearAll()
        {
            this.reminder.Text = "";
            this.IMEI_Start.Clear();
            this.IMEI_Start.Focus();
            this.Re_IMEINum.Clear();
            this.ShowSN.Clear();
            this.SIMStart.Clear();
            this.VIPStart.Clear();
            this.BATStart.Clear();
            this.MACStart.Clear();
            this.ICCIDStart.Clear();
            this.EquipmentStart.Clear();
            this.choose_sim.Checked = false;
            this.choose_vip.Checked = false;
            this.choose_bat.Checked = false;
            this.choose_iccid.Checked = false;
            this.choose_mac.Checked = false;
            this.choose_Equipment.Checked = false;
            this.UpdataSimByImei.Checked = false;
            this.UpdateIMEIBySim.Checked = false;
            this.NoPaper.Checked = false;
            this.NoCheckCode.Checked = false;
            this.SIMStart.ReadOnly = true;
            this.VIPStart.ReadOnly = true;
            this.BATStart.ReadOnly = true;
            this.ICCIDStart.ReadOnly = true;
            this.MACStart.ReadOnly = true;
            this.EquipmentStart.ReadOnly = true;
            this.AutoTest.Checked = true;
            this.WriteImei.Checked = true;
            this.ParamDownload.Checked = true;
            this.Couple.Checked = true;
            this.Tempalte1Num.Text = 1.ToString();
            this.Tempalte2Num.Text = 1.ToString();
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

        private void PrintDate_Leave(object sender, EventArgs e)
        {
            if (this.PrintDate.Text != "")
            {
                 if (!IsDate(this.PrintDate.Text))
                {
                    MessageBox.Show("请输入正确的日期格式");
                    this.PrintDate.Text = System.DateTime.Now.ToString("yyyy.MM.dd");
                }
            }
        }

        public static bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        private void VIP_num1_Leave(object sender, EventArgs e)
        {
            if (this.VIP_num1.Text != "")
            {
                if (HasChinese(this.VIP_num1.Text))
                {
                    player.Play();
                    this.VIP_num1.Clear();
                }
            }
        }

        private void VIP_num2_Leave(object sender, EventArgs e)
        {
            if (this.VIP_num2.Text != "")
            {
                if (HasChinese(this.VIP_num2.Text))
                {
                    player.Play();
                    this.VIP_num2.Clear();
                }
            }
        }

        private void Debug_print_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Select_Template1.Text != "" || this.Select_Template2.Text != "")
                {
                    dj = this.Printer1.Text;
                    dj2 = this.Printer2.Text;
                    LabelFormatDocument btFormat = btEngine.Documents.Open(lj);

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
                    GetValue("Information", "电池号", out outString);
                    btFormat.SubStrings[outString].Value = this.BAT_num1.Text;
                    GetValue("Information", "备注", out outString);
                    btFormat.SubStrings[outString].Value = this.Remake.Text;
                    GetValue("Information", "生产日期", out outString);
                    btFormat.SubStrings[outString].Value = this.PrintDate.Text;
                    //打印份数,同序列打印的份数
                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                    //指定打印机名称
                    btFormat.PrintSetup.PrinterName = dj;
                    btFormat.Print();
                    if (lj2 != "")
                    {
                        xc = 1;
                        //调试打印双模板线程
                        Thread thread = new Thread(new ThreadStart(Thread1));
                        //启动线程
                        thread.Start();
                    }
                    Form1.Log("调试打印了机身贴IMEI号为" + this.IMEI_num1.Text + "的制单", null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex.Message);
            }
        }

        public void Thread1()
        {
            if (xc == 1)
            {
                this.reminder.AppendText("OK");
                btFormat = btEngine.Documents.Open(lj2);
                btFormat.PrintSetup.PrinterName = dj2;
                //对模板相应字段进行赋值
                GetValue("Information", "IMEI", out outString);
                btFormat.SubStrings[outString].Value = this.IMEI_num1.Text + getimei15(this.IMEI_num1.Text);
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
                GetValue("Information", "电池号", out outString);
                btFormat.SubStrings[outString].Value = this.BAT_num1.Text;
                GetValue("Information", "备注", out outString);
                btFormat.SubStrings[outString].Value = this.Remake.Text;
                GetValue("Information", "生产日期", out outString);
                btFormat.SubStrings[outString].Value = this.PrintDate.Text;
                //打印份数,同序列打印的份数
                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN2;
                btFormat.Print();
                xc = 0;
            }
        }

        private void choose_sim_Click(object sender, EventArgs e)
        {
            if (this.UpdataSimByImei.Checked ==false && this.UpdateIMEIBySim.Checked == false)
            {
                player.Play();
                this.reminder.AppendText("请先选择右侧更新关系\r\n");
                this.choose_sim.Checked = false;
                return;
            }
            if (this.choose_sim.Checked == true)
            {
                AssociatedFields[0] = "SIM";
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
                //this.choose_iccid.Enabled = false;
                if (this.choose_iccid.Checked == false)
                {
                    this.choose_iccid.Checked = true;
                    //this.ICCIDStart.ReadOnly = true;
                    //AssociatedFields[3] = "ICCID";
                }
            }
            else
            {
                AssociatedFields.Remove(0);
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
                this.UpdataSimByImei.Checked = false;
                this.UpdateIMEIBySim.Checked = false;
                //this.choose_iccid.Enabled = true;
                if (AssociatedFields.Count == 0) {
                    this.NoPaper.Checked = false;
                }
            }
        }

        private void choose_vip_Click(object sender, EventArgs e)
        {
            if (this.choose_vip.Checked == true)
            {
                AssociatedFields[1] = "VIP";
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
            }
            else
            {
                AssociatedFields.Remove(1);
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
                if (AssociatedFields.Count == 0)
                {
                    this.NoPaper.Checked = false;
                }
            }
        }

        private void choose_bat_Click(object sender, EventArgs e)
        {
            if (this.choose_bat.Checked == true)
            {
                AssociatedFields[2] = "BAT";
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
            }
            else
            {
                AssociatedFields.Remove(2);
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
                if (AssociatedFields.Count == 0)
                {
                    this.NoPaper.Checked = false;
                }
            }
        }

        private void choose_iccid_Click(object sender, EventArgs e)
        {
            if (this.choose_iccid.Checked == true)
            {
                AssociatedFields[3] = "ICCID";
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
                if(this.choose_sim.Checked == true)
                {
                    this.choose_sim.Checked = false;
                    AssociatedFields.Remove(0);
                }
                this.choose_sim.Enabled = false;

            }
            else
            {
                AssociatedFields.Remove(3);
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();

                this.choose_sim.Enabled = true;

                if (AssociatedFields.Count == 0)
                {
                    this.NoPaper.Checked = false;
                }
            }
        }

        private void choose_mac_Click(object sender, EventArgs e)
        {
            if (this.choose_mac.Checked == true)
            {
                AssociatedFields[4] = "MAC";
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
            }
            else
            {
                AssociatedFields.Remove(4);
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
                if (AssociatedFields.Count == 0)
                {
                    this.NoPaper.Checked = false;
                }
            }
        }

        private void choose_Equipment_Click(object sender, EventArgs e)
        {
            if (this.choose_Equipment.Checked == true)
            {
                AssociatedFields[5] = "Equipment";
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
            }
            else
            {
                AssociatedFields.Remove(5);
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
                if (AssociatedFields.Count == 0)
                {
                    this.NoPaper.Checked = false;
                }
            }
        }


        //重打复选框
        private void choose_reprint_Click(object sender, EventArgs e)
        {
            if (this.choose_reprint.Checked == true)
            {
                if (this.Select_Template1.Text == "" && this.Select_Template2.Text == "")
                {
                    player.Play();
                    this.reminder.AppendText("请先选择模板\r\n");
                    this.choose_reprint.Checked = false;
                    return;
                }
                //this.Re_IMEINum.ReadOnly = false;
                ////this.IMEI_Start.ReadOnly = true;
                //this.Re_IMEINum.Focus();
                this.Re_Tem1.Visible = true;
                this.Re_Tem2.Visible = true;
                if (this.Select_Template1.Text != "")
                {
                    this.Re_Tem1.Checked = true;
                }
                else
                {
                    this.Re_Tem1.Checked = false;
                }
                if (this.Select_Template2.Text != "")
                {
                    this.Re_Tem2.Checked = true;
                }
                else
                {
                    this.Re_Tem2.Checked = false;
                }
            }
            else
            {
                this.Re_IMEINum.ReadOnly = true;
                //this.IMEI_Start.ReadOnly = false;
                this.IMEI_Start.Focus();
                this.Re_Tem1.Visible = false;
                this.Re_Tem2.Visible = false;
            }
        }

        //根据SIM卡号更新IMEI复选框点击事件
        private void UpdateIMEIBySim_Click(object sender, EventArgs e)
        {
            if (this.UpdateIMEIBySim.Checked == true)
            {
                if (this.UpdataSimByImei.Checked == true)
                {
                    this.UpdataSimByImei.Checked = false;
                }
            }
        }

        //根据IMEI更新SIM卡号复选框点击事件
        private void UpdataSimByImei_Click(object sender, EventArgs e)
        {
            if (this.UpdataSimByImei.Checked == true)
            {
                if (this.UpdateIMEIBySim.Checked == true)
                {
                    this.UpdateIMEIBySim.Checked = false;
                }
            }
        }

        private void NoPaper_Click(object sender, EventArgs e)
        {
            if (this.NoPaper.Checked == true)
            {
                if (this.choose_sim.Checked ==false && this.choose_vip.Checked == false && this.choose_bat.Checked ==false && this.choose_iccid.Checked ==false && this.choose_mac.Checked==false && this.choose_Equipment.Checked==false) {
                    this.NoPaper.Checked = false;
                    player.Play();
                    this.reminder.AppendText("请先选择关联字段\r\n");
                }
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
            }
            else
            {
                this.IMEI_Start.Clear();
                this.SIMStart.Clear();
                this.VIPStart.Clear();
                this.BATStart.Clear();
                this.ICCIDStart.Clear();
                this.MACStart.Clear();
                this.EquipmentStart.Clear();
                this.IMEI_Start.Focus();
            }
            
        }

        //拼接Sql语句 查询字段
        public string SplicingCheckSQLStr(string FieldNumber)
        {
            string[] FindFieldstr = FindField.Split(',');
            string Sqlstr = "IMEI1 = '" + FieldNumber + "' OR ";
            string De = "= '";
            string Or = "' OR ";

            for(int i=0; i< FindFieldstr.Count()-1; i++)
            {
                Sqlstr += FindFieldstr[i] + De + FieldNumber + Or;
            }
            
            return Sqlstr = Sqlstr.Substring(0,Sqlstr.Length-3);
        }

        //重打
        private void Re_IMEINum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.Re_IMEINum.Text != "")
                {
                    //查询复选框不为空时
                    if(CheckFields.Count != 0)
                    {
                        LDrs.Clear();

                        //查询勾选字段，带出所有的字段显示UI
                        LDrs = DRSB.SelectAllFieldBLL(SplicingCheckSQLStr(this.Re_IMEINum.Text));
                        if(LDrs.Count != 0)
                        {
                            foreach (DataRelativeSheet a in LDrs)
                            {
                                this.SIMStart.Text = a.IMEI3;
                                this.VIPStart.Text = a.IMEI8;
                                this.BATStart.Text = a.IMEI9;
                                this.ICCIDStart.Text = a.IMEI4;
                                this.MACStart.Text = a.IMEI6;
                                this.RFIDStart.Text = a.RFID;
                                this.EquipmentStart.Text = a.IMEI7;
                                this.GLB_SN.Text = a.SN;
                                this.Re_IMEINum.Text = a.IMEI1;

                                int Chekcfalge = 0;
                                if(this.CheckSN.Checked == true)
                                {
                                    if(this.GLB_SN.Text == "")
                                    {
                                        this.reminder.AppendText("SN号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if(this.CheckSIM.Checked == true)
                                {
                                    if (this.SIMStart.Text == "")
                                    {
                                        this.reminder.AppendText("SIM号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if (this.CheckICCID.Checked == true)
                                {
                                    if (this.ICCIDStart.Text == "")
                                    {
                                        this.reminder.AppendText("ICCID号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if (this.CheckMAC.Checked == true)
                                {
                                    if (this.MACStart.Text == "")
                                    {
                                        this.reminder.AppendText("MAC号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if (this.CheckVIP.Checked == true)
                                {
                                    if (this.VIPStart.Text == "")
                                    {
                                        this.reminder.AppendText("VIP号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if (this.CheckBAT.Checked == true)
                                {
                                    if (this.BATStart.Text == "")
                                    {
                                        this.reminder.AppendText("BAT号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if (this.CheckRFID.Checked == true)
                                {
                                    if (this.RFIDStart.Text == "")
                                    {
                                        this.reminder.AppendText("RFID号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if (this.CheckEquipment.Checked == true)
                                {
                                    if (this.EquipmentStart.Text == "")
                                    {
                                        this.reminder.AppendText("Equipment号为空\r\n");
                                        Chekcfalge = 1;
                                    }
                                }
                                if(Chekcfalge == 1)
                                {
                                    player.Play();
                                    this.GLB_SN.Clear();
                                    this.SIMStart.Clear();
                                    this.VIPStart.Clear();
                                    this.BATStart.Clear();
                                    this.ICCIDStart.Clear();
                                    this.MACStart.Clear();
                                    this.RFIDStart.Clear();
                                    this.EquipmentStart.Clear();
                                    this.Re_IMEINum.Clear();
                                    this.Re_IMEINum.Focus();
                                    return;
                                }

                            }
                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText(this.Re_IMEINum.Text+"无关联\r\n");
                            this.GLB_SN.Clear();
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.RFIDStart.Clear();
                            this.EquipmentStart.Clear();
                            this.Re_IMEINum.Clear();
                            this.Re_IMEINum.Focus();
                            return;

                        }
                    }

                    //扫入关联表SN或IMEI2可带出IMEI
                    //Gl_IMEI = DRSB.SelectIMEIBySnOrIMEI2BLL(this.Re_IMEINum.Text);
                    //if (Gl_IMEI != "")
                    //{
                    //    this.Re_IMEINum.Text = Gl_IMEI;
                    //}
                    int IMEI_Len = this.IMEI_num1.Text.Length;
                    if (this.Re_IMEINum.Text.Length ==15 && IMEI_Len==14)
                    {
                        if (long.Parse(this.Re_IMEINum.Text.Substring(0, IMEI_Len)) < long.Parse(this.IMEI_num1.Text) || long.Parse(this.Re_IMEINum.Text.Substring(0, IMEI_Len)) > long.Parse(this.IMEI_num2.Text))
                        {
                            player.Play();
                            this.reminder.AppendText(this.Re_IMEINum.Text + "IMEI不在范围内\r\n");
                            this.Re_IMEINum.Clear();
                            this.Re_IMEINum.Focus();
                            return;
                        }
                    }
                    else
                    {
                        if (this.Re_IMEINum.Text.CompareTo(this.IMEI_num1.Text) == -1 || this.Re_IMEINum.Text.CompareTo(this.IMEI_num2.Text) == 1)
                        {
                            player.Play();
                            this.reminder.AppendText(this.Re_IMEINum.Text + "IMEI不在范围内\r\n");
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
                    if (this.Re_Tem1.Checked == true || this.Re_Tem2.Checked == true)
                    {
                        this.Re_IMEINum.Enabled = false;
                        if (PMB.CheckReCHOrJSIMEIBLL(this.Re_IMEINum.Text, 2))
                        {
                            string RE_PrintTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (this.Re_Tem1.Checked == true)
                            {
                                LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //对模板相应字段进行赋值
                                btFormat.SubStrings["ProductDate"].Value = this.PrintDate.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                PrintMessage a = PMB.SelectSnByIMEIBLL(this.Re_IMEINum.Text,0);
                                btFormat.SubStrings["SIM"].Value = a.SIM;
                                btFormat.SubStrings["VIP"].Value = a.VIP;
                                btFormat.SubStrings["BAT"].Value = a.BAT;
                                btFormat.SubStrings["ICCID"].Value = a.ICCID;
                                btFormat.SubStrings["MAC"].Value = a.MAC;
                                btFormat.SubStrings["Equipment"].Value = a.Equipment;
                                btFormat.SubStrings["SN"].Value = a.SN;
                                btFormat.SubStrings["RFID"].Value = a.RFID;
                                btFormat.SubStrings["IMEI"].Value = a.IMEI;
                                //if (a.RFID != "")
                                //    this.RFIDStart.Text = a.RFID;
                                //GLBSN = DRSB.SelectGLBSNByImeiBLL(this.Re_IMEINum.Text);
                                //btFormat.SubStrings["GLB_SN"].Value = GLBSN;
                                btFormat.Print();
                                //更新打印信息到数据表
                                if (PMB.UpdateRePrintBLL(this.Re_IMEINum.Text, RE_PrintTime, 2, this.Select_Template1.Text, this.Select_Template2.Text))
                                {
                                    Form1.Log("重打了IMEI号为" + this.Re_IMEINum.Text + "的制单", null);
                                }
                            }
                            if (this.Re_Tem2.Checked == true)
                            {
                                LabelFormatDocument btFormat = btEngine.Documents.Open(lj2);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer2.Text;
                                //对模板相应字段进行赋值
                                btFormat.SubStrings["ProductDate"].Value = this.PrintDate.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN2;
                                PrintMessage a = PMB.SelectSnByIMEIBLL(this.Re_IMEINum.Text, 0);
                                btFormat.SubStrings["SIM"].Value = a.SIM;
                                btFormat.SubStrings["VIP"].Value = a.VIP;
                                btFormat.SubStrings["BAT"].Value = a.BAT;
                                btFormat.SubStrings["ICCID"].Value = a.ICCID;
                                btFormat.SubStrings["MAC"].Value = a.MAC;
                                btFormat.SubStrings["Equipment"].Value = a.Equipment;
                                btFormat.SubStrings["SN"].Value = a.SN;
                                btFormat.SubStrings["RFID"].Value = a.RFID;
                                btFormat.SubStrings["IMEI"].Value = a.IMEI;
                                //GLBSN = DRSB.SelectGLBSNByImeiBLL(this.Re_IMEINum.Text);
                                //btFormat.SubStrings["GLB_SN"].Value = GLBSN;
                                btFormat.Print();

                                //更新打印信息到数据表
                                if (PMB.UpdateRePrintBLL(this.Re_IMEINum.Text, RE_PrintTime, 2, this.Select_Template1.Text, this.Select_Template2.Text))
                                {
                                    Form1.Log("重打了IMEI号为" + this.Re_IMEINum.Text + "的制单", null);
                                }
                            }

                        }
                        else
                        {
                            player.Play();
                            this.reminder.AppendText(this.Re_IMEINum.Text + "无记录\r\n");
                            this.Re_IMEINum.Clear();
                            this.Re_IMEINum.Focus();
                        }
                        this.Re_IMEINum.Enabled = true;
                        //this.RFIDStart.Clear();
                        this.Re_IMEINum.Clear();
                        this.Re_IMEINum.Focus();
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
                    if(this.Re_IMEINum.Enabled == false)
                        this.Re_IMEINum.Enabled = true;
                    MessageBox.Show("Exception:" + ex.Message);
                }
            }
        }

        //不关联任何字段，只输入IMEI进行打印
        private void IMEI_Start_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Gl_IMEI = DRSB.SelectIMEIBySnOrIMEI2BLL(this.IMEI_Start.Text);
                if (Gl_IMEI != "")
                {
                    this.IMEI_Start.Text = Gl_IMEI;
                }
                if (this.NoCheckCode.Checked == false)//打印校验码
                {
                    string imei14;
                    String imeiRes = "";
                    if (IsNumeric(this.IMEI_Start.Text) && this.IMEI_Start.Text.Length == 15)
                    {
                        imei14 = this.IMEI_Start.Text.Substring(0, 14);
                        long IMEI_Start = long.Parse(imei14);
                        if (IMEI_Start < long.Parse(this.IMEI_num1.Text) || IMEI_Start > long.Parse(this.IMEI_num2.Text))
                        {
                            player.Play();
                            this.reminder.AppendText(this.IMEI_Start.Text + "不在范围内\r\n");
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
                                this.reminder.AppendText(this.IMEI_Start.Text + "校验错误\r\n");
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                        }
                    }
                    else if (this.IMEI_Start.Text == "")
                    {
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
                    if (this.IMEI_Start.Text == "")
                    {
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (this.IMEI_Start.Text.Length != this.IMEI_num1.Text.Length)
                    {
                        player.Play();
                        this.reminder.AppendText("IMEI号位数与起始位数不一致\r\n");
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (this.IMEI_Start.Text.CompareTo(this.IMEI_num1.Text) == -1 || this.IMEI_Start.Text.CompareTo(this.IMEI_num2.Text) == 1)
                    {
                        player.Play();
                        this.reminder.AppendText("IMEI号不在范围\r\n");
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                }
                //查漏测，checklog = 1时触发查询
                if (checklog == 1)
                {
                    MissingIMEI = MissingSql.Replace("IMEIInput", this.IMEI_Start.Text);
                    string CheckResult = TRB.CheckOneBefStationBLL(MissingIMEI, this.IMEI_Start.Text);
                    if (CheckResult!="1")
                    {
                        player5.Play();
                        this.reminder.AppendText(this.IMEI_Start.Text + CheckResult+ "漏测\r\n");
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                }

                //根据IMEI号更新SIM号
                if (this.UpdataSimByImei.Checked == true)
                {
                    if (!DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("IMEI号-"+ this.IMEI_Start.Text + "不存在关联表\r\n");
                        this.IMEI_Start.Clear();
                        this.SIMStart.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                }

                
                //根据IMEI到关联表查询带出相应字段    
                drs = DRSB.SelectByIMEIBLL(this.IMEI_Start.Text); //关链表里有则带出所有字段
                if (drs.Count != 0)
                {
                    foreach (DataRelativeSheet a in drs)
                    {
                        this.GLB_SN.Text = a.SN;
                        this.SIMStart.Text = a.IMEI3;
                        this.ICCIDStart.Text = a.IMEI4;
                        //对带出的ICCID进行范围判断
                        if (this.ICCIDStart.Text != "" && this.ICCID_digits.Text != "" && this.ICCID_prefix.Text != "")
                        {
                            int iccid_width = this.ICCIDStart.Text.Length;
                            if (iccid_width != int.Parse(this.ICCID_digits.Text))
                            {
                                player.Play();
                                this.reminder.AppendText("ICCID不在范围内\r\n");
                                this.SIMStart.Clear();
                                this.ICCIDStart.Clear();
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                            else
                            {
                                int iccid_prefix_width = this.ICCID_prefix.Text.Length;
                                string iccid_prefix = this.ICCIDStart.Text.Substring(0, iccid_prefix_width);
                                if (iccid_prefix != this.ICCID_prefix.Text)
                                {
                                    player.Play();
                                    this.reminder.AppendText("ICCID不在范围内\r\n");
                                    this.SIMStart.Clear();
                                    this.ICCIDStart.Clear();
                                    this.IMEI_Start.Clear();
                                    this.IMEI_Start.Focus();
                                    return;
                                }
                            }
                        }
                        this.MACStart.Text = a.IMEI6;
                        this.EquipmentStart.Text = a.IMEI7;
                        this.VIPStart.Text = a.IMEI8;
                        this.BATStart.Text = a.IMEI9;
                        this.RFIDStart.Text = a.RFID;
                        if (a.IMEI2 != "")
                        {
                            ASS_sn = a.IMEI2;
                        }
                        else
                        {
                            ASS_sn = this.SN1_num.Text;
                            Sn_mark = 1;
                        }
                    }
                }
                else
                {
                    Sn_mark = 1;
                    ASS_sn = this.SN1_num.Text;
                }

                //选择查询关联表字段
                if (CheckFields.Count != 0)
                {
                    //根据IMEI到关联表查询带出相应字段    
                    //drs = DRSB.SelectByIMEIBLL(this.IMEI_Start.Text); //关链表里有则带出所有字段
                    //if (drs.Count != 0)
                    //{
                        int checkFalge = 0;

                        foreach (DataRelativeSheet a in drs)
                        {
                            if (CheckSN.Checked == true)
                            {
                                if (this.GLB_SN.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联 SN号为空\r\n");
                                    checkFalge = 1;
                                }
                            }


                            if (this.CheckSIM.Checked == true)
                            {
                                if (this.SIMStart.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联SIM号为空\r\n");
                                    checkFalge = 1;
                                }
                            }

                            if (this.CheckICCID.Checked == true)
                            {
                                if (this.ICCIDStart.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联ICCID号为空\r\n");
                                    checkFalge = 1;
                                }
                            }

                            if (this.CheckMAC.Checked == true)
                            {
                                if (this.MACStart.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联MAC号为空\r\n");
                                    checkFalge = 1;
                                }
                            }
                            if (this.CheckEquipment.Checked == true)
                            {
                                if (this.EquipmentStart.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联Equipment号为空\r\n");
                                    checkFalge = 1;
                                }
                            }
                            if (this.CheckVIP.Checked == true)
                            {
                                if (this.VIPStart.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联VIP号为空\r\n");
                                    checkFalge = 1;
                                }

                            }
                            if (this.CheckBAT.Checked == true)
                            {
                                if (this.BATStart.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联BAT号为空\r\n");
                                    checkFalge = 1;
                                }
                            }
                            if (this.CheckRFID.Checked == true)
                            {
                                if (this.RFIDStart.Text == "")
                                {
                                    this.reminder.AppendText(this.IMEI_Start.Text + "关联RFID号为空\r\n");
                                    checkFalge = 1;
                                }
                            }

                            if (checkFalge == 1)
                            {
                                player.Play();
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                        }
                    //}

                }

                //判断SortedDictionary里的长度为否为0，为0代表不关联字段，直接执行打印；不为0则执行光标跳转
                if (AssociatedFields.Count == 0)
                {
                    try
                    {
                        ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                        if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                        {
                            if (Sn_mark == 1)
                            {
                                if (ASS_sn.CompareTo(this.SN2_num.Text) == 1)
                                {
                                    this.reminder.AppendText("SN号超出范围\r\n");
                                    this.IMEI_Start.Clear();
                                    this.IMEI_Start.Focus();
                                    return;
                                }
                            }
                            btFormat = btEngine.Documents.Open(lj);
                            //指定打印机名称
                            btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                            //打印份数,同序列打印的份数
                            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                            //对模板相应字段进行赋值;
                            ValuesToTemplate(btFormat);
                            btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                            IMEI = this.IMEI_Start.Text;
                            btFormat.SubStrings["SN"].Value = ASS_sn;
                            DZSN = ASS_sn;
                            this.ShowSN.Text = ASS_sn;
                            btFormat.Print();
                            Form1.Log("主线程打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                            if (this.Select_Template2.Text != "")
                            {
                                xc2 = 1;
                            }
                            //记录关联数据信息到关联表
                            if (!DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                drs.Add(new DataRelativeSheet()
                                {
                                    IMEI1 = this.IMEI_Start.Text,
                                    IMEI2 = ASS_sn,
                                    IMEI3 = "",
                                    IMEI4 = "",
                                    IMEI5 = "",
                                    IMEI6 = "",
                                    IMEI7 = "",
                                    IMEI8 = "",
                                    IMEI9 = "",
                                    IMEI10 = "",
                                    IMEI11 = "",
                                    IMEI12 = "",
                                    ZhiDan = this.CB_ZhiDan.Text,
                                    TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")
                                });
                                DRSB.InsertRelativeSheetBLL(drs);
                            }
                            //记录打印信息日志
                            PList.Claer();
                            PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                            PList.IMEI = this.IMEI_Start.Text.Trim();
                            PList.IMEIStart = this.IMEI_num1.Text.Trim();
                            PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                            PList.SN = ASS_sn;
                            PList.IMEIRel = this.IMEIRel.Text.Trim();
                            PList.SIM = this.SIMStart.Text.Trim();
                            PList.VIP = this.VIPStart.Text.Trim();
                            PList.BAT = this.BATStart.Text.Trim();
                            PList.SoftModel = this.SoftModel.Text.Trim();
                            PList.Version = this.SoftwareVersion.Text.Trim();
                            PList.Remark = this.Remake.Text.Trim();
                            PList.JS_PrintTime = "";
                            PList.JS_TemplatePath = "";
                            PList.CH_PrintTime = ProductTime;
                            PList.CH_TemplatePath1 = this.Select_Template1.Text;
                            PList.CH_TemplatePath2 = this.Select_Template2.Text;
                            PList.ICCID = this.ICCIDStart.Text;
                            PList.MAC = this.MACStart.Text;
                            PList.Equipment = this.EquipmentStart.Text;
                            PList.RFID = this.RFIDStart.Text;
                            PList.CHUserName = this.UserShow.Text;
                            if (PMB.InsertPrintMessageBLL(PList))
                            {
                                if (SN1_num.Text != "" && Sn_mark == 1)
                                {
                                    sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                    sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                    sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                    string sn2_suffix = SN2_num.Text.Remove(0, (this.SN2_num.Text.Length) - s);
                                    if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                    {
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                        this.SN1_num.Text = sn1;
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.MACStart.Clear();
                                        this.EquipmentStart.Clear();
                                        this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                                else
                                {
                                    this.IMEI_Start.Clear();
                                    this.SIMStart.Clear();
                                    this.VIPStart.Clear();
                                    this.BATStart.Clear();
                                    this.ICCIDStart.Clear();
                                    this.MACStart.Clear();
                                    this.EquipmentStart.Clear();
                                    this.ShowSN.Clear();
                                    this.GLB_SN.Clear();
                                    this.RFIDStart.Clear();
                                    this.IMEI_Start.Focus();
                                }
                            }
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        else if (PMB.CheckCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                        {
                            btFormat = btEngine.Documents.Open(lj);
                            //指定打印机名称
                            btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                            //打印份数,同序列打印的份数
                            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                            //对模板相应字段进行赋值
                            ValuesToTemplate(btFormat);
                            btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                            IMEI = this.IMEI_Start.Text;
                            list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                            foreach (PrintMessage a in list)
                            {
                                if (a.SN != "")
                                {
                                    ASS_sn = a.SN;
                                    btFormat.SubStrings["SN"].Value = a.SN;
                                    DZSN = a.SN;
                                    this.ShowSN.Text = a.SN;
                                    //更新SN号
                                    DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                    MOPB.UpdateCHAssociatedBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, a.SN,this.CB_ZhiDan.Text,this.RFIDStart.Text, this.UserShow.Text);
                                }
                                else
                                {
                                    if (Sn_mark == 0)
                                    {
                                        btFormat.SubStrings["SN"].Value = ASS_sn;
                                        DZSN = ASS_sn;
                                        this.ShowSN.Text = ASS_sn;
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                        MOPB.UpdateCHAssociatedBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, ASS_sn, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                        DZSN = this.SN1_num.Text;
                                        if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                        {
                                            this.reminder.AppendText("SN号超出范围\r\n");
                                            this.IMEI_Start.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                        this.ShowSN.Text = this.SN1_num.Text;
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                        MOPB.UpdateCHAssociatedBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        if (this.SN1_num.Text != "")
                                        {
                                            sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                            sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                            sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                            MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                            this.SN1_num.Text = sn1;
                                        }
                                    }
                                }
                            }
                            btFormat.Print();
                            Form1.Log("主线程打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                            if (this.Select_Template2.Text != "")
                            {
                                xc2 = 1;
                            }
                            //记录关联数据信息到关联表
                            if (!DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                drs.Add(new DataRelativeSheet()
                                {
                                    IMEI1 = this.IMEI_Start.Text,
                                    IMEI2 = this.ShowSN.Text,
                                    IMEI3 = "",
                                    IMEI4 = "",
                                    IMEI5 = "",
                                    IMEI6 = "",
                                    IMEI7 = "",
                                    IMEI8 = "",
                                    IMEI9 = "",
                                    IMEI10 = "",
                                    IMEI11 = "",
                                    IMEI12 = "",
                                    ZhiDan = this.CB_ZhiDan.Text,
                                    TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")
                                });
                                DRSB.InsertRelativeSheetBLL(drs);
                            }
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.ShowSN.Clear();
                            this.GLB_SN.Clear();
                            this.RFIDStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
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
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception:" + ex.Message);
                    }
                }
                else
                {
                    slog = 1;
                    switch (AssociatedFields.Keys.First())
                    {
                        case 0:
                            {
                                if (this.NoPaper.Checked == false)
                                {
                                    if (PMB.CheckReCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                    else
                                    {
                                        this.SIMStart.Focus();
                                    }
                                }
                                else
                                {
                                    this.SIMStart.Focus();
                                }
                            }
                            break;
                        case 1:
                            {
                                if (this.NoPaper.Checked == false)
                                {
                                    if (PMB.CheckReCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                    else
                                    {
                                        this.VIPStart.Focus();
                                    }
                                }
                                else
                                {
                                    this.VIPStart.Focus();
                                }
                            }
                            break;
                        case 2:
                            {
                                if (this.NoPaper.Checked == false)
                                {
                                    if (PMB.CheckReCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                    else
                                    {
                                        this.BATStart.Focus();
                                    }
                                }
                                else
                                {
                                    this.BATStart.Focus();
                                }
                            }
                            break;
                        case 3:
                            {
                                if (this.NoPaper.Checked == false)
                                {
                                    if (PMB.CheckReCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                    else
                                    {
                                        this.ICCIDStart.Focus();
                                    }
                                }
                                else
                                {
                                    this.ICCIDStart.Focus();
                                }
                            }
                            break;
                        case 4:
                            {
                                if (this.NoPaper.Checked == false)
                                {
                                    if (PMB.CheckReCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                    else
                                    {
                                        this.MACStart.Focus();
                                    }
                                }
                                else
                                {
                                    this.MACStart.Focus();
                                }
                            }
                            break;
                        case 5:
                            {
                                if (this.NoPaper.Checked == false)
                                {
                                    if (PMB.CheckReCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                    else
                                    {
                                        this.EquipmentStart.Focus();
                                    }
                                }
                                else
                                {
                                    this.EquipmentStart.Focus();
                                }
                            }
                            break;
                        case 6:
                            {
                                if (this.NoPaper.Checked == false)
                                {
                                    if (PMB.CheckReCHOrJSIMEIBLL(this.IMEI_Start.Text, 2))
                                    {
                                        player.Play();
                                        this.reminder.AppendText(this.IMEI_Start.Text + "重号\r\n");
                                        this.IMEI_Start.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                    else
                                    {
                                        this.RFIDStart.Focus();
                                    }
                                }
                                else
                                {
                                    this.RFIDStart.Focus();
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void PrintTemplate2()
        {
            while (true)
            {
                if(Form1.QuitThreadFalge == 1)
                {
                    return;
                }

                if (xc2 == 1)
                {
                    btFormat = btEngine.Documents.Open(lj2);
                    //指定打印机名称
                    btFormat.PrintSetup.PrinterName = this.Printer2.Text;
                    //打印份数,同序列打印的份数
                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN2;
                    //对模板相应字段进行赋值;
                    //ValuesToTemplate(btFormat);
                    btFormat.SubStrings["SIM"].Value = SIM;
                    btFormat.SubStrings["VIP"].Value = VIP;
                    btFormat.SubStrings["BAT"].Value = BAT;
                    btFormat.SubStrings["ICCID"].Value = ICCID;
                    btFormat.SubStrings["MAC"].Value = MAC;
                    btFormat.SubStrings["Equipment"].Value = Equipment;
                    btFormat.SubStrings["IMEI"].Value = IMEI;
                    btFormat.SubStrings["SN"].Value = DZSN;
                    btFormat.SubStrings["GLB_SN"].Value = GLBSN;
                    btFormat.SubStrings["RFID"].Value = RFID;
                    btFormat.SubStrings["ProductDate"].Value = this.PrintDate.Text;
                    btFormat.Print();
                    Form1.Log("使用线程2打印了IMEI号:" + IMEI +",SN:"+ DZSN + ",SIM卡号:"+SIM+",电池号:"+BAT+",VIP号:"+VIP + ",蓝牙号:" + MAC + ",ICCID号:" + ICCID + ",设备号:" + Equipment + "的彩盒贴制单", null);
                    xc2 = 0;
                }
                Thread.Sleep(10);
            }
        }

        //给关联数据赋值，供打印时调用
        private void ValuesToTemplate(LabelFormatDocument btFormat)
        {
            btFormat.SubStrings["ProductDate"].Value = this.PrintDate.Text;
            btFormat.SubStrings["SIM"].Value = this.SIMStart.Text;
            SIM = this.SIMStart.Text;
            btFormat.SubStrings["VIP"].Value = this.VIPStart.Text;
            VIP = this.VIPStart.Text;
            btFormat.SubStrings["BAT"].Value = this.BATStart.Text;
            BAT = this.BATStart.Text;
            btFormat.SubStrings["ICCID"].Value = this.ICCIDStart.Text;
            ICCID = this.ICCIDStart.Text;
            btFormat.SubStrings["MAC"].Value = this.MACStart.Text;
            MAC = this.MACStart.Text;
            btFormat.SubStrings["Equipment"].Value = this.EquipmentStart.Text;      
            Equipment = this.EquipmentStart.Text;
            btFormat.SubStrings["GLB_SN"].Value = this.GLB_SN.Text;
            GLBSN = this.GLB_SN.Text;
            btFormat.SubStrings["RFID"].Value = this.RFIDStart.Text;
            RFID = this.RFIDStart.Text;
        }

        //扫描SIM卡后触发事件
        private void SIMStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            //是否按下Enter键，13是Enter键的值
            if (e.KeyChar == 13) {
                //扫入SIM号不为空
                if (this.SIMStart.Text != "")
                {
                    //SIM卡号位数不为空时
                    if (this.SIM_digits.Text != "")
                    {
                        int sim_width = this.SIMStart.Text.Length;
                        if (sim_width != int.Parse(this.SIM_digits.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("SIM号位数错误\r\n");
                            this.SIMStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else
                    {
                        player.Play();
                        this.reminder.AppendText("该制单未绑定SIM位数\r\n");
                        this.SIMStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (this.SIM_prefix.Text != "")
                    {
                        int sim_prefix_width = this.SIM_prefix.Text.Length;
                        string sim_prefix = this.SIMStart.Text.Substring(0, sim_prefix_width);
                        if (sim_prefix != this.SIM_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("SIM号前缀错误\r\n");
                            this.SIMStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }

                    //根据SIM号更新IMEI号
                    if (this.UpdateIMEIBySim.Checked == true)
                    {
                        //判断关联表是否有该SIM号，有的话根据该SIM号更新IMEI，无则插入一条记录
                        if (!DRSB.CheckSIMBLL(this.SIMStart.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("SIM号-"+this.SIMStart.Text + "不存在关联表\r\n");
                            this.SIMStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }

                    //检查SIM卡号是否重号，是的话直接清空返回  //打印表
                    if (PMB.CheckSIMBLL(this.SIMStart.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("SIM-" + this.SIMStart.Text + "重号\r\n");
                        this.SIMStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                   
                    //检查制单号是否与关联表的制单号一致
                    string GLBzhidan = DRSB.SelectZhidanBySimBLL(this.SIMStart.Text);
                    

                    //带出ICCID
                    if(this.choose_iccid.Checked == true)
                    {
                        //根据SIM卡号带出ICCID 有值带值，无值带空
                        this.ICCIDStart.Text = DRSB.SelectIccidBySimBLL(this.SIMStart.Text, G_zhidan);
                        if (this.ICCIDStart.Text == "-1")//用SIM号查出的制单号不与当前制单不一致 若一致则带出ICCID
                        {
                            player.Play();
                            this.reminder.AppendText("制单号与关联表不一致\r\n");
                            this.SIMStart.Clear();
                            this.ICCIDStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                        //对带出的ICCID进行范围判断
                        if (this.ICCIDStart.Text != "" && this.ICCID_digits.Text != "" && this.ICCID_prefix.Text != "")
                        {
                            int iccid_width = this.ICCIDStart.Text.Length;
                            if (iccid_width != int.Parse(this.ICCID_digits.Text))
                            {
                                player.Play();
                                this.reminder.AppendText("ICCID不在范围内\r\n");
                                this.SIMStart.Clear();
                                this.ICCIDStart.Clear();
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();
                                return;
                            }
                            else
                            {
                                int iccid_prefix_width = this.ICCID_prefix.Text.Length;
                                string iccid_prefix = this.ICCIDStart.Text.Substring(0, iccid_prefix_width);
                                if (iccid_prefix != this.ICCID_prefix.Text)
                                {
                                    player.Play();
                                    this.reminder.AppendText("ICCID不在范围内\r\n");
                                    this.SIMStart.Clear();
                                    this.ICCIDStart.Clear();
                                    this.IMEI_Start.Clear();
                                    this.IMEI_Start.Focus();
                                    return;
                                }
                            }
                        }
                        if (this.ICCIDStart.Text == "")
                        {
                            player.Play();
                            this.reminder.AppendText("此SIM卡号带出的ICCID号为空\r\n");
                            this.SIMStart.Clear();
                            this.ICCIDStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                        ICCID = this.ICCIDStart.Text;
                        //更新关联表SIM（连带ICCID）//根据IMEI号更新SIM号与ICCID
                        if (this.UpdataSimByImei.Checked == true)
                        {
                            DRSB.UpdateSIMByIMEIBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text);
                        }

                        //判断关联表是否有该SIM号，有的话根据该SIM号更新IMEI，无则插入一条记录
                        if (this.UpdateIMEIBySim.Checked == true)
                        {
                            DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                        }
                        //打印及跳转
                        if (AssociatedFields.Count == 1)
                        {
                            if (this.NoPaper.Checked == false)
                            {
                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                {
                                    //根据IMEI到关联表带出SN（关联表里的IMEI2）号
                                    ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                    if (ASS_sn == "")
                                    {
                                        ASS_sn = this.SN1_num.Text;
                                        Sn_mark = 1;
                                        if (ASS_sn.CompareTo(this.SN2_num.Text) == 1)
                                        {
                                            this.reminder.AppendText("SN号超出范围\r\n");
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Sn_mark = 0;
                                    }
                                    btFormat = btEngine.Documents.Open(lj);
                                    //指定打印机名称
                                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                    //打印份数,同序列打印的份数
                                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                    //对模板相应字段进行赋值
                                    ValuesToTemplate(btFormat);
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    IMEI = this.IMEI_Start.Text;
                                    btFormat.SubStrings["SN"].Value = ASS_sn;
                                    DZSN = ASS_sn;
                                    this.ShowSN.Text = ASS_sn;
                                    btFormat.Print();
                                    Form1.Log("关联SIM打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                    if (this.Select_Template2.Text != "")
                                    {
                                        xc2 = 1;
                                    }
                                    //更新IMEI通过SIM号
                                    if (this.UpdateIMEIBySim.Checked == true)
                                    {
                                        DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    }
                                    //记录打印信息日志
                                    PList.Claer();
                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                    PList.IMEI = this.IMEI_Start.Text.Trim();
                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                    PList.SN = ASS_sn;
                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                    PList.SIM = this.SIMStart.Text.Trim();
                                    PList.VIP = this.VIPStart.Text.Trim();
                                    PList.BAT = this.BATStart.Text.Trim();
                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                    PList.Remark = this.Remake.Text.Trim();
                                    PList.JS_PrintTime = "";
                                    PList.JS_TemplatePath = "";
                                    PList.CH_PrintTime = ProductTime;
                                    PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                    PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                    PList.ICCID = this.ICCIDStart.Text;
                                    PList.MAC = this.MACStart.Text;
                                    PList.Equipment = this.EquipmentStart.Text;
                                    PList.RFID = this.RFIDStart.Text;
                                    PList.CHUserName = this.UserShow.Text;
                                    //记录成功后更新订单配置表里的SN号
                                    if (PMB.InsertPrintMessageBLL(PList))
                                    {
                                        if (SN1_num.Text != "" && Sn_mark == 1)
                                        {
                                            sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                            sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                            sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                            if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                            {
                                                //更新SN号
                                                DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                                this.SN1_num.Text = sn1;
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.ICCIDStart.Clear();
                                                this.ShowSN.Clear();
                                                this.GLB_SN.Clear();
                                                this.RFIDStart.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.ICCIDStart.Clear();
                                            this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    //检查订单状态是否为未开始，是的话更改为进行中
                                    if (this.updata_inline.Visible == true)
                                    {
                                        MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);//更新订单状态
                                        statusChange();
                                    }
                                }
                                else
                                {
                                    ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                    if (ASS_sn == "")
                                    {
                                        ASS_sn = this.SN1_num.Text;
                                        Sn_mark = 1;
                                    }
                                    else
                                    {
                                        Sn_mark = 0;
                                    }
                                    btFormat = btEngine.Documents.Open(lj);
                                    //指定打印机名称
                                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                    //打印份数,同序列打印的份数
                                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                    //对模板相应字段进行赋值
                                    ValuesToTemplate(btFormat);
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    IMEI = this.IMEI_Start.Text;
                                    list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                    foreach (PrintMessage a in list)
                                    {
                                        if (a.SN != "")
                                        {
                                            btFormat.SubStrings["SN"].Value = a.SN;
                                            DZSN = a.SN;
                                            ASS_sn = a.SN;
                                            this.ShowSN.Text = a.SN;
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                            PMB.UpdateSN_SIM_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.ICCIDStart.Text, a.SN, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                        else
                                        {
                                            if (Sn_mark == 1)
                                            {
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                DZSN = this.SN1_num.Text;
                                                //判断递增SN号是否超出范围
                                                if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                                {
                                                    this.reminder.AppendText("SN号超出范围\r\n");
                                                    this.IMEI_Start.Clear();
                                                    this.SIMStart.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                                }
                                                this.ShowSN.Text = this.SN1_num.Text;
                                                DZSN = this.SN1_num.Text;
                                                //更新SN号
                                                DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                                PMB.UpdateSN_SIM_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                                if (this.SN1_num.Text != "")
                                                {
                                                    sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                    sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                    MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                    this.SN1_num.Text = sn1;
                                                }
                                            }
                                            else
                                            {
                                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                                this.ShowSN.Text = ASS_sn;
                                                //更新SN号
                                                DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                                PMB.UpdateSN_SIM_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.ICCIDStart.Text, ASS_sn, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            }
                                        }
                                    }
                                    btFormat.Print();
                                    Form1.Log("主线程关联SIM打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                    if (this.Select_Template2.Text != "")
                                    {
                                        xc2 = 1;
                                    }
                                    //判断关联表是否有该SIM号，有的话根据该SIM号更新IMEI，无则插入一条记录
                                    if (this.UpdateIMEIBySim.Checked == true)
                                    {
                                        DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    }
                                    this.IMEI_Start.Clear();
                                    this.SIMStart.Clear();
                                    this.ICCIDStart.Clear();
                                    this.ShowSN.Clear();
                                    this.GLB_SN.Clear();
                                    this.RFIDStart.Clear();
                                    this.IMEI_Start.Focus();
                                    if (this.updata_inline.Visible == true)
                                    {
                                        MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                        statusChange();
                                    }
                                }
                            }
                            else
                            {
                                UpInt1 = 0;
                                UpInt2 = 0;
                                //判断关联表是否有该SIM号，有的话根据该SIM号更新IMEI，无则插入一条记录
                                if (this.UpdateIMEIBySim.Checked == true)
                                {
                                    DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    UpInt1 = 1;
                                }
                                if (PMB.UpdateSimIccidBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text))
                                {
                                    UpInt2 = 2;
                                }

                                switch (UpInt1 + UpInt2)
                                {
                                    case 0:
                                        BindingCountF++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        Bind_DGV.Rows[0].Cells[4].Value = this.ICCIDStart.Text;
                                        player.Play();
                                        break;
                                    case 1:
                                        BindingCountS++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "更 新";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        //Bind_DGV.Rows[0].Cells[4].Value = this.ICCIDStart.Text;
                                        player4.Play();
                                        break;
                                    case 2:
                                        BindingCountS++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        Bind_DGV.Rows[0].Cells[4].Value = this.ICCIDStart.Text;
                                        player4.Play();
                                        break;
                                    case 3:
                                        BindingCountS++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        Bind_DGV.Rows[0].Cells[4].Value = this.ICCIDStart.Text;
                                        player4.Play();
                                        break;
                                }
                                this.SIMStart.Clear();
                                this.ICCIDStart.Clear();
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();

                            }
                        }
                        else
                        {
                            switch (SortDictio[slog])
                            {
                                case 1:
                                    this.VIPStart.Focus();
                                    break;
                                case 2:
                                    this.BATStart.Focus();
                                    break;
                                case 3:
                                    this.ICCIDStart.Focus();
                                    break;
                                case 4:
                                    this.MACStart.Focus();
                                    break;
                                case 5:
                                    this.EquipmentStart.Focus();
                                    break;
                                case 6:
                                    this.RFIDStart.Focus();
                                    break;
                            }
                            slog++;
                        }
                    }
                    else
                    {

                        if (AssociatedFields.Count == 1)
                        {
                            if (this.NoPaper.Checked == false)
                            {
                                ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                                {
                                    //根据IMEI到关联表带出SN（关联表里的IMEI2）号
                                    ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                    if (ASS_sn == "")
                                    {
                                        ASS_sn = this.SN1_num.Text;
                                        Sn_mark = 1;
                                        if (ASS_sn.CompareTo(this.SN2_num.Text) == 1)
                                        {
                                            this.reminder.AppendText("SN号超出范围\r\n");
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.IMEI_Start.Focus();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Sn_mark = 0;
                                    }
                                    btFormat = btEngine.Documents.Open(lj);
                                    //指定打印机名称
                                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                    //打印份数,同序列打印的份数
                                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                    //对模板相应字段进行赋值
                                    ValuesToTemplate(btFormat);
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    IMEI = this.IMEI_Start.Text;
                                    btFormat.SubStrings["SN"].Value = ASS_sn;
                                    DZSN = ASS_sn;
                                    this.ShowSN.Text = ASS_sn;
                                    btFormat.Print();
                                    Form1.Log("关联SIM打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                    if (this.Select_Template2.Text != "")
                                    {
                                        xc2 = 1;
                                    }
                                    //更新IMEI通过SIM号
                                    if (this.UpdateIMEIBySim.Checked == true)
                                    {
                                        DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    }
                                    //记录打印信息日志
                                    PList.Claer();
                                    PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                    PList.IMEI = this.IMEI_Start.Text.Trim();
                                    PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                    PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                    PList.SN = ASS_sn;
                                    PList.IMEIRel = this.IMEIRel.Text.Trim();
                                    PList.SIM = this.SIMStart.Text.Trim();
                                    PList.VIP = this.VIPStart.Text.Trim();
                                    PList.BAT = this.BATStart.Text.Trim();
                                    PList.SoftModel = this.SoftModel.Text.Trim();
                                    PList.Version = this.SoftwareVersion.Text.Trim();
                                    PList.Remark = this.Remake.Text.Trim();
                                    PList.JS_PrintTime = "";
                                    PList.JS_TemplatePath = "";
                                    PList.CH_PrintTime = ProductTime;
                                    PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                    PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                    PList.ICCID = this.ICCIDStart.Text;
                                    PList.MAC = this.MACStart.Text;
                                    PList.Equipment = this.EquipmentStart.Text;
                                    PList.RFID = this.RFIDStart.Text;
                                    PList.CHUserName = this.UserShow.Text;
                                    //记录成功后更新订单配置表里的SN号
                                    if (PMB.InsertPrintMessageBLL(PList))
                                    {
                                        if (SN1_num.Text != "" && Sn_mark == 1)
                                        {
                                            sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                            sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                            sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                            if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                            {
                                                //更新SN号
                                                DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                                this.SN1_num.Text = sn1;
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.ICCIDStart.Clear();
                                                this.ShowSN.Clear();
                                                this.GLB_SN.Clear();
                                                this.RFIDStart.Clear();
                                                this.IMEI_Start.Focus();
                                            }
                                        }
                                        else
                                        {
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.ICCIDStart.Clear();
                                            this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    //检查订单状态是否为未开始，是的话更改为进行中
                                    if (this.updata_inline.Visible == true)
                                    {
                                        MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);//更新订单状态
                                        statusChange();
                                    }
                                }
                                else
                                {
                                    ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                    if (ASS_sn == "")
                                    {
                                        ASS_sn = this.SN1_num.Text;
                                        Sn_mark = 1;
                                    }
                                    else
                                    {
                                        Sn_mark = 0;
                                    }
                                    btFormat = btEngine.Documents.Open(lj);
                                    //指定打印机名称
                                    btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                    //打印份数,同序列打印的份数
                                    btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                    //对模板相应字段进行赋值
                                    ValuesToTemplate(btFormat);
                                    btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                    IMEI = this.IMEI_Start.Text;
                                    list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                    foreach (PrintMessage a in list)
                                    {
                                        if (a.SN != "")
                                        {
                                            btFormat.SubStrings["SN"].Value = a.SN;
                                            DZSN = a.SN;
                                            ASS_sn = a.SN;
                                            this.ShowSN.Text = a.SN;
                                            PMB.UpdateSN_SIMBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, a.SN, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                        else
                                        {
                                            if (Sn_mark == 1)
                                            {
                                                btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                                DZSN = this.SN1_num.Text;
                                                //判断递增SN号是否超出范围
                                                if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                                {
                                                    this.reminder.AppendText("SN号超出范围\r\n");
                                                    this.IMEI_Start.Clear();
                                                    this.SIMStart.Clear();
                                                    this.IMEI_Start.Focus();
                                                    return;
                                                }
                                                this.ShowSN.Text = this.SN1_num.Text;
                                                DZSN = this.SN1_num.Text;
                                                //更新SN号
                                                DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                                PMB.UpdateSN_SIMBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                                if (this.SN1_num.Text != "")
                                                {
                                                    sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                    sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                    sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                    MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                    this.SN1_num.Text = sn1;
                                                }
                                            }
                                            else
                                            {
                                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                                this.ShowSN.Text = ASS_sn;
                                                PMB.UpdateSN_SIMBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, ASS_sn, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            }
                                        }
                                    }
                                    btFormat.Print();
                                    Form1.Log("主线程关联SIM打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                    if (this.Select_Template2.Text != "")
                                    {
                                        xc2 = 1;
                                    }
                                    //判断关联表是否有该SIM号，有的话根据该SIM号更新IMEI，无则插入一条记录
                                    if (this.UpdateIMEIBySim.Checked == true)
                                    {
                                        DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    }
                                    //更新关联表SIM
                                    if (this.UpdataSimByImei.Checked == true)
                                    {
                                        DRSB.UpdateSIMByIMEIBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                        //this.reminder.AppendText(this.SIMStart.Text +"更新"+ this.IMEI_Start.Text +"\r\n");
                                    }
                                    this.IMEI_Start.Clear();
                                    this.SIMStart.Clear();
                                    this.ICCIDStart.Clear();
                                    this.ShowSN.Clear();
                                    this.GLB_SN.Clear();
                                    this.RFIDStart.Clear();
                                    this.IMEI_Start.Focus();
                                    if (this.updata_inline.Visible == true)
                                    {
                                        MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                        statusChange();
                                    }
                                }
                            }
                            else
                            {
                                UpInt1 = 0;
                                UpInt2 = 0;
                                //判断关联表是否有该SIM号，有的话根据该SIM号更新IMEI，无则插入一条记录
                                if (this.UpdateIMEIBySim.Checked == true)
                                {
                                    DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    UpInt1 = 1;
                                }
                                //更新关联表SIM
                                if (this.UpdataSimByImei.Checked == true)
                                {
                                    DRSB.UpdateSIMByIMEIBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    UpInt1 = 1;
                                    //this.reminder.AppendText(this.SIMStart.Text +"更新"+ this.IMEI_Start.Text +"\r\n");
                                }
                                if (PMB.UpdateSimIccidBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text))
                                {
                                    UpInt2 = 2;
                                }

                                switch (UpInt1 + UpInt2)
                                {
                                    case 0:
                                        BindingCountF++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        player.Play();
                                        break;
                                    case 1:
                                        BindingCountS++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "更 新";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        player4.Play();
                                        break;
                                    case 2:
                                        BindingCountS++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        player4.Play();
                                        break;
                                    case 3:
                                        BindingCountS++;
                                        Bind_DGV.Rows.Insert(0);
                                        Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                        Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                        Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                        Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                        player4.Play();
                                        break;
                                }
                                this.SIMStart.Clear();
                                this.ICCIDStart.Clear();
                                this.IMEI_Start.Clear();
                                this.IMEI_Start.Focus();

                            }
                        }
                        else
                        {
                            switch (SortDictio[slog])
                            {
                                case 1:
                                    this.VIPStart.Focus();
                                    break;
                                case 2:
                                    this.BATStart.Focus();
                                    break;
                                case 3:
                                    this.ICCIDStart.Focus();
                                    break;
                                case 4:
                                    this.MACStart.Focus();
                                    break;
                                case 5:
                                    this.EquipmentStart.Focus();
                                    break;
                                case 6:
                                    this.RFIDStart.Focus();
                                    break;
                            }
                            slog++;
                        }
                    }
                    
                }
                else
                { this.SIMStart.Focus(); }
            }
        }

        private void VIPStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) {
                if (this.VIPStart.Text != "")
                {
                    //判断扫入VIP是否在范围内
                    if (this.VIP_digits.Text != "")
                    {
                        int vip_width = this.VIPStart.Text.Length;
                        if (vip_width != int.Parse(this.VIP_digits.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("VIP位数错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else {
                        player.Play();
                        this.reminder.AppendText("该制单未绑定VIP位数\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if(this.VIP_prefix.Text != "")
                    {
                        int vip_prefix_width = this.VIP_prefix.Text.Length;
                        string vip_prefix = this.VIPStart.Text.Substring(0, vip_prefix_width);
                        if (vip_prefix != this.VIP_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("VIP前缀错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    //检查VIP是否重号
                    if (PMB.CheckVIPBLL(this.VIPStart.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("VIP-"+this.VIPStart.Text+"重号\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    //打印及跳转
                    if (AssociatedFields.Count == 1)
                    {
                        //出贴纸
                        if (this.NoPaper.Checked == false)
                        {
                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                    if (ASS_sn.CompareTo(this.SN2_num.Text) == 1)
                                    {
                                        this.reminder.AppendText("SN号超出范围\r\n");
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.IMEI_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                DZSN = ASS_sn;
                                this.ShowSN.Text = ASS_sn;
                                btFormat.Print();
                                Form1.Log("关联VIP打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                //记录打印信息日志
                                PList.Claer();
                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                PList.SN = ASS_sn;
                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                PList.SIM = this.SIMStart.Text.Trim();
                                PList.VIP = this.VIPStart.Text.Trim();
                                PList.BAT = this.BATStart.Text.Trim();
                                PList.SoftModel = this.SoftModel.Text.Trim();
                                PList.Version = this.SoftwareVersion.Text.Trim();
                                PList.Remark = this.Remake.Text.Trim();
                                PList.JS_PrintTime = "";
                                PList.JS_TemplatePath = "";
                                PList.CH_PrintTime = ProductTime;
                                PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                PList.ICCID = this.ICCIDStart.Text;
                                PList.MAC = this.MACStart.Text;
                                PList.Equipment = this.EquipmentStart.Text;
                                PList.RFID = this.RFIDStart.Text;
                                PList.CHUserName = this.UserShow.Text;
                                if (PMB.InsertPrintMessageBLL(PList))
                                {
                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                    }
                                    else
                                    {
                                        //记录关联数据信息到关联表
                                        Drs.Claer();
                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                        Drs.IMEI2 = this.ShowSN.Text;
                                        Drs.IMEI3 = this.SIMStart.Text;
                                        Drs.IMEI4 = this.ICCIDStart.Text;
                                        Drs.IMEI5 = "";
                                        Drs.IMEI6 = "";
                                        Drs.IMEI7 = "";
                                        Drs.IMEI8 = this.VIPStart.Text;
                                        Drs.IMEI9 = "";
                                        Drs.IMEI10 = "";
                                        Drs.IMEI11 = "";
                                        Drs.IMEI12 = "";
                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        DRSB.InsertRelativeSheetBLL(Drs);
                                    }
                                    if (SN1_num.Text != "" && Sn_mark == 1)
                                    {
                                        sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                        sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                        if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                        {
                                            this.SN1_num.Text = sn1;
                                            this.IMEI_Start.Clear();
                                            this.VIPStart.Clear();
                                            //this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        this.IMEI_Start.Clear();
                                        this.VIPStart.Clear();
                                        //this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                            }
                            else
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                foreach (PrintMessage a in list)
                                {
                                    if (a.SN != "")
                                    {
                                        btFormat.SubStrings["SN"].Value = a.SN;
                                        DZSN = a.SN;
                                        this.ShowSN.Text = a.SN;
                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                        }
                                        else
                                        {
                                            //记录关联数据信息到关联表
                                            Drs.Claer();
                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                            Drs.IMEI2 = this.ShowSN.Text;
                                            Drs.IMEI3 = this.SIMStart.Text;
                                            Drs.IMEI4 = this.ICCIDStart.Text;
                                            Drs.IMEI5 = "";
                                            Drs.IMEI6 = "";
                                            Drs.IMEI7 = "";
                                            Drs.IMEI8 = this.VIPStart.Text;
                                            Drs.IMEI9 = "";
                                            Drs.IMEI10 = "";
                                            Drs.IMEI11 = "";
                                            Drs.IMEI12 = "";
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);
                                        }
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                        PMB.UpdateSN_VIPOrSIMOrICCIDOrRFIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, a.SN, this.SIMStart.Text, this.ICCIDStart.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        if (Sn_mark == 1)
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            DZSN = this.SN1_num.Text;
                                            if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                            {
                                                this.reminder.AppendText("SN号超出范围\r\n");
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.VIPStart.Clear();
                                                this.IMEI_Start.Focus();
                                                return;
                                            }
                                            this.ShowSN.Text = this.SN1_num.Text;
                                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                            }
                                            else
                                            {
                                                //记录关联数据信息到关联表
                                                Drs.Claer();
                                                Drs.IMEI1 = this.IMEI_Start.Text;
                                                Drs.IMEI2 = this.ShowSN.Text;
                                                Drs.IMEI3 = this.SIMStart.Text;
                                                Drs.IMEI4 = this.ICCIDStart.Text;
                                                Drs.IMEI5 = "";
                                                Drs.IMEI6 = "";
                                                Drs.IMEI7 = "";
                                                Drs.IMEI8 = this.VIPStart.Text;
                                                Drs.IMEI9 = "";
                                                Drs.IMEI10 = "";
                                                Drs.IMEI11 = "";
                                                Drs.IMEI12 = "";
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);
                                            }
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                            PMB.UpdateSN_VIPOrSIMOrICCIDOrRFIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, this.SN1_num.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            if (this.SN1_num.Text != "")
                                            {
                                                sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1;
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = ASS_sn;
                                            DZSN = ASS_sn;
                                            this.ShowSN.Text = ASS_sn;
                                            DRSB.UpdateVIPBLL(this.IMEI_Start.Text, this.VIPStart.Text);
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                            PMB.UpdateSN_VIPOrSIMOrICCIDOrRFIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, ASS_sn, this.SIMStart.Text, this.ICCIDStart.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                    }
                                }
                                btFormat.Print();
                                Form1.Log("关联VIP打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                this.IMEI_Start.Clear();
                                this.VIPStart.Clear();
                                //this.ShowSN.Clear();
                                this.GLB_SN.Clear();
                                this.RFIDStart.Clear();
                                this.IMEI_Start.Focus();
                            }
                            //检查订单状态是否为未开始，是的话更改为进行中
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        //不出贴纸
                        else
                        {
                            int UpInt = 0;
                            //查询关联表，有数据则UPDATA进去，没有则INSERT
                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                UpInt++;
                            }
                            else
                            {
                                //记录关联数据信息到关联表
                                Drs.Claer();
                                Drs.IMEI1 = this.IMEI_Start.Text;
                                Drs.IMEI2 = "";
                                Drs.IMEI3 = this.SIMStart.Text;
                                Drs.IMEI4 = this.ICCIDStart.Text;
                                Drs.IMEI5 = "";
                                Drs.IMEI6 = "";
                                Drs.IMEI7 = "";
                                Drs.IMEI8 = this.VIPStart.Text;
                                Drs.IMEI9 = "";
                                Drs.IMEI10 = "";
                                Drs.IMEI11 = "";
                                Drs.IMEI12 = "";
                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                DRSB.InsertRelativeSheetBLL(Drs);
                                UpInt++;
                            }
                            //打印表，打印过了才更新关联数据
                            if (PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                PMB.UpdateVIPBLL(this.IMEI_Start.Text, this.VIPStart.Text);
                                UpInt++;
                            }
                            if (UpInt == 2 || UpInt == 1)
                            {
                                BindingCountS++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                player4.Play();
                            }
                            else
                            {
                                BindingCountF++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                player.Play();
                            }
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.BATStart.Clear();
                            this.VIPStart.Clear();
                            this.MACStart.Clear();
                            this.ICCIDStart.Clear();
                            this.EquipmentStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                    }
                    else if(AssociatedFields.Count == 2 && SortDictio[0]==0)
                    {
                        if (this.NoPaper.Checked == false)
                        {
                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                    if (ASS_sn.CompareTo(this.SN2_num.Text) == 1)
                                    {
                                        this.reminder.AppendText("SN号超出范围\r\n");
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.IMEI_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                DZSN = ASS_sn;
                                this.ShowSN.Text = ASS_sn;
                                btFormat.Print();
                                Form1.Log("关联SIM && VIP打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                //记录打印信息日志
                                PList.Claer();
                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                PList.SN = ASS_sn;
                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                PList.SIM = this.SIMStart.Text.Trim();
                                PList.VIP = this.VIPStart.Text.Trim();
                                PList.BAT = this.BATStart.Text.Trim();
                                PList.SoftModel = this.SoftModel.Text.Trim();
                                PList.Version = this.SoftwareVersion.Text.Trim();
                                PList.Remark = this.Remake.Text.Trim();
                                PList.JS_PrintTime = "";
                                PList.JS_TemplatePath = "";
                                PList.CH_PrintTime = ProductTime;
                                PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                PList.ICCID = this.ICCIDStart.Text;
                                PList.MAC = this.MACStart.Text;
                                PList.Equipment = this.EquipmentStart.Text;
                                PList.RFID = this.RFIDStart.Text;
                                PList.CHUserName = this.UserShow.Text;
                                if (PMB.InsertPrintMessageBLL(PList))
                                {
                                    //判断关联表是否有该SIM号，有的话根据该SIM号更新IMEI，无则插入一条记录
                                    if (this.UpdateIMEIBySim.Checked == true)
                                    {
                                        DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                    }
                                    DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                    //else
                                    //{
                                    //    //记录关联数据信息到关联表
                                    //    drs.Add(new DataRelativeSheet()
                                    //    {
                                    //        IMEI1 = this.IMEI_Start.Text,
                                    //        IMEI2 = this.ShowSN.Text,
                                    //        IMEI3 = this.SIMStart.Text,
                                    //        IMEI4 = "",
                                    //        IMEI5 = "",
                                    //        IMEI6 = "",
                                    //        IMEI7 = "",
                                    //        IMEI8 = this.VIPStart.Text,
                                    //        IMEI9 = "",
                                    //        IMEI10 = "",
                                    //        IMEI11 = "",
                                    //        IMEI12 = "",
                                    //        ZhiDan = this.CB_ZhiDan.Text,
                                    //        TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")
                                    //    });
                                    //    DRSB.InsertRelativeSheetBLL(drs);
                                    //}
                                    if (SN1_num.Text != "" && Sn_mark == 1)
                                    {
                                        sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                        sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                        if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                        {
                                            this.SN1_num.Text = sn1;
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.VIPStart.Clear();
                                            this.ICCIDStart.Clear();
                                            //this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.ICCIDStart.Clear();
                                        //this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                            }
                            else
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                foreach (PrintMessage a in list)
                                {
                                    if (a.SN != "")
                                    {
                                        btFormat.SubStrings["SN"].Value = a.SN;
                                        DZSN = a.SN;
                                        this.ShowSN.Text = a.SN;
                                        //扫入SIM号的情况下一定会更新某一个
                                        if (this.UpdateIMEIBySim.Checked == true)
                                        {
                                            DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                        }
                                        DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                        PMB.UpdateSN_VIPOrSIMOrICCIDOrRFIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, a.SN, this.SIMStart.Text, this.ICCIDStart.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        if (Sn_mark == 1)
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            DZSN = this.SN1_num.Text;
                                            if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                            {
                                                this.reminder.AppendText("SN号超出范围\r\n");
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.VIPStart.Clear();
                                                this.IMEI_Start.Focus();
                                                return;
                                            }
                                            this.ShowSN.Text = this.SN1_num.Text;
                                            //扫入SIM号的情况下一定会更新某一个
                                            if (this.UpdateIMEIBySim.Checked == true)
                                            {
                                                DRSB.UpdateIMEIBySIMBLL(this.IMEI_Start.Text, this.SIMStart.Text);
                                            }
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                            DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                            PMB.UpdateSN_VIPOrSIMOrICCIDOrRFIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, this.SN1_num.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            if (this.SN1_num.Text != "")
                                            {
                                                sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1;
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = ASS_sn;
                                            DZSN = ASS_sn;
                                            this.ShowSN.Text = ASS_sn;
                                            DRSB.UpdateVIP_SIM_OR_ICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text);
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                            PMB.UpdateSN_VIPOrSIMOrICCIDOrRFIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, ASS_sn, this.SIMStart.Text, this.ICCIDStart.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                    }
                                }
                                btFormat.Print();
                                Form1.Log("关联SIM && VIP打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                this.IMEI_Start.Clear();
                                this.SIMStart.Clear();
                                this.VIPStart.Clear();
                                this.ICCIDStart.Clear();
                                //this.ShowSN.Clear();
                                this.GLB_SN.Clear();
                                this.RFIDStart.Clear();
                                this.IMEI_Start.Focus();
                            }
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        else
                        {
                            int UpInt = 0;
                            //查询关联表，有数据则UPDATA进去，没有则INSERT
                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                DRSB.UpdateVIPBLL(this.IMEI_Start.Text, this.VIPStart.Text);
                                UpInt++;
                            }
                            else
                            {
                                //记录关联数据信息到关联表
                                Drs.Claer();
                                Drs.IMEI1 = this.IMEI_Start.Text;
                                Drs.IMEI2 = "";
                                Drs.IMEI3 = this.SIMStart.Text;
                                Drs.IMEI4 = this.ICCIDStart.Text;
                                Drs.IMEI5 = "";
                                Drs.IMEI6 = "";
                                Drs.IMEI7 = "";
                                Drs.IMEI8 = this.VIPStart.Text;
                                Drs.IMEI9 = "";
                                Drs.IMEI10 = "";
                                Drs.IMEI11 = "";
                                Drs.IMEI12 = "";
                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                DRSB.InsertRelativeSheetBLL(Drs);
                                UpInt++;
                            }
                            //打印表，打印过了才更新关联数据
                            if (PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                PMB.UpdateSimVipIccidBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.ICCIDStart.Text);
                                UpInt++;
                            }
                            if (UpInt == 2 || UpInt == 1)
                            {
                                BindingCountS++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                player4.Play();
                            }
                            else
                            {
                                BindingCountF++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                player.Play();
                            }
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.BATStart.Clear();
                            this.VIPStart.Clear();
                            this.MACStart.Clear();
                            this.ICCIDStart.Clear();
                            this.EquipmentStart.Clear();
                            this.RFIDStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                    }
                    else
                    {
                        switch (SortDictio[slog])
                        {
                            case 2:
                                this.BATStart.Focus();
                                break;
                            case 3:
                                this.ICCIDStart.Focus();
                                break;
                            case 4:
                                this.MACStart.Focus();
                                break;
                            case 5:
                                this.EquipmentStart.Focus();
                                break;
                            case 6:
                                this.RFIDStart.Focus();
                                break;
                        }
                        slog++;
                    }
                }
                else  {
                    this.VIPStart.Focus();
                }
            }
        }

        private void BATStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.BATStart.Text != "")
                {
                    //判断BAT是否在范围内
                    if (this.BAT_digits.Text != "")
                    {
                        int bat_width = this.BATStart.Text.Length;
                        if (bat_width != int.Parse(this.BAT_digits.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("BAT位数错误\r\n");
                            this.BATStart.Clear();
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else
                    {
                        player.Play();
                        this.reminder.AppendText("此制单未绑定BAT位数\r\n");
                        this.BATStart.Clear();
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if(this.BAT_prefix.Text != "")
                    {
                        int bat_prefix_width = this.BAT_prefix.Text.Length;
                        string bat_prefix = this.BATStart.Text.Substring(0, bat_prefix_width);
                        if (bat_prefix != this.BAT_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("BAT前缀错误\r\n");
                            this.BATStart.Clear();
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    //检查BAT是否重号
                    if (PMB.CheckBATBLL(this.BATStart.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("BAT-"+this.BATStart.Text+"重号\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    //打印及跳转
                    if (AssociatedFields.Last().Value == "BAT")
                    {
                        if (this.NoPaper.Checked == false)
                        {
                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                    if (ASS_sn.CompareTo(this.SN2_num.Text) == 1)
                                    {
                                        this.reminder.AppendText("SN号超出范围\r\n");
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.IMEI_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                DZSN = ASS_sn;
                                this.ShowSN.Text = ASS_sn;
                                btFormat.Print();
                                Form1.Log("关联BAT为" + this.BATStart.Text + "&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                //记录打印信息日志
                                PList.Claer();
                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                PList.SN = ASS_sn;
                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                PList.SIM = this.SIMStart.Text.Trim();
                                PList.VIP = this.VIPStart.Text.Trim();
                                PList.BAT = this.BATStart.Text.Trim();
                                PList.SoftModel = this.SoftModel.Text.Trim();
                                PList.Version = this.SoftwareVersion.Text.Trim();
                                PList.Remark = this.Remake.Text.Trim();
                                PList.JS_PrintTime = "";
                                PList.JS_TemplatePath = "";
                                PList.CH_PrintTime = ProductTime;
                                PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                PList.ICCID = this.ICCIDStart.Text;
                                PList.MAC = this.MACStart.Text;
                                PList.Equipment = this.EquipmentStart.Text;
                                PList.RFID = this.RFIDStart.Text;
                                PList.CHUserName = this.UserShow.Text;
                                if (PMB.InsertPrintMessageBLL(PList))
                                {
                                    
                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        DRSB.UpdateVipAndBatOrSIMOrOICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text, this.BATStart.Text);
                                    }
                                    else
                                    {
                                        //记录关联数据信息到关联表
                                        Drs.Claer();
                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                        Drs.IMEI2 = this.ShowSN.Text;
                                        Drs.IMEI3 = this.SIMStart.Text;
                                        Drs.IMEI4 = this.ICCIDStart.Text;
                                        Drs.IMEI5 = "";
                                        Drs.IMEI6 = "";
                                        Drs.IMEI7 = "";
                                        Drs.IMEI8 = this.VIPStart.Text;
                                        Drs.IMEI9 = this.BATStart.Text;
                                        Drs.IMEI10 = "";
                                        Drs.IMEI11 = "";
                                        Drs.IMEI12 = "";
                                        Drs.RFID = "";
                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        DRSB.InsertRelativeSheetBLL(Drs);
                                    }
                                    
                                    if (SN1_num.Text != "" && Sn_mark == 1)
                                    {
                                        sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                        sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                        if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                        {
                                            this.SN1_num.Text = sn1;
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.VIPStart.Clear();
                                            this.BATStart.Clear();
                                            //this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        //this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                            }
                            else
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                //根据IMEI判断SN有没有打印过了
                                list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                foreach (PrintMessage a in list)
                                {
                                    if (a.SN != "")
                                    {
                                        btFormat.SubStrings["SN"].Value = a.SN;
                                        DZSN = a.SN;
                                        this.ShowSN.Text = a.SN;
                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            DRSB.UpdateVipAndBatOrSIMOrOICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text, this.BATStart.Text);
                                        }
                                        else
                                        {
                                            //记录关联数据信息到关联表
                                            Drs.Claer();
                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                            Drs.IMEI2 = this.ShowSN.Text;
                                            Drs.IMEI3 = this.SIMStart.Text;
                                            Drs.IMEI4 = this.ICCIDStart.Text;
                                            Drs.IMEI5 = "";
                                            Drs.IMEI6 = "";
                                            Drs.IMEI7 = "";
                                            Drs.IMEI8 = this.VIPStart.Text;
                                            Drs.IMEI9 = this.BATStart.Text;
                                            Drs.IMEI10 = "";
                                            Drs.IMEI11 = "";
                                            Drs.IMEI12 = "";
                                            Drs.RFID = "";
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);
                                        }
                                        
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                        PMB.UpdateSN_SIM_VIP_BAT_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, a.SN,this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        if (Sn_mark == 1)
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            DZSN = this.SN1_num.Text;
                                            //判断SN是否超出范围
                                            if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                            {
                                                this.reminder.AppendText("SN号超出范围\r\n");
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.VIPStart.Clear();
                                                this.BATStart.Clear();
                                                this.IMEI_Start.Focus();
                                                return;
                                            }
                                            this.ShowSN.Text = this.SN1_num.Text;
                                            
                                            //查关联表IMEI，存在的话则更新数据进去，不存在则插入一条数据
                                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                DRSB.UpdateVipAndBatOrSIMOrOICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text, this.BATStart.Text);
                                            }
                                            else
                                            {
                                                //记录关联数据信息到关联表
                                                Drs.Claer();
                                                Drs.IMEI1 = this.IMEI_Start.Text;
                                                Drs.IMEI2 = this.ShowSN.Text;
                                                Drs.IMEI3 = this.SIMStart.Text;
                                                Drs.IMEI4 = this.ICCIDStart.Text;
                                                Drs.IMEI5 = "";
                                                Drs.IMEI6 = "";
                                                Drs.IMEI7 = "";
                                                Drs.IMEI8 = this.VIPStart.Text;
                                                Drs.IMEI9 = this.BATStart.Text;
                                                Drs.IMEI10 = "";
                                                Drs.IMEI11 = "";
                                                Drs.IMEI12 = "";
                                                Drs.RFID = "";
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);
                                            }
                                            
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            if (this.SN1_num.Text != "")
                                            {
                                                sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1;
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = ASS_sn;
                                            DZSN = ASS_sn;
                                            this.ShowSN.Text = ASS_sn;
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                            DRSB.UpdateVipAndBatOrSIMOrOICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text, this.BATStart.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, ASS_sn, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                    }
                                }
                                btFormat.Print();
                                Form1.Log("关联BAT&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                this.IMEI_Start.Clear();
                                this.SIMStart.Clear();
                                this.VIPStart.Clear();
                                this.BATStart.Clear();
                                this.ICCIDStart.Clear();
                                //this.ShowSN.Clear();
                                this.GLB_SN.Clear();
                                this.RFIDStart.Clear();
                                this.IMEI_Start.Focus();
                            }
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        else
                        {
                            int UpInt = 0;
                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                DRSB.UpdateVipAndBatOrSIMOrOICCIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.ICCIDStart.Text, this.VIPStart.Text, this.BATStart.Text);
                                UpInt++;
                            }
                            else
                            {
                                //记录关联数据信息到关联表
                                Drs.Claer();
                                Drs.IMEI1 = this.IMEI_Start.Text;
                                Drs.IMEI2 = "";
                                Drs.IMEI3 = this.SIMStart.Text;
                                Drs.IMEI4 = this.ICCIDStart.Text;
                                Drs.IMEI5 = "";
                                Drs.IMEI6 = "";
                                Drs.IMEI7 = "";
                                Drs.IMEI8 = this.VIPStart.Text;
                                Drs.IMEI9 = this.BATStart.Text;
                                Drs.IMEI10 = "";
                                Drs.IMEI11 = "";
                                Drs.IMEI12 = "";
                                Drs.RFID = "";
                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                DRSB.InsertRelativeSheetBLL(Drs);
                                UpInt++;
                            }
                            if (PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                PMB.UpdateVipAndBatBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text);
                                UpInt++;
                            }
                            if (UpInt == 2 || UpInt == 1)
                            {
                                BindingCountS++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                player4.Play();
                            }
                            else
                            {
                                BindingCountF++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                player.Play();
                            }
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.BATStart.Clear();
                            this.VIPStart.Clear();
                            this.MACStart.Clear();
                            this.ICCIDStart.Clear();
                            this.EquipmentStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                    }
                    else
                    {
                        switch (SortDictio[slog])
                        {
                            case 3:
                                this.ICCIDStart.Focus();
                                break;
                            case 4:
                                this.MACStart.Focus();
                                break;
                            case 5:
                                this.EquipmentStart.Focus();
                                break;
                            case 6:
                                this.RFIDStart.Focus();
                                break;
                        }
                        slog++;
                    }
                }
                else{this.BATStart.Focus();}
            }
        }

        private void ICCIDStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.ICCIDStart.Text != "")
                {
                    //判断ICCI是否在范围内
                    if (this.ICCID_digits.Text != "")
                    {
                        int iccid_width = this.ICCIDStart.Text.Length;
                        if (iccid_width != int.Parse(this.ICCID_digits.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("ICCID位数错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else
                    {
                        player.Play();
                        this.reminder.AppendText("该制单未绑定ICCID位数\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if(this.ICCID_prefix.Text != "")
                    {
                        int iccid_prefix_width = this.ICCID_prefix.Text.Length;
                        string iccid_prefix = this.ICCIDStart.Text.Substring(0, iccid_prefix_width);
                        if (iccid_prefix != this.ICCID_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("ICCID前缀错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    //检查ICCID是否重号
                    if (PMB.CheckICCIDBLL(this.ICCIDStart.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("ICCID-"+ this.ICCIDStart.Text+"重号\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (AssociatedFields.Last().Value == "ICCID")
                    {
                        if (this.NoPaper.Checked == false)
                        {
                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                    if (ASS_sn.CompareTo(this.SN2_num.Text) == 1)
                                    {
                                        this.reminder.AppendText("SN号超出范围\r\n");
                                        this.IMEI_Start.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                DZSN = ASS_sn;
                                this.ShowSN.Text = ASS_sn;
                                btFormat.Print();
                                Form1.Log("关联ICCID&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                //记录打印信息日志
                                PList.Claer();
                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                PList.SN = this.SN1_num.Text;
                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                PList.SIM = this.SIMStart.Text.Trim();
                                PList.VIP = this.VIPStart.Text.Trim();
                                PList.BAT = this.BATStart.Text.Trim();
                                PList.SoftModel = this.SoftModel.Text.Trim();
                                PList.Version = this.SoftwareVersion.Text.Trim();
                                PList.Remark = this.Remake.Text.Trim();
                                PList.JS_PrintTime = "";
                                PList.JS_TemplatePath = "";
                                PList.CH_PrintTime = ProductTime;
                                PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                PList.ICCID = this.ICCIDStart.Text;
                                PList.MAC = this.MACStart.Text;
                                PList.Equipment = this.EquipmentStart.Text;
                                PList.RFID = this.RFIDStart.Text;
                                PList.CHUserName = this.UserShow.Text;
                                if (PMB.InsertPrintMessageBLL(PList))
                                {
                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        DRSB.UpdateIccid_OrVipOrBatBLL(this.IMEI_Start.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text);
                                    }
                                    else
                                    {
                                        //记录关联数据信息到关联表
                                        Drs.Claer();
                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                        Drs.IMEI2 = this.ShowSN.Text;
                                        Drs.IMEI3 = "";
                                        Drs.IMEI4 = this.ICCIDStart.Text;
                                        Drs.IMEI5 = "";
                                        Drs.IMEI6 = "";
                                        Drs.IMEI7 = "";
                                        Drs.IMEI8 = this.VIPStart.Text;
                                        Drs.IMEI9 = this.BATStart.Text;
                                        Drs.IMEI10 = "";
                                        Drs.IMEI11 = "";
                                        Drs.IMEI12 = "";
                                        Drs.RFID = "";
                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        DRSB.InsertRelativeSheetBLL(Drs);
                                    }

                                    if (SN1_num.Text != "" && Sn_mark == 1)
                                    {
                                        sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                        sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');

                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                        if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                        {
                                            this.SN1_num.Text = sn1;
                                            this.IMEI_Start.Clear();
                                            this.VIPStart.Clear();
                                            this.BATStart.Clear();
                                            this.ICCIDStart.Clear();
                                            //this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        this.IMEI_Start.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        //this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                            }
                            else
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                //根据IMEI判断SN有没有打印过了
                                list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                foreach (PrintMessage a in list)
                                {
                                    if (a.SN != "")
                                    {
                                        btFormat.SubStrings["SN"].Value = a.SN;
                                        DZSN = a.SN;
                                        this.ShowSN.Text = a.SN;
                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            DRSB.UpdateIccid_OrVipOrBatBLL(this.IMEI_Start.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text);
                                        }
                                        else
                                        {
                                            //记录关联数据信息到关联表
                                            Drs.Claer();
                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                            Drs.IMEI2 = this.ShowSN.Text;
                                            Drs.IMEI3 = "";
                                            Drs.IMEI4 = this.ICCIDStart.Text;
                                            Drs.IMEI5 = "";
                                            Drs.IMEI6 = "";
                                            Drs.IMEI7 = "";
                                            Drs.IMEI8 = this.VIPStart.Text;
                                            Drs.IMEI9 = this.BATStart.Text;
                                            Drs.IMEI10 = "";
                                            Drs.IMEI11 = "";
                                            Drs.IMEI12 = "";
                                            Drs.RFID = "";
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);
                                        }

                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                        PMB.UpdateSN_VIP_BAT_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, a.SN,this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        if (Sn_mark == 1)
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            DZSN = this.SN1_num.Text;
                                            //判断SN是否超出范围
                                            if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                            {
                                                this.reminder.AppendText("SN号超出范围\r\n");
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.VIPStart.Clear();
                                                this.BATStart.Clear();
                                                this.IMEI_Start.Focus();
                                                return;
                                            }
                                            this.ShowSN.Text = this.SN1_num.Text;
                                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                DRSB.UpdateIccid_OrVipOrBatBLL(this.IMEI_Start.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text);
                                            }
                                            else
                                            {
                                                //记录关联数据信息到关联表
                                                Drs.Claer();
                                                Drs.IMEI1 = this.IMEI_Start.Text;
                                                Drs.IMEI2 = this.ShowSN.Text;
                                                Drs.IMEI3 = "";
                                                Drs.IMEI4 = this.ICCIDStart.Text;
                                                Drs.IMEI5 = "";
                                                Drs.IMEI6 = "";
                                                Drs.IMEI7 = "";
                                                Drs.IMEI8 = this.VIPStart.Text;
                                                Drs.IMEI9 = this.BATStart.Text;
                                                Drs.IMEI10 = "";
                                                Drs.IMEI11 = "";
                                                Drs.IMEI12 = "";
                                                Drs.RFID = "";
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);
                                            }
                                           
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                            //更新打印记录表
                                            PMB.UpdateSN_VIP_BAT_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            if (this.SN1_num.Text != "")
                                            {
                                                sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1;
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = ASS_sn;
                                            DZSN = ASS_sn;
                                            this.ShowSN.Text = ASS_sn;
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                            PMB.UpdateSN_VIP_BAT_ICCIDBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                    }
                                }
                                btFormat.Print();
                                Form1.Log("关联ICCID&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                this.IMEI_Start.Clear();
                                this.VIPStart.Clear();
                                this.BATStart.Clear();
                                this.ICCIDStart.Clear();
                                //this.ShowSN.Clear();
                                this.GLB_SN.Clear();
                                this.RFIDStart.Clear();
                                this.IMEI_Start.Focus();
                            }
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        else
                        {
                            int UpInt = 0;
                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                DRSB.UpdateIccid_OrVipOrBatBLL(this.IMEI_Start.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text);
                                UpInt++;
                            }
                            else
                            {
                                //记录关联数据信息到关联表
                                Drs.Claer();
                                Drs.IMEI1 = this.IMEI_Start.Text;
                                Drs.IMEI2 = "";
                                Drs.IMEI3 = this.SIMStart.Text;
                                Drs.IMEI4 = this.ICCIDStart.Text;
                                Drs.IMEI5 = "";
                                Drs.IMEI6 = "";
                                Drs.IMEI7 = "";
                                Drs.IMEI8 = this.VIPStart.Text;
                                Drs.IMEI9 = this.BATStart.Text;
                                Drs.IMEI10 = "";
                                Drs.IMEI11 = "";
                                Drs.IMEI12 = "";
                                Drs.RFID = "";
                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                DRSB.InsertRelativeSheetBLL(Drs);
                                UpInt++;
                            }
                            if (PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                PMB.UpdateVipAndBatBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text);
                                UpInt++;
                            }
                            if (UpInt == 2 || UpInt == 1)
                            {
                                BindingCountS++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                player4.Play();
                            }
                            else
                            {
                                BindingCountF++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                player.Play();
                            }
                            
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.BATStart.Clear();
                            this.VIPStart.Clear();
                            this.MACStart.Clear();
                            this.ICCIDStart.Clear();
                            this.EquipmentStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                    }
                    else
                    {
                        switch (SortDictio[slog])
                        {
                            case 4:
                                this.MACStart.Focus();
                                break;
                            case 5:
                                this.EquipmentStart.Focus();
                                break;
                            case 6:
                                this.RFIDStart.Focus();
                                break;
                        }
                        slog++;
                    }
                }
                else {
                    this.ICCIDStart.Focus();
                }
            }
        }

        private void MACStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.MACStart.Text != "")
                {
                    if (this.MAC_digits.Text != "")
                    {
                        int mac_width = this.MACStart.Text.Length;
                        if (mac_width != int.Parse(this.MAC_digits.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("MAC位数错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else
                    {
                        player.Play();
                        this.reminder.AppendText("该制单未绑定MAC位数\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.MACStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if(this.MAC_prefix.Text != "")
                    {
                        int mac_prefix_width = this.MAC_prefix.Text.Length;
                        string mac_prefix = this.MACStart.Text.Substring(0, mac_prefix_width);
                        if (mac_prefix != this.MAC_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("MAC前缀错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    if (PMB.CheckMACBLL(this.MACStart.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("MAC"+this.MACStart.Text+"重号\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.MACStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (AssociatedFields.Last().Value == "MAC")
                    {
                        if (this.NoPaper.Checked == false)
                        {
                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                    //判断SN是否超出范围
                                    if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                    {
                                        this.reminder.AppendText("SN号超出范围\r\n");
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.MACStart.Clear();
                                        this.IMEI_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                DZSN = ASS_sn;
                                this.ShowSN.Text = ASS_sn;
                                btFormat.Print();
                                Form1.Log("关联MAC&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                //记录打印信息日志
                                PList.Claer();
                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                PList.SN = ASS_sn;
                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                PList.SIM = this.SIMStart.Text.Trim();
                                PList.VIP = this.VIPStart.Text.Trim();
                                PList.BAT = this.BATStart.Text.Trim();
                                PList.SoftModel = this.SoftModel.Text.Trim();
                                PList.Version = this.SoftwareVersion.Text.Trim();
                                PList.Remark = this.Remake.Text.Trim();
                                PList.JS_PrintTime = "";
                                PList.JS_TemplatePath = "";
                                PList.CH_PrintTime = ProductTime;
                                PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                PList.ICCID = this.ICCIDStart.Text;
                                PList.MAC = this.MACStart.Text;
                                PList.Equipment = this.EquipmentStart.Text;
                                PList.RFID = this.RFIDStart.Text;
                                PList.CHUserName = this.UserShow.Text;
                                if (PMB.InsertPrintMessageBLL(PList))
                                {
                                     if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                     {
                                        DRSB.UpdateSimOrVipAndBatOrIccidAndMacBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text);
                                     }
                                    else
                                    {
                                        //记录关联数据信息到关联表
                                        Drs.Claer();
                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                        Drs.IMEI2 = this.ShowSN.Text;
                                        Drs.IMEI3 = this.SIMStart.Text;
                                        Drs.IMEI4 = this.ICCIDStart.Text;
                                        Drs.IMEI5 = "";
                                        Drs.IMEI6 = this.MACStart.Text;
                                        Drs.IMEI7 = "";
                                        Drs.IMEI8 = this.VIPStart.Text;
                                        Drs.IMEI9 = this.BATStart.Text;
                                        Drs.IMEI10 = "";
                                        Drs.IMEI11 = "";
                                        Drs.IMEI12 = "";
                                        Drs.RFID = this.RFIDStart.Text;
                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        DRSB.InsertRelativeSheetBLL(Drs);
                                    }
                                   
                                    if (SN1_num.Text != "" && Sn_mark == 1)
                                    {
                                        sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                        sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                        if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                        {
                                            this.SN1_num.Text = sn1;
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.VIPStart.Clear();
                                            this.BATStart.Clear();
                                            this.ICCIDStart.Clear();
                                            this.MACStart.Clear();
                                            //this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.MACStart.Clear();
                                        //this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                            }
                            else
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                foreach (PrintMessage a in list)
                                {
                                    if (a.SN != "")
                                    {
                                        btFormat.SubStrings["SN"].Value = a.SN;
                                        DZSN = a.SN;
                                        this.ShowSN.Text = a.SN;

                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            DRSB.UpdateSimOrVipAndBatOrIccidAndMacBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text);
                                        }
                                        else
                                        {
                                            //记录关联数据信息到关联表
                                            Drs.Claer();
                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                            Drs.IMEI2 = this.ShowSN.Text;
                                            Drs.IMEI3 = this.SIMStart.Text;
                                            Drs.IMEI4 = this.ICCIDStart.Text;
                                            Drs.IMEI5 = "";
                                            Drs.IMEI6 = this.MACStart.Text;
                                            Drs.IMEI7 = "";
                                            Drs.IMEI8 = this.VIPStart.Text;
                                            Drs.IMEI9 = this.BATStart.Text;
                                            Drs.IMEI10 = "";
                                            Drs.IMEI11 = "";
                                            Drs.IMEI12 = "";
                                            Drs.RFID = this.RFIDStart.Text;
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);
                                        }
                             
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                        PMB.UpdateSN_SIM_VIP_BAT_ICCID_MACBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, a.SN,this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        if (Sn_mark == 1)
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            DZSN = this.SN1_num.Text;
                                            //判断SN是否超出范围
                                            if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                            {
                                                this.reminder.AppendText("SN号超出范围\r\n");
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.VIPStart.Clear();
                                                this.BATStart.Clear();
                                                this.ICCIDStart.Clear();
                                                this.MACStart.Clear();
                                                this.IMEI_Start.Focus();
                                                return;
                                            }
                                            this.ShowSN.Text = this.SN1_num.Text;

                                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                DRSB.UpdateSimOrVipAndBatOrIccidAndMacBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text);
                                            }
                                            else
                                            {
                                                //记录关联数据信息到关联表
                                                Drs.Claer();
                                                Drs.IMEI1 = this.IMEI_Start.Text;
                                                Drs.IMEI2 = this.ShowSN.Text;
                                                Drs.IMEI3 = this.SIMStart.Text;
                                                Drs.IMEI4 = this.ICCIDStart.Text;
                                                Drs.IMEI5 = "";
                                                Drs.IMEI6 = this.MACStart.Text;
                                                Drs.IMEI7 = "";
                                                Drs.IMEI8 = this.VIPStart.Text;
                                                Drs.IMEI9 = this.BATStart.Text;
                                                Drs.IMEI10 = "";
                                                Drs.IMEI11 = "";
                                                Drs.IMEI12 = "";
                                                Drs.RFID = this.RFIDStart.Text;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);
                                            }
                                    
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCID_MACBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            if (this.SN1_num.Text != "")
                                            {
                                                sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1;
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = ASS_sn;
                                            DZSN = ASS_sn;
                                            this.ShowSN.Text = ASS_sn;
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                            DRSB.UpdateSimOrVipAndBatOrIccidAndMacBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCID_MACBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, ASS_sn, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                    }
                                }
                                btFormat.Print();
                                Form1.Log("关联MAC&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                this.IMEI_Start.Clear();
                                this.SIMStart.Clear();
                                this.VIPStart.Clear();
                                this.BATStart.Clear();
                                this.ICCIDStart.Clear();
                                this.MACStart.Clear();
                                //this.ShowSN.Clear();
                                this.GLB_SN.Clear();
                                this.RFIDStart.Clear();
                                this.IMEI_Start.Focus();
                            }
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        else
                        {
                            int UpInt = 0;
                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                DRSB.UpdateSimOrVipAndBatOrIccidAndMacBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text);
                                UpInt++;
                            }
                            else
                            {
                                //记录关联数据信息到关联表
                                Drs.Claer();
                                Drs.IMEI1 = this.IMEI_Start.Text;
                                Drs.IMEI2 = "";
                                Drs.IMEI3 = this.SIMStart.Text;
                                Drs.IMEI4 = this.ICCIDStart.Text;
                                Drs.IMEI5 = "";
                                Drs.IMEI6 = this.MACStart.Text;
                                Drs.IMEI7 = "";
                                Drs.IMEI8 = this.VIPStart.Text;
                                Drs.IMEI9 = this.BATStart.Text;
                                Drs.IMEI10 = "";
                                Drs.IMEI11 = "";
                                Drs.IMEI12 = "";
                                Drs.RFID = this.RFIDStart.Text;
                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                DRSB.InsertRelativeSheetBLL(Drs);
                                UpInt++;
                            }
                            if (PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                PMB.UpdateVipAndBatAndMacBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text);
                                UpInt++;
                            }
                            if (UpInt == 2 || UpInt == 1)
                            {
                                BindingCountS++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                Bind_DGV.Rows[0].Cells[7].Value = this.MACStart.Text;
                                player4.Play();
                            }
                            else
                            {
                                BindingCountF++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                Bind_DGV.Rows[0].Cells[7].Value = this.MACStart.Text;
                                player.Play();
                            }
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.BATStart.Clear();
                            this.VIPStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                    }
                    else
                    {
                        switch (SortDictio[slog])
                        {
                            case 5:
                                this.EquipmentStart.Focus();
                                break;
                            case 6:
                                this.RFIDStart.Focus();
                                break;
                        }
                        slog++;
                    }
                }
                else {this.MACStart.Focus();}
            }
        }

        private void EquipmentStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.EquipmentStart.Text != "")
                {
                    if (this.Equipment_digits.Text != "")
                    {
                        int Equipment_width = this.EquipmentStart.Text.Length;
                        if (Equipment_width != int.Parse(this.Equipment_digits.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("设备号位数错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else
                    {
                        player.Play();
                        this.reminder.AppendText("该制单未绑定位数\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.MACStart.Clear();
                        this.EquipmentStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if(this.Equipment_prefix.Text != "")
                    {
                        int Equipment_prefix_width = this.Equipment_prefix.Text.Length;
                        string Equipment_prefix = this.EquipmentStart.Text.Substring(0, Equipment_prefix_width);
                        if (Equipment_prefix != this.Equipment_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("设备号前缀错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    if (PMB.CheckEquipmentBLL(this.EquipmentStart.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("设备号-"+this.EquipmentStart.Text+"重号\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.MACStart.Clear();
                        this.EquipmentStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (AssociatedFields.Last().Value == "Equipment")
                    {
                        if (this.NoPaper.Checked == false)
                        {
                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                    //判断SN是否超出范围
                                    if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                    {
                                        this.reminder.AppendText("SN号超出范围\r\n");
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.MACStart.Clear();
                                        this.EquipmentStart.Clear();
                                        this.IMEI_Start.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                DZSN = ASS_sn;
                                this.ShowSN.Text = ASS_sn;
                                btFormat.Print();
                                Form1.Log("关联设备号&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                //记录打印信息日志
                                PList.Claer();
                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                PList.SN = ASS_sn;
                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                PList.SIM = this.SIMStart.Text.Trim();
                                PList.VIP = this.VIPStart.Text.Trim();
                                PList.BAT = this.BATStart.Text.Trim();
                                PList.SoftModel = this.SoftModel.Text.Trim();
                                PList.Version = this.SoftwareVersion.Text.Trim();
                                PList.Remark = this.Remake.Text.Trim();
                                PList.JS_PrintTime = "";
                                PList.JS_TemplatePath = "";
                                PList.CH_PrintTime = ProductTime;
                                PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                PList.ICCID = this.ICCIDStart.Text;
                                PList.MAC = this.MACStart.Text;
                                PList.Equipment = this.EquipmentStart.Text;
                                PList.RFID = this.RFIDStart.Text;
                                PList.CHUserName = this.UserShow.Text;
                                if (PMB.InsertPrintMessageBLL(PList))
                                {
                              
                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        DRSB.UpdateSIMOrVipAndBatOrICCIDAndMacAndEquBLL(this.IMEI_Start.Text, this.SIMStart.Text,this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text,this.MACStart.Text, this.EquipmentStart.Text);
                                    }
                                    else
                                    {
                                        //记录关联数据信息到关联表
                                        Drs.Claer();
                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                        Drs.IMEI2 = this.ShowSN.Text;
                                        Drs.IMEI3 = this.SIMStart.Text;
                                        Drs.IMEI4 = this.ICCIDStart.Text;
                                        Drs.IMEI5 = "";
                                        Drs.IMEI6 = this.MACStart.Text;
                                        Drs.IMEI7 = this.EquipmentStart.Text;
                                        Drs.IMEI8 = this.VIPStart.Text;
                                        Drs.IMEI9 = this.BATStart.Text;
                                        Drs.IMEI10 = "";
                                        Drs.IMEI11 = "";
                                        Drs.IMEI12 = "";
                                        Drs.RFID = "";
                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        DRSB.InsertRelativeSheetBLL(Drs);
                                    }
                                    
                                    if (SN1_num.Text != "" && Sn_mark == 1)
                                    {
                                        sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                        sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                        if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                        {
                                            this.SN1_num.Text = sn1;
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.VIPStart.Clear();
                                            this.BATStart.Clear();
                                            this.ICCIDStart.Clear();
                                            this.MACStart.Clear();
                                            this.EquipmentStart.Clear();
                                            //this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.MACStart.Clear();
                                        this.EquipmentStart.Clear();
                                        //this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                            }
                            else
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                foreach (PrintMessage a in list)
                                {
                                    if (a.SN != "")
                                    {
                                        btFormat.SubStrings["SN"].Value = a.SN;
                                        DZSN = a.SN;
                                        this.ShowSN.Text = a.SN;
                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            DRSB.UpdateSIMOrVipAndBatOrICCIDAndMacAndEquBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text);
                                        }
                                        else
                                        {
                                            //记录关联数据信息到关联表
                                            Drs.Claer();
                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                            Drs.IMEI2 = this.ShowSN.Text;
                                            Drs.IMEI3 = this.SIMStart.Text;
                                            Drs.IMEI4 = this.ICCIDStart.Text;
                                            Drs.IMEI5 = "";
                                            Drs.IMEI6 = this.MACStart.Text;
                                            Drs.IMEI7 = this.EquipmentStart.Text;
                                            Drs.IMEI8 = this.VIPStart.Text;
                                            Drs.IMEI9 = this.BATStart.Text;
                                            Drs.IMEI10 = "";
                                            Drs.IMEI11 = "";
                                            Drs.IMEI12 = "";
                                            Drs.RFID = "";
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);
                                        }
                                    
                                        PMB.UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, a.SN,this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        if (Sn_mark == 1)
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            DZSN = this.SN1_num.Text;
                                            //判断SN是否超出范围
                                            if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                            {
                                                this.reminder.AppendText("SN号超出范围\r\n");
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.VIPStart.Clear();
                                                this.BATStart.Clear();
                                                this.ICCIDStart.Clear();
                                                this.MACStart.Clear();
                                                this.EquipmentStart.Clear();
                                                this.IMEI_Start.Focus();
                                                return;
                                            }
                                            this.ShowSN.Text = this.SN1_num.Text;
                                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                DRSB.UpdateSIMOrVipAndBatOrICCIDAndMacAndEquBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text);
                                            }
                                            else
                                            {
                                                //记录关联数据信息到关联表
                                                Drs.Claer();
                                                Drs.IMEI1 = this.IMEI_Start.Text;
                                                Drs.IMEI2 = this.ShowSN.Text;
                                                Drs.IMEI3 = this.SIMStart.Text;
                                                Drs.IMEI4 = this.ICCIDStart.Text;
                                                Drs.IMEI5 = "";
                                                Drs.IMEI6 = this.MACStart.Text;
                                                Drs.IMEI7 = this.EquipmentStart.Text;
                                                Drs.IMEI8 = this.VIPStart.Text;
                                                Drs.IMEI9 = this.BATStart.Text;
                                                Drs.IMEI10 = "";
                                                Drs.IMEI11 = "";
                                                Drs.IMEI12 = "";
                                                Drs.RFID = "";
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);
                                            }
                                            
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            if (this.SN1_num.Text != "")
                                            {
                                                sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1;
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = ASS_sn;
                                            DZSN = ASS_sn;
                                            this.ShowSN.Text = ASS_sn;
                                            DRSB.UpdateVipAndBatAndMacAndEquBLL(this.IMEI_Start.Text, this.VIPStart.Text, this.BATStart.Text, this.MACStart.Text, this.EquipmentStart.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, ASS_sn, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                    }
                                }
                                btFormat.Print();
                                Form1.Log("关联设备号&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                this.IMEI_Start.Clear();
                                this.SIMStart.Clear();
                                this.VIPStart.Clear();
                                this.BATStart.Clear();
                                this.ICCIDStart.Clear();
                                this.MACStart.Clear();
                                this.EquipmentStart.Clear();
                                //this.ShowSN.Clear();
                                this.GLB_SN.Clear();
                                this.RFIDStart.Clear();
                                this.IMEI_Start.Focus();
                            }
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        else
                        {
                            int UpInt = 0;
                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                DRSB.UpdateSIMOrVipAndBatOrICCIDAndMacAndEquBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text);
                                UpInt++;
                            }
                            else
                            {
                                //记录关联数据信息到关联表
                                Drs.Claer();
                                Drs.IMEI1 = this.IMEI_Start.Text;
                                Drs.IMEI2 = "";
                                Drs.IMEI3 = this.SIMStart.Text;
                                Drs.IMEI4 = this.ICCIDStart.Text;
                                Drs.IMEI5 = "";
                                Drs.IMEI6 = this.MACStart.Text;
                                Drs.IMEI7 = this.EquipmentStart.Text;
                                Drs.IMEI8 = this.VIPStart.Text;
                                Drs.IMEI9 = this.BATStart.Text;
                                Drs.IMEI10 = "";
                                Drs.IMEI11 = "";
                                Drs.IMEI12 = "";
                                Drs.RFID = "";
                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                DRSB.InsertRelativeSheetBLL(Drs);
                                UpInt++;
                            }
                            if (PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                PMB.UpdateVipAndBatAndMacAndEquBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text);
                                UpInt++;
                            }
                            if (UpInt == 2 || UpInt == 1)
                            {
                                BindingCountS++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                Bind_DGV.Rows[0].Cells[7].Value = this.MACStart.Text;
                                Bind_DGV.Rows[0].Cells[8].Value = this.EquipmentStart.Text;
                                player4.Play();
                            }
                            else
                            {
                                BindingCountF++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                Bind_DGV.Rows[0].Cells[7].Value = this.MACStart.Text;
                                Bind_DGV.Rows[0].Cells[8].Value = this.EquipmentStart.Text;
                                player.Play();
                            }
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.BATStart.Clear();
                            this.VIPStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.RFIDStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                    }
                    else
                    {
                        switch (SortDictio[slog])
                        {
                            case 6:
                                this.RFIDStart.Focus();
                                break;
                        }
                        slog++;
                    }
                }
                else {this.EquipmentStart.Focus();}
            }
        }

        private void RFIDStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (this.RFIDStart.Text != "")
                {
                    if (this.RFID_digits.Text != "")
                    {
                        int RFID_width = this.RFIDStart.Text.Length;
                        if (RFID_width != int.Parse(this.RFID_digits.Text))
                        {
                            player.Play();
                            this.reminder.AppendText("RFID号位数错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.RFIDStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    else
                    {
                        player.Play();
                        this.reminder.AppendText("该制单未绑定位数\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.MACStart.Clear();
                        this.EquipmentStart.Clear();
                        this.RFIDStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (this.RFID_prefix.Text != "")
                    {
                        int RFID_prefix_width = this.RFID_prefix.Text.Length;
                        string RFID_prefix = this.RFIDStart.Text.Substring(0, RFID_prefix_width);
                        if (RFID_prefix != this.RFID_prefix.Text)
                        {
                            player.Play();
                            this.reminder.AppendText("RFID号前缀错误\r\n");
                            this.SIMStart.Clear();
                            this.VIPStart.Clear();
                            this.BATStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.RFIDStart.Clear();
                            this.IMEI_Start.Clear();
                            this.IMEI_Start.Focus();
                            return;
                        }
                    }
                    if (PMB.CheckRFIDBLL(this.RFIDStart.Text))
                    {
                        player.Play();
                        this.reminder.AppendText("RFID号-" + this.RFIDStart.Text + "重号\r\n");
                        this.SIMStart.Clear();
                        this.VIPStart.Clear();
                        this.BATStart.Clear();
                        this.ICCIDStart.Clear();
                        this.MACStart.Clear();
                        this.EquipmentStart.Clear();
                        this.RFIDStart.Clear();
                        this.IMEI_Start.Clear();
                        this.IMEI_Start.Focus();
                        return;
                    }
                    if (AssociatedFields.Last().Value == "RFID")
                    {
                        if (this.NoPaper.Checked == false)
                        {
                            ProductTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                            if (!PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                    //判断SN是否超出范围
                                    if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                    {
                                        this.reminder.AppendText("SN号超出范围\r\n");
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.MACStart.Clear();
                                        this.EquipmentStart.Clear();
                                        this.IMEI_Start.Focus();
                                        return;
                                    }

                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                btFormat.SubStrings["SN"].Value = ASS_sn;
                                DZSN = ASS_sn;
                                this.ShowSN.Text = ASS_sn;
                                btFormat.Print();
                                Form1.Log("关联设备号&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                //记录打印信息日志
                                PList.Claer();
                                PList.Zhidan = this.CB_ZhiDan.Text.Trim();
                                PList.IMEI = this.IMEI_Start.Text.Trim();
                                PList.IMEIStart = this.IMEI_num1.Text.Trim();
                                PList.IMEIEnd = this.IMEI_num2.Text.Trim();
                                PList.SN = ASS_sn;
                                PList.IMEIRel = this.IMEIRel.Text.Trim();
                                PList.SIM = this.SIMStart.Text.Trim();
                                PList.VIP = this.VIPStart.Text.Trim();
                                PList.BAT = this.BATStart.Text.Trim();
                                PList.SoftModel = this.SoftModel.Text.Trim();
                                PList.Version = this.SoftwareVersion.Text.Trim();
                                PList.Remark = this.Remake.Text.Trim();
                                PList.JS_PrintTime = "";
                                PList.JS_TemplatePath = "";
                                PList.CH_PrintTime = ProductTime;
                                PList.CH_TemplatePath1 = this.Select_Template1.Text;
                                PList.CH_TemplatePath2 = this.Select_Template2.Text;
                                PList.ICCID = this.ICCIDStart.Text;
                                PList.MAC = this.MACStart.Text;
                                PList.Equipment = this.EquipmentStart.Text;
                                PList.RFID = this.RFIDStart.Text;
                                PList.CHUserName = this.UserShow.Text;
                                if (PMB.InsertPrintMessageBLL(PList))
                                {

                                    if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                    {
                                        DRSB.UpdateVipAndBatAndMacAndEquAndRFIDDAL(this.IMEI_Start.Text, this.SIMStart.Text,this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text,this.MACStart.Text, this.EquipmentStart.Text, this.RFIDStart.Text);
                                    }
                                    else
                                    {
                                        //记录关联数据信息到关联表
                                        Drs.Claer();
                                        Drs.IMEI1 = this.IMEI_Start.Text;
                                        Drs.IMEI2 = this.ShowSN.Text;
                                        Drs.IMEI3 = "";
                                        Drs.IMEI4 = "";
                                        Drs.IMEI5 = "";
                                        Drs.IMEI6 = this.MACStart.Text;
                                        Drs.IMEI7 = this.EquipmentStart.Text;
                                        Drs.IMEI8 = this.VIPStart.Text;
                                        Drs.IMEI9 = this.BATStart.Text;
                                        Drs.IMEI10 = "";
                                        Drs.IMEI11 = "";
                                        Drs.IMEI12 = "";
                                        Drs.RFID = this.RFIDStart.Text;
                                        Drs.ZhiDan = this.CB_ZhiDan.Text;
                                        Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                        DRSB.InsertRelativeSheetBLL(Drs);
                                    }
                                    if (SN1_num.Text != "" && Sn_mark == 1)
                                    {
                                        sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                        sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                        sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                        if (MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0')))
                                        {
                                            this.SN1_num.Text = sn1;
                                            this.IMEI_Start.Clear();
                                            this.SIMStart.Clear();
                                            this.VIPStart.Clear();
                                            this.BATStart.Clear();
                                            this.ICCIDStart.Clear();
                                            this.MACStart.Clear();
                                            this.EquipmentStart.Clear();
                                            //this.ShowSN.Clear();
                                            this.GLB_SN.Clear();
                                            this.RFIDStart.Clear();
                                            this.IMEI_Start.Focus();
                                        }
                                    }
                                    else
                                    {
                                        this.IMEI_Start.Clear();
                                        this.SIMStart.Clear();
                                        this.VIPStart.Clear();
                                        this.BATStart.Clear();
                                        this.ICCIDStart.Clear();
                                        this.MACStart.Clear();
                                        this.EquipmentStart.Clear();
                                        //this.ShowSN.Clear();
                                        this.GLB_SN.Clear();
                                        this.RFIDStart.Clear();
                                        this.IMEI_Start.Focus();
                                    }
                                }
                            }
                            else
                            {
                                ASS_sn = DRSB.SelectSNByImeiBLL(this.IMEI_Start.Text);
                                if (ASS_sn == "")
                                {
                                    ASS_sn = this.SN1_num.Text;
                                    Sn_mark = 1;
                                }
                                else
                                {
                                    Sn_mark = 0;
                                }
                                btFormat = btEngine.Documents.Open(lj);
                                //指定打印机名称
                                btFormat.PrintSetup.PrinterName = this.Printer1.Text;
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN1;
                                //对模板相应字段进行赋值
                                ValuesToTemplate(btFormat);
                                btFormat.SubStrings["IMEI"].Value = this.IMEI_Start.Text;
                                IMEI = this.IMEI_Start.Text;
                                list = PMB.SelectSnByIMEIBLL(this.IMEI_Start.Text);
                                foreach (PrintMessage a in list)
                                {
                                    if (a.SN != "")
                                    {
                                        btFormat.SubStrings["SN"].Value = a.SN;
                                        DZSN = a.SN;
                                        this.ShowSN.Text = a.SN;
                                        if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                        {
                                            DRSB.UpdateVipAndBatAndMacAndEquAndRFIDDAL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.RFIDStart.Text);
                                        }
                                        else
                                        {
                                            //记录关联数据信息到关联表
                                            Drs.Claer();
                                            Drs.IMEI1 = this.IMEI_Start.Text;
                                            Drs.IMEI2 = this.ShowSN.Text;
                                            Drs.IMEI3 = this.SIMStart.Text;
                                            Drs.IMEI4 = this.ICCIDStart.Text;
                                            Drs.IMEI5 = "";
                                            Drs.IMEI6 = this.MACStart.Text;
                                            Drs.IMEI7 = this.EquipmentStart.Text;
                                            Drs.IMEI8 = this.VIPStart.Text;
                                            Drs.IMEI9 = this.BATStart.Text;
                                            Drs.IMEI10 = "";
                                            Drs.IMEI11 = "";
                                            Drs.IMEI12 = "";
                                            Drs.RFID = this.RFIDStart.Text;
                                            Drs.ZhiDan = this.CB_ZhiDan.Text;
                                            Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                            DRSB.InsertRelativeSheetBLL(Drs);
                                        }
                                        //更新SN号
                                        DRSB.UpdateSNDAL(this.IMEI_Start.Text, a.SN);
                                        PMB.UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, a.SN, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                    }
                                    else
                                    {
                                        if (Sn_mark == 1)
                                        {
                                            btFormat.SubStrings["SN"].Value = this.SN1_num.Text;
                                            DZSN = this.SN1_num.Text;
                                            //判断SN是否超出范围
                                            if (this.SN1_num.Text.CompareTo(this.SN2_num.Text) == 1)
                                            {
                                                this.reminder.AppendText("SN号超出范围\r\n");
                                                this.IMEI_Start.Clear();
                                                this.SIMStart.Clear();
                                                this.VIPStart.Clear();
                                                this.BATStart.Clear();
                                                this.ICCIDStart.Clear();
                                                this.MACStart.Clear();
                                                this.EquipmentStart.Clear();
                                                this.IMEI_Start.Focus();
                                                return;
                                            }
                                            this.ShowSN.Text = this.SN1_num.Text;
                                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                                            {
                                                DRSB.UpdateVipAndBatAndMacAndEquAndRFIDDAL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.RFIDStart.Text);
                                            }
                                            else
                                            {
                                                //记录关联数据信息到关联表
                                                Drs.Claer();
                                                Drs.IMEI1 = this.IMEI_Start.Text;
                                                Drs.IMEI2 = this.ShowSN.Text;
                                                Drs.IMEI3 = this.SIMStart.Text;
                                                Drs.IMEI4 = this.ICCIDStart.Text;
                                                Drs.IMEI5 = "";
                                                Drs.IMEI6 = this.MACStart.Text;
                                                Drs.IMEI7 = this.EquipmentStart.Text;
                                                Drs.IMEI8 = this.VIPStart.Text;
                                                Drs.IMEI9 = this.BATStart.Text;
                                                Drs.IMEI10 = "";
                                                Drs.IMEI11 = "";
                                                Drs.IMEI12 = "";
                                                Drs.RFID = this.RFIDStart.Text;
                                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                                DRSB.InsertRelativeSheetBLL(Drs);
                                            }
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, this.SN1_num.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.SN1_num.Text, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                            if (this.SN1_num.Text != "")
                                            {
                                                sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
                                                sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
                                                sn1 = sn1_prefix + (sn1_suffix + 1).ToString().PadLeft(s, '0');
                                                MOPB.UpdateSNAddOneBLL(this.CB_ZhiDan.Text, (sn1_suffix + 1).ToString().PadLeft(s, '0'));
                                                this.SN1_num.Text = sn1;
                                            }
                                        }
                                        else
                                        {
                                            btFormat.SubStrings["SN"].Value = ASS_sn;
                                            DZSN = ASS_sn;
                                            this.ShowSN.Text = ASS_sn;
                                            //更新SN号
                                            DRSB.UpdateSNDAL(this.IMEI_Start.Text, ASS_sn);
                                            DRSB.UpdateVipAndBatAndMacAndEquAndRFIDDAL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.RFIDStart.Text);
                                            PMB.UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentBLL(this.IMEI_Start.Text, ProductTime, this.Select_Template1.Text, this.Select_Template2.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, ASS_sn, this.CB_ZhiDan.Text, this.RFIDStart.Text, this.UserShow.Text);
                                        }
                                    }
                                }
                                btFormat.Print();
                                Form1.Log("关联RFID号&&打印了IMEI号为" + this.IMEI_Start.Text + "的彩盒贴制单", null);
                                if (this.Select_Template2.Text != "")
                                {
                                    xc2 = 1;
                                }
                                this.IMEI_Start.Clear();
                                this.SIMStart.Clear();
                                this.VIPStart.Clear();
                                this.BATStart.Clear();
                                this.ICCIDStart.Clear();
                                this.MACStart.Clear();
                                this.EquipmentStart.Clear();
                                //this.ShowSN.Clear();
                                this.GLB_SN.Clear();
                                this.RFIDStart.Clear();
                                this.IMEI_Start.Focus();
                            }
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                        else
                        {
                            int UpInt = 0;
                            if (DRSB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                DRSB.UpdateVipAndBatAndMacAndEquAndRFIDDAL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.RFIDStart.Text);
                                UpInt++;
                            }
                            else
                            {
                                //记录关联数据信息到关联表
                                Drs.Claer();
                                Drs.IMEI1 = this.IMEI_Start.Text;
                                Drs.IMEI2 = "";
                                Drs.IMEI3 = this.SIMStart.Text;
                                Drs.IMEI4 = this.ICCIDStart.Text;
                                Drs.IMEI5 = "";
                                Drs.IMEI6 = this.MACStart.Text;
                                Drs.IMEI7 = this.EquipmentStart.Text;
                                Drs.IMEI8 = this.VIPStart.Text;
                                Drs.IMEI9 = this.BATStart.Text;
                                Drs.IMEI10 = "";
                                Drs.IMEI11 = "";
                                Drs.IMEI12 = "";
                                Drs.IMEI12 = "";
                                Drs.ZhiDan = this.CB_ZhiDan.Text;
                                Drs.TestTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff");
                                DRSB.InsertRelativeSheetBLL(Drs);
                                UpInt++;
                            }
                            if (PMB.CheckIMEIBLL(this.IMEI_Start.Text))
                            {
                                PMB.UpdateVipAndBatAndMacAndEquAndRFIDBLL(this.IMEI_Start.Text, this.SIMStart.Text, this.VIPStart.Text, this.BATStart.Text, this.ICCIDStart.Text, this.MACStart.Text, this.EquipmentStart.Text, this.RFIDStart.Text);
                                UpInt++;
                            }
                            if (UpInt == 2 || UpInt == 1)
                            {
                                BindingCountS++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountS.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                Bind_DGV.Rows[0].Cells[7].Value = this.MACStart.Text;
                                Bind_DGV.Rows[0].Cells[8].Value = this.EquipmentStart.Text;
                                Bind_DGV.Rows[0].Cells[9].Value = this.RFIDStart.Text;
                                player4.Play();
                            }
                            else
                            {
                                BindingCountF++;
                                Bind_DGV.Rows.Insert(0);
                                Bind_DGV.Rows[0].Cells[0].Value = BindingCountF.ToString();
                                Bind_DGV.Rows[0].Cells[1].Value = "未 绑 定";
                                Bind_DGV.Rows[0].Cells[2].Value = this.IMEI_Start.Text;
                                Bind_DGV.Rows[0].Cells[3].Value = this.SIMStart.Text;
                                Bind_DGV.Rows[0].Cells[4].Value = this.VIPStart.Text;
                                Bind_DGV.Rows[0].Cells[5].Value = this.BATStart.Text;
                                Bind_DGV.Rows[0].Cells[6].Value = this.ICCIDStart.Text;
                                Bind_DGV.Rows[0].Cells[7].Value = this.MACStart.Text;
                                Bind_DGV.Rows[0].Cells[8].Value = this.EquipmentStart.Text;
                                Bind_DGV.Rows[0].Cells[9].Value = this.RFIDStart.Text;
                                player.Play();
                            }
                            this.IMEI_Start.Clear();
                            this.SIMStart.Clear();
                            this.BATStart.Clear();
                            this.VIPStart.Clear();
                            this.ICCIDStart.Clear();
                            this.MACStart.Clear();
                            this.EquipmentStart.Clear();
                            this.RFIDStart.Clear();
                            this.IMEI_Start.Focus();
                            if (this.updata_inline.Visible == true)
                            {
                                MOPB.UpdateStatusByZhiDanBLL(this.CB_ZhiDan.Text);
                                statusChange();
                            }
                        }
                    }
                }
                else { this.EquipmentStart.Focus(); }
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
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
                ClearAll();
                this.CB_ZhiDan.Text = "";
                this.Select_Template1.Clear();
                this.Select_Template2.Clear();
                this.SoftModel.Clear();
                this.SN1_num.Clear();
                this.SN2_num.Clear();
                this.ProductDate.Clear();
                this.Color.Clear();
                this.Weight.Clear();
                this.ProductNo.Clear();
                this.SoftwareVersion.Clear();
                this.IMEI_num1.Clear();
                this.IMEI_num2.Clear();
                this.SIM_num1.Clear();
                this.SIM_num2.Clear();
                this.SIM_digits.Clear();
                this.SIM_prefix.Clear();
                this.BAT_num1.Clear();
                this.BAT_num2.Clear();
                this.BAT_digits.Clear();
                this.BAT_prefix.Clear();
                this.VIP_num1.Clear();
                this.VIP_num2.Clear();
                this.VIP_digits.Clear();
                this.VIP_prefix.Clear();
                this.ICCID_digits.Clear();
                this.ICCID_prefix.Clear();
                this.MAC_digits.Clear();
                this.MAC_prefix.Clear();
                this.Equipment_digits.Clear();
                this.Equipment_prefix.Clear();
                this.Select_Template1.Clear();
                this.Select_Template2.Clear();
                this.PrintDate.Clear();
                this.Remake.Clear();
                this.IMEIRel.Clear();
                this.SN1_num.ReadOnly = true;
                this.SN2_num.ReadOnly = true;
                this.ProductDate.ReadOnly = true;
                this.VIP_num1.ReadOnly = true;
                this.VIP_num2.ReadOnly = true;
                this.VIP_digits.ReadOnly = true;
                this.VIP_prefix.ReadOnly = true;
                this.BAT_num1.ReadOnly = true;
                this.BAT_num2.ReadOnly = true;
                this.BAT_digits.ReadOnly = true;
                this.BAT_prefix.ReadOnly = true;
                this.SIM_num1.ReadOnly = true;
                this.SIM_num2.ReadOnly = true;
                this.SIM_digits.ReadOnly = true;
                this.SIM_prefix.ReadOnly = true;
                this.ICCID_digits.ReadOnly = true;
                this.ICCID_prefix.ReadOnly = true;
                this.MAC_digits.ReadOnly = true;
                this.MAC_prefix.ReadOnly = true;
                this.Equipment_digits.ReadOnly = true;
                this.Equipment_prefix.ReadOnly = true;
                this.updata_inline.Visible = false;
            }
        }

        private void Open_file1_Click(object sender, EventArgs e)
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
        private void CheckSN_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckSN.Checked == false)
            {
                CheckFields.Remove(0);
            }
            else
            {
                CheckFields[0] = "SN";
            }
        }
        private void CheckSIM_CheckedChanged(object sender, EventArgs e)
        {
            if(this.CheckSIM.Checked == false)
            {
                CheckFields.Remove(1);
            }
            else
            {
                CheckFields[1] = "SIM";
            }
        }

        private void CheckICCID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckICCID.Checked == false)
            {
                CheckFields.Remove(2);
            }
            else
            {
                CheckFields[2] = "ICCID";
            }

        }

        private void CheckMAC_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckMAC.Checked == false)
            {
                CheckFields.Remove(3);
            }
            else
            {
                CheckFields[3] = "MAC";
            }

        }

        private void CheckEquipment_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckEquipment.Checked == false)
            {
                CheckFields.Remove(4);
            }
            else
            {
                CheckFields[4] = "Equipment";
            }
        }

        private void CheckVIP_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckVIP.Checked == false)
            {
                CheckFields.Remove(5);
            }
            else
            {
                CheckFields[5] = "VIP";
            }
        }

        private void CheckBAT_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckBAT.Checked == false)
            {
                CheckFields.Remove(6);
            }
            else
            {
                CheckFields[6] = "BAT";
            }
        }
        
        private void CheckRFID_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckRFID.Checked == false)
            {
                CheckFields.Remove(7);
            }
            else
            {
                CheckFields[7] = "RFID";
            }
        }
        //登录
        private void SiginIN_Click(object sender, EventArgs e)
        {
            SignIn sigin = new SignIn();
            sigin.ShowDialog();
            if(sigin.UserNamestr1 != "")
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
            if (MessageBox.Show("是否退出当前账号？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                this.UserShow.Clear();
            }
        }



        private void Open_file2_Click(object sender, EventArgs e)
        {
            if (this.Select_Template2.Text == "")
            {
                player1.Play();
            }
            else
            {
                string path = this.Select_Template2.Text;
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

        private void Tempalte1Num_Leave(object sender, EventArgs e)
        {
            if (this.Tempalte1Num.Text == "")
            {
                this.Tempalte1Num.Text = 1.ToString();
            }
            else
            {
                if (IsNumeric(this.Tempalte1Num.Text))
                {
                    TN1 = int.Parse(this.Tempalte1Num.Text);
                }
                else
                {
                    this.reminder.AppendText("请输入数字\r\n");
                    this.Tempalte1Num.Clear();
                    this.Tempalte1Num.Focus();
                }
            }
        }

        private void Tempalte2Num_Leave(object sender, EventArgs e)
        {
            if (this.Tempalte2Num.Text == "")
            {
                this.Tempalte2Num.Text = 1.ToString();
            }
            else
            {
                if (IsNumeric(this.Tempalte2Num.Text))
                {
                    TN2 = int.Parse(this.Tempalte2Num.Text);
                }
                else
                {
                    this.reminder.AppendText("请输入数字\r\n");
                    this.Tempalte2Num.Clear();
                    this.Tempalte2Num.Focus();
                }
            }
        }

        private void updata_inline_Click(object sender, EventArgs e)
        {
            string sn1_prefix = SN1_num.Text.Substring(0, this.SN1_num.Text.Length - s);
            long sn1_suffix = long.Parse(SN1_num.Text.Remove(0, (this.SN1_num.Text.Length) - s));
            if (MOPB.UpdateInlineByZhiDanBLL(this.CB_ZhiDan.Text,sn1_prefix,sn1_suffix.ToString().PadLeft(s, '0'),this.ProductDate.Text, this.SIM_num1.Text,this.SIM_num2.Text,this.SIM_digits.Text,this.SIM_prefix.Text,this.VIP_num1.Text,this.VIP_num2.Text,this.VIP_digits.Text,this.VIP_prefix.Text,this.BAT_num1.Text,this.BAT_num2.Text,this.BAT_digits.Text,this.BAT_prefix.Text,this.ICCID_digits.Text,this.ICCID_prefix.Text,this.MAC_digits.Text,this.MAC_prefix.Text,this.Equipment_digits.Text,this.Equipment_prefix.Text) > 0)
            {
                player4.Play();
                this.PrintDate.Text = this.ProductDate.Text;
            }
        }

        private void statusChange()
        {
            this.SN1_num.ReadOnly = true;
            this.SN2_num.ReadOnly = true;
            this.ProductDate.ReadOnly = true;
            this.VIP_num1.ReadOnly = true;
            this.VIP_num2.ReadOnly = true;
            this.VIP_digits.ReadOnly = true;
            this.VIP_prefix.ReadOnly = true;
            this.BAT_num1.ReadOnly = true;
            this.BAT_num2.ReadOnly = true;
            this.BAT_digits.ReadOnly = true;
            this.BAT_prefix.ReadOnly = true;
            this.SIM_num1.ReadOnly = true;
            this.SIM_num2.ReadOnly = true;
            this.SIM_digits.ReadOnly = true;
            this.SIM_prefix.ReadOnly = true;
            this.ICCID_digits.ReadOnly = true;
            this.ICCID_prefix.ReadOnly = true;
            this.MAC_digits.ReadOnly = true;
            this.MAC_prefix.ReadOnly = true;
            this.Equipment_digits.ReadOnly = true;
            this.Equipment_prefix.ReadOnly = true;
            this.updata_inline.Visible = false;
        }

        //锁定按钮函数
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
            if (this.Select_Template1.Text == "" && this.Select_Template2.Text == "")
            {
                player.Play();
                this.reminder.AppendText("模板不能为空\r\n");
                return;
            }
            AssociatedFields.Clear();
            //勾选重打框
            if (this.choose_reprint.Checked == true)
            {
                this.Re_IMEINum.ReadOnly = false;
                //this.IMEI_Start.ReadOnly = true;
                this.Re_IMEINum.Focus();

                FindField = "";

                //拼接查询字段
                if (this.CheckSN.Checked == true)
                {
                    FindField += "IMEI2 ,";
                }
                if (this.CheckSIM.Checked == true)
                {
                    FindField += "IMEI3 ,";
                }
                if(this.CheckVIP.Checked == true)
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
            }
            else
            {
                if (this.choose_sim.Checked == true)
                {
                    if (this.SIM_prefix.Text == "")
                    {
                        DialogResult dr = MessageBox.Show("确定SIM号前缀为空？", "系统提示", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    this.SIMStart.ReadOnly = false;
                    AssociatedFields[0] = "SIM";
                }
                if (this.choose_vip.Checked == true)
                {
                    if (this.VIP_prefix.Text == "")
                    {
                        DialogResult dr = MessageBox.Show("确定VIP号前缀为空？", "系统提示", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    this.VIPStart.ReadOnly = false;
                    AssociatedFields[1] = "VIP";
                }
                if (this.choose_bat.Checked == true)
                {
                    if (this.BAT_prefix.Text == "")
                    {
                        DialogResult dr = MessageBox.Show("确定BAT号前缀为空？", "系统提示", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    this.BATStart.ReadOnly = false;
                    AssociatedFields[2] = "BAT";
                }
                if (this.choose_iccid.Checked == true)
                {
                    if (this.ICCID_prefix.Text == "")
                    {
                        DialogResult dr = MessageBox.Show("确定ICCID号前缀为空？", "系统提示", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    this.ICCIDStart.ReadOnly = false;
                    AssociatedFields[3] = "ICCID";
                }
                if (this.choose_mac.Checked == true)
                {
                    if (this.MAC_prefix.Text == "")
                    {
                        DialogResult dr = MessageBox.Show("确定MAC号前缀为空？", "系统提示", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    this.MACStart.ReadOnly = false;
                    AssociatedFields[4] = "MAC";
                }
                if (this.choose_Equipment.Checked == true)
                {
                    if (this.Equipment_prefix.Text == "")
                    {
                        DialogResult dr = MessageBox.Show("确定Equipment号前缀为空？", "系统提示", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    this.EquipmentStart.ReadOnly = false;
                    AssociatedFields[5] = "Equipment";
                }
                if (this.choose_RFID.Checked == true)
                {
                    if (this.RFID_prefix.Text == "")
                    {
                        DialogResult dr = MessageBox.Show("确定RFID号前缀为空？", "系统提示", MessageBoxButtons.OKCancel);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    this.RFIDStart.ReadOnly = false;
                    AssociatedFields[6] = "RFID";
                }

                //当sim与iccid同时勾选，只打开sim文本扫入
                if(this.choose_sim.Checked == true && this.choose_iccid.Checked == true)
                {
                    this.ICCIDStart.ReadOnly = true;
                    AssociatedFields.Remove(3);

                }
                this.IMEI_Start.Focus();
                this.IMEI_Start.ReadOnly = false;
                this.PrintDate.ReadOnly = false;
            }
            this.open_template1.Enabled = false;
            this.open_template2.Enabled = false;
            this.Select_Template1.Enabled = false;
            this.Select_Template2.Enabled = false;
            this.Printer1.Enabled = false;
            this.Printer2.Enabled = false;
            this.CB_ZhiDan.Enabled = false;
            this.Refresh_zhidan.Enabled = false;
            this.Refresh_template.Enabled = false;
            this.Open_file1.Enabled = false;
            this.Open_file2.Enabled = false;
            this.Debug_print.Enabled = false;
            this.ToLock.Enabled = false;
            this.ToUnlock.Enabled = true;
            this.choose_sim.Enabled = false;
            this.choose_vip.Enabled = false;
            this.choose_bat.Enabled = false;
            this.choose_mac.Enabled = false;
            this.choose_iccid.Enabled = false;
            this.choose_Equipment.Enabled = false;
            this.choose_RFID.Enabled = false;
            this.Re_Tem1.Enabled = false;
            this.Re_Tem2.Enabled = false;
            this.UpdateIMEIBySim.Enabled = false;
            this.UpdataSimByImei.Enabled = false;
            this.choose_reprint.Enabled = false;
            this.NoCheckCode.Enabled = false;
            this.NoPaper.Enabled = false;
            this.AutoTest.Enabled = false;
            this.Couple.Enabled = false;
            this.WriteImei.Enabled = false;
            this.ParamDownload.Enabled = false;
            this.GPS.Enabled = false;
            this.PrintDate.ReadOnly = true;
            this.Tempalte1Num.ReadOnly = true;
            this.Tempalte2Num.ReadOnly = true;

            this.SN1_num.Enabled = false;
            this.SN2_num.Enabled = false;
            this.ProductDate.Enabled = false;
            this.VIP_num1.Enabled = false;
            this.VIP_num2.Enabled = false;
            this.VIP_digits.Enabled = false;
            this.VIP_prefix.Enabled = false;
            this.BAT_num1.Enabled = false;
            this.BAT_num2.Enabled = false;
            this.BAT_digits.Enabled = false;
            this.BAT_prefix.Enabled = false;
            this.SIM_num1.Enabled = false;
            this.SIM_num2.Enabled = false;
            this.SIM_digits.Enabled = false;
            this.SIM_prefix.Enabled = false;
            this.ICCID_digits.Enabled = false;
            this.ICCID_prefix.Enabled = false;
            this.MAC_digits.Enabled = false;
            this.MAC_prefix.Enabled = false;
            this.Equipment_digits.Enabled = false;
            this.Equipment_prefix.Enabled = false;
            this.updata_inline.Enabled = false;

            this.CheckSN.Enabled = false;
            this.CheckSIM.Enabled = false;
            this.CheckBAT.Enabled = false;
            this.CheckICCID.Enabled = false;
            this.CheckMAC.Enabled = false;
            this.CheckEquipment.Enabled = false;
            this.CheckVIP.Enabled = false;
            this.CheckRFID.Enabled = false;

            this.SiginIN.Enabled = false;
            this.QuitBt.Enabled = false;


            BindingCountF = 0;
            BindingCountS = 0;
            //清空列表
            while (this.Bind_DGV.Rows.Count > 1)
            {
                this.Bind_DGV.Rows.RemoveAt(0);
            }


            //查工位
            MissingSql = "SELECT id FROM dbo.Gps_TestResult  WHERE IMEI='IMEIInput'";
            if(this.AutoTest.Checked == true)
            {
                MissingSql += "AND AutoTestResult = '1'";
                checklog = 1;
            }
            if (this.Couple.Checked == true)
            {
                MissingSql += "AND CoupleResult = '1'";
                checklog = 1;
            }
            if (this.WriteImei.Checked == true)
            {
                MissingSql += "AND WriteImeiResult = '1'";
                checklog = 1;
            }
            if (this.ParamDownload.Checked == true)
            {
                MissingSql += "AND ParamDownloadResult = '1'";
                checklog = 1;
            }
            if (this.GPS.Checked == true)
            {
                MissingSql += "AND GPSResult = '1'";
                checklog = 1;
            }
            if (this.AutoTest.Checked == false && this.Couple.Checked == false && this.WriteImei.Checked == false && this.ParamDownload.Checked == false && this.GPS.Checked ==false)
            {
                checklog = 0;
            }
            int SortFlag= 0;
            foreach(int key in AssociatedFields.Keys)
            {
                SortDictio[SortFlag] = key;
                SortFlag++;
            }
            //记录页面选择到数据库
            MPRPB.InsertPrintRecordParamBLL(this.CB_ZhiDan.Text, Convert.ToInt32(this.choose_sim.Checked), Convert.ToInt32(this.choose_vip.Checked), Convert.ToInt32(this.choose_bat.Checked), Convert.ToInt32(this.choose_iccid.Checked), Convert.ToInt32(this.choose_mac.Checked),Convert.ToInt32(this.choose_Equipment.Checked), Convert.ToInt32(this.choose_RFID.Checked), Convert.ToInt32(this.NoCheckCode.Checked), Convert.ToInt32(this.NoPaper.Checked), Convert.ToInt32(this.UpdataSimByImei.Checked), Convert.ToInt32(this.UpdateIMEIBySim.Checked), Convert.ToInt32(this.AutoTest.Checked), Convert.ToInt32(this.Couple.Checked), Convert.ToInt32(this.WriteImei.Checked), Convert.ToInt32(this.ParamDownload.Checked), int.Parse(this.Tempalte1Num.Text), int.Parse(this.Tempalte2Num.Text), Convert.ToInt32(this.GPS.Checked)
            , Convert.ToInt32(this.CheckSN.Checked), Convert.ToInt32(this.CheckSIM.Checked), Convert.ToInt32(this.CheckBAT.Checked), Convert.ToInt32(this.CheckICCID.Checked), Convert.ToInt32(this.CheckMAC.Checked), Convert.ToInt32(this.CheckEquipment.Checked), Convert.ToInt32(this.CheckVIP.Checked), Convert.ToInt32(this.CheckRFID.Checked));
            //将制单放在全局变量G_zhidan里
            if (this.CB_ZhiDan.Text.Contains("-"))
            {
                string[] zd = this.CB_ZhiDan.Text.Split('-');
                G_zhidan = zd[0];
            }
            else
            {
                G_zhidan = this.CB_ZhiDan.Text;
            }

            MOPB.UpdateCH_TemplatePath1DAL(G_zhidan, lj, lj2);


        }

        //点解锁时弹出输入密码对话框
        private void ToUnlock_Click(object sender, EventArgs e)
        {
            CH_Unlock chunlock = new CH_Unlock(this);
            chunlock.ShowDialog();
        }

        //解锁的内容
        public void unlock_content()
        {
            this.open_template1.Enabled = true;
            this.open_template2.Enabled = true;
            this.Select_Template1.Enabled = true;
            this.Select_Template2.Enabled = true;
            this.Printer1.Enabled = true;
            this.Printer2.Enabled = true;
            this.CB_ZhiDan.Enabled = true;
            this.Refresh_zhidan.Enabled = true;
            this.Refresh_template.Enabled = true;
            this.Open_file1.Enabled = true;
            this.Open_file2.Enabled = true;
            this.Debug_print.Enabled = true;
            this.ToLock.Enabled = true;
            this.choose_sim.Enabled = true;
            this.choose_vip.Enabled = true;
            this.choose_bat.Enabled = true;
            this.choose_mac.Enabled = true;
            this.choose_iccid.Enabled = true;
            this.choose_Equipment.Enabled = true;
            this.choose_RFID.Enabled = true;
            this.Re_Tem1.Enabled = true;
            this.Re_Tem2.Enabled = true;
            this.UpdateIMEIBySim.Enabled = true;
            this.UpdataSimByImei.Enabled = true;
            this.choose_reprint.Enabled = true;
            this.NoCheckCode.Enabled = true;
            this.NoPaper.Enabled = true;
            this.AutoTest.Enabled = true;
            this.Couple.Enabled = true;
            this.WriteImei.Enabled = true;
            this.ParamDownload.Enabled = true;
            this.GPS.Enabled = true;
            this.PrintDate.ReadOnly = false;
            this.Tempalte1Num.ReadOnly = false;
            this.Tempalte2Num.ReadOnly = false;

            this.SN1_num.Enabled = true;
            this.SN2_num.Enabled = true;
            this.ProductDate.Enabled = true;
            this.VIP_num1.Enabled = true;
            this.VIP_num2.Enabled = true;
            this.VIP_digits.Enabled = true;
            this.VIP_prefix.Enabled = true;
            this.BAT_num1.Enabled = true;
            this.BAT_num2.Enabled = true;
            this.BAT_digits.Enabled = true;
            this.BAT_prefix.Enabled = true;
            this.SIM_num1.Enabled = true;
            this.SIM_num2.Enabled = true;
            this.SIM_digits.Enabled = true;
            this.SIM_prefix.Enabled = true;
            this.ICCID_digits.Enabled = true;
            this.ICCID_prefix.Enabled = true;
            this.MAC_digits.Enabled = true;
            this.MAC_prefix.Enabled = true;
            this.Equipment_digits.Enabled = true;
            this.Equipment_prefix.Enabled = true;
            this.updata_inline.Enabled = true;

            this.CheckSN.Enabled = true;
            this.CheckSIM.Enabled = true;
            this.CheckBAT.Enabled = true;
            this.CheckICCID.Enabled = true;
            this.CheckMAC.Enabled = true;
            this.CheckEquipment.Enabled = true;
            this.CheckVIP.Enabled = true;
            this.CheckRFID.Enabled = true;

            this.IMEI_Start.ReadOnly = true;
            this.PrintDate.ReadOnly = true;
            this.SIMStart.ReadOnly = true;
            this.VIPStart.ReadOnly = true;
            this.BATStart.ReadOnly = true;
            this.ICCIDStart.ReadOnly = true;
            this.MACStart.ReadOnly = true;
            this.EquipmentStart.ReadOnly = true;
            this.RFIDStart.ReadOnly = true;

            this.SiginIN.Enabled = true;
            this.QuitBt.Enabled = true;

            MissingSql = "";
            AssociatedFields.Clear();
        }

        //刷新模板按钮函数
        private void Refresh_template_Click(object sender, EventArgs e)
        {
            if (this.Select_Template1.Text != "")
            {
                string path = this.Select_Template1.Text;
                string filename = Path.GetFileName(path);
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "彩盒贴模板"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板");
                }
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "彩盒贴模板\\" + RefreshNum))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板\\" + RefreshNum);
                }
                File.Copy(path, AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板\\" + RefreshNum + "\\" + filename, true);
                lj = AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板\\" + RefreshNum + "\\" + filename;
                this.reminder.AppendText("刷新模板1成功\r\n");
            }
            if (this.Select_Template2.Text != "")
            {
                string path1 = this.Select_Template2.Text;
                string filename1 = Path.GetFileName(path1);
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "彩盒贴模板"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板");
                }
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "彩盒贴模板\\" + RefreshNum))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板\\" + RefreshNum);
                }
                File.Copy(path1, AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板\\" + RefreshNum + "\\" + filename1, true);
                lj2 = AppDomain.CurrentDomain.BaseDirectory + "\\彩盒贴模板\\" + RefreshNum + "\\" + filename1;
                this.reminder.AppendText("刷新模板2成功\r\n");
            }
             RefreshNum++;
        }

        //
        public void SetDGV()
        {
            Bind_DGV.Rows.Add();
            Bind_DGV.Rows[0].Cells[5].Value = "1";
            Bind_DGV.Rows.Insert(0);
            Bind_DGV.Rows[0].Cells[0].Value = "2";
        }


    }
}
