import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface VideoSession {
  sessionId: string;
  apiKey: string;
  token: string;
  sessionName: string;
  isArchived: boolean;
  createdAt: string;
  status: string;
  participantCount: number;
  streamCount: number;
}

export interface CreateVideoSessionDto {
  sessionName: string;
  isArchived: boolean;
  consultationId?: string;
}

export interface GenerateTokenDto {
  role: 'publisher' | 'subscriber' | 'moderator';
  expireTime?: string;
}

export interface VideoCallState {
  isInCall: boolean;
  isMuted: boolean;
  isVideoEnabled: boolean;
  isScreenSharing: boolean;
  isRecording: boolean;
  sessionId?: string;
  token?: string;
  apiKey?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VideoCallService {
  private videoCallState$ = new BehaviorSubject<VideoCallState>({
    isInCall: false,
    isMuted: false,
    isVideoEnabled: true,
    isScreenSharing: false,
    isRecording: false
  });

  constructor(private http: HttpClient) {}

  // Create a new video session
  createSession(createDto: CreateVideoSessionDto): Observable<any> {
    return this.http.post(`${environment.apiUrl}/videocall/sessions`, createDto);
  }

  // Generate token for joining session
  generateToken(sessionId: string, generateDto: GenerateTokenDto): Observable<any> {
    return this.http.post(`${environment.apiUrl}/videocall/sessions/${sessionId}/token`, generateDto);
  }

  // Get session information
  getSession(sessionId: string): Observable<any> {
    return this.http.get(`${environment.apiUrl}/videocall/sessions/${sessionId}`);
  }

  // Start recording
  startRecording(sessionId: string, recordingOptions: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}/videocall/sessions/${sessionId}/recordings`, recordingOptions);
  }

  // Stop recording
  stopRecording(recordingId: string): Observable<any> {
    return this.http.post(`${environment.apiUrl}/videocall/recordings/${recordingId}/stop`, {});
  }

  // Get session recordings
  getSessionRecordings(sessionId: string): Observable<any> {
    return this.http.get(`${environment.apiUrl}/videocall/sessions/${sessionId}/recordings`);
  }

  // Start broadcast
  startBroadcast(sessionId: string, broadcastOptions: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}/videocall/sessions/${sessionId}/broadcasts`, broadcastOptions);
  }

  // Stop broadcast
  stopBroadcast(broadcastId: string): Observable<any> {
    return this.http.post(`${environment.apiUrl}/videocall/broadcasts/${broadcastId}/stop`, {});
  }

  // Get session analytics
  getSessionAnalytics(sessionId: string): Observable<any> {
    return this.http.get(`${environment.apiUrl}/videocall/sessions/${sessionId}/analytics`);
  }

  // Health check
  getHealth(): Observable<any> {
    return this.http.get(`${environment.apiUrl}/videocall/health`);
  }

  // State management
  getVideoCallState(): Observable<VideoCallState> {
    return this.videoCallState$.asObservable();
  }

  updateVideoCallState(updates: Partial<VideoCallState>): void {
    const currentState = this.videoCallState$.value;
    this.videoCallState$.next({ ...currentState, ...updates });
  }

  // Helper methods for common operations
  joinCall(sessionId: string, role: 'publisher' | 'subscriber' = 'publisher'): Observable<any> {
    const generateDto: GenerateTokenDto = { role };
    return this.generateToken(sessionId, generateDto);
  }

  leaveCall(): void {
    this.updateVideoCallState({
      isInCall: false,
      isMuted: false,
      isVideoEnabled: true,
      isScreenSharing: false,
      isRecording: false,
      sessionId: undefined,
      token: undefined,
      apiKey: undefined
    });
  }

  toggleMute(): void {
    const currentState = this.videoCallState$.value;
    this.updateVideoCallState({ isMuted: !currentState.isMuted });
  }

  toggleVideo(): void {
    const currentState = this.videoCallState$.value;
    this.updateVideoCallState({ isVideoEnabled: !currentState.isVideoEnabled });
  }

  toggleScreenSharing(): void {
    const currentState = this.videoCallState$.value;
    this.updateVideoCallState({ isScreenSharing: !currentState.isScreenSharing });
  }

  toggleRecording(): void {
    const currentState = this.videoCallState$.value;
    this.updateVideoCallState({ isRecording: !currentState.isRecording });
  }
} 