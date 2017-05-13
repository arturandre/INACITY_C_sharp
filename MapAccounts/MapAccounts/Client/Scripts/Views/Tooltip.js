$('.masterTooltip').hover(function () {
    // Hover over code
    var title = $(this).attr('title');
    $(this).data('tipText', title).removeAttr('title');
    $('<p class="tooltipcustom"></p>')
    .text(title)
    .appendTo('body')
    .fadeIn('slow');
}, function () {
    // Hover out code
    $(this).attr('title', $(this).data('tipText'));
    $('.tooltipcustom').remove();
}).mousemove(function (e) {
    var mousex = e.pageX + 20; //Get X coordinates
    var mousey = e.pageY + 10; //Get Y coordinates
    $('.tooltipcustom')
    .css({ top: mousey, left: mousex });
});