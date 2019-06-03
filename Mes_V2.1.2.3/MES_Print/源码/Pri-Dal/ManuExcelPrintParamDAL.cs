using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Print_Message;

namespace ExcelPrint.Param.DAL
{
    class ManuExcelPrintParamDAL
    {
        string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        public void refreshCon()
        {
            conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;
        }

        //检查IMEI1号或IMEI2是否存在，任意一个存在返回1，否则返回0
        public int CheckIMEI1OrIMEI2DAL(string IMEI1,string IMEI2)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuExcelPrintParam WHERE (IMEI1='" + IMEI1 + "' OR IMEI2 = '" + IMEI2 + "')";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    return 1;
                }
                return 0;
            }
        }

        //检查IMEI1号是否存在，存在返回1，否则返回0
        public int CheckIMEI1DAL(string IMEI1)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM dbo.Gps_ManuExcelPrintParam WHERE IMEI1='" + IMEI1 + "'";
                    SqlDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        return 1;
                    }
                    return 0;
                }
            }
        }

        //插入Excel打印数据到ManuExcelPrintParam表
        public int InsertManuExcelPrintDAL(List<ManuExcelPrintParam> list)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    int i = list.Count;
                    if (i > 0)
                    {
                        command.CommandText = "INSERT INTO Gps_ManuExcelPrintParam(IMEI1,IMEI2,IMEI3,IMEI4,IMEI5,PrintTime,Template,RePrintNum,ReFirstPrintTime,ReEndPrintTime) VALUES('" + list[i - 1].IMEI1 + "','" + list[i - 1].IMEI2 + "','" + list[i - 1].IMEI3 + "','" + list[i - 1].IMEI4 + "','" + list[i - 1].IMEI5 + "','" + list[i - 1].PrintTime + "','" + list[i - 1].Template + "','0',NULL,NULL)";
                    }
                    int httpstr = command.ExecuteNonQuery();
                    return httpstr;
                }
            }
        }

        //根据IMEI1-5获取关联数据
        public List<ManuExcelPrintParam> SelectByImei1to5DAL(string Imei)
        {
            List<ManuExcelPrintParam> pm = new List<ManuExcelPrintParam>();
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuExcelPrintParam WHERE (IMEI1 ='" + Imei + "' OR IMEI2 = '" + Imei + "'OR IMEI3 = '" + Imei + "'OR IMEI4 = '" + Imei + "'OR IMEI5 = '" + Imei + "')";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    pm.Add(new ManuExcelPrintParam()
                    {
                        IMEI1 = dr.IsDBNull(1) ? "" : dr.GetString(1),
                        IMEI2 = dr.IsDBNull(2) ? "" : dr.GetString(2),
                        IMEI3 = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        IMEI4 = dr.IsDBNull(4) ? "" : dr.GetString(4),
                        IMEI5 = dr.IsDBNull(5) ? "" : dr.GetString(5),
                        PrintTime = dr.IsDBNull(6) ? "" : dr.GetDateTime(6).ToString(),
                        Template = dr.IsDBNull(7) ? "" : dr.GetString(7),
                        RePrintNum =dr.GetInt32(8),
                        ReFirstPrintTime = dr.IsDBNull(9) ? "" : dr.GetDateTime(9).ToString(),
                        ReEndPrintTime = dr.IsDBNull(10) ? "" : dr.GetDateTime(10).ToString()
                    });
                }
                return pm;
            }
        }

        //根据IMEI1获取关联数据
        public List<ManuExcelPrintParam> SelectByImei1DAL(string Imei1)
        {
            List<ManuExcelPrintParam> pm = new List<ManuExcelPrintParam>();
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuExcelPrintParam WHERE IMEI1 ='" + Imei1 + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    pm.Add(new ManuExcelPrintParam()
                    {
                        IMEI1 = dr.IsDBNull(1) ? "" : dr.GetString(1),
                        IMEI2 = dr.IsDBNull(2) ? "" : dr.GetString(2),
                        IMEI3 = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        IMEI4 = dr.IsDBNull(4) ? "" : dr.GetString(4),
                        IMEI5 = dr.IsDBNull(5) ? "" : dr.GetString(5)
                    });
                }
                return pm;
            }
        }

        //根据重打次数不为0获取重打记录
        public List<ManuExcelPrintParam> SelectAllRePrintDAL()
        {
            List<ManuExcelPrintParam> pm = new List<ManuExcelPrintParam>();
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuExcelPrintParam WHERE RePrintNum!=0 ";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    pm.Add(new ManuExcelPrintParam()
                    {
                        IMEI1 = dr.IsDBNull(1) ? "" : dr.GetString(1),
                        IMEI2 = dr.IsDBNull(2) ? "" : dr.GetString(2),
                        IMEI3 = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        IMEI4 = dr.IsDBNull(4) ? "" : dr.GetString(4),
                        IMEI5 = dr.IsDBNull(5) ? "" : dr.GetString(5),
                        PrintTime = dr.IsDBNull(6) ? "" : dr.GetDateTime(6).ToString(),
                        Template = dr.IsDBNull(7) ? "" : dr.GetString(7),
                        RePrintNum = dr.GetInt32(8),
                        ReFirstPrintTime = dr.IsDBNull(9) ? "" : dr.GetDateTime(9).ToString(),
                        ReEndPrintTime = dr.IsDBNull(10) ? "" : dr.GetDateTime(10).ToString()
                    });
                }
                return pm;
            }
        }

        //更新重打时间
        public int UpdateRePrintTimeDAL(string IMEI1, string RePrintTime)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                if (SelectRePrintNumByIMEI1(IMEI1) == 0)
                {
                    command.CommandText = "UPDATE dbo.Gps_ManuExcelPrintParam SET ReFirstPrintTime ='" + RePrintTime + "',RePrintNum = RePrintNum+1 WHERE IMEI1='" + IMEI1 + "'";
                }
                else
                {
                    command.CommandText = "UPDATE dbo.Gps_ManuExcelPrintParam SET ReEndPrintTime ='" + RePrintTime + "',RePrintNum = RePrintNum+1 WHERE IMEI1='" + IMEI1 + "'";
                }
                return command.ExecuteNonQuery();
            }
        }

        //获取重打次数
        public int SelectRePrintNumByIMEI1(string IMEI1)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "select  * FROM dbo.Gps_ManuExcelPrintParam WHERE IMEI1='" + IMEI1 + "'";
                SqlDataReader dr = command.ExecuteReader();
                int RePrintNum = 0;
                while (dr.Read())
                {
                    RePrintNum = dr.GetInt32(8);
                }
                return RePrintNum;
            }
        }


    }
}
