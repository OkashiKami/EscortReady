// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function OnUserEntryVRCNameChanged(txt)
{
    var id = txt.id.split('_')[1];
    var apiUrl = "api/Values/UVRCU?index=" + id + "&name=" + txt.value;
    // Options to be given as parameter
    // in fetch for making requests
    // other then GET
    let options = {
        method: 'POST',
        headers: {
            'Content-Type':
                'application/json;charset=utf-8'
        },
        body: JSON.stringify(id + txt.value)
    }
    // Fake api for making post requests
    let response = fetch(apiUrl, options);

};

// Write your JavaScript code.
function OnUserEntryVRCNameChanged(select) {
    var id = select.id.split('_')[1];
    var apiUrl = "api/Values/UDU?index=" + id + "&name=" + txt.value;
    // Options to be given as parameter
    // in fetch for making requests
    // other then GET
    let options = {
        method: 'POST',
        headers: {
            'Content-Type':
                'application/json;charset=utf-8'
        },
        body: JSON.stringify(id + txt.value)
    }
    // Fake api for making post requests
    let response = fetch(apiUrl, options);

};