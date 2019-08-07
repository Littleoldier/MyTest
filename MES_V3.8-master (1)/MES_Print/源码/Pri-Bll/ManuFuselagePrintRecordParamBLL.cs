using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Print_Message;
using FuselagePrintRecord.Param.DAL;

namespace ManuFuselagePrintRecord.Param.BLL
{
    class ManuFuselagePrintRecordParamBLL
    {
        FuselagePrintRecordParamDAL FPRPD = new FuselagePrintRecordParamDAL();

        public void refeshConBLL()
        {
            FPRPD.refreshCon();
        }

        public bool InsertPrintRecordParamBLL(string Zhidan, int PrintOneByOne, int Pltplot, int CustomerSupplySN, int NoPrintCheckCode, int NoPrintingSN, int IMEIHexadecimal, int SNHexadecimal, int ReplayOneByOne, int BattingInBatches, int NoParityBit, int Hexadecimal,
             int JSCheckSnMark, int JSCheckSimMark, int JSCheckBatMark, int JSCheckIccidMark, int JSCheckMacMark, int JSCheckEquipmentMark, int JSCheckVipMark, int JSCheckRfidMark ,int PrintMode1,int PrintMode2, int JSCheckIMEI2Mark, int JSCheckInseIMEI2Mark)
        {
            if (FPRPD.InsertFuselagePrintRecordParamDAL(Zhidan, PrintOneByOne, Pltplot, CustomerSupplySN, NoPrintCheckCode, NoPrintingSN, IMEIHexadecimal, SNHexadecimal, ReplayOneByOne, BattingInBatches, NoParityBit, Hexadecimal,
              JSCheckSnMark,  JSCheckSimMark,  JSCheckBatMark,  JSCheckIccidMark,  JSCheckMacMark,  JSCheckEquipmentMark,  JSCheckVipMark,  JSCheckRfidMark,  PrintMode1,  PrintMode2,  JSCheckIMEI2Mark,  JSCheckInseIMEI2Mark) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ManuFuselagePrintRecordParam selectRecordParamByzhidanBLL(string zhidan)
        {
            return FPRPD.selectFuselageRecordParamByzhidanDAL(zhidan);
        }
    }
}
