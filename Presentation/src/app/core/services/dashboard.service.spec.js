"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var http_1 = require("@angular/http");
var http_2 = require("@angular/common/http");
var dashboard_service_1 = require("../../core/services/dashboard.service");
var environmentSpecific_service_1 = require("../../core/services/environmentSpecific.service");
var utility_service_1 = require("../../core/services/utility.service");
describe('partner list component', function () {
    var dashboardService;
    var serverResponse;
    beforeEach(function () {
        var httpClientStub = new http_2.HttpClient(null);
        var environmentSpecificServiceStub = new environmentSpecific_service_1.EnvironmentSpecificService(new http_1.Http(null, null));
        var utilityServiceStub = new utility_service_1.UtilityService(null);
        dashboardService = new dashboard_service_1.DashboardService(httpClientStub, utilityServiceStub, environmentSpecificServiceStub);
        serverResponse = {
            "subscriptions": [
                {
                    "id": 1,
                    "name": "ec1",
                    "enterpriseClientId": 45,
                    "product": 1,
                    "productName": "iOS Mobile Security",
                    "licencingEnvironment": 1,
                    "licencingEnvironmentName": "Development",
                    "brandId": "3443",
                    "campaign": "10YearLicence",
                    "seatCount": 5,
                    "coreAuthUsername": "authusername_value",
                    "regAuthUsername": "authusername_value",
                    "status": true,
                    "licenceKey": "",
                    "clientDownloadLocation": "clientdownloadlocation_value",
                    "partner": {
                        "id": 1,
                        "name": "Partner-edited"
                    },
                    "enterprise": {
                        "id": 1,
                        "name": "enterprise1"
                    },
                    "creationTime": "2018-01-18T03:47:02",
                    "cancelationTime": null
                },
                {
                    "id": 2,
                    "name": "subscription_name",
                    "enterpriseClientId": 45,
                    "product": 1,
                    "productName": "iOS Mobile Security",
                    "licencingEnvironment": 1,
                    "licencingEnvironmentName": "Development",
                    "brandId": "3459",
                    "campaign": "iosenterprise",
                    "seatCount": 5,
                    "coreAuthUsername": "authusername_value",
                    "regAuthUsername": "authusername_value",
                    "status": true,
                    "licenceKey": "MPXX-W4XB-29PV-VX4W-JWND",
                    "clientDownloadLocation": "clientdownloadlocation_value",
                    "partner": {
                        "id": 2,
                        "name": "Partner-edited"
                    },
                    "enterprise": {
                        "id": 2,
                        "name": "enterprise1"
                    },
                    "creationTime": "2018-01-18T05:36:52",
                    "cancelationTime": null
                },
                {
                    "id": 3,
                    "name": "subscription_name2",
                    "enterpriseClientId": 45,
                    "product": 1,
                    "productName": "iOS Mobile Security",
                    "licencingEnvironment": 1,
                    "licencingEnvironmentName": "Development",
                    "brandId": "3459",
                    "campaign": "iosenterprise",
                    "seatCount": 26,
                    "coreAuthUsername": "tparker",
                    "regAuthUsername": "authusername_value",
                    "status": true,
                    "licenceKey": "WJUY-VSJT-Q2XY-2DBJ-5GU3",
                    "clientDownloadLocation": "",
                    "partner": {
                        "id": 3,
                        "name": "Partner-edited"
                    },
                    "enterprise": {
                        "id": 3,
                        "name": "enterprise1"
                    },
                    "creationTime": "2018-01-18T08:23:32",
                    "cancelationTime": null
                }
            ],
            "partners": [
                {
                    "name": "Partner-edited",
                    "location": "melbourne",
                    "creationTime": "2018-01-17T02:32:51",
                    "modificationTime": "2018-01-17T02:37:12",
                    "id": 1
                },
                {
                    "name": "Partner2",
                    "location": "Sydney333",
                    "creationTime": "2018-01-19T04:03:39",
                    "modificationTime": null,
                    "id": 2
                },
                {
                    "name": "Partner2333",
                    "location": "Sydney333",
                    "creationTime": "2018-01-19T04:12:07",
                    "modificationTime": null,
                    "id": 3
                }
            ],
            "ec": [
                {
                    "name": "enterprise1",
                    "location": "sydney",
                    "partnerId": 1,
                    "creationTime": "2018-01-17T02:38:14",
                    "modificationTime": null,
                    "id": 1
                },
                {
                    "name": "e h",
                    "location": "e h",
                    "partnerId": 2,
                    "creationTime": "2018-02-08T05:49:22",
                    "modificationTime": null,
                    "id": 2
                },
                {
                    "name": "E B B",
                    "location": "E B B",
                    "partnerId": 3,
                    "creationTime": "2018-02-08T22:14:35",
                    "modificationTime": null,
                    "id": 3
                }
            ],
        };
    });
    it('overview doesn\'t lose informatoin', function () {
        var overview = dashboardService.getOverview(serverResponse);
        expect(overview.dashboardPartners.length).toEqual(3);
        expect(overview.dashboardPartners[0].partnerName).toEqual('Partner-edited');
        expect(overview.dashboardPartners[1].partnerName).toEqual('Partner2');
        expect(overview.dashboardPartners[2].partnerName).toEqual('Partner2333');
        expect(overview.dashboardPartners[0].dashboardEnterprises[0].enterpriseName).toEqual('enterprise1');
        expect(overview.dashboardPartners[1].dashboardEnterprises[0].enterpriseName).toEqual('e h');
        expect(overview.dashboardPartners[2].dashboardEnterprises[0].enterpriseName).toEqual('E B B');
        expect(overview.dashboardPartners[0].dashboardEnterprises[0].subscriptions[0].name).toEqual('ec1');
        expect(overview.dashboardPartners[1].dashboardEnterprises[0].subscriptions[0].name).toEqual('subscription_name');
        expect(overview.dashboardPartners[2].dashboardEnterprises[0].subscriptions[0].name).toEqual('subscription_name2');
    });
});
//# sourceMappingURL=dashboard.service.spec.js.map