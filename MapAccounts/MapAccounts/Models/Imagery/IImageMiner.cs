using System.Collections.Generic;

namespace MapAccounts.Models.Imagery
{
    public interface IImageMiner
    {
        void getImagesForPoints(ICollection<StreetPointModel> streetPointModel);
        string getImageBase64(object param);
    }
}
