function PictureDTO()
{
	this.imageID = -1;
    this.panoID = -1;
    this.heading = -1;
    this.base64image = "";
    this.imageURI = "";
    this.location = {};
    this.filterResults = null;
}

PictureDTO.initialize = function (_imageID, _panoID, _heading, _base64image, _imageURI, _location, _filterResults) {
	var ret = new PictureDTO();
	ret.imageID = _imageID;
	ret.panoID = _panoID;
	ret.heading = _heading;
	ret.base64image = _base64image;
	ret.imageURI = _imageURI;
	ret.location = _location;
	ret.filterResults = _filterResults;
	return ret;
};