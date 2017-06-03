var tbAddress = document.getElementById("tbAddress");
//var btPictures = document.getElementById("btPictures");
var btPictures = document.getElementById("btPictures");
var btAmenitiesImages = document.getElementById("btAmenitiesImages");
var btSnapInMap = document.getElementById("btSnapInMap");
var btSaveSession = document.getElementById("btSaveSession");

var btStreets = document.getElementById("btStreets");
var lblMaxImages = document.getElementById("lblMaxImages");
var lblNImages = document.getElementById("lblNImages");

var btHeatMapStreetsToggle = document.getElementById("btHeatMapStreetsToggle");
var modeStreetHeatMap = false;

var divReports = document.getElementById("divReports");
var divAmenities = document.getElementById("divAmenities");
var divAmenitiesControls = document.getElementById("divAmenitiesControls");

var divStreetControls = document.getElementById("divStreetControls");
var divFilters = document.getElementById("divFilters");

var imgPreview = document.getElementById("imgPreview");

var sdNImages = document.getElementById("sdNImages");
var sdLocationPoint = document.getElementById("sdLocationPoint");

var btTreesFilter = document.getElementById("btTreesFilter");
var btCracksFilter = document.getElementById("btCracksFilter");
var btGenericFilter = document.getElementById("btGenericFilter");


var filterButtons = [btTreesFilter, btCracksFilter, btGenericFilter];

var btDownloadImages = document.getElementById("btDownloadImages");


var btSchool = document.getElementById("btSchool");
var btPharmacy = document.getElementById("btPharmacy");
var btBusStop = document.getElementById("btBusStop");
var btHospital = document.getElementById("btHospital");
var btTreesHeatMap = document.getElementById("btTreesHeatMap");


var divLoading = document.getElementById("divLoading");

/*
0 - Stopped
1 - Play
2 - Paused
*/
var statePlayPause = 0;

var cbInterpolatePoints = document.getElementById("cbInterpolatePoints");

var setPlayPauseState = function (state) {
    statePlayPause = state;
    switch (state) {
        //Stopped
        case 0:
            gsdrawer.clearImagePresentation();
            btAmenitiesImages.src = "Content/playbutton.png";
            break;
            //Playing
        case 1:
            gsdrawer.showAllAmenitiesImages();
            btAmenitiesImages.src = "Content/pausebutton.png";
            break;
            //Paused
        case 2:
            //TODO: Trocar esse clear por um pause que depois possa continuar de onde parou
            gsdrawer.clearImagePresentation();
            btAmenitiesImages.src = "Content/playbutton.png";
            break;
        default:
            //TODO: Tratar caso em que o estado é inválido
            break;
    }
}

function updateControls(step) {
    updateLocationPointSlider();
    for (var i = 0; i < filterButtons.length; i++) {
        filterButtons[i].style.border = '';
        filterButtons[i].disabled = false;
    }
    switch (step) {
        //Tela inicial do sistema -> Selecionar região
    	case 0:
    		$("#btHeatMapStreetsToggle").addClass("disabled");
    		$("#btPictures").siblings().addClass("disabled");
            gsdrawer.clearRegions();
            divAmenities.style.display = 'none';
            divStreetControls.style.display = 'none';
            btStreets.disabled = true;
            divReports.style.display = 'none';
            //btPictures.disabled = true;
            //btPictures.display = 'block';
            sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            divAmenitiesControls.style.display = 'none';
            break;
            //Região selecionada -> Coletar ruas | Amenities
    	case 1:
    		$("#btPictures").siblings().addClass("disabled");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            btStreets.disabled = false;
            divReports.style.display = 'none';
            //btPictures.disabled = true;
            //btPictures.display = 'block';
            sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            divAmenitiesControls.style.display = 'none';
            break;
            //Ruas coletadas -> Selecionar rua
    	case 2:
    		$("#btHeatMapStreetsToggle").addClass("disabled");
    		$("#btPictures").siblings().addClass("disabled");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            btStreets.disabled = true;
            divReports.style.display = 'none';
            //btPictures.disabled = false;
            //btPictures.display = 'block';
            sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            divAmenitiesControls.style.display = 'none';
            break;
            //Rua selecionada -> Ver imagens
    	case 3:
    		$("#btHeatMapStreetsToggle").addClass("disabled");
        	$("#btPictures").siblings().addClass("disabled");
        	divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            btStreets.disabled = true;
            divReports.style.display = 'block';
            //btPictures.disabled = false;
            //btPictures.display = 'block';
            sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            divAmenitiesControls.style.display = 'none';
            break;
            //Imagens coletadas -> Selecionar filtro(s)
    	case 4:
    		$("#btHeatMapStreetsToggle").addClass("disabled");
    		$("#btPictures").siblings().removeClass("disabled");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            //btStreets.disabled = true;
            divReports.style.display = 'block';
            //btPictures.disabled = true;
            //btPictures.display = 'none';
            sdLocationPoint.disabled = false;
            btDownloadImages.disabled = false;
            //divFilters.style.display = 'block';
            divAmenitiesControls.style.display = 'none';
            break;
            //Filtro selecionado -> Ver HeatMap | Interagir com slider de imagem
    	case 5:
    		$("#btHeatMapStreetsToggle").removeClass("disabled");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            btStreets.disabled = true;
            divReports.style.display = 'block';
            //btPictures.disabled = true;
            //btPictures.display = 'none';
            sdLocationPoint.disabled = false;
            btDownloadImages.disabled = false;
            //divFilters.style.display = 'block';
            divAmenitiesControls.style.display = 'none';
            break;
            //Vendo imagens de amenities (Pontos de ônibus e afins)
        case 6:
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            btStreets.disabled = false;
            divReports.style.display = 'none';
            //btPictures.disabled = true;
            //btPictures.display = 'none';
            sdLocationPoint.disabled = false;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';

            divAmenitiesControls.style.display = 'flex';
            break;
        default:
            break;
    }
}

$(".dropdown-menu > .btn").click(function () {
    $(this).addClass("active").siblings().removeClass("active");
});

function updateLocationPointSlider() {
    if (!gsdrawer || !gsdrawer.originalImages || gsdrawer.originalImages.length === 0) {
        divMapNavigator.style.display = 'none';
    }
    else {
        divMapNavigator.style.display = 'flex';
        sdLocationPoint.max = gsdrawer.originalImages.length - 1;
    }
}

//#region Time estimation
var timeWaitFactor = 6481829.0909;

function getEstimatedTimeForStreetsInArea(areaSize) { return areaSize / timeWaitFactor; }

function showTimeEstimationConfirmPopup(areaSize) {
    var estimatedTime = getEstimatedTimeForStreetsInArea(areaSize);
    if (estimatedTime > 3) {
        return confirm("O tempo estimado para esta consulta é de " + Math.ceil(estimatedTime) + " a " + Math.ceil(estimatedTime) * 2 + " segundos, deseja continuar?");
    }
    return true;
}

function showTimeEstimationFilterConfirmPopup() {
    var estimatedTime = 60;
    return confirm("O tempo estimado para esta consulta é de " + Math.ceil(estimatedTime) + " a " + Math.ceil(estimatedTime) * 2 + " segundos, deseja continuar?");
}
//#endregion Time estimation

//#region Ajax abort
var getStreetsInRegionAjaxArray = null;

//#endregion Ajax abort

//Ajax time variables
var start_time = null;

$(function () {
    $.getScript("Client/Scripts/Views/Tooltip.js", function () {
        //TODO: Handle script loading
    });
    $.getScript("Client/Scripts/Modules/ImageDownloader.js", function () {
        //TODO: Handle script loading
    });


    bindings();
    $(document).ajaxError(function (e, xhr) {
        if (xhr.status === 403) {
            var response = $.parseJSON(xhr.responseText);
            window.location = response.LogOnUrl;
        }
    });

    $(document).on({
        ajaxStart: function () {
            divLoading.style.display = 'block';
            start_time = new Date().getTime();
        },
        ajaxStop: function () {
            divLoading.style.display = 'none';
            btStreets.value = 'Coletar ruas';
            var request_time = new Date().getTime() - start_time;
            console.log("Ajax call took: " + request_time + " ms");
        }
    });
});

function bindings() {
    btSnapInMap.onclick = btSnapInMapClick;
    btAmenitiesImages.onclick = btAmenitiesImagesClick;
    btAddress.onclick = panMapByAddress;
    btStreets.onclick = btStreetsClick;
    btPictures.onclick = getImagesForStreetClick;
    btSaveSession.onclick = saveSessionClick;
    btTreesFilter.onclick = function (e) { getFilteredImages('Trees', e.currentTarget); };
    btCracksFilter.onclick = function (e) { getFilteredImages('Cracks', e.currentTarget); };
    btGenericFilter.onclick = function (e) { getFilteredImages('Generic', e.currentTarget); };
    btDownloadImages.onclick = downloadImages;
    btHeatMapStreetsToggle.onclick = toggleHeatMap;

    btSchool.onclick =
        btPharmacy.onclick =
        btHospital.onclick =
        btBusStop.onclick = mapNode;

    btTreesHeatMap.onclick = heatmapFeature;

    sdLocationPoint.oninput = sdLocationPoint.onchange = function (e) {
        gsdrawer.clearImagePresentation();
        gsdrawer.setImgPresentationPosition(e.currentTarget.value);
    };

    GSDrawer.initMap = function () {
        var myStyles = [
    {
        featureType: "poi",
        elementType: "labels",
        stylers: [
              { visibility: "off" }
        ]
    }
        ];
        mapOptions.styles = myStyles;
        gsdrawer.setMap(new google.maps.Map(document.getElementById("map"),
            mapOptions));
        $.each(gsdrawer.selectedRegions, function (index, region) {
            gsdrawer.drawRegionOnMap(region);
            gsdrawer.drawStreetsInMap(region.StreetDTO);
        });
        gsdrawer.onSelectedStreetChanged = function (newStreet) {
            updateControls(newStreet === null ? 1 : 2);
        }
        gsdrawer.onSelectedRegionChanged = function () {
            updateControls(1);
        }
        gsdrawer.onStreetFocused = function (obj) {
            updateControls(3);
        }
        gsdrawer.onImagePresentation = function () {
            updateLocationPointSlider();
        }
    };
    imgPreview.src = "/out8.jpg";
    gsdrawer.imgPreview = document.getElementById("imgPreview");
}

var btStreetsClick = function () {
    if (btStreets.value === 'Abortar chamada') {
        if (!!getStreetsInRegionAjaxArray) {
            $.each(getStreetsInRegionAjaxArray, function (index, ajaxCall) {
                ajaxCall.abort();
            });
            console.log("Chamada ao getStreetsInRegions abortada!");
        }
        btStreets.value = 'Coletar ruas';
        return;
    }
    var confirmaSelecao = showTimeEstimationConfirmPopup(
            area(
            {
                lat: gsdrawer.selectedRegions[0].Bounds.North,
                lng: gsdrawer.selectedRegions[0].Bounds.West
            },
            {
                lat: gsdrawer.selectedRegions[0].Bounds.South,
                lng: gsdrawer.selectedRegions[0].Bounds.East
            }));
    if (confirmaSelecao) {
        btStreets.value = 'Abortar chamada';
        getStreetsInRegionAjaxArray = gsdrawer.getStreetsInRegions(function () {
            $.each(gsdrawer.selectedRegions, function (indexOfRegion, region) {
                gsdrawer.drawStreetsInMap(region.StreetDTO);
                /*$.each(region.StreetDTO, function (iStreet, Street) {
                    var gspan = new GSPanoramaMiner();
                    gspan.getPanoramasForStreet(Street, function () {
                        console.log(gspan.validPanoramas);
                        gsdrawer.resetStreetColor(Street);
                    });
                });*/
            });
            updateControls(2);
        });
    }
};

var btSnapInMapClick = function () {
    gsdrawer.snapin = !gsdrawer.snapin;
    btSnapInMap.src = gsdrawer.snapin ? unsnapbutton : snapbutton;
};

var btAmenitiesImagesClick = function () {
    statePlayPause === 1 ? setPlayPauseState(2) : setPlayPauseState(1);
};

function heatmapFeature(e) {
    switch (e.target.id) {
        case "btTreesHeatMap":
            toggleHeatMap(true);
            gsdrawer.plotHeatmapFromDB("Trees");
            break;
        default:
            break;
    }
}

function mapNode(e) {
    var amenity = "";
    var message = "";
    switch (e.target.id) {
        case "btSchool":
            amenity = "school";
            message = "Buscando escolas...";
            break;
        case "btPharmacy":
            amenity = "pharmacy";
            message = "Buscando farmácias...";
            break;
        case "btBusStop":
            amenity = "bus_station";
            message = "Buscando pontos de ônibus...";
            break;
        case "btHospital":
            amenity = "hospital";
            message = "Buscando hospitais...";
            break;
        default:
            break;
    }
    if (amenity.length > 0) {
        gsdrawer.showAmenity(amenity, function (success) {
            if (success) {
                //TODO: Inserir código para apresentar imagens de amenities
                updateControls(6);
            }
        });
    }


}

function toggleHeatMap(force) {
    gsdrawer.toggleHeatMapMode(force);
    if (force === true || force === false) {
        modeStreetHeatMap = force;
        btHeatMapStreetsToggle.value = force ? "Ver ruas" : "Ver HeatMap";
    }
    else {
        modeStreetHeatMap = !modeStreetHeatMap;
        btHeatMapStreetsToggle.value = modeStreetHeatMap ? "Ver ruas" : "Ver HeatMap";
    }
}

function downloadImages() {
    var imgDownloader = new ImageDownloader();
    if (gsdrawer.imageType === 'originalSet') {
        imgDownloader.getImagesForSelectedStreet(gsdrawer.getSelectedStreet(), function (data) {
            if ($.isArray(data) && data.length > 0) {
                var zip = new JSZip();
                var img = zip.folder("images");
                for (var i = 0; i < data.length; i++) {
                    img.file("" + i + ".jpg", data[i].base64image, { base64: true });
                }
                zip.generateAsync({ type: "blob" })
                .then(function (content) {
                    // see FileSaver.js
                    saveAs(content, "imagens.zip");
                });
            }
        });
    }
    else {
        if ($.isArray(gsdrawer.originalImages) && gsdrawer.originalImages.length > 0) {
            var zip = new JSZip();
            var img = zip.folder("images");
            for (var i = 0; i < gsdrawer.originalImages.length; i++) {
                img.file("" + i + ".jpg", gsdrawer.originalImages[i].filterResults[gsdrawer.imageType].base64image, { base64: true });
            }
            zip.generateAsync({ type: "blob" })
            .then(function (content) {
                // see FileSaver.js
                saveAs(content, "imagens.zip");
            });
        }
    }
    /*
    if (!!gsdrawer.previewImages && gsdrawer.previewImages.length > 0) {
        var zip = new JSZip();
        var img = zip.folder("images");
        for (var i = 0; i < gsdrawer.previewImages.length; i++) {
            img.file("" + i + ".jpg", gsdrawer.previewImages[i], { base64: true });
        }
        zip.generateAsync({ type: "blob" })
        .then(function (content) {
            // see FileSaver.js
            saveAs(content, "imagens.zip");
        });
    }*/
}

function getFilteredImages(type, obj) {

    if (!!gsdrawer.getImagesMetaData()[type]) {
        gsdrawer.setHeatMapData(type);
        toggleHeatMap(true);
        gsdrawer.startImagePresentation(type);
        updateControls(5);
        return;
    }

    if (!showTimeEstimationFilterConfirmPopup()) {
        return;
    }
    var filterCall = null;
    var filterUrl = "";
    if (type === 'Generic') {
        filterCall = imfilter.applyGenericFilter;
        filterUrl = window.location.origin + '/api/ImageFilter/GenericFilterTest';
    }
    else {
        filterCall = imfilter.applyGenericFilter;
        filterUrl = window.location.origin + '/api/ImageFilter/DetectFeaturesInSequence';
    }

    gsdrawer.imgPreview = document.getElementById("imgPreview");



    filterCall(gsdrawer.originalImages, filterUrl + "/" + type, type, function (filterDTOs, type) {
        gsdrawer.setFilteredSet(filterDTOs, type);
        gsdrawer.setHeatMapData(type);
        toggleHeatMap(true);
        gsdrawer.startImagePresentation(type);


        updateControls(5);
        //obj.style.border = '1px solid #00FF00';
        //obj.disabled = true;
    });
    //}

}

function getTreeFilteredImages() {
    getFilteredImages('Trees', this);
}

function getCrackFilteredImages() {
    getFilteredImages('Cracks', this);
}

function getImagesForStreetClick() {
    divLoading.style.display = 'block';
    try {
        gsdrawer.getImagesFromSelectedStreet(function (status) {
            if (status.validImage === 0) {
                alert('Não foi possível coletar nenhuma imagem para esta rua. \n' + status.wrongImage + ' localizações em endereços incorretos. \n' + status.zeroImage + ' localizações sem imagens.');
            }
            else {
                alert('Foram coletadas ' + status.validImage + ' imagem(ns) válida(s) para esta rua. \n' + status.wrongImage + ' localização(ões) em endereço(s) incorreto(s). \n' + status.zeroImage + ' localização(ões) sem imagem(ns).');
            }
            updateControls(4);
            divLoading.style.display = 'none';
        }, cbInterpolatePoints.checked);
    } catch (e) {
    	console.log(e);
    	divLoading.style.display = 'none';
    }
}


function saveSessionClick() {
    var region = gsdrawer.selectedRegions[0];
    var regionDTO = region;
    regionDTO.GSrectangle = undefined;

    if (!regionDTO.ID) regionDTO.ID = -1;
    gsdrawer.saveRegions([regionDTO], function () { });
}

function panMapByAddress() {
    var address = tbAddress.value;
    gsdrawer.getGeocoder().geocode({ 'address': address }, function (results, status) {
        if (status === google.maps.GeocoderStatus.OK) {
            gsdrawer.getMap().setCenter(results[0].geometry.location);
        } else {
            alert('Geocode falhou: ' + status);
        }
    });
}


