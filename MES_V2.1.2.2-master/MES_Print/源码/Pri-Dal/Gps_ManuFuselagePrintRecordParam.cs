using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Print_Message
{
    class ManuFuselagePrintRecordParam
    {

        //1制单号
        public string ZhiDan { get; set; }

        //逐个打印
        public int PrintOneByOne { get; set; }

        //批量打印
        public int Pltplot { get; set; }

        //客供SN
        public int CustomerSupplySN  { get; set; }

        //不打印校验码
        public int NoPrintCheckCode { get; set; }

        //不打印SN
        public int NoPrintingSN { get; set; }

        //IMEI十六进制
        public int IMEIHexadecimal { get; set; }

        //SN十六进制
        public int SNHexadecimal { get; set; }

        //逐个重打
        public int ReplayOneByOne  { get; set; }

        //批量重打
        public int BattingInBatches  { get; set; }

        //无校验位
        public int NoParityBit { get; set; }

        //十六进制
        public int Hexadecimal { get; set; }

    }
}
