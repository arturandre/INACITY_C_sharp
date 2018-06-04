var geometryLib = null;
var AMENITY_PANORAMA_MAXDISTANCE = 10;

//Funções associadas ao callback de inicialização do Google Maps

class GSDrawer {
    constructor() {
        this.snapin = false;
        this.originalImages = [];
        this.regionMarkers = [];
        this.amenityMarkers = [];
        this.selectedRegions = [];
        this.polylinesOnMap = [];
        this.lastInfoWindow = null;
        this.selectedMarker = null;
        this.autoplay = true;

        this.onPause = null;
        this.onImagePresentation = null;
        this.onClearImagePresentation = null;
        this.onImageChanged = null;
        this.onSelectedStreetChanged = null;
        this.onSelectedRegionChanged = null;
        this.onRegionSet = null;
        this.onStreetFocused = null;
        this.onStreetsLoaded = null;
        this.onStreetsUnloaded = null;

        if (!GSDrawer.initialized) {
            GSDrawer.initialized = true;
            GSDrawer.MAX_IMAGES_PER_REGION = 27000;
            GSDrawer.initMap = null;
            GSDrawer.pictureIndex = 0;

            GSDrawer.redStroke = {
                geodesic: true,
                strokeColor: '#FF0000',
                strokeOpacity: 0.6,
                strokeWeight: 13
            };

        }

        let filterType = ["Cracks", "Trees"];
        let imagesMetaData =
            {
                "streetName": null,
                "interpolate": null
            };
        let imgIndex = 0;

        let geocoder = null;

        let map = null;
        let imagePinPoint = null;
        let imagePinPointArrow = null;

        let selectedStreet = null;

        let heatMap = null;
        let heatMapMode = false;

        let isAmenitiesVisible = false;

        this.getImgIndex = function () { return imgIndex; }
        this.setImgIndex = function (nval) { imgIndex = nval; }
        this.setMap = function (_map) {
            if (!geometryLib) geometryLib = google.maps.geometry;
            map = _map;
            if (!heatMap) heatMap = new GSHeatMap();
            if (!geocoder) geocoder = new google.maps.Geocoder();
            let that = this;
            map.addListener('click', function (e) {
                var marker = new google.maps.Marker({
                    position: e.latLng,
                    map: map
                });
                that.regionMarkers.unshift(marker);
                if (that.regionMarkers.length == 2) {
                    var region = that.setRegion();


                    //TODO: Remover isso para permitir a seleção de diversas regiões simultâneamente
                    that.clearRegions();

                    that.selectedRegions.push(region);
                    that.drawRegionOnMap(region);
                    //TODO: Configurar um meio para que o usuário possa selecionar um conjunto de regiões
                    //e este evento seja chamado na mudança do conjunto (alteração do conjunto atual ou novo conjunto)
                    if (that.onSelectedRegionChanged) {
                        that.onSelectedRegionChanged();
                    }


                }
            });

        }

        this.showInfoWindow = function (infowindow, map, marker) {
            if (!!this.lastInfoWindow)
                this.lastInfoWindow.close();
            this.lastInfoWindow = infowindow;
            infowindow.open(map, marker);
        }

        this.setImagePinPoint = function (positionData) {
            if (imagePinPoint) {
                imagePinPoint.setMap(null);
                imagePinPoint = null;
            }
            if (imagePinPointArrow) {
                imagePinPointArrow.setMap(null);
                imagePinPointArrow = null;
            }
            if (positionData) {
                var position = new google.maps.LatLng({ lat: positionData.location.lat, lng: positionData.location.lng });
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

                var gspan = new GSPanoramaMiner(null, null, 10);

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
            this.resetImageData();
            if (this.onSelectedStreetChanged) {
                this.onSelectedStreetChanged(Street);
            }
        }

        this.resetImageData = function () {
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
    }


    /*
    static initMap() {
        console.log("Google Maps Loaded");
        //var myLatlng = { lat: -23.562883, lng: -46.654671 };
        //var mapOptions = {
        //    zoom: 15,
        //    center: myLatlng
        //};
        callInitMap();
    }

    static callInitMap() {
        if (GSDrawer.initMap) {
            GSDrawer.initMap();
        }
        else {
            setTimeout(callInitMap, 1000);
        }
    }*/

    static streetToGSPolylineArray(Street, mColor) {
        var polylineArray = [];
        $.each(Street.Trechos, function (indexSegment, segment) {
            var configStroke = GSDrawer.redStroke;
            configStroke.strokeColor = mColor ? mColor : Street.color;
            configStroke.path = segment;
            var polyline = new google.maps.Polyline(configStroke);
            polylineArray.push(polyline);
        });
        return polylineArray;
    };

    static checkForDistance(p2, location) {
        var _lat2 = location.lat();
        var _lng2 = location.lng();
        var _p2 = new google.maps.LatLng({ lat: _lat2, lng: _lng2 });

        return geometryLib.spherical.computeDistanceBetween(p2, _p2) <= AMENITY_PANORAMA_MAXDISTANCE;
    }

    static rColor() {
        var text = "";
        var possible = "AB0123456789";
        var desiredLength = 6;

        for (var i = 0; i < desiredLength; i++)
            text += possible.charAt(Math.floor(Math.random() * possible.length));

        return '#' + text;
    };

    clearAmenities() {
        this.toggleAmenities(false);
        this.amenityMarkers = [];
    }

    showAmenity(amenityType, callback) {
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

    drawRegionOnMap(region) {
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

    focusStreetOnMap(Street) {
        if (this.getSelectedStreet() === Street) return;
        if (this.getSelectedStreet() && !confirm(getResourceString("CHANGE_ADDRESS_WARN"))) {
            return;
        }
        var that = this;
        this.setSelectedStreet(Street);
        console.log(Street ? Street.Trechos.length : null);
        for (var polyline in that.polylinesOnMap) {
            $.each(that.polylinesOnMap[polyline], function (i, segment) {
                if (polyline === Street.Name) {
                    segment.setMap(null);
                }
                else {
                    segment.setOptions({ strokeColor: '#FF0000' });
                }
            });
        }
        var polylines = GSDrawer.streetToGSPolylineArray(Street);
        $.each(polylines, function (i, p) {
            p.setOptions({ strokeColor: '#0000FF' });
            google.maps.event.addListener(p, 'click', function () {
                that.focusStreetOnMap(Street);
            });
            p.setMap(that.getMap());

        });
        this.polylinesOnMap[Street.Name] = polylines;
        if (this.onStreetFocused) {
            this.onStreetFocused(this);
        }
    };

    clearPolylinesOnMap() {
        for (var polyline in this.polylinesOnMap) {
            $.each(this.polylinesOnMap[polyline], function (i, segment) {
                segment.setMap(null);
            });
        }
        this.polylinesOnMap = [];
        if (this.onStreetsUnloaded) this.onStreetsUnloaded();
        this.setSelectedStreet(null);
    };

    resetStreetColor(Street) {
        $.each(this.polylinesOnMap[Street.Name], function (i, segment) {
            segment.setOptions({ strokeColor: Street.color });
        });
    };

    drawStreetsInMap(streets, mColor) {
        var that = this;
        this.clearPolylinesOnMap();
        $.each(streets, function (indexOfStreet, Street) {
            if (!Street.color) Street.color = GSDrawer.rColor();
            var polylines = GSDrawer.streetToGSPolylineArray(Street, mColor);
            $.each(polylines, function (indexOfPolyline, polyline) {
                google.maps.event.addListener(polyline, 'click', function () {
                    that.focusStreetOnMap(Street);
                });
            });
            $.each(polylines, function (i, p) { p.setMap(that.getMap()); });
            that.polylinesOnMap[Street.Name] = polylines;
        });
    };

    clearRegions() {
        if (this.getSelectedStreet() && !confirm(getResourceString("CHANGE_ADDRESS_WARN"))) {
            return;
        }
        while (this.selectedRegions.length > 0) {
            var region = this.selectedRegions.pop();
            region.GSrectangle.setMap(null);
        }
        this.clearPolylinesOnMap();
        this.clearImagePresentation();
        this.clearAmenities();
        this.resetImageData();
        this.setImagePinPoint(null);
    };

    getSelectedRegions() {
        if (this.selectedRegions.length > 0)
            //TODO: Retornar a lista de regiões selecionadas
            return this.selectedRegions[0];
    }

    setRegion() {
        {
            var coords = [];
            while (this.regionMarkers.length > 0) {
                var marker = this.regionMarkers.pop();
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
            var upperLeft = new google.maps.LatLng({ lat: North, lng: West });
            var upperRight = new google.maps.LatLng({ lat: North, lng: East });
            var bottomRight = new google.maps.LatLng({ lat: South, lng: East });
            var bottomLeft = new google.maps.LatLng({ lat: South, lng: West });
            region.LatLngMat = new Array();
            region.LatLngMat.push(upperLeft);
            region.LatLngMat.push(upperRight);
            region.LatLngMat.push(bottomRight);
            region.LatLngMat.push(bottomLeft);
            return region;
        }
    };

    areStreetsLoaded() {
        return this.selectedRegions.length > 0 && this.selectedRegions[0].StreetDTO.length > 0;
    };

    getStreetsInRegions(callback) {
        //if (!callback) throw "Error in getStreetsInRegions! Callback missing!";
        var that = this;
        jQuery.support.cors = true;
        var ajaxCalls = [];
        $.each(that.selectedRegions, function (indexOfRegion, region) {
            var call = $.ajax({
                url: '/api/MapMiner/StreetsInRegion',
                type: 'POST',
                dataType: 'json',
                data: region.Bounds,
                success: function (data) {
                    if ($.isArray(data) && data.length > 0) {
                        region.StreetDTO = data;
                        $.each(that.selectedRegions, function (indexOfRegion, region) {
                            that.drawStreetsInMap(region.StreetDTO);
                        });
                        if (that.onStreetsLoaded) {
                            that.onStreetsLoaded();
                        }
                        if (callback)
                            callback();
                    }
                },
                error: function (request, textStatus, errorThrown) {
                    if (request.statusText === 'abort') {
                        return;
                    }
                    alert(request + '\n' + textStatus + '\n' + errorThrown);
                }
            });
            ajaxCalls.push(call);
        });

        return ajaxCalls;
    };

    saveRegions(callback) {
        if (!callback) throw "Error in saveRegions! Callback is missing!";
        jQuery.support.cors = true;
        $.each(this.selectedRegions, function (indexOfRegion, region) {
            $.ajax({
                url: '/Home/SaveSection',
                type: 'POST',
                dataType: 'json',
                data: region,
                statusCode:
                {
                    500: function (x, y, z) {
                        console.log("x: " + x);
                        console.log("y: " + y);
                        console.log("z: " + z);
                    },
                    403: function () { },
                    200: function (data) {
                        console.debug("saved!");
                    }
                }
            });
        });
    };

    getImagesFromSelectedRegion(interpolate, callback) {
        var that = this;

        var auxImagesVector = [];
        var streetThreads = 0;
        var auxFunc = function () {
            that.selectedRegions.map(function (i) { streetThreads += i.StreetDTO.length; });
            if (streetThreads > that.MAX_IMAGES_PER_REGION) {
                alert("Erro: Número de ruas maior que " + that.MAX_IMAGES_PER_REGION + " (" + streetThreads + "), por favor selecione uma região menor");
                return;
            }
            $.each(that.selectedRegions, function (idxRegion, region) {
                $.each(region.StreetDTO, function (idxStreet, Street) {
                    var auxGSPano = new GSPanoramaMiner(null, null, null);
                    auxGSPano.getImagesForStreet(Street, interpolate, function (status) {
                        auxImagesVector.push(Street);
                        //console.log(Street); console.log(status);
                        if (--streetThreads === 0) {
                            that.originalImages = [];
                            GSDrawer.pictureIndex = 0;

                            $.each(auxImagesVector, function (idxImagesVector, ImagesVector) {
                                //that.originalImages.push(ImagesVector);
                                for (let j = 0; j < ImagesVector.Trechos.length; j++) {
                                    for (let i = 0; i < ImagesVector.Trechos[j].length; i++) {
                                        var ponto = ImagesVector.Trechos[j][i];
                                        var panoDTO = ponto.PanoramaDTO;
                                        if (panoDTO && panoDTO.pano) {
                                            panoDTO.Pictures[0].filterResults = [];
                                            that.originalImages.push(panoDTO.Pictures[0]);
                                        }
                                    }
                                }
                                if (that.getImgIndex() >= that.originalImages.length) {
                                    that.setImgIndex(0);
                                }
                                if (that.originalImages && that.originalImages.length > 0) {
                                    that.startImagePresentation("originalSet");
                                    var type = "originalSet";
                                    that.getImagesMetaData()[type] = true;
                                }
                            });
                        }
                    });
                    //auxImagesVector.push(that.originalImages);
                });
            });
        };

        //TODO: Tratar para todas as regiões selecionadas
        if (!that.selectedRegions[0].StreetDTO) {
            that.getStreetsInRegions(function () {
                auxFunc();
                if (callback)
                    callback();
            });
        }
        else {
            return auxFunc();
        }

    };

    clearImagePresentation() {
        if (this.imgPresenter) {
            clearInterval(this.imgPresenter);
            this.imgPresenter = null;
        }
        if (this.onClearImagePresentation) {
            this.onClearImagePresentation();
        }
    };

    startImagePresentation(chosenSet) {
        this.clearImagePresentation();
        if (!!this.imgPreview.onload & !!this.imgPreview.onerror) {
            this.imgPreview.onload = this.imgPreview.onerror = null;
        }
        this.imageType = chosenSet;

        this.setImgPresenter();
    };

    nextImage() {
        var actualImgIndex = this.getImgIndex();
        actualImgIndex++;
        actualImgIndex %= this.originalImages.length;
        this.setImgIndex(actualImgIndex);
    };

    previousImage() {
        var actualImgIndex = this.getImgIndex();
        actualImgIndex--;
        actualImgIndex += this.originalImages.length;
        actualImgIndex %= this.originalImages.length;
        this.setImgIndex(actualImgIndex);
    };

    setImgPresenter() {
        if (!this.imgPreview || !this.originalImages || this.originalImages.length === 0) {
            //TODO: Tratamento de quando o timer é chamado mas não há imagens para apresentar
            return;
        }

        //this.imgPresenter = setInterval(function () {
        this.setImgPresentationPosition(this.getImgIndex());
        this.nextImage();
        //}.bind(this), 1000);
        if (this.onImagePresentation) {
            this.onImagePresentation();
        }
    };

    setImgByUrl(imageUrl) {
        //that = this;
        this.imgPreview.src = imageUrl;
        if (this.onImageChanged) {
            this.onImageChanged();
        }
    };

    setImgByBase64Data(base64image) {
        this.imgPreview.src = "data:image/png;base64," + base64image;
        if (this.onImageChanged) {
            this.onImageChanged();
        }
    };

    play() {
        this.autoplay = true;
        this.setImgPresentationPosition(this.getImgIndex());
    };

    pause() {
        this.autoplay = false;
        this.clearImagePresentation();
        if (this.onPause) {
            this.onPause();
        }
    };

    setImgPresentationPosition(pos) {
        var that = this;

        if (!Number.isInteger(pos)) {
            pos = that.getImgIndex();
        }
        else {
            that.setImgIndex(pos);
        }

        if (!that.imgPreview.onload) {
            that.imgPreview.onload = function (e) {
                console.log("Imagem de index: " + that.getImgIndex() + " carregada.");
                if (that.autoplay) {
                    that.imgPresenter = setTimeout(function () {
                        that.setImgPresentationPosition(that.getImgIndex());
                        that.nextImage();
                    }, 1000);
                }
            };
        }
        if (!that.imgPreview.onerror) {
            that.imgPreview.onerror = function (e) {
                console.log("Erro na imagem de index: " + that.getImgIndex());
                that.setImgPresentationPosition(that.getImgIndex());
                that.nextImage();
            };
        }

        if (this.imageType === 'originalSet') {
            //this.imgPreview.src = this.originalImages[pos].imageURI;
            this.setImgByUrl(this.originalImages[pos].imageURI);
        }
        else if (this.imageType === 'amenitiesSet') {
            this.setImgByBase64Data(this.amenityMarkers[pos].filterResults[this.imageType].base64image);
            return;
        }
        else {
            //this.imgPreview.src = "data:image/png;base64," + this.originalImages[pos].filterResults[this.imageType].base64image;
            if (this.originalImages[pos].filterResults)
                this.setImgByBase64Data(this.originalImages[pos].filterResults[this.imageType].base64image);
        }
        if (this.snapin === true) {
            this.getMap().setCenter(this.originalImages[pos].location);
            this.getMap().setZoom(21);
        }
        this.setImagePinPoint(this.originalImages[pos]);
    };

    setFilteredSet(filterDTOs, type) {
        var that = this;

        that.getImagesMetaData()[type] = true;
        $.each(filterDTOs, function (ip, p) {
            that.originalImages[p.imageID].filterResults[type] = p;
        });
        that.originalImages = that.originalImages.filter(function (el) {
            return el.filterResults !== null && el.filterResults[type] !== null;
        });
    };

    setFilteredImage(filterDTO, type) {
        var that = this;

        that.getImagesMetaData()[type] = true;
        that.originalImages[filterDTO.imageID].filterResults[type] = filterDTO;
    }

    //TODO: Transfer this to the "frontend image manager" and make it private
    loadImagesFromStreetIntoArray() {
        var that = this;
        //pano = null;
        that.originalImages = [];
        for (let j = 0; j < that.getSelectedStreet().Trechos.length; j++) {
            for (let i = 0; i < that.getSelectedStreet().Trechos[j].length; i++) {
                var ponto = that.getSelectedStreet().Trechos[j][i];
                var panoDTO = ponto.PanoramaDTO;
                if (panoDTO.pano) {
                    panoDTO.Pictures[0].filterResults = [];
                    that.originalImages.push(panoDTO.Pictures[0]);
                }
            }
        }
        if (that.getImgIndex() >= that.originalImages.length) {
            that.setImgIndex(0);
        }
        if (that.originalImages && that.originalImages.length > 0) {
            that.startImagePresentation("originalSet");
            var type = "originalSet";
            that.getImagesMetaData()[type] = true;
        }

    };
}





