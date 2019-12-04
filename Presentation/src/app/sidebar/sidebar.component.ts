import { Component, OnInit, AfterViewInit, AfterViewChecked, AfterContentInit } from '@angular/core';
import { Router } from '@angular/router';

import { AuthenticationService } from '../core/services/authentication.service';
import { UserService } from '../core/services/user.service';
import { User } from '../core/models/user';
import { UserRole } from '../core/models/userRole';
import { TranslateService } from '@ngx-translate/core';

declare var $: any;
//Metadata
export interface RouteInfo {
    path: string;
    title: string;
    type: string;
    icontype: string;
    // icon: string;
    children?: ChildrenItems[];
}

export interface ChildrenItems {
    path: string;
    title: string;
    ab: string;
    type?: string;
}


@Component({
    moduleId: module.id,
    selector: 'sidebar-cmp',
    templateUrl: 'sidebar.component.html',
    styleUrls: ['./sidebar.component.css']
})

export class SidebarComponent implements OnInit {
    public menuItems: any[];
    public user: User;
    public hideUser: boolean = true;
    public hidePartner: boolean = true;
    public hideEnterprise: boolean = true;
    public hideSubsription: boolean = true;
    public hideAdmin: boolean = true;
    public UserFullName: string = 'User Name';

    constructor(
        private router: Router,
        private authenticationService: AuthenticationService,
        private userService: UserService,
        private translateService: TranslateService,
    ) {
    }

    isNotMobileMenu() {
        if ($(window).width() > 991) {
            return false;
        }
        return true;
    }

    ngOnInit() {

        this.translateService.get('SideBar.Dashboard').subscribe((res: string) => {
            ROUTES[0].title = res;
        });

        var isWindows = navigator.platform.indexOf('Win') > -1 ? true : false;

        this.user = this.userService.getCurrentUser();
        this.hideMenuItems();

        isWindows = navigator.platform.indexOf('Win') > -1 ? true : false;

        if (isWindows) {
            // if we are on windows OS we activate the perfectScrollbar function
            $('.sidebar .sidebar-wrapper, .main-panel').perfectScrollbar();
            $('html').addClass('perfect-scrollbar-on');
        } else {
            $('html').addClass('perfect-scrollbar-off');
        }

        this.setUserName();

    }

    setUserName() {
        this.userService.getFullName(this.user.userId).subscribe(
                (data: any) => {
                    if (data.d.fullname)
                        this.UserFullName = data.d.fullname;
                },
                (err) => {
                }
            );

    }

    hideMenuItems() {

        if (this.user.role == UserRole.entAdmin) {
            this.hideUser = false;

            this.hidePartner = false;
            this.hideEnterprise = false;
            this.hideSubsription = false;
            this.hideAdmin = false;

        } else if (this.user.role == UserRole.PartnerAdmin) {
            this.hideUser = true;
            this.hidePartner = true;
            this.hideEnterprise = true;
            this.hideSubsription = true;
            this.hideAdmin = true;
        } else if (this.user.role == UserRole.EnterpriseAdmin) {
            this.hideUser = true;
            this.hidePartner = true;
            this.hideEnterprise = true;
            this.hideSubsription = true;
            this.hideAdmin = true;
        }

        this.menuItems = ROUTES;

        if (this.hidePartner)
            this.menuItems = this.menuItems.filter(menuItem => menuItem.title != 'Partners');

        if (this.hideEnterprise)
            this.menuItems = this.menuItems.filter(menuItem => menuItem.title != 'Enterprises');

        if (this.hideSubsription)
            this.menuItems = this.menuItems.filter(menuItem => menuItem.title != 'Subscriptions');

        if (this.hideUser)
            this.menuItems = this.menuItems.filter(menuItem => menuItem.title != 'Users');

        if (this.hideAdmin)
            this.menuItems = this.menuItems.filter(menuItem => menuItem.title != 'Admin');

    }

    ngAfterViewInit() {
        var $sidebarParent = $('.sidebar .nav > li.active .collapse li.active > a').parent().parent().parent();

        var collapseId = $sidebarParent.siblings('a').attr("href");

        $(collapseId).collapse("show");
    }

    logout() {

        this.authenticationService.logOut();

        this.router.navigateByUrl('/pages/login');

    }
}

//Menu Items
export let ROUTES: RouteInfo[] = [
    {
        path: '/dashboard/overview',
        title: 'Dashboard',
        type: 'link',
        icontype: 'ti-dashboard',
    }, 
    {
        path: '/partner/list',
        title: 'Partners',
        type: 'link',
        icontype: 'ti-world',
    }, {
        path: '/enterprise/list',
        title: 'Enterprises',
        type: 'link',
        icontype: 'ti-id-badge',
    }, {
        path: '/subscription/list',
        title: 'Subscriptions',
        type: 'link',
        icontype: 'ti-view-list-alt',
    }, {
        path: '/user/list',
        title: 'Users',
        type: 'link',
        icontype: 'ti-user'
    }, {
        path: '/admin/report',
        title: 'Admin',
        type: 'link',
        icontype: 'ti-write'
    }
];
