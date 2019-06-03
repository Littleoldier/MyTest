using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Print_Message;

namespace ManuOrder.Param.DAL
{
    class ManuOrderParamDAL
    {
        string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        //返回制单号
        public List<Gps_ManuOrderParam> SelectZhidanNumDAL()
        {
            conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            List<Gps_ManuOrderParam> list = new List<Gps_ManuOrderParam>();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuOrderParam WHERE Status='1' OR Status='0' ORDER BY ZhiDan";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new Gps_ManuOrderParam()
                    {
                        ZhiDan = dr.GetString(1)
                    });
                }
                return list;
            }
        }

        //检查制单号是否存在
        public int CheckZhiDanDAL(string ZhiDan)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuOrderParam WHERE ZhiDan='" + ZhiDan + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    return 1;
                }
                return 0;
            }
        }


        //根据制单号返回该制单相关信息
        public List<Gps_ManuOrderParam> selectManuOrderParamByzhidanDAL(string ZhidanNum)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            List<Gps_ManuOrderParam> list = new List<Gps_ManuOrderParam>();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuOrderParam WHERE ZhiDan='" + ZhidanNum + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new Gps_ManuOrderParam()
                    {
                        SoftModel = dr.GetString(2),
                        SN1 = dr.GetString(3),
                        SN2 = dr.GetString(4),
                        SN3 = dr.IsDBNull(5) ? "" : dr.GetString(5),
                        Box_No1 = dr.GetString(6),
                        Box_No2 = dr.GetString(7),
                        ProductDate = dr.GetString(8),
                        Color = dr.GetString(9),
                        Weight = dr.GetString(10),
                        Qty = dr.GetString(11),
                        ProductNo = dr.GetString(12),
                        Version = dr.GetString(13),
                        IMEIStart = dr.GetString(14),
                        IMEIEnd = dr.GetString(15),
                        SIMStart = dr.IsDBNull(16) ? "" : dr.GetString(16),
                        SIMEnd = dr.IsDBNull(17) ? "" : dr.GetString(17),
                        BATStart = dr.IsDBNull(18) ? "" : dr.GetString(18),
                        BATEnd = dr.IsDBNull(19) ? "" : dr.GetString(19),
                        VIPStart = dr.IsDBNull(20) ? "" : dr.GetString(20),
                        VIPEnd = dr.IsDBNull(21) ? "" : dr.GetString(21),
                        IMEIRel = dr.GetInt32(22).ToString(),
                        Remark1 = dr.IsDBNull(25) ? "" : dr.GetString(25),
                        Remark5 = dr.IsDBNull(29) ? "" : dr.GetString(29),
                        status = dr.GetInt32(30),
                        JST_template = dr.IsDBNull(32) ? "" : dr.GetString(32),
                        CHT_template1 = dr.IsDBNull(33) ? "" : dr.GetString(33),
                        CHT_template2 = dr.IsDBNull(34) ? "" : dr.GetString(34),
                        BAT_prefix = dr.IsDBNull(35) ? "" : dr.GetString(35),
                        BAT_digits = dr.IsDBNull(36) ? "" : dr.GetString(36),
                        SIM_prefix = dr.IsDBNull(37) ? "" : dr.GetString(37),
                        SIM_digits = dr.IsDBNull(38) ? "" : dr.GetString(38),
                        VIP_prefix = dr.IsDBNull(39) ? "" : dr.GetString(39),
                        VIP_digits = dr.IsDBNull(40) ? "" : dr.GetString(40),
                        ICCID_prefix = dr.IsDBNull(41) ? "" : dr.GetString(41),
                        ICCID_digits = dr.IsDBNull(42) ? "" : dr.GetString(42),
                        IMEIPrints = dr.IsDBNull(43) ? "" : dr.GetString(43),
                        MAC_prefix = dr.IsDBNull(44) ? "" : dr.GetString(44),
                        MAC_digits = dr.IsDBNull(45) ? "" : dr.GetString(45),
                        Equipment_prefix = dr.IsDBNull(46) ? "" : dr.GetString(46),
                        Equipment_digits = dr.IsDBNull(47) ? "" : dr.GetString(47)
                    });
                }
                return list;
            }
        }

        //根据制单号返回该制单相关信息
        public Gps_ManuOrderParam selectManuOrderParamByzhidanDAL(string ZhidanNum,int NullInt)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            Gps_ManuOrderParam list = new Gps_ManuOrderParam();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuOrderParam WHERE ZhiDan='" + ZhidanNum + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.SoftModel = dr.GetString(2);
                    list.SN1 = dr.GetString(3);
                    list.SN2 = dr.GetString(4);
                    list.SN3 = dr.IsDBNull(5) ? "" : dr.GetString(5);
                    list.Box_No1 = dr.GetString(6);
                    list.Box_No2 = dr.GetString(7);
                    list.ProductDate = dr.GetString(8);
                    list.Color = dr.GetString(9);
                    list.Weight = dr.GetString(10);
                    list.Qty = dr.GetString(11);
                    list.ProductNo = dr.GetString(12);
                    list.Version = dr.GetString(13);
                    list.IMEIStart = dr.GetString(14);
                    list.IMEIEnd = dr.GetString(15);
                    list.SIMStart = dr.IsDBNull(16) ? "" : dr.GetString(16);
                    list.SIMEnd = dr.IsDBNull(17) ? "" : dr.GetString(17);
                    list.BATStart = dr.IsDBNull(18) ? "" : dr.GetString(18);
                    list.BATEnd = dr.IsDBNull(19) ? "" : dr.GetString(19);
                    list.VIPStart = dr.IsDBNull(20) ? "" : dr.GetString(20);
                    list.VIPEnd = dr.IsDBNull(21) ? "" : dr.GetString(21);
                    list.IMEIRel = dr.GetInt32(22).ToString();
                    list.Remark1 = dr.IsDBNull(25) ? "" : dr.GetString(25);
                    list.Remark5 = dr.IsDBNull(29) ? "" : dr.GetString(29);
                    list.status = dr.GetInt32(30);
                    list.JST_template = dr.IsDBNull(32) ? "" : dr.GetString(32);
                    list.CHT_template1 = dr.IsDBNull(33) ? "" : dr.GetString(33);
                    list.CHT_template2 = dr.IsDBNull(34) ? "" : dr.GetString(34);
                    list.BAT_prefix = dr.IsDBNull(35) ? "" : dr.GetString(35);
                    list.BAT_digits = dr.IsDBNull(36) ? "" : dr.GetString(36);
                    list.SIM_prefix = dr.IsDBNull(37) ? "" : dr.GetString(37);
                    list.SIM_digits = dr.IsDBNull(38) ? "" : dr.GetString(38);
                    list.VIP_prefix = dr.IsDBNull(39) ? "" : dr.GetString(39);
                    list.VIP_digits = dr.IsDBNull(40) ? "" : dr.GetString(40);
                    list.ICCID_prefix = dr.IsDBNull(41) ? "" : dr.GetString(41);
                    list.ICCID_digits = dr.IsDBNull(42) ? "" : dr.GetString(42);
                    list.IMEIPrints = dr.IsDBNull(43) ? "" : dr.GetString(43);
                    list.MAC_prefix = dr.IsDBNull(44) ? "" : dr.GetString(44);
                    list.MAC_digits = dr.IsDBNull(45) ? "" : dr.GetString(45);
                    list.Equipment_prefix = dr.IsDBNull(46) ? "" : dr.GetString(46);
                    list.Equipment_digits = dr.IsDBNull(47) ? "" : dr.GetString(47);
                }
                return list;
            }
        }

        //根据制单号更新SN2号
        public int UpdateSNnumberDAL(string ZhiDanNum, string SN2, string ImeiPrints)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE Gps_ManuOrderParam SET SN2 ='" + SN2 + "',IMEIPrints = '" + ImeiPrints + "' WHERE ZhiDan='" + ZhiDanNum + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }

        //根据制单号更新彩盒模板路径
        public int UpdateCH_TemplatePath1DAL(string ZhiDanNum, string lj1, string lj2)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE Gps_ManuOrderParam SET [CHT_template1] ='" + lj1 + "', [CHT_template2] ='" + lj2 + "' WHERE ZhiDan='" + ZhiDanNum + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }

        //根据制单号更新机身模板路径
        public int UpdateJS_TemplatePathDAL(string ZhiDanNum, string JS_TemplatePath)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE Gps_ManuOrderParam SET [JST_template] ='" + JS_TemplatePath + "' WHERE ZhiDan='" + ZhiDanNum + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }


        //打印时该SN号已存在--SN号++
        public int UpdateSNAddOneDAL(string ZhiDanNum, string SN2)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE Gps_ManuOrderParam SET SN2 ='" + SN2 + "'WHERE ZhiDan='" + ZhiDanNum + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }

        //更新彩盒打印信息
        public int UpdateCHmesDAL(string IMEI, string CHPrintTime, string lj1, string lj2,String sn)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE dbo.Gps_ManuPrintParam SET SN = '" + sn + "', CH_PrintTime='" + CHPrintTime + "', CH_TemplatePath1 ='" + lj1 + "', CH_TemplatePath2 ='" + lj2 + "' WHERE IMEI='" + IMEI + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }

        //更新彩盒关联打印信息
        public int UpdateCHAssociatedDAL(string IMEI, string CHPrintTime, string lj1, string lj2, string SIM, string VIP, string BAT, string ICCID, string MAC, string Equipment, string SN,string zhidan, string RFID, string CHUserName)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    string CH_PrintTime = CHPrintTime == "" ? "NULL" : "'" + CHPrintTime + "'";
                    command.CommandText = "UPDATE dbo.Gps_ManuPrintParam SET ZhiDan ='"+zhidan+"', SN='" + SN + "', CH_PrintTime=" + CH_PrintTime + ", CH_TemplatePath1 ='" + lj1 + "', CH_TemplatePath2 ='" + lj2 + "',SIM='" + SIM + "',VIP='" + VIP + "',BAT='" + BAT + "',ICCID='" + ICCID + "',MAC='" + MAC + "',Equipment='" + Equipment + "',RFID='"+RFID+ "',CHUserName='" + CHUserName + "' WHERE IMEI='" + IMEI + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }

        //更新机身打印信息
        public int UpdateJSmesDAL(string IMEI, string JSPrintTime, string lj1)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE dbo.Gps_ManuPrintParam SET JS_PrintTime='" + JSPrintTime + "', JS_TemplatePath ='" + lj1 + "' WHERE IMEI='" + IMEI + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }

        //根据制单号更新状态，打印后0改成1
        public int UpdateStatusByZhiDanDAL(string ZhiDanNum)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "UPDATE Gps_ManuOrderParam SET Status = 1 WHERE (ZhiDan='" + ZhiDanNum + "' AND Status=0)";
                return command.ExecuteNonQuery();
            }
        }

        //根据制单号更新数据
        public int UpdateInlineByZhiDanDAL(string ZhiDanNum,string SN1, string SN2, string ProductData, string SIM1, string SIM2, string SIM_dig, string SIM_pre, string VIP1, string VIP2, string VIP_dig, string VIP_pre, string BAT1, string BAT2, string BAT_dig, string BAT_pre, string ICCID_dig, string ICCID_pre, string MAC_dig, string MAC_pre, string Equipment_dig, string Equipment_pre)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "UPDATE Gps_ManuOrderParam SET SN1='" + SN1 + "',SN2='" + SN2 + "',ProductDate='" + ProductData + "',SIMStart='" + SIM1 + "',SIMEnd='" + SIM2 + "',BATStart='" + BAT1 + "',BATEnd='" + BAT2 + "',VIPStart='" + VIP1 + "',VIPEnd='" + VIP2 + "',BAT_prefix='" + BAT_pre + "',BAT_digits='" + BAT_dig + "',SIM_prefix='" + SIM_pre + "',SIM_digits='" + SIM_dig + "',VIP_prefix='" + VIP_pre + "',VIP_digits='" + VIP_dig + "',ICCID_prefix='" + ICCID_pre + "',ICCID_digits='" + ICCID_dig + "',MAC_prefix='" + MAC_pre + "',MAC_digits='" + MAC_dig + "',Equipment_prefix='" + Equipment_pre + "',Equipment_digits='" + Equipment_dig + "' WHERE (ZhiDan='" + ZhiDanNum + "' AND Status=0)";
                return command.ExecuteNonQuery();
            }
        }

        //根据制单号更新remark5
        public int UpdateRemark5DAL(string ZhiDanNum, string remark5)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "UPDATE Gps_ManuOrderParam SET Remark5 ='" + remark5 + "' WHERE ZhiDan='" + ZhiDanNum + "'";
                return command.ExecuteNonQuery();
            }
        }


    }
}
