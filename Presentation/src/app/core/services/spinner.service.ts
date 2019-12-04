import { Injectable } from '@angular/core'

@Injectable()
export class SpinnerService {

    markLoading() {

        loaddingNumbers++;
    }

    markFinishedLoading() {
        if (loaddingNumbers > 0)
            loaddingNumbers--;

    }

    getIsLoadding() {
        return loaddingNumbers > 0;
    }


}

export let loaddingNumbers: number = 0;

