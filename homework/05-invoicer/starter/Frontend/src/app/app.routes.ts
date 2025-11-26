import { Routes } from '@angular/router';
import { TimeentryList } from './timeentry-list/timeentry-list';
import { TimeentryEdit } from './timeentry-edit/timeentry-edit';

export const routes: Routes = [
    { path: '', redirectTo: 'entries', pathMatch: 'full' },
    { path: 'entries', component: TimeentryList },
    { path: 'entries/:id', component: TimeentryEdit },
];
