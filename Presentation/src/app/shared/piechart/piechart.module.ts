import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PiechartComponent } from './piechart.component';
import { FormsModule } from '@angular/forms';

@NgModule({
    imports: [
        RouterModule, CommonModule, FormsModule,
    ],
    declarations: [ PiechartComponent ],
    exports: [ PiechartComponent ]
})

export class PiechartModule {}
