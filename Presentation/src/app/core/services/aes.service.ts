import {Injectable } from '@angular/core'
import { HttpClient } from '@angular/common/http';
import { HttpErrorResponse } from '@angular/common/http';
import { UtilityService } from './utility.service';

import { EnvironmentSpecificService } from './environmentSpecific.service';
import { EnvSpecific } from '../models/envSpecific';

import * as glob from '../variables/global.variable';

declare let CryptoJS: any;

@Injectable()
export class AesService {

    public rootUrl: string;

    constructor(private http: HttpClient, private utilityService: UtilityService, private envSpecificSvc: EnvironmentSpecificService) {
        envSpecificSvc.subscribe(this, this.setRootUrl);
    }

    setRootUrl(caller: any, es: EnvSpecific) {
        const thisCaller = caller as AesService;
        thisCaller.rootUrl = es.rooturl;
    }

    getKey(subscriptionId: number) {
        return this.http.get(this.rootUrl + glob.KeyGetUrl + subscriptionId);
    }

    processEncrypt(subscriptionId: number, text: string) {
        let serverModel = {
            subscriptionId: subscriptionId.toString(),
            plainText: text
        };

        return this.http.post(this.rootUrl + glob.TestEncryptUrl, serverModel);
    }

    processDecrypt(base64Key: string, base64Iv: string, base64Encrypted: string) {
        let serverModel = {
            key: base64Key,
            vector: base64Iv,
            encryptedString: base64Encrypted 
        };

        return this.http.post(this.rootUrl + glob.TestDecryptUrl, serverModel);
    }

    decrypt(myVector: string, myEncrypted: string, mykey: string) {

        var iv = CryptoJS.enc.Base64.parse(myVector);
        var key = CryptoJS.enc.Base64.parse(mykey);

        var decrypted = CryptoJS.AES.decrypt(myEncrypted, key, {
            iv: iv,
            padding: CryptoJS.pad.Pkcs7,
            mode: CryptoJS.mode.CBC

        })

        var decryptedString = decrypted.toString(CryptoJS.enc.Utf8);

        return decryptedString;

    }

}