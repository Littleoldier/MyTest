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

        public bool InsertPrintRecordParamBLL(string Zhidan, int SimMark, int VipMark, int BatMark, int IccidMark, int MacMark, int EquipmentMark, int NoCheckMark, int NoPaperMark, int UpdataSimMark, int UpdateIMEIMark, int AutoTestMark, int CoupleMark, int WriteImeiMark, int ParamDownloadMark, int TemPlate1Num, int TemPlate2Num, int GpsMark)
        {
            if (MPRPD.InsertPrintRecordParamDAL(Zhidan, SimMark,VipMark, BatMark, IccidMark, MacMark, EquipmentMark, NoCheckMark,  NoPaperMark,  UpdataSimMark,  UpdateIMEIMark,  AutoTestMark,  CoupleMark,  WriteImeiMark,  ParamDownloadMark,  TemPlate1Num,  TemPlate2Num, GpsMark) > 0)
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
