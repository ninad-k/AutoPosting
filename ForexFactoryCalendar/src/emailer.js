const nodemailer = require('nodemailer');

async function sendEmail(htmlContent, context) {
    const transporter = nodemailer.createTransport({
        host: process.env.SMTP_HOST,
        port: process.env.SMTP_PORT,
        secure: process.env.SMTP_PORT == 465, // true for 465, false for other ports
        auth: {
            user: process.env.SMTP_USERNAME,
            pass: process.env.SMTP_PASSWORD,
        },
    });

    const todayStr = new Date().toLocaleDateString('en-IN', {
        timeZone: 'Asia/Kolkata',
        day: '2-digit', month: 'short', year: 'numeric'
    });

    const mailOptions = {
        from: process.env.EMAIL_FROM,
        to: process.env.EMAIL_TO, // comma separated list
        subject: `ForexFactory USD Events – ${todayStr} – IST`,
        html: htmlContent,
    };

    try {
        const info = await transporter.sendMail(mailOptions);
        context.log("Message sent: %s", info.messageId);
        return info;
    } catch (error) {
        context.log.error("Error sending email:", error);
        throw error;
    }
}

module.exports = { sendEmail };
