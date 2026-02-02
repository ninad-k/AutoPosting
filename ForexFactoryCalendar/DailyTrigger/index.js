const orchestrator = require('../src/orchestrator');

module.exports = async function (context, myTimer) {
    const timeStamp = new Date().toISOString();
    
    if (myTimer.isPastDue) {
        context.log('JavaScript timer trigger function is running late!');
    }
    
    context.log('ForexFactory Daily Trigger fired at:', timeStamp);
    
    try {
        await orchestrator.run(context);
    } catch (error) {
        context.log.error("Orchestrator failed:", error);
        // Might want to send an error email here if critical
    }
    
    context.log('ForexFactory Daily Trigger completed.');
};
