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

        public int InsertFuselagePrintRecordParamDAL(string Zhidan, int PrintOneByOneMark, int PltplotMark, int CustomerSupplySNMark, int NoPrintCheckCodeMark, int NoPrintingSNMark, int IMEIHexadecimalMark, int SNHexadecimalMark, int ReplayOneByOneMark, int BattingInBatchesMark, int NoParityBitMark, int HexadecimalMark,
           int JSCheckSnMark, int JSCheckSimMark, int JSCheckBatMark, int JSCheckIccidMark, int JSCheckMacMark, int JSCheckEquipmentMark, int JSCheckVipMark, int JSCheckRfidMark, int PrintMode1, int PrintMode2, int JSCheckIMEI2Mark, int JSCheckInseIMEI2Mark)
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
                        command.CommandText = "UPDATE dbo.Gps_ManuPrintRecordParam SET PrintOneByOneMark ='" + PrintOneByOneMark + "',PltplotMark = '" + PltplotMark + "',CustomerSupplySNMark ='" + CustomerSupplySNMark + "',NoPrintCheckCodeMark = '" + NoPrintCheckCodeMark + "',NoPrintingSNMark ='" + NoPrintingSNMark + "',IMEIHexadecimalMark = '" + IMEIHexadecimalMark + "',SNHexadecimalMark ='" + SNHexadecimalMark + "',ReplayOneByOneMark = '" + ReplayOneByOneMark + "',BattingInBatchesMark ='" + BattingInBatchesMark + "',NoParityBitMark = '" + NoParityBitMark + "',HexadecimalMark ='" + HexadecimalMark +
                              "',JSCheckSnMark = '" + JSCheckSnMark + "',JSCheckSimMark ='" + JSCheckSimMark + "',JSCheckBatMark = '" + JSCheckBatMark + "',JSCheckIccidMark ='" + JSCheckIccidMark + "',JSCheckMacMark = '" + JSCheckMacMark + "',JSCheckEquipmentMark ='" + JSCheckEquipmentMark + "',JSCheckVipMark = '" + JSCheckVipMark + "',JSCheckRfidMark ='" + JSCheckRfidMark + "',PrintMode1 ='" + PrintMode1 + "',PrintMode2 ='" + PrintMode2 + "',JSCheckIMEI2Mark ='" + JSCheckIMEI2Mark + "',JSCheckInseIMEI2Mark ='" + JSCheckInseIMEI2Mark + "' WHERE ZhiDan='" + Zhidan + "'";
                        httpstr = command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO dbo.Gps_ManuPrintRecordParam(ZhiDan,PrintOneByOneMark,PltplotMark,CustomerSupplySNMark,NoPrintCheckCodeMark,NoPrintingSNMark,IMEIHexadecimalMark,SNHexadecimalMark,ReplayOneByOneMark,BattingInBatchesMark,NoParityBitMark,HexadecimalMark , JSCheckSnMark,  JSCheckSimMark,  JSCheckBatMark,  JSCheckIccidMark,  JSCheckMacMark,  JSCheckEquipmentMark,  JSCheckVipMark,  JSCheckRfidMark, PrintMode1, PrintMode2, JSCheckIMEI2Mark, JSCheckInseIMEI2Mark) VALUES('" + 
                        Zhidan + "','" + PrintOneByOneMark + "','" + PltplotMark + "','" + CustomerSupplySNMark + "','" + NoPrintCheckCodeMark + "','" + NoPrintingSNMark + "','" + IMEIHexadecimalMark + "','" + SNHexadecimalMark + "','" + ReplayOneByOneMark + "','" + BattingInBatchesMark + "','" + NoParityBitMark + "','" + HexadecimalMark + "','" + JSCheckSnMark + "','" + JSCheckSimMark + "','" + JSCheckBatMark + "','" + JSCheckIccidMark + "','" + JSCheckMacMark + "','" + JSCheckEquipmentMark + "','" + JSCheckVipMark + "','" + JSCheckRfidMark + "','" + PrintMode1 + "','" + PrintMode2 +"' ,'"+ JSCheckIMEI2Mark + "','"+ JSCheckInseIMEI2Mark + "')";
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
                command.CommandText = "SELECT PrintOneByOneMark,PltplotMark,CustomerSupplySNMark,NoPrintCheckCodeMark,NoPrintingSNMark,IMEIHexadecimalMark,SNHexadecimalMark,ReplayOneByOneMark,BattingInBatchesMark,NoParityBitMark,HexadecimalMark,JSCheckSnMark,  JSCheckSimMark,  JSCheckBatMark,  JSCheckIccidMark,  JSCheckMacMark,  JSCheckEquipmentMark,  JSCheckVipMark,  JSCheckRfidMark, PrintMode1, PrintMode2, JSCheckIMEI2Mark, JSCheckInseIMEI2Mark FROM dbo.Gps_ManuPrintRecordParam WHERE ZhiDan='" + ZhidanNum + "'";
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
                        Hexadecimal = dr.IsDBNull(10) ? 0 : dr.GetInt32(10),

                        JSCheckSnMark = dr.IsDBNull(11) ? 0 : dr.GetInt32(11),
                        JSCheckSimMark = dr.IsDBNull(12) ? 0 : dr.GetInt32(12),
                        JSCheckBatMark = dr.IsDBNull(13) ? 0 : dr.GetInt32(13),
                        JSCheckIccidMark = dr.IsDBNull(14) ? 0 : dr.GetInt32(14),
                        JSCheckMacMark = dr.IsDBNull(15) ? 0 : dr.GetInt32(15),
                        JSCheckEquipmentMark = dr.IsDBNull(16) ? 0 : dr.GetInt32(16),
                        JSCheckVipMark = dr.IsDBNull(17) ? 0 : dr.GetInt32(17),
                        JSCheckRfidMark = dr.IsDBNull(18) ? 0 : dr.GetInt32(18),

                        PrintMode1 = dr.IsDBNull(19) ? 0 : dr.GetInt32(19),
                        PrintMode2 = dr.IsDBNull(20) ? 0 : dr.GetInt32(20),
                        JSCheckIMEI2Mark = dr.IsDBNull(21) ? 0 : dr.GetInt32(21) ,
                        JSCheckInseIMEI2Mark = dr.IsDBNull(22) ? 0 : dr.GetInt32(22),
                    });
                }
                return list;
            }
        }
    }
}
