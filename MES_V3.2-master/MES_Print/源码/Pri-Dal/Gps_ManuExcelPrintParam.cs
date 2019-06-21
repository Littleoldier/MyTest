using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Print_Message
{
    class ManuExcelPrintParam
    {
        //SN
        public int ID { get; set; }

        public string IMEI1 { get; set; }

        public string IMEI2 { get; set; }

        public string IMEI3 { get; set; }

        public string IMEI4 { get; set; }

        public string IMEI5 { get; set; }

        //打印时间
        public string PrintTime { get; set; }

        //模板
        public string Template { get; set; }

        //重打次数
        public int RePrintNum { get; set; }

        //首次重打时间
        public string ReFirstPrintTime { get; set; }

        //末次重打时间
        public string ReEndPrintTime { get; set; }
    }
}
