using System;

namespace MapAccounts.Models.Primitives
{
    public class FilterResultDTO
    {
        public FilterResultDTO()
        {
            this.imageID = -1;
            this.panoID = null;
        }

        public enum CaracteristicType
        {
            Trees,
            Cracks,
            Generic

        }
        //public PointDTO Location { get; set; }
        public int imageID { get; set; }
        public String panoID { get; set; }
        public CaracteristicType Type { get; set; }
        public String base64image { get; set; }
        public Boolean IsCaracteristicPresent { get; set; }
        public Double? Density { get; set; }
        public int ProcessedArea { get; set; }
    }
}