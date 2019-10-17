using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Print_Message;
using System.Configuration;

namespace LPrintMarkData.Param.Pri_Dal
{
    class LPrintMarkDataDAL
    {
        private static string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;


        //检查IMEI号是否存在
        public int CheckIMEINumberDAL(string IMEINumber)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT IMEI FROM dbo.[LPrintMarkData] WHERE IMEI='" + IMEINumber + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    return 1;
                }
                return 0;
            }
        }

         //范围检查IMEI号是否存在，存在返回IMEI，否则返回0
        public List<PrintMessage> CheckRangeIMEIDAL(string StarIMEI, string EndIMEI)
        {
            List<PrintMessage> pm = new List<PrintMessage>();
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "SELECT IMEI FROM dbo.[LPrintMarkData] WHERE (IMEI>='" + StarIMEI + "' AND IMEI<='" + EndIMEI + "')";
                    SqlDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        pm.Add(new PrintMessage()
                        {
                            IMEI = dr.GetString(0)
                        });
                    }
                    return pm;
                }
            }
        }

        //检查IMEI号是否存在，存在返回1，否则返回0
        public int CheckIMEIDAL(string IMEInumber)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "SELECT ID FROM dbo.LPrintMarkData WHERE IMEI='" + IMEInumber + "'";
                    string dr = Convert.ToString(command.ExecuteScalar());
                    if (dr != "")
                    {
                        return 1;
                    }
                    return 0;
                }
            }
        }
    }
}
