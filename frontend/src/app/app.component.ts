import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface Transaction {
  id: number;
  sourceSystem: string;
  transactionId: number;
  cardNumber?: string;
  locationCode: string;
  productName: string;
  amount: number;
  transactionTime: string;
  status: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <h1>Transactions Ingest</h1>
      <button (click)="importTransactions()" [disabled]="importing" style="margin-bottom: 16px; padding: 8px 16px;">
        {{ importing ? 'Importing...' : 'Run Import' }}
      </button>
      <div *ngIf="message" style="margin-bottom: 12px; color: green;">{{ message }}</div>

      <table border="1" cellpadding="8" cellspacing="0" style="width: 100%; border-collapse: collapse;">
        <thead>
          <tr style="background: #f0f0f0;">
            <th>Source System</th>
            <th>Transaction ID</th>
            <th>Location</th>
            <th>Product</th>
            <th>Amount</th>
            <th>Transaction Time</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let t of transactions">
            <td>{{ t.sourceSystem }}</td>
            <td>{{ t.transactionId }}</td>
            <td>{{ t.locationCode }}</td>
            <td>{{ t.productName }}</td>
            <td>{{ t.amount | number:'1.2-2' }}</td>
            <td>{{ t.transactionTime | date:'short' }}</td>
            <td>{{ t.status }}</td>
          </tr>
          <tr *ngIf="transactions.length === 0">
            <td colspan="7" style="text-align: center;">No transactions. Click Import.</td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class AppComponent implements OnInit {
  private http = inject(HttpClient);
  transactions: Transaction[] = [];
  importing = false;
  message = '';

  private api = 'http://localhost:5000/api'; // adjust if needed

  ngOnInit() {
    this.load();
  }

  load() {
    this.http.get<Transaction[]>(`${this.api}/transactions`).subscribe({
      next: data => this.transactions = data,
      error: err => console.error(err)
    });
  }

  importTransactions() {
    this.importing = true;
    this.message = '';
    this.http.post(`${this.api}/import`, {}).subscribe({
      next: () => {
        this.message = 'Import completed';
        this.load();
        this.importing = false;
      },
      error: err => {
        this.message = 'Import failed';
        this.importing = false;
        console.error(err);
      }
    });
  }
}
