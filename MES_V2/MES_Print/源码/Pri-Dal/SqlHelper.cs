using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Print_DAL
{
    public class SqlHelper
    {
        //通过配置文件（app.config:xml）读取连接字符串
        private static readonly string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        //字段，通过连接字符串获取连接对象
        private SqlConnection con = new SqlConnection(conStr);

        ////属性，判断连接对象的状态并打开连接对象
        public SqlConnection Con
        {
            get
            {
                switch (con.State)
                {
                    case ConnectionState.Broken:
                        con.Close(); //先正常关闭，释放资源
                        con.Open();
                        break;
                    case ConnectionState.Closed:
                        con.Open();
                        break;
                    case ConnectionState.Connecting:
                        break;
                    case ConnectionState.Executing:
                        break;
                    case ConnectionState.Fetching:
                        break;
                    case ConnectionState.Open:
                        break;
                    default:
                        break;
                }
                return con;
            }
            set { con = value; }
        }

        //执行存储过程或者SQL语句并返回数据集DataSet

        //public DataSet GetDataSet(string strSQL, CommandType cmdType, params SqlParameter[] values)
        //{
        //    SqlCommand cmd = PrepareCommand(strSQL, cmdType, values);
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    DataSet ds = new DataSet();
        //    da.Fill(ds);
        //    return ds;
        //}

        //执行存储过程或者SQL语句并返回SqlDatareader


        public SqlDataReader GetDataReader(string strSQL, CommandType cmdType, params SqlParameter[] values)
        {
            SqlCommand cmd = PrepareCommand(strSQL, cmdType, values);
            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        //执行存储过程或者SQL语句并返回首行首列(新增方法的主键)
        public object ExecuteScalar(string strSQL, CommandType cmdType, params SqlParameter[] values)
        {
            SqlCommand cmd = PrepareCommand(strSQL, cmdType, values);
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// 返回影响行数SQL，增删改
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdType">存储过程</param>
        /// <param name="pms">参数数组</param>
        /// <returns>int</returns>
        public int ExecuteNonQuery(string strSQL, CommandType cmdType, params SqlParameter[] values)
        {
            SqlCommand cmd = PrepareCommand(strSQL, cmdType, values);
            return cmd.ExecuteNonQuery();
        }

        //内部方法，实例化命令对象并配置相关属性
        private SqlCommand PrepareCommand(string strSQL, CommandType cmdType, params SqlParameter[] values)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Con;
            cmd.CommandText = strSQL;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = 60;
            cmd.Parameters.AddRange(values);
            return cmd;
        }

    }
}