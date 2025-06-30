import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { Subscription } from 'rxjs';
import { ReportService } from '../report.service';

export interface ErrorInfo {
  errorType: 1 | 2;
  errorLog: string;
  errorTime: string;
  workstationId: string;
  userName: string;
  ipAddress: string;
  msgNumber: string;
  desc: string;
  errorSource: string;
  dllName: string;
  version: string;
  routine: string;
  lineNumber: string;
}

@Component({
  selector: 'app-alert-prompt',
  templateUrl: './alert-prompt.component.html',
  styleUrls: ['./alert-prompt.component.css'],
  imports: [CommonModule],
})
export class AlertPromptComponent {
  visible = false;
  messageType: 'alert' | 'confirm' | 'error' = 'alert';
  private cachedUserName: string | null = null;

  messageText = '';
  okButtonText = 'OK';
  cancelButtonText = 'Cancel';
  errorMessage: ErrorInfo | null = null;

  @Output() onOK = new EventEmitter<void>();
  @Output() onCancel = new EventEmitter<void>();
  busy: Subscription | null = null;

  constructor(private reportService: ReportService) {}
  showAlert(message: string, okButtonText = 'OK') {
    this.messageType = 'alert';
    this.messageText = message;
    this.okButtonText = okButtonText;
    this.visible = true;
  }

  showConfirm(message: string, okButtonText = 'Yes', cancelButtonText = 'No') {
    this.messageType = 'confirm';
    this.messageText = message;
    this.okButtonText = okButtonText;
    this.cancelButtonText = cancelButtonText;
    this.visible = true;
  }

  showError(error: ErrorInfo) {
    this.messageType = 'error';
    this.errorMessage = error;
    this.visible = true;
    if (this.errorMessage && this.errorMessage.errorType === 1) {
      if (this.cachedUserName !== null) {
        this.errorMessage.userName = this.cachedUserName;
      } else {
        this.busy = this.reportService.getUserName().subscribe((data) => {
          this.cachedUserName = data ?? '';
          if (this.errorMessage) {
            this.errorMessage.userName = this.cachedUserName ?? '';
          }
        });
      }
    }
  }

  confirmOK() {
    this.visible = false;
    this.onOK.emit();
  }

  confirmCancel() {
    this.visible = false;
    this.onCancel.emit();
  }
}
