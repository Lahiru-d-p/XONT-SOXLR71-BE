using System;
using System.Collections.Generic;
using System.Data;
using XONT.Common.Data;
using XONT.Common.Message;
using System.Text;
using XONT.Ventura.AppConsole;

namespace XONT.VENTURA.SOXLR71
{
    public class ReportDAL
    {
        private CommonDBService dbService;
        ParameterSet paramSet;
        List<SPParameter> spParametersList;
        private DataTable dTable;

        public ReportDAL()
        {
            dbService = new CommonDBService();
        }

        public DataTable GetTerritoryPrompt(string businessUnit, ref MessageSet message)
        {
            DataTable dtResults = null;
            try
            {
                ParameterSet paramSet = new ParameterSet();
                List<SPParameter> spParametersList = new List<SPParameter>();
                paramSet.SetSPParameterList(spParametersList, "BusinessUnit", businessUnit, "");
                paramSet.SetSPParameterList(spParametersList, "ExecutionType", "1", "");

                dbService.StartService();
                dtResults = dbService.FillDataTable(CommonVar.DBConName.UserDB, "[RD].[usp_SOXLR71GetPromptData]", spParametersList);
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(0, ex, "GetTerritoryPrompt", "XONT.VENTURA.SOXLR71.DAL");
            }
            finally
            {
                dbService.CloseService();
            }

            return dtResults;
        }

        public DataTable GetDistributorPrompt(string businessUnit, ref MessageSet message)
        {
            DataTable dtResults = null;
            try
            {
                ParameterSet paramSet = new ParameterSet();
                List<SPParameter> spParametersList = new List<SPParameter>();
                paramSet.SetSPParameterList(spParametersList, "BusinessUnit", businessUnit, "");
                paramSet.SetSPParameterList(spParametersList, "ExecutionType", "2", "");

                dbService.StartService();
                dtResults = dbService.FillDataTable(CommonVar.DBConName.UserDB, "[RD].[usp_SOXLR71GetPromptData]", spParametersList);
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(0, ex, "GetDistributorPrompt", "XONT.VENTURA.SOXLR71.DAL");
            }
            finally
            {
                dbService.CloseService();
            }

            return dtResults;
        }

        public DataTable GetReportData(Selection selection, out MessageSet msg)
        {
            msg = null;
            try
            {
                paramSet = new ParameterSet();
                spParametersList = new List<SPParameter>();
                string VATOnlyFlag = "";
                string NonVATOnlyFlag = "";
                string ReportTypeFlag = "";

                if (selection.VATFlag)
                    VATOnlyFlag = "1";
                else
                    VATOnlyFlag = "0";

                if (selection.NonVATFlag)
                    NonVATOnlyFlag = "1";
                else
                    NonVATOnlyFlag = "0";

                if (selection.ReportDetailFlag)
                    ReportTypeFlag = "detail";
                else
                    ReportTypeFlag = "summary";

                paramSet.SetSPParameterList(spParametersList, "BusinessUnit", selection.BusinessUnit, "");
                paramSet.SetSPParameterList(spParametersList, "DistributorCode", selection.DistributorCode, "");
                paramSet.SetSPParameterList(spParametersList, "TerritoryCode", selection.TerritoryCode, "");
                paramSet.SetSPParameterList(spParametersList, "FromDate", selection.FromDate, "");
                paramSet.SetSPParameterList(spParametersList, "ToDate", selection.ToDate, "");
                paramSet.SetSPParameterList(spParametersList, "ExecutionType", "3", "");
                paramSet.SetSPParameterList(spParametersList, "VATOnlyFlag", VATOnlyFlag, "");
                paramSet.SetSPParameterList(spParametersList, "NonVATOnlyFlag", NonVATOnlyFlag, "");
                paramSet.SetSPParameterList(spParametersList, "ReportTypeFlag", ReportTypeFlag, "");

                dbService.StartService();
                dTable = dbService.FillDataTable(CommonVar.DBConName.UserDB, "[RD].[usp_SOXLR71GetReportData]", spParametersList);
            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetReportDataByDetail", "XONT.VENTURA.SOXLR71.DAL.dll");
            }
            finally
            {
                dbService.CloseService();
            }
            return dTable;
        }

        public DataTable GetDistributorVATRegNo(Selection selection, string businessUnit, ref MessageSet message)
        {
            DataTable dtResults = null;
            try
            {
                ParameterSet paramSet = new ParameterSet();
                List<SPParameter> spParametersList = new List<SPParameter>();
                paramSet.SetSPParameterList(spParametersList, "BusinessUnit", businessUnit, "");
                paramSet.SetSPParameterList(spParametersList, "DistributorCode", selection.DistributorCode, "");
                paramSet.SetSPParameterList(spParametersList, "TerritoryCode", selection.TerritoryCode, "");

                if (selection.DistributorFlag && !selection.TerritoryFlag)
                    paramSet.SetSPParameterList(spParametersList, "ExecutionType", "1", "");
                else
                    paramSet.SetSPParameterList(spParametersList, "ExecutionType", "2", "");

                dbService.StartService();
                dtResults = dbService.FillDataTable(CommonVar.DBConName.UserDB, "[RD].[usp_SOXLR71GetReportData]", spParametersList);
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(0, ex, "GetDistributorVATRegNo", "XONT.VENTURA.SOXLR71.DAL");
            }
            finally
            {
                dbService.CloseService();
            }

            return dtResults;
        }

        public ControlData GetControlData(string businessUnit, out MessageSet msg)
        {
            msg = null;
            var controlData = new ControlData();
            try
            {
                string command = " SELECT AllowDecimalPointFlag, DecimalPlaces";
                command += $"  from RD.Control   ";
                command += $" where BusinessUnit='{businessUnit}'";

                dbService.StartService();
                dTable = dbService.FillDataTable(CommonVar.DBConName.UserDB, command);
                if (dTable.Rows.Count > 0)
                {
                    controlData.AllowDecimalPointFlag = dTable.Rows[0]["AllowDecimalPointFlag"].ToString().Trim();
                    int decimalplace = 0;
                    int.TryParse(dTable.Rows[0]["DecimalPlaces"].ToString().Trim(), out decimalplace);
                    controlData.DecimalPlaces = decimalplace;
                }

            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetControlData", "XONT.VENTURA.SOXLR71.DAL.dll");
            }
            finally
            {
                dbService.CloseService();
            }

            return controlData;
        }

        public BusinessUnit GetBusinessUnit(string businessUnit, ref MessageSet msg)
        {
            BusinessUnit bu = new BusinessUnit();
            DataTable dtResult = new DataTable();
            try
            {
                string str = "SELECT ISNULL(ZYBusinessUnit.BusinessUnit,'') AS BusinessUnit " + " ,ISNULL(ZYBusinessUnit.BusinessUnitName,'') AS BusinessUnitName " + " ,ISNULL(ZYBusinessUnit.AddressLine1,'') AS AddressLine1 " + " ,ISNULL(ZYBusinessUnit.AddressLine2,'') AS AddressLine2 " + " ,ISNULL(ZYBusinessUnit.AddressLine3,'') AS AddressLine3 " + " ,ISNULL(ZYBusinessUnit.AddressLine4,'') AS AddressLine4 " + " ,ISNULL(ZYBusinessUnit.AddressLine5,'') AS AddressLine5 " + " ,ISNULL(ZYBusinessUnit.PostCode,'') AS PostCode " + " ,ISNULL(ZYBusinessUnit.TelephoneNumber,'') AS TelephoneNumber " + " ,ISNULL(ZYBusinessUnit.FaxNumber,'') AS FaxNumber " + " ,ISNULL(ZYBusinessUnit.EmailAddress,'') AS EmailAddress " + " ,ISNULL(ZYBusinessUnit.WebAddress,'') AS WebAddress " + " ,ISNULL(ZYBusinessUnit.VATRegistrationNumber,'') AS VATRegistrationNumber " + " ,ZYBusinessUnit.Logo " + " FROM  ZYBusinessUnit with(nolock)" + " WHERE (ZYBusinessUnit.BusinessUnit='" + businessUnit + "')";

                dbService.StartService();
                dtResult = dbService.FillDataTable(CommonVar.DBConName.SystemDB, str);

                if (dtResult.Rows.Count > 0)
                {
                    bu.BusinessUnitCode = dtResult.Rows[0]["Businessunit"].ToString().Trim();
                    bu.BusinessUnitName = dtResult.Rows[0]["BusinessUnitName"].ToString().Trim();
                    bu.AddressLine1 = dtResult.Rows[0]["AddressLine1"].ToString().Trim();
                    bu.AddressLine2 = dtResult.Rows[0]["AddressLine2"].ToString().Trim();
                    bu.AddressLine3 = dtResult.Rows[0]["AddressLine3"].ToString().Trim();
                    bu.AddressLine4 = dtResult.Rows[0]["AddressLine4"].ToString().Trim();
                    bu.AddressLine5 = dtResult.Rows[0]["AddressLine5"].ToString().Trim();
                    bu.PostCode = dtResult.Rows[0]["PostCode"].ToString().Trim();
                    bu.TelephoneNumber = dtResult.Rows[0]["TelephoneNumber"].ToString().Trim();
                    bu.FaxNumber = dtResult.Rows[0]["FaxNumber"].ToString().Trim();
                    bu.EmailAddress = dtResult.Rows[0]["EmailAddress"].ToString().Trim();
                    bu.WebAddress = dtResult.Rows[0]["WebAddress"].ToString().Trim();
                    bu.VATRegistrationNumber = dtResult.Rows[0]["VATRegistrationNumber"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetBusinessUnit", "XONT.VENTURA.SOXLR71.DAL.dll");
            }
            finally
            {
                dbService.CloseService();
            }
            return bu;
        }

        public DataTable GetBusinessUnitLogo(string businessUnit, out MessageSet msg)
        {
            msg = null;
            try
            {
                StringBuilder command = new StringBuilder("SELECT  BusinessUnit,Logo");
                command.Append(" ,ShowLogo = CASE WHEN Logo IS NULL THEN 0 ELSE 1 END");
                command.Append(" FROM  ZYBusinessUnit ");
                command.Append($" WHERE (ZYBusinessUnit.BusinessUnit='{businessUnit}')");

                dbService.StartService();
                dTable = dbService.FillDataTable(CommonVar.DBConName.SystemDB, command.ToString());
            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetBusinessUnitLogo", "XONT.VENTURA.SOXLR71.DAL.dll");
            }
            finally
            {
                dbService.CloseService();
            }
            return dTable;
        }

    }
}
