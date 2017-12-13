//$(function () {
    console.log('SignalRClient.js: Started.');
    var imageHubProxy = $.connection.ImageHub;

    imageHubProxy.client.sendFilteredCollection = function (filteredImage, type, jobId) {
        //Defined in HomeIndexScript.js
        gsdrawer.setFilteredImage(filteredImage, type);
        jobManager.updateJobBy(jobId, 1);
    }

    $.connection.hub.start().done(function () {
    });

    imageHubProxy.checkHubConnection = function(retry, done)
    { 
        var isHubDisconnected = $.connection.hub && $.connection.hub.state === $.signalR.connectionState.disconnected;
        if (isHubDisconnected && retry) {
            $.connection.hub.start().done(
                function (done) {
                    if (done) done();
                });
        }
        else if (done) { done(); }
        return isHubDisconnected;
    }
//});