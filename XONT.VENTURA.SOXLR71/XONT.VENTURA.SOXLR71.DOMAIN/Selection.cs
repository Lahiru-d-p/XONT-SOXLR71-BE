using System;

namespace XONT.VENTURA.SOXLR71
{
    public class Selection
    {
        public string BusinessUnit { get; set; }
        public bool TerritoryFlag { get; set; }
        public bool DistributorFlag { get; set; }
        public string TerritoryCode { get; set; }
        public string TerritoryName { get; set; }
        public string DistributorCode { get; set; }
        public string DistributorName { get; set; }
        public string DistributorVATRegistrationNo { get; set; }
        public string UserName { get; set; }
        public string PowerUser { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool VATFlag { get; set; }
        public bool NonVATFlag { get; set; }
        public bool ReportSummaryFlag { get; set; }
        public bool ReportDetailFlag { get; set; }
        public string ImagePath { get; set; }

    }

    [Serializable]
    public class ControlData
    {
        public ControlData()
        {
            AllowDecimalPointFlag = "";
            DecimalPlaces = 0;
        }
        public string AllowDecimalPointFlag { get; set; }
        public int DecimalPlaces { get; set; }
    }
}
