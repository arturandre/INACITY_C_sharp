﻿function ImageFilter()
{

}

ImageFilter.prototype.getTreesForSelectedStreet = function (Street, callback) {
    var that = this;
    if (!callback) throw "Error in getTreesForSelectedStreet! Callback is missing!"
    jQuery.support.cors = true;
    $.ajax({
        url: '/api/ImageFilter/TreesByStreet/',
        type: 'POST',
        dataType: 'json',
        data: Street,
        statusCode:
            {
                500: function (x, y, z) {
                    console.log("x: " + x);
                    console.log("y: " + y);
                    console.log("z: " + z);
                },
                403: function () { },
                200: function (data) {
                    var pictures = [];
                    $.each(data, function (i, v)
                    {
                        
                        pictures.push(v)
                    });
                    callback(data, 'Trees');
                    console.debug("Filter Images adquired!");
                }
            }
    });
}

ImageFilter.prototype.getCracksForSelectedStreet = function (Street, callback)
{
    var that = this;
    if (!callback) throw "Error in getTreesForSelectedStreet! Callback is missing!"
    jQuery.support.cors = true;
    
    $.ajax({
        url: '/api/ImageFilter/CracksByStreet',
        type: 'POST',
        dataType: 'json',
        data: Street,
        statusCode:
            {
                500: function (x, y, z) {
                    console.log("x: " + x);
                    console.log("y: " + y);
                    console.log("z: " + z);
                },
                403: function () { },
                200: function (data) {
                    callback(data, 'Cracks');
                    console.debug("Filter Images adquired!");
                }
            }
    });
}

ImageFilter.prototype.applyGenericFilter = function(pictures, remoteUrl, type, callback)
{
    if (!callback) throw "Error in applyGenericFilter! Callback is missing!"
    jQuery.support.cors = true;

    var settings = {
        "async": true,
        "crossDomain": true,
        "url": remoteUrl,
        "method": "POST",
        "headers": {
            "content-type": "application/json",
            "cache-control": "no-cache",
            "postman-token": "b9ca612d-c37a-2df8-4073-8e684fc2e7cb"
        },
        "processData": false,
        "data": JSON.stringify(pictures),
    }

    $.ajax(settings).done(function (response) {
        if (response.statusCode == 500)
        { 
            console.log(response);
        }
        else if (response.statusCode == 403) {}
        else
        {
            callback(response, type);
            console.debug("Filter Images adquired!");
        }

            
            
        
    });

    
}