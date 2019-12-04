import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SearchbarComponent } from './searchbar.component';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
    imports: [
        RouterModule, CommonModule, FormsModule, TranslateModule
    ],
    declarations: [ SearchbarComponent ],
    exports: [ SearchbarComponent ]
})

export class SearchbarModule {}
