var req = new XMLHttpRequest();
req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/sdkmessageprocessingsteps(a21ff0a8-6611-43b5-8efe-000206efc31b)?$select=_eventhandler_value,name,rank,_sdkmessageid_value,sdkmessageprocessingstepid,sdkmessageprocessingstepidunique,_sdkmessageprocessingstepsecureconfigid_value,solutionid,stage,statecode,versionnumber", true);
req.setRequestHeader("OData-MaxVersion", "4.0");
req.setRequestHeader("OData-Version", "4.0");
req.setRequestHeader("Accept", "application/json");
req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
req.onreadystatechange = function() {
    if (this.readyState === 4) {
        req.onreadystatechange = null;
        if (this.status === 200) {
            var result = JSON.parse(this.response);
            var _eventhandler_value = result["_eventhandler_value"];
            var _eventhandler_value_formatted = result["_eventhandler_value@OData.Community.Display.V1.FormattedValue"];
            var _eventhandler_value_lookuplogicalname = result["_eventhandler_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            var name = result["name"];
            var rank = result["rank"];
            var rank_formatted = result["rank@OData.Community.Display.V1.FormattedValue"];
            var _sdkmessageid_value = result["_sdkmessageid_value"];
            var _sdkmessageid_value_formatted = result["_sdkmessageid_value@OData.Community.Display.V1.FormattedValue"];
            var _sdkmessageid_value_lookuplogicalname = result["_sdkmessageid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            var sdkmessageprocessingstepid = result["sdkmessageprocessingstepid"];
            var sdkmessageprocessingstepidunique = result["sdkmessageprocessingstepidunique"];
            var _sdkmessageprocessingstepsecureconfigid_value = result["_sdkmessageprocessingstepsecureconfigid_value"];
            var _sdkmessageprocessingstepsecureconfigid_value_formatted = result["_sdkmessageprocessingstepsecureconfigid_value@OData.Community.Display.V1.FormattedValue"];
            var _sdkmessageprocessingstepsecureconfigid_value_lookuplogicalname = result["_sdkmessageprocessingstepsecureconfigid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
            var solutionid = result["solutionid"];
            var stage = result["stage"];
            var stage_formatted = result["stage@OData.Community.Display.V1.FormattedValue"];
            var statecode = result["statecode"];
            var statecode_formatted = result["statecode@OData.Community.Display.V1.FormattedValue"];
            var versionnumber = result["versionnumber"];
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }
};
// req.send();

var entity = {};
entity.name = "";
entity["plugintypeid@odata.bind"] = "/sdkmessagefilters()";
entity.rank = null;
entity["sdkmessageid@odata.bind"] = "/sdkmessages()";

var req = new XMLHttpRequest();
req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/sdkmessageprocessingsteps", true);
req.setRequestHeader("OData-MaxVersion", "4.0");
req.setRequestHeader("OData-Version", "4.0");
req.setRequestHeader("Accept", "application/json");
req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
req.onreadystatechange = function() {
    if (this.readyState === 4) {
        req.onreadystatechange = null;
        if (this.status === 204) {
            var uri = this.getResponseHeader("OData-EntityId");
            var regExp = /\(([^)]+)\)/;
            var matches = regExp.exec(uri);
            var newEntityId = matches[1];
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }
};
// req.send(JSON.stringify(entity));