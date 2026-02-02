function format(events) {
    if (!events || events.length === 0) {
        return '<p>No USD events scheduled for today.</p>';
    }

    // Basic styles to mimic a clean table
    const style = `
        <style>
            table { width: 100%; border-collapse: collapse; font-family: Arial, sans-serif; font-size: 13px; }
            th, td { padding: 8px; border-bottom: 1px solid #ddd; text-align: left; }
            th { background-color: #f4f4f4; color: #333; }
            .impact-High { color: #d32f2f; font-weight: bold; }
            .impact-Medium { color: #f57c00; }
            .impact-Low { color: #388e3c; }
            .header { background-color: #054f77; color: white; padding: 10px; margin-bottom: 20px; }
            .footer { font-size: 11px; color: #777; margin-top: 20px; border-top: 1px solid #eee; padding-top: 5px; }
        </style>
    `;

    const rows = events.map(e => `
        <tr>
            <td>${e.time}</td>
            <td>${e.currency}</td>
            <td class="impact-${e.impact}">${e.impact}</td>
            <td>${e.event}</td>
            <td>${e.actual || '-'}</td>
            <td>${e.forecast || '-'}</td>
            <td>${e.previous || '-'}</td>
        </tr>
    `).join('');

    const html = `
        <html>
        <head>${style}</head>
        <body>
            <div class="header">
                <h2>ForexFactory USD Events</h2>
                <p>${events[0].date} (IST)</p>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Time</th>
                        <th>Cur</th>
                        <th>Imp</th>
                        <th>Event</th>
                        <th>Actual</th>
                        <th>Forecast</th>
                        <th>Previous</th>
                    </tr>
                </thead>
                <tbody>
                    ${rows}
                </tbody>
            </table>
            <div class="footer">
                Generated automatically at ${new Date().toLocaleString('en-IN', { timeZone: 'Asia/Kolkata' })} IST
            </div>
        </body>
        </html>
    `;

    return html;
}

module.exports = { format };
