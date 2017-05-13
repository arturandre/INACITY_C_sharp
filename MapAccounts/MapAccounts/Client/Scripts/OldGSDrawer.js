//TODO: Encapsular estar propriedades que auxiliam nos desenhos do mapa dentro de um objeto 'MapDrawer'
var regionMarkers = [];
var selectedRegions = [];
var polylinesOnMap = {};

var map;
var geocoder;

function initMap() {
    console.log("Alles raito to Start!");
    var myLatlng = { lat: -23.562883, lng: -46.654671 };
    var mapOptions = {
        zoom: 15,
        center: myLatlng

    };

    map = new google.maps.Map(document.getElementById("map"),
         mapOptions);
    map.addListener('click', function (e) {
        mapClick(e.latLng, map);
    });

    geocoder = new google.maps.Geocoder();
}

var configStroke = {
    geodesic: true,
    strokeColor: '#FF0000',
    strokeOpacity: 0.6,
    strokeWeight: 13
};

function drawRegionOnMap(region, map) {
    var vertices = [];

    vertices.push({ lat: region.Bounds.North, lng: region.Bounds.East });
    vertices.push({ lat: region.Bounds.South, lng: region.Bounds.East });
    vertices.push({ lat: region.Bounds.South, lng: region.Bounds.West });
    vertices.push({ lat: region.Bounds.North, lng: region.Bounds.West });
    vertices.push({ lat: region.Bounds.North, lng: region.Bounds.East });

    region.GSrectangle = new google.maps.Polyline({
        path: vertices,
        geodesic: true,
        strokeColor: '#FF0000',
        strokeOpacity: 0.6,
        map: map,
        strokeWeight: 2
    });
}

function focusStreetOnMap(Street, map) {
    for (var polyline in polylinesOnMap) {
        $.each(polylinesOnMap[polyline], function (i, segment) {
            if (polyline == Street.Name) {
                segment.setMap(null);
                //segment.setOptions({ strokeColor: '#0000FF' });
            }
            else {
                segment.setOptions({ strokeColor: '#FF0000' });
            }
        });
    }
    var polylines = streetToGSPolylineArray(Street);
    $.each(polylines, function (i, p) {
        p.setMap(map);
        p.setOptions({ strokeColor: '#0000FF' });
    });
    polylinesOnMap[Street.Name] = polylines;
}

function clearPolylinesOnMap() {
    for (var polyline in polylinesOnMap) {
        $.each(polylinesOnMap[polyline], function (i, segment) {
            segment.setMap(null);
        });
    }
    polylinesOnMap = [];
}

function drawStreetsInMap(streets, map) {
    $.each(streets, function (indexOfStreet, Street) {
        if (!Street.color) Street.color = rColor();
        var polylines = streetToGSPolylineArray(Street);
        $.each(polylines, function (i, p) { p.setMap(map); });
        polylinesOnMap[Street.Name] = polylines;
    });
}

function streetToGSPolylineArray(Street) {
    var polylineArray = [];
    $.each(Street.Segments, function (indexSegment, segment) {
        configStroke.strokeColor = Street.color;
        configStroke.path = segment;
        var polyline = new google.maps.Polyline(configStroke);
        google.maps.event.addListener(polyline, 'click', function () {
            focusStreetOnMap(Street, map);
        });
        polylineArray.push(polyline);
    });
    return polylineArray;
}


function clearRegions() {
    while (selectedRegions.length > 0) {
        var region = selectedRegions.pop();
        region.GSrectangle.setMap(null);
    }
}

function mapClick(latLng, map) {
    var marker = new google.maps.Marker({
        position: latLng,
        map: map,
        type: 'regionMarker'
    });
    regionMarkers.unshift(marker);
    if (regionMarkers.length == 2) {
        var region = getRegion();

        //TODO: Remover isso para permitir a seleção de diversas regiões simultâneamente
        clearRegions();

        selectedRegions.push(region);
        drawRegionOnMap(region, map);
    }
}

function getRegion() {
    {
        var coords = [];
        while (regionMarkers.length > 0) {
            var marker = regionMarkers.pop();
            coords.push(marker.position);
            marker.setMap(null);
        }
        var South;
        var West;
        var North;
        var East;
        if (coords[0].lat() < coords[1].lat()) {
            South = coords[0].lat();
            North = coords[1].lat();
        }
        else {
            North = coords[0].lat();
            South = coords[1].lat();
        }
        if (coords[0].lng() < coords[1].lng()) {
            West = coords[0].lng();
            East = coords[1].lng();
        }
        else {
            East = coords[0].lng();
            West = coords[1].lng();
        }
        var Bounds = {
            "South": South,
            "West": West,
            "North": North,
            "East": East
        };
        var region = {};
        region.Bounds = Bounds;
        return region;
    }


}

function panMapByAddress() {
    var address = tbAddress.value;
    geocoder.geocode({ 'address': address }, function (results, status) {
        if (status === google.maps.GeocoderStatus.OK) {
            map.setCenter(results[0].geometry.location);
        } else {
            alert('Geocode falhou: ' + status);
        }
    });
}

function rColor() {
    var text = "";
    var possible = "AB0123456789";
    var desiredLength = 6;

    for (var i = 0; i < desiredLength; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));

    return '#' + text;
}

function getStreetsInRegions(regions, callback) {
    if (!callback) throw "Error in getStreetsInRegions! Callback missing!"
    jQuery.support.cors = true;
    $.each(regions, function (indexOfRegion, region) {
        $.ajax({
            url: '/api/MapMiner/StreetsInRegion',
            type: 'POST',
            dataType: 'json',
            data: region.Bounds,
            success: function (data) {
                if ($.isArray(data) && data.length > 0) {
                    region.StreetDTO = data;
                    callback();
                }
            },
            error: function (x, y, z) {
                alert(x + '\n' + y + '\n' + z);
            }
        });
    });
}

function saveRegions(regions, callback) {
    if (!callback) throw "Error in saveRegions! Callback missing!"
    jQuery.support.cors = true;
    $.each(regions, function (indexOfRegion, region) {
        $.ajax({
            url: '/Home/SaveSection',
            type: 'POST',
            dataType: 'json',
            data: region,
            success: function (data) {
                console.debug("saved!");
            }
        });
    });
}

function saveStreets(streets, callback) {
    if (!callback) throw "Error in saveStreets! Callback missing!"
    jQuery.support.cors = true;
    $.each(streets, function (indexOfRegion, street) {
        $.ajax({
            url: '/Home/SaveStreets',
            type: 'POST',
            dataType: 'json',
            data: street,
            success: function (data) {
                if (data) {
                    street.ID = data.ID;
                }
            }
        });
    });
}