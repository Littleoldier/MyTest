using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Print_Message;
using ManuOrder.Param.DAL;

namespace ManuOrder.Param.BLL
{
    public class ManuOrderParamBLL
    {
        ManuOrderParamDAL MOPD = new ManuOrderParamDAL();

        public List<Gps_ManuOrderParam> SelectZhidanNumBLL() {
            return MOPD.SelectZhidanNumDAL();
        }

        public List<Gps_ManuOrderParam> selectManuOrderParamByzhidanBLL(string ZhidanNum) {
            return MOPD.selectManuOrderParamByzhidanDAL(ZhidanNum);
        }

        public Gps_ManuOrderParam selectManuOrderParamByzhidanBLL(string ZhidanNum,int NullInt)
        {
            return MOPD.selectManuOrderParamByzhidanDAL(ZhidanNum, NullInt);
        }

        public bool CheckZhiDanBLL(string ZhiDan)
        {
            if (MOPD.CheckZhiDanDAL(ZhiDan) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //更新彩盒模板路径
        public bool UpdateCH_TemplatePath1DAL(string ZhiDanNum, string lj1, string lj2)
        {
            if (MOPD.UpdateCH_TemplatePath1DAL(ZhiDanNum, lj1, lj2) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //更新机身模板路径
        public bool UpdateJS_TemplatePathDAL(string ZhiDanNum, string lj1)
        {
            if (MOPD.UpdateJS_TemplatePathDAL(ZhiDanNum, lj1) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSNnumberBLL(string ZhiDanNum, string SN2, string imeiPrints) {
            if (MOPD.UpdateSNnumberDAL(ZhiDanNum,SN2,imeiPrints)> 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateSNAddOneBLL(string ZhiDanNum,string SN2)
        {
            if (MOPD.UpdateSNAddOneDAL(ZhiDanNum,SN2) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateCHmesBLL(string IMEI, string CHPrintTime,string lj1,string lj2,string SN)
        {
            if (MOPD.UpdateCHmesDAL(IMEI, CHPrintTime, lj1, lj2,SN) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateCHAssociatedBLL(string IMEI, string CHPrintTime, string lj1, string lj2 ,string SIM,string VIP,string BAT,string ICCID,string MAC,string Equipment,string SN, string zhidan,string RFID)
        {
            if (MOPD.UpdateCHAssociatedDAL(IMEI, CHPrintTime, lj1, lj2,SIM,VIP,BAT,ICCID,MAC,Equipment,SN,zhidan,RFID) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateJSmesBLL(string IMEI, string JSPrintTime, string lj1)
        {
            if (MOPD.UpdateJSmesDAL(IMEI, JSPrintTime, lj1) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateStatusByZhiDanBLL(string ZhiDanNum)
        {
            MOPD.UpdateStatusByZhiDanDAL(ZhiDanNum);
        }

        public int UpdateInlineByZhiDanBLL(string ZhiDanNum,string SN1,string SN2,string ProductData, string SIM1,string SIM2,string SIM_dig,string SIM_pre,string VIP1, string VIP2, string VIP_dig,string VIP_pre,string BAT1,string BAT2,string BAT_dig,string BAT_pre,string ICCID_dig,string ICCID_pre, string MAC_dig, string MAC_pre, string Equipment_dig, string Equipment_pre)
        {
           return MOPD.UpdateInlineByZhiDanDAL(ZhiDanNum,SN1,SN2,ProductData,SIM1,SIM2,SIM_dig,SIM_pre,VIP1,VIP2,VIP_dig,VIP_pre,BAT1,BAT2,BAT_dig,BAT_pre, ICCID_dig, ICCID_pre, MAC_dig, MAC_pre, Equipment_dig, Equipment_pre);
        }

        public bool UpdateRemark5BLL(string Zhidan, string remarkk5)
        {
            if (MOPD.UpdateRemark5DAL(Zhidan, remarkk5) > 0)
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
