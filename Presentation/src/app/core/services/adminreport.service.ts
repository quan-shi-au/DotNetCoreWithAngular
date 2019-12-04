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
export class AdminReportService {
    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as AdminReportService;
        thisCaller.rootUrl = es.rooturl;
    }

    getAdminReport() {
        return this.http.get(this.rootUrl + glob.AdminReportListUrl);
    }

    generateAdminReport() {
        return this.http.get(this.rootUrl + glob.AdminReportGenerateUrl);
    }

    getServerErrorByCode(code: string) {

        return this.utilityService.getErrorMessage(code, AdminReportServerCodeMappings, 'Error, failed to retrieve report.');
    }


}

const AdminReportServerCodeMappings: ServerCodeMapping[] = [
];


