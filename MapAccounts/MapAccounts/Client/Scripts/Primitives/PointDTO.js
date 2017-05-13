function PointDTO()
{
    this.ID = -1;
    this.lat = -1;
    this.lng = -1;
}

PointDTO.initialize = function(_ID, _lat, _lng)
{
    var ret = new PointDTO();
    ret.ID = _ID;
    ret.lat = _lat;
    ret.lng = _lng;
    return ret;
}