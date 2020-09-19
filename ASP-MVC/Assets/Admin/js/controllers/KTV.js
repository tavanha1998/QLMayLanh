var ktv = {
    init: function() {
        ktv.registerEvents();
    },
    registerEvents: function () {
        $(".dsDC").off('click').on('click', function () {
            var idktv = $(this).data('id');
            ktv.LoadDungCu(idktv);
        });
        
    },
    LoadDungCu: function (idktv) {
        $.ajax({
            url: "/KTV/Home/LoadDungCu",
            data: { idktv: idktv },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-template').html();
                    if (data != "") {
                        $.each(data, function (i, item) {
                            html += Mustache.render(template, {
                                TenVatDung: item.TenDC,
                                NgayXuat: item.NgayXuat,
                                NgayTra: item.NgayTra,
                                GhiChu: item.GhiChu,
                                TinhTrang: item.TinhTrang,
                            });
                        });
                    }
                    else {
                        html += '<p style="text-align:center;column-span:2;color:blue">Không có dữ liệu</p>';
                        $('#table_body').parent().html(html);
                    }
                    $('#table_body').html(html);
                }
            }
        })
    },
}
ktv.init();