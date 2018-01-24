$(document).ready(function() {
    // new tab links
    $('a[rel="external"]').attr("target", "_blank");
    $('a[href^="#"]:not("#clone")').click(function(event){event.preventDefault();var dest=0;if($(this.hash).offset().top > $(document).height()-$(window).height()){dest=$(document).height()-$(window).height(); }else{ dest=$(this.hash).offset().top;}$('html,body').animate({scrollTop:dest}, 2000,'swing');});
    // lightbox
    lightboxThings();
});

function lightboxThings() {
    $('a[rel*="lightbox"], a[rel*="productZoom"], a[rel*="iframe"]').fancybox();
    if ( $("#moby").css("display") != "none" )
        $('a[rel*=nobox]').attr('rel', 'lightbox');
    else
       $('a[rel*="lightbox"], a[rel*="productZoom"]').attr('rel', 'nobox');
}

// hide lightbox for mobile
$(window).resize(function () { lightboxThings(); });