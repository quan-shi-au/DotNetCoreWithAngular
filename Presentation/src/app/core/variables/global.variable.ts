
/*
 * token name
 */
export const TokenRoleProperty: string = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
export const TokenNameProperty: string = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";


export const LastUserActiveDate: string = "LastUserActiveDate";

/*
 * url
 */
export const DashboardUrl: string = 'dashboard/get';

export const PartnerInsertUrl: string = 'partners/add';
export const PartnerListUrl: string = 'partners/fetch';
export const PartnerListUrlWithPage: string = 'partners/fetchp/';

export const TokenGetUrl: string = 'token/get';
export const VerifyTokenUrl: string = 'user/verifyetoken';
export const RegenerateTokenUrl: string = 'token/regenerate';

export const EnterpriseListUrl: string = 'ec/fetch';
export const EnterpriseListUrlWithPage: string = 'ec/fetchp/';
export const EnterpriseInsertUrl: string = 'ec/add';
export const EnterpriseForPartnerUrl: string = 'ec/getbypid/';

export const SubscriptionListUrl: string = 'sub/fetch';
export const SubscriptionListUrlWithPage: string = 'sub/fetchp/';
export const SubscriptionInsertUrl: string = 'sub/add';
export const SubscriptionSetSeatCount: string = 'sub/setseatcount';
export const SubscriptionCancelUrl: string = 'sub/cancel';
export const SubscriptionGetUrl: string = 'sub/get/';
export const SubscriptionSendInstructionsUrl: string = 'sub/sendinstructions';

export const SeatDeactivationUrl: string = 'lic/deseat';
export const SeatDetailUrl: string = 'seat/fetchp';

export const UserGetById: string = 'user/get/';
export const UserGetFullName: string = 'user/getfullname';
export const UserListUrl: string = 'user/fetch';
export const UserListUrlWithPage: string = 'user/fetchp';
export const UserInsertUrl: string = 'user/add';
export const UserLockUrl: string = 'user/lock';
export const UserUnlockUrl: string = 'user/unlock';
export const UserDeleteUrl: string = 'user/delete';
export const PasswordResetUrl: string = 'user/sendreset';
export const ChangePasswordWithTokenUrl: string = 'user/confirmreset';
export const sendWelcomeUrl: string = 'user/sendwelcome';

export const ProductListUrl: string = 'lookup/product/fetch';

export const PageGetUrl: string = 'cfg/pagesize';

export const LicenceEnvironmentListUrl: string = 'lookup/le/fetch';

export const ReportGetById: string = 'report/fetch/';

export const AdminReportListUrl: string = 'report/getruns';
export const AdminReportGenerateUrl: string = 'report/start';

export const KeyGetUrl: string = 'ekey/getkey/';
export const TestEncryptUrl: string = 'ekey/testencrypt';
export const TestDecryptUrl: string = 'ekey/testdecrypt';

export const CemsGetEventUrl: string = 'api/getall';

/*
 * error code
 */

export const SuccessCode = '01';

export const MaxNumber = 2147483647;


export const usageId = 'usageId';


export const LanguageIdEnglish = 'en';
export const LanguageIdChineseSimplified = 'zhhans';
export const LanguageIdChineseTraditional = 'zhhant';
export const LanguageIdKorean = 'ko';

export const LicencingEnvironmentDev: number = 1;
export const LicencingEnvironmentprod: number = 2;

export const DeviceProductType = {
    SC_SS_Win: 1,
    SC_SS_MacOs: 2,
    SC_SS_Android: 3,
    SC_SS_iOS: 4
}

export const DeviceEventUrl = {
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
}

export const DeviceEventResult = {
    success: 1,
    apiFailed: -1,
    somethingWrong: -2,
    unhandledException: -3
}