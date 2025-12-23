import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Field, form, maxLength, required } from '@angular/forms/signals';
import { SessionService } from '../../services/session.service';
import { Router } from '@angular/router';
import { Api } from '../../api/api';
import { giftCategoriesGet, wishlistNameItemsAddPost } from '../../api/functions';

interface ItemModel {
  itemName: string;
  category: string;
}

@Component({
  selector: 'app-add-item-page',
  imports: [Field],
  templateUrl: './add-item-page.html',
  styleUrl: './add-item-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddItemPage implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly sessionService = inject(SessionService);
  protected readonly error = signal<string | null>(null);
  protected readonly saving = signal<boolean>(false);
  protected readonly categories = signal<string[]>([]);
  protected readonly sentSuccessfully = signal<boolean>(false);

  protected readonly itemModel = signal<ItemModel>({
    itemName: '',
    category: ''
  });

  protected readonly itemForm = form(this.itemModel, (schemaPath) => {
    required(schemaPath.itemName, { message: 'Item name is required' });
    maxLength(schemaPath.itemName, 100, { message: 'Item name must be at most 100 characters' });
    required(schemaPath.category, { message: 'Category is required' });
    maxLength(schemaPath.category, 50, { message: 'Category must be at most 50 characters' });
  });

  async ngOnInit(): Promise<void> {
    if (!this.sessionService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    this.categories.set(await this.api.invoke(giftCategoriesGet));
  }

  protected async onSubmit(event: Event) {
      event.preventDefault();
      this.error.set(null);
      this.saving.set(true);
      
      if (this.itemForm.itemName().invalid() || 
        this.itemForm.category().invalid()) {
        this.error.set('Please correct the errors in the form.');
        return;
      }
  
      try {
        const itemName = this.itemForm.itemName().value().trim();
        const category = this.itemForm.category().value().trim();
        const name = this.sessionService.wishlistName();
        const pin = this.sessionService.pin();
  
        const response = await this.api.invoke(wishlistNameItemsAddPost, { 
          name: name,
          body: {
            pin: pin,
            wishListName: name,
            itemName: itemName,
            category: category
          } 
        });
      } catch (error: any) {
        this.error.set('Error saving: ' + (error.message || JSON.stringify(error)));
      } finally {
        this.saving.set(false);
        this.sentSuccessfully.set(true);
      }
    }
}
