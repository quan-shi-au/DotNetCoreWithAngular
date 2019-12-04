"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/*
 * token name
 */
exports.TokenRoleProperty = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
exports.TokenNameProperty = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
exports.LastUserActiveDate = "LastUserActiveDate";
/*
 * url
 */
exports.DashboardUrl = 'dashboard/get';
exports.PartnerInsertUrl = 'partners/add';
exports.PartnerListUrl = 'partners/fetch';
exports.PartnerListUrlWithPage = 'partners/fetchp/';
exports.TokenGetUrl = 'token/get';
exports.VerifyTokenUrl = 'user/verifyetoken';
exports.RegenerateTokenUrl = 'token/regenerate';
exports.EnterpriseListUrl = 'ec/fetch';
exports.EnterpriseListUrlWithPage = 'ec/fetchp/';
exports.EnterpriseInsertUrl = 'ec/add';
exports.EnterpriseForPartnerUrl = 'ec/getbypid/';
exports.SubscriptionListUrl = 'sub/fetch';
exports.SubscriptionListUrlWithPage = 'sub/fetchp/';
exports.SubscriptionInsertUrl = 'sub/add';
exports.SubscriptionSetSeatCount = 'sub/setseatcount';
exports.SubscriptionCancelUrl = 'sub/cancel';
exports.SubscriptionGetUrl = 'sub/get/';
exports.SubscriptionSendInstructionsUrl = 'sub/sendinstructions';
exports.SeatDeactivationUrl = 'lic/deseat';
exports.SeatDetailUrl = 'seat/fetchp';
exports.UserGetById = 'user/get/';
exports.UserGetFullName = 'user/getfullname';
exports.UserListUrl = 'user/fetch';
exports.UserListUrlWithPage = 'user/fetchp';
exports.UserInsertUrl = 'user/add';
exports.UserLockUrl = 'user/lock';
exports.UserUnlockUrl = 'user/unlock';
exports.UserDeleteUrl = 'user/delete';
exports.PasswordResetUrl = 'user/sendreset';
exports.ChangePasswordWithTokenUrl = 'user/confirmreset';
exports.sendWelcomeUrl = 'user/sendwelcome';
exports.ProductListUrl = 'lookup/product/fetch';
exports.PageGetUrl = 'cfg/pagesize';
exports.LicenceEnvironmentListUrl = 'lookup/le/fetch';
exports.ReportGetById = 'report/fetch/';
exports.AdminReportListUrl = 'report/getruns';
exports.AdminReportGenerateUrl = 'report/start';
exports.KeyGetUrl = 'ekey/getkey/';
exports.TestEncryptUrl = 'ekey/testencrypt';
exports.TestDecryptUrl = 'ekey/testdecrypt';
exports.CemsGetEventUrl = 'api/getall';
/*
 * error code
 */
exports.SuccessCode = '01';
exports.MaxNumber = 2147483647;
exports.usageId = 'usageId';
exports.LanguageIdEnglish = 'en';
exports.LanguageIdChineseSimplified = 'zhhans';
exports.LanguageIdChineseTraditional = 'zhhant';
exports.LanguageIdKorean = 'ko';
exports.LicencingEnvironmentDev = 1;
exports.LicencingEnvironmentprod = 2;
exports.DeviceProductType = {
    SC_SS_Win: 1,
    SC_SS_MacOs: 2,
    SC_SS_Android: 3,
    SC_SS_iOS: 4
};
exports.DeviceEventUrl = {
    info: 'device/info',
    webprotect: 'device/webprotect',
    health: 'device/health',
    scansummary: 'device/scansummary',
    malwaredetect: 'device/malwaredetect',
    secureapps: 'device/secureapps',
    malwareremediate: 'device/malwareremediate',
    realtimeprotect: 'device/realtimeprotect',
    firewallevent: 'device/firewallevent',
    firewallpolicy: 'device/firewallpolicy'
};
exports.DeviceEventResult = {
    success: 1,
    apiFailed: -1,
    somethingWrong: -2,
    unhandledException: -3
};
//# sourceMappingURL=global.variable.js.map