import { NgxStripeModule } from 'ngx-stripe';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { PaymentFormComponent } from './payment-form/payment-form.component';

@NgModule({
  declarations: [
    AppComponent,
    PaymentFormComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    NgxStripeModule.forRoot('pk_test_51RbfqBCI7YurXiFNHk4WcajFzdxGJCxD32qJbtcQCTSaVU5qbpHZZR2D4iujZeh3bcGZEEtCetI94SadTICFXjFG005IoPIAYC'),
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { } 