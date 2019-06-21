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

        public int InsertPrintRecordParamDAL(string Zhidan, int SimMark, int VipMark, int BatMark, int IccidMark, int MacMark, int EquipmentMark, int RfidMark, int NoCheckMark, int NoPaperMark,int UpdataSimMark,int UpdateIMEIMark,int AutoTestMark,int CoupleMark,int WriteImeiMark,int ParamDownloadMark,int TemPlate1Num,int TemPlate2Num,int GpsMark
            , int CHCheckSnMark, int CHCheckSimMark, int CHCheckBatMark, int CHCheckIccidMark, int CHCheckMacMark, int CHCheckEquipmentMark, int CHCheckVipMark, int CHCheckRfidMark, int CHCheckImei14Mark)
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
                        command.CommandText = "UPDATE dbo.Gps_ManuPrintRecordParam SET SimMark ='" + SimMark + "',VipMark = '" + VipMark + "',BatMark ='" + BatMark + "',IccidMark = '" + IccidMark + "',MacMark ='" + MacMark + "',EquipmentMark = '" + EquipmentMark + "',RfidMark = '" + RfidMark + "',NoCheckMark ='" + NoCheckMark + "',NoPaperMark = '" + NoPaperMark + "',UpdataSimMark ='" + UpdataSimMark + "',UpdateIMEIMark = '" + UpdateIMEIMark + "',AutoTestMark ='" + AutoTestMark + "',CoupleMark = '" + CoupleMark + "',WriteImeiMark ='" + WriteImeiMark + "',ParamDownloadMark = '" + ParamDownloadMark + "',TemPlate1Num ='" + TemPlate1Num + "',TemPlate2Num = '" + TemPlate2Num + "',GPSMark = '" + GpsMark +
                            "',CHCheckSnMark ='" + CHCheckSnMark + "',CHCheckSimMark = '" + CHCheckSimMark + "',CHCheckBatMark = '" + CHCheckBatMark + "',CHCheckIccidMark ='" + CHCheckIccidMark + "',CHCheckMacMark = '" + CHCheckMacMark + "',CHCheckEquipmentMark = '" + CHCheckEquipmentMark + "',CHCheckVipMark = '" + CHCheckVipMark + "',CHCheckRfidMark = '" + CHCheckRfidMark + "',CHCheckImei14Mark = '" + CHCheckImei14Mark + "' WHERE ZhiDan='" + Zhidan + "'";
                        httpstr = command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO dbo.Gps_ManuPrintRecordParam(ZhiDan,SimMark,VipMark,BatMark,IccidMark,MacMark,EquipmentMark,RfidMark,NoCheckMark,NoPaperMark,UpdataSimMark,UpdateIMEIMark,AutoTestMark,CoupleMark,WriteImeiMark,ParamDownloadMark,TemPlate1Num,TemPlate2Num,GPSMark,  CHCheckSnMark,  CHCheckSimMark,  CHCheckBatMark,  CHCheckIccidMark,  CHCheckMacMark,  CHCheckEquipmentMark,  CHCheckVipMark,  CHCheckRfidMark, CHCheckImei14Mark) VALUES('" + Zhidan + "','" + SimMark + "','" + VipMark + "','" + BatMark + "','" + IccidMark + "','" + MacMark + "','" + EquipmentMark + "','" + RfidMark + "','" + NoCheckMark + "','" + NoPaperMark + "','" + UpdataSimMark + "','" + UpdateIMEIMark + "','" + AutoTestMark + "'," + CoupleMark + ",'" + WriteImeiMark + "','"+ ParamDownloadMark +"'," + TemPlate1Num + ",'" + TemPlate2Num + "','" + GpsMark + "','" + CHCheckSnMark + "','" + CHCheckSimMark + "'," + CHCheckBatMark + ",'" + CHCheckIccidMark + "','" + CHCheckMacMark + "'," + CHCheckEquipmentMark + ",'" + CHCheckVipMark + "','" + CHCheckRfidMark + "','"+ CHCheckImei14Mark + "')";
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
                command.CommandText = "SELECT SimMark,VipMark,BatMark,IccidMark,MacMark,EquipmentMark,RfidMark, NoCheckMark,NoPaperMark,UpdataSimMark,UpdateIMEIMark,AutoTestMark,CoupleMark,WriteImeiMark,ParamDownloadMark,TemPlate1Num,TemPlate2Num,GPSMark,  CHCheckSnMark,  CHCheckSimMark,  CHCheckBatMark,  CHCheckIccidMark,  CHCheckMacMark,  CHCheckEquipmentMark,  CHCheckVipMark,  CHCheckRfidMark ,CHCheckImei14Mark FROM dbo.Gps_ManuPrintRecordParam WHERE ZhiDan='" + ZhidanNum + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ManuPrintRecordParam()
                    {
                        SimMark = dr.IsDBNull(0) ? 0 : dr.GetInt32(0),
                        VipMark = dr.IsDBNull(1) ? 0 : dr.GetInt32(1),
                        BatMark = dr.IsDBNull(2) ? 0 : dr.GetInt32(2),
                        IccidMark = dr.IsDBNull(3) ? 0 : dr.GetInt32(3),
                        MacMark = dr.IsDBNull(4) ? 0 : dr.GetInt32(4),
                        EquipmentMark = dr.IsDBNull(5) ? 0 : dr.GetInt32(5),
                        RfidMark = dr.IsDBNull(6) ? 0 : dr.GetInt32(6),
                        NoCheckMark = dr.IsDBNull(7) ? 0 : dr.GetInt32(7),
                        NoPaperMark = dr.IsDBNull(8) ? 0 : dr.GetInt32(8),
                        UpdataSimMark = dr.IsDBNull(9) ? 0 : dr.GetInt32(9),
                        UpdateIMEIMark = dr.IsDBNull(10) ? 0 : dr.GetInt32(10),
                        AutoTestMark = dr.IsDBNull(11) ? 0 : dr.GetInt32(11),
                        CoupleMark = dr.IsDBNull(12) ? 0 : dr.GetInt32(12),
                        WriteImeiMark = dr.IsDBNull(13) ? 0 : dr.GetInt32(13),
                        ParamDownloadMark = dr.IsDBNull(14) ? 0 : dr.GetInt32(14),
                        TemPlate1Num = dr.IsDBNull(15) ? 1: dr.GetInt32(15),
                        TemPlate2Num = dr.IsDBNull(16) ? 1 : dr.GetInt32(16),
                        GPSMark = dr.IsDBNull(17) ? 0 : dr.GetInt32(17),

                        CHCheckSnMark = dr.IsDBNull(18) ? 0 : dr.GetInt32(18),
                        CHCheckSimMark = dr.IsDBNull(19) ? 0 : dr.GetInt32(19),
                        CHCheckBatMark = dr.IsDBNull(20) ? 0 : dr.GetInt32(20),
                        CHCheckIccidMark = dr.IsDBNull(21) ? 0 : dr.GetInt32(21),
                        CHCheckMacMark = dr.IsDBNull(22) ? 0 : dr.GetInt32(22),
                        CHCheckEquipmentMark = dr.IsDBNull(23) ? 0 : dr.GetInt32(23),
                        CHCheckVipMark = dr.IsDBNull(24) ? 0 : dr.GetInt32(24),
                        CHCheckRfidMark = dr.IsDBNull(25) ? 0 : dr.GetInt32(25),
                        CHCheckImei14Mark = dr.IsDBNull(26) ? 0 : dr.GetInt32(26),
                    });
                }
                return list;
            }
        }

    }
}
