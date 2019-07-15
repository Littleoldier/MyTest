using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using UserDataMessage;

namespace UserAccount.Pri_Dal
{
    class LUserAccountDAL
    {
       private static string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

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
        public UserMessage GetUserType(string UserName, string Password)
        {
            
            SqlConnection conn1 = new SqlConnection(conStr);
            UserMessage Um = new UserMessage();
            conn1.Open();
            using (SqlCommand command = conn1.CreateCommand())
            {
                int DataOk = 0;
                command.CommandText = "select Name,UserType,UserDes FROM [GPSTest].[dbo].[LUserAccount] WHERE Name='" + UserName + "' AND Password='" + Password + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    Um.Name = dr.IsDBNull(0) ? "" : dr.GetString(0);
                    Um.UserType = dr.IsDBNull(1) ? "" : dr.GetString(1);
                    Um.UserDes = dr.IsDBNull(2) ? "" : dr.GetString(2);
                    DataOk = 1;
                }

                if(DataOk == 1)
                {
                    return Um;
                }

                Um.Clear();
                return Um;
            }
        }

    }
}
