import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private hubConnection: signalR.HubConnection | null = null;
  private subscriptionUpdated$ = new Subject<any>();

  constructor(private zone: NgZone) {}

  connect() {
    if (this.hubConnection) return;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/api/hubs/subscription') // Adjust the URL to match your backend
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('SubscriptionUpdated', (data) => {
      this.zone.run(() => this.subscriptionUpdated$.next(data));
    });

    this.hubConnection.start().catch(err => console.error('SignalR connection error:', err));
  }

  disconnect() {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }

  onSubscriptionUpdated() {
    return this.subscriptionUpdated$.asObservable();
  }
} 