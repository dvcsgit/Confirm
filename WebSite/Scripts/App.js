!function ($) {
    $.fn.Overlay = function (show) {
        return this.each(function () {
            if (show == 'show') {
                $(this).addClass('position-relative').append('<div class="widget-box-overlay"><i class=" ace-icon loading-icon fa fa-spinner fa-spin fa-2x white" style="margin-top:10px;"></i></div>');
            }
            if (show == 'hide') {
                $(this).removeClass('position-relative').find('.widget-box-overlay').remove();
            }
        });
    };
}(jQuery);

!function ($) {
    $.extend({
        Overlay: function (show) {
            if (show == 'show') {
                if (!($('#Overlay').length > 0)) {
                    $('body').append('<div id="Overlay" class="widget-box-overlay"><i class=" ace-icon loading-icon fa fa-spinner fa-spin fa-2x white"></i></div>');
                }

                $('#Overlay').show();
            }
            if (show == 'hide') {
                $('#Overlay').hide();
            }
        },
        Redirect: function (url) {
            location.replace(url);
        },
        HtmlEnCode: function (value) {
            return $('<div/>').text(value).html();
        }
    });
}(jQuery);

function padLeft(str, length) {
    if (str.length >= length) {
        return str;
    }
    else {
        return padLeft("0" + str, length);
    }
}

function ConfirmPost(msg, url, d, successUrl) {
    $.ConfirmDialog(msg, function (confirmed) {
        if (confirmed) {
            $.ajax({
                type: "POST",
                cache: false,
                url: url,
                data: d,
                dataType: "json",
                beforeSend: function () {
                    $.Overlay('show');
                },
                success: function (data) {
                    $.Overlay('hide');

                    if (data.IsSuccess) {
                        $.Redirect(successUrl)
                    }
                    else {
                        $.ErrorDialog(data.Message);
                    }
                },
                error: function (x, h, r) {
                    $.Overlay('hide');

                    $.ErrorDialog(x.responseText);
                }
            });
        }
    });
}
