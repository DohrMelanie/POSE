import { Component, inject, model, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { Api } from '../api/api';
import { OrderDto } from '../api/models';
import { getOrders } from '../api/functions';

@Component({
  selector: 'app-order-list',
  imports: [RouterLink, FormsModule, DatePipe, CurrencyPipe],
  templateUrl: './order-list.html',
  styleUrl: './order-list.css',
})
export class OrderList implements OnInit {

  private api = inject(Api);

  protected readonly minCost = signal<number | null>(null);
  protected readonly orders = signal<OrderDto[]>([]);

  async ngOnInit() {
    const orders = await this.api.invoke(getOrders, {});
    this.orders.set(orders);
  }

  async applyFilter(): Promise<void> {
    const orders = await this.api.invoke(getOrders, {minCost: this.minCost() ?? undefined});
    this.orders.set(orders);
  }
  // TODO: Add logic for component here
}
