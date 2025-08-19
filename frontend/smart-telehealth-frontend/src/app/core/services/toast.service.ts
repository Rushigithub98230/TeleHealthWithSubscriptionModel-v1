import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class ToastService {
	constructor(private snackBar: MatSnackBar) {}

	private open(message: string, panelClass: string[], duration = 5000): void {
		const config: MatSnackBarConfig = {
			duration,
			horizontalPosition: 'end',
			verticalPosition: 'top',
			panelClass
		};
		this.snackBar.open(message, 'Close', config);
	}

	showSuccess(message: string, duration = 5000): void {
		this.open(message, ['success-snackbar'], duration);
	}

	showError(message: string, duration = 7000): void {
		this.open(message, ['error-snackbar'], duration);
	}

	showWarning(message: string, duration = 6000): void {
		this.open(message, ['warning-snackbar'], duration);
	}

	showInfo(message: string, duration = 5000): void {
		this.open(message, ['info-snackbar'], duration);
	}
}
