import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {HttpErrorResponse} from '@angular/common/http';
import { Partner } from '../models/partner';
import * as glob from '../variables/global.variable';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import { UtilityService } from './utility.service';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/from';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class PartnerService {
    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as PartnerService;
        thisCaller.rootUrl = es.rooturl;
    }

    getPartners() {
        return this.http.get(this.rootUrl + glob.PartnerListUrl);
    }

    getPartnersWithPage(pageNumber) {
        return this.http.get(this.rootUrl + glob.PartnerListUrlWithPage + pageNumber);
    }

    insertPartner(partner: Partner) {
        return this.http.post(this.rootUrl + glob.PartnerInsertUrl, partner);
    }

    getServerErrorByCode(code: string) {
        return this.utilityService.getErrorMessage(code, PartnerServerCodeMappings, '');
    }


}

const PartnerServerCodeMappings: ServerCodeMapping[] = [
    { code: '02', description: "Partner name already exists" },
    { code: '03', description: "Failed to add to database" },
];

