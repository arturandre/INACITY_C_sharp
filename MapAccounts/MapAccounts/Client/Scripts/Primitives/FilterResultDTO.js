function FilterResultDTO()
{
    this.imageID = -1;
    this.base64image = "";
    //PointDTO
    this.location = {};
    /*
    public enum CaracteristicType
        {
            Trees,
            Cracks

        }
    */
    this.type = null;
    this.isCaracteristicPresent = null;
    this.density = null;
    this.processedArea = null;
}

FilterResultDTO.prototype.initialize = function (_imageID, _base64image, _location, _type, _isCaracteristicPresent, _density, _processedArea)
{
    this.imageID = _imageID;
    this.base64image = _base64image;
    this.location = _location;
    this.type = _type;
    this.isCaracteristicPresent = _isCaracteristicPresent;
    this.density = _density;
    this.processedArea = _processedArea;
}