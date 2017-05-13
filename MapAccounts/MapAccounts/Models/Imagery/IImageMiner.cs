using System.Collections.Generic;

namespace MapAccounts.Models.Imagery
{
    interface IImageMiner
    {
        //void getImagesForPoints(IEnumerable<PointDTO> points);
        void getImagesForPoints(ICollection<StreetPointModel> streetPointModel);
    }
}
