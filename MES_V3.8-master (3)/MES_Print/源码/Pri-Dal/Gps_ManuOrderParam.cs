using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Print_Message
{
    public class Gps_ManuOrderParam
    {
        //制单号
        public string ZhiDan { get; set; }

        //软件机型
        public string SoftModel { get; set; }

        //SN固定前缀
        public string SN1 { get; set; }

        //SN2--机身贴后缀
        public string SN2 { get; set; }

        //SN3--彩盒贴后缀
        public string SN3 { get; set; }

        //盒子号1
        public string Box_No1 { get; set; }

        //盒子号2
        public string Box_No2 { get; set; }

        //生产日期
        public string ProductDate { get; set; }

        //颜色
        public string Color { get; set; }

        //重量
        public string Weight { get; set; }

        //Qty
        public string Qty { get; set; }

        //生产号
        public string ProductNo { get; set; }

        //版本
        public string Version { get; set; }

        //IMEI起始位
        public string IMEIStart { get; set; }

        //IMEI终止位
        public string IMEIEnd { get; set; }

        //SIM起始位
        public string SIMStart { get; set; }

        //SIM终止位
        public string SIMEnd { get; set; }

        //BAT起始位
        public string BATStart { get; set; }

        //BAT终止位
        public string BATEnd { get; set; }

        //VIP起始位
        public string VIPStart { get; set; }

        //VIP终止位
        public string VIPEnd { get; set; }

        //绑定关系
        public string IMEIRel { get; set; }

        //备注
        public string Remark1 { get; set; }

        //备注5
        public string Remark5 { get; set; }

        //制单状态
        public int status { get; set; }

        //机身贴模板
        public string JST_template { get; set; }

        //彩盒贴模板1
        public string CHT_template1 { get; set; }

        //彩盒贴模板2
        public string CHT_template2 { get; set; }

        //BAT前缀
        public string BAT_prefix { get; set; }

        //BAT位数
        public string BAT_digits { get; set; }

        //SIM前缀
        public string SIM_prefix { get; set; }

        //SIM位数
        public string SIM_digits { get; set; }

        //VIP前缀
        public string VIP_prefix { get; set; }

        //VIP位数
        public string VIP_digits { get; set; }

        //ICCID前缀
        public string ICCID_prefix { get; set; }

        //ICCID位数
        public string ICCID_digits { get; set; }

        //IMEI当前打印位
        public string IMEIPrints { get; set; }

        //蓝牙位数
        public string MAC_digits { get; set; }

        //蓝牙前缀
        public string MAC_prefix { get; set; }

        //设备号前缀
        public string Equipment_prefix { get; set; }

        //设备号位数
        public string Equipment_digits { get; set; }

        //RFID起始位
        public string RFIDStart { get; set; }

        //RFID终止位
        public string RFIDEnd { get; set; }

        //RFID号前缀
        public string RFID_prefix { get; set; }

        //RFID号位数
        public string RFID_digits { get; set; }

        //IMEI2起始位
        public string IMEI2Start { get; set; }

        //IMEI2终止位
        public string IMEI2End { get; set; }


        //IMEI2当前打印位
        public string IMEI2Prints { get; set; }

        //2绑定关系
        public string IMEI2Rel { get; set; }    

        //周期
        public int Week { get; set; }

        //IMEIrangeNum
        public string IMEIRangeNum { get; set; }


        public void claer()
        {
                        //软件机型
             SoftModel = "";

            //SN固定前缀
             SN1 = "";

            //SN2--机身贴后缀
             SN2 = "";

            //SN3--彩盒贴后缀
             SN3 = "";

            //盒子号1
             Box_No1 = "";

            //盒子号2
             Box_No2 = "";

            //生产日期
             ProductDate = "";

            //颜色
             Color = "";

            //重量
             Weight = "";

            //Qty
             Qty = "";

            //生产号
             ProductNo = "";

            //版本
             Version = "";

            //IMEI起始位
             IMEIStart = "";

            //IMEI终止位
             IMEIEnd = "";

            //SIM起始位
             SIMStart = "";

            //SIM终止位
             SIMEnd = "";

            //BAT起始位
             BATStart = "";

            //BAT终止位
             BATEnd = "";

            //VIP起始位
             VIPStart = "";

            //VIP终止位
             VIPEnd = "";

            //绑定关系
             IMEIRel = "";

            //备注
             Remark1 = "";

            //备注5
             Remark5 = "";

            //制单状态
             status = 0;

            //机身贴模板
             JST_template = "";

            //彩盒贴模板1
             CHT_template1 = "";

            //彩盒贴模板2
             CHT_template2 = "";

            //BAT前缀
             BAT_prefix = "";

            //BAT位数
             BAT_digits = "";

            //SIM前缀
             SIM_prefix = "";

            //SIM位数
             SIM_digits = "";

            //VIP前缀
             VIP_prefix = "";

            //VIP位数
             VIP_digits = "";

            //ICCID前缀
             ICCID_prefix = "";

            //ICCID位数
             ICCID_digits = "";

            //IMEI当前打印位
             IMEIPrints = "";

            //蓝牙位数
             MAC_digits = "";

            //蓝牙前缀
             MAC_prefix = "";

            //设备号前缀
             Equipment_prefix = "";

            //设备号位数
             Equipment_digits = "";

            //RFID起始位
             RFIDStart = "";

            //RFID终止位
             RFIDEnd = "";

            //RFID号前缀
             RFID_prefix = "";

            //RFID号位数
             RFID_digits = "";

            //IMEI2起始位
             IMEI2Start = "";

            //IMEI2终止位
             IMEI2End = "";


            //IMEI2当前打印位
             IMEI2Prints = "";

            //2绑定关系
             IMEI2Rel = "";

            //周期
            Week = -1;

            IMEIRangeNum = "";
       }
    }
}
