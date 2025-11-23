import { Routes } from '@angular/router';
import { Todolist } from './todolist/todolist';

export const routes: Routes = [
    { path: 'todos', component: Todolist },
    { path: '', redirectTo: '/todos', pathMatch: 'full' }
];
