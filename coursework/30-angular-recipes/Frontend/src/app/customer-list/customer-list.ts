import { Component, inject, signal, computed, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe, CurrencyPipe } from '@angular/common';

import { firstValueFrom } from 'rxjs';
import { Customer } from '../api/models/customer';
import { CustomerEdit } from '../customer-edit/customer-edit';
import { form, Field, required, min, max, maxLength } from '@angular/forms/signals';
import { customersGet, customersIdDelete, customersIdPatch } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';

interface CustomerFormModel {
  name: string;
  dateOfBirth: string;
  revenue: number;
  customerValue: number;
  isActive: boolean;
}

// TODO: If unfamiliar, research about Angular standalone components (no NgModule needed)
@Component({
  selector: 'app-customer-list',
  imports: [DatePipe, CurrencyPipe, CustomerEdit, Field],
  templateUrl: './customer-list.html',
  styleUrl: './customer-list.css',
})
export class CustomerList implements OnInit {
  @ViewChild(CustomerEdit) customerEdit!: CustomerEdit;
  // TODO: If unfamiliar, research about Angular inject (dependency injection)
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  // TODO: If unfamiliar, research about Angular signals (in this case writeable signals)
  protected readonly customers = signal<Customer[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly selectedCustomerIds = signal<number[]>([]);
  protected readonly editingCustomerId = signal<number | undefined>(undefined);

  // TODO: If unfamiliar, research about Angular computed signals
  protected readonly selectedCount = computed(() => this.selectedCustomerIds().length);
  protected readonly hasSelection = computed(() => this.selectedCount() > 0);

  ngOnInit(): void {
    this.loadCustomers();
  }

  private async loadCustomers() {
    this.loading.set(true);
    this.error.set(null);

    try {
      // TODO: If unfamiliar, research about Angular firstValueFrom (to convert an Observable to a Promise)
      const response = await firstValueFrom(customersGet(this.http, this.apiConfig.rootUrl));
      this.customers.set(response.body);
    } catch (error) {
      this.error.set('Error loading customers: ' + JSON.stringify(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected async deleteCustomer(id: number): Promise<void> {
    const customer = this.customers().find((c) => c.id === id);
    if (!customer) return;

    const confirmed = confirm(
      `Do you really want to delete customer "${customer.name}"?\n\nThis action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      await firstValueFrom(customersIdDelete(this.http, this.apiConfig.rootUrl, { id }));
      // TODO: If unfamiliar, research about signal update function (for immutable state updates)
      // Remove customer from list
      this.customers.update((currentCustomers) => currentCustomers.filter((c) => c.id !== id));
    } catch (err: any) {
      alert('Error deleting customer: ' + err.message);
    }
  }

  protected toggleCustomerSelection(id: number): void {
    this.selectedCustomerIds.update((currentSelection) => {
      if (currentSelection.includes(id)) {
        // Remove from selection
        return currentSelection.filter((customerId) => customerId !== id);
      } else {
        // Add to selection
        return [...currentSelection, id];
      }
    });
  }

  protected showEditDialog(id: number): void {
    this.editingCustomerId.set(id);
    this.customerEdit.open(id);
  }

  protected onCustomerSaved(): void {
    this.loadCustomers();
  }

  protected onCustomerCancelled(): void {
    // Dialog closes itself, nothing else to do
  }

  protected readonly inlineEditCustomerId = signal<number | undefined>(undefined);

  protected readonly inlineCustomerModel = signal<CustomerFormModel>({
    name: '',
    dateOfBirth: '',
    revenue: 0,
    customerValue: 0,
    isActive: false,
  });

  protected readonly inlineForm = form(this.inlineCustomerModel, (schemaPath) => {
    required(schemaPath.name);
    maxLength(schemaPath.name, 50);
    required(schemaPath.dateOfBirth);
    required(schemaPath.revenue);
    min(schemaPath.revenue, 0);
    required(schemaPath.customerValue);
    min(schemaPath.customerValue, 0);
    max(schemaPath.customerValue, 10);
  });

  protected editInline(id: number): void {
    const customer = this.customers().find((c) => c.id === id);
    if (customer) {
      this.inlineEditCustomerId.set(id);
      this.inlineCustomerModel.set({
        name: customer.name,
        dateOfBirth: customer.dateOfBirth,
        revenue: customer.revenue,
        customerValue: customer.customerValue,
        isActive: customer.isActive,
      });
    }
  }

  protected async saveInline(): Promise<void> {
    const id = this.inlineEditCustomerId();

    if (!this.inlineForm().valid()) {
      alert('Please correct the errors.');
      return;
    }

    if (!id) return;

    try {
      const formData = this.inlineCustomerModel();
      await firstValueFrom(
        customersIdPatch(this.http, this.apiConfig.rootUrl, {
          id: id,
          body: {
            name: formData.name,
            dateOfBirth: formData.dateOfBirth,
            revenue: formData.revenue,
            customerValue: formData.customerValue,
            isActive: formData.isActive,
          },
        })
      );

      this.inlineEditCustomerId.set(undefined);
      this.loadCustomers();
    } catch (error: any) {
      alert('Error saving customer: ' + (error.message || JSON.stringify(error)));
    }
  }

  protected cancelInline(): void {
    this.inlineEditCustomerId.set(undefined);
  }

  protected isCustomerSelected(id: number): boolean {
    return this.selectedCustomerIds().includes(id);
  }

  protected async deleteSelectedCustomers(): Promise<void> {
    const selectedIds = this.selectedCustomerIds();
    if (selectedIds.length === 0) return;

    const customerNames = this.customers()
      .filter((c) => selectedIds.includes(c.id!))
      .map((c) => c.name)
      .join(', ');

    const confirmed = confirm(
      `Do you really want to delete ${selectedIds.length} customer(s)?\n\n${customerNames}\n\nThis action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      // Delete all selected customers
      await Promise.all(
        selectedIds.map((id) =>
          firstValueFrom(customersIdDelete(this.http, this.apiConfig.rootUrl, { id }))
        )
      );

      // Remove deleted customers from list
      this.customers.update((currentCustomers) =>
        currentCustomers.filter((c) => !selectedIds.includes(c.id!))
      );

      // Clear selection
      this.selectedCustomerIds.update(() => []);
    } catch (err: any) {
      alert('Error deleting customers: ' + err.message);
    }
  }
}
