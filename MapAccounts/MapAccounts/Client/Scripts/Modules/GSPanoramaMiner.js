class GSPanoramaMiner
{
    constructor(_currentStreet, _completedCallback, _rangeForPanorama) {
        this.validPanoramas = [];
        this.currentThreads = 0;
        this.concatenatedPoints = [];
        this.rangeForPanorama = _rangeForPanorama;
        this.currentStreet = _currentStreet;
        this.completedCallback = _completedCallback;
    }

    getPanoramaId(Point, callback)
    {
        var that = this;
        var sv = new google.maps.StreetViewService();
        sv.getPanoramaByLocation(Point, that.rangeForPanorama, function (data, status) {
            callback(data, status);
        });
    }

    getPanoramasForStreet() {
        var that = this;
        that.concatenatedPoints = [];
        that.currentStreet.Trechos.map(function (i) { that.currentThreads += i.length; });
        console.log(that.currentThreads);
        $.each(that.currentStreet.Trechos, function (iTrecho, Trecho) {
            $.each(Trecho, function (idxPoint, Point) {
                (function (Point) {
                    var sv = new google.maps.StreetViewService();
                    sv.getPanoramaByLocation(Point, that.rangeForPanorama, function (data, status) {
                        Point.panoramaStatus = status;
                        Point.PanoramaDTO = new PanoramaDTO();
                        if (Point.panoramaStatus === "OK") {
                            if (GSPanoramaMiner.stringDistance(GSPanoramaMiner.normalizeStreetName(data.location.description), GSPanoramaMiner.normalizeStreetName(that.currentStreet.Name)) < 0.9) {
                                Point.panoramaStatus = 'WRONG_STREET';
                            }
                            //Panorama's ID when correct have 22 characters length
                            else if (data.location.pano.length == 22) {
                                Point.PanoramaDTO.pano = data.location.pano;
                                Point.PanoramaDTO.frontAngle = data.tiles.originHeading;
                                Point.PanoramaDTO.pitch = data.tiles.originPitch;
                            }
                            else
                            {
                                Point.panoramaStatus = "WRONG_PANO";
                            }
                        }
                        if (--that.currentThreads == 0) {
                            that.cleanResults();
                            if (that.completedCallback) {
                                that.completedCallback();
                            }
                        }
                    });
                })(Point);
            });
        });
    };

    cleanResults() {
        var that = this;
        var last = "";

        $.each(that.currentStreet.Trechos, function (iTrecho, Trecho) {
            $.each(Trecho, function (idxPoint, Point) {

                if (Point.panoramaStatus === "OK") {
                    if (Point.PanoramaDTO.pano === last) {
                        Point.PanoramaDTO = new PanoramaDTO();
                        Point.panoramaStatus = "SAME_POINT";
                    }
                    else {
                        last = Point.PanoramaDTO.pano;
                    }
                }
            });
        });
    }


    getImagesForStreet(Street, interpolate, callback, _maxDistance, _maxRadius) {
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
                            //Geocoordinate.js -> midpoint
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
            var trechos = Street.Trechos;
            for (var j = 0; j < trechos.length; j++) {
                var points = trechos[j];
                for (var i = 0; i < points.length - 1; i++) {
                    var point = points[i];
                    var nextPoint = points[i + 1];
                    var pano = point.PanoramaDTO.pano;
                    if (point.panoramaStatus === "OK") {
                        var finalURL = 'http://maps.googleapis.com/maps/api/streetview?size=640x640&pano=' +
                            pano + '&heading=' +
                            point.PanoramaDTO.frontAngle +
                            '&pitch=' + point.PanoramaDTO.pitch +
                            '&key=' + gsvkey /* gsvkey@Index.cshtml */
                            +
                            '&sensor=false';
                        //var finalURL = 'https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=' + pano + '&output=tile&x=1&y=0&zoom=2&nbt&fover=2&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac';
                        //var finalURL = 'https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid='+pano+'&output=tile&x=1&y=1&zoom=2&nbt&fover=2&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac'
                        //var finalURL = 'https://cbks1.googleapis.com/cbk?output=tile&cb_client=apiv3&v=4&gl=US&zoom=3&x=3&y=1&panoid=' + pano + '&fover=2&onerr=3&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac';
                        //var finalURL = 'https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=' + pano + '&output=tile&x=6&y=3&zoom=4&nbt&fover=2&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac';
                        var picture = PictureDTO.initialize(GSDrawer.pictureIndex++, pano, point.PanoramaDTO.frontAngle, null, finalURL,
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
                        '&key=' + gsvkey /* gsvkey@Index.cshtml */
                        +'&sensor=false';




                    picture = PictureDTO.initialize(GSDrawer.pictureIndex++, lastPano, lastPoint.PanoramaDTO.frontAngle, null, finalURL,
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

    static normalizeStreetName(streetName) {
        return streetName.normalize('NFD').replace(/[\u0300-\u036f]/g, "")
            .replace('Avenida ', '')
            .replace('Rua ', '')
            .replace('Estrada ', '')
            .replace('Senhor ', '')
            .replace('Senhora ', '')
            .replace('Doutor ', '')
            .replace('Doutora ', '')
            .replace('Prefeito ', '')
            .replace('Marechal ', '')
            .replace('Professor ', '')
            .replace('Professora ', '')
            .replace('Av. ', '')
            .replace('R. ', '')
            .replace('Estr. ', '')
            .replace('Sr. ', '')
            .replace('Sra. ', '')
            .replace('Dr. ', '')
            .replace('Dra. ', '')
            .replace('Pref. ', '')
            .replace('Mal. ', '')
            .replace('Prof. ', '');;


    };

    //lcs
    static stringDistance(x, y) {
        var s, i, j, m, n,
            lcs = [], row = [], c = [],
            left, diag, latch;
        //make sure shorter string is the column string
        if (m < n) { s = x; x = y; y = s; }
        m = x.length;
        n = y.length;
        //build the c-table
        for (j = 0; j < n; row[j++] = 0);
        for (i = 0; i < m; i++) {
            c[i] = row = row.slice();
            for (diag = 0, j = 0; j < n; j++ , diag = latch) {
                latch = row[j];
                if (x[i] == y[j]) { row[j] = diag + 1; }
                else {
                    left = row[j - 1] || 0;
                    if (left > row[j]) { row[j] = left; }
                }
            }
        }
        i-- , j--;
        //row[j] now contains the length of the lcs
        //recover the lcs from the table
        while (i > -1 && j > -1) {
            switch (c[i][j]) {
                default: j--;
                    lcs.unshift(x[i]);
                case (i && c[i - 1][j]): i--;
                    continue;
                case (j && c[i][j - 1]): j--;
            }
        }
        var z = lcs.join('').length;
        return Math.max(z / x.length, z / y.length);
    };

}