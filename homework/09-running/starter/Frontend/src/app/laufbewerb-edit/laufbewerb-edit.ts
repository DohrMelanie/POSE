import { CompetitionReqDto } from './../api/models/competition-req-dto';
import { CategoryDto } from './../api/models/category-dto';
import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { form, FormField, maxLength, min, PathKind, required, SchemaPath } from '@angular/forms/signals';
import { ApiConfiguration } from '../api/api-configuration';
import { Api } from '../api/api';
import { CompetitionDto } from '../api/models';
import { createCompetition, getCategories, getCompetitionById, updateCompetition } from '../api/functions';

@Component({
  selector: 'app-laufbewerb-edit',
  standalone: true,
  imports: [FormField],
  templateUrl: './laufbewerb-edit.html',
  styleUrl: './laufbewerb-edit.css',
})
export class LaufbewerbEdit implements OnInit{
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  id = input<number | null>(null);
  protected readonly title = computed<string>(() => this.id() == null ? "Laufbewerb hinzufügen" : "Laufbewerb ändern");
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<boolean>(false);
  protected readonly saving = signal<boolean>(false);
  protected readonly categories = signal<CategoryDto[]>([]);
  protected readonly comp = signal<CompetitionDto>({
    id: -1,
    category: {
      id: -1,
      name: ''
    },
    date: '',
    length: 0,
    name: '',
    place: ''
  });

  protected readonly form = form(this.comp, (schema) => {
    required(schema.name, { message: 'Name is required' });
    maxLength(schema.name, 100, { message: 'Max length is 100 characters' });
    required(schema.category, { message: 'Category is required' });
    required(schema.date, { message: 'Date is required' });
    maxLength(schema.place, 100, { message: 'Max length is 100 characters' });
    maxLength(schema.name, 100, { message: 'Max length is 100 characters' });
    min(schema.length, 0.01, { message: 'Minimum is 0.01' });
    custom(schema.length, (val) => (val * 100) % 1 === 0, { 
      message: 'Only 2 decimal places allowed' 
    });
  });

  async ngOnInit() {
    if (this.id() != null) {
      this.comp.set(await this.api.invoke(getCompetitionById, {
        id: this.id()!
      }));
    }
    this.categories.set(await this.api.invoke(getCategories, {}));
  }
  async save() {
    if (this.id() == null) {
      const body: CompetitionReqDto = {
        category: this.comp().category,
        length: this.comp().length,
        name: this.comp().name,
        place: this.comp().place
      };
      await this.api.invoke(createCompetition, { body });
    } else {
      const body = this.comp();
      await this.api.invoke(updateCompetition, { body });
    }

    this.router.navigate(["/laufbewerbe"]);
  }

  async cancel() {
    this.router.navigate(["/laufbewerbe"]);
  }

  async onCategoryChange(event: Event) {
    let value = (event.target as HTMLSelectElement).value;
    this.comp().category = this.categories().find(c => c.id == parseInt(value))!;
  }
}
function custom(length: SchemaPath<number, 1, PathKind.Child>, arg1: (val: any) => boolean, arg2: { message: string; }) {
  throw new Error('Function not implemented.');
}

