import { getCompetitions } from './../api/fn/laufbewerbe-endpoints/get-competitions';
import { getCategories } from './../api/fn/laufbewerbe-endpoints/get-categories';
import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ApiConfiguration } from '../api/api-configuration';
import { Api } from '../api/api';
import { CategoryDto, CompetitionDto } from '../api/models';
import { deleteCompetition } from '../api/functions';

@Component({
  selector: 'app-laufbewerb-list',
  standalone: true,
  imports: [RouterLink, FormsModule, DecimalPipe],
  templateUrl: './laufbewerb-list.html',
  styleUrl: './laufbewerb-list.css',
})
export class LaufbewerbList implements OnInit{
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly api = inject(Api);

  protected readonly filterName = signal<string | null>(null);
  protected readonly filterCategory = signal<CategoryDto | null>(null);
  protected readonly categories = signal<CategoryDto[]>([]);
  protected readonly competitions = signal<CompetitionDto[]>([]);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<boolean>(false);

  async ngOnInit() {
    this.loading.set(true);
    this.categories.set(await this.api.invoke(getCategories, {}));
    this.competitions.set(await this.api.invoke(getCompetitions, {}));
    this.loading.set(false);
  }

  async applyFilter() {
    this.loading.set(true);
    console.log(this.filterCategory());
    try {
      this.competitions.set(await this.api.invoke(getCompetitions, {
        name: this.filterName() ?? undefined,
        categoryId: this.filterCategory()?.id
      }));
    } catch(err: any) {
      this.error.set(err);
    }

    this.loading.set(false);
  }

  applyFilterCategory(event: Event) {
    let value = (event.target as HTMLSelectElement).value;
    this.filterCategory.set(this.categories().find(c => c.id == parseInt(value))!);
  }

  async disableFilter() {
    this.loading.set(true);
    this.filterName.set(null);
    this.filterCategory.set(null);
    this.competitions.set(await this.api.invoke(getCompetitions, {}));
  }

  async deleteComp(compId: number) {
    this.loading.set(true);
    try {
      await this.api.invoke(deleteCompetition, {id: compId});
      this.competitions.set(await this.api.invoke(getCompetitions, {
        name: this.filterName() ?? undefined,
        categoryId: this.filterCategory()?.id
      }));
    } catch(err: any) {
      this.error.set(err);
    }
    this.loading.set(false);
  }
 }
