function GSHeatMap() {

    var map = null;

    /* Data points defined as an array of LatLng objects */
    this.heatmapData = [
  new google.maps.LatLng(37.782, -122.447),
  new google.maps.LatLng(37.782, -122.445),
  new google.maps.LatLng(37.782, -122.443),
  new google.maps.LatLng(37.782, -122.441),
  new google.maps.LatLng(37.782, -122.439),
  new google.maps.LatLng(37.782, -122.437),
  new google.maps.LatLng(37.782, -122.435),
  new google.maps.LatLng(37.785, -122.447),
  new google.maps.LatLng(37.785, -122.445),
  new google.maps.LatLng(37.785, -122.443),
  new google.maps.LatLng(37.785, -122.441),
  new google.maps.LatLng(37.785, -122.439),
  new google.maps.LatLng(37.785, -122.437),
  new google.maps.LatLng(37.785, -122.435)
    ];


    var heatmap = new google.maps.visualization.HeatmapLayer({
        data: this.heatmapData
    });


    this.sanFrancisco = new google.maps.LatLng(37.774546, -122.433523);

    this.setMap = function (_map) {
        map = _map;
        heatmap.setMap(map);
    }

    this.setData = function(locations, weights)
    {
        var formattedData = [];

        $.each(locations, function (i, v) {
            formattedData.push({location: new google.maps.LatLng(v.lat, v.lng), weight: 100*weights[i]});
        });

        heatmap.setData(formattedData);
    }
    //this.getMap = function () {
    //    return map;
    //}

}
