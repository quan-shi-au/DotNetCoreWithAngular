import { Component, OnInit } from '@angular/core';
import { SpinnerService } from './core/services/spinner.service';
import { LanguageService } from './core/services/language.service';
import { Router, NavigationEnd } from '@angular/router';

declare var $: any;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit  {

    constructor(
        private spinnerService: SpinnerService,
        private languageService: LanguageService,
        private router: Router
    ) {

    }

    ngOnInit() {
        this.languageService.setDefaultLanguage();
        this.languageService.initializeLanguage();

        this.router.events.subscribe((evt) => {

            if (!(evt instanceof NavigationEnd)) {
                return;
            }

            $('.main-panel').scrollTop(0);
        });
    }

    getLoading() {

        return this.spinnerService.getIsLoadding();
    }
}
