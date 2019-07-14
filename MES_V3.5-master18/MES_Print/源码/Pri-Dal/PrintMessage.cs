using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Print_Message
{
    public class PrintMessage
    {
        //0唯一标示符
        public int ID { get; set; }

        //1制单号
        public string Zhidan { get; set; }

        //2IMEI号
        public string IMEI { get; set; }

        //3IMEI起始位
        public string IMEIStart { get; set; }

        //4IMEI终止位
        public string IMEIEnd { get; set; }

        //5SN号
        public string SN { get; set; }

        //6绑定类型
        public string IMEIRel { get; set; }

        //7SIM号
        public string SIM { get; set; }

        //8VIP号
        public string VIP { get; set; }

        //9BAT号
        public string BAT { get; set; }

        //10机型
        public string SoftModel { get; set; }

        //11版本
        public string Version { get; set; }

        //12备注
        public string Remark { get; set; }

        //13机身打印时间
        public string JS_PrintTime { get; set; }

        //14机身模板路径
        public string JS_TemplatePath { get; set; }

        //15机身重打次数
        public int JS_RePrintNum { get; set; }

        //16机身首次重打时间
        public string JS_ReFirstPrintTime { get; set; }

        //17机身最后重打时间
        public string JS_ReEndPrintTime { get; set; }

        //18用户
        public string UserName { get; set; }

        //19彩盒打印时间
        public string CH_PrintTime { get; set; }

        //彩盒模板1
        public string CH_TemplatePath1 { get; set; }

        //彩盒模板2
        public string CH_TemplatePath2 { get; set; }

        //彩盒重打次数
        public string CH_RePrintNum { get; set; }

        //彩盒首次重打时间
        public string CH_ReFirstPrintTime { get; set; }

        //彩盒末次重打时间
        public string CH_ReEndPrintTime { get; set; }

        //7SIM号
        public string ICCID { get; set; }

        //7SIM号
        public string MAC { get; set; }

        //7SIM号
        public string Equipment { get; set; }

        //RFID号
        public string RFID { get; set; }        
        
        //彩盒用户
        public string JSUserName { get; set; }

        //机身用户
        public string CHUserName { get; set; }



        public string IMEI2 { get; set; }


        public string IMEI2Start { get; set; }



        public string IMEI2End { get; set; }


        //public string IMEI2Rel { get; set; }

        //彩盒用户姓名
        public string JSUserDes { get; set; }

        //机身用户姓名
        public string CHUserDes { get; set; }



        public void Claer()
        {
             //1制单号
            Zhidan = "";

            //2IMEI号
            IMEI = "";

            //3IMEI起始位
            IMEIStart = "";

            //4IMEI终止位
            IMEIEnd = "";

            //5SN号
            SN = "";

            //6绑定类型
            IMEIRel = "";

            //7SIM号
            SIM = "";

            //8VIP号
            VIP = "";

            //9BAT号
            BAT = "";

            //10机型
            SoftModel = "";

            //11版本
            Version = "";

            //12备注
            Remark = "";

            //13机身打印时间
            JS_PrintTime = "";

            //14机身模板路径
            JS_TemplatePath = "";

            //15机身重打次数
            JS_RePrintNum = 0;

            //16机身首次重打时间
            JS_ReFirstPrintTime = "";

            //17机身最后重打时间
            JS_ReEndPrintTime = "";

            //18用户
            UserName = "";

            //19彩盒打印时间
            CH_PrintTime = "";

            //彩盒模板1
            CH_TemplatePath1 = "";

            //彩盒模板2
            CH_TemplatePath2 = "";

            //彩盒重打次数
            CH_RePrintNum = "";

            //彩盒首次重打时间
            CH_ReFirstPrintTime = "";

            //彩盒末次重打时间
            CH_ReEndPrintTime = "";

            //7SIM号
            ICCID = "";

            //7SIM号
            MAC = "";

            //7SIM号
            Equipment = "";

            //RFID号
            RFID = "";

            //彩盒用户
            JSUserName = "";

            //机身用户
            CHUserName = "";

            IMEI2 = "";
            
            IMEI2Start = "";

            IMEI2End = "";

            //IMEI2Rel = "";


            //彩盒用户姓名
            JSUserDes = "";

            //机身用户姓名
            CHUserDes = "";

       }

    }
}
