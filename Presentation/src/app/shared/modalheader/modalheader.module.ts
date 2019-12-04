import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ModalheaderComponent } from './modalheader.component';
@NgModule({
    imports: [
        RouterModule,
        CommonModule,
        TranslateModule
    ],
    declarations: [ModalheaderComponent ],
    exports: [ModalheaderComponent ]
})

export class ModalheaderModule {}
