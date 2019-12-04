import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {HttpErrorResponse} from '@angular/common/http';
import { Partner } from '../models/partner';
import * as glob from '../variables/global.variable';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import { DashboardOverview } from '../models/dashboardOverview';
import { DashboardEnterprise } from '../models/dashboardEnterprise';
import { DashboardPartner } from '../models/dashboardPartner';
import { Enterprise } from '../models/enterprise';
import { Subscription } from '../models/subscription';

import { UtilityService } from './utility.service';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/from';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class DashboardService {

    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as DashboardService;
        thisCaller.rootUrl = es.rooturl;
    }

    getDashboard() {
        return this.http.get(this.rootUrl + glob.DashboardUrl);
    }

    stringSort(s1, s2) {
        if (s1.name > s2.name)
            return 1;
        else if (s1.name < s2.name)
            return -1;
        else
            return 0;

    }

    sortServerResponse(serverResponse) {

        if (serverResponse.partners && serverResponse.partners instanceof Array)
            serverResponse.partners.sort(this.stringSort);

        if (serverResponse.ec && serverResponse.ec instanceof Array)
            serverResponse.ec.sort(this.stringSort);

        if (serverResponse.subscriptions && serverResponse.subscriptions instanceof Array)
            serverResponse.subscriptions.sort(this.stringSort);

        return serverResponse;

    }

    overwriteSubscription(serverResponse) {
        serverResponse.subscriptions = serverResponse.subscriptions.map(x => {
            x.status = x.status ? 'Active' : 'Cancelled';
            return x;
        });
    }

    getOverview(serverResponse): DashboardOverview {

        let overview: DashboardOverview = {};

        serverResponse = this.sortServerResponse(serverResponse);

        this.overwriteSubscription(serverResponse);

        if (!overview.dashboardPartners)
            overview.dashboardPartners = [];

        if (serverResponse.partners && serverResponse.partners instanceof Array) {
            for (let p of serverResponse.partners) {
                var partner: Partner = <Partner>p;

                overview.dashboardPartners.push({ partnerName: partner.name, partnerId: partner.id, dashboardEnterprises: [] });
            }
        } else {
            overview.dashboardPartners.push({ partnerName: serverResponse.partners.name, partnerId: serverResponse.partners.id, dashboardEnterprises: [] });
        }

        if (serverResponse.ec && serverResponse.ec instanceof Array) {

            for (let e of serverResponse.ec) {
                var enterprise: Enterprise = <Enterprise>e;
                var mappedPartner: DashboardPartner = overview.dashboardPartners.filter(x => x.partnerId == enterprise.partnerId)[0];

                mappedPartner.dashboardEnterprises.push({ enterpriseName: enterprise.name, enterpriseId: enterprise.id, subscriptions: [] });
            }
        } else {
            var mappedPartner: DashboardPartner = overview.dashboardPartners.filter(x => x.partnerId == serverResponse.ec.partnerId)[0];
            mappedPartner.dashboardEnterprises.push({ enterpriseName: serverResponse.ec.name, enterpriseId: serverResponse.ec.id, subscriptions: [] });
        }

        for (let p of serverResponse.subscriptions) {
            var subscription: Subscription = <Subscription>p;

            var mappedDashboardPartner: DashboardPartner = overview.dashboardPartners.filter(x => x.partnerId == subscription.partner.id)[0];
            var mappedDashboardEnterprise: DashboardEnterprise = mappedDashboardPartner.dashboardEnterprises.filter(x => x.enterpriseId == subscription.enterprise.id)[0];
            subscription.creationTime = this.utilityService.convertUtcToLocalDate(subscription.creationTime);

            mappedDashboardEnterprise.subscriptions.push(subscription);
        }

        return overview;
    }

}

