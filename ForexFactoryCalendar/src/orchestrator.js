const scraper = require('./scraper');
const filter = require('./filter');
const formatter = require('./formatter');
const emailer = require('./emailer');
const screenshot = require('./screenshot'); // Fallback

async function run(context) {
    const targetUrl = process.env.TARGET_URL || 'https://www.forexfactory.com/calendar/apply-settings/1?navigation=0';

    context.log(`Starting scrape for ${targetUrl}`);

    let data = [];
    let htmlContent = '';
    let screenshotBuffer = null;

    try {
        // 1. Scrape
        try {
            data = await scraper.scrape(targetUrl);
            context.log(`Scraped ${data.length} events.`);
        } catch (err) {
            context.log.error("Scraping failed (HTML parsing):", err);
            // Fallback to screenshot only mode if HTML parsing fails completely?
            // For now, rethrow or handle.
            throw err;
        }

        // 2. Filter
        const filteredEvents = filter.filterEvents(data);
        context.log(`Filtered to ${filteredEvents.length} relevant events (Today + USD).`);

        if (filteredEvents.length === 0) {
            context.log("No events found matching criteria. Skipping email.");
            return;
        }

        // 3. Format
        try {
            htmlContent = formatter.format(filteredEvents);
        } catch (err) {
            context.log.error("Formatting failed:", err);
            throw err;
        }

        // 4. Fallback/Screenshot (Optional or if requested)
        // If we want to attach a screenshot as verification or fallback
        // screenshotBuffer = await screenshot.capture(filteredEvents); 

    } catch (error) {
        context.log.error("Error in core pipeline:", error);
        // Attempt fallback screenshot of the raw page if scraping failed?
        // For now, just email the error or exit.
        return;
    }

    // 5. Send Email
    try {
        await emailer.sendEmail(htmlContent, context);
        context.log("Email sent successfully.");
    } catch (err) {
        context.log.error("Email sending failed:", err);
        throw err;
    }
}

module.exports = { run };
