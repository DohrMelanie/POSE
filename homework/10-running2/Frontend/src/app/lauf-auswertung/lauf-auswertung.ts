import { ComputeReqDto } from './../api/models/compute-req-dto';
import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ApiConfiguration } from '../api/api-configuration';
import { Api } from '../api/api';
import { CompetitionDto, EvaluationDto, ParticipantDto } from '../api/models';
import { computeEvaluation, getCompetitions, getParticipants } from '../api/functions';
import { DatePipe } from '@angular/common';
@Component({
  selector: 'app-lauf-auswertung',
  standalone: true,
  imports: [DecimalPipe, FormsModule, DatePipe],
  templateUrl: './lauf-auswertung.html',
  styleUrl: './lauf-auswertung.css',
})
export class LaufAuswertung implements OnInit{
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly api = inject(Api);
  protected readonly competitions = signal<CompetitionDto[]>([]);
  protected readonly participants = signal<ParticipantDto[]>([]);
  protected readonly competitionId = signal<number | null>(null);
  protected readonly participantId = signal<number | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly evaluation = signal<EvaluationDto | null>(null);
  protected readonly error = signal<string | null>(null);

  async ngOnInit() {
    this.competitions.set(await this.api.invoke(getCompetitions, {}));
  }

  async onCompChange() {
    this.participants.set(await this.api.invoke(getParticipants, {
      compId: this.competitionId()!
    }));
  }

  async calculateEvaluation() {
    console.log(this.participantId());
    const dto: ComputeReqDto = {
        participantId: this.participantId()!
    }
    try {
      this.evaluation.set(await this.api.invoke(computeEvaluation, { 
        body: dto
    }));
    } catch(err: any) {
      this.error.set(err.error);
    }
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
