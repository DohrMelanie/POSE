import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { environment} from "../environments/environment"

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  url = signal(environment.apiBaseUrl);
}
