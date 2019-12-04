import { Injectable } from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpHeaders,
    HttpResponse,
    HttpErrorResponse
} from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import { SpinnerService } from './spinner.service';
import { Router } from '@angular/router';


@Injectable()
export class TokenInterceptor implements HttpInterceptor {
    constructor(private spinnerService: SpinnerService, private router: Router) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        var newUrl = this.getNewUrl(req);

        var skipSpinner = false;
        if (newUrl.indexOf('report/start') >= 0)
            skipSpinner = true;

        const authReq = req.clone({
            headers: req.headers.set('Authorization', `Bearer ` + localStorage.getItem('managerToken')),
            url: newUrl
        });

        setTimeout(() => {
            if (!skipSpinner)
                this.spinnerService.markLoading();
        }, 0)
        return next.handle(authReq).do((event: HttpEvent<any>) => {
            if (event instanceof HttpResponse)
                setTimeout(() => {
                    if (!skipSpinner)
                        this.spinnerService.markFinishedLoading();
                }, 0);

        }, (err: any) => {
                setTimeout(() => {
                    if (!skipSpinner)
                        this.spinnerService.markFinishedLoading();
                }, 0);

                if (err instanceof HttpErrorResponse) {
                    if (err.status === 401)
                        this.router.navigate(['/pages/login']);
                }
            }
        );
    }

    getNewUrl(req) : string {
        var newUrl;
        if (req.url.indexOf('token') >= 0) {
            newUrl = req.url
        }
        else if (req.url.indexOf('?') >= 0) {
            newUrl = req.url + "&datetime=" + new Date().getTime();
        }
        else {
            newUrl = req.url + "?datetime=" + new Date().getTime();
        }

        return newUrl;
    }
}
