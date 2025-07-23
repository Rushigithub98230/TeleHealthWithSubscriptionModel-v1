import { TestBed } from '@angular/core/testing';

import { ProviderApiService } from './provider-api.service';

describe('ProviderApiService', () => {
  let service: ProviderApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProviderApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
