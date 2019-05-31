using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Print_Message;

namespace FuselagePrintRecord.Param.DAL
{
    class FuselagePrintRecordParamDAL
    {
        string conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;

        public void refreshCon()
        {
            conStr = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;
        }

        public int InsertFuselagePrintRecordParamDAL(string Zhidan, int PrintOneByOneMark, int PltplotMark, int CustomerSupplySNMark, int NoPrintCheckCodeMark, int NoPrintingSNMark, int IMEIHexadecimalMark, int SNHexadecimalMark, int ReplayOneByOneMark, int BattingInBatchesMark, int NoParityBitMark, int HexadecimalMark)
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
                        command.CommandText = "UPDATE dbo.Gps_ManuPrintRecordParam SET PrintOneByOneMark ='" + PrintOneByOneMark + "',PltplotMark = '" + PltplotMark + "',CustomerSupplySNMark ='" + CustomerSupplySNMark + "',NoPrintCheckCodeMark = '" + NoPrintCheckCodeMark + "',NoPrintingSNMark ='" + NoPrintingSNMark + "',IMEIHexadecimalMark = '" + IMEIHexadecimalMark + "',SNHexadecimalMark ='" + SNHexadecimalMark + "',ReplayOneByOneMark = '" + ReplayOneByOneMark + "',BattingInBatchesMark ='" + BattingInBatchesMark + "',NoParityBitMark = '" + NoParityBitMark + "',HexadecimalMark ='" + HexadecimalMark + "' WHERE ZhiDan='" + Zhidan + "'";
                        httpstr = command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO dbo.Gps_ManuPrintRecordParam(ZhiDan,PrintOneByOneMark,PltplotMark,CustomerSupplySNMark,NoPrintCheckCodeMark,NoPrintingSNMark,IMEIHexadecimalMark,SNHexadecimalMark,ReplayOneByOneMark,BattingInBatchesMark,NoParityBitMark,HexadecimalMark) VALUES('" + Zhidan + "','" + PrintOneByOneMark + "','" + PltplotMark + "','" + CustomerSupplySNMark + "','" + NoPrintCheckCodeMark + "','" + NoPrintingSNMark + "','" + IMEIHexadecimalMark + "','" + SNHexadecimalMark + "','" + ReplayOneByOneMark + "','" + BattingInBatchesMark + "','" + NoParityBitMark + "','" + HexadecimalMark + "')";
                        httpstr = command.ExecuteNonQuery();
                    }
                    return httpstr;
                }
            }
        }

        //根据制单号返回该制单相关信息
        public List<ManuFuselagePrintRecordParam> selectFuselageRecordParamByzhidanDAL(string ZhidanNum)
        {
            SqlConnection conn1 = new SqlConnection(conStr);
            conn1.Open();
            List<ManuFuselagePrintRecordParam> list = new List<ManuFuselagePrintRecordParam>();
            using (SqlCommand command = conn1.CreateCommand())
            {
                command.CommandText = "SELECT PrintOneByOneMark,PltplotMark,CustomerSupplySNMark,NoPrintCheckCodeMark,NoPrintingSNMark,IMEIHexadecimalMark,SNHexadecimalMark,ReplayOneByOneMark,BattingInBatchesMark,NoParityBitMark,HexadecimalMark FROM dbo.Gps_ManuPrintRecordParam WHERE ZhiDan='" + ZhidanNum + "'";
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ManuFuselagePrintRecordParam()
                    {

                        PrintOneByOne = dr.IsDBNull(0) ? 0 : dr.GetInt32(0),
                        Pltplot = dr.IsDBNull(1) ? 0 : dr.GetInt32(1),
                        CustomerSupplySN = dr.IsDBNull(2) ? 0 : dr.GetInt32(2),
                        NoPrintCheckCode = dr.IsDBNull(3) ? 0 : dr.GetInt32(3),
                        NoPrintingSN = dr.IsDBNull(4) ? 0 : dr.GetInt32(4),
                        IMEIHexadecimal = dr.IsDBNull(5) ? 0 : dr.GetInt32(5),
                        SNHexadecimal = dr.IsDBNull(6) ? 0 : dr.GetInt32(6),
                        ReplayOneByOne = dr.IsDBNull(7) ? 0 : dr.GetInt32(7),
                        BattingInBatches = dr.IsDBNull(8) ? 0 : dr.GetInt32(8),
                        NoParityBit = dr.IsDBNull(9) ? 0 : dr.GetInt32(9),
                        Hexadecimal = dr.IsDBNull(10) ? 0 : dr.GetInt32(10)
                    });
                }
                return list;
            }
        }
    }
}
