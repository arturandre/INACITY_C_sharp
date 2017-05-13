var tbAddress = document.getElementById("tbAddress");
var btAddress = document.getElementById("btAddress");
var btSaveSection = document.getElementById("btSaveSection");

var btStreets = document.getElementById("btStreets");
var divReports = document.getElementById("divReports");

var gsdrawer = new GSDrawer();

$(function () {
    bindings();
    $(document).ajaxError(function (e, xhr) {
        if (xhr.status == 403) {
            var response = $.parseJSON(xhr.responseText);
            window.location = response.LogOnUrl;
        }
    });
});

function bindings() {
    btAddress.onclick = panMapByAddress;
    btStreets.onclick = function () {
        gsdrawer.getStreetsInRegions(function () {
            $.each(gsdrawer.selectedRegions, function (indexOfRegion, region) {
                gsdrawer.drawStreetsInMap(region.StreetDTO);
            });
        });
    };
    btSaveSection.onclick = saveSessionClick;
    GSDrawer.initMap = function () {
        gsdrawer.setMap(new google.maps.Map(document.getElementById("map"),
            mapOptions));
        $.each(gsdrawer.selectedRegions, function (index, region) {
            gsdrawer.drawRegionOnMap(region);
            gsdrawer.drawStreetsInMap(region.StreetDTO);
        });
    }
}

function panMapByAddress() {
    var address = tbAddress.value;
    gsdrawer.geocoder.geocode({ 'address': address }, function (results, status) {
        if (status === google.maps.GeocoderStatus.OK) {
            gsdrawer.getMap().setCenter(results[0].geometry.location);
        } else {
            alert('Geocode falhou: ' + status);
        }
    });
}





