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

        public int JSRelationSnMark { get; set; }

        public int JSRelationSimMark { get; set; }

        public int JSRelationBatMark { get; set; }

        public int JSRelationIccidMark { get; set; }

        public int JSRelationMacMark { get; set; }

        public int JSRelationEquipmentMark { get; set; }

        public int JSRelationVipMark { get; set; }

        public int JSRelationRfidMark { get; set; }

        public int JSCheckSnMark { get; set; }
        public int JSCheckSimMark { get; set; }

        public int JSCheckBatMark { get; set; }

        public int JSCheckIccidMark { get; set; }

        public int JSCheckMacMark { get; set; }

        public int JSCheckEquipmentMark { get; set; }

        public int JSCheckVipMark { get; set; }

        public int JSCheckRfidMark { get; set; }

        public int PrintMode1 { get; set; }

        public int PrintMode2 { get; set; }

        public int JSCheckIMEI2Mark { get; set; }

        public int JSCheckInseIMEI2Mark { get; set; }

        public void clear()
        {
                    //逐个打印
            PrintOneByOne =0;

            //批量打印
            Pltplot =0;

            //客供SN
            CustomerSupplySN =0;

            //不打印校验码
            NoPrintCheckCode =0;

            //不打印SN
            NoPrintingSN =0;

            //IMEI十六进制
            IMEIHexadecimal =0;

            //SN十六进制
            SNHexadecimal =0;

            //逐个重打
            ReplayOneByOne =0;

            //批量重打
            BattingInBatches =0;

            //无校验位
            NoParityBit =0;

            //十六进制
            Hexadecimal =0;

            JSRelationSnMark =0;

            JSRelationSimMark =0;

            JSRelationBatMark =0;

            JSRelationIccidMark =0;

            JSRelationMacMark =0;

            JSRelationEquipmentMark =0;

            JSRelationVipMark =0;

            JSRelationRfidMark =0;

            JSCheckSnMark =0;
            JSCheckSimMark =0;

            JSCheckBatMark =0;

            JSCheckIccidMark =0;

            JSCheckMacMark =0;

            JSCheckEquipmentMark =0;

            JSCheckVipMark =0;

            JSCheckRfidMark =0;

            PrintMode1 =0;

            PrintMode2 =0;

            JSCheckIMEI2Mark =0;

            JSCheckInseIMEI2Mark =0;
         }
    
    }
}
