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

        //查关联表
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


        public bool CheckVIPBLL(string SIMnumber)
        {
            if (DRSD.CheckVIPDAL(SIMnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public bool CheckBATBLL(string SIMnumber)
        {
            if (DRSD.CheckBATDAL(SIMnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public bool CheckICCIDBLL(string SIMnumber)
        {
            if (DRSD.CheckICCIDDAL(SIMnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public bool CheckMACBLL(string SIMnumber)
        {
            if (DRSD.CheckMACDAL(SIMnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public bool CheckEquipmentBLL(string SIMnumber)
        {
            if (DRSD.CheckEquipmentDAL(SIMnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public bool CheckRFIDBLL(string SIMnumber)
        {
            if (DRSD.CheckRFIDDAL(SIMnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool CheckIMEIExitRFIDBLL(string IMEInumber)
        {
            if (DRSD.CheckIMEIExitRFIDDAL(IMEInumber) > 0)
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

        
         public bool UpdateVIP_SIM_OR_ICCIDBLL(string IMEI, string SIM, string VIP, string ICCID )
        {
            if (DRSD.UpdateVIP_SIM_OR_ICCIDDAL(IMEI, SIM,VIP, ICCID) > 0)
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

        public bool UpdateVipAndBatOrSIMOrOICCIDBLL(string IMEI, string SIM, string ICCID, string VIP, string BAT)
        {
            if (DRSD.UpdateVipAndBatOrSIMOrOICCIDDAL(IMEI, SIM, ICCID, VIP, BAT) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateIccidAndBatBLL(string IMEI, string VIP, string BAT)
        {
            if (DRSD.UpdateIccidAndBatDAL(IMEI, VIP, BAT) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateIccid_OrVipOrBatBLL(string IMEI, string VIP, string BAT,string ICCID)
        {
            if (DRSD.UpdateIccid_OrVipOrBatDAL( IMEI,  VIP,  BAT,  ICCID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSNDAL(string IMEI, string SN)
        {
            if (DRSD.UpdateSNDAL(IMEI, SN) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_RFIDDAL(string IMEI, string SN, string RFID)
        {
            if (DRSD.UpdateSN_RFIDDAL(IMEI, SN,RFID) > 0)
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

        public bool UpdateSimOrVipAndBatOrIccidAndMacBLL(string IMEI, string SIM, string VIP, string BAT, string ICCID, string MAC)
        {
            if (DRSD.UpdateSimOrVipAndBatOrIccidAndMacDAL( IMEI,  SIM,  VIP,  BAT,  ICCID,  MAC) > 0)
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


        public bool UpdateSIMOrVipAndBatOrICCIDAndMacAndEquBLL(string IMEI, string SIM, string VIP, string BAT, string ICCID, string MAC, string Equipment)
        {
            if (DRSD.UpdateSIMOrVipAndBatOrICCIDAndMacAndEquDAL( IMEI,  SIM,  VIP,  BAT,  ICCID,  MAC,  Equipment) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public bool UpdateVipAndBatAndMacAndEquAndRFIDDAL(string IMEI, string VIP, string BAT, string MAC, string Equipment, string RFID)
        //{
        //    if (DRSD.UpdateVipAndBatAndMacAndEquAndRFIDDAL(IMEI, VIP, BAT, MAC, Equipment, RFID) > 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        public bool UpdateVipAndBatAndMacAndEquAndRFIDDAL(string IMEI, string SIM, string VIP, string BAT, string ICCID, string MAC, string Equipment, string RFID)
        {
            if (DRSD.UpdateVipAndBatAndMacAndEquAndRFIDDAL(IMEI, SIM ,VIP, BAT, ICCID, MAC, Equipment, RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool UpdateIMEI14DAL(string IMEI,  string IMEI14)
        {
            if (DRSD.UpdateIMEI14DAL(IMEI, IMEI14) > 0)
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

        public bool InsertRelativeSheetBLL(DataRelativeSheet list)
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

        public string SelectIMEIFieldBLL(string IMEI2Value)
        {
            return DRSD.SelectIMEIFieldDALL(IMEI2Value);
        }

        //字段查询所用字段
        public List<DataRelativeSheet> SelectAllFieldBLL(string FieldStr)
        {
            return DRSD.SelectAllFieldBLL(FieldStr);
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


        public bool UpdateDRSFromExcelBLL(string ExcelSql)
        {
            if (DRSD.UpdateDRSFromExcelDAL(ExcelSql) > 0)
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
