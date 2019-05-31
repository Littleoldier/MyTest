using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Print_Message;
using Print.Message.Dal;

namespace Print.Message.Bll
{
    public class PrintMessageBLL
    {
        PrintMessageDAL PMD = new PrintMessageDAL();

        public void refeshConBLL()
        {
            PMD.refreshCon();
        }

        public bool InsertPrintMessageBLL(List<PrintMessage> list) {
            if (PMD.InsertPrintMessageDAL(list) > 0)
            {
                return true;
            }
            else {
                return false;
            }
        }

        public bool CheckCHOrJSIMEIBLL(string IMEInumber, int PrintType)
        {
            if (PMD.CheckCHOrJSIMEIDAL(IMEInumber,PrintType) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_SIM_ICCIDBLL(string IMEI, string CHPrintTime, string lj1, string lj2, string SIM, string ICCID, string SN, string zhidan, string RFID)
        {
            if (PMD.UpdateSN_SIM_ICCIDDAL(IMEI, CHPrintTime, lj1, lj2, SIM, ICCID, SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_VIPBLL(string IMEI, string CHPrintTime, string lj1, string lj2, string VIP, string SN,string zhidan, string RFID)
        {
            if (PMD.UpdateSN_VIPDAL(IMEI, CHPrintTime, lj1, lj2, VIP, SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_SIM_VIP_ICCIDBLL(string IMEI, string CHPrintTime, string lj1, string lj2, string SIM, string VIP, string ICCID, string SN,string zhidan, string RFID)
        {
            if (PMD.UpdateSN_SIM_VIP_ICCIDDAL(IMEI, CHPrintTime, lj1, lj2, SIM, VIP,ICCID, SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_SIM_VIP_BAT_ICCIDBLL(string IMEI, string CHPrintTime, string lj1, string lj2, string SIM, string VIP, string BAT, string ICCID, string SN,string zhidan, string RFID)
        {
            if (PMD.UpdateSN_SIM_VIP_BAT_ICCIDDAL(IMEI, CHPrintTime, lj1, lj2, SIM, VIP, BAT, ICCID, SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_VIP_BAT_ICCIDBLL(string IMEI, string CHPrintTime, string lj1, string lj2, string VIP, string BAT, string ICCID, string SN, string zhidan, string RFID)
        {
            if (PMD.UpdateSN_VIP_BAT_ICCIDDAL(IMEI, CHPrintTime, lj1, lj2, VIP, BAT, ICCID, SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_SIM_VIP_BAT_ICCID_MACBLL(string IMEI, string CHPrintTime, string lj1, string lj2, string SIM, string VIP, string BAT, string ICCID, string MAC, string SN, string zhidan, string RFID)
        {
            if (PMD.UpdateSN_SIM_VIP_BAT_ICCID_MACDAL(IMEI, CHPrintTime, lj1, lj2, SIM, VIP, BAT, ICCID,MAC, SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentBLL(string IMEI, string CHPrintTime, string lj1, string lj2, string SIM, string VIP, string BAT, string ICCID, string MAC,string Equipment, string SN,string zhidan, string RFID)
        {
            if (PMD.UpdateSN_SIM_VIP_BAT_ICCID_MAC_EquipmentDAL(IMEI, CHPrintTime, lj1, lj2, SIM, VIP, BAT, ICCID, MAC,Equipment, SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSimIccidBLL(string IMEI, string SIM,string ICCID)
        {
            if (PMD.UpdateSimIccidDAL(IMEI, SIM, ICCID) > 0)
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
            if (PMD.UpdateVIPDAL(IMEI,VIP) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSimVipIccidBLL(string IMEI, string SIM, string VIP, string ICCID)
        {
            if (PMD.UpdateSimVipIccidDAL(IMEI, SIM, VIP, ICCID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateVipAndBatBLL(string IMEI, string SIM, string VIP, string BAT, string ICCID)
        {
            if (PMD.UpdateVipAndBatDAL(IMEI, SIM, VIP, BAT, ICCID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateIccidAndBatBLL(string IMEI, string SIM, string VIP, string BAT, string ICCID)
        {
            if (PMD.UpdateVipAndBatDAL(IMEI, SIM, VIP, BAT, ICCID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateVipAndBatAndMacBLL(string IMEI, string SIM, string VIP, string BAT, string ICCID,string MAC)
        {
            if (PMD.UpdateVipAndBatAndMacDAL(IMEI, SIM, VIP, BAT, ICCID,MAC) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateVipAndBatAndMacAndEquBLL(string IMEI, string SIM, string VIP, string BAT, string ICCID, string MAC, string Equipment)
        {
            if (PMD.UpdateVipAndBatAndMacAndEquDAL(IMEI, SIM, VIP, BAT, ICCID, MAC,Equipment) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckReCHOrJSIMEIBLL(string IMEInumber, int PrintType)
        {
            if (PMD.CheckReCHOrJSIMEIDAL(IMEInumber, PrintType) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int CheckReJSRangeIMEIBLL(string IMEIStart, string IMEIEnd)
        {
            return PMD.CheckReJSRangeIMEIDAL(IMEIStart, IMEIEnd);
        }

        public bool CheckIMEIBLL(string IMEInumber)
        {
            if (PMD.CheckIMEIDAL(IMEInumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<PrintMessage> CheckRangeIMEIBLL(string StarIMEI,string EndIMEI)
        {
            return PMD.CheckRangeIMEIDAL(StarIMEI,EndIMEI);
        }

        public bool CheckSNBLL(string SNnumber)
        {
            if (PMD.CheckSNDAL(SNnumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckSIMBLL(string SIM)
        {
            if (PMD.CheckSIMDAL(SIM) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckVIPBLL(string VIP)
        {
            if (PMD.CheckVIPDAL(VIP) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckBATBLL(string BAT)
        {
            if (PMD.CheckBATDAL(BAT) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckICCIDBLL(string ICCID)
        {
            if (PMD.CheckICCIDDAL(ICCID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckMACBLL(string MAC)
        {
            if (PMD.CheckMACDAL(MAC) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckEquipmentBLL(string Equipment)
        {
            if (PMD.CheckEquipmentDAL(Equipment) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<PrintMessage> SelectSnByIMEIBLL(string IMEInumber) {
            return PMD.SelectSnByIMEIDAL(IMEInumber);
        }

        public PrintMessage SelectSnByIMEIBLL(string IMEInumber, int NULLint)
        {
            return PMD.SelectSnByIMEIDAL(IMEInumber , NULLint);
        }

        public string SelectOnlySnByIMEIBLL(string IMEInumber)
        {
            return PMD.SelectOnlySnByIMEIDAL(IMEInumber);
        }


        public bool UpdateRePrintBLL(string IMEInumber,string RePrintTime,int PrintType,string lj,string lj1)
        {
            if (PrintType == 1)
            {
                if (PMD.SelectJS_RePrintNumByIMEIDAL(IMEInumber) == 0)
                {
                    if (PMD.UpdateRePrintDAL(IMEInumber, RePrintTime, lj) > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (PMD.UpdateReEndPrintDAL(IMEInumber, RePrintTime, lj) > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (PMD.SelectCH_RePrintNumByIMEIDAL(IMEInumber) == 0)
                {
                    if (PMD.UpdateCHRePrintDAL(IMEInumber, RePrintTime, lj,lj1) > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (PMD.UpdateCHReEndPrintDAL(IMEInumber, RePrintTime, lj,lj1) > 0)
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

        public List<PrintMessage> SelectAllReJSTBLL()
        {
            return PMD.SelectAllReJSTDAL();
        }

        public List<PrintMessage> SelectAllReCHTBLL()
        {
            return PMD.SelectAllReCHTDAL();
        }

        public List<PrintMessage> SelectReMesByZhiDanOrIMEIBLL(string InputNum)
        {
            return PMD.SelectReMesByZhiDanOrIMEIDAL(InputNum);
        }

        public List<PrintMessage> SelectPrintMesBySNOrIMEIBLL(string conditions)
        {
            return PMD.SelectPrintMesBySNOrIMEIDAL(conditions);
        }

        public bool DeletePrintMessageBLL(int ID,string field)
        {
            if (PMD.DeletePrintMessageDAL(ID,field) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<PrintMessage> SelectPrintMesByZhiDanBLL(string ZhiDan)
        {
            return PMD.SelectPrintMesByZhiDanDAL(ZhiDan);
        }

        public string SelectPresentImeiByZhidanBLL(string ZhiDan)
        {
            return PMD.SelectPresentImeiByZhidanDAL(ZhiDan);
        }

        public string SelectPresentSnByZhidanBLL(string ZhiDan)
        {
            return PMD.SelectPresentSNByZhidanDAL(ZhiDan);
        }

    }
}
