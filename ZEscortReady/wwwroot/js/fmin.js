function callSelectServerCSMethod() {
    var id = $('#h_guildid').val();
    //alert("This fuction was called");
    var apiUrl = "api/values?id=" + id;
    // Options to be given as parameter
    // in fetch for making requests
    // other then GET
    let options =
    {
        method: 'POST',
        headers:
        {
            'Content-Type': 'application/json;charset=utf-8'
        },
    }
    // Fake api for making post requests
    let res = fetch(apiUrl, options);
    location.reload()
}
function callRemoveServerCSMethod() {
    location.reload();
    //alert("This fuction was called");
    var id = $('#h_guildid').val();
    var apiUrl = "api/values?id=" + 0;

    // Options to be given as parameter
    // in fetch for making requests
    // other then GET
    let options =
    {
        method: 'POST',
        headers:
        {
            'Content-Type': 'application/json;charset=utf-8'
        },
    }
    // Fake api for making post requests
    let res = fetch(apiUrl, options);
    location.reload();
}
function callApplyCSMethod() {
    var apiUrl = "api/values/apply";
    let res = fetch(apiUrl);
    location.reload();
}

function callAddVRCUserEntry() {
    var apiUrl = "api/values/vrcAddUserEntry";
    let res = fetch(apiUrl);
    location.reload();
}
function callRemoveVRCUserEntry() {
    var index = $('#h_index').val();
    var apiUrl = "api/values/vrcRemoveUserEntry?id=" + index;
    // Options to be given as parameter
    // in fetch for making requests
    // other then GET
    let options =
    {
        method: 'POST',
        headers:
        {
            'Content-Type': 'application/json;charset=utf-8'
        },
    }
    // Fake api for making post requests
    let res = fetch(apiUrl, options);
    location.reload();
}