import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ApiConfiguration } from '../api/api-configuration';
import { Api } from '../api/api';
import { CompetitionDto, EvalReqDto, EvaluationDto, ParticipantDto } from '../api/models';
import { computeEvaluation, getCompetitions, getParticipants } from '../api/functions';

@Component({
  selector: 'app-lauf-auswertung',
  standalone: true,
  imports: [DecimalPipe, FormsModule],
  templateUrl: './lauf-auswertung.html',
  styleUrl: './lauf-auswertung.css',
})
export class LaufAuswertung implements OnInit{
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly api = inject(Api);

  protected readonly competitions = signal<CompetitionDto[]>([]);
  protected readonly participants = signal<ParticipantDto[]>([]);
  protected readonly comp = signal<CompetitionDto | null>(null);
  protected readonly participant = signal<ParticipantDto | null>(null);
  protected readonly calculating = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly evaluation = signal<EvaluationDto | null>(null);

  async ngOnInit() {
    this.competitions.set(await this.api.invoke(getCompetitions, {}));
  }

  async onCompChange(event: Event) {
    let value = (event.target as HTMLSelectElement).value;
    this.comp.set(this.competitions().find(c => c.id == parseInt(value))!);
    this.participants.set(await this.api.invoke(getParticipants, {id: this.comp()!.id}))
  }

  async onParticipantChange(event: Event) {
    let value = (event.target as HTMLSelectElement).value;
    this.participant.set(this.participants().find(c => c.id == parseInt(value))!);
  }

  async calculate() {
    this.calculating.set(true);
    try {
      const req: EvalReqDto = {
        compId: this.comp()!.id,
        participantId: this.participant()!.id
      }

      this.evaluation.set(await this.api.invoke(computeEvaluation, { body: req}));
    } catch(e: any) {
      this.error.set(e.error);
    }
    this.calculating.set(false);
  }

  getTime(seconds: number) {
    let minutes = Math.floor(seconds / 60);
    seconds = seconds % 60;

    if (minutes > 60) {
      let hours = Math.floor(minutes / 60);
      minutes = minutes % 60;
      return hours.toString().padStart(2, '0') + ":" + minutes.toString().padStart(2, '0') + ":" + seconds.toString().padStart(2, '0'); 
    }
    return minutes.toString().padStart(2, '0') + ":" + seconds.toString().padStart(2, '0'); 
  }
}
