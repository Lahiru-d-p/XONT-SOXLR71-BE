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
        public async Task<IActionResult> GetTerritoryPrompt()
        {
            try
            {
                var result = await _bll.GetTerritoryPrompt(_user.BusinessUnit.Trim());

                if (result.Message != null)
                {
                    return GetErrorMessageResponse(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return GetErrorMessageRespose(ex, "GetTerritoryPrompt");
            }
        }

        [Route("~/api/SOXLR71/GetDistributorPrompt")]
        [HttpGet]
        public async Task<IActionResult> GetDistributorPrompt()
        {
            try
            {
                var result = await _bll.GetDistributorPrompt(_user.BusinessUnit.Trim());

                if (result.Message != null)
                {
                    return GetErrorMessageResponse(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return GetErrorMessageRespose(ex, "GetDistributorPrompt");
            }
        }

        [Route("~/api/SOXLR71/GenerateExcel")]
        [HttpPost]
        public async Task<IActionResult> GenerateExcel([FromBody] ExcelGenerationSelection selection)
        {
            try
            {

                var excelResult = new Result<byte[]>();
                selection.SelectionCriteria.ImagePath = Path.Combine(_env.ContentRootPath, "SOXLR71", "logo.jpg");


                if (_user != null)
                {
                    selection.SelectionCriteria.BusinessUnit = _user.BusinessUnit;
                    selection.SelectionCriteria.UserName = _user.UserName;
                    selection.SelectionCriteria.PowerUser = _user.PowerUser;
                }

                var controlDataResult = await _bll.GetControlData(_user.BusinessUnit.Trim());

                if (controlDataResult.Message != null)
                    return GetErrorMessageResponse(controlDataResult.Message);
                
                var businessUnitResult = await  _bll.GetBusinessUnit(_user.BusinessUnit);

                if (businessUnitResult.Message != null)
                    return GetErrorMessageResponse(businessUnitResult.Message);

                var rptSourceResult = await _bll.GetReportData(selection.SelectionCriteria);

                if (rptSourceResult.Message != null)
                    return GetErrorMessageResponse(rptSourceResult.Message);

                if (rptSourceResult.Data.Rows.Count > 0)
                {
                    var dtLogoResult = await _bll.GetBusinessUnitLogo(_user.BusinessUnit);

                    if (dtLogoResult.Message != null)
                        return GetErrorMessageResponse(dtLogoResult.Message);

                    var session = _httpContextAccessor.HttpContext?.Session;
                    if (session != null)
                    {
                        session.SetString("Main_BusinessUnitLogo", JsonConvert.SerializeObject(dtLogoResult.Data));
                    }

                    string reportName = string.Format("SOXLR71{0}.xlsx", DateTime.Now.Ticks);

                    if (selection.SelectionCriteria.ReportDetailFlag)
                        excelResult = await _bll.GenerateExcelByDetail(selection.SelectionCriteria, rptSourceResult.Data, dtLogoResult.Data, reportName, controlDataResult.Data, businessUnitResult.Data);
                    else
                        excelResult = await _bll.GenerateExcelBySummary(selection.SelectionCriteria, rptSourceResult.Data, dtLogoResult.Data, reportName, controlDataResult.Data, businessUnitResult.Data);


                    if (excelResult.Message != null)
                        return GetErrorMessageResponse(excelResult.Message);

                    return Ok(new { fileContents = excelResult.Data, ReportName = reportName });
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