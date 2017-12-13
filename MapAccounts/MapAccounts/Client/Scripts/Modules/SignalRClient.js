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

//});