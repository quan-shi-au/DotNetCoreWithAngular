namespace ent.manager.WebApi.Results
{
    public class ResultCode
    {

        public static string GenericException = "00";
        public static string Success = "01";

        public class TokenResultCodes
        {
            public static string UserLockedOut = "02";
            public static string UserUnAuthenticated = "03";

        }

        public class PartnerResultCodes
        {
            public static string PartnerNameAlreadyExists = "02";
            public static string PartnerFailedToAdd = "03";
        }

        public class EnterpriseResultCodes
        {
            public static string EnterpriseNameAlreadyExists = "02";
            public static string EnterpriseFailedToAdd = "03";
            public static string EkeyFailedToAdd = "04";
        }

        public class SubscriptionResultCodes
        {
            public static string SubscriptionDoesntExist = "02";
            public static string SubscriptionAlreadyExists = "03";
            public static string SubscriptionFailedToAdd = "04";
            public static string SubscriptionFailedToCancel = "05";
            public static string SubscriptionFailedToSetSeatCount = "06";
            public static string SubscriptionLicenceAlreadyCancel = "07";
            public static string SubsscriptionSendInstructionsNoReceiver = "08";
            public static string SubsscriptionLicenceGenerationFailedr = "09";
            public static string InvalidSubscriptionAuthentication = "10";
            public static string AddingSubscriptionAuthenticationDetailsFailed = "11";
        }

        public class UserResultCodes
        {

            public static string UsernameAlreadyUsed = "02";
            public static string UserFailedToAdd = "03";
            public static string UserDoesntExistForDelete = "04";
            public static string UserFailedToDelete = "05";
            public static string UserFailedToDisable = "06";
            public static string UserFailedToEnable = "07";
            public static string UserVerifyECTUserDoesntExist = "08";
            public static string UserVerifyECTUserIsLocked = "09";
            public static string UserVerifyECTLinkOpened = "10";
            public static string UserSendResetUserDoesntExist = "11";
            public static string UserSetPassMissmatch = "12";
            public static string UserSetPassSetFailed = "13";
            public static string UserDoesntExistGet = "14";
            public static string UserAddInvalidRole = "15";
            public static string UserSendWelcomeUserDoesntExist = "16";

        }

        public class ReportResultCodes
        {

            public static string SubscriptionDoesntExist = "02";
            public static string ReportDoesntExist = "03";
            public static string InvalidDateFilter = "04";
        }

        public class LicenceResultCodes
        {

            public static string SubscriptionDoesntExist = "02";
            public static string FailedToDeactivateSeat = "03";


        }

        public class SeatResultCodes
        {

            public static string SubscriptionDoesntExist = "02";
            public static string FailedToDeactivateSeat = "03";


        }

        public class UserDataResultCodes
        {

            public static string EkeyDoesntExist = "02";
            public static string SubscriptionDoesntExist = "03";


        }


    }


}
