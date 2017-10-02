class GSPanoramaMiner {
    constructor(_currentStreet, _completedCallback, _rangeForPanorama) {
        this.validPanoramas = [];
        this.currentThreads = 0;
        this.concatenatedPoints = [];
        this.rangeForPanorama = _rangeForPanorama;
        this.currentStreet = _currentStreet;
        this.completedCallback = _completedCallback;
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