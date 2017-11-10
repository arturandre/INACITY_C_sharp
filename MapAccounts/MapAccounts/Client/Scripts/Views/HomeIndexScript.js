var btSearchAddress = document.getElementById("btSearchAddress");
var btPictures = document.getElementById("btPictures");
var btAmenitiesImages = document.getElementById("btAmenitiesImages");
var btSnapInMap = document.getElementById("btSnapInMap");
var btSaveSession = document.getElementById("btSaveSession");

var btUnsetStreet = document.getElementById("btUnsetStreet");

var btPreviousStreetImage = document.getElementById("btPreviousStreetImage");
var btAutoPlayStreetImages = document.getElementById("btAutoPlayStreetImages");
var btAutoPlayPauseStreetImages = document.getElementById("btAutoPlayPauseStreetImages");
var btNextStreetImage = document.getElementById("btNextStreetImage");

var btStreets = document.getElementById("btStreets");
var lblMaxImages = document.getElementById("lblMaxImages");
var lblNImages = document.getElementById("lblNImages");

var btHeatMapStreetsToggle = document.getElementById("btHeatMapStreetsToggle");
var modeStreetHeatMap = false;

var divReports = document.getElementById("divReports");
var divAmenities = document.getElementById("divAmenities");
//var divAmenitiesControls = document.getElementById("divAmenitiesControls");
var divImageNavControls = document.getElementById("divImageNavControls");


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
            gsdrawer.pause();
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

    //Definido no Index.cshtml da Home
    //setHelpMessage(step);
    $("#lblGuide").html("");
    switch (step) {
        //Tela inicial do sistema -> Selecionar região
        case 0:
            //$("#lblGuide").html("Selecione o canto superior esquerdo e o inferior direito de uma região:");
            $("#btHeatMapStreetsToggle").addClass("disabled");
            $("#btPictures").siblings().addClass("disabled");
            $("#divImageNavControls").addClass("hidden");
            $("#lblGuide").html(getResourceString("GD_STEP1"));
            gsdrawer.clearRegions();
            divAmenities.style.display = 'none';
            divStreetControls.style.display = 'none';

            divReports.style.display = 'none';
            //btPictures.disabled = true;
            //btPictures.display = 'block';
            sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            //divAmenitiesControls.style.display = 'none';
            break;
        //Região selecionada -> Coletar ruas | Amenities
        case 1:
            //$("#lblGuide").html("Pressione coletar ruas se a região for a desejada.");
            $("#btPictures").siblings().addClass("disabled");
            $("#divImageNavControls").addClass("hidden");
            $("#lblGuide").html(getResourceString("GD_STEP2"));
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            divReports.style.display = 'none';
            //btPictures.disabled = true;
            //btPictures.display = 'block';
            sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            //divAmenitiesControls.style.display = 'none';
            break;
        //Ruas coletadas -> Selecionar rua
        case 2:
            //$("#lblGuide").html("Selecione uma rua ou escolha visualizar as imagens da região.");
            $("#btHeatMapStreetsToggle").addClass("disabled");
            $("#btPictures").siblings().addClass("disabled");
            $("#divImageNavControls").addClass("hidden");
            $("#lblGuide").html(getResourceString("GD_STEP3"));
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            divReports.style.display = 'block';
            //btPictures.disabled = false;
            //btPictures.display = 'block';
            sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            //divAmenitiesControls.style.display = 'none';
            break;
        //Rua selecionada -> Ver imagens
        case 3:
            //$("#lblGuide").html("");
            $("#btHeatMapStreetsToggle").addClass("disabled");
            $("#btPictures").siblings().removeClass("disabled");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            divReports.style.display = 'block';
            //btPictures.disabled = false;
            //btPictures.display = 'block';
            //sdLocationPoint.disabled = true;
            btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';
            //divAmenitiesControls.style.display = 'none';
            break;
        //Imagens coletadas -> Selecionar filtro(s)
        case 4:
            //$("#lblGuide").html("");
            $("#btHeatMapStreetsToggle").addClass("disabled");
            $("#btPictures").siblings().removeClass("disabled");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            divReports.style.display = 'block';
            //btPictures.disabled = true;
            //btPictures.display = 'none';
            //sdLocationPoint.disabled = false;
            btDownloadImages.disabled = false;
            //divFilters.style.display = 'block';
            //divAmenitiesControls.style.display = 'none';
            break;
        //Filtro selecionado -> Ver HeatMap | Interagir com slider de imagem
        case 5:
            //$("#lblGuide").html("");
            $("#btHeatMapStreetsToggle").removeClass("disabled");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            divReports.style.display = 'block';
            //btPictures.disabled = true;
            //btPictures.display = 'none';
            //sdLocationPoint.disabled = false;
            btDownloadImages.disabled = false;
            //divFilters.style.display = 'block';
            //divAmenitiesControls.style.display = 'none';
            break;
        //Vendo imagens de amenities (Pontos de ônibus e afins)
        case 6:
            //$("#lblGuide").html("");
            divAmenities.style.display = 'block';
            divStreetControls.style.display = 'block';
            //divReports.style.display = 'none';
            //btPictures.disabled = true;
            //btPictures.display = 'none';
            //sdLocationPoint.disabled = false;
            //btDownloadImages.disabled = true;
            //divFilters.style.display = 'none';

            //divAmenitiesControls.style.display = 'flex';
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

function showTimeEstimationFilterConfirmPopup(time, maxtime) {
    var message = "";
    if (!!time) {
        var tsegs = Math.floor(time % 60);
        var tmins = Math.floor(time / 60) % 60;
        var thours = Math.floor(time / 3600);

        var hmessage = (thours > 0) ? thours + " horas " : "";
        var mmessage = (tmins > 0) ? tmins + " minutos " : "";
        var smessage = (tsegs > 0) ? tsegs + " segundos " : "";

        var tmessage = hmessage + mmessage + smessage;

        if (!!maxtime && Math.floor(maxtime) !== Math.floor(time)) {
            var maxtsegs = Math.floor(maxtime % 60);
            var maxtmins = Math.floor(maxtime / 60) % 60;
            var maxthours = Math.floor(maxtime / 3600);
            var hmaxmessage = (maxthours > 0) ? maxthours + " horas " : "";
            var mmaxmessage = (maxtmins > 0) ? maxtmins + " minutos " : "";
            var smaxmessage = (maxtsegs > 0) ? maxtsegs + " segundos " : "";

            var tmaxmessage = hmaxmessage + mmaxmessage + smaxmessage;

            message = "O tempo estimado para esta consulta é de " + tmessage + " a " + tmaxmessage + ", deseja continuar?";
        }
        else {
            message = "O tempo estimado para esta consulta é de " + tmessage + ", deseja continuar?";
        }
    }
    else {
        message = "Tempo estimado indisponível, deseja continuar?";
    }
    return confirm(message);
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
            //btStreets.value = 'Coletar ruas';
            btStreets.value = getResourceString("GET_STREETS");
            var request_time = new Date().getTime() - start_time;
            console.log("Ajax call took: " + request_time + " ms");
        }
    });
});

function bindings() {
    btUnsetStreet.onclick = btUnsetStreetClick;
    btSnapInMap.onclick = btSnapInMapClick;
    btAmenitiesImages.onclick = btAmenitiesImagesClick;
    btSearchAddress.onclick = panMapByAddress;
    btStreets.onclick = btStreetsClick;
    btPictures.onclick = getImagesForStreetClick;
    btSaveSession.onclick = saveSessionClick;
    btTreesFilter.onclick = function (e) { getFilteredImages('Trees', e.currentTarget); };
    btCracksFilter.onclick = function (e) { getFilteredImages('Cracks', e.currentTarget); };
    btGenericFilter.onclick = function (e) { getFilteredImages('Generic', e.currentTarget); };
    btDownloadImages.onclick = downloadImages;
    btHeatMapStreetsToggle.onclick = toggleHeatMap;
    btPreviousStreetImage.onclick = btPreviousStreetImageClick;
    btAutoPlayPauseStreetImages.onclick = btAutoPlayStreetImages.onclick = btAutoPlayStreetImagesClick;
    btNextStreetImage.onclick = btNextStreetImageClick;

    btSchool.onclick =
        btPharmacy.onclick =
        btHospital.onclick =
        btBusStop.onclick = mapNode;

    btTreesHeatMap.onclick = heatmapFeature;

    sdLocationPoint.oninput = sdLocationPoint.onchange = function (e) {
        gsdrawer.pause();
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

        gsdrawer.onPause = function () {
            btAutoPlayStreetImages.classList.remove('hidden');
            btAutoPlayPauseStreetImages.classList.add('hidden');
        }

        gsdrawer.onImageChanged = function () {
            sdLocationPoint.value = gsdrawer.getImgIndex();
        }

        gsdrawer.onSelectedStreetChanged = function (newStreet) {

            if (newStreet === null) {
                $("#btUnsetStreet").addClass("hidden");
            }
            else {
                $("#btUnsetStreet").removeClass("hidden");
            }

        }
        gsdrawer.onSelectedRegionChanged = function () {
            updateControls(2);
        }
        gsdrawer.onStreetFocused = function (obj) {
            updateControls(3);
        }
        gsdrawer.onImagePresentation = function () {
            divImageNavControls.classList.remove("hidden");
            sdLocationPoint.disabled = false;
            updateLocationPointSlider();
        }
        gsdrawer.onStreetsLoaded = function () {
            btStreets.disabled = true;
        }
        gsdrawer.onStreetsLoaded = function () {
            btStreets.disabled = false;
        }
        gsdrawer.onClearImagePresentation = function ()
        {
            imgPreview.src = "/out8.jpg";
        }
    };
    imgPreview.src = "/out8.jpg";
    gsdrawer.imgPreview = document.getElementById("imgPreview");
}

var btUnsetStreetClick = function ()
{
    gsdrawer.drawStreetsInMap(gsdrawer.selectedRegions[0].StreetDTO);
    updateControls(2);
}

var btPreviousStreetImageClick = function () {
    gsdrawer.pause();
    gsdrawer.previousImage();
    gsdrawer.setImgPresentationPosition();
}

var btAutoPlayStreetImagesClick = function () {
    btAutoPlayStreetImages.classList.toggle('hidden');
    btAutoPlayPauseStreetImages.classList.toggle('hidden');

    if (btAutoPlayPauseStreetImages.classList.contains('hidden')) {
        //Autoplay off
        gsdrawer.pause();
    }
    else if (btAutoPlayStreetImages.classList.contains('hidden')) {
        //Autoplay on
        gsdrawer.play();
    }


}

var btNextStreetImageClick = function () {
    gsdrawer.pause();
    gsdrawer.nextImage();
    gsdrawer.setImgPresentationPosition();
}

var btStreetsClick = function () {
    if (btStreets.value === getResourceString("ABORT_CALL")) {
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
        btHeatMapStreetsToggle.value = force ? getResourceString("SEE_STREETS") : getResourceString("SEE_HEATMAP");
    }
    else {
        modeStreetHeatMap = !modeStreetHeatMap;
        btHeatMapStreetsToggle.value = modeStreetHeatMap ? getResourceString("SEE_STREETS") : getResourceString("SEE_HEATMAP");
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
}

function getFilteredImages(type, obj) {

    if (gsdrawer.getImagesMetaData()[type]) {
        gsdrawer.setHeatMapData(type);
        toggleHeatMap(true);
        gsdrawer.startImagePresentation(type);
        updateControls(5);
        return;
    }

    if (type === "Trees") {
        var numImages = gsdrawer.originalImages.length;
        var minSampledTimePerImage = 0.96812 * numImages;
        var maxSampledTimePerImage = 1.10391 * numImages;
        if (!showTimeEstimationFilterConfirmPopup(minSampledTimePerImage, maxSampledTimePerImage)) {
            return;
        }
    }
    else {
        if (!showTimeEstimationFilterConfirmPopup()) {
            return;
        }
    }

    var filterCall = null;
    var filterUrl = "";
        filterCall = imfilter.applyGenericFilter;
        if (type === 'Generic') {
            filterUrl = window.location.origin + '/api/ImageFilter/GenericFilterTest';
            var newFilter = prompt(getResourceString("SET_FILTER_ENDPOINT"), filterUrl);
            if (newFilter !== null && newFilter !== "")
            {
                filterUrl = newFilter;
            }
    }
    else {
        filterUrl = window.location.origin + '/api/ImageFilter/DetectFeaturesInSequence';
    }

    gsdrawer.imgPreview = document.getElementById("imgPreview");



    filterCall(gsdrawer.originalImages, filterUrl + "/" + type, type, function (filterDTOs, type) {
        gsdrawer.setFilteredSet(filterDTOs, type);
        gsdrawer.setHeatMapData(type);
        toggleHeatMap(true);
        gsdrawer.startImagePresentation(type);

        updateControls(5);
    });
}

function getTreeFilteredImages() {
    getFilteredImages('Trees', this);
}

function getCrackFilteredImages() {
    getFilteredImages('Cracks', this);
}

function getImagesForStreetClick() {
    var type = 'originalSet';
    if (gsdrawer.getImagesMetaData()[type]) {
        gsdrawer.setHeatMapData(type);
        toggleHeatMap(false);
        gsdrawer.startImagePresentation(type);
        updateControls(5);
        return;
    }

    divLoading.style.display = 'block';
    try {
        const theStreet = gsdrawer.getSelectedStreet();
        if (theStreet) {
            gsdrawer.getImagesForStreet(theStreet, cbInterpolatePoints.checked,
                function (status) {
                    if (status.validImage === 0) {
                        //
                        alert(getResourceString("NO_IMAGES_1") + status.wrongImage + getResourceString("NO_IMAGES_2") + status.zeroImage + getResourceString("NO_IMAGES_3"));
                    }
                    else {
                        alert(getResourceString("NO_IMAGES_1") + status.validImage + getResourceString("NO_IMAGES_2") + status.wrongImage + getResourceString("NO_IMAGES_3") + status.zeroImage + getResourceString("NO_IMAGES_4"));
                    }
                    gsdrawer.loadImagesFromStreetIntoArray();
                    updateControls(4);
                    divLoading.style.display = 'none';
                }, 10, 10);
            //gsdrawer.getImagesFromSelectedStreet(, cbInterpolatePoints.checked);
        }
        else {
            gsdrawer.getImagesFromSelectedRegion(false, function (status) {
                updateControls(4);
                divLoading.style.display = 'none';
            }, cbInterpolatePoints.checked);
        }
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
            gsdrawer.getMap().setZoom(15);
        } else {
            alert('Geocode falhou: ' + status);
        }
    });
}


