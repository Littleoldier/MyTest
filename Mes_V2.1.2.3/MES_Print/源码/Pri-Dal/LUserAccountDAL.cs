using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace UserAccount.Pri_Dal
{
    class LUserAccountDAL
    {
        string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        public void refreshCon()
        {
            conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;
        }

        //检查账号是否存在
        public int CheckUserNamePassword(string UserName, string Password)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "select * FROM [GPSTest].[dbo].[LUserAccount] WHERE Name='" + UserName + "' AND Password='" + Password + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    return 1;
                }
                return 0;
            }
        }

        //获取用户类型
        public string GetUserType(string UserName, string Password)
        {
            string Usertype = "";
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "select * FROM [GPSTest].[dbo].[LUserAccount] WHERE Name='" + UserName + "' AND Password='" + Password + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    return Usertype = dr.IsDBNull(0) ? "" : dr.GetString(0);
                }
                return Usertype;
            }
        }

        public string SetNameDAL(string UserName)
        {
            return "";
        }
    }
}
