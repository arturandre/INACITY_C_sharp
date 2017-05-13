var btStartSession = document.getElementById("btStartSession");

$(function () {
    
});

function initMap() {
    for (var i = 0; i < gsdrawers.length; i++) {
        gsdrawers[i].setMap(new google.maps.Map(document.getElementById(gsdrawers[i].id),
             gsdrawers[i].mapOptions));
        gsdrawers[i].drawRegionOnMap(gsdrawers[i].selectedRegions[0]);
        gsdrawers[i].drawStreetsInMap(gsdrawers[i].selectedRegions[0].StreetDTO);
    }
}