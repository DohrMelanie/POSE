import { Routes } from '@angular/router';
import { CashregisterComponent } from './cashregister/cashregister.component';

export const routes: Routes = [
    { path: 'cashregister', component: CashregisterComponent},
    { path: '', redirectTo: 'cashregister', pathMatch: 'full' },
];
