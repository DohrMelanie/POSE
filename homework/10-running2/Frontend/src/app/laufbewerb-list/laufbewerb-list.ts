import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ApiConfiguration } from '../api/api-configuration';
import { Api } from '../api/api';
import { CategoryDto, CompetitionDto } from '../api/models';
import { deleteCompetitionById, getCategories, getCompetitions } from '../api/functions';

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
  protected readonly filterCategory = signal<number | null>(null);
  protected readonly categories = signal<CategoryDto[]>([]);
  protected readonly competitions = signal<CompetitionDto[]>([]);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  async ngOnInit() {
    this.loading.set(true);
    this.categories.set(await this.api.invoke(getCategories, {}));
    this.competitions.set(await this.api.invoke(getCompetitions, {}));
    this.loading.set(false);
  }

  async applyFilter() {
    this.loading.set(true);
    try {
      this.competitions.set(await this.api.invoke(getCompetitions, {
        categoryId: this.filterCategory() ?? undefined,
        name: this.filterName() ?? undefined
      }));
    } catch(err: any) {
      this.error.set(err.error);
    }

    this.loading.set(false);
  }
  async cancelFilter() {
    this.loading.set(true);
    this.filterCategory.set(null);
    this.filterName.set(null);
    this.competitions.set(await this.api.invoke(getCompetitions, {}));
    this.loading.set(false);
  }

  async deleteCompetition(compId: number) {
    this.loading.set(true);    
    await this.api.invoke(deleteCompetitionById, {id: compId});

    try {
      this.competitions.set(await this.api.invoke(getCompetitions, {
        categoryId: this.filterCategory() ?? undefined,
        name: this.filterName() ?? undefined
      }));
    } catch(err: any) {
      this.error.set(err.error);
    }

    this.loading.set(false);
  }

  getCategoryName(categoryId: number) { 
    const category = this.categories().find(c => c.id === categoryId);
    return category ? category.name : 'Unknown';
  }
}
