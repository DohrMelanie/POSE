import { Component, inject, input, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import {
  form,
  FormField,
  maxLength,
  min,
  pattern,
  required,
  validate,
} from '@angular/forms/signals';
import { ApiConfiguration } from '../api/api-configuration';
import { Api } from '../api/api';
import { CategoryDto, CompetitionDto, CompetitionReqDto } from '../api/models';
import {
  createCompetition,
  getCategories,
  getCompetitionById,
  updateCompetition,
} from '../api/functions';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-laufbewerb-edit',
  standalone: true,
  imports: [FormField, FormsModule],
  templateUrl: './laufbewerb-edit.html',
  styleUrl: './laufbewerb-edit.css',
})
export class LaufbewerbEdit implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  id = input<number | null>(null);

  protected readonly model = signal<CompetitionReqDto>({
    categoryId: 0,
    date: '',
    length: 0,
    name: '',
    place: '',
  });

  protected readonly loading = signal<boolean>(false);
  protected readonly saving = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly categories = signal<CategoryDto[]>([]);

  protected readonly addForm = form(this.model, (schema) => {
    required(schema.name, { message: 'Name is required' });
    required(schema.date, { message: 'Date is required' });
    required(schema.place, { message: 'Place is required' });
    required(schema.categoryId, { message: 'Category is required' });

    maxLength(schema.name, 100, { message: 'Max length is 100 characters' });
    maxLength(schema.place, 50, { message: 'Max length is 100 characters' });
    min(schema.length, 0.01, { message: 'Minimum for length is 0.01' });
  });

  async ngOnInit() {
    try {
      if (this.id() != null) {
        this.model.set(await this.api.invoke(getCompetitionById, { id: this.id()! }));
      }
      this.categories.set(await this.api.invoke(getCategories, {}));
    } catch (err: any) {
      this.error.set(err.error);
    }
  }

  async save() {
    this.saving.set(true);
    try {
      if (this.id() == null) {
        await this.api.invoke(createCompetition, { body: this.model() });
      } else {
        console.log(this.model());
        const dto = {
          categoryId: this.model().categoryId!,
          date: this.model().date!,
          length: this.model().length!,
          name: this.model().name!,
          place: this.model().place!,
          id: this.id()!,
        };
        await this.api.invoke(updateCompetition, { body: dto });
      }
    } catch (err: any) {
      this.error.set(err.error);
    }
    this.saving.set(false);
    this.router.navigate(['/laufbewerbe']);
  }

  async cancel() {
    this.router.navigate(['/laufbewerbe']);
  }

  onCategoryChange(event: Event) {
    let value = (event.target as HTMLSelectElement).value;
    this.model().categoryId = parseInt(value);
  }
}
