"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var router_1 = require("@angular/router");
var xont_ventura_services_1 = require("xont-ventura-services");
var report_service_1 = require("../report.service");
var ListComponent = /** @class */ (function () {
    function ListComponent(router, datetimeService, commonService, reportService) {
        var _this = this;
        this.router = router;
        this.datetimeService = datetimeService;
        this.commonService = commonService;
        this.reportService = reportService;
        this.selection = {
            FromDate: null,
            FromDateShow: '',
            RootURL: '',
            ToDate: null,
            ToDateShow: '',
            TerritoryCode: '',
            TerritoryName: '',
            DistributorCode: '',
            DistributorName: '',
            VATFlag: true,
            NonVATFlag: true,
            TerritoryFlag: false,
            DistributorFlag: true,
            ReportSummaryFlag: true,
            ReportDetailFlag: false
        };
        this.returnRequests = [];
        this.reportService.componentMethodCalled$
            .subscribe(function (error) {
            _this.msgPrompt.show(error, 'SOXLR71');
        });
    }
    ListComponent.prototype.ngOnInit = function () {
        this.selection.FromDate = new Date();
        this.selection.FromDateShow = this.datetimeService.getDisplayDate(this.selection.FromDate);
        this.selection.ToDate = new Date();
        this.selection.ToDateShow = this.datetimeService.getDisplayDate(this.selection.ToDate);
    };
    ListComponent.prototype.lpmtDistributor_DataBind = function () {
        this.lpmtDistributor.dataSourceObservable = this.reportService.getDistributorPrompt();
        this.selection.TerritoryCode = '';
        this.selection.TerritoryName = '';
    };
    ListComponent.prototype.lpmtTerritory_DataBind = function () {
        this.lpmtTerritory.dataSourceObservable = this.reportService.getTerritoryPrompt();
        this.selection.DistributorCode = '';
        this.selection.DistributorName = '';
    };
    ListComponent.prototype.btnOK_click_verifyBeforeSubmit = function (event) {
        if (!this.selection.VATFlag && !this.selection.NonVATFlag) {
            this.VATStatusAlert.showAlert("Please select at least one option from \"VAT Status\" checkboxes.", "OK");
            event.preventDefault();
        }
        else {
            this.btnOK_click();
        }
    };
    ListComponent.prototype.btnOK_click = function () {
        var _this = this;
        if (this.selection.FromDateShow != "")
            this.selection.FromDate = this.datetimeService.getDateTimeForString(this.selection.FromDateShow);
        else
            this.selection.FromDate = null;
        if (this.selection.ToDateShow != "")
            this.selection.ToDate = this.datetimeService.getDateTimeForString(this.selection.ToDateShow);
        else
            this.selection.ToDate = null;
        this.selection.RootURL = this.commonService.getRootURL();
        this.busy = this.reportService.generateExcel({ SelectionCriteria: this.selection })
            .subscribe(function (jsonData) {
            var sampleArr = _this.base64ToArrayBuffer(jsonData.fileContents);
            _this.generateExcelFile(sampleArr, jsonData.ReportName);
        });
    };
    ListComponent.prototype.base64ToArrayBuffer = function (base64) {
        var binaryString = window.atob(base64);
        var bytes = new Uint8Array(binaryString.length);
        return bytes.map(function (byte, i) { return binaryString.charCodeAt(i); });
    };
    ListComponent.prototype.generateExcelFile = function (byte, reportName) {
        var blob = new Blob([byte], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
        if (navigator.msSaveBlob) {
            navigator.msSaveBlob(blob, reportName);
        }
        else {
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = reportName;
            link.click();
        }
    };
    __decorate([
        core_1.ViewChild('msgPrompt'),
        __metadata("design:type", Object)
    ], ListComponent.prototype, "msgPrompt", void 0);
    __decorate([
        core_1.ViewChild('VATStatusAlert'),
        __metadata("design:type", Object)
    ], ListComponent.prototype, "VATStatusAlert", void 0);
    __decorate([
        core_1.ViewChild('lpmtDistributor'),
        __metadata("design:type", Object)
    ], ListComponent.prototype, "lpmtDistributor", void 0);
    __decorate([
        core_1.ViewChild('lpmtTerritory'),
        __metadata("design:type", Object)
    ], ListComponent.prototype, "lpmtTerritory", void 0);
    ListComponent = __decorate([
        core_1.Component({
            selector: 'list',
            templateUrl: './app/list.component/list.component.html?v=' + String(localStorage.getItem("SOXLR71_ComponentVersion"))
        }),
        __metadata("design:paramtypes", [router_1.Router, xont_ventura_services_1.DatetimeService, xont_ventura_services_1.CommonService, report_service_1.ReportService])
    ], ListComponent);
    return ListComponent;
}());
exports.ListComponent = ListComponent;
//# sourceMappingURL=list.component.js.map