using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace TestResult.Param.DAL
{
    class TestResultDAL
    {
        string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        public void refreshCon()
        {
            conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;
        }

        //查工位
        public string CheckOneStationDAL(string MissingSql,string IMEI)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                string Result = "";
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = MissingSql;
                    if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                    {
                        return "1";
                    }
                    else
                    {
                        command.CommandText = "SELECT AutoTestResult,CoupleResult,WriteImeiResult,ParamDownloadResult,GPSResult FROM dbo.Gps_TestResult  WHERE IMEI='" + IMEI+"'";
                        SqlDataReader dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            if (dr.GetInt32(0) == 0)
                            {
                                Result += "功能、";
                            }
                            if (dr.GetInt32(1) == 0)
                            {
                                Result += "耦合、";
                            }
                            if (dr.GetInt32(2) == 0)
                            {
                                Result += "写号、";
                            }
                            if (dr.GetInt32(3) == 0)
                            {
                                Result += "参数下载、";
                            }
                            if (dr.GetInt32(4) == 0)
                            {
                                Result += "功能2、";
                            }
                        }
                        return Result;
                    }
                }
            }
        }

    }
}
