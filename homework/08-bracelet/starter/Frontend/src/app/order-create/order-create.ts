import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { BraceletBuilder } from '../bracelet-builder/bracelet-builder';
import { BraceletPreview } from '../bracelet-preview/bracelet-preview';
import { Api } from '../api/api';
import { BraceletItem, BraceletDataService } from '../bracelet-data.service';
import { createOrder, validateBracelet } from '../api/functions';
import { CreateOrderDto, ValidationResult } from '../api/models';

@Component({
  selector: 'app-order-create',
  imports: [FormsModule, DecimalPipe, BraceletBuilder, BraceletPreview],
  templateUrl: './order-create.html',
  styleUrl: './order-create.css',
})
export class OrderCreate {
  private api = inject(Api);
  private router = inject(Router);
  private braceletDataService = inject(BraceletDataService);

  protected readonly customerName = signal<string>('');
  protected readonly customerAddress = signal<string>('');
  protected readonly bracelets = signal<{bracelet: BraceletItem[], cost: number}[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly errorAtOrder = signal<string | null>(null);
  protected readonly warning = signal<string | null>(null);
  protected readonly total = signal<number>(0);
  protected readonly validating = signal<boolean>(false);
  protected readonly currentBracelet = signal<BraceletItem[]>([]);
  protected readonly currentCost = signal<number>(0);

  onBraceletChanged(items: BraceletItem[]): void {
    this.error.set(null);
    this.warning.set(null);
    this.currentBracelet.set(items);
  }

  removeBracelet(index: number): void {
    this.total.update(current => current - this.bracelets()[index].cost);
    this.bracelets().splice(index, 1);
  }

  async placeOrder(): Promise<void> {
    const data: string[] = this.bracelets().map(b => this.braceletDataService.serialize(b.bracelet));

    const dto: CreateOrderDto = {
      address: this.customerAddress(),
      braceletData: data,
      customerName: this.customerName()
    }
    try {
      await this.api.invoke(createOrder, {body: dto});
      this.router.navigate(["/orders"]);
    } catch(err: any) {
      this.errorAtOrder.set(err.error);
    } 
  }

  async addBraceletToOrder(): Promise<void> {
    const data = this.braceletDataService.serialize(this.currentBracelet());
    
    this.validating.set(true);
    const resp: ValidationResult = await this.api.invoke(validateBracelet, {data: data});
    
    this.warning.set(resp.mixedColorWarning ? "Mixed Colors!" : null);
    if (resp.error == null) {
      if (!resp.mixedColorWarning) {
        this.bracelets().push({bracelet: this.currentBracelet(), cost: resp.cost!});
        this.currentBracelet.set([]);
        this.total.update(t => t + resp.cost!);
      } else {
        this.currentCost.set(resp.cost!);
      }
    } else {
      this.error.set(resp.error);
    }
    this.validating.set(false);
  }

  addAnyway(): void {
    this.bracelets().push({bracelet: this.currentBracelet(), cost: this.currentCost()});
    this.currentBracelet.set([]);
    this.warning.set(null);
    this.total.update(t => t + this.currentCost());
  }
}
