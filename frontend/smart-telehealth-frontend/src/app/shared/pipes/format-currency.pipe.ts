import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatCurrency'
})
export class FormatCurrencyPipe implements PipeTransform {
  transform(
    value: number, 
    currency: string = 'USD', 
    locale: string = 'en-US',
    display: 'symbol' | 'code' | 'name' = 'symbol'
  ): string {
    if (value === null || value === undefined || isNaN(value)) return '$0.00';

    const options: Intl.NumberFormatOptions = {
      style: 'currency',
      currency: currency,
      currencyDisplay: display,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    };

    return new Intl.NumberFormat(locale, options).format(value);
  }
}
