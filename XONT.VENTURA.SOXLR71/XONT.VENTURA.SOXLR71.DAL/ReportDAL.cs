using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using XONT.Common.Data;
using XONT.Common.Message;
using XONT.Ventura.AppConsole;
using XONT.VENTURA.SOXLR71.DOMAIN;

namespace XONT.VENTURA.SOXLR71.DAL
{

    public class ReportDAL
    {
        private DataTable dTable;
        private readonly string _userDbConnectionString;
        private readonly string _systemDbConnectionString;
        private readonly IConfiguration _configuration;
        public ReportDAL(IConfiguration configuration)
        {
            _configuration = configuration;
            _userDbConnectionString = _configuration.GetConnectionString("UserDB");
            _systemDbConnectionString = _configuration.GetConnectionString("SystemDB");
        }
        public DataTable GetTerritoryPrompt(string businessUnit, ref MessageSet message)
        {
            DataTable dtResults = null;
            try
            {
                SqlParameter[] parameters =
                   {
                new SqlParameter("@BusinessUnit", SqlDbType.VarChar) { Value = businessUnit },
                new SqlParameter("@ExecutionType", SqlDbType.Char) { Value = "1" }
            };
                dtResults = ExecuteStoredProcedure(_userDbConnectionString, "[RD].[usp_SOXLR71GetPromptData]", parameters);
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(0, ex, "GetTerritoryPrompt", "XONT.VENTURA.SOXLR71.DAL");
            }
            return dtResults;
        }

        public DataTable GetDistributorPrompt(string businessUnit, ref MessageSet message)
        {
            DataTable dtResults = null;
            try
            {
                SqlParameter[] parameters =
                   {
                new SqlParameter("@BusinessUnit", SqlDbType.VarChar) { Value = businessUnit },
                new SqlParameter("@ExecutionType", SqlDbType.Char) { Value = "2" }
            };
                dtResults = ExecuteStoredProcedure(_userDbConnectionString, "[RD].[usp_SOXLR71GetPromptData]", parameters);

            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(0, ex, "GetDistributorPrompt", "XONT.VENTURA.SOXLR71.DAL");
            }
            return dtResults;
        }

        public DataTable GetReportData(Selection selection, out MessageSet msg)
        {
            msg = null;
            DataTable dTable = new DataTable();

            try
            {
                string VATOnlyFlag = selection.VATFlag ? "1" : "0";
                string NonVATOnlyFlag = selection.NonVATFlag ? "1" : "0";
                string ReportTypeFlag = selection.ReportDetailFlag ? "detail" : "summary";

                SqlParameter[] parameters =
                {
            new SqlParameter("@BusinessUnit", SqlDbType.VarChar) { Value = selection.BusinessUnit  },
            new SqlParameter("@DistributorCode", SqlDbType.VarChar) { Value = selection.DistributorCode  },
            new SqlParameter("@TerritoryCode", SqlDbType.VarChar) { Value = selection.TerritoryCode },
            new SqlParameter("@FromDate", SqlDbType.VarChar) { Value = selection.FromDate },
            new SqlParameter("@ToDate", SqlDbType.VarChar) { Value = selection.ToDate },
            new SqlParameter("@ExecutionType", SqlDbType.VarChar) { Value = "3" },
            new SqlParameter("@VATOnlyFlag", SqlDbType.Char) { Value = VATOnlyFlag },
            new SqlParameter("@NonVATOnlyFlag", SqlDbType.Char) { Value = NonVATOnlyFlag },
            new SqlParameter("@ReportTypeFlag", SqlDbType.VarChar) { Value = ReportTypeFlag }
        };

                dTable = ExecuteStoredProcedure(_userDbConnectionString, "[RD].[usp_SOXLR71GetReportData]", parameters);
            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetReportData", "XONT.VENTURA.SOXLR71.DAL.dll");
            }

            return dTable;
        }

        public DataTable GetDistributorVATRegNo(Selection selection, string businessUnit, ref MessageSet message)
        {
            DataTable dtResults = null;
            try
            {
                string executionType = (selection.DistributorFlag && !selection.TerritoryFlag) ? "1" : "2";

                SqlParameter[] parameters =
                {
                new SqlParameter("@BusinessUnit", SqlDbType.VarChar) { Value = businessUnit },
                new SqlParameter("@DistributorCode", SqlDbType.VarChar) { Value = selection.DistributorCode  },
                new SqlParameter("@TerritoryCode", SqlDbType.VarChar) { Value = selection.TerritoryCode },
                new SqlParameter("@ExecutionType", SqlDbType.Char) { Value = executionType }
            };
                dtResults = ExecuteStoredProcedure(_userDbConnectionString, "[RD].[usp_SOXLR71GetReportData]", parameters);
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(0, ex, "GetDistributorVATRegNo", "XONT.VENTURA.SOXLR71.DAL");
            }
            return dtResults;
        }

        public ControlData GetControlData(string businessUnit, out MessageSet msg)
        {
            msg = null;
            var controlData = new ControlData();
            try
            {
                string query = @" SELECT AllowDecimalPointFlag, DecimalPlaces from RD.Control  where BusinessUnit= @BusinessUnit";
                SqlParameter[] parameters =
                {
                new SqlParameter("@BusinessUnit",SqlDbType.VarChar){ Value=businessUnit}

            };
                dTable = ExecuteQuery(_userDbConnectionString, query, parameters);
                if (dTable.Rows.Count > 0)
                {
                    var row = dTable.Rows[0];
                    controlData.AllowDecimalPointFlag = row["AllowDecimalPointFlag"].ToString().Trim();
                    int decimalplace = 0;
                    int.TryParse(row["DecimalPlaces"].ToString().Trim(), out decimalplace);
                    controlData.DecimalPlaces = decimalplace;
                }

            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetControlData", "XONT.VENTURA.SOXLR71.DAL.dll");
            }
            return controlData;
        }

        public BusinessUnit GetBusinessUnit(string businessUnit, ref MessageSet msg)
        {
            BusinessUnit bu = new BusinessUnit();
            DataTable dtResult = new DataTable();
            try
            {
                string query = @"SELECT ISNULL(ZYBusinessUnit.BusinessUnit,'') AS BusinessUnit ,ISNULL(ZYBusinessUnit.BusinessUnitName,'') AS BusinessUnitName
                ,ISNULL(ZYBusinessUnit.AddressLine1,'') AS AddressLine1  ,ISNULL(ZYBusinessUnit.AddressLine2,'') AS AddressLine2
                ,ISNULL(ZYBusinessUnit.AddressLine3,'') AS AddressLine3  ,ISNULL(ZYBusinessUnit.AddressLine4,'') AS AddressLine4
                ,ISNULL(ZYBusinessUnit.AddressLine5,'') AS AddressLine5  ,ISNULL(ZYBusinessUnit.PostCode,'') AS PostCode
                ,ISNULL(ZYBusinessUnit.TelephoneNumber,'') AS TelephoneNumber ,ISNULL(ZYBusinessUnit.FaxNumber,'') AS FaxNumber
                ,ISNULL(ZYBusinessUnit.EmailAddress,'') AS EmailAddress ,ISNULL(ZYBusinessUnit.WebAddress,'') AS WebAddress
                ,ISNULL(ZYBusinessUnit.VATRegistrationNumber,'') AS VATRegistrationNumber  ,ZYBusinessUnit.Logo 
                FROM  ZYBusinessUnit with(nolock)  WHERE (ZYBusinessUnit.BusinessUnit= @BusinessUnit)";

                SqlParameter[] parameters =
                {
                new SqlParameter("@BusinessUnit", SqlDbType.VarChar) { Value = businessUnit }
            };
                dtResult = ExecuteQuery(_systemDbConnectionString, query, parameters);

                if (dtResult.Rows.Count > 0)
                {
                    var row = dtResult.Rows[0];
                    bu.BusinessUnitCode = row["Businessunit"].ToString().Trim();
                    bu.BusinessUnitName = row["BusinessUnitName"].ToString().Trim();
                    bu.AddressLine1 = row["AddressLine1"].ToString().Trim();
                    bu.AddressLine2 = row["AddressLine2"].ToString().Trim();
                    bu.AddressLine3 = row["AddressLine3"].ToString().Trim();
                    bu.AddressLine4 = row["AddressLine4"].ToString().Trim();
                    bu.AddressLine5 = row["AddressLine5"].ToString().Trim();
                    bu.PostCode = row["PostCode"].ToString().Trim();
                    bu.TelephoneNumber = row["TelephoneNumber"].ToString().Trim();
                    bu.FaxNumber = row["FaxNumber"].ToString().Trim();
                    bu.EmailAddress = row["EmailAddress"].ToString().Trim();
                    bu.WebAddress = row["WebAddress"].ToString().Trim();
                    bu.VATRegistrationNumber = row["VATRegistrationNumber"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetBusinessUnit", "XONT.VENTURA.SOXLR71.DAL.dll");
            }
            return bu;
        }

        public DataTable GetBusinessUnitLogo(string businessUnit, out MessageSet msg)
        {
            msg = null;
            DataTable dTable = null;

            try
            {
                string query = @"
                SELECT BusinessUnit, Logo,
                       ShowLogo = CASE WHEN Logo IS NULL THEN 0 ELSE 1 END
                FROM ZYBusinessUnit
                WHERE BusinessUnit = @BusinessUnit";

                SqlParameter[] parameters =
                {
                new SqlParameter("@BusinessUnit", SqlDbType.VarChar) { Value = businessUnit }
            };

                dTable = ExecuteQuery(_systemDbConnectionString, query, parameters);
            }
            catch (Exception ex)
            {
                msg = MessageCreate.CreateErrorMessage(0, ex, "GetBusinessUnitLogo", "XONT.VENTURA.SOXLR71.DAL.dll");
            }

            return dTable;
        }
        public DataTable ExecuteStoredProcedure(string connectionString, string spName, SqlParameter[] parameters)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parameters);

                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable ExecuteQuery(string connectionString, string query, SqlParameter[] parameters)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddRange(parameters);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }

            return dt;
        }


    }
}
