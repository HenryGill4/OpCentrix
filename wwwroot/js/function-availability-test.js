// ?? **JavaScript Function Availability Test Script**
// Run this script in browser console on each modal page to verify functions are available

console.log('?? Testing JavaScript Function Availability...\n');

// Define test suites for each modal type
const testSuites = {
    'Scheduler Job Modal': [
        'updateJobFromPart',
        'filterPartsByMachine',
        'updateEndTimeFromStart', 
        'updateEndTimeFromQuantity',
        'updateDurationDisplay',
        'checkTimeSlotAvailability',
        'handleDurationChange',
        'suggestNextAvailableTime',
        'showFormLoading',
        'hideFormLoading'
    ],
    
    'Parts Management Modal': [
        'updateMaterialDefaults',
        'calculateVolume', 
        'updateDurationDisplay',
        'calculateTotalCost',
        'calculateMargin'
    ],
    
    'Print Tracking Modals': [
        'closeModalSafely',
        'closeModal',
        'updateCalculatedDuration',
        'updateMaterialInfo',
        'togglePrototypeAddition',
        'addPrototypeEntry',
        'removePrototypeEntry'
    ]
};

// Test all function suites
Object.keys(testSuites).forEach(suiteName => {
    console.log(`\n?? Testing ${suiteName}:`);
    console.log('?'.repeat(50));
    
    const functions = testSuites[suiteName];
    let available = 0;
    let missing = 0;
    
    functions.forEach(funcName => {
        const exists = typeof window[funcName] === 'function';
        const status = exists ? '?' : '?';
        const message = exists ? 'Available' : 'Missing';
        
        console.log(`${status} ${funcName}: ${message}`);
        
        if (exists) {
            available++;
        } else {
            missing++;
        }
    });
    
    console.log(`\n?? Results: ${available} available, ${missing} missing`);
    
    if (missing === 0) {
        console.log('?? All functions available for ' + suiteName);
    } else {
        console.warn('??  Some functions missing for ' + suiteName);
    }
});

// Test main namespace objects
console.log('\n???  Testing Namespace Objects:');
console.log('?'.repeat(50));

const namespaces = ['JobForm', 'PartForm', 'PostPrintModal', 'PrintTracking'];
namespaces.forEach(ns => {
    const exists = typeof window[ns] === 'object' && window[ns] !== null;
    const status = exists ? '?' : '?';
    const message = exists ? 'Available' : 'Missing';
    console.log(`${status} window.${ns}: ${message}`);
});

// Test jQuery availability (if used)
console.log('\n?? Testing Dependencies:');
console.log('?'.repeat(50));
console.log(`? jQuery: ${typeof $ !== 'undefined' ? 'Available' : 'Not available'}`);
console.log(`? HTMX: ${typeof htmx !== 'undefined' ? 'Available' : 'Not available'}`);

console.log('\n?? Function availability test complete!');
console.log('Run this script on each modal page to verify all functions are working.');