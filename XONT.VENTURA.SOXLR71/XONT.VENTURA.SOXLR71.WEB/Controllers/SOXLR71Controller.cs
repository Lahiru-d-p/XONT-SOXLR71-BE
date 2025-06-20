using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Web;
using XONT.Common.Message;
using XONT.Ventura.AppConsole;

namespace XONT.VENTURA.SOXLR71
{
    [ApiController]
    [Route("api/SOXLR71")]
    public class SOXLR71Controller : ControllerBase
    {

        private ReportBLL _bll;
        private User _user = null;
        private BusinessUnit _bUnit = null;
        private MessageSet _msg = null;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SOXLR71Controller(IHttpContextAccessor httpContextAccessor)
        {
            _bll = new ReportBLL();
            _httpContextAccessor = httpContextAccessor;
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                _user = session.GetObject<User>("Main_LoginUser");
                _bUnit = session.GetObject<BusinessUnit>("Main_BusinessUnitDetail");
            }
            _msg = null;
        }

        [Route("~/api/SOXLR71/GetTerritoryPrompt")]
        [HttpGet]
        public IActionResult GetTerritoryPrompt()
        {
            try
            {
                DataTable dt = _bll.GetTerritoryPrompt(_user.BusinessUnit.Trim(), ref _msg);

                if (_msg != null)
                {
                    return GetErrorMessageResponse(_msg);
                }
                return Ok(dt);
            }
            catch (Exception ex)
            {
                return GetErrorMessageRespose(ex, "GetTerritoryPrompt");
            }
        }

        [Route("~/api/SOXLR71/GetDistributorPrompt")]
        [HttpGet]
        public IActionResult GetDistributorPrompt()
        {
            try
            {
                DataTable dt = _bll.GetDistributorPrompt(_user.BusinessUnit.Trim(), ref _msg);

                if (_msg != null)
                {
                    return GetErrorMessageResponse(_msg);
                }
                return Ok(dt);
            }
            catch (Exception ex)
            {
                return GetErrorMessageRespose(ex, "GetDistributorPrompt");
            }
        }

        [Route("~/api/SOXLR71/GenerateExcel")]
        [HttpPost]
        public IActionResult GenerateExcel([FromBody] dynamic data)
        {
            try
            {
                _bll = new ReportBLL();
                byte[] excelBytes;

                Selection selection = data.SelectionCriteria.ToObject<Selection>();

                var webHostEnvironment = _httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
                if (webHostEnvironment != null)
                {
                    selection.ImagePath = Path.Combine(webHostEnvironment.WebRootPath, "SOXLR71", "logo.jpg");
                }

                if (_user != null)
                {
                    selection.BusinessUnit = _user.BusinessUnit;
                    selection.UserName = _user.UserName;
                    selection.PowerUser = _user.PowerUser;
                }

                DataTable rptSource = new DataTable();
                ControlData controlData = _bll.GetControlData(_user.BusinessUnit.Trim(), out _msg);
                BusinessUnit businessUnit = _bll.GetBusinessUnit(_user.BusinessUnit, ref _msg);

                rptSource = _bll.GetReportData(selection, out _msg);

                if (_msg != null)
                    return GetErrorMessageResponse(_msg);

                if (rptSource.Rows.Count > 0)
                {
                    DataTable dtLogo = _bll.GetBusinessUnitLogo(_user.BusinessUnit, out _msg);

                    if (_msg != null)
                        return GetErrorMessageResponse(_msg);

                    var session = _httpContextAccessor.HttpContext?.Session;
                    if (session != null)
                    {
                        session.SetObject("Main_BusinessUnitLogo", dtLogo);
                    }

                    string reportName = string.Format("SOXLR71{0}.xlsx", DateTime.Now.Ticks);

                    if (selection.ReportDetailFlag)
                        excelBytes = _bll.GenerateExcelByDetail(selection, rptSource, dtLogo, reportName, controlData, businessUnit, ref _msg);
                    else
                        excelBytes = _bll.GenerateExcelBySummary(selection, rptSource, dtLogo, reportName, controlData, businessUnit, ref _msg);

                    return Ok(new { fileContents = excelBytes, ReportName = reportName });
                }
                else
                {
                    _msg = MessageCreate.CreateUserMessage(200008, "", "", "", "", "", "");
                    return GetErrorMessageResponse(_msg);
                }
            }
            catch (Exception ex)
            {
                return GetErrorMessageRespose(ex, "MakeReportReady");
            }
        }

        #region Error Message Handling
        private IActionResult GetErrorMessageResponse(MessageSet msg)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, msg);
        }

        private IActionResult GetErrorMessageRespose(Exception ex, string methodName)
        {
            _msg = MessageCreate.CreateErrorMessage(0, ex, methodName, "XONT.VENTURA.SOXLR71.WEB.dll");
            return GetErrorMessageResponse(_msg);
        }
        #endregion
    }
    public static class SessionExtensions
    {
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
    }
}