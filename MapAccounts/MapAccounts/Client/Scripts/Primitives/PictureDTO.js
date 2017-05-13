function PictureDTO()
{
    this.ID = -1;
    this.heading = -1;
    this.base64image = "";
    this.imageURI = "";
    this.location = {};
    this.filterResults = null;
}

PictureDTO.initialize = function (_ID, _heading, _base64image, _imageURI, _location, _filterResults)
{
    var ret = new PictureDTO();
    ret.ID = _ID;
    ret.heading = _heading;
    ret.base64image = _base64image;
    ret.imageURI = _imageURI;
    ret.location = _location;
    ret.filterResults = _filterResults;
    return ret;
}