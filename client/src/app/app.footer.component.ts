import {Component} from '@angular/core';

@Component({
    selector: 'app-footer',
    template: `
        <div class="layout-footer clearfix">
            <img src="assets/layout/images/auto-filer.svg">
            <div class="layout-footer-right">
                <a href="https://github.com/primefaces"><i class="pi pi-github"></i></a>
            </div>
        </div>
    `
})
export class AppFooterComponent {

}
