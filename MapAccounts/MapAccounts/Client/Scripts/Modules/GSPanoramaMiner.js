function GSPanoramaMiner() {
    this.validPanoramas = [];
    this.currentThreads = 0;
    this.concatenatedPoints = [];
    this.currentStreet = null;
    this.completedCallback = null;
}

GSPanoramaMiner.prototype.getPanoramasForStreet = function (Street, completedCallback) {

    this.currentStreet = Street;
    this.completedCallback = completedCallback;
    var that = this;
    that.concatenatedPoints = [];
    $.each(Street.Trechos, function (iTrecho, Trecho) {
        that.concatenatedPoints = that.concatenatedPoints.concat(Trecho);
    });
    //that.concatenatedPoints = Street.Trechos;
    this.Compare(0, this.concatenatedPoints.length - 1);
};

GSPanoramaMiner.prototype.Compare = function (s, e) {
    this.currentThreads++;
    if (s >= e) {
        this.getPanoramaId(this.concatenatedPoints[s], function (data, status) {
            if (status === "OK") {
                if (GSPanoramaMiner.stringDistance(data.location.description, GSPanoramaMiner.normalizeStreetName(this.currentStreet.Name)) < 0.9) {
                    this.validPanoramas[s] = 'WRONG_STREET';
                }
                else {
                    this.validPanoramas[s] = { pano: data.location.pano, result: data };
                }
            }
            else {
                this.validPanoramas[s] = status;
            }
        }.bind(this));
    }
    var se = s + Math.floor((e - s) / 2);

    var pminf = (!!this.validPanoramas[s]);
    var pmeanf = (!!this.validPanoramas[se]);
    var pmaxf = (!!this.validPanoramas[e]);

    var pmin = this.validPanoramas[s];
    var pmean = this.validPanoramas[se];
    var pmax = this.validPanoramas[e];

    if (s < se && !pmin)
        this.getPanoramaId(this.concatenatedPoints[s], function (data, status) {
            pminf = true;
            if (status === "OK") {
                if (GSPanoramaMiner.stringDistance(data.location.description, GSPanoramaMiner.normalizeStreetName(this.currentStreet.Name)) < 0.9) {
                    pmin = 'WRONG_STREET';
                }
                else {
                    pmin = { pano: data.location.pano, result: data };
                }
            }
            else {
                pmin = status;
            }
            if (pmeanf) {
                if (pmin != pmean || status !== "OK") {
                    if (se - s == 1) {
                        this.validPanoramas[s] = pmin;
                        this.validPanoramas[se] = pmean;
                    }
                    else
                        this.Compare(s, se);
                }
                else {
                    for (var i = s; i <= se; i++)
                        this.validPanoramas[i] = pmin;
                }
            }
        }.bind(this));

    if (!pmean)
        this.getPanoramaId(this.concatenatedPoints[se], function (data, status) {
            pmeanf = true;
            if (status === "OK") {
                if (GSPanoramaMiner.stringDistance(data.location.description, GSPanoramaMiner.normalizeStreetName(this.currentStreet.Name)) < 0.9) {
                    pmean = 'WRONG_STREET';
                }
                else {
                    pmean = { pano: data.location.pano, result: data };
                }
            }
            else {
                pmean = status;
            }
            if (s < se)
                if (pminf) {
                    if (pmin != pmean || status !== "OK") {
                        if (se - s == 1) {
                            this.validPanoramas[s] = pmin;
                            this.validPanoramas[se] = pmean;
                        }
                        else
                            this.Compare(s, se);
                    }
                    else {
                        for (var i = s; i <= se; i++)
                            this.validPanoramas[i] = pmin;
                    }
                }
            if (se < e)
                if (pmaxf) {
                    if (pmean != pmax || status !== "OK") {
                        if (e - se == 1) {
                            this.validPanoramas[se] = pmean;
                            this.validPanoramas[e] = pmax;
                        }
                        else
                            this.Compare(se + 1, e);
                    }
                    else {
                        for (var i = se + 1; i <= e; i++)
                            this.validPanoramas[i] = pmean;
                    }
                }
        }.bind(this));

    if (se < e && !pmax)
        this.getPanoramaId(this.concatenatedPoints[e], function (data, status) {
            pmaxf = true;
            if (status === "OK") {
                if (GSPanoramaMiner.stringDistance(data.location.description, GSPanoramaMiner.normalizeStreetName(this.currentStreet.Name)) < 0.9) {
                    pmax = 'WRONG_STREET';
                }
                else {
                    pmax = { pano: data.location.pano, result: data };
                }
            }
            else {
                pmax = status;
            }
            if (pmeanf) {
                if (pmean != pmax || status !== "OK") {
                    if (e - se == 1) {
                        this.validPanoramas[se] = pmean;
                        this.validPanoramas[e] = pmax;
                    }
                    else
                        this.Compare(se + 1, e);
                }
                else {
                    for (var i = se + 1; i <= e; i++)
                        this.validPanoramas[i] = pmean;
                }
            }
        }.bind(this));
    if (--this.currentThreads == 0) {
        this.cleanResults();
    }
};

GSPanoramaMiner.prototype.getPanoramaId = function (StreetPointDTO, callback, _distance) {
    this.currentThreads++;
    var distance = !!_distance ? _distance : 5;
    var sv = new google.maps.StreetViewService();
    //sv.getPanorama({ location: StreetPointDTO, radius: 5 }, function (data, status) {
    sv.getPanoramaByLocation(StreetPointDTO, distance, function (data, status) {
        callback(data, status);
        if (--this.currentThreads == 0) {
            this.cleanResults();
        }
    }.bind(this));
};

GSPanoramaMiner.prototype.getPanoramaUrl = function (latLng, _heading, _pitch, callback) {
    var returnURL = "";
    var returnData = null;
    var heading = !!_heading ? _heading : 0;
    var pitch = !!_pitch ? _pitch : 0;
    this.getPanoramaId(latLng, function (data, status) {
        if (status === "OK") {
            returnURL = 'http://maps.googleapis.com/maps/api/streetview?size=640x640&pano=' +
                    data.location.pano +
                    '&heading=' + heading +
                    '&pitch=' + pitch +
                    '&key=AIzaSyCzw_81uL52LSQVYvXEpweaBsr3m - xHYac&sensor=false';
            returnData = data;
            callback(returnUrl, returnData);
        }
    }, 20);
};

GSPanoramaMiner.prototype.cleanResults = function () {
    var aux = [];
    var last = "";
    var tIndex = 0;
    var count = 0;
    $.each(this.validPanoramas, function (i, p) {
        if (!!p && !!p.pano && p.pano != "ZERO_RESULTS" && p.pano != last & p.pano.length == 22) {
            var panorama = new PanoramaDTO();
            panorama.pano = p.pano;
            panorama.frontAngle = p.result.tiles.originHeading;
            panorama.pitch = p.result.tiles.originPitch;
            while (i >= this.currentStreet.Trechos[tIndex].length + count) {
                count += this.currentStreet.Trechos[tIndex].length;
                tIndex++;
            }
            this.currentStreet.Trechos[tIndex][i - count].PanoramaDTO = panorama;
            last = p.pano;
        }
    }.bind(this));

    if (this.completedCallback) {
        this.completedCallback();
    }
}

GSPanoramaMiner.normalizeStreetName = function (streetName) {
    return streetName
        .replace('Avenida ', '')
        .replace('Rua ', '')
        .replace('Estrada ', '')
        .replace('Senhor ', '')
        .replace('Senhora ', '')
        .replace('Doutor ', '')
        .replace('Doutora ', '')
        .replace('Marechal ', '')
        .replace('Av. ', '')
        .replace('R. ', '')
        .replace('Estr. ', '')
        .replace('Sr. ', '')
        .replace('Sra. ', '')
        .replace('Dr. ', '')
        .replace('Dra. ', '')
        .replace('Mal. ', '');

};

//lcs
GSPanoramaMiner.stringDistance = function (x, y) {
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
        for (diag = 0, j = 0; j < n; j++, diag = latch) {
            latch = row[j];
            if (x[i] == y[j]) { row[j] = diag + 1; }
            else {
                left = row[j - 1] || 0;
                if (left > row[j]) { row[j] = left; }
            }
        }
    }
    i--, j--;
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
    z = lcs.join('').length;
    return Math.max(z / x.length, z / y.length);
};