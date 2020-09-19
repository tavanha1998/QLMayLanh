var MuonDC = {
    init: function () {
        MuonDC.registerEvents();
    },
    registerEvents: function () {
        //$(window).load(function () {
        //    var width = $(window).width();
        //    if (width <= 768) {
        //        $('#responsive1').css('width', '200px');
        //        $('#responsive1').css('float', '');
        //    }
        //});

        $(".openModal").off('click').on('click', function () {
            MuonDC.LoadDungCu();
            var listDC = [];
            MuonDC.chonVatDung(listDC);
            $(".btnXacNhan").off('click').on('click', function () {
                MuonDC.LapPhieu(listDC);
            });
        });
        //search vật dụng realtime
        $('#searchrealtime').on('keyup', function () {
            var value = $(this).val().toLowerCase();
            $('#table_body2 tr').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        //Xem CT
        $(".XemCT").off('click').on('click', function () {
            var idp = $(this).data('id');
            MuonDC.CTPhieu(idp);
        });
    },

    LapPhieu: function (listDC) {
        $.ajax({
            url: "/KTV/KhoDungCu/LapPhieu",
            data: { listDC:listDC },
            type: "POST",
            dataType: "json",
            success: function (response) {
                if (response.status) {
                    window.location.reload();
                }
                else
                    alert(response.mess);
            }
        })
    },

    chonVatDung: function (listDC) {
        $(document).delegate(".btnchonVatDung", "click", function () {
            var id = $(this).data('id');
            if ($('#iconchondungcu_' + id).hasClass("fas fa-check-square")) {
                $('#rowvd_' + id).css('background-color', "rgba(0,0,0,.075)");
                $('#btnChon_' + id).css('background-color', "#e74a3b");
                $('#btnChon_' + id).css('border-color', '#e74a3b');
                $('#iconchondungcu_' + id).toggleClass('fas fa-check-square fas fa-minus-square');
                listDC.push(id);
            }
            else {
                $('#rowvd_' + id).css("background-color", "");
                $('#btnChon_' + id).css('background-color', "");
                $('#btnChon_' + id).css('border-color', '');
                $('#iconchondungcu_' + id).toggleClass('fas fa-minus-square fas fa-check-square');
                listDC.splice(listDC.indexOf(id), 1);
            }

        })
    },
    LoadDungCu: function () {
        $.ajax({
            url: "/KTV/KhoDungCu/LoadDungCu",
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-template2').html();
                    if (data != "") {
                        $.each(data, function (i, item) {
                            html += Mustache.render(template, {
                                TenVatDung: item.TenVatDung,
                                ID:item.ID
                            });
                        });
                    }
                    else {
                        html += '<p style="text-align:center;column-span:2;color:blue">Không có dữ liệu</p>';
                        $('#table_body2').parent().html(html);
                    }
                    $('#table_body2').html(html);
                }
            }
        })
    },
    CTPhieu: function (idp) {
        $.ajax({
            url: "/KTV/KhoDungCu/CTPhieu",
            data: {idp:idp},
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-template3').html();
                    if (data != "") {
                        $.each(data, function (i, item) {
                            html += Mustache.render(template, {
                                TenVatDung: item.TenDC,
                                NgayMuon: item.NgayXuat
                            });
                        });
                    }
                    else {
                        html += '<p style="text-align:center;column-span:2;color:blue">Không có dữ liệu</p>';
                        $('#table_body3').parent().html(html);
                    }
                    $('#table_body3').html(html);
                }
            }
        })
    },
}
MuonDC.init();