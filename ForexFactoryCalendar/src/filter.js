const moment = require('moment-timezone');

function filterEvents(events) {
    // Current IST Date
    const istDate = moment().tz('Asia/Kolkata');
    const todayStr = istDate.format('MMM D'); // Matches "Jan 26" format often seen
    const dayStr = istDate.format('ddd'); // "Sun"

    // ForexFactory date format usually: "Sun Jan 26"
    // We need to match this. 
    // Wait, the scraper extracts "Sun Jan 26".
    // Let's rely on loose matching or exact string construction.

    // Careful: The scraper just passed the raw string.
    // If we run this on Jan 26, we want "Jan 26".

    // Refined strategy:
    // 1. Construct target date strings
    const targetDatePart = istDate.format('MMM D'); // "Jan 26"

    return events.filter(event => {
        // 1. Filter Currency
        const isUSD = event.currency === 'USD';

        // 2. Filter Date
        // Event date is like "Sun Jan 26"
        // We check if it contains our target "Jan 26"
        // This avoids issue with "Sun" vs "Mon" if timezone diffs cause confusion, 
        // though strictly we want exact match.
        const isToday = event.date && event.date.includes(targetDatePart);

        return isUSD && isToday;
    });
}

module.exports = { filterEvents };
