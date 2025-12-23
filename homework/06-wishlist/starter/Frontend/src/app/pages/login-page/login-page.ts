import { ChangeDetectionStrategy, Component, inject, Inject, OnInit, signal } from '@angular/core';
import { Field, form, maxLength, minLength, required } from '@angular/forms/signals';
import { SessionService } from '../../services/session.service';
import { Router } from '@angular/router';
import { Api } from '../../api/api';
import { verifyPinNamePost } from '../../api/functions';

interface LoginModel {
  name: string;
  pin: string;
}

@Component({
  selector: 'app-login-page',
  imports: [Field],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage {
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  private readonly sessionService = inject(SessionService);
  protected readonly pin = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly saving = signal<boolean>(false);
  protected readonly name = signal<string | null>(null);

  protected readonly loginModel = signal<LoginModel>({
    name: '',
    pin: ''
  });

  protected readonly loginForm = form(this.loginModel, (schemaPath) => {
    required(schemaPath.name, { message: 'Name is required' });
    required(schemaPath.pin, { message: 'Pin is required' });
    maxLength(schemaPath.pin, 6, { message: 'PIN must 6 characters' });
    minLength(schemaPath.pin, 6, { message: 'PIN must 6 characters' });
    maxLength(schemaPath.name, 100, { message: 'Name must be at most 100 characters' });
  });

  protected async onSubmit(event: Event) {
    event.preventDefault();
    this.error.set(null);
    this.saving.set(true);
    
    if (this.loginForm.name().invalid() || 
      this.loginForm.pin().invalid()) {
      this.error.set('Please correct the errors in the form.');
      return;
    }

    try {
      const name = this.loginForm.name().value().trim();
      const pin = this.loginForm.pin().value().trim();

      const response = await this.api.invoke(verifyPinNamePost, { 
        name: name,
        body: {
          pin: pin,
          wishListName: name
        } 
      });
      console.log(response);
      const role = response.role?.toLowerCase();
      if (role !== 'parent' && role !== 'child') {
        this.error.set('Invalid PIN');
        return;
      }
      this.sessionService.setSession(name, pin, role);
      await this.router.navigate([role === 'parent' ? '/parent' : '/add-item'])
    } catch (error: any) {
      this.error.set('Error saving: ' + (error.message || JSON.stringify(error)));
    } finally {
      this.saving.set(false);
    }
  }
}
