const { chromium } = require('playwright');

async function capture(filteredEvents) {
    // Note: Rendering a raw HTML table string to a screenshot 
    // is easier than navigating to the page and filtering DOM elements 
    // if we just want to email the result.
    // However, if the requirement is to screenshot the *original* page, 
    // we would need to pass the page context. 
    // Given the prompt "FALLBACK: Take screenshot of filtered table",
    // we can just render the HTML we generated.

    // BUT the prompt says "Output options: PRIMARY: Send filtered table... FALLBACK: Take screenshot of filtered table".
    // It implies if email HTML support is bad, send an image of it?
    // OR "Screenshot fallback: Playwright (headless)" usually implies scraping method.

    // Let's implement a method that takes the HTML string (from formatter) 
    // and converts it to an image buffer.

    // Since I don't have the HTML string passed here in the signature (orchestrator passes events),
    // let's rely on formatter.format logic or just re-generate it.

    const formatter = require('./formatter');
    const html = formatter.format(filteredEvents);

    let browser = null;
    try {
        browser = await chromium.launch({ headless: true });
        const page = await browser.newPage();
        await page.setContent(html);

        // Adjust viewport to fit content
        const buffer = await page.screenshot({ fullPage: true });
        return buffer;
    } catch (err) {
        console.error("Screenshot capture failed:", err);
        return null; // non-fatal
    } finally {
        if (browser) await browser.close();
    }
}

module.exports = { capture };
