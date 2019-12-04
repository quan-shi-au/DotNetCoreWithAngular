import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {HttpErrorResponse} from '@angular/common/http';
import { Partner } from '../models/partner';
import * as glob from '../variables/global.variable';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/from';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class LookupService {
    public rootUrl: string;

    constructor(private http: HttpClient, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as LookupService;
        thisCaller.rootUrl = es.rooturl;
    }

    getProducts() {
        return this.http.get(this.rootUrl + glob.ProductListUrl);
    }

    getLicenceEnvironments() {
        return this.http.get(this.rootUrl + glob.LicenceEnvironmentListUrl);
    }

}
