using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Print_Message;

namespace PrintRecord.Param.DAL
{
    class PrintRecordParamDAL
    {
       string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        public void refreshCon()
        {
            conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;
        }

        public int InsertPrintRecordParamDAL(string Zhidan, int SimMark, int VipMark, int BatMark, int IccidMark, int MacMark, int EquipmentMark, int NoCheckMark, int NoPaperMark,int UpdataSimMark,int UpdateIMEIMark,int AutoTestMark,int CoupleMark,int WriteImeiMark,int ParamDownloadMark,int TemPlate1Num,int TemPlate2Num,int GpsMark)
        {
            using (SqlConnection conn1 = new SqlConnection(conStr))
            {
                conn1.Open();
                int httpstr;
                using (SqlCommand command = conn1.CreateCommand())
                {
                    command.CommandText = "SELECT ID FROM dbo.Gps_ManuPrintRecordParam WHERE ZhiDan ='" + Zhidan + "'";
                    if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                    {
                        command.CommandText = "UPDATE dbo.Gps_ManuPrintRecordParam SET SimMark ='" + SimMark + "',VipMark = '" + VipMark + "',BatMark ='" + BatMark + "',IccidMark = '" + IccidMark + "',MacMark ='" + MacMark + "',EquipmentMark = '" + EquipmentMark + "',NoCheckMark ='" + NoCheckMark + "',NoPaperMark = '" + NoPaperMark + "',UpdataSimMark ='" + UpdataSimMark + "',UpdateIMEIMark = '" + UpdateIMEIMark + "',AutoTestMark ='" + AutoTestMark + "',CoupleMark = '" + CoupleMark + "',WriteImeiMark ='" + WriteImeiMark + "',ParamDownloadMark = '" + ParamDownloadMark + "',TemPlate1Num ='" + TemPlate1Num + "',TemPlate2Num = '" + TemPlate2Num + "',GPSMark = '" + GpsMark + "' WHERE ZhiDan='" + Zhidan + "'";
                        httpstr = command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO dbo.Gps_ManuPrintRecordParam(ZhiDan,SimMark,VipMark,BatMark,IccidMark,MacMark,EquipmentMark,NoCheckMark,NoPaperMark,UpdataSimMark,UpdateIMEIMark,AutoTestMark,CoupleMark,WriteImeiMark,ParamDownloadMark,TemPlate1Num,TemPlate2Num,GPSMark) VALUES('" + Zhidan + "','" + SimMark + "','" + VipMark + "','" + BatMark + "','" + IccidMark + "','" + MacMark + "','" + EquipmentMark + "','" + NoCheckMark + "','" + NoPaperMark + "','" + UpdataSimMark + "','" + UpdateIMEIMark + "','" + AutoTestMark + "'," + CoupleMark + ",'" + WriteImeiMark + "','"+ ParamDownloadMark +"'," + TemPlate1Num + ",'" + TemPlate2Num + "','" + GpsMark + "')";
                        httpstr = command.ExecuteNonQuery();
                    }
                    return httpstr;
                }
            }
        }


        //根据制单号返回该制单相关信息
        public List<ManuPrintRecordParam> selectRecordParamByzhidanDAL(string ZhidanNum)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            List<ManuPrintRecordParam> list = new List<ManuPrintRecordParam>();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbo.Gps_ManuPrintRecordParam WHERE ZhiDan='" + ZhidanNum + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ManuPrintRecordParam()
                    {
                        SimMark = dr.GetInt32(2),
                        VipMark = dr.GetInt32(3),
                        BatMark = dr.GetInt32(4),
                        IccidMark = dr.GetInt32(5),
                        MacMark = dr.GetInt32(6),
                        EquipmentMark = dr.GetInt32(7),
                        NoCheckMark = dr.GetInt32(8),
                        NoPaperMark = dr.GetInt32(9),
                        UpdataSimMark = dr.GetInt32(10),
                        UpdateIMEIMark = dr.GetInt32(11),
                        AutoTestMark = dr.GetInt32(12),
                        CoupleMark = dr.GetInt32(13),
                        WriteImeiMark = dr.GetInt32(14),
                        ParamDownloadMark = dr.GetInt32(15),
                        TemPlate1Num = dr.GetInt32(16),
                        TemPlate2Num = dr.GetInt32(17),
                        GPSMark = dr.GetInt32(18)
                    });
                }
                return list;
            }
        }

    }
}
