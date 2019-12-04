import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import { HttpErrorResponse } from '@angular/common/http';
import * as glob from '../variables/global.variable';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import { UtilityService } from './utility.service';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/from';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class DeviceeventsService {
    public rootUrl: string; 
    public cemsRootUrlProd: string;
    public cemsApiKeyProd: string;
    public cemsRootUrlDev: string;
    public cemsApiKeyDev: string;

    /*
     * iOS:
     *  Device Health Event:    05  01
     *  Device Information Event: 01    01
     *  feature status: Web Protect Event:   03  03
     *  Scan Summary Event:     02  01 
     *  
     *  Android:
     *  Device Health Event: 05 01
     *  Device Informatoin Event:   01  01
     *  feature status: Web Protect Event:  03 03
     *  Malware Detection Event: 02 02
     *  Secure Apps Event:  06  01
     *  Scan Summary Event: 02  01
     */
    private eventDefinitions: any[] = [
        { name: 'DeviceHealth', event: '05', subevent: '01', isIos: true, isAndroid: true, recordNumber: 1 },
        { name: 'DeviceInformation', event: '01', subevent: '01', isIos: true, isAndroid: true, recordNumber: 1 },
        { name: 'WebProtect', event: '03', subevent: '03', isIos: true, isAndroid: true, recordNumber: 1 },
        { name: 'ScanSummary', event: '02', subevent: '01', isIos: true, isAndroid: true, recordNumber: 10 },
        { name: 'MalwareDetection', event: '02', subevent: '02', isIos: false, isAndroid: true, recordNumber: 10 },
        { name: 'SecureApps', event: '06', subevent: '01', isIos: false, isAndroid: true, recordNumber: 10 },
    ];



    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as DeviceeventsService;

        thisCaller.rootUrl = es.rooturl;
        thisCaller.cemsRootUrlProd = es.cemsRootUrlProd;
        thisCaller.cemsApiKeyProd = es.cemsApiKeyProd;
        thisCaller.cemsRootUrlDev = es.cemsRootUrlDev;
        thisCaller.cemsApiKeyDev = es.cemsApiKeyDev;
    }

    getEventDefinition(eventName) {
        for (var i = 0; i < this.eventDefinitions.length; i++) {
            if (this.eventDefinitions[i].name == eventName)
                return this.eventDefinitions[i];
        }

        return null;
    }

    getDeviceEvents(deviceEventQuery, url) {

        return this.http.post(this.rootUrl + url, deviceEventQuery);
    }

    getServerErrorByCode(code: string) {
        return this.utilityService.getErrorMessage(code, DeviceEventsServerCodeMappings, '');
    }


    getDeviceMemory(memoryInMb) {

        if (+memoryInMb <= 0 || isNaN(+memoryInMb))
            return "";
        else if (+memoryInMb < 1000)
            return +memoryInMb + "MB";
        else {
            return this.utilityService.roundNumber(memoryInMb / 1000, 1) + "GB";
        }
    }


}

const DeviceEventsServerCodeMappings: ServerCodeMapping[] = [
];

