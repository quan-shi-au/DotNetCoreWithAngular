import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {HttpErrorResponse} from '@angular/common/http';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import * as glob from '../variables/global.variable';
import { UtilityService } from './utility.service';
import { SeatFilter } from '../models/seatFilter';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class LicenceService {
    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {

        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {

        const thisCaller = caller as LicenceService;
        thisCaller.rootUrl = es.rooturl;
    }

    deactivateSeat(subscriptionId,  seatDetail) {
        var serverModel = {
            sid: subscriptionId,
            sk: seatDetail.seatKey
        }
        return this.http.post(this.rootUrl + glob.SeatDeactivationUrl, serverModel);
    }

    getServerErrorByCode(code: string) {
        return this.utilityService.getErrorMessage(code, LicenceServerCodeMappings, '');
    }

    getSeatDetail(subscriptionId, pageNumber, filter: SeatFilter) {
        var serverModel = {
            s: subscriptionId,
            i: pageNumber,
            f: {
                fn: !filter.firstName ? '' : filter.firstName,
                ln: !filter.lastName ? '' : filter.lastName,
                od: !filter.optionalData ? '' : filter.optionalData,
                dn: !filter.deviceName ? '' : filter.deviceName,
            }

        }

        return this.http.post(this.rootUrl + glob.SeatDetailUrl, serverModel);

    }

}

const LicenceServerCodeMappings: ServerCodeMapping[] = [
    { code: '02', description: "Subscription doesn't exist" },
    { code: '03', description: "Seat can't be de-activated" },
];


