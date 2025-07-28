import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';

import { VideoCallService, VideoSession, VideoCallState } from '../../../core/services/video-call.service';

declare var OT: any; // OpenTok global variable

@Component({
  selector: 'app-video-call',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './video-call.component.html',
  styleUrl: './video-call.component.scss'
})
export class VideoCallComponent implements OnInit, OnDestroy {
  @Input() sessionId?: string;
  @Input() consultationId?: string;
  @Input() userRole: 'patient' | 'provider' = 'patient';
  @Output() callEnded = new EventEmitter<void>();
  @Output() callStarted = new EventEmitter<VideoSession>();

  // OpenTok variables
  private session: any;
  private publisher: any;
  private subscribers: any[] = [];
  private recordingId?: string;
  private broadcastId?: string;

  // Component state
  videoCallState: VideoCallState = {
    isInCall: false,
    isMuted: false,
    isVideoEnabled: true,
    isScreenSharing: false,
    isRecording: false
  };

  loading = false;
  error = '';
  participants: any[] = [];
  connectionQuality = 'good';
  callDuration = 0;
  private callTimer?: any;

  constructor(
    private videoCallService: VideoCallService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.subscribeToVideoCallState();
    if (this.sessionId) {
      this.joinExistingSession();
    } else if (this.consultationId) {
      this.createNewSession();
    }
  }

  ngOnDestroy(): void {
    this.cleanup();
  }

  private subscribeToVideoCallState(): void {
    this.videoCallService.getVideoCallState().subscribe(state => {
      this.videoCallState = state;
    });
  }

  private async createNewSession(): Promise<void> {
    if (!this.consultationId) return;

    this.loading = true;
    this.error = '';

    try {
      const createDto = {
        sessionName: `Consultation-${this.consultationId}`,
        isArchived: true,
        consultationId: this.consultationId
      };

      const response: any = await this.videoCallService.createSession(createDto).toPromise();
      
      if (response.success && response.data) {
        this.sessionId = response.data.sessionId;
        await this.joinSession(response.data);
      } else {
        throw new Error(response.message || 'Failed to create session');
      }
    } catch (error: any) {
      this.error = error.message || 'Failed to create video session';
      this.snackBar.open(this.error, 'Close', { duration: 5000 });
    } finally {
      this.loading = false;
    }
  }

  async joinExistingSession(): Promise<void> {
    if (!this.sessionId) return;

    this.loading = true;
    this.error = '';

    try {
      const response: any = await this.videoCallService.getSession(this.sessionId).toPromise();
      
      if (response.success && response.data) {
        await this.joinSession(response.data);
      } else {
        throw new Error(response.message || 'Failed to get session');
      }
    } catch (error: any) {
      this.error = error.message || 'Failed to join session';
      this.snackBar.open(this.error, 'Close', { duration: 5000 });
    } finally {
      this.loading = false;
    }
  }

  private async joinSession(sessionData: VideoSession): Promise<void> {
    try {
      // Initialize OpenTok session
      this.session = OT.initSession(sessionData.apiKey, sessionData.sessionId);
      
      // Set up session event handlers
      this.setupSessionEventHandlers();
      
      // Generate token for this user
      const role = this.userRole === 'provider' ? 'publisher' : 'subscriber';
      const tokenResponse: any = await this.videoCallService.generateToken(sessionData.sessionId, { role }).toPromise();
      
      if (tokenResponse.success && tokenResponse.data) {
        // Connect to session
        this.session.connect(tokenResponse.data, (error: any) => {
          if (error) {
            this.error = 'Failed to connect to video session';
            this.snackBar.open(this.error, 'Close', { duration: 5000 });
          } else {
            this.onSessionConnected(sessionData);
          }
        });
      } else {
        throw new Error(tokenResponse.message || 'Failed to generate token');
      }
    } catch (error: any) {
      this.error = error.message || 'Failed to join session';
      this.snackBar.open(this.error, 'Close', { duration: 5000 });
    }
  }

  private setupSessionEventHandlers(): void {
    // Stream created event
    this.session.on('streamCreated', (event: any) => {
      this.subscribers.push(this.session.subscribe(event.stream, 'subscribers', {
        insertMode: 'append',
        width: '100%',
        height: '100%'
      }));
      this.updateParticipants();
    });

    // Stream destroyed event
    this.session.on('streamDestroyed', (event: any) => {
      const index = this.subscribers.findIndex(sub => sub.stream === event.stream);
      if (index > -1) {
        this.subscribers.splice(index, 1);
      }
      this.updateParticipants();
    });

    // Session disconnected event
    this.session.on('sessionDisconnected', (event: any) => {
      this.endCall();
    });

    // Connection quality event
    this.session.on('connectionQualityChanged', (event: any) => {
      this.connectionQuality = event.quality;
    });
  }

  private onSessionConnected(sessionData: VideoSession): void {
    this.videoCallService.updateVideoCallState({
      isInCall: true,
      sessionId: sessionData.sessionId,
      apiKey: sessionData.apiKey
    });

    // Initialize publisher
    this.publisher = OT.initPublisher('publisher', {
      insertMode: 'append',
      width: '100%',
      height: '100%'
    });

    // Publish stream
    this.session.publish(this.publisher, (error: any) => {
      if (error) {
        this.error = 'Failed to publish stream';
        this.snackBar.open(this.error, 'Close', { duration: 5000 });
      }
    });

    this.callStarted.emit(sessionData);
    this.startCallTimer();
    this.snackBar.open('Connected to video call', 'Close', { duration: 3000 });
  }

  private updateParticipants(): void {
    this.participants = [
      { id: 'publisher', name: 'You', isPublisher: true },
      ...this.subscribers.map((sub, index) => ({
        id: sub.stream.id,
        name: `Participant ${index + 1}`,
        isPublisher: false
      }))
    ];
  }

  private startCallTimer(): void {
    this.callTimer = setInterval(() => {
      this.callDuration++;
    }, 1000);
  }

  private stopCallTimer(): void {
    if (this.callTimer) {
      clearInterval(this.callTimer);
      this.callTimer = undefined;
    }
  }

  // Public methods for UI interactions
  toggleMute(): void {
    if (this.publisher) {
      this.publisher.publishAudio(!this.videoCallState.isMuted);
      this.videoCallService.toggleMute();
    }
  }

  toggleVideo(): void {
    if (this.publisher) {
      this.publisher.publishVideo(!this.videoCallState.isVideoEnabled);
      this.videoCallService.toggleVideo();
    }
  }

  toggleScreenSharing(): void {
    // Implementation for screen sharing
    this.videoCallService.toggleScreenSharing();
    this.snackBar.open('Screen sharing feature coming soon', 'Close', { duration: 3000 });
  }

  async toggleRecording(): Promise<void> {
    if (!this.sessionId) return;

    try {
      if (this.videoCallState.isRecording) {
        // Stop recording
        if (this.recordingId) {
          await this.videoCallService.stopRecording(this.recordingId).toPromise();
          this.recordingId = undefined;
        }
      } else {
        // Start recording
        const recordingOptions = {
          name: `Consultation-${this.consultationId || 'session'}`,
          hasAudio: true,
          hasVideo: true,
          outputMode: 'composed',
          resolution: '1280x720'
        };

        const response: any = await this.videoCallService.startRecording(this.sessionId, recordingOptions).toPromise();
        if (response.success && response.data) {
          this.recordingId = response.data.recordingId;
        }
      }

      this.videoCallService.toggleRecording();
    } catch (error: any) {
      this.snackBar.open('Failed to toggle recording', 'Close', { duration: 3000 });
    }
  }

  endCall(): void {
    this.cleanup();
    this.callEnded.emit();
    this.snackBar.open('Call ended', 'Close', { duration: 3000 });
  }

  private cleanup(): void {
    this.stopCallTimer();
    
    if (this.session) {
      this.session.disconnect();
    }

    if (this.publisher) {
      this.publisher.destroy();
    }

    this.subscribers.forEach(sub => sub.destroy());
    this.subscribers = [];

    this.videoCallService.leaveCall();
  }

  getCallDuration(): string {
    const minutes = Math.floor(this.callDuration / 60);
    const seconds = this.callDuration % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  getConnectionQualityColor(): string {
    switch (this.connectionQuality) {
      case 'excellent': return 'green';
      case 'good': return 'yellow';
      case 'fair': return 'orange';
      case 'poor': return 'red';
      default: return 'gray';
    }
  }
} 