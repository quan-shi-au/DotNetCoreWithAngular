import { Injectable } from '@angular/core'
import { HttpClient } from '@angular/common/http';
import * as glob from '../variables/global.variable';

import { User } from '../models/user';
import { UserRole } from '../models/userRole';
import { UserFilter } from '../models/userFilter';
import { EnumMapping } from '../../core/models/enumMapping';
import { AuthenticationService } from './authentication.service';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

@Injectable()
export class UserService {
    public rootUrl: string;

    constructor(private authenticationService: AuthenticationService, private http: HttpClient, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as UserService;
        thisCaller.rootUrl = es.rooturl;
    }

    public userHasScopes(role: UserRole, scopes: string): boolean {
        switch (role) {
            case UserRole.entAdmin:
                return true;
            case UserRole.PartnerAdmin:
                if (scopes == this.getUserRoleDescription(UserRole.PartnerAdmin) ||
                    scopes == this.getUserRoleDescription(UserRole.EnterpriseAdmin))
                    return true;
                else
                    return false;
            case UserRole.EnterpriseAdmin:
                if (scopes == this.getUserRoleDescription(UserRole.EnterpriseAdmin))
                    return true;
                else
                    return false;
            default:
                return false;
        }
    }

    getUsers() {
        return this.http.get<User[]>(this.rootUrl + glob.UserListUrl);
    }


    getUsersWithPage(pageNumber: number, userFilter: UserFilter) {

        var serverRole = '';
        var serverRoleMapping = ServerRoleMapping.filter(x => x.clientRole == userFilter.role);
        if (serverRoleMapping && serverRoleMapping[0])
            serverRole = serverRoleMapping[0].serverRole;

        var serverModel = {
            i: pageNumber,
            f: {
                fn: !userFilter.firstName ? '' : userFilter.firstName,
                ln: !userFilter.lastName ? '' : userFilter.lastName,
                un: !userFilter.userName ? '' : userFilter.userName,
                eid: !userFilter.enterpriseId ? '' : userFilter.enterpriseId,
                pid: !userFilter.partnerId ? '' : userFilter.partnerId,
                role: serverRole
            }
        };

        return this.http.post(this.rootUrl + glob.UserListUrlWithPage, serverModel);
    }

    resetPassword(userName) {

        var serverModel = {
            username: userName
        };

        return this.http.post(this.rootUrl + glob.PasswordResetUrl, serverModel);
    }

    sendWelcome(userName) {

        var serverModel = {
            username: userName
        };

        return this.http.post(this.rootUrl + glob.sendWelcomeUrl, serverModel);
    }

    changePasswordWithToken(id, token, password) {

        var serverModel = {
            id: id,
            token: token,
            password: password,
            repassword: password
        };

        return this.http.post(this.rootUrl + glob.ChangePasswordWithTokenUrl, serverModel);
    }

    getCurrentUser(): User {

        return this.authenticationService.GetCurrentUserIdAndRole();

    }

    getUserRoleDescription(userRole: UserRole): string {

        var mappings = UserRoleMappings.filter(x => x.id == userRole);

        if (mappings && mappings[0])
            return mappings[0].description;
        else
            return '';
    }

    getUserRoleMappings(): EnumMapping[] {
        return UserRoleMappings;
    }

    createUser(user: User) {

        var mapping = ServerRoleMapping.filter(x => x.clientRole == user.role)[0];

        var serverModel: any = {
            username: user.email,
            firstname: user.firstName,
            lastname: user.lastName,
            role: mapping.serverRole,
            domain: user.domain
        };

        if (user.partner && user.partner.id)
            serverModel.partnerid = user.partner.id;

        if (user.enterprise && user.enterprise.id)
            serverModel.enterpriseid = user.enterprise.id;

        return this.http.post(this.rootUrl + glob.UserInsertUrl, serverModel);

    }

    lockUser(userName: string) {

        var serverModel = {  
            username: userName
        };

        return this.http.post(this.rootUrl + glob.UserLockUrl, serverModel);
    }

    unlockUser(userName: string) {

        var serverModel = {
            username: userName
        };

        return this.http.post(this.rootUrl + glob.UserUnlockUrl, serverModel);
    }

    deleteUser(userName: string) {

        var serverModel = {
            username: userName
        };

        return this.http.post(this.rootUrl + glob.UserDeleteUrl, serverModel);
    }

    GetRoleDescrptionByServerRole(serverRole): string {
        if (serverRole) {
            var mapping = ServerRoleMapping.filter(x => x.serverRole == serverRole)[0];

            return UserRoleMappings.filter(x => x.id == mapping.clientRole)[0].description;
        } else {
            return '';
        }

    }

    getServerErrorByCode(code: string) {
        return this.authenticationService.getServerErrorByCode(code);
    }

    getUser(userId) {

        return this.http.get(this.rootUrl + glob.UserGetById + userId);
    }

    getFullName(userName) {

        return this.http.post(this.rootUrl + glob.UserGetFullName, { username: userName});
    }

}

var UserRoleMappings: EnumMapping[] = [
    { id: UserRole.entAdmin, description: 'ent Admin' },
    { id: UserRole.PartnerAdmin, description: 'Partner Admin' },
    { id: UserRole.EnterpriseAdmin, description: 'Enterprise Admin' },

];

export let ServerRoleMapping = [
    { clientRole: UserRole.entAdmin, serverRole: 'admin' },
    { clientRole: UserRole.PartnerAdmin, serverRole: 'partner' },
    { clientRole: UserRole.EnterpriseAdmin, serverRole: 'ec' },
];

export let CurrentUser: User;

