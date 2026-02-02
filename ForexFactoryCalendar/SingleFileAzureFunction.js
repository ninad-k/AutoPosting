/**
 * Azure Function: ForexFactory USD Event Emailer
 * Trigger: Timer (Daily at 2:00 PM IST)
 * 
 * REQUIRED DEPENDENCIES (package.json):
 * {
 *   "axios": "^1.6.0",
 *   "cheerio": "^1.0.0-rc.12",
 *   "moment-timezone": "^0.5.43",
 *   "nodemailer": "^6.9.7"
 * }
 */

const axios = require('axios');
const cheerio = require('cheerio');
const moment = require('moment-timezone');
const nodemailer = require('nodemailer');

// --- MODULE: SCRAPER ---
async function scrape(url) {
    try {
        const response = await axios.get(url, {
            headers: {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
                'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
                'Accept-Language': 'en-US,en;q=0.5'
            }
        });

        const $ = cheerio.load(response.data);
        const events = [];
        let currentDate = null;

        const table = $('table.calendar__table');
        if (!table.length) throw new Error("Calendar table not found");

        $('tr').each((i, el) => {
            const row = $(el);
            const dateCell = row.find('.calendar__date');
            if (dateCell.length > 0) {
                const dateText = dateCell.text().trim();
                if (dateText) currentDate = dateText;
            }

            if (row.hasClass('calendar__row')) {
                const currency = row.find('.calendar__currency').text().trim();
                const time = row.find('.calendar__time').text().trim();
                const impactSpan = row.find('.calendar__impact span');
                const impactRaw = impactSpan.attr('title') || 'Low';
                const eventName = row.find('.calendar__event').text().trim();
                const actual = row.find('.calendar__actual').text().trim();
                const forecast = row.find('.calendar__forecast').text().trim();
                const previous = row.find('.calendar__previous').text().trim();

                let impactLevel = 'Low';
                if (impactSpan.hasClass('icon--impact-red') || impactRaw.toLowerCase().includes('high')) impactLevel = 'High';
                else if (impactSpan.hasClass('icon--impact-orange') || impactRaw.toLowerCase().includes('medium')) impactLevel = 'Medium';

                if (currency && eventName) {
                    events.push({
                        date: currentDate,
                        time,
                        currency,
                        impact: impactLevel,
                        event: eventName,
                        actual,
                        forecast,
                        previous
                    });
                }
            }
        });
        return events;
    } catch (error) {
        throw error;
    }
}

// --- MODULE: FILTER ---
function filterEvents(events) {
    const istDate = moment().tz('Asia/Kolkata');
    const targetDatePart = istDate.format('MMM D'); // e.g., "Jan 27"

    return events.filter(event => {
        const isUSD = event.currency === 'USD';
        const isToday = event.date && event.date.includes(targetDatePart);
        return isUSD && isToday;
    });
}

// --- MODULE: FORMATTER ---
function format(events) {
    if (!events || events.length === 0) return '<p>No USD events scheduled for today.</p>';

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

    return `
        <html>
        <head>${style}</head>
        <body>
            <div class="header">
                <h2>ForexFactory USD Events</h2>
                <p>${events[0].date} (IST)</p>
            </div>
            <table>
                <thead>
                    <tr><th>Time</th><th>Cur</th><th>Imp</th><th>Event</th><th>Actual</th><th>Forecast</th><th>Previous</th></tr>
                </thead>
                <tbody>${rows}</tbody>
            </table>
            <div class="footer">Generated automatically at ${new Date().toLocaleString('en-IN', { timeZone: 'Asia/Kolkata' })} IST</div>
        </body>
        </html>
    `;
}

// --- MODULE: EMAILER ---
async function sendEmail(htmlContent, context) {
    const transporter = nodemailer.createTransport({
        host: process.env.SMTP_HOST,
        port: process.env.SMTP_PORT,
        secure: process.env.SMTP_PORT == 465,
        auth: {
            user: process.env.SMTP_USERNAME,
            pass: process.env.SMTP_PASSWORD,
        },
    });

    const todayStr = new Date().toLocaleDateString('en-IN', {
        timeZone: 'Asia/Kolkata',
        day: '2-digit', month: 'short', year: 'numeric'
    });

    await transporter.sendMail({
        from: process.env.EMAIL_FROM,
        to: process.env.EMAIL_TO,
        subject: `ForexFactory USD Events – ${todayStr} – IST`,
        html: htmlContent,
    });
}

// --- AZURE FUNCTION ENTRY POINT ---
module.exports = async function (context, myTimer) {
    const targetUrl = process.env.TARGET_URL || 'https://www.forexfactory.com/calendar/apply-settings/1?navigation=0';
    context.log(`Running ForexFactory Scraper at: ${new Date().toISOString()}`);

    try {
        // 1. Scrape
        const data = await scrape(targetUrl);
        context.log(`Scraped ${data.length} total events.`);

        // 2. Filter
        const filteredEvents = filterEvents(data);
        context.log(`Filtered to ${filteredEvents.length} USD events.`);

        if (filteredEvents.length === 0) {
            context.log("No events found. Skipping email.");
            return;
        }

        // 3. Format
        const htmlContent = format(filteredEvents);

        // 4. Send
        await sendEmail(htmlContent, context);
        context.log("Email sent successfully.");

    } catch (error) {
        context.log.error("Execution failed:", error);
    }
};
