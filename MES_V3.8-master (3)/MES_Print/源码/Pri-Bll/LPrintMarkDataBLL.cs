using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPrintMarkData.Param.Pri_Dal;
using Print_Message;

namespace LPrintMarkData.Param.Pri_BLL
{
    class LPrintMarkDataBLL
    {
        private LPrintMarkDataDAL LPMDD = new LPrintMarkDataDAL();


        //public bool CheckIMEIBLL(string IMEInumber)
        //{
        //    if (LPMDD.CheckIMEINumberDAL(IMEInumber) > 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}


        public bool CheckIMEIBLL(string IMEInumber)
        {
            if (LPMDD.CheckIMEIDAL(IMEInumber) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<PrintMessage> CheckRangeIMEIBLL(string StarIMEI, string EndIMEI)
        {
            return LPMDD.CheckRangeIMEIDAL(StarIMEI, EndIMEI);
        }
    }
}
