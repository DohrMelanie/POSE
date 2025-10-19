import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { environment} from "../environments/environment.development"

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('CashRegister');
  url = signal(environment.apiBaseUrl);
}
