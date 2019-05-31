using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataRelative.Param.DAL;
using Print_Message;

namespace DataRelative.Param.BLL
{
    class DataRelativeSheetBLL
    {
        DataRelativeSheetDAL DRSD = new DataRelativeSheetDAL();

        public void refeshConBLL()
        {
            DRSD.refreshCon();
        }

        public List<DataRelativeSheet> SelectByIMEIBLL(string IMEI1)
        {
            return DRSD.SelectByImeiDAL(IMEI1);
        }

        public string SelectIccidBySimBLL(string SIM,string G_zhidan)
        {
            return DRSD.SelectIccidBySimDAL(SIM, G_zhidan);
        }

        public string SelectZhidanBySimBLL(string SIM)
        {
            return DRSD.SelectzhidanBySimDAL(SIM);
        }

        public string SelectSNByImeiBLL(string IMEI)
        {
            return DRSD.SelectSNByImeiDAL(IMEI);
        }

        public string SelectGLBSNByImeiBLL(string IMEI)
        {
            return DRSD.SelectGLBSNByImeiDAL(IMEI);
        }

        public bool CheckIMEIBLL(string IMEInumber)
        {
            if (DRSD.CheckIMEIDAL(IMEInumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<DataRelativeSheet> GetCheckIMEIBLL(string IMEInumber)
        {
            return DRSD.GetCheckIMEIDAL(IMEInumber);

        }

        public bool CheckSIMBLL(string SIMnumber)
        {
            if (DRSD.CheckSIMDAL(SIMnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string CheckSIMByIMEIBLL(string IMEI)
        {
            return DRSD.CheckSIMByIMEIDAL(IMEI);
         }

        public bool UpdateIMEIBySIMBLL(string IMEI, string SIM)
        {
            if (DRSD.UpdateIMEIBySIMDAL(IMEI, SIM) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSIMByIMEIBLL(string IMEI, string SIM,string ICCID)
        {
            if (DRSD.UpdateSIMByIMEIDAL(IMEI, SIM, ICCID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSIMByIMEIBLL(string IMEI, string SIM)
        {
            if (DRSD.UpdateSIMByIMEIDAL(IMEI, SIM) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateAssociatedBLL(string IMEI, string VIP, string BAT, string MAC, string Equipment)
        {
            if (DRSD.UpdateAssociatedDAL(IMEI, VIP, BAT, MAC, Equipment) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateVIPBLL(string IMEI, string VIP)
        {
            if (DRSD.UpdateVIPDAL(IMEI, VIP) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateVipAndBatBLL(string IMEI, string VIP, string BAT)
        {
            if (DRSD.UpdateVipAndBatDAL(IMEI, VIP, BAT) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateVipAndBatAndMacBLL(string IMEI, string VIP, string BAT, string MAC)
        {
            if (DRSD.UpdateVipAndBatAndMacDAL(IMEI, VIP, BAT,MAC) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateVipAndBatAndMacAndEquBLL(string IMEI, string VIP, string BAT, string MAC, string Equipment)
        {
            if (DRSD.UpdateVipAndBatAndMacAndEquDAL(IMEI, VIP, BAT, MAC, Equipment) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InsertRelativeSheetBLL(List<DataRelativeSheet> list)
        {
            if (DRSD.InsertRelativeSheetDAL(list) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string SelectIMEIBySnOrIMEI2BLL(string IMEI2Value)
        {
            return DRSD.SelectIMEIBySnOrIMEI2DAL(IMEI2Value);
        }

        public bool InsertRSFromExcelBLL(string ExcelSql)
        {
            if (DRSD.InsertRSFromExcelDAL(ExcelSql) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
