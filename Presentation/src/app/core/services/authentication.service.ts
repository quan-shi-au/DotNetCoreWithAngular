import { Injectable, OnInit, OnDestroy } from '@angular/core'
import { User } from '../models/user';
import { UserRole } from "../models/userRole";
import { ApiResult } from "../models/apiResult";
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import 'rxjs/add/operator/do';
import * as glob from '../variables/global.variable';
import { ServerCodeMapping } from '../models/serverCodeMapping';

import { JwtHelper  } from 'angular2-jwt';
import { UtilityService } from './utility.service';
import { Observable, Subscription } from 'rxjs/Rx';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class AuthenticationService implements OnDestroy {

    private jwtHelper: JwtHelper = new JwtHelper();
    private timer;
    private sub: Subscription;
    private timerStartedTime: number;
    public rootUrl: string;

    constructor(
        private http: HttpClient,
        private utilityService: UtilityService,
        private envSpecificSvc: EnvironmentSpecificService,

    ) {

        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as AuthenticationService;
        thisCaller.rootUrl = es.rooturl;
    }


    public login(userId: string, password: string) {

        return this.http.post(
            this.rootUrl + glob.TokenGetUrl, {
            un: userId,
            pw: password
            }
        ).do((data: ApiResult) => {
            try {
                if (data.c == glob.SuccessCode)
                    this.postLoginProcess(data.d);
            } catch(err) {
                // there will be an exception to decode the server response if it's not a JWT.
            }
        }, (err: any) => {
            });
    }

    postLoginProcess(tokenData) {

        var expirationDate = this.jwtHelper.getTokenExpirationDate(tokenData);

        // 10 minutes before expiry
        this.timerStartedTime = (new Date()).valueOf();
        var timerPeriod = expirationDate.valueOf() - this.timerStartedTime - 10 * 60 * 1000;

        this.timer = Observable.timer(timerPeriod);
        this.sub = this.timer.subscribe(t => this.isUserActive(t));

        localStorage.setItem('managerToken', tokenData);

    }

    isUserActive(t) {

        var lastActiveTime = +localStorage.getItem(glob.LastUserActiveDate);
        if (lastActiveTime > this.timerStartedTime) {

            return this.http.post(
                this.rootUrl + glob.RegenerateTokenUrl, {
                    un: " ",
                    pw: " "
                }
            ).subscribe(
                (data: ApiResult) => {
                    try {
                        if (data.c == glob.SuccessCode)
                            this.postLoginProcess(data.d);
                    } catch (err) {
                        // there will be an exception to decode the server response if it's not a JWT.
                    }
                },
                (err) => {

                }
            );
        }
    }

    verifyToken(id, token) {

        var encodedToken = encodeURIComponent(token);

        return this.http.get(this.rootUrl + glob.VerifyTokenUrl + '?id=' + id + '&token=' + encodedToken);
    }

    logOut() {

        localStorage.removeItem('managerToken');
    }

    public IsLoggedIn(): boolean {

        var tokenValue = this.getTokenValue();

        return tokenValue && !this.jwtHelper.isTokenExpired(tokenValue); 
    }

    public GetCurrentUserId(): string {
        var tokenValue = this.getTokenValue();

        var token = this.jwtHelper.decodeToken(tokenValue);

        if (token)
            return <string>token[glob.TokenNameProperty];
        else
            return null;
    }

    public getTokenValue(): string {
        return localStorage.getItem('managerToken');
    }

    public GetCurrentUserIdAndRole(): User {
        var tokenValue = this.getTokenValue();

        var token = this.jwtHelper.decodeToken(tokenValue);

        if (token) {
            var tokenRole = <string>token[glob.TokenRoleProperty];
            var userRole: UserRole;
            switch (tokenRole) {
                case 'admin':
                    userRole = UserRole.entAdmin;
                    break;
                case 'partner':
                    userRole = UserRole.PartnerAdmin;
                    break;
                case 'ec':
                    userRole = UserRole.EnterpriseAdmin;
                    break;
            }

            return <User>{
                userId: <string>token[glob.TokenNameProperty],
                role: userRole
            }
        } else
            return null;
    }

    getServerErrorByCode(code: string) {
        return this.utilityService.getTranslatedMessage(code, UserServerCodeMappings, 'Common.GeneralServerError');
    }

    isLinkPreviouslyOpened(code: string) {
        return code === '10';
    }

    ngOnDestroy(): void {
        this.sub.unsubscribe();
    }

}

const UserServerCodeMappings: ServerCodeMapping[] = [
    { code: '02', description: "username already used, use different email", languageToken: "Message.UserNameAlreadyExist" },
    { code: '03', description: "Failed to add to database", languageToken: "Message.GeneralDbError" },
    { code: '04', description: "user doesn't exist", languageToken: "Message.UserDoesNotExist" },
    { code: '05', description: "deletion failed", languageToken: "Message.DeletionFailed" },
    { code: '06', description: "user disabling failed", languageToken: "Message.UserDisablingFailed" },
    { code: '07', description: "user enabling failed", languageToken: "Message.UserEnablingFailed" },
    { code: '08', description: "user doesn't exist", languageToken: "Message.UserDoesNotExist" },
    { code: '09', description: "user is locked(disabled)", languageToken: "Message.UserDisabled" },
    { code: '10', description: "link previously opened", languageToken: "Message.LinkPreviouslyOpened" },
    { code: '11', description: "user doesn't exist", languageToken: "Message.UserDoesNotExist" },
    { code: '12', description: "provided passwords mismatch", languageToken: "Message.PasswordsNotMatch" },
    { code: '13', description: "password set failed", languageToken: "Message.PasswordSetFailed" },
    { code: '14', description: "user doesn't exist", languageToken: "Message.UserDoesNotExist" },
    { code: '15', description: "invalid role", languageToken: "Message.InvalidRole" },
];
