import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { TravelListItemDto, TravelReimbursementDto } from '../api/models';
import { getTravels } from '../api/functions';

@Component({
  selector: 'app-travels-list',
  imports: [RouterLink],
  templateUrl: './travels-list.html',
  styleUrl: './travels-list.css'
})
export class TravelsList {
  private readonly api = inject(Api);
  protected readonly travels = signal<TravelListItemDto[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly error = signal<string | null>(null);

  async ngOnInit() {
    try {
      this.travels.set(await this.api.invoke(getTravels));
    } catch {
      this.error.set("Something went wrong");
    }
    this.loading.set(false);
  }
}
