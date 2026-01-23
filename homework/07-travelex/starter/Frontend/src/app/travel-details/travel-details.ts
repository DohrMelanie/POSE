import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { DatePipe, DecimalPipe } from '@angular/common';
import { TravelDetailsDto } from '../api/models';
import { getTravelDetailsById } from '../api/functions';

@Component({
  selector: 'app-travel-details',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './travel-details.html',
  styleUrl: './travel-details.css'
})
export class TravelDetails implements OnInit {
  private readonly api = inject(Api);
  protected readonly travel = signal<TravelDetailsDto>({  
    end: "",
    expenses: -1,
    id: -1,
    mileage: -1,
    perDiem: -1,
    purpose: "",
    reimbursements: [],
    start: "",
    travelerName: ""
  });
  protected readonly total = computed<number>(() => this.travel().expenses + this.travel().mileage + this.travel().perDiem);
  protected readonly loading = signal<boolean>(true);
  protected readonly error = signal<string | null>(null);
  private route = inject(ActivatedRoute);
  public id = input.required<number>();

  async ngOnInit() {
    try {
      this.travel.set(await this.api.invoke(getTravelDetailsById, {id: this.id()}));
    } catch {
      this.error.set("Something went wrong");
    }
    this.loading.set(false);
  }
}
