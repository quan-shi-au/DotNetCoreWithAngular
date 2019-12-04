import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { UserService } from './user.service';
import { UserRole } from '../models/userRole';

@Injectable()
export class ScopeGuardService implements CanActivate {

    constructor(private router: Router, private userService: UserService) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {

        const scopes = (route.data as any).expectedScopes;

        var currentUser = this.userService.getCurrentUser();

        if (this.userService.userHasScopes(currentUser.role, scopes))
            return true;

        this.router.navigate(['/pages/login'], { queryParams: { returnUrl: state.url } });
        return false;
    }
}