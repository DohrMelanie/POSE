import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { BASE_URL } from './app.config';

export interface Item {
  id: number;
  name: string;
  price: number;
  amount?: number | null;
  amountName?: string | null;
}

export interface ReceiptLine {
  id?: number;
  itemId: number;
  item?: Item;
  quantity: number;
  totalPrice: number;
}

export interface Receipt {
  id?: number;
  receiptLines: ReceiptLine[];
  total: number;
}

@Injectable({ providedIn: 'root' })
export class CashregisterService {
  private http = inject(HttpClient);
  private base = inject(BASE_URL);

  getItems(): Promise<Item[]> {
    return firstValueFrom(this.http.get<Item[]>(`${this.base}/items`));
  }

  checkout(receipt: Receipt): Promise<any> {
    return firstValueFrom(this.http.put(`${this.base}/checkout`, receipt.receiptLines));
  }
}
