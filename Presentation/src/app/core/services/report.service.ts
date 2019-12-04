import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {HttpErrorResponse} from '@angular/common/http';
import { Partner } from '../models/partner';
import * as glob from '../variables/global.variable';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import { Enterprise } from '../models/enterprise';
import { Subscription } from '../models/subscription';

import { UtilityService } from './utility.service';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/from';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class ReportService {
    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as ReportService;
        thisCaller.rootUrl = es.rooturl;
    }

    getReport(subscriptionId) {
        return this.http.get(this.rootUrl + glob.ReportGetById + subscriptionId.toString());
    }

    getServerErrorByCode(code: string) {

        return this.utilityService.getErrorMessage(code, ReportServerCodeMappings, 'Error, failed to retrieve report.');
    }


}

const ReportServerCodeMappings: ServerCodeMapping[] = [
    { code: '02', description: "Subscription Doesn't Exist" },
    { code: '03', description: "Report Doesn't Exist", languageToken: "DashboardLicence.ReportNotAvailable" },
    { code: '04', description: "Invalid Date Filter" },
];


