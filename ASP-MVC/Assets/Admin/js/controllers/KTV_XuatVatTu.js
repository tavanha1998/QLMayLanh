var XuatVT = {
    init: function () {
        XuatVT.registerEvents();
    },
    registerEvents: function () {
        //$(window).load(function () {
        //    var width = $(window).width();
        //    if (width <= 768) {
        //        $('#responsive1').css('width', '200px');
        //        $('#responsive1').css('float', '');
        //        $('#responsive2').css('width', '250px');
        //        $('#responsive3').css('padding-left', '');
        //        $('#responsive4').css('width', '250px');
        //        $('#responsive5').css('padding-left', '');
        //        $('#responsive6').css('padding-left', '');
        //    }
        //});

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
            XuatVT.CTPhieu(idp);
        });

        //Auto đơn giá
        $('#IDVatTu').change(function () {
            XuatVT.autoDonGia();
        });

        //Thêm CT vào bảng
        $(".TaoCT").off('click').on('click', function () {
            var str = $("#IDVatTu").val().split('.');
            var idvattu = str[0];
            var soluong = $(".col-md-15 #valsoluong").val();
            var dongia = $(".col-md-15 #valdongia").val();
            if (idvattu != "" && soluong != "" && dongia != "" && idvattu > 0 && soluong > 0 && dongia > 0) {
                XuatVT.ThemCTPN(idvattu, soluong, dongia);
                //phieuxuat_ktv.resetctbbnt();
            }
            else
                alert("Vui lòng kiểm tra thông tin: Vật tư, đơn giá (>0), số lượng (>0)");
        });

        //Xóa ct
        $(document).delegate(".delct", 'click', function () {
            var id = $(this).data('id');
            $('#row_' + id).remove();
        });

        //Tạo phiếu xuất
        $(".LapPhieuXuat").off('click').on('click', function () {
            var table = $('#tableCT tr').length;
            if (table > 0) {
                var strt = $("#IDYCPV").val().split('.');
                var idycpv = strt[0];
                var ghichu = $("#ghichu").val();

                if (idycpv != "" && idycpv > 0) {

                    var checkconfirm = confirm("Xác nhận lưu?");
                    if (checkconfirm == true) {
                        var listCT = [];
                        XuatVT.TaoCTPhieuXuat(listCT);
                        XuatVT.LapPhieuXuat(idycpv, ghichu, listCT);
                    }
                    //else
                    //    location.reload();
                }
                else
                    alert("Bạn chưa chọn yêu cầu");
            }
            else
                alert("Bạn chưa nhập thông tin chi tiết");
        })
    },

    LapPhieuXuat: function (idycpv, ghichu, listCT) {
        $.ajax({
            url: "/KTV/KhoVatTu/LapPhieuXuat",
            type: "POST",
            data: { idycpv: idycpv, ghichu: ghichu,listCT:listCT },
            dataType: "json",
            success: function (response) {
                if (response.status == true) {
                    alert("Thêm phiếu thành công");
                    window.location.reload();
                }
                else
                    alert(response.mess);
            }
        })
    },

    TaoCTPhieuXuat: function (listCT) {
        $('#tableCT tr').each(function () {
            var Obj = {
                IDVatTu: $(this).find(".getvalueidvattu").html(),
                SLLay: $(this).find(".getvaluesoluong").html(),
                DonGiaJSON: $(this).find(".getvaluedongia").html(),
            };
            listCT.push(Obj);
        });
    },

    ThemCTPN: function (idvattu, soluong, dongia) {
        $.ajax({
            url: "/KTV/KhoVatTu/ThemCTPX",
            data: { idvattu: idvattu, soluong: soluong },
            dataType: "json",
            type: "POST",
            success: function (response) {
                if (response.status) {
                    var s = '<tr id="row_' + response.vattu.ID + '">' +
                        '<td class="getvalueidvattu">' + response.vattu.ID + '</td>' +
                        '<td>' + response.vattu.TenVatTu + '</td>' +
                        '<td class="getvaluesoluong">' + soluong + '</td>' +
                        '<td>' + response.vattu.DonVi + '</td>' +
                        '<td class="getvaluedongia">' + dongia + '</td>' +
                        '<td><a data-id="' + response.vattu.ID + '" class="delct btn btn-circle">X</a></td>' +
                        '</tr > ';
                    $('#tableCT').append(s);
                    XuatVT.resetctbbnt();
                }
                else
                    alert(response.mess);
            }
        })
    },

    resetctbbnt: function () {
        $("#IDVatTu").val('');
        $(".col-md-15 #valsoluong").val('');
        $(".col-md-15 #valdongia").val('');
    },

    autoDonGia: function () {
        var str = $('#IDVatTu').val().split('.');
        var idvattu = str[0];
        if (idvattu != "" && idvattu > 0) {
            $.ajax({
                url: "/KTV/KhoVatTu/autoDonGia",
                type: "POST",
                data: { idvattu: idvattu },
                dataType: "json",
                success: function (response) {
                    if (response.status) {
                        $('#valdongia').val(response.dongia);
                    }
                }
            })
        }
    },

    CTPhieu: function (idp) {
        $.ajax({
            url: "/KTV/KhoVatTu/CTPhieu",
            data: { idp: idp },
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
                                TenVatTu: item.TenVatTu,
                                SLLay: item.SLLay,
                                SLTra: item.SLTra,
                                SLThucTe: item.SLThucTe,
                                DonVi: item.DonVi,
                                GiaBan: item.GiaBan,
                                ThanhTien:item.ThanhTien,
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
XuatVT.init();