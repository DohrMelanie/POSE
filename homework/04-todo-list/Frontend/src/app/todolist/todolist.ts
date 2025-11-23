import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiConfiguration } from '../api/api-configuration';
import { TodoItem } from '../api/models';
import { Api } from '../api/api';
import { environment } from '../../environments/environment';
import { getAllTodoItems, itemsPost, itemsIdPut, itemsIdDelete } from '../api/functions';

@Component({
  selector: 'app-todolist',
  imports: [FormsModule],
  templateUrl: './todolist.html',
  styleUrl: './todolist.css',
})
export class Todolist {
  protected readonly todos = signal<TodoItem[]>([]);
  protected newTodoTitle = signal<string>('');
  protected newTodoAssignee = signal<string>('');
  
  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadTodos();
  }

  async loadTodos() {
    const items = await this.api.invoke(getAllTodoItems, {});
    this.todos.set(items || []);
  }

  async addTodo() {
    if (!this.newTodoTitle() || !this.newTodoAssignee()) {
      return;
    }

    const newTodo: TodoItem = {
      title: this.newTodoTitle(),
      assignee: this.newTodoAssignee(),
      isCompleted: false,
    };

    await this.api.invoke(itemsPost, { body: newTodo });
    this.newTodoTitle.set('');
    this.newTodoAssignee.set('');
    await this.loadTodos();
  }

  async updateTodo(todo: TodoItem) {
    if (todo.id) {
      await this.api.invoke(itemsIdPut, { 
        id: todo.id, 
        body: todo 
      });
    }
  }

  async deleteTodo(id: number | undefined) {
    if (id) {
      await this.api.invoke(itemsIdDelete, { id });
      await this.loadTodos();
    }
  }

  async toggleComplete(todo: TodoItem) {
    todo.isCompleted = !todo.isCompleted;
    await this.updateTodo(todo);
  }
}
