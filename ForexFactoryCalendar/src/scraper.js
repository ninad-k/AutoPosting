const axios = require('axios');
const cheerio = require('cheerio');

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

        // ForexFactory calendar structure often has date headers in rows
        // We need to keep track of the "current date" as we iterate rows
        let currentDate = null;

        const table = $('table.calendar__table');
        if (!table.length) {
            throw new Error("Calendar table not found in HTML response");
        }

        $('tr').each((i, el) => {
            const row = $(el);

            // Check if this is a date row (often has class 'calendar__row--new-day' or contains the date)
            // The structure is tricky. Usually:
            // <tr class="calendar__row--new-day"> <td class="date">...</td> ... </tr>
            // <tr class="calendar__row"> <td class="date"></td> ... </tr> (empty date cell implies same day)

            const dateCell = row.find('.calendar__date');
            if (dateCell.length > 0) {
                const dateText = dateCell.text().trim();
                if (dateText) {
                    currentDate = dateText; // Update current date context
                    // Note: Date text format is usually "Sun Jan 26"
                }
            }

            // We only care about actual event rows
            if (row.hasClass('calendar__row')) { // Ensure it's a data row
                const currency = row.find('.calendar__currency').text().trim();
                const time = row.find('.calendar__time').text().trim();
                const impactSpan = row.find('.calendar__impact span');
                const impact = impactSpan.attr('title') || 'Low'; // title often has "High Impact"
                const eventName = row.find('.calendar__event').text().trim();
                const actual = row.find('.calendar__actual').text().trim();
                const forecast = row.find('.calendar__forecast').text().trim();
                const previous = row.find('.calendar__previous').text().trim();

                // Simplify impact to manageable string
                let impactLevel = 'Low';
                if (impactSpan.hasClass('icon--impact-red') || impact.toLowerCase().includes('high')) impactLevel = 'High';
                else if (impactSpan.hasClass('icon--impact-orange') || impact.toLowerCase().includes('medium')) impactLevel = 'Medium';

                if (currency && eventName) {
                    events.push({
                        date: currentDate, // This might need normalization later (e.g. adding Year)
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
        console.error("Scraping error:", error.message);
        throw error;
    }
}

module.exports = { scrape };
