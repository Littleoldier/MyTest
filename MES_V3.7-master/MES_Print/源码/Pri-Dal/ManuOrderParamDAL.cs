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
                //command.CommandText = "SELECT * FROM dbo.Gps_ManuOrderParam WHERE ZhiDan='" + ZhidanNum + "'";
                command.CommandText = "SELECT       " +
                      "SoftModel,     SN1 ,       SN2 ,         SN3 ,        BoxNo1 ,          BoxNo2 ,           ProductDate,      Color ,       Weight ,       Qty ,         ProductNo ,   Version ,     IMEIStart ,   IMEIEnd ,    SIMStart , SIMEnd ,   BATStart ,  BATEnd ,    VIPStart ,   VIPEnd ," +
                      "IMEIRel ,      Remark1,    Remark5 ,     status ,     JST_template ,     CHT_template1 ,     CHT_template2 , BAT_prefix ,  BAT_digits ,   SIM_prefix ,  SIM_digits ,  VIP_prefix,   VIP_digits ,  ICCID_prefix ,"+
                      "ICCID_digits , IMEIPrints, MAC_prefix ,  MAC_digits , Equipment_prefix , Equipment_digits ,  IMEI2Start ,    IMEI2End,     IMEI2Prints    FROM dbo.Gps_ManuOrderParam WHERE ZhiDan='" + ZhidanNum + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new Gps_ManuOrderParam()
                    {
                        SoftModel = dr.GetString(0),
                        SN1 = dr.GetString(1),
                        SN2 = dr.GetString(2),
                        SN3 = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        Box_No1 = dr.GetString(4),
                        Box_No2 = dr.GetString(5),
                        ProductDate = dr.GetString(6),
                        Color = dr.GetString(7),
                        Weight = dr.GetString(8),
                        Qty = dr.GetString(9),
                        ProductNo = dr.GetString(10),
                        Version = dr.GetString(11),
                        IMEIStart = dr.GetString(12),
                        IMEIEnd = dr.GetString(13),
                        SIMStart = dr.IsDBNull(14) ? "" : dr.GetString(14),
                        SIMEnd = dr.IsDBNull(15) ? "" : dr.GetString(15),
                        BATStart = dr.IsDBNull(16) ? "" : dr.GetString(16),
                        BATEnd = dr.IsDBNull(17) ? "" : dr.GetString(17),
                        VIPStart = dr.IsDBNull(18) ? "" : dr.GetString(18),
                        VIPEnd = dr.IsDBNull(19) ? "" : dr.GetString(19),
                        IMEIRel = dr.GetInt32(20).ToString(),
                        Remark1 = dr.IsDBNull(21) ? "" : dr.GetString(21),
                        Remark5 = dr.IsDBNull(22) ? "" : dr.GetString(22),
                        status = dr.GetInt32(23),
                        JST_template = dr.IsDBNull(24) ? "" : dr.GetString(24),
                        CHT_template1 = dr.IsDBNull(25) ? "" : dr.GetString(25),
                        CHT_template2 = dr.IsDBNull(26) ? "" : dr.GetString(26),
                        BAT_prefix = dr.IsDBNull(27) ? "" : dr.GetString(27),
                        BAT_digits = dr.IsDBNull(28) ? "" : dr.GetString(28),
                        SIM_prefix = dr.IsDBNull(29) ? "" : dr.GetString(29),
                        SIM_digits = dr.IsDBNull(30) ? "" : dr.GetString(30),
                        VIP_prefix = dr.IsDBNull(31) ? "" : dr.GetString(31),
                        VIP_digits = dr.IsDBNull(32) ? "" : dr.GetString(32),
                        ICCID_prefix = dr.IsDBNull(33) ? "" : dr.GetString(33),
                        ICCID_digits = dr.IsDBNull(34) ? "" : dr.GetString(34),
                        IMEIPrints = dr.IsDBNull(35) ? "" : dr.GetString(35),
                        MAC_prefix = dr.IsDBNull(36) ? "" : dr.GetString(36),
                        MAC_digits = dr.IsDBNull(37) ? "" : dr.GetString(37),
                        Equipment_prefix = dr.IsDBNull(38) ? "" : dr.GetString(38),
                        Equipment_digits = dr.IsDBNull(39) ? "" : dr.GetString(39),
                        IMEI2Start = dr.IsDBNull(40) ? "" : dr.GetString(40),
                        IMEI2End = dr.IsDBNull(41) ? "" : dr.GetString(41),
                        IMEI2Prints = dr.IsDBNull(42) ? "" : dr.GetString(42),
                        //IMEI2Rel = dr.IsDBNull(55) ? "" : dr.GetInt32(55).ToString(),
                        //IMEI2Rel = dr.IsDBNull(55) ? "" : dr.GetString(55)
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
                //command.CommandText = "SELECT * FROM dbo.Gps_ManuOrderParam WHERE ZhiDan='" + ZhidanNum + "'";
                command.CommandText = "SELECT "+
                     "SoftModel ,    SN1 ,           SN2 ,          SN3 ,        BoxNo1 ,      BoxNo2 ,       ProductDate ,      Color ,            Weight ,       Qty ,           ProductNo ,    Version ,      IMEIStart ,   IMEIEnd ,      SIMStart ,      SIMEnd ,  "+
                    " BATStart ,     BATEnd ,        VIPStart ,     VIPEnd ,     IMEIRel ,     Remark1 ,      Remark5 ,          status ,           JST_template , CHT_template1 , CHT_template2,  BAT_prefix ,   BAT_digits ,  SIM_prefix ,  SIM_digits ,    VIP_prefix , " +
                    " VIP_digits ,   ICCID_prefix ,  ICCID_digits , IMEIPrints ,   MAC_prefix , MAC_digits ,  Equipment_prefix , Equipment_digits , RFID_Start ,     RFID_End ,      RFID_prefix ,   RFID_digits     FROM dbo.Gps_ManuOrderParam WHERE ZhiDan='" + ZhidanNum + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.SoftModel = dr.GetString(0);
                    list.SN1 = dr.GetString(1);
                    list.SN2 = dr.GetString(2);
                    list.SN3 = dr.IsDBNull(3) ? "" : dr.GetString(3);
                    list.Box_No1 = dr.GetString(4);
                    list.Box_No2 = dr.GetString(5);
                    list.ProductDate = dr.GetString(6);
                    list.Color = dr.GetString(7);
                    list.Weight = dr.GetString(8);
                    list.Qty = dr.GetString(9);
                    list.ProductNo = dr.GetString(10);
                    list.Version = dr.GetString(11);
                    list.IMEIStart = dr.GetString(12);
                    list.IMEIEnd = dr.GetString(13);
                    list.SIMStart = dr.IsDBNull(14) ? "" : dr.GetString(14);
                    list.SIMEnd = dr.IsDBNull(15) ? "" : dr.GetString(15);

                    list.BATStart = dr.IsDBNull(16) ? "" : dr.GetString(16);
                    list.BATEnd = dr.IsDBNull(17) ? "" : dr.GetString(17);
                    list.VIPStart = dr.IsDBNull(18) ? "" : dr.GetString(18);
                    list.VIPEnd = dr.IsDBNull(19) ? "" : dr.GetString(19);
                    list.IMEIRel = dr.GetInt32(20).ToString();
                    list.Remark1 = dr.IsDBNull(21) ? "" : dr.GetString(21);
                    list.Remark5 = dr.IsDBNull(22) ? "" : dr.GetString(22);
                    list.status = dr.GetInt32(23);
                    list.JST_template = dr.IsDBNull(24) ? "" : dr.GetString(24);
                    list.CHT_template1 = dr.IsDBNull(25) ? "" : dr.GetString(25);
                    list.CHT_template2 = dr.IsDBNull(26) ? "" : dr.GetString(26);
                    list.BAT_prefix = dr.IsDBNull(27) ? "" : dr.GetString(27);
                    list.BAT_digits = dr.IsDBNull(28) ? "" : dr.GetString(28);
                    list.SIM_prefix = dr.IsDBNull(29) ? "" : dr.GetString(29);
                    list.SIM_digits = dr.IsDBNull(30) ? "" : dr.GetString(30);

                    list.VIP_prefix = dr.IsDBNull(31) ? "" : dr.GetString(31);
                    list.VIP_digits = dr.IsDBNull(32) ? "" : dr.GetString(32);
                    list.ICCID_prefix = dr.IsDBNull(33) ? "" : dr.GetString(33);
                    list.ICCID_digits = dr.IsDBNull(34) ? "" : dr.GetString(34);
                    list.IMEIPrints = dr.IsDBNull(35) ? "" : dr.GetString(35);
                    list.MAC_prefix = dr.IsDBNull(36) ? "" : dr.GetString(36);
                    list.MAC_digits = dr.IsDBNull(37) ? "" : dr.GetString(37);
                    list.Equipment_prefix = dr.IsDBNull(38) ? "" : dr.GetString(38);
                    list.Equipment_digits = dr.IsDBNull(39) ? "" : dr.GetString(39);
                    list.RFIDStart = dr.IsDBNull(40) ? "" : dr.GetString(40);
                    list.RFIDEnd = dr.IsDBNull(41) ? "" : dr.GetString(41);
                    list.RFID_prefix = dr.IsDBNull(42) ? "" : dr.GetString(42);
                    list.RFID_digits = dr.IsDBNull(43) ? "" : dr.GetString(43);
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

        //根据制单号更新SN2号
        public int UpdateSNnumberDAL(string ZhiDanNum, string SN2)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE Gps_ManuOrderParam SET SN2 ='" + SN2 +  "' WHERE ZhiDan='" + ZhiDanNum + "'";
                    return command.ExecuteNonQuery();
                }
            }
        }

        //根据制单号更新SN2号
        public int UpdateIMEI2SNnumberDAL(string ZhiDanNum, string SN2, string ImeiPrints,string Imei2Prints)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE Gps_ManuOrderParam SET SN2 ='" + SN2 + "',IMEIPrints = '" + ImeiPrints + "',IMEI2Prints = '" + Imei2Prints + "' WHERE ZhiDan='" + ZhiDanNum + "'";
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
        
        //更新机身打印信息
        public int UpdateJSmesIMEI2DAL(string IMEI,string IMEI2, string JSPrintTime,string JSUserName, string JSUserDes, string IMEI2Start, string IMEI2End, string lj1)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "UPDATE dbo.Gps_ManuPrintParam SET JS_PrintTime='" + JSPrintTime + "', JS_TemplatePath ='" + lj1 + "', IMEI2 ='" + IMEI2 + "', JSUserName ='" + JSUserName + "', JSUserDes ='" + JSUserDes + "', IMEI2Start ='" + IMEI2Start + "', IMEI2End ='" + IMEI2End + "' WHERE IMEI='" + IMEI + "'";
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
        public int UpdateInlineByZhiDanDAL(string ZhiDanNum,string SN1, string SN2, string ProductData, string SIM1, string SIM2, string SIM_dig, string SIM_pre, string VIP1, string VIP2, string VIP_dig, string VIP_pre, string BAT1, string BAT2, string BAT_dig, string BAT_pre, string ICCID_dig, string ICCID_pre, string MAC_dig, string MAC_pre, string Equipment_dig, string Equipment_pre, string RFID_Num1,string RFID_Num2, string RFID_prefix,string RFID_digits)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "UPDATE Gps_ManuOrderParam SET SN1='" + SN1 + "',SN2='" + SN2 + "',ProductDate='" + ProductData + "',SIMStart='" + SIM1 + "',SIMEnd='" + SIM2 + "',BATStart='" + BAT1 + "',BATEnd='" + BAT2 + "',VIPStart='" + VIP1 + "',VIPEnd='" + VIP2 + "',BAT_prefix='" + BAT_pre + "',BAT_digits='" + BAT_dig + "',SIM_prefix='" + SIM_pre + "',SIM_digits='" + SIM_dig + "',VIP_prefix='" + VIP_pre + "',VIP_digits='" + VIP_dig + "',ICCID_prefix='" + ICCID_pre + "',ICCID_digits='" + ICCID_dig + "',MAC_prefix='" + MAC_pre + "',MAC_digits='" + MAC_dig + "',Equipment_prefix='" + Equipment_pre + "',Equipment_digits='" + Equipment_dig + "',RFID_Start='" + RFID_Num1 + "',RFID_End='" + RFID_Num2 + "',RFID_prefix='" + RFID_prefix + "',RFID_digits='" + RFID_digits + "' WHERE (ZhiDan='" + ZhiDanNum + "' AND Status=0)";
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
