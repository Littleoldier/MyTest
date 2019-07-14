using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelPrint.Param.DAL;
using Print_Message;

namespace ExcelPrint.Param.Bll 
{
    class ManuExcelPrintParamBLL
    {
        ManuExcelPrintParamDAL MEPPD = new ManuExcelPrintParamDAL();

        public void refeshConBLL()
        {
            MEPPD.refreshCon();
        }

        public bool CheckIMEIBLL(string IMEI1,string IMEI2)
        {
            if (MEPPD.CheckIMEI1OrIMEI2DAL(IMEI1,IMEI2) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckIMEI1BLL(string IMEI1)
        {
            if (MEPPD.CheckIMEI1DAL(IMEI1) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InsertManuExcelPrintBLL(List<ManuExcelPrintParam> list)
        {
            if (MEPPD.InsertManuExcelPrintDAL(list) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<ManuExcelPrintParam> SelectByImei1to5BLL(string IMEI)
        {
            return MEPPD.SelectByImei1to5DAL(IMEI);
        }

        public List<ManuExcelPrintParam> SelectByImei1BLL(string IMEI1)
        {
            return MEPPD.SelectByImei1DAL(IMEI1);
        }

        public List<ManuExcelPrintParam> SelectAllRePrintBLL()
        {
            return MEPPD.SelectAllRePrintDAL();
        }

        public bool UpdateRePrintTimeBLL(string IMEI1, string RePrintTime)
        {
            if (MEPPD.UpdateRePrintTimeDAL(IMEI1, RePrintTime) > 0)
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
