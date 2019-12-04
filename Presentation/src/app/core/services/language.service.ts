import { Injectable, OnInit, OnDestroy } from '@angular/core'
import { TranslateService } from '@ngx-translate/core';
import * as glob from '../variables/global.variable';

@Injectable()
export class LanguageService  {

    constructor(
        private translateService: TranslateService
    ) {
    }

    initializeLanguage() {

        var storedLanguageId = this.getLanguageId();
        if (storedLanguageId) {
            this.translateService.use(storedLanguageId);
        } else {
            this.translateService.use(glob.LanguageIdEnglish);
            this.storeLanguageId(glob.LanguageIdEnglish);
        }
    }

    setDefaultLanguage() {
        this.translateService.setDefaultLang(glob.LanguageIdEnglish);
    }

    configureLanguage(language) {
        this.translateService.use(language);
    }

    storeLanguageId(languageId) {
        localStorage.setItem('managerLanguageId', languageId);
    }

    getLanguageId() {
        return localStorage.getItem('managerLanguageId');
    }

}

