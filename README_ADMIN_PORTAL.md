# Admin Portal Documentation

## Analytics Dashboard

The Analytics Dashboard provides admins with real-time insights into key business metrics, including revenue, user growth, and plan distribution. It is fully responsive, accessible, and production-ready.

### Features
- **Key Metrics:** Total subscriptions, active subscriptions, total revenue, monthly recurring revenue, total users, and total providers displayed in Material cards.
- **Charts:**
  - Revenue trend (line chart)
  - User growth (line chart)
  - Plan distribution (pie chart)
- **Export/Reporting:** Download analytics as CSV or PDF for custom date ranges.
- **Accessibility:** All cards, charts, and buttons are keyboard accessible and screen reader friendly.
- **Responsive Design:** Layout adapts to all screen sizes.

### API Endpoints Used
- `GET /api/Admin/dashboard` — Key metrics
- `GET /api/Analytics/billing` — Revenue trend data
- `GET /api/Analytics/users` — User growth data
- `GET /api/Analytics/subscriptions` — Plan distribution data
- `GET /api/Analytics/reports/subscriptions` — Export analytics as CSV/PDF

### Charting Library
- Uses [ng2-charts](https://valor-software.com/ng2-charts/) and [Chart.js](https://www.chartjs.org/) for all dashboard charts.
- Charts are configured for accessibility and responsiveness.
- To customize or add new charts, update the chart data and options in `analytics-dashboard.component.ts`.

### Setup & Usage
- The dashboard is available at `/admin/analytics`.
- All data is fetched from backend analytics endpoints.
- To run locally: `ng serve` in the `healthcare-portal` directory.
- To run tests: `ng test` for unit tests, `npx cypress open` for E2E tests.

### Testing
- **Unit tests:** Cover API calls, chart data mapping, and export logic.
- **E2E tests:** Validate metrics display, chart rendering, and export functionality.

### Extensibility
- To add new metrics or charts, extend the backend analytics endpoints and update the dashboard component.
- To add new export/report types, extend the backend `/api/Analytics/reports` endpoints and update the export logic.

---

For more details on other admin features, see the relevant sections in this README. 