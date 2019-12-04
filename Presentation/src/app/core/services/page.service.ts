import { Injectable, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, Subscription, BehaviorSubject } from 'rxjs/Rx';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/toPromise';
import { ApiResult } from '../models/apiResult';
import * as glob from '../variables/global.variable';
import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';


@Injectable()
export class PageService {
    private rootUrl: string;
    public pageSizeSubject = new BehaviorSubject<number>(0);
    private pageSize: number = 0;

    setPageSize(size) {
        this.pageSizeSubject.next(size);
    }

    constructor(private http: HttpClient, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {

        const thisCaller = caller as PageService;
        thisCaller.rootUrl = es.rooturl;
    }

    public loadPageSize() {

        return this.http.get(this.rootUrl + glob.PageGetUrl)
            .subscribe(
            (data: ApiResult) => {
                if (data.c === glob.SuccessCode) {
                    this.pageSize = +data.d;
                    this.pageSizeSubject.next(this.pageSize);
                }
            },
            (err) => {
            }
        );
    }

    public getPageSize() {
        if (!this.pageSize)
            this.loadPageSize();

        return this.pageSize;
    }

}
