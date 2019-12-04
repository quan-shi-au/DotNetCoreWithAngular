import { Component, OnInit, OnDestroy, ViewChild, HostListener } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';
import { LocationStrategy, PlatformLocation, Location } from '@angular/common';
import 'rxjs/add/operator/filter';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import * as glob from '../../core/variables/global.variable';

declare var $: any;

@Component({
    selector: 'app-layout',
    templateUrl: './admin-layout.component.html'
})

export class AdminLayoutComponent implements OnInit {
    location: Location;
    private _router: Subscription;
    // url: string;

    @ViewChild('sidebar') sidebar;
    @ViewChild(NavbarComponent) navbar: NavbarComponent;
    constructor( private router: Router, location:Location ) {
      this.location = location;
    }

    ngOnInit() {
        this._router = this.router.events.filter(event => event instanceof NavigationEnd).subscribe(event => {
            //   this.url = event.url;
            this.navbar.sidebarClose();
        });

        var isWindows = navigator.platform.indexOf('Win') > -1 ? true : false;
        if (isWindows){
            var $main_panel = $('.main-panel');
            $main_panel.perfectScrollbar();
        }

    }

    @HostListener('window:keyup', [])
    @HostListener('window:click', [])
    @HostListener('window:mousemove', [])
    onUserEvent() {
        localStorage.setItem(glob.LastUserActiveDate, (new Date()).valueOf().toString());
    }

    scrollCalled() {
        localStorage.setItem(glob.LastUserActiveDate, (new Date()).valueOf().toString());
    }

    public isMap(){
        // console.log(this.location.prepareExternalUrl(this.location.path()));
        if(this.location.prepareExternalUrl(this.location.path()) == '/maps/fullscreen'){
            return true;
        }
        else {
            return false;
        }
    }
}
