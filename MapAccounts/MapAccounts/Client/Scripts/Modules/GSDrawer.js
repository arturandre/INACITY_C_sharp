var geometryLib = null;
var AMENITY_PANORAMA_MAXDISTANCE = 10;
//Funções associadas ao callback de inicialização do Google Maps

function initMap() {
    console.log("Google Maps Loaded");
    //var myLatlng = { lat: -23.562883, lng: -46.654671 };
    //var mapOptions = {
    //    zoom: 15,
    //    center: myLatlng
    //};
    callInitMap();
}

function callInitMap() {
    if (GSDrawer.initMap) {
        GSDrawer.initMap();
    }
    else {
        setTimeout(callInitMap, 1000);
    }
}


GSDrawer.prototype.onPause = null;
GSDrawer.prototype.onImagePresentation = null;
GSDrawer.prototype.onImageChanged = null;
GSDrawer.prototype.onSelectedStreetChanged = null;
GSDrawer.prototype.onSelectedRegionChanged = null;
GSDrawer.prototype.onStreetFocused = null;


function GSDrawer() {
    var that = this;

    var filterType = ["Cracks", "Trees"];

    this.snapin = false;
    

    this.originalImages = [];
    var imagesMetaData =
        {
            "streetName": null,
            "interpolate": null,
        };

    this.regionMarkers = [];
    this.amenityMarkers = [];
    this.selectedRegions = [];
    this.polylinesOnMap = [];
    this.lastInfoWindow = null;
    this.selectedMarker = null;
    this.autoplay = true;

    var imgIndex = 0;

    var geocoder = null;

    var map = null;
    var imagePinPoint = null;
    var imagePinPointArrow = null;

    var selectedStreet = null;

    var heatMap = null;
    var heatMapMode = false;

    var isAmenitiesVisible = false;

    this.getImgIndex = function () { return imgIndex; }
    this.setImgIndex = function (nval) { imgIndex = nval; }

    this.setMap = function (_map) {
        if (!geometryLib) geometryLib = google.maps.geometry;
        map = _map;
        if (!heatMap) heatMap = new GSHeatMap();
        if (!geocoder) geocoder = new google.maps.Geocoder();
        with ({ gsdrawer: this }) {
            map.addListener('click', function (e) {
                var marker = new google.maps.Marker({
                    position: e.latLng,
                    map: map
                });
                gsdrawer.regionMarkers.unshift(marker);
                if (gsdrawer.regionMarkers.length == 2) {
                    var region = gsdrawer.getRegion();

                    //TODO: Remover isso para permitir a seleção de diversas regiões simultâneamente
                    gsdrawer.clearRegions();

                    gsdrawer.selectedRegions.push(region);
                    gsdrawer.drawRegionOnMap(region);
                    if (!!gsdrawer.onSelectedRegionChanged) {
                        gsdrawer.onSelectedRegionChanged();
                    }
                }
            });
        };
    }

    this.showInfoWindow = function (infowindow, map, marker) {
        if (!!this.lastInfoWindow)
            this.lastInfoWindow.close();
        this.lastInfoWindow = infowindow;
        infowindow.open(map, marker);
    }

    this.setImagePinPoint = function (positionData) {
        var position = new google.maps.LatLng({ lat: positionData.location.lat, lng: positionData.location.lng });
        if (!!imagePinPoint) {
            imagePinPoint.setMap(null);
            imagePinPoint = null;
        }
        if (!!imagePinPointArrow) {
            imagePinPointArrow.setMap(null);
            imagePinPointArrow = null;
        }
        if (!!position) {
            imagePinPoint = new google.maps.Marker({
                icon: window.location.origin + '/Content/blueeyedmarker.png',
                position: position,
                map: map
            });
            var arrowSymbol = {
                path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW
            };
            imagePinPointArrow = new google.maps.Polyline({
                strokeColor: "#00FF00",
                zIndex: google.maps.Marker.MAX_ZINDEX + 1,
                path: [position,
                    geometryLib.spherical.computeOffset(position, 20, positionData.heading)],
                icons: [{
                    icon: arrowSymbol,
                    offset: '100%'
                }],
                map: map
            });

        }
    }

    this.getMap = function () {
        return map;
    }

    this.getGeocoder = function () {
        return geocoder;
    }

    this.showAmenityImage = function (address) {
        var that = this;
        geocoder.geocode({ 'address': address }, function (results, status) {
            var lat2 = that.selectedMarker.position.lat();
            var lng2 = that.selectedMarker.position.lng();
            var p2 = new google.maps.LatLng({ lat: lat2, lng: lng2 });

            if (status === google.maps.GeocoderStatus.OK) {
                if (checkForDistance(p2, results[0].geometry.location)) {
                    lat2 = results[0].geometry.location.lat();
                    lng2 = results[0].geometry.location.lng();
                    p2 = new google.maps.LatLng({ lat: lat2, lng: lng2 });
                }
            }
            else if (!confirm('A geocodificação para este endereço falhou com o erro: ' + status + '. A aproximação pode ser imprecisa, deseja continuar?')) {
                return;
            }

            var gspan = new GSPanoramaMiner();

            gspan.getPanoramaId(p2, function (data, status) {
                if (status === "OK") {

                    that.getMap().setCenter(data.location.latLng);

                    var lat1 = data.location.latLng.lat();
                    var lng1 = data.location.latLng.lng();

                    var p1 = new google.maps.LatLng({ lat: lat1, lng: lng1 });

                    var busStopHeading = geometryLib.spherical.computeHeading(p1, p2);

                    //that.selectedMarker.setPosition(data.location.latLng);
                    that.setImagePinPoint({ location: { lat: data.location.latLng.lat(), lng: data.location.latLng.lng() }, heading: busStopHeading });


                    var busStopImageUrl = 'http://maps.googleapis.com/maps/api/streetview?size=640x640&pano=' +
                        data.location.pano +
                        '&heading=' + busStopHeading +
                        '&pitch=0' +
                        '&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac&sensor=false';
                    that.pause();
                    that.setImgByUrl(busStopImageUrl);
                    console.debug(busStopImageUrl);
                }
                else {
                    alert("Infelizmente não foi possível encontrar imagens para este ponto.");
                }

            }, AMENITY_PANORAMA_MAXDISTANCE);


        });

    }

    this.setSelectedStreet = function (Street) {
    	if (selectedStreet == Street) return;

    	this.originalImages = [];
        this.pause();
        selectedStreet = Street;
        resetImageData();
        if (!!this.onSelectedStreetChanged) {
            this.onSelectedStreetChanged(Street);
        }
    }

    var resetImageData = function () {
        imagesMetaData =
        {
            "interpolate": null,
        };
        this.originalImages = [];
    }

    this.getImagesMetaData = function () {
        return imagesMetaData;
    }

    this.getSelectedStreet = function () {
        return selectedStreet;
    }

    this.toggleHeatMapMode = function (force) {
        if (force == true || force == false) {
            heatMapMode = force;
        }
        else {
            heatMapMode = !heatMapMode;
        }
        heatMap.setMap(heatMapMode ? map : null);
        for (var polyline in this.polylinesOnMap) {
            $.each(this.polylinesOnMap[polyline], function (i, segment) {
                segment.setMap(!heatMapMode ? map : null);
            });
        }
    }

    this.setHeatMapData = function (type) {
        var locations = [];
        var weights = [];
        $.each(this.originalImages, function (ip, p) {
            locations.push(p.location);
            var f = p.filterResults[type];
            if (!!f) {
                weights.push(!!f.Density ? f.Density : f.isCaracteristicPresent == true ? 1 : 0);
            }
        });
        heatMap.setData(locations, weights);
    }

    this.plotHeatmapFromDB = function (featureType) {
        var region = this.selectedRegions[0];
        $.ajax({
            url: '/api/DBHeatMap/GetFeaturesInRegion',
            type: 'POST',
            dataType: 'json',
            data: region.Bounds,
            success: function (data) {
                if ($.isArray(data) && data.length > 0) {
                    var locations = [];
                    var weights = [];
                    $.each(data, function (idxHMP, heatMapPoint) {
                        locations.push(heatMapPoint.location);
                        weights.push(featureType == "Trees" ? heatMapPoint.TreesDensity : CracksDensity);
                        heatMap.setData(locations, weights);
                    });
                }

            },
            error: function (request, textStatus, errorThrown) {
                if (request.statusText === 'abort') {
                    return;
                }
                alert(request + '\n' + textStatus + '\n' + errorThrown);
            }
        });
    }

    this.toggleAmenities = function (forceState) {
        if (forceState != null)
            isAmenitiesVisible = forceState;
        else
            isAmenitiesVisible = !isAmenitiesVisible;
        for (var i = 0; i < this.amenityMarkers.length; i++) {
            var marker = this.amenityMarkers[i];
            marker.setMap(isAmenitiesVisible ? map : null);
        }
    }

    checkForDistance = function (p2, location) {
        var _lat2 = location.lat();
        var _lng2 = location.lng();
        var _p2 = new google.maps.LatLng({ lat: _lat2, lng: _lng2 });

        return geometryLib.spherical.computeDistanceBetween(p2, _p2) <= AMENITY_PANORAMA_MAXDISTANCE;
    }

    this.showAllAmenitiesImages = function () {
        var that = this;
        var pictureIndex = 0;
        that.originalImages = [];
        var geocoderThreadCounter = 0;
        var getPanoramaIdThreadCounter = 0;
        $.each(that.amenityMarkers, function (idxMarker, vMarker) {
            geocoderThreadCounter++;
            geocoder.geocode({ 'address': vMarker.address }, function (results, status) {
                geocoderThreadCounter--;
                var lat2 = vMarker.position.lat();
                var lng2 = vMarker.position.lng();
                var p2 = new google.maps.LatLng({ lat: lat2, lng: lng2 });

                if (status === google.maps.GeocoderStatus.OK) {
                    if (checkForDistance(p2, results[0].geometry.location)) {
                        lat2 = results[0].geometry.location.lat();
                        lng2 = results[0].geometry.location.lng();
                        p2 = new google.maps.LatLng({ lat: lat2, lng: lng2 });
                    }
                }
                //else if (!confirm('A geocodificação para este endereço falhou com o erro: ' + status + '. A aproximação pode ser imprecisa, deseja continuar?')) {
                //    return;
                //}

                var gspan = new GSPanoramaMiner();
                getPanoramaIdThreadCounter++;
                gspan.getPanoramaId(p2, function (data, status) {
                    getPanoramaIdThreadCounter--;
                    if (status === "OK") {
                        var position = data.location.latLng;
                        //var _North = GSLatLngToJSON(geometryLib.spherical.computeOffset(position, 5, 0));
                        //var _South = GSLatLngToJSON(geometryLib.spherical.computeOffset(position, 5, 180));
                        //var _East = GSLatLngToJSON(geometryLib.spherical.computeOffset(position, 5, 90));
                        //var _West = GSLatLngToJSON(geometryLib.spherical.computeOffset(position, 5, 270));
                        //var localRegion = {Bounds: { North: _North, South: _South, East: _East, West: _West }};

                        that.getMap().setCenter(data.location.latLng);

                        var lat1 = position.lat();
                        var lng1 = position.lng();

                        var p1 = new google.maps.LatLng({ lat: lat1, lng: lng1 });

                        var cameraHeading = geometryLib.spherical.computeHeading(p1, p2);

                        vMarker.cameraHeading = cameraHeading;

                        //that.selectedMarker.setPosition(data.location.latLng);
                        //that.setImagePinPoint({ location: { lat: data.location.latLng.lat(), lng: data.location.latLng.lng() }, heading: busStopHeading });
                        var imageUrl = 'http://maps.googleapis.com/maps/api/streetview?size=640x640&pano=' +
                            position.pano +
                            '&heading=' + cameraHeading +
                            '&pitch=0' +
                            '&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac&sensor=false';
                        vMarker.imageUrl = imageUrl;

                        var picture = PictureDTO.initialize(pictureIndex++, position.pano, cameraHeading, null, imageUrl,
                        PointDTO.initialize(position.ID, lat1, lng1), null);

                        that.originalImages.push(picture);
                    }
                    else {
                        //alert("Infelizmente não foi possível encontrar imagens para este ponto.");
                    }

                    //geocoderThreadCounter e getPanoramaIdThreadCounter são usados para identificar o final das chamadas assíncronas
                    if (geocoderThreadCounter == getPanoramaIdThreadCounter && geocoderThreadCounter == 0) {
                        that.startImagePresentation("originalSet");
                    }
                }, AMENITY_PANORAMA_MAXDISTANCE);//gspan.getPanoramaId(p2, function (data, status) {
            });
        });

    }

    this.putAmenity = function (latLng, title, address) {
        var that = this;
        address = !!address ? address.replace(new RegExp("\"", 'g'), "") : null;
        var infowindow = new google.maps.InfoWindow({
            content: " "
        });
        infowindow.setContent(title + '</br>' + "<button onclick=\"gsdrawer.showAmenityImage('" + address + "')\">Ver imagem</button>");
        var marker = new google.maps.Marker({
            position: latLng,
            map: map,
            title: title
        });
        marker.address = address;

        marker.addListener('click', function () {
            that.showInfoWindow(infowindow, map, marker);
            if (!!that.selectedMarker) {
                that.selectedMarker.setIcon(null);
            }
            marker.setIcon(window.location.origin + '/Content/greenmarker.png');
            that.selectedMarker = marker;
            that.getMap().setCenter(marker.position);
        });

        this.amenityMarkers.push(marker);
    }

    $.getScript("Client/Scripts/Modules/GSDrawerProto.js", function () {
        //TODO: Handle script loading
    });

}
function myFunction(address) {
    alert(address);
}

GSDrawer.initMap = null;

GSDrawer.redStroke = {
    geodesic: true,
    strokeColor: '#FF0000',
    strokeOpacity: 0.6,
    strokeWeight: 13
};

GSDrawer.prototype.clearAmenities = function () {
    this.toggleAmenities(false);
    this.amenityMarkers = [];
}


GSDrawer.prototype.showAmenity = function (amenityType, callback) {
    this.clearAmenities();
    var that = this;
    $.each(this.selectedRegions, function (indexOfRegion, region) {
        $.ajax({
            url: '/api/MapMiner/AmenitiesInRegion/' + amenityType,
            type: 'POST',
            dataType: 'json',
            data: region.Bounds,
            success: function (data) {
                if ($.isArray(data)) {
                    if (data.length > 0) {
                        $.each(data, function (i, p) {
                            that.putAmenity({ lat: p.lat, lng: p.lng }, p.name, p.address);
                        });
                        callback(true);
                        return;
                    }
                    else {
                        alert('Nenhuma ocorrência foi encontrada para esta região.');
                    }
                }
                else {
                    alert('Retorno não era um vetor (ERRO).');
                }
                callback(false);
                return;
            },
            error: function (x, y, z) {
                callback(false);
                alert(x + '\n' + y + '\n' + z);
                return;
            }
        });
    });
}

GSDrawer.prototype.drawRegionOnMap = function (region) {
    var vertices = [];

    vertices.push({ lat: region.Bounds.North, lng: region.Bounds.East });
    vertices.push({ lat: region.Bounds.South, lng: region.Bounds.East });
    vertices.push({ lat: region.Bounds.South, lng: region.Bounds.West });
    vertices.push({ lat: region.Bounds.North, lng: region.Bounds.West });
    vertices.push({ lat: region.Bounds.North, lng: region.Bounds.East });

    console.debug("area> " + area(vertices[0], vertices[2]));

    region.GSrectangle = new google.maps.Polyline({
        path: vertices,
        geodesic: true,
        strokeColor: '#FF0000',
        strokeOpacity: 0.6,
        map: this.getMap(),
        strokeWeight: 2
    });
}



