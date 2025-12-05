import { Component, inject, signal, output, ViewChild, ElementRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { form, Field, required, min, max, maxLength } from '@angular/forms/signals';
import { CustomerPatchDto } from '../api/models/customer-patch-dto';
import { customersIdGet, customersIdPatch } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';

interface CustomerFormModel {
  name: string;
  dateOfBirth: string;
  revenue: number;
  customerValue: number;
  isActive: boolean;
}

@Component({
  selector: 'app-customer-edit',
  imports: [Field],
  templateUrl: './customer-edit.html',
  styleUrl: './customer-edit.css',
})
export class CustomerEdit {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  @ViewChild('dialog') dialog!: ElementRef<HTMLDialogElement>;

  // Internal signal for the customer ID
  protected readonly customerId = signal<number | undefined>(undefined);

  // Outputs for dialog control
  saved = output<void>();
  cancelled = output<void>();

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly saving = signal(false);

  // Form with validation
  protected readonly customerModel = signal<CustomerFormModel>({
    name: '',
    dateOfBirth: '',
    revenue: 0,
    customerValue: 0,
    isActive: true,
  });

  protected readonly customerForm = form(this.customerModel, (schemaPath) => {
    required(schemaPath.name, { message: 'Name is required' });
    required(schemaPath.dateOfBirth, { message: 'Date of birth is required' });
    required(schemaPath.revenue, { message: 'Revenue is required' });
    required(schemaPath.customerValue, { message: 'Customer value is required' });

    maxLength(schemaPath.name, 50, { message: 'Name must be at most 50 characters' });

    min(schemaPath.revenue, 0, { message: 'Revenue must be at least 0' });
    min(schemaPath.customerValue, 0, { message: 'Customer value must be at least 0' });
    max(schemaPath.customerValue, 10, { message: 'Customer value must be at most 10' });
  });

  open(id: number) {
    this.customerId.set(id);
    this.dialog.nativeElement.showModal();
    this.loadCustomer();
  }

  close() {
    this.dialog.nativeElement.close();
  }

  private async loadCustomer() {
    console.log('Loading customer...');
    this.loading.set(true);
    this.error.set(null);

    try {
      const id = this.customerId();
      console.log(id);
      if (!id) throw new Error('No customer ID provided');

      const response = await firstValueFrom(
        customersIdGet(this.http, this.apiConfig.rootUrl, { id })
      );
      console.log(response);
      const customer = response.body;

      // Fill form with customer data
      this.customerModel.set({
        name: customer.name,
        dateOfBirth: customer.dateOfBirth,
        revenue: customer.revenue,
        customerValue: customer.customerValue,
        isActive: customer.isActive,
      });
    } catch (error) {
      console.log(error);
      this.error.set('Error loading customer: ' + JSON.stringify(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSubmit(event: Event) {
    event.preventDefault();

    // Check if all fields are valid
    if (
      this.customerForm.name().invalid() ||
      this.customerForm.dateOfBirth().invalid() ||
      this.customerForm.revenue().invalid() ||
      this.customerForm.customerValue().invalid()
    ) {
      this.error.set('Please correct the errors in the form.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    try {
      const id = this.customerId();
      if (!id) throw new Error('No customer ID provided');

      const formData = this.customerModel();
      const patchDto: CustomerPatchDto = {
        name: formData.name,
        dateOfBirth: formData.dateOfBirth,
        revenue: formData.revenue,
        customerValue: formData.customerValue,
        isActive: formData.isActive,
      };

      await firstValueFrom(
        customersIdPatch(this.http, this.apiConfig.rootUrl, {
          id,
          body: patchDto,
        })
      );

      this.saved.emit();
      this.close();
    } catch (error: any) {
      this.error.set('Error saving: ' + (error.message || JSON.stringify(error)));
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel() {
    if (this.customerForm().dirty()) {
      if (!confirm('You have unsaved changes. Are you sure you want to cancel?')) {
        return;
      }
    }

    this.cancelled.emit();
    this.close();
  }
}
