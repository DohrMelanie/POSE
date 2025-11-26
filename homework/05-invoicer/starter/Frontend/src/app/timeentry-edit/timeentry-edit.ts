import { Component, inject, input, OnInit, signal } from '@angular/core';
import { Employee, Project, TimeEntryDto, TimeEntryUpdateDto } from '../api/models';
import { getEmployees, getProjects, getTimeEntries, getTimeEntry, updateTimeEntry } from '../api/functions';
import { Router, ActivatedRoute } from '@angular/router';
import { environment } from '../../environments/environment.development';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-timeentry-edit',
  imports: [FormsModule],
  templateUrl: './timeentry-edit.html',
  styleUrl: './timeentry-edit.css',
})
export class TimeentryEdit implements OnInit{
  protected readonly employees = signal<Employee[]>([]);
  protected readonly projects = signal<Project[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly saving = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  protected readonly date = signal<string>('');
  protected readonly startTime = signal<string>('');
  protected readonly endTime = signal<string>('');
  protected readonly description = signal<string>('');
  protected readonly employeeId = signal<number | null>(null);
  protected readonly projectId = signal<number | null>(null);

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  public timeEntryId = input.required<number>()

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadData();
  }

  private async loadData() {
    try {
      const ts = await this.api.invoke(getTimeEntry, { id: this.timeEntryId() });

      const [timeEntry, employees, projects] = await Promise.all([
        this.api.invoke(getTimeEntry, { id: this.timeEntryId() }),
        this.api.invoke(getEmployees, {}),
        this.api.invoke(getProjects, {})
      ]);

      this.employees.set(employees.filter(e => e !== null) as Employee[]);
      this.projects.set(projects.filter(p => p !== null) as Project[]);

      if (timeEntry) {
        this.date.set(timeEntry.date || '');
        this.startTime.set(timeEntry.startTime || '');
        this.endTime.set(timeEntry.endTime || '');
        this.description.set(timeEntry.description || '');
        this.employeeId.set(timeEntry.employeeId || null);
        this.projectId.set(timeEntry.projectId || null);
      } else {
        this.error.set('Time entry not found');
      }
    } catch (err) {
      this.error.set('Failed to load data');
      console.error(err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSubmit() {
    if (!this.timeEntryId() || !this.employeeId() || !this.projectId()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    try {
      const updateDto: TimeEntryUpdateDto = {
        date: this.date(),
        startTime: this.startTime(),
        endTime: this.endTime(),
        description: this.description(),
        employeeId: this.employeeId()!,
        projectId: this.projectId()!
      };

      await this.api.invoke(updateTimeEntry, {
        id: this.timeEntryId(),
        body: updateDto
      });

      this.router.navigate(['/timesheet']);
    } catch (err: any) {
      this.error.set(err.error || 'Failed to save time entry');
      console.error(err);
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel() {
    this.router.navigate(['/timeentries']);
  }
}
