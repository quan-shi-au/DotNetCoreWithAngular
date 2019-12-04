import { Injectable } from '@angular/core';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import { TranslateService } from '@ngx-translate/core';

@Injectable()
export class UtilityService {

    constructor(private translateService: TranslateService,
    ) {

    }

    public static trim(stringValue) : string {
        if (!stringValue)
            return '';

        return stringValue.replace(/\s+$/, "").replace(/^\s+/, "")
        
    }

    getTranslatedMessage(code: string, subscriptionServerCodeMappings: ServerCodeMapping[], defaultLanguageToken: string) {

        var mapping = subscriptionServerCodeMappings.filter(x => x.code == code);

        if (mapping && mapping[0]) {
            return this.translateService.instant(mapping[0].languageToken);
        } else {
            if (defaultLanguageToken)
                return this.translateService.instant(defaultLanguageToken);
            else
                return this.translateService.instant("Common.GeneralServerError");
        }

    }

    getErrorMessage(code: string, subscriptionServerCodeMappings: ServerCodeMapping[], defaultMessage: string) {
        var mapping = subscriptionServerCodeMappings.filter(x => x.code == code);

        if (mapping && mapping[0] && mapping[0].languageToken) {
            return this.getTranslatedMessage(code, subscriptionServerCodeMappings, '');
        }
        else if (mapping && mapping[0]) {
            return mapping[0].description;
        } else {
            if (defaultMessage)
                return defaultMessage;
            else
                return 'Server error';
        }
    }


    // 14/11/2018 02:53:26 dd/MM/yyyy HH:mm:ss
    convertToIsoDateString(dateString) {

        var dateRegular = /(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})/;
        var dateArray = dateRegular.exec(dateString);

        if (dateArray && dateArray.length && dateArray.length == 7)
            return dateArray[3] + '-' + dateArray[2] + '-' + dateArray[1] + 'T' + dateArray[4] + ':' + dateArray[5] + ':' + dateArray[6];
        else
            return '';

    }

    // 2018-09-12T17:56:45Z
    convertToDate(dateString) {
        dateString = dateString.replace(' ', 'T');

        var dateRegular = /(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/;
        var dateArray = dateRegular.exec(dateString);

        if (dateArray && dateArray.length && dateArray.length == 7)
            return new Date(+dateArray[1], +dateArray[2] - 1, +dateArray[3], +dateArray[4], +dateArray[5], +dateArray[6]);
        else
            return null;

    }

    dayDifference(beginDate, endDate) {

        var beginDateTime = beginDate.getTime();
        var endDateTime = endDate.getTime();

        return Math.round((endDateTime - beginDateTime) / (1000 * 60 * 60 * 24));

    }

    isValidPart(lexicographical, x) {
        return (lexicographical ? /^\d+[A-Za-z]*$/ : /^\d+$/).test(x);
    }

    /*
     *  return 0 if v1 == v2
     *      1 if v1 > v2
     *      -1 if v1 < v2
     *      NaN if not valid
     */
    versionCompare(v1, v2, options) {

        var lexicographical = options && options.lexicographical,
            zeroExtend = options && options.zeroExtend,
            v1parts = v1.split('.'),
            v2parts = v2.split('.'),
            isV1Valid = true,
            isV2Valid = true
            ;

        if (!lexicographical)
            lexicographical = false;

        if (!zeroExtend)
            zeroExtend = false;

        if (!v1)
            isV1Valid = false;

        if (!v2)
            isV2Valid = false;

        for (var x in v1parts) {
            if (!this.isValidPart(lexicographical, x)) {
                isV1Valid = false;
                break;
            }
        }

        for (var y in v2parts) {
            if (!this.isValidPart(lexicographical, y)) {
                isV2Valid = false;
                break;
            }
        }

        if (!isV1Valid && !isV2Valid)
            return NaN;
        else if (!isV1Valid && isV2Valid)
            return -1;
        else if (isV1Valid && !isV2Valid)
            return 1;

        if (zeroExtend) {
            while (v1parts.length < v2parts.length) v1parts.push ("0");
            while (v2parts.length < v1parts.length) v2parts.push("0");
        }

        if (!lexicographical) {
            v1parts = v1parts.map(Number);
            v2parts = v2parts.map(Number);
        }

        for (var i = 0; i < v1parts.length; ++i) {
            if (v2parts.length == i) {
                return 1;
            }

            if (v1parts[i] == v2parts[i])
                continue;
            else if (v1parts[i] > v2parts[i])
                return 1;
            else
                return -1;
        }

        if (v1parts.length != v2parts.length)
            return -1;

        return 0;
    }

    roundNumber(num, scale) {

        if (("" + num).indexOf("e") < 0) {
            return +(Math.round(+(num + "e+" + scale)) + "e-" + scale);
        } else {
            var arr = ("" + num).split("e");
            var sig = "";
            if (+arr[1] + scale > 0) {
                sig = "+";
            }
            return +(Math.round(+(+arr[0] + "e" + sig + (+arr[1] + scale))) + "e-" + scale);
        }

    }

    convertUtcToLocalDate(utcDateString) {
        if (!utcDateString)
            return null;

        var utcDate = this.convertToDate(utcDateString);
        var offsetInTicks = (new Date().getTimezoneOffset()) * (-1) * 60 * 1000;
        var localDate = new Date(utcDate.getTime() + offsetInTicks);

        return localDate;
    }

    convertUtcToLocalDateWithAuFormat(utcDateStringAu) {
        if (!utcDateStringAu)
            return null;

        var utcDateString = this.convertToIsoDateString(utcDateStringAu);
        return this.convertUtcToLocalDate(utcDateString);

    }

}