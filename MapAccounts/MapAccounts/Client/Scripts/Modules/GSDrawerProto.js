

GSDrawer.prototype.focusStreetOnMap = function (Street) {
    if (this.getSelectedStreet() === Street) return;
    if (this.getSelectedStreet() && !confirm(getResourceString("CHANGE_ADDRESS_WARN")))
    {
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
        p.setMap(that.getMap());
        p.setOptions({ strokeColor: '#0000FF' });
        google.maps.event.addListener(p, 'click', function () {
            that.focusStreetOnMap(Street);
        });

    });
    this.polylinesOnMap[Street.Name] = polylines;
    if (this.onStreetFocused) {
        this.onStreetFocused(this);
    }
};

GSDrawer.prototype.clearPolylinesOnMap = function () {
    for (var polyline in this.polylinesOnMap) {
        $.each(this.polylinesOnMap[polyline], function (i, segment) {
            segment.setMap(null);
        });
    }
    this.polylinesOnMap = [];
    if (this.onStreetsUnloaded) this.onStreetsUnloaded();
    this.setSelectedStreet(null);
};

GSDrawer.prototype.resetStreetColor = function (Street) {
    $.each(this.polylinesOnMap[Street.Name], function (i, segment) {
        segment.setOptions({ strokeColor: Street.color });
    });
};

GSDrawer.prototype.drawStreetsInMap = function (streets, mColor) {
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

GSDrawer.streetToGSPolylineArray = function (Street, mColor) {
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

GSDrawer.prototype.clearRegions = function () {
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

GSDrawer.prototype.getRegion = function () {
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
        return region;
    }
};

GSDrawer.rColor = function () {
    var text = "";
    var possible = "AB0123456789";
    var desiredLength = 6;

    for (var i = 0; i < desiredLength; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));

    return '#' + text;
};

GSDrawer.prototype.areStreetsLoaded = function () {
    return this.selectedRegions.length > 0 && gsdrawer.selectedRegions[0].StreetDTO.length > 0;
};

GSDrawer.prototype.getStreetsInRegions = function (callback) {
    //if (!callback) throw "Error in getStreetsInRegions! Callback missing!";
    that = this;
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

GSDrawer.prototype.saveRegions = function (callback) {
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

GSDrawer.prototype.getImagesFromSelectedRegion = function (interpolate, callback) {
    var that = this;

    var auxImagesVector = [];
    var streetThreads = 0;
    var auxFunc = function () {
        gsdrawer.selectedRegions.map(function (i) { streetThreads += i.StreetDTO.length; });
        if (streetThreads > 100)
        {
            alert("Erro: Número de ruas maior que 100 (" + streetThreads + "), por favor selecione uma região menor");
            return;
        }
        $.each(that.selectedRegions, function (idxRegion, region) {
            $.each(region.StreetDTO, function (idxStreet, Street) {
                that.getImagesForStreet(Street, interpolate, function (status) {
                    auxImagesVector.push(Street);
                    //console.log(Street); console.log(status);
                    if (--streetThreads === 0) {
                        that.originalImages = [];

                        $.each(auxImagesVector, function (idxImagesVector, ImagesVector) {
                            //that.originalImages.push(ImagesVector);
                            for (j = 0; j < ImagesVector.Trechos.length; j++) {
                                for (i = 0; i < ImagesVector.Trechos[j].length; i++) {
                                    var ponto = ImagesVector.Trechos[j][i];
                                    var panoDTO = ponto.PanoramaDTO;
                                    if (panoDTO && panoDTO.pano) {
                                        panoDTO.Pictures[0].filterResults = {};
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

GSDrawer.prototype.getImagesForStreet = function (Street, interpolate, callback, _maxDistance, _maxRadius) {
    if (!callback) throw "Error in getImagesForStreet! Callback is missing!";

    var maxDistance = _maxDistance ? _maxDistance : 10;
    var maxRadius = _maxRadius ? _maxRadius : 10;

    jQuery.support.cors = true;



    //INTERPOLATE STREET'S POINTS
    if (interpolate) {
        for (var iTrecho = 0; iTrecho < Street.Trechos.length; iTrecho++) {
            var trecho = Street.Trechos[iTrecho];
            var points = trecho.slice(0);
            for (var i = 0; i < points.length - 1; i++) {
                var p1 = points[i];
                var p2 = points[i + 1];
                var pm1 = p1;
                var pi = [];
                do {
                    var updated = false;
                    var pm2 = p2;
                    while (distance(pm1, pm2) > maxDistance) {
                        pm2 = midpoint(pm1, pm2);
                        updated = true;
                    }
                    if (updated) {
                        pi.push(pm2);
                        pm1 = pm2;
                    }
                } while (updated);
                Street.Trechos[iTrecho].splice.apply(Street.Trechos[iTrecho], [i + 1, 0].concat(pi));
            }
        }
    }
    //INTERPOLATE STREET'S POINTS

    //GET Panorama information

    var panoramaCallback = function () {
        var zeroImage = 0;
        var wrongImage = 0;
        var validImage = 0;
        $.each(Street.Trechos, function (iTrecho, Trecho) {
            $.each(Trecho, function (idxPoint, Point) {
                var vPano = Point.panoramaStatus;
                if (vPano === 'ZERO_RESULTS' || !vPano) zeroImage++;
                else if (vPano === 'WRONG_STREET') wrongImage++;
                else validImage++;
            });
        });
        var ret_status = {
            "validImage": validImage,
            "wrongImage": wrongImage,
            "zeroImage": zeroImage
        };
        if (validImage === 0) {
            callback(ret_status);
            return;
        }
        //gsdrawer.resetStreetColor(Street);
        var pictureIndex = 0;
        var trechos = Street.Trechos;
        for (var j = 0; j < trechos.length; j++) {
            var points = trechos[j];
            for (var i = 0; i < points.length - 1; i++) {
                var point = points[i];
                var nextPoint = points[i + 1];
                //if (!point.PanoramaDTO) point.PanoramaDTO = {};
                //if (point.PanoramaDTO.Pictures === null) point.PanoramaDTO.Pictures = [];
                var pano = point.PanoramaDTO.pano;
                if (point.panoramaStatus === "OK"){
                ///if (!!pano && pano.length === 22) {
                    var finalURL = 'http://maps.googleapis.com/maps/api/streetview?size=640x640&pano=' +
                        pano + '&heading=' +
                        point.PanoramaDTO.frontAngle +
                        '&pitch=' + point.PanoramaDTO.pitch +
                        '&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac&sensor=false';
                    //var finalURL = 'https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=' + pano + '&output=tile&x=1&y=0&zoom=2&nbt&fover=2&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac';
                    //var finalURL = 'https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid='+pano+'&output=tile&x=1&y=1&zoom=2&nbt&fover=2&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac'
                    //var finalURL = 'https://cbks1.googleapis.com/cbk?output=tile&cb_client=apiv3&v=4&gl=US&zoom=3&x=3&y=1&panoid=' + pano + '&fover=2&onerr=3&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac';
                    //var finalURL = 'https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=' + pano + '&output=tile&x=6&y=3&zoom=4&nbt&fover=2&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac';
                    var picture = PictureDTO.initialize(pictureIndex++, pano, point.PanoramaDTO.frontAngle, null, finalURL,
                        PointDTO.initialize(point.ID, point.lat, point.lng), null);

                    point.PanoramaDTO.Pictures.push(picture);
                    Street.imagesLoaded = true;
                }
            }
            var lastPoint = points[points.length - 1];
            if (!lastPoint.PanoramaDTO) lastPoint.PanoramaDTO = {};
            var secondToLastPoint = points[points.length - 2];
            var lastAngle = secondToLastPoint.PanoramaDTO.frontAngle;
            if (lastPoint.PanoramaDTO.Pictures === null) lastPoint.PanoramaDTO.Pictures = [];

            var lastPano = lastPoint.PanoramaDTO.pano;
            if (lastPano) {
                //var finalURL = 'https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=' + lastPano + '&output=tile&x=6&y=3&zoom=4&nbt&fover=2&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac';
                finalURL = 'http://maps.googleapis.com/maps/api/streetview?size=640x640&pano=' +
                    lastPano + '&heading=' +

                    lastPoint.PanoramaDTO.frontAngle +
                    '&pitch=' + point.PanoramaDTO.pitch +
                    '&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac&sensor=false';




                picture = PictureDTO.initialize(pictureIndex++, lastPano, lastPoint.PanoramaDTO.frontAngle, null, finalURL,
                    PointDTO.initialize(lastPoint.ID, lastPoint.lat, lastPoint.lng), null);

                lastPoint.PanoramaDTO.Pictures.push(picture);
                Street.imagesLoaded = true;
            }
        }
        
        callback(ret_status);

    };

    var gspano = new GSPanoramaMiner(Street,
        panoramaCallback, maxRadius);
    gspano.getPanoramasForStreet();

};

//Private
GSDrawer.prototype.loadImagesFromStreetIntoArray = function () {
    var that = this;
    //pano = null;
    that.originalImages = [];
    for (j = 0; j < that.getSelectedStreet().Trechos.length; j++) {
        for (i = 0; i < that.getSelectedStreet().Trechos[j].length; i++) {
            var ponto = that.getSelectedStreet().Trechos[j][i];
            var panoDTO = ponto.PanoramaDTO;
            if (panoDTO.pano) {
                panoDTO.Pictures[0].filterResults = {};
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

GSDrawer.prototype.clearImagePresentation = function () {
    if (this.imgPresenter) {
        clearInterval(gsdrawer.imgPresenter);
        this.imgPresenter = null;
    }
    if (this.onClearImagePresentation)
    {
        this.onClearImagePresentation();
    }
};

GSDrawer.prototype.startImagePresentation = function (chosenSet) {
    this.clearImagePresentation();
    if (!!this.imgPreview.onload & !!this.imgPreview.onerror) {
        this.imgPreview.onload = this.imgPreview.onerror = null;
    }
    this.imageType = chosenSet;

    this.setImgPresenter();
};

GSDrawer.prototype.nextImage = function () {
    var actualImgIndex = this.getImgIndex();
    actualImgIndex++;
    actualImgIndex %= this.originalImages.length;
    this.setImgIndex(actualImgIndex);
};

GSDrawer.prototype.previousImage = function () {
    var actualImgIndex = this.getImgIndex();
    actualImgIndex--;
    actualImgIndex += this.originalImages.length;
    actualImgIndex %= this.originalImages.length;
    this.setImgIndex(actualImgIndex);
};

GSDrawer.prototype.setImgPresenter = function () {
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

GSDrawer.prototype.setImgByUrl = function (imageUrl) {
    that = this;
    that.imgPreview.src = imageUrl;
    if (that.onImageChanged) {
        that.onImageChanged();
    }
};

GSDrawer.prototype.setImgByBase64Data = function (base64image) {
    this.imgPreview.src = "data:image/png;base64," + base64image;
    if (this.onImageChanged) {
        this.onImageChanged();
    }
};

GSDrawer.prototype.play = function () {
    this.autoplay = true;
    this.setImgPresentationPosition(this.getImgIndex());
};

GSDrawer.prototype.pause = function () {
    this.autoplay = false;
    this.clearImagePresentation();
    if (this.onPause) {
        this.onPause();
    }
};

GSDrawer.prototype.setImgPresentationPosition = function (pos) {
    var that = this;

    if (!pos) {
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
        this.getMap().setCenter(gsdrawer.originalImages[pos].location);
        this.getMap().setZoom(21);
    }
    this.setImagePinPoint(this.originalImages[pos]);
};

GSDrawer.prototype.setFilteredSet = function (filterDTOs, type) {
    var that = this;

    that.getImagesMetaData()[type] = true;
    $.each(filterDTOs, function (ip, p) {
        that.originalImages[p.imageID].filterResults[type] = p;
    });
    that.originalImages = that.originalImages.filter(function (el) {
        return el.filterResults !== null && el.filterResults[type] !== null;
    });

};