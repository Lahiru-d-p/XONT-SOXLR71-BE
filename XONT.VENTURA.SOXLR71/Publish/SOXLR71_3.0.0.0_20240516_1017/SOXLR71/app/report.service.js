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
var http_1 = require("@angular/http");
require("rxjs/add/operator/map");
require("rxjs/add/operator/catch");
var Rx_1 = require("rxjs/Rx");
var Subject_1 = require("rxjs/Subject");
var xont_ventura_services_1 = require("xont-ventura-services");
var ReportService = /** @class */ (function () {
    function ReportService(http, commonService, datetimeService) {
        this.http = http;
        this.commonService = commonService;
        this.datetimeService = datetimeService;
        this.componentMethodCallSource = new Subject_1.Subject();
        this.componentMethodCalled$ = this.componentMethodCallSource.asObservable();
    }
    ReportService.prototype.getTerritoryPrompt = function () {
        var _this = this;
        return this.http.get(this.getSiteName() + '/api/SOXLR71/GetTerritoryPrompt')
            .map(function (response) { return response.json(); })
            .catch(function (error) { return _this.handleError(error.json()); });
    };
    ReportService.prototype.getDistributorPrompt = function () {
        var _this = this;
        return this.http.get(this.getSiteName() + '/api/SOXLR71/GetDistributorPrompt')
            .map(function (response) { return response.json(); })
            .catch(function (error) { return _this.handleError(error.json()); });
    };
    ReportService.prototype.generateExcel = function (data) {
        var _this = this;
        var headers = new http_1.Headers();
        headers.append('Content-Type', 'application/json');
        return this.http.post(this.getSiteName() + '/api/SOXLR71/GenerateExcel', data, { headers: headers })
            .map(function (response) { return response.json(); })
            .catch(function (error) { return _this.handleError(error.json()); });
    };
    ReportService.prototype.handleError = function (error) {
        this.componentMethodCallSource.next(error);
        return Rx_1.Observable.throw(error);
    };
    ReportService.prototype.getSiteName = function () {
        return this.commonService.getAPIPrefix('SOXLR71');
    };
    ReportService = __decorate([
        core_1.Injectable(),
        __metadata("design:paramtypes", [http_1.Http, xont_ventura_services_1.CommonService, xont_ventura_services_1.DatetimeService])
    ], ReportService);
    return ReportService;
}());
exports.ReportService = ReportService;
//# sourceMappingURL=report.service.js.map