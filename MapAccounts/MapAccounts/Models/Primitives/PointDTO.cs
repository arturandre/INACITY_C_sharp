namespace MapAccounts.Models.Primitives
{
    public class PointDTO
    {
        public PointDTO() { }

        public PointDTO(StreetPointModel point)
        {
            ID = point.ID;
            lat = point.lat;
            lng = point.lng;
        }

        public string ID { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public PanoramaDTO PanoramaDTO { get; set; }
    }
}