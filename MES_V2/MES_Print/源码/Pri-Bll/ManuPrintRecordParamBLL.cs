using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Print_Message;
using PrintRecord.Param.DAL;

namespace ManuPrintRecord.Param.BLL
{
    class ManuPrintRecordParamBLL
    {
        PrintRecordParamDAL MPRPD = new PrintRecordParamDAL();

        public void refeshConBLL()
        {
            MPRPD.refreshCon();
        }

        public bool InsertPrintRecordParamBLL(string Zhidan, int SimMark, int VipMark, int BatMark, int IccidMark, int MacMark, int EquipmentMark, int RfidMark, int NoCheckMark, int NoPaperMark, int UpdataSimMark, int UpdateIMEIMark, int AutoTestMark, int CoupleMark, int WriteImeiMark, int ParamDownloadMark, int TemPlate1Num, int TemPlate2Num, int GpsMark
            , int CHCheckSnMark, int CHCheckSimMark, int CHCheckBatMark, int CHCheckIccidMark, int CHCheckMacMark, int CHCheckEquipmentMark, int CHCheckVipMark, int CHCheckRfidMark)
        {
            if (MPRPD.InsertPrintRecordParamDAL(Zhidan, SimMark,VipMark, BatMark, IccidMark, MacMark, EquipmentMark, RfidMark, NoCheckMark,  NoPaperMark,  UpdataSimMark,  UpdateIMEIMark,  AutoTestMark,  CoupleMark,  WriteImeiMark,  ParamDownloadMark,  TemPlate1Num,  TemPlate2Num, GpsMark,  CHCheckSnMark,  CHCheckSimMark,  CHCheckBatMark,  CHCheckIccidMark,  CHCheckMacMark,  CHCheckEquipmentMark,  CHCheckVipMark,  CHCheckRfidMark) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<ManuPrintRecordParam> selectRecordParamByzhidanBLL(string zhidan)
        {
            return MPRPD.selectRecordParamByzhidanDAL(zhidan);
        }


    }
}
