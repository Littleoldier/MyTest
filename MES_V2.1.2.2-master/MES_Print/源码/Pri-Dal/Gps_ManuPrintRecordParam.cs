using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Print_Message
{
    public class ManuPrintRecordParam
    {
        //0唯一标示符
        public int ID { get; set; }

        //1制单号
        public string ZhiDan { get; set; }

        //2Sim号标志位
        public int SimMark { get; set; }

        //3Vip号标志位
        public int VipMark { get; set; }

        //4BAT号标志位
        public int BatMark { get; set; }

        //5ICCID号标志位
        public int IccidMark { get; set; }

        //6MAC号标志位
        public int MacMark { get; set; }

        //7EQUIPMENT号标志位
        public int EquipmentMark { get; set; }

        //8有无检验位标志位
        public int NoCheckMark { get; set; }

        //9是否出贴纸标志位
        public int NoPaperMark { get; set; }

        //10更新Sim号根据IMEI标志位
        public int UpdataSimMark { get; set; }

        //11更新IMEI根据Sim标志位
        public int UpdateIMEIMark { get; set; }

        //12功能1检测标志位
        public int AutoTestMark { get; set; }

        //13耦合位检测标志位
        public int CoupleMark { get; set; }

        //14写号检测标志位
        public int WriteImeiMark { get; set; }

        //15参数下载检测标志位
        public int ParamDownloadMark { get; set; }

        //16模板1贴纸份数标志位
        public int TemPlate1Num { get; set; }

        //17模板2贴纸份数标志位
        public int TemPlate2Num { get; set; }

        //18功能2检测标志位
        public int GPSMark { get; set; }

    }
}
