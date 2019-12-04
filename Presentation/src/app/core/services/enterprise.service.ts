import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {HttpErrorResponse} from '@angular/common/http';
import { Enterprise } from '../models/enterprise';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import { UtilityService } from './utility.service';
import * as glob from '../variables/global.variable';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class EnterpriseService {
    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as EnterpriseService;
        thisCaller.rootUrl = es.rooturl;
    }

    getEnterprises() {
        return this.http.get(this.rootUrl + glob.EnterpriseListUrl);
    }

    getEnterprisesWithPage(pageNumber) {
        return this.http.get(this.rootUrl + glob.EnterpriseListUrlWithPage + pageNumber);
    }

    getEnterprisesForPartner(pid: number) {
        return this.http.get(this.rootUrl + glob.EnterpriseForPartnerUrl + pid);
    }

    insertEnterprise(enterprise: any) {

        return this.http.post(this.rootUrl + glob.EnterpriseInsertUrl, enterprise);
    }

    getServerErrorByCode(code: string) {
        return this.utilityService.getErrorMessage(code, EnterpriseServerCodeMappings, '');
    }
}

const EnterpriseServerCodeMappings: ServerCodeMapping[] = [
    { code: '02', description: "Enterprise name already exists" },
    { code: '03', description: "Failed to add to database" },
];

