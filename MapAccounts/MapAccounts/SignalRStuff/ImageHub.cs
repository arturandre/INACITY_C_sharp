using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using MapAccounts.Models.Primitives;
using static MapAccounts.Models.Primitives.FilterResultDTO;
using MapAccounts.Managers;

namespace MapAccounts.SignalRStuff
{
    [HubName("ImageHub")]
    public class ImageHub : Hub<ISignalRClient>
    {
        public override Task OnConnected()
        {


            return base.OnConnected();
        }


        public async Task DetectFeaturesInSequence(IEnumerable<PictureDTO> pictures, String filterType, int jobId)
        {
            var connectionId = Context.ConnectionId;
            if (pictures == null) return;
            for (int i = 0; i < pictures.Count(); i++)
            {
                var p = pictures.ElementAt(i);
                p.base64image = (new Models.Imagery.Google.GSMiner()).DownloadBase64ImageFromURI(p.imageURI);
                if (p.base64image != null)
                {
                    ImageFilterManager.getInstance().detectFeatureInPictureDTO(ref p, (CaracteristicType)Enum.Parse(typeof(CaracteristicType), filterType));

                    try
                    {
                        Task.Run(() =>
                        {
                            ResultsStoreManager storage = new ResultsStoreManager();
                            storage.StoreHeatmapPoint(p, (CaracteristicType)Enum.Parse(typeof(CaracteristicType), filterType));
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Clients.Client(Context.ConnectionId).sendFilteredCollection(p.filterResults.ElementAt(0), p.filterResults.ElementAt(0).Type.ToString(), jobId);
                    p = null;
                }
            }
        }




    }

    public interface ISignalRClient
    {
        void sendFilteredCollection(FilterResultDTO filteredImage, string type, int jobId);
    }
}