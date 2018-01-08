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

imageHubProxy.checkHubConnection = function (retry, donefunction) {
    var isHubDisconnected = $.connection.hub && $.connection.hub.state === $.signalR.connectionState.disconnected;
    if (isHubDisconnected && retry) {
        $.connection.hub.start().done(
            function (donefunction) {
                if (donefunction) donefunction();
            }.bind(null, donefunction));
    }
    else if (donefunction) { donefunction(); }
    return isHubDisconnected;
}

imageHubProxy.detectFeaturesInSequence = function (imagesList, type, jobId) {
    var that = this;
    that.checkHubConnection(true, function () {
        let chunkSize = 1;
        let index = 0;
        for (let i = 0; i < gsdrawer.originalImages.length; i += chunkSize) {
            let upperLimit = Math.min(i + chunkSize, gsdrawer.originalImages.length);
            that.server.detectFeaturesInSequence(gsdrawer.originalImages.slice(i, upperLimit), type, jobId)
                .done(function (status) {
                if (status != "OK") {
                    console.warn("Error: detectFeaturesInSequence returned with message: " + status);
                }
                else {
                    console.log("detectFeaturesInSequence returned OK!");
                }
            }).fail(function (error, a, b, c) {
                console.log('Error: ' + error);
            });
        }
    });
}
//});