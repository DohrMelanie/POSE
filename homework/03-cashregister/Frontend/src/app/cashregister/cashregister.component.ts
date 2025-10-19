import { Component, signal, effect, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CashregisterService, Item, Receipt, ReceiptLine } from '../cashregister.service';

@Component({
  selector: 'app-cashregister',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cashregister.component.html',
  styleUrls: ['./cashregister.component.css']
})
export class CashregisterComponent {
  private service = inject(CashregisterService);

  items = signal<Item[]>([]);
  receiptLines = signal<ReceiptLine[]>([]);
  total = computed(() => {
    return this.receiptLines().reduce((sum, line) => sum + (line.totalPrice ?? 0), 0);
  });
  loading = signal(false);
  error = signal<string | null>(null);

  constructor() {
    this.loadItems();
  }

  async loadItems() {
    this.loading.set(true);
    this.error.set(null);
    try {
      const data = await this.service.getItems();
      this.items.set(data);
    } catch (e: any) {
      this.error.set((e && e.message) || 'Failed to load items');
    } finally {
      this.loading.set(false);
    }
  }

  addItem(item: Item) {
    // find existing line
    const lines = [...this.receiptLines()];
    const idx = lines.findIndex(l => l.itemId === item.id);
    if (idx >= 0) {
      const existing = { ...lines[idx] };
      existing.quantity += 1;
      existing.totalPrice = Number((existing.quantity * item.price).toFixed(2));
      lines[idx] = existing;
    } else {
      lines.push({ itemId: item.id, item, quantity: 1, totalPrice: Number(item.price.toFixed(2)) });
    }
    this.receiptLines.set(lines);
  }

  removeLine(line: ReceiptLine) {
    const lines = this.receiptLines().filter(l => l !== line);
    this.receiptLines.set(lines);
  }

  trackItem(_: number, item: Item) {
    return item.id;
  }

  trackLine(_: number, line: ReceiptLine) {
    return line.id ?? `line-${line.itemId}`;
  }

  async checkout() {
    const receipt: Receipt = {
      receiptLines: this.receiptLines(),
      total: this.total()
    };

    this.loading.set(true);
    this.error.set(null);
    try {
      await this.service.checkout(receipt);
      this.receiptLines.set([]);
    } catch (e: any) {
      this.error.set((e && e.message) || 'Checkout failed');
    } finally {
      this.loading.set(false);
    }
  }
}
