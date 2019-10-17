using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Print_Message
{
    class DataRelativeSheet
    {
        //SN
        public string SN { get; set; }

        //IMEI号
        public string IMEI1 { get; set; }

        //SN
        public string IMEI2 { get; set; }

        //SIM卡号
        public string IMEI3 { get; set; }

        //ICCID号
        public string IMEI4 { get; set; }

        //密码
        public string IMEI5 { get; set; }

        //MAC蓝牙号
        public string IMEI6 { get; set; }

        //Equipment设备号
        public string IMEI7 { get; set; }

        //VIP服务卡号
        public string IMEI8 { get; set; }

        //BAT电池号
        public string IMEI9 { get; set; }

        //第二个锁ID
        public string IMEI10 { get; set; }

        //
        public string IMEI11 { get; set; }

        //
        public string IMEI12 { get; set; }

        //制单号
        public string ZhiDan { get; set; }

        //测试时间
        public string TestTime { get; set; }

        //RFID
        public string RFID { get; set; }        
        
        
        //IMEI14
        public string IMEI14 { get; set; }


        public void Claer()
        {
             //SN
             SN = "";

            //IMEI号
             IMEI1 = "";

            //SN
             IMEI2 = "";

            //SIM卡号
             IMEI3 = "";

            //ICCID号
             IMEI4 = "";

            //密码
             IMEI5 = "";

            //MAC蓝牙号
             IMEI6 = "";

            //Equipment设备号
             IMEI7 = "";

            //VIP服务卡号
             IMEI8 = "";

            //BAT电池号
             IMEI9 = "";

            //第二个锁ID
             IMEI10 = "";

            //
             IMEI11 = "";

            //
             IMEI12 = "";

            //制单号
             ZhiDan = "";

            //测试时间
             TestTime = "";

            //IMEI13
             RFID = "";

            //IMEI14
            IMEI14 = "";
         }
    }
}
