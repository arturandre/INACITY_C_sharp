function ImageDownloader()
{
}

ImageDownloader.prototype.getImagesForSelectedStreet = function (Street, callback) {
    var that = this;
    if (!callback) throw "Error in getImagesForSelectedStreet! Callback is missing!"
    jQuery.support.cors = true;

    $.ajax({
        url: '/api/ImageMiner/ImagesFromStreet',
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
                    callback(data);
                    console.debug("Images adquired!");
                }
            }
    });
}