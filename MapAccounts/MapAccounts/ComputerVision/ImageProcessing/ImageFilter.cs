using Emgu.CV;
using Emgu.CV.Structure;
using MapAccounts.Models.Primitives;

namespace MapAccounts.ComputerVision.ImageProcessing
{
    public abstract class ImageFilter
    {
        public abstract FilterResultDTO.CaracteristicType FilterType { get; }
        public abstract FilterResultDTO filterImage(Image<Bgr, byte> Image);
    }
}
