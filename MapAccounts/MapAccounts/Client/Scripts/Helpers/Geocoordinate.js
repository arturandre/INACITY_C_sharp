function deg2rad(t) {
    return (t * Math.PI) / 180.0;
}

function angleBetweenPoints(p1, p2)
{
    var latA = deg2rad(p1.lat);
    var lngA = deg2rad(p1.lng);
    var latB = deg2rad(p2.lat);
    var lngB = deg2rad(p2.lng);
    var phi = Math.log(Math.tan((latB / 2.0) + (Math.PI / 4)) / Math.tan((latA / 2.0) + (Math.PI / 4)));
    var lon = Math.abs(lngA - lngB);
/*Rolamento*/
    var theta = Math.atan2(lon, phi);
    return (theta*180.0)/Math.PI;
}

function midpoint(p1, p2)
{
    var lat1 = deg2rad(p1.lat);
    var lng1 = deg2rad(p1.lng);
    var lat2 = deg2rad(p2.lat);
    var lng2 = deg2rad(p2.lng);

    var dLng = lng2 - lng1;

    var Bx = Math.cos(lat2) * Math.cos(dLng);
    var By = Math.cos(lat2) * Math.sin(dLng);
    var lat = Math.atan2(Math.sin(lat1) + Math.sin(lat2), Math.sqrt((Math.cos(lat1) + Bx) * (Math.cos(lat1) + Bx) + By * By));
    var lng = lng1 + Math.atan2(By, Math.cos(lat1) + Bx);

    lat = lat * 180.0 / Math.PI;
    lng = lng * 180.0 / Math.PI;

    return { lat: lat, lng: lng };
}

//http://www.movable-type.co.uk/scripts/latlong.html
function distance(p1, p2)
{
    var R = 6371008.8;
    var lat1 = deg2rad(p1.lat);
    var lng1 = deg2rad(p1.lng);
    var lat2 = deg2rad(p2.lat);
    var lng2 = deg2rad(p2.lng);
            
    var dLat = lat2 - lat1;
    var dLon = lng2 - lng1;
    var a = Math.sin(dLat / 2.0) * Math.sin(dLat / 2.0) +
            Math.sin(dLon / 2.0) * Math.sin(dLon / 2.0) * Math.cos(lat1) * Math.cos(lat2);
    var c = 2.0 * Math.atan2(Math.sqrt(a), Math.sqrt(1.0 - a));
    var d = R * c;
    return d;
}

//http://math.stackexchange.com/questions/1014010/how-would-i-calculate-the-area-of-a-rectangle-on-a-sphere-using-vertical-and-hor
function areaOld(p1, p2)
{
    var R = 6371008.8;
    var lat1 = deg2rad(p1.lat);
    var lng1 = deg2rad(p1.lng);
    var lat2 = deg2rad(p2.lat);
    var lng2 = deg2rad(p2.lng);

    var alfa = Math.abs(lat1 - lat2)/2;
    var beta = Math.abs(lng1 - lng2) / 2;
    var gama = Math.acos(Math.cos(alfa) * Math.cos(beta)) / 2;

    var a_ = Math.atan(Math.sin(beta) / Math.tan(alfa));
    var b_ = Math.atan(Math.sin(alfa) / Math.tan(beta));

    var c = Math.acos(Math.sin(a_) * Math.sin(b_) * Math.sin(gama) - Math.cos(a_) * Math.cos(b_));

    var c4 = (R * R) * (4 * c - 2 * Math.PI);
    return c4;
}

//http://math.stackexchange.com/questions/1205927/how-to-calculate-the-area-covered-by-any-spherical-rectangle
function area(p1, p2)
{
    var R = 6371008.8;
    var lat1 = deg2rad(p1.lat);
    var lng1 = deg2rad(p1.lng);
    var lat2 = deg2rad(p2.lat);
    var lng2 = deg2rad(p2.lng);

    var l = Math.abs(lng1 - lng2);
    var w = Math.abs(lat1 - lat2);

    return 4 * Math.asin(Math.tan(l / 2) * Math.tan(w / 2))*R*R;

}

function GSLatLngToJSON(latlngobj)
{
    return { lat: latlngobj.lat(), lng: latlngobj.lng() };
}