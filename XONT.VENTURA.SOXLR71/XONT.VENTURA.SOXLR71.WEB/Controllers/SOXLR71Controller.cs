using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;
using XONT.Common.Message;
using XONT.Ventura.AppConsole;
using XONT.VENTURA.SOXLR71.BLL;
using XONT.VENTURA.SOXLR71.DOMAIN;

namespace XONT.VENTURA.SOXLR71
{
    [ApiController]
    [Route("api/SOXLR71")]
    public class SOXLR71Controller : ControllerBase
    {

        private User _user = null;
        private BusinessUnit _bUnit = null;
        private MessageSet _msg = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ReportBLL _bll;
        private readonly IWebHostEnvironment _env;


        public SOXLR71Controller(IHttpContextAccessor httpContextAccessor, ReportBLL bll, IWebHostEnvironment env)
        {
            _httpContextAccessor = httpContextAccessor;
            _bll = bll;
            _env = env;

            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var userStr = session.GetString("Main_LoginUser");
                if (!string.IsNullOrWhiteSpace(userStr))
                {
                    _user = JsonConvert.DeserializeObject<User>(userStr);
                }

                var bUnitStr = session.GetString("Main_BusinessUnitDetail");
                if (!string.IsNullOrWhiteSpace(bUnitStr))
                {
                    _bUnit = JsonConvert.DeserializeObject<BusinessUnit>(bUnitStr);
                }
            }
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
        public IActionResult GenerateExcel([FromBody] ExcelGenerationSelection selection)
        {
            try
            {
                byte[] excelBytes;

                selection.SelectionCriteria.ImagePath = Path.Combine(_env.ContentRootPath, "SOXLR71", "logo.jpg");


                if (_user != null)
                {
                    selection.SelectionCriteria.BusinessUnit = _user.BusinessUnit;
                    selection.SelectionCriteria.UserName = _user.UserName;
                    selection.SelectionCriteria.PowerUser = _user.PowerUser;
                }

                DataTable rptSource = new DataTable();
                ControlData controlData = _bll.GetControlData(_user.BusinessUnit.Trim(), out _msg);
                BusinessUnit businessUnit = _bll.GetBusinessUnit(_user.BusinessUnit, ref _msg);

                rptSource = _bll.GetReportData(selection.SelectionCriteria, out _msg);

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
                        session.SetString("Main_BusinessUnitLogo", JsonConvert.SerializeObject(dtLogo));
                    }

                    string reportName = string.Format("SOXLR71{0}.xlsx", DateTime.Now.Ticks);

                    if (selection.SelectionCriteria.ReportDetailFlag)
                        excelBytes = _bll.GenerateExcelByDetail(selection.SelectionCriteria, rptSource, dtLogo, reportName, controlData, businessUnit, ref _msg);
                    else
                        excelBytes = _bll.GenerateExcelBySummary(selection.SelectionCriteria, rptSource, dtLogo, reportName, controlData, businessUnit, ref _msg);

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
}