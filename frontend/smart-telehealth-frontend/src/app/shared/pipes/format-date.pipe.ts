import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
	name: 'formatDate'
})
export class FormatDatePipe implements PipeTransform {
	transform(value: string | Date, format: 'short' | 'medium' | 'long' | 'full' = 'medium'): string {
		if (!value) return '-';

		const date = new Date(value);
		if (isNaN(date.getTime())) return '-';

		let options: Intl.DateTimeFormatOptions;
		switch (format) {
			case 'short':
				options = { month: 'short', day: 'numeric', year: 'numeric' };
				break;
			case 'medium':
				options = { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' };
				break;
			case 'long':
				options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
				break;
			case 'full':
				options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit', second: '2-digit', timeZoneName: 'short' };
				break;
			default:
				options = {};
		}

		return date.toLocaleString('en-US', options);
	}
}
