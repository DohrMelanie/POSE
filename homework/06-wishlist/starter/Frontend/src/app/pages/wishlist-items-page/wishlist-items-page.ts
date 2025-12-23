import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { WishlistItemResp } from '../../api/models';
import { wishlistNameItemsItemIdDelete, wishlistNameItemsItemIdMarkAsBoughtPost, wishlistNameItemsPost } from '../../api/functions';
import { SessionService } from '../../services/session.service';
import { Router } from '@angular/router';
import { Api } from '../../api/api';

@Component({
  selector: 'app-wishlist-items-page',
  imports: [],
  templateUrl: './wishlist-items-page.html',
  styleUrl: './wishlist-items-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WishlistItemsPage implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly sessionService = inject(SessionService);

  protected readonly items = signal<WishlistItemResp[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly saving = signal<boolean>(false);

  ngOnInit(): void {
    this.loadItems();
  }

  private async loadItems() {
    this.loading.set(true);
    this.error.set(null);
    
    const name = this.sessionService.wishlistName();
    const pin = this.sessionService.pin();
    try {
      const response = await this.api.invoke(wishlistNameItemsPost, { 
        name: name,
        body: {
          wishListName: name,
          pin: pin
        }
      });
      this.items.set(response);
    } catch (error: any) {
      this.error.set('Error saving: ' + (error.message || JSON.stringify(error)));
    } finally {
      this.loading.set(false);
    }
  }
  protected async changeStatus(item: WishlistItemResp) {
    try {
      this.saving.set(true);
      this.error.set(null);
      const response = await this.api.invoke(wishlistNameItemsItemIdMarkAsBoughtPost, {
        name: this.sessionService.wishlistName(),
        itemId: item.id!,
        body: {
          wishListName: this.sessionService.wishlistName(),
          pin: this.sessionService.pin(),
        }
      });
    } catch (error: any) {
      this.error.set('Error saving: ' + (error.message || JSON.stringify(error)));
    } finally {
      this.saving.set(false);
      await this.loadItems();
    }
  }

  protected async deleteItem(item: WishlistItemResp) { 
    try {
      this.saving.set(true);
      this.error.set(null);
      const response = await this.api.invoke(wishlistNameItemsItemIdDelete, {
        name: this.sessionService.wishlistName(),
        itemId: item.id!,
        body: {
          wishListName: this.sessionService.wishlistName(),
          pin: this.sessionService.pin(),
        }
      });
    } catch (error: any) {
      this.error.set('Error saving: ' + (error.message || JSON.stringify(error)));
    } finally {
      this.saving.set(false);
      await this.loadItems();
    }
  }
}
