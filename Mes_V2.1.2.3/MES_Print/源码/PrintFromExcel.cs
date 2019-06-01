using System;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using Print.Message.Bll;
using System.Drawing.Printing;
using Seagull.BarTender.Print;
using System.Text.RegularExpressions;
using ExcelPrint.Param.Bll;
using System.IO;
using Print_Message;
using System.Media;
using ManuOrder.Param.BLL;
using DataRelative.Param.BLL;
using System.Threading;

namespace WindowsForms_print
{
    public partial class PrintFromExcel : Form
    {
        InputExcelBLL IEB = new InputExcelBLL();
        ManuOrderParamBLL MOPB = new ManuOrderParamBLL();
        ManuExcelPrintParamBLL MEPPB = new ManuExcelPrintParamBLL();
        DataRelativeSheetBLL DRSB = new DataRelativeSheetBLL();
        List<ManuExcelPrintParam> mepp = new List<ManuExcelPrintParam>();
        List<Gps_ManuOrderParam> G_MOP = new List<Gps_ManuOrderParam>();
        Engine btEngine = new Engine();
        LabelFormatDocument btFormat;

        SoundPlayer player = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "失败.wav");

        //记录模板刷新次数
        int RefreshNum = 0;

        //打印参数
        int TN =1;
        string lj = "";

        //存是否重复的标志
        bool[] IsRepeat = {false};
        //存是否已打印标志
        bool[] IsPrint = {false};
        //开启上传打印数据标志
        bool BeginInsertFlag = false;

        //记录Excel字段，用来插入关联表
        string IMEI; string SN; string SIM; string VIP; string BAT; string MAC; string ICCID; string Equipment;
        //插入关联表的Sql语句
        string DrsbSql; string ToReplay;
        //记录字典里的个数
        int SdNum;
        //记录字典里有没有IMEI这个字段
        int HaveImei1 = 0;

        //导入Excel时将字段放入容器
        SortedDictionary<int, string> AssociatedFields = new SortedDictionary<int, string>();

        public PrintFromExcel()
        {
            InitializeComponent();
            int wid = Screen.PrimaryScreen.WorkingArea.Width;
            this.Width = wid;
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

        private void PrintFromExcel_Load(object sender, EventArgs e)
        {
            PrintDocument print = new PrintDocument();
            string sDefault = print.PrinterSettings.PrinterName;//默认打印机名
            this.Printer.Text = sDefault;
            foreach (string sPrint in PrinterSettings.InstalledPrinters)//获取所有打印机名称
            {
                Printer.Items.Add(sPrint);
            }
            G_MOP = MOPB.SelectZhidanNumBLL();
            foreach (Gps_ManuOrderParam a in G_MOP)
            {
                this.CB_Zhidan.Items.Add(a.ZhiDan);
            }
            btEngine.Start();
        }

        //选择模板
        private void Open_Template_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件|*.btw";
            ofd.ShowDialog();
            string path = ofd.FileName;
            this.Select_Template.Text = path;
            lj = path;
        }

        //导入Excel文件
        private void Import_Click(object sender, EventArgs e)
        {
            DrsbSql = "";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "文本文件|*.xls;*.xlsx";
            dialog.ShowDialog();
            string path = dialog.FileName;
            if (path == "")
            {
                return;
            }
            string strExtension = path.Substring(path.LastIndexOf('.'));
            this.dataGridView1.DataSource = "";
            this.ImportPath.Text = path;
            DataTable dt = IEB.GetExcelDatatable(path, strExtension);
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[0].Width = 200;
            dataGridView1.Columns[1].Width = 200;
            dataGridView1.Columns[2].Width = 200;
            dataGridView1.Columns[3].Width = 200;
            dataGridView1.Columns[4].Width = 200;
            //判断导入的Excel是否有IMEI字段
            for (int a = 0; a < 5; a++)
            {
                if (dataGridView1.Rows[0].Cells[a].Value.ToString().Equals("IMEI", StringComparison.OrdinalIgnoreCase))
                {
                    HaveImei1 = 1;
                    break;
                }
                HaveImei1 = 0;
            }
            //将Excel字段放入字典
            for (int i = 0; i < 5; i++)
            {
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("IMEI", StringComparison.OrdinalIgnoreCase))
                {
                    IMEI = "IMEI1";
                    AssociatedFields[i] = IMEI;
                }
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("SN", StringComparison.OrdinalIgnoreCase))
                {
                    SN = "IMEI2";
                    AssociatedFields[i] = SN;
                }
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("SIM", StringComparison.OrdinalIgnoreCase))
                {
                    SIM = "IMEI3";
                    AssociatedFields[i] = SIM;
                }
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("ICCID", StringComparison.OrdinalIgnoreCase))
                {
                    ICCID = "IMEI4";
                    AssociatedFields[i] = ICCID;
                }
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("MAC", StringComparison.OrdinalIgnoreCase))
                {
                    MAC = "IMEI6";
                    AssociatedFields[i] = MAC;
                }
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("Equipment", StringComparison.OrdinalIgnoreCase))
                {
                    Equipment = "IMEI7";
                    AssociatedFields[i] = Equipment;
                }
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("VIP", StringComparison.OrdinalIgnoreCase))
                {
                    VIP = "IMEI8";
                    AssociatedFields[i] = VIP;
                }
                if (dataGridView1.Rows[0].Cells[i].Value.ToString().Equals("BAT", StringComparison.OrdinalIgnoreCase))
                {
                    BAT = "IMEI9";
                    AssociatedFields[i] = BAT;
                }
            }
            SdNum = AssociatedFields.Count;
            //拼凑sql语句
            if (HaveImei1 == 1)
            {
                DrsbSql = " INSERT INTO dbo.DataRelativeSheet(";
                for (int j = 0; j < AssociatedFields.Count; j++)
                {
                    DrsbSql += AssociatedFields[j] + ",";
                }
                DrsbSql += "ZhiDan,TestTime) VALUES(TheValues";
            }
            else
            {
                DrsbSql = " INSERT INTO dbo.DataRelativeSheet(IMEI1,";
                for (int j = 0; j < AssociatedFields.Count; j++)
                {
                    DrsbSql += AssociatedFields[j] + ",";
                }
                DrsbSql += "ZhiDan,TestTime) VALUES(TheValues";
            }
        }

        //判断行数的输入格式是否为“数字-数字”
        public static bool IsTrue(string inString)
        {
            Regex regex = new Regex("^\\d+\\-+\\d");
            return regex.IsMatch(inString.Trim());
        }

        //鼠标离开模板打印份数框时触发的函数
        private void TemplateNum_Leave(object sender, EventArgs e)
        {
            if (this.TemplateNum.Text == "")
            {
                this.TemplateNum.Text = 1.ToString();
            }
            else
            {
                if (IsNumeric(this.TemplateNum.Text))
                {
                    TN = int.Parse(this.TemplateNum.Text);
                }
                else
                {
                    player.Play();
                    this.remined.AppendText("请输入数字\r\n");
                    this.TemplateNum.Text = 1.ToString();
                    this.TemplateNum.Focus();
                }
            }
        }

        //输入行数后按下Ecter键触发的函数
        private void RowNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if(this.ImportPath.Text == "")
                {
                    player.Play();
                    this.remined.AppendText("请先导入Excel\r\n");
                    this.RowNumber.Clear();
                    this.RowNumber.Focus();
                    return;
                }
                if (this.RowNumber.Text != "")
                {
                    int i = 1;
                    this.dataGridView1.DataSource = "";
                    string strExtension = this.ImportPath.Text.Substring(this.ImportPath.Text.LastIndexOf('.'));
                    DataTable dt = IEB.GetExcelDatatable(this.ImportPath.Text,strExtension);
                    DataRow dr2 = dt.Rows[0];
                    DataTable dd = new DataTable();
                    dd.Columns.Add(" ");
                    dd.Columns.Add(new DataColumn(dr2[0].ToString(), typeof(string)));
                    dd.Columns.Add(new DataColumn(dr2[1].ToString(), typeof(string)));
                    dd.Columns.Add(new DataColumn(dr2[2].ToString(), typeof(string)));
                    dd.Columns.Add(new DataColumn(dr2[3].ToString(), typeof(string)));
                    dd.Columns.Add(new DataColumn(dr2[4].ToString(), typeof(string)));
                    if (IsNumeric(this.RowNumber.Text))//单条数据
                    {
                        DataRow dr3 = dt.Rows[int.Parse(this.RowNumber.Text)];
                        if (dr3[0].ToString() == "")
                        {
                            player.Play();
                            this.remined.AppendText("您输入的行数大于Excel行数\r\n");
                            this.RowNumber.Clear();
                            this.RowNumber.Focus();
                            return;
                        }
                        dd.Rows.Add(1, dr3[0].ToString(), dr3[1].ToString(), dr3[2].ToString(), dr3[3].ToString(), dr3[4].ToString());
                        dataGridView1.DataSource = dd;
                        dataGridView1.Columns[0].Width = 50;
                        dataGridView1.Columns[1].Width = 200;
                        dataGridView1.Columns[2].Width = 200;
                        dataGridView1.Columns[3].Width = 200;
                        dataGridView1.Columns[4].Width = 200;
                        dataGridView1.Columns[5].Width = 200;
                    }
                    else//多条数据
                    {
                        if (IsTrue(this.RowNumber.Text))
                        {
                            string[] range = this.RowNumber.Text.Split('-');
                            if(int.Parse(range[0])> int.Parse(range[1]))
                            {
                                player.Play();
                                this.remined.AppendText("起始位不能大于终止位\r\n");
                                this.RowNumber.Clear();
                                this.RowNumber.Focus();
                                return;
                            }
                            for (int j = int.Parse(range[0]); j <= int.Parse(range[1]); j++)
                            {
                                DataRow dr3 = dt.Rows[j];
                                dd.Rows.Add(i, dr3[0].ToString(), dr3[1].ToString(), dr3[2].ToString(), dr3[3].ToString(), dr3[4].ToString());
                                dataGridView1.DataSource = dd;
                                i++;
                            }
                            //开启查询线程
                            Thread thread1 = new Thread(CheckUseThread);
                            thread1.Start();
                            dataGridView1.Columns[0].Width = 50;
                            dataGridView1.Columns[1].Width = 200;
                            dataGridView1.Columns[2].Width = 200;
                            dataGridView1.Columns[3].Width = 200;
                            dataGridView1.Columns[4].Width = 200;
                            dataGridView1.Columns[5].Width = 200;
                        }
                        else
                        {
                            player.Play();
                            this.remined.AppendText("请输入'数字-数字'格式\r\n");
                            this.RowNumber.Clear();
                            this.RowNumber.Focus();
                        }
                    }
                }
            }
        }

        //查重线程
        public void CheckUseThread()
        {
            int last = this.dataGridView1.RowCount;
            for (int i = 1; i < last; i++)
            {
                if (!MEPPB.CheckIMEIBLL(dataGridView1.Rows[i].Cells[0].Value.ToString(), dataGridView1.Rows[i].Cells[1].Value.ToString()))
                {
                    IsRepeat[i] = false;
                }
                else
                {
                    IsRepeat[i] = true;
                }
            }
        }

        //打印按钮函数
        private void ExcelToPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.ImportPath.Text == "")
                {
                    player.Play();
                    this.remined.AppendText("请先导入Excel\r\n");
                    return;
                }
                if (this.Select_Template.Text != "")
                {
                    if (IsNumeric(this.RowNumber.Text))//单个打印
                    {
                        string strExtension = this.ImportPath.Text.Substring(this.ImportPath.Text.LastIndexOf('.'));//先从excel获取数据，在打印
                        DataTable dt = IEB.GetExcelDatatable(this.ImportPath.Text, strExtension);//先从excel获取数据，在打印
                        DataRow dr3 = dt.Rows[int.Parse(this.RowNumber.Text)];//先从excel获取数据，在打印
                        if (!MEPPB.CheckIMEIBLL(dr3[0].ToString(), dr3[1].ToString()))  //不重号
                        {
                            LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                            //指定打印机名称
                            btFormat.PrintSetup.PrinterName = this.Printer.Text;
                            //对模板相应字段进行赋值
                            btFormat.SubStrings["IMEI1"].Value = dr3[0].ToString();
                            btFormat.SubStrings["IMEI2"].Value = dr3[1].ToString();
                            btFormat.SubStrings["IMEI3"].Value = dr3[2].ToString();
                            btFormat.SubStrings["IMEI4"].Value = dr3[3].ToString();
                            btFormat.SubStrings["IMEI5"].Value = dr3[4].ToString();
                            btFormat.SubStrings["ProductDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
                            //打印份数,同序列打印的份数
                            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                            //记录数据
                            mepp.Add(new ManuExcelPrintParam()
                            {
                                IMEI1 = dr3[0].ToString(),
                                IMEI2 = dr3[1].ToString(),
                                IMEI3 = dr3[2].ToString(),
                                IMEI4 = dr3[3].ToString(),
                                IMEI5 = dr3[4].ToString(),
                                PrintTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff"),
                                Template = this.Select_Template.Text
                            });
                            if (MEPPB.InsertManuExcelPrintBLL(mepp))
                            {
                                ToReplay = "";
                                if (HaveImei1 == 1)              //插入关联表
                                {
                                    for (int k = 0; k < SdNum; k++)
                                    {
                                        ToReplay += "'" + dr3[k].ToString() + "',";
                                    }
                                    ToReplay += "'" + this.CB_Zhidan.Text + "','" + System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff") + "')";
                                    string Insql = DrsbSql.Replace("TheValues", ToReplay);
                                    DRSB.InsertRSFromExcelBLL(Insql);
                                    
                                }
                                else                                //插入关联表
                                {
                                    ToReplay += "'" + dr3[0].ToString() + "',";
                                    for (int k = 0; k < SdNum; k++)
                                    {
                                        ToReplay += "'" + dr3[k].ToString() + "',";
                                    }
                                    ToReplay += "'" + this.CB_Zhidan.Text + "','" + System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff") + "')";
                                    string Insql = DrsbSql.Replace("TheValues", ToReplay);
                                    DRSB.InsertRSFromExcelBLL(Insql);
                                   
                                }
                                btFormat.Print();//打印
                                Form1.Log("Excel打印了机身贴IMEI号为" + dr3[1].ToString() + "的制单", null);
                            }
                        }
                        else
                        {
                            player.Play();
                            this.remined.AppendText(dr3[0].ToString() + "或" + dr3[1].ToString() + "重号\r\n");
                        }
                    }
                    else if (IsTrue(this.RowNumber.Text))//批量打印
                    {
                        //string[] range = this.RowNumber.Text.Split('-');
                        //LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        ////指定打印机
                        //btFormat.PrintSetup.PrinterName = this.Printer.Text;
                        //string strExtension = this.ImportPath.Text.Substring(this.ImportPath.Text.LastIndexOf('.'));
                        //DataTable dt = IEB.GetExcelDatatable(this.ImportPath.Text, strExtension);
                        //for (int i = int.Parse(range[0]); i <= int.Parse(range[1]); i++)
                        //{
                        //    DataRow dr3 = dt.Rows[i];
                        //    if (!MEPPB.CheckIMEIBLL(dr3[0].ToString(), dr3[1].ToString()))
                        //    {
                        //        //对模板相应字段进行赋值
                        //        btFormat.SubStrings["IMEI1"].Value = dr3[0].ToString();
                        //        btFormat.SubStrings["IMEI2"].Value = dr3[1].ToString();
                        //        btFormat.SubStrings["IMEI3"].Value = dr3[2].ToString();
                        //        btFormat.SubStrings["IMEI4"].Value = dr3[3].ToString();
                        //        btFormat.SubStrings["IMEI5"].Value = dr3[4].ToString();
                        //        btFormat.SubStrings["ProductDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
                        //        //打印份数,同序列打印的份数
                        //        btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                        //        //记录数据
                        //        mepp.Add(new ManuExcelPrintParam()
                        //        {
                        //            IMEI1 = dr3[0].ToString(),
                        //            IMEI2 = dr3[1].ToString(),
                        //            IMEI3 = dr3[2].ToString(),
                        //            IMEI4 = dr3[3].ToString(),
                        //            IMEI5 = dr3[4].ToString(),
                        //            PrintTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff"),
                        //            Template = this.Select_Template.Text
                        //        });
                        //        if (MEPPB.InsertManuExcelPrintBLL(mepp))
                        //        {
                        //            ToReplay = "";
                        //            if (HaveImei1 == 1)
                        //            {
                        //                for (int k = 0; k < SdNum; k++)
                        //                {
                        //                    ToReplay += "'" + dr3[k].ToString() + "',";
                        //                }
                        //                ToReplay += "'" + this.CB_Zhidan.Text + "','" + System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff") + "')";
                        //                string Insql = DrsbSql.Replace("TheValues", ToReplay);
                        //                DRSB.InsertRSFromExcelBLL(Insql);

                        //            }
                        //            else
                        //            {
                        //                ToReplay += "'" + dr3[0].ToString() + "',";
                        //                for (int k = 0; k < SdNum; k++)
                        //                {
                        //                    ToReplay += "'" + dr3[k].ToString() + "',";
                        //                }
                        //                ToReplay += "'" + this.CB_Zhidan.Text + "','" + System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff") + "')";
                        //                string Insql = DrsbSql.Replace("TheValues", ToReplay);
                        //                DRSB.InsertRSFromExcelBLL(Insql);

                        //            }
                        //            btFormat.Print();//打印
                        //            Form1.Log("Excel打印了机身贴IMEI号为" + dr3[1].ToString() + "的制单", null);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        player.Play();
                        //        this.remined.AppendText(dr3[0].ToString() + "或" + dr3[1].ToString() + "重号\r\n");
                        //    }
                        //}
                        BeginInsertFlag = true;
                        Thread thread1 = new Thread(InsertPrintDataThread);
                        thread1.Start();
                        int rowCount = this.dataGridView1.RowCount;
                        for (int i = 1;i<rowCount;i++)
                        {
                            if (IsRepeat[i] == false)
                            {
                                btFormat.SubStrings["IMEI1"].Value = dataGridView1.Rows[i].Cells[0].Value.ToString();
                                btFormat.SubStrings["IMEI2"].Value = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                btFormat.SubStrings["IMEI3"].Value = dataGridView1.Rows[i].Cells[2].Value.ToString();
                                btFormat.SubStrings["IMEI4"].Value = dataGridView1.Rows[i].Cells[3].Value.ToString();
                                btFormat.SubStrings["IMEI5"].Value = dataGridView1.Rows[i].Cells[4].Value.ToString();
                                btFormat.SubStrings["ProductDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                                btFormat.Print();//打印
                                Form1.Log("Excel打印了机身贴IMEI号为" + dataGridView1.Rows[i].Cells[0].Value.ToString() + "的制单", null);
                            }
                            else
                            {
                                player.Play();
                                this.remined.AppendText(dataGridView1.Rows[i].Cells[0].Value.ToString() + "或" + dataGridView1.Rows[i].Cells[1].Value.ToString() + "重号\r\n");
                            }
                        }
                    }
                    else
                    {
                        player.Play();
                        this.remined.AppendText("请输入正确的行数格式：数字-数字\r\n");
                        this.RowNumber.Clear();
                        this.RowNumber.Focus();
                        this.dataGridView1.DataSource = "";
                    }
                }
                else
                {
                    player.Play();
                    this.remined.AppendText("请先选择模板\r\n");
                }
            }
            catch (Exception ex)
            {
                if (ToReplay != "")
                    ToReplay = "";
                MessageBox.Show("Exception:" + ex.Message);
            }
        }

        //上传数据线程
        public void InsertPrintDataThread()
        {
            int i = 1;
            while(BeginInsertFlag)
            {
                if (IsPrint[i] == true)
                {
                    mepp.Add(new ManuExcelPrintParam()
                    {
                        IMEI1 = dataGridView1.Rows[i].Cells[0].Value.ToString(),
                        IMEI2 = dataGridView1.Rows[i].Cells[1].Value.ToString(),
                        IMEI3 = dataGridView1.Rows[i].Cells[2].Value.ToString(),
                        IMEI4 = dataGridView1.Rows[i].Cells[3].Value.ToString(),
                        IMEI5 = dataGridView1.Rows[i].Cells[4].Value.ToString(),
                        PrintTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff"),
                        Template = this.Select_Template.Text
                    });
                    if (MEPPB.InsertManuExcelPrintBLL(mepp))//上传成功
                    {
                        ToReplay = "";
                        if (HaveImei1 == 1)//上传关联关系到关联表
                        {
                            for (int k = 0; k < SdNum; k++)
                            {
                                ToReplay += "'" + dataGridView1.Rows[i].Cells[k].Value.ToString() + "',";
                            }
                            ToReplay += "'" + this.CB_Zhidan.Text + "','" + System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff") + "')";
                            string Insql = DrsbSql.Replace("TheValues", ToReplay);
                            DRSB.InsertRSFromExcelBLL(Insql);

                        }
                        else
                        {
                            ToReplay += "'" + dataGridView1.Rows[i].Cells[0].Value.ToString() + "',";
                            for (int k = 0; k < SdNum; k++)
                            {
                                ToReplay += "'" + dataGridView1.Rows[i].Cells[k].Value.ToString() + "',";
                            }
                            ToReplay += "'" + this.CB_Zhidan.Text + "','" + System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff") + "')";
                            string Insql = DrsbSql.Replace("TheValues", ToReplay);
                            DRSB.InsertRSFromExcelBLL(Insql);
                        }
                    }
                    i++;
                    if (i>dataGridView1.RowCount-1)
                    {
                        BeginInsertFlag = false;
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        //打开Excel按钮函数
        private void OpenExcel_Click(object sender, EventArgs e)
        {
            if (this.ImportPath.Text == "")
            {
                player.Play();
               this.remined.AppendText("请先导入Excel\r\n");
            }
            else
            {
                string path = this.ImportPath.Text;
                if (File.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    player.Play();
                    this.remined.AppendText("Excel不存在\r\n");
                }
            }
        }

        //查询函数
        private void E_Check_Click(object sender, EventArgs e)
        {
            if (this.WhatToCheck.Text != "")
            {
                this.dataGridView1.DataSource = "";
                int co=1;
                mepp = MEPPB.SelectByImei1to5BLL(this.WhatToCheck.Text);
                if (mepp.Count == 0)
                {
                    player.Play();
                    this.remined.AppendText("查找不到关于" + this.WhatToCheck.Text + "的记录\r\n");
                    this.WhatToCheck.Clear();
                }
                else
                {
                    DataTable dd = new DataTable();
                    dd.Columns.Add(" ");
                    dd.Columns.Add("IMEI1");
                    dd.Columns.Add("IMEI2");
                    dd.Columns.Add("IMEI3");
                    dd.Columns.Add("IMEI4");
                    dd.Columns.Add("IMEI5");
                    dd.Columns.Add("打印时间");
                    dd.Columns.Add("打印模板");
                    dd.Columns.Add("重打次数");
                    dd.Columns.Add("首次重打时间");
                    dd.Columns.Add("末次重打时间");
                    foreach (ManuExcelPrintParam a in mepp)
                    {
                        dd.Rows.Add(co, a.IMEI1, a.IMEI2, a.IMEI3, a.IMEI4, a.IMEI5,a.PrintTime,a.Template,a.RePrintNum,a.ReFirstPrintTime,a.ReEndPrintTime);
                        co++;
                    }
                    dataGridView1.DataSource = dd;
                    dataGridView1.Columns[0].Width = 20;
                    dataGridView1.Columns[1].Width = 120;
                    dataGridView1.Columns[2].Width = 120;
                    dataGridView1.Columns[3].Width = 150;
                    dataGridView1.Columns[4].Width = 150;
                    dataGridView1.Columns[5].Width = 150;
                    dataGridView1.Columns[6].Width = 150;
                    dataGridView1.Columns[7].Width = 150;
                    dataGridView1.Columns[8].Width = 100;
                    dataGridView1.Columns[9].Width = 150;
                    dataGridView1.Columns[10].Width = 150;
                }
            }
            else
            {
                this.dataGridView1.DataSource = "";
            }
        }

        //打开模板按钮
        private void OpenTemplate_Click(object sender, EventArgs e)
        {
            if (this.Select_Template.Text == "")
            {
                player.Play();
            }
            else
            {
                string path = this.Select_Template.Text;
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

        //调试打印
        private void Debug_Print_Click(object sender, EventArgs e)
        {
            //判断导入Excel没
            if (this.ImportPath.Text == "")
            {
                player.Play();
                this.remined.AppendText("请先导入Excel\r\n");
                return;
            }
            //判断模板是否为空
            if (this.Select_Template.Text == "")
            {
                player.Play();
                this.remined.AppendText("请先选择模板\r\n");
                return;
            }
            string strExtension = this.ImportPath.Text.Substring(this.ImportPath.Text.LastIndexOf('.'));
            DataTable dt = IEB.GetExcelDatatable(this.ImportPath.Text, strExtension);
            DataRow dr3 = dt.Rows[1];
            LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
            //指定打印机名称
            btFormat.PrintSetup.PrinterName = this.Printer.Text;
            //对模板相应字段进行赋值
            btFormat.SubStrings["IMEI1"].Value = dr3[0].ToString();
            btFormat.SubStrings["IMEI2"].Value = dr3[1].ToString();
            btFormat.SubStrings["IMEI3"].Value = dr3[2].ToString();
            btFormat.SubStrings["IMEI4"].Value = dr3[3].ToString();
            btFormat.SubStrings["IMEI5"].Value = dr3[4].ToString();
            btFormat.SubStrings["ProductDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
            //打印份数,同序列打印的份数
            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
            btFormat.Print();
            Form1.Log("Excel调试打印了机身贴IMEI号为" + dr3[1].ToString() + "的制单", null);
        }

        //刷新模板
        private void RefeshTemplate_Click(object sender, EventArgs e)
        {
            if (this.Select_Template.Text != "")
            {
                string path = this.Select_Template.Text;
                string filename = Path.GetFileName(path);
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Excel模板"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Excel模板");
                }
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Excel模板\\" + RefreshNum))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Excel模板\\" + RefreshNum);
                }
                File.Copy(path, AppDomain.CurrentDomain.BaseDirectory + "\\Excel模板\\" + RefreshNum + "\\" + filename, true);
                lj = AppDomain.CurrentDomain.BaseDirectory + "\\Excel模板\\" + RefreshNum + "\\" + filename;
                this.remined.AppendText("刷新模板成功\r\n");
                RefreshNum++;
            }
        }

        //重打查询按钮函数
        private void RePrintCheck_Click(object sender, EventArgs e)
        {
            mepp = MEPPB.SelectAllRePrintBLL();
            int co = 1;
            if (mepp.Count == 0)
            {
                player.Play();
                this.remined.AppendText("无重打记录\r\n");
            }
            else
            {
                DataTable dd = new DataTable();
                dd.Columns.Add(" ");
                dd.Columns.Add("IMEI1");
                dd.Columns.Add("IMEI2");
                dd.Columns.Add("IMEI3");
                dd.Columns.Add("IMEI4");
                dd.Columns.Add("IMEI5");
                dd.Columns.Add("打印时间");
                dd.Columns.Add("打印模板");
                dd.Columns.Add("重打次数");
                dd.Columns.Add("首次重打时间");
                dd.Columns.Add("末次重打时间");
                foreach (ManuExcelPrintParam a in mepp)
                {
                    dd.Rows.Add(co, a.IMEI1, a.IMEI2, a.IMEI3, a.IMEI4, a.IMEI5, a.PrintTime, a.Template, a.RePrintNum, a.ReFirstPrintTime, a.ReEndPrintTime);
                    co++;
                }
                dataGridView1.DataSource = dd;
                dataGridView1.Columns[0].Width = 20;
                dataGridView1.Columns[1].Width = 120;
                dataGridView1.Columns[2].Width = 120;
                dataGridView1.Columns[3].Width = 150;
                dataGridView1.Columns[4].Width = 150;
                dataGridView1.Columns[5].Width = 150;
                dataGridView1.Columns[6].Width = 150;
                dataGridView1.Columns[7].Width = 150;
                dataGridView1.Columns[8].Width = 100;
                dataGridView1.Columns[9].Width = 150;
                dataGridView1.Columns[10].Width = 150;
            }
        }

        //重打按钮函数
        private void RePrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.ImportPath.Text == "")
                {
                    player.Play();
                    this.remined.AppendText("请先导入Excel\r\n");
                    return;
                }
                if (this.Select_Template.Text != "")
                {
                    if (IsNumeric(this.RowNumber.Text))
                    {
                        string strExtension = this.ImportPath.Text.Substring(this.ImportPath.Text.LastIndexOf('.'));
                        DataTable dt = IEB.GetExcelDatatable(this.ImportPath.Text, strExtension);
                        DataRow dr3 = dt.Rows[int.Parse(this.RowNumber.Text)];
                        if (MEPPB.CheckIMEIBLL(dr3[0].ToString(), dr3[1].ToString()))
                        {
                            LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                            //指定打印机名称
                            btFormat.PrintSetup.PrinterName = this.Printer.Text;
                            //对模板相应字段进行赋值
                            btFormat.SubStrings["IMEI1"].Value = dr3[0].ToString();
                            btFormat.SubStrings["IMEI2"].Value = dr3[1].ToString();
                            btFormat.SubStrings["IMEI3"].Value = dr3[2].ToString();
                            btFormat.SubStrings["IMEI4"].Value = dr3[3].ToString();
                            btFormat.SubStrings["IMEI5"].Value = dr3[4].ToString();
                            btFormat.SubStrings["ProductDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
                            //打印份数,同序列打印的份数
                            btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                            if (MEPPB.UpdateRePrintTimeBLL(dr3[0].ToString(), DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")))
                            {
                                btFormat.Print();
                                Form1.Log("Excel打印了机身贴IMEI号为" + dr3[1].ToString() + "的制单", null);
                            }
                        }
                        else
                        {
                            player.Play();
                            this.remined.AppendText(dr3[0].ToString() + "这条数据无需重打\r\n");
                        }
                    }
                    else if (IsTrue(this.RowNumber.Text))
                    {
                        LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                        //指定打印机名称
                        btFormat.PrintSetup.PrinterName = this.Printer.Text;
                        string[] range = this.RowNumber.Text.Split('-');
                        string strExtension = this.ImportPath.Text.Substring(this.ImportPath.Text.LastIndexOf('.'));
                        DataTable dt = IEB.GetExcelDatatable(this.ImportPath.Text, strExtension);
                        for (int i = int.Parse(range[0]); i <= int.Parse(range[1]); i++)
                        {
                            DataRow dr3 = dt.Rows[i];
                            if (MEPPB.CheckIMEIBLL(dr3[0].ToString(), dr3[1].ToString()))
                            {
                                //对模板相应字段进行赋值
                                btFormat.SubStrings["IMEI1"].Value = dr3[0].ToString();
                                btFormat.SubStrings["IMEI2"].Value = dr3[1].ToString();
                                btFormat.SubStrings["IMEI3"].Value = dr3[2].ToString();
                                btFormat.SubStrings["IMEI4"].Value = dr3[3].ToString();
                                btFormat.SubStrings["IMEI5"].Value = dr3[4].ToString();
                                btFormat.SubStrings["ProductDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
                                //打印份数,同序列打印的份数
                                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                                if (MEPPB.UpdateRePrintTimeBLL(dr3[0].ToString(), DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")))
                                {
                                    btFormat.Print();
                                    Form1.Log("Excel重打印了机身贴IMEI号为" + dr3[1].ToString() + "的制单", null);
                                }
                            }
                            else
                            {
                                player.Play();
                                this.remined.AppendText(dr3[0].ToString() + "这条数据无需重打\r\n");
                            }
                        }
                    }
                    else
                    {
                        player.Play();
                        this.remined.AppendText("请输入正确的行数格式：数字-数字\r\n");
                        this.RowNumber.Clear();
                        this.RowNumber.Focus();
                        this.dataGridView1.DataSource = "";
                    }
                }
                else
                {
                    player.Play();
                    this.remined.AppendText("请先选择模板\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception:" + ex.Message);
            }
        }

        //扫入IMEI1进行重打
        private void RePrintIMEI1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if(this.RePrintIMEI1.Text == "")
                {
                    return;
                }
                //是否已经选择了打印机
                if (this.Printer.Text == "")
                {
                    player.Play();
                    this.RePrintIMEI1.Clear();
                    this.RePrintIMEI1.Focus();
                    this.remined.AppendText("请选择打印机\r\n");
                    return;
                }
                //是否选择了模板
                if (this.Select_Template.Text == "")
                {
                    player.Play();
                    this.RePrintIMEI1.Clear();
                    this.RePrintIMEI1.Focus();
                    this.remined.AppendText("请选择模板\r\n");
                    return;
                }
                //判断是否打印过，没有打印过的不允许重打
                if (!MEPPB.CheckIMEI1BLL(this.RePrintIMEI1.Text))
                {
                    player.Play();
                    this.RePrintIMEI1.Clear();
                    this.RePrintIMEI1.Focus();
                    this.remined.AppendText(this.RePrintIMEI1.Text+"没有记录，无需重打\r\n");
                    return;
                }
                //打开模板
                LabelFormatDocument btFormat = btEngine.Documents.Open(lj);
                //指定打印机名称
                btFormat.PrintSetup.PrinterName = this.Printer.Text;
                mepp = MEPPB.SelectByImei1BLL(this.RePrintIMEI1.Text);
                foreach(ManuExcelPrintParam a in mepp)
                {
                    //对模板相应字段进行赋值
                    btFormat.SubStrings["IMEI1"].Value = a.IMEI1;
                    btFormat.SubStrings["IMEI2"].Value = a.IMEI2;
                    btFormat.SubStrings["IMEI3"].Value = a.IMEI3;
                    btFormat.SubStrings["IMEI4"].Value = a.IMEI4;
                    btFormat.SubStrings["IMEI5"].Value = a.IMEI5;
                    btFormat.SubStrings["ProductDate"].Value = DateTime.Now.ToString("yyyy.MM.dd");
                }
                //打印份数,同序列打印的份数
                btFormat.PrintSetup.IdenticalCopiesOfLabel = TN;
                if (MEPPB.UpdateRePrintTimeBLL(this.RePrintIMEI1.Text, DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")))
                {
                    btFormat.Print();
                    Form1.Log("Excel重打印了机身贴IMEI号为" + this.RePrintIMEI1.Text + "的制单", null);
                    this.RePrintIMEI1.Clear();
                    this.RePrintIMEI1.Focus();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string buttonText = this.button1.Name.ToString();
            this.button1.Enabled = false;
            if (buttonText == "锁定")
            {
                this.RePrint.Enabled = true;
                this.Debug_Print.Enabled = true;
                this.ExcelToPrint.Enabled = true;
                this.RowNumber.Enabled = true;
                this.button1.Enabled = true;
                this.button1.Name = "解锁";
            }
            else
            {
                this.RePrint.Enabled = false;
                this.Debug_Print.Enabled = false;
                this.ExcelToPrint.Enabled = false;
                this.RowNumber.Enabled = false;
                this.button1.Enabled = true;
                this.button1.Name = "锁定";
            }
        }

}
}
