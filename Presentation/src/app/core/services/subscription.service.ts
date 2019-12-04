import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {HttpErrorResponse} from '@angular/common/http';
import { Subscription } from '../models/subscription';
import { ServerCodeMapping } from '../models/serverCodeMapping';
import * as glob from '../variables/global.variable';
import { UtilityService } from './utility.service';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class SubscriptionService {
    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {

        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {

        const thisCaller = caller as SubscriptionService;
        thisCaller.rootUrl = es.rooturl;
    }

    getSubscriptionsWithPage(pageNumber) {
        return this.http.get(this.rootUrl + glob.SubscriptionListUrlWithPage + pageNumber);
    }

    getSubscription(subscriptionId) {
        return this.http.get(this.rootUrl + glob.SubscriptionGetUrl + subscriptionId);
    }

    insertSubscription(subscription: Subscription) {

        return this.http.post(this.rootUrl + glob.SubscriptionInsertUrl, subscription);
    }

    getServerErrorByCode(code: string) {
        return this.utilityService.getErrorMessage(code, SubscriptionServerCodeMappings, '');
    }

    setSeatCount(subscriptionId: number, seatCount: number) {

        var serverModel = {
            sid: subscriptionId,
            scnt: seatCount
        };

        return this.http.post(this.rootUrl + glob.SubscriptionSetSeatCount, serverModel);
    }

    sendInstructions(subscriptionId: number) {

        var serverModel = {
            sid: subscriptionId
        };

        return this.http.post(this.rootUrl + glob.SubscriptionSendInstructionsUrl, serverModel);
    }

    cancel(subscriptionId: number) {

        var serverModel = {
            sid: subscriptionId
        };

        return this.http.post(this.rootUrl + glob.SubscriptionCancelUrl, serverModel);
    }


}

const SubscriptionServerCodeMappings: ServerCodeMapping[] = [
    { code: '02', description: "Subscription doesn't exist" },
    { code: '03', description: "Subscription already exists" },
    { code: '04', description: "Failed to add to database" },
    { code: '05', description: "Failed to cancel subscription" },
    { code: '06', description: "Failed to set seat count for subscription" },
    { code: '07', description: "Subscription licence has already been cancelled" },
    { code: '08', description: "Failed to send instructions. No enterprise admin exists." },
    { code: '09', description: "System failed to create subscription, as licence can not be generated." },
];


