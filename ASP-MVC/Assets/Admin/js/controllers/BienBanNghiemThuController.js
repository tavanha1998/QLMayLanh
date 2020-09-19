var bbnt = {
init: function () {

    //bbnt.ThemKTV();
        //bbnt.HoanThanhYC();
    bbnt.registerEvents();
    //bbnt.LoadKTV();
},

    registerEvents: function () {
        
        //Lostfocus sẽ update mã bbnt
        $('.onkeypress').off('keypress').on('keypress', function (e) {
            var idbbnt = $(this).data('idbbnt');
            if (e.which == 13) {
                var ma = $('#mabbnt_' + idbbnt).val();
                bbnt.updateMaBBNT(idbbnt, ma);
            }
        });

        // dialog xác nhận lưu ctbbnt 
        $('.Opendialog').off('click').on('click', function () {
            var table = $('#tableCT tr').length;
            var idbbnt = $(this).data('idbbnt');
            var idyc = $(this).data('idyc');
            var idkh = $(this).data('idkh');
            if (table > 0) {
                bbnt.LuuCTBBNT(idbbnt, idkh);
                $('.xacnhan').off('click').on('click', function () {
                    var ngaylamtiep = $('#ngaylamtiep').val();
                    bbnt.XacNhan(idyc, idkh, ngaylamtiep);
                })
            } else
                alert('Bạn chưa nhập thông tin chi tiết');
            

        });

        //Lay gia tri khi selectitem
        $('#txtDV').on('change', function () {
            var value = $(this).val();
            value = value.split('.')[0];
            if (value != '' && value > 0) {
                $.ajax({
                    url: "/admin/BienBanNghiemThu/timDV",
                    type: "POST",
                    dataType: "json",
                    data: { madv: value },
                    success: function (res) {
                        if (res)
                            $('#iddv').val(value);
                        else
                            alert("Không tìm thấy dịch vụ");
                    }
                })
            }
            else {
                $('#txtDV').val('');
            }
        });

        $('#txtDV').on('blur', function () {
            var value = $(this).val();
            var idyc = $('.getIDYC').data('id');
            value = value.split('.')[0];
            if (value != '' && value > 0) {
                $.ajax({
                    url: "/admin/BienBanNghiemThu/refreshDataML",
                    type: "POST",
                    dataType: "json",
                    data: { madv: value,idyc:idyc },
                    success: function (res) {
                        if (res.status == true && res.data.length > 0) {
                            var arr = res.data;
                            var item = $('<datalist id="dataMay"></datalist>');
                            item.append("<option value = 'ALL'>ALL " + (arr.length) + "</option>");
                            for (i = 0; i < arr.length; i++) {
                                item.append("<option value = " + arr[i].Ma + ">" + arr[i].ViTri +' '+  arr[i].TenMay + "</option>");
                            }
                            $('#dataMay').replaceWith(item);
                        }
                        else if (res.status == true && res.data.length == 0) {
                            var item = $('<datalist id="dataMay"></datalist>');
                            item.append("<option value = 'Không có dữ liệu'></option>");
                            $('#dataMay').replaceWith(item);
                        }

                    }
                })
            }
        });

        // Hoàn thành cả yêu cầu
        $('.hoanthanhyeucau').off('click').on('click', function () {
            var idyc = $(this).data('id');
            bbnt.HoanThanhYC(idyc);
        });

        //Mở pnl
        $('.PNL').off('click').on('click', function () {
            var idyc = $(this).data('id');
            bbnt.LoadCP_PX(idyc);
            bbnt.LoadCPKhac(idyc);
            bbnt.LoadBBNT(idyc);
            $('.themChiPhi').off('click').on('click', function () {
                bbnt.ThemChiPhi(idyc);
                
            });
        });

        //xóa bbnt
        $('.xoabbnt').off('click').on('click', function (e) {
            e.preventDefault();
            var idbbnt = $(this).data('idbbnt');
            bbnt.XoaBBNT(idbbnt);
        });
        //xóa chi tiết bbnt
        $('.xoact').off('click').on('click', function (e) {
            e.preventDefault();
            var idct = $(this).data('id');
            bbnt.XoaCT(idct);
        });

        $('.listKTV').off('click').on('click', function () {
            bbnt.resetForm();
            var idbbnt = $(this).data('id');
            $('#hidID').val(idbbnt);
            bbnt.LoadKTV(idbbnt);
        });
        $('.addKTV').off('click').on('click', function () {
            var idbbnt = $('#hidID').val();
            bbnt.ThemKTV(idbbnt);
        });

        // xóa chiphi
        $(document).delegate(".xoacp2", "click", function () {
            var id = $(this).data('id');
            var idyc = $(this).data('idyc');
            bbnt.XoaChiPhi(id,idyc);
        });

        // xóa ktv phụ trách
        $(document).delegate(".xoaktv", "click", function () {
            var id = $(this).data('id');
            bbnt.XoaKTV(id);
        });

        //search real time DSDichVu
        $('#filterDV').on('keyup', function () {
            var value = $(this).val().toLowerCase();
            $('#tableDV tr').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        $('#filterMay').on('keyup', function () {
            var value = $(this).val().toLowerCase();
            $('#tableMay tr').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        //autocomplete KTV phụ trách
        //$("#IDKTV").autocomplete({
        //    source: function (request, response) {
        //        bbnt.autocompleteKTV(request, response);
        //    }
        //});
        //autocomplete IDMay
        //$(".col-md-12 #txtMay").autocomplete({
        //    source: function (request, response) {
        //        var idkh = $(".layidKH").data('id');
        //        var idyc = $(".layidKH").data('idyc');
        //        bbnt.autocompleteMayPV(idkh,idyc ,request, response);
        //    },
        //    select: function (event, ui) {
        //        $(".col-md-12 #maypv").val(ui.item.id);
        //    }
        //});
        //autocomplete IDDV_SP
        //$(".col-md-12 #txtDV").autocomplete({
        //    source: function (request, response) {
        //        bbnt.autocompleteDVSP(request, response);
        //    },
        //    select: function (event, ui) {
        //        $(".col-md-12 #iddv").val(ui.item.id);
        //    }
        //});
        //Them CT vao bang tạm
        $(".TaoCT").off('click').on('click', function () {
            var ObjCTBBNT = {
                CPDauVao : $(".col-md-10 #CPDauVao").val(),
                IDYC : $(this).data('idyc'),
                IDKH: $(this).data('idkh'),
                IDDichVuJSON: $(".col-md-12 #iddv").val(),
                DonGia: $('#dongia').val(),
                DiemJSON: $('#diem').val(),
                Diem: $('#diem').val(),
                MayLanh : $("#txtMay").val().toUpperCase(),
                GhiChu : $(".col-md-10 #ghichu").val(),
                SoLuong : $("#soluong").val()
            }

            if (ObjCTBBNT.IDDichVu != "" && ObjCTBBNT.SoLuong > 0) {
                bbnt.ThemCTBBNT(ObjCTBBNT);
                //listCTBBNT.push(ObjCTBBNT);
                bbnt.refreshMay(ObjCTBBNT.IDKH, ObjCTBBNT.IDYC, ObjCTBBNT.IDDichVuJSON);
            }
            else
                alert("Bạn chưa nhập mã dịch vụ hoặc số lượng phải > 0");

            });
         //check mã máy lạnh
        $('#txtMay').on('keyup', function (e) {
            e.preventDefault();
            var idkh = $('.layidKH').data('id');
            var idyc = $('.getIDYC').data('id');
            var iddv=$('#iddv').val();
            var s = $(this).val().split(',');
            if(iddv != "" && iddv>0)
                bbnt.refreshMay(idkh,idyc,iddv);
            for (i = 0; i < s.length; i++) {
                if (s[i].toUpperCase() === "ALL") {
                    $('#txtMay').val('ALL');
                    break;
                }
                var obj = $('#dataMay').find("option[value='" + s[i] + "']");
                if (obj != null && obj.length > 0)
                    $('#dataMay').children("option[value='" + s[i] + "']").remove();
            }
        });

        //Hiển thị thông tin DichVu
        $('#txtDV').on('change', function () {
            var s = $(this).val();
            if (s == "") {
                $('#dongia').val('');
                $('#diem').val('');
            }
            else {
                var madv = s.split('.')[0];
                bbnt.getDV(madv);
            }
        });

        //Xoa ct trong bảng tạm
        $(document).delegate(".delct",'click', function () {
            var id = $(this).data('id');
            $('#row_' + id).remove();
        });
        //Edit ctbbnt
        $('.chinhsuact').off('click').on('click', function () {
            var id = $(this).data('id');
            var kh = $(this).data('kh');
            var stt = $(this).data('stt');
            //$('#validmay_' + id).autocomplete({
            //    source: function (request, response) {
            //        bbnt.autocompleteMayPV(kh,stt, request, response);
            //    },
            //    select: function (event, ui) {
            //        $("#validmay").val(ui.item.id);
            //    }
            //});
            //$('#valdvsp_' + id).autocomplete({
            //    source: function (request, response) {
            //        bbnt.autocompleteDVSP(request, response);
            //    },
            //    select: function (event, ui) {
            //        $("#valdvsp").val(ui.item.id);
            //    }
            //});
            if ($('#valsl_' + id).is(':hidden')) {
                $('#valsl_' + id).prop('hidden', false);
                $('#sl_' + id).prop('hidden', true);
                $('#valcpdv_' + id).prop('hidden', false);
                $('#cpdv_' + id).prop('hidden', true);
                $('#valdongia_' + id).prop('hidden', false);
                $('#dongia_' + id).prop('hidden', true);
                $('#valdiem_' + id).prop('hidden', false);
                $('#diem_' + id).prop('hidden', true);
                $('#valgc_' + id).prop('hidden', false);
                $('#gc_' + id).prop('hidden', true);
                $('#valml_' + id).prop('hidden', false);
                $('#ml_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#valsl_' + id).prop('hidden', true);
                $('#valsl_' + id).prop('hidden', true);
                $('#sl_' + id).prop('hidden', false);
                $('#valcpdv_' + id).prop('hidden', true);
                $('#cpdv_' + id).prop('hidden', false);
                $('#valdongia_' + id).prop('hidden', true);
                $('#dongia_' + id).prop('hidden', false);
                $('#valdiem_' + id).prop('hidden', true);
                $('#diem_' + id).prop('hidden', false);
                $('#valgc_' + id).prop('hidden', true);
                $('#gc_' + id).prop('hidden', false);
                $('#valml_' + id).prop('hidden', true);
                $('#ml_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var sl = $('#valsl_' + id).val();
                var ghichu = $('#valgc_' + id).val();
                var ml = $('#valml_' + id).val();
                var dongia = $('#valdongia_' + id).val();
                var diem = $('#valdiem_' + id).val();
                var CPDauVao = $('#valcpdv_' + id).val();
                bbnt.updateData(id, ml, sl, dongia, diem, CPDauVao ,ghichu, kh,stt)
            }
        });
        //enter update record
        $('.updatectBBNT').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            var stt = $(this).data('ycpv');
            var kh = $(this).data('kh');
            bbnt.enterupdate(id,e,kh,stt);
        });
        // Thêm DVSP mới
        $('.themdv').off('click').on('click',function () {
            //var madv = $('#madv').val();
            //var tendv = $('#tendv').val();
            //var loaidv = $('#loaidv').val();
            //var dongia = $('#dongia').val();
            //var diem = $('#diem').val();
            var Object = {
                MaDV_SP: $('#madv').val(),
                TenDichVu_SanPham: $('#tendv').val(),
                Loai: $('#loaidv').val(),
                DonGia: $('#dongiadv').val(),
                Diem: $('#diemdv').val()
            };
            var cf = confirm("Xác nhận lưu?");
            if (cf == true) {
                $.ajax({
                    url: '/admin/BienBanNghiemThu/NewDVSP',
                    type: 'POST',
                    datatype: 'json',
                    data: ({ dvsp: JSON.stringify(Object)}),
                    success: function (response) {
                        if (response.status) {
                            alert(response.mess);
                            var html = '<option value="' + response.ma + '. ' + response.ten + '"></option>';
                            $('#dataDV').append(html);
                            $('#txtDV').val(response.ma + '. ' + response.ten);
                            $('#iddv').val(response.ma);
                            $('#dongia').val($('#dongiadv').val());
                            $('#diem').val($('#diemdv').val());
                        }
                        else {
                            alert(response.mess);
                        }
                    }
                });
            }
        });
    },

    //checkYCPV: function (idyc,idmay){
    //    $.ajax({
    //        url: "/admin/BienBanNghiemThu/CheckYCPV",
    //        data: { idyc:idyc, idmay:idmay},
    //        dataType: "json",
    //        type: "POST",
    //        success: function (response) {
    //            if (response.status) {
    //                $(".col-md-12 #iddv").val(response.data);
    //            }
    //        }
    //    });
    //},

    refreshMay: function (idkh,idyc,iddv) {
        $.ajax({
            url: '/admin/BienBanNghiemThu/getDsMay',
            type: 'POST',
            async: false,
            datatype: 'json',
            data: { idkh: idkh, idyc: idyc, iddv: iddv },
            success: function (response) {
                if (response.status) {
                    var arr = response.data;
                    if (arr.length > 0) {
                        var item = $('<datalist id="dataMay"></datalist>');
                        item.append("<option value = 'ALL'>ALL " + (arr.length) + "</option>");
                        for (i = 0; i < arr.length; i++) {
                            item.append("<option value = " + arr[i].Ma + ">" + arr[i].ViTri + arr[i].TenMay + "</option>");
                        }
                        $('#dataMay').replaceWith(item);
                    }
                    else {
                        var item = $('<datalist id="dataMay"></datalist>');
                        item.append("<option value = 'Không có dữ liệu'></option>");
                        $('#dataMay').replaceWith(item);
                    }
                }
            }
        });
    },

    getDV: function (madv) {
        $.ajax({
            url: '/admin/BienBanNghiemThu/getDV',
            type: 'POST',
            datatype: 'json',
            data: {madv:madv},
            success: function (response) {
                if (response.status) {
                    $('#dongia').val(response.dongia);
                    $('#diem').val(response.diem);
                }
            }
        });
    },

    enterupdate: function (id, e, kh,stt) {
            if (e.which == 13) {
                var sl = $('#valsl_' + id).val();
                var ghichu = $('#valgc_' + id).val();
                var ml = $('#valml_' + id).val();
                var dongia = $('#valdongia_' + id).val();
                var diem = $('#valdiem_' + id).val();
                var CPDauVao = $('#valcpdv_' + id).val();
                bbnt.updateData(id, ml, sl, dongia, diem, CPDauVao,ghichu, kh,stt)
            }
    },

    updateData: function (id, ml, sl, dongia, diem, CPDauVao,ghichu, kh,stt) {
        var dataObject = {
            ID: id,
            MayLanh: ml,
            SoLuong: sl,
            GhiChu: ghichu,
            DonGia: dongia,
            CPDauVao: CPDauVao
        };
        var cf = confirm("Xác nhận lưu?");
        if (cf == true) {
            $.ajax({
                url: '/admin/BienBanNghiemThu/Update',
                type: 'POST',
                datatype: 'json',
                data: ({ ctbbnt: JSON.stringify(dataObject),diem:diem, kh:kh, ycpv:stt}),
                success: function (response) {
                    if (response.status) {
                        alert("Cập nhập thành công");
                        location.reload();
                    }
                    else {
                        alert("Cập nhập thất bại," + response.mess);
                        location.reload();
                    }
                }
            });
        }
    },
    LuuCTBBNT: function (idbbnt, idkh) {
        var listCTBBNT = [];
        $('#tableCT tr').each(function () {
            var obj = {
                CPDauVao: $(this).find(".getvaluecpdv").html(),
                //IDYC: $(this).data('idyc'),
                //IDKH: $(this).data('idkh'),
                IDDichVu: $(this).find(".getvaluedv").html(),
                DonGia: $(this).find(".getvaluedg").html(),
                DiemJSON: $(this).find(".getvaluediem").html(),
                Diem: $(this).find(".getvaluediem").html(),
                MayLanh: $(this).find(".getvalueml").html(),
                GhiChu: $(this).find(".getvaluegc").html(),
                SoLuong: $(this).find(".getvaluesl").html()
            }
            listCTBBNT.push(obj);
        });
        $.ajax({
            url: "/admin/BienBanNghiemThu/LuuCTBBNT",
            async: false,
            data: { list: listCTBBNT, idkh: idkh, idbbnt: idbbnt },
            dataType: "json",
            type: "POST",
            success: function (response) {
                if (!response.status) {
                    alert(response.mess);
                }
                else {
                    jQuery.noConflict();
                    $('#modalNgayLamTiep').modal('show');
                }
            },
        });
    },
    
ThemCTBBNT: function (obj) {
    $.ajax({
        url: "/admin/BienBanNghiemThu/ThemCTBBNT",
        data: { iddv: obj.IDDichVuJSON, idkh: obj.IDKH, idyc: obj.IDYC, maylanh: obj.MayLanh },
        dataType: "json",
        type: "POST",
        success: function (response) {
            if (response.status) {
                var s = '<tr id="row_' + response.datadv.ID + '">' +
                    '<td class="getvaluedv" hidden>' + response.datadv.ID + '</td>' +
                    '<td>' + response.datadv.MaDV_SP + '</td>' +
                    '<td>' + response.datadv.TenDichVu_SanPham + '</td>' +
                    '<td class="getvaluedg">' + obj.DonGia + '</td>' +
                    '<td class="getvaluediem">' + obj.DiemJSON + '</td>' +
                    '<td class="getvaluesl">' + obj.SoLuong + '</td>' +
                    '<td class="getvalueml">' + obj.MayLanh + '</td>' +
                    '<td class="getvaluecpdv">' + obj.CPDauVao + '</td>' +
                    '<td class="getvaluegc">' + obj.GhiChu + '</td>' +
                    '<td><a data-id="' + response.datadv.ID + '" class="delct btn btn-circle">X</a></td>' +
                    '</tr > ';
                $('#tableCT').append(s);
                bbnt.resetctbbnt();
            }
            else
                alert(response.mess);
        }
    })
},
resetctbbnt: function()
{
    $(".col-md-12 #txtMay").val('');
    $(".col-md-12 #maypv").val('');
    $(".col-md-12 #iddv").val('');
    $(".col-md-12 #txtDV").val('');
    $("#soluong").val('');
    $("#dongia").val('');
    $("#diem").val('');
    $(".col-md-10 #CPDauVao").val('');
    $(".col-md-10 #ghichu").val('');
},    
autocompleteKTV: function (request, response) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/DropDownKTV",
            type: "POST",
            dataType: "json",
            data: { s: request.term },
            success: function (data) {
                response($.map(data, function (item) {
                    return { label: item.TenKTV + " - " + item.SDT, value: item.ID };
                }))
            }
        })
    },
autocompleteDVSP: function (request, response) {
    $.ajax({
        url: "/admin/BienBanNghiemThu/DropDownDV",
        type: "POST",
        dataType: "json",
        data: { s: request.term },
        success: function (data) {
            response($.map(data, function (item) {
                return { label: item.MaDV_SP + " - " + item.TenDichVu_SanPham, value: item.MaDV_SP + " - " + item.TenDichVu_SanPham, id: item.ID };
            }))
        }
    })
},
autocompleteMayPV: function (idkh, idyc, request, response) {
    $.ajax({
        url: "/admin/BienBanNghiemThu/DropDownMayPV",
        type: "POST",
        dataType: "json",
        data: { s: request.term, idkh: idkh, idyc:idyc },
        success: function (data) {
            response($.map(data, function (item) {
                return { label: item.ID + " - " + item.TenMay + " - " + item.Model + " - " + item.ViTri, value: item.TenMay + " - " + item.ViTri, id: item.ID};
            }))
        }
    })
},
resetForm: function () {
    $('#IDKTV').val('');
    $('#Diem').val('');
    $('#DanhGia').val('');
    },

    LoadBBNT: function (idyc) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/LoadBBNT",
            data: { idyc: idyc },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-templateCPBBNT').html();
                    $.each(data, function (i, item) {
                        html += Mustache.render(template, {
                            ID: item.ID,
                            TenDVSP: item.TenDVSP,
                            SoLuong: item.SoLuong,
                            DonGia: item.DonGia,
                            Ma: item.Ma,
                            CPDauVao: item.CPDauVao,
                            GhiChu: item.GhiChu,
                            NgayPV: item.NgayPV,
                        });
                    });
                    $('#table_bodyCPBBNT').html(html);
                    $('#lblTongTienBBNT').val('Tổng tiền: ' + res.tt);
                }
                else
                    alert("Không có dữ liệu");
            }
        })
    },

    LoadCP_PX: function (idyc) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/LoadCPPX",
            data: { idyc: idyc },
            dataType: "json",
            type: "GET",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-templateCP1').html();
                    $.each(data, function (i, item) {
                        html += Mustache.render(template, {
                            ID: item.ID,
                            TenVatTu: item.TenVatTu,
                            SoLuongLay: item.SoLuongLay,
                            SoLuong: item.SoLuongTT,
                            DonGia: item.DonGia,
                            ThanhTien: item.ThanhTien,
                            TenKTV: item.TenKTV,
                        });
                    });
                    $('#table_bodyCP1').html(html);
                    $('#lblTongTienCPXK').val('Tổng tiền: ' + res.tt);
                }
                else
                    alert("Không có dữ liệu");
            }
        })
    },

    LoadCPKhac: function (idyc) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/LoadCPKhac",
            data: { idyc: idyc },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-templateCP2').html();
                    $.each(data, function (i, item) {
                        html += Mustache.render(template, {
                            ID: item.ID,
                            TenChiPhi: item.TenChiPhi,
                            SoLuong: item.SoLuong,
                            GiaDauVao:item.GiaDauVao,
                            DonGia: item.DonGia,
                            ThanhTien: item.SoLuong * item.DonGia,
                            GhiChu: item.GhiChu,
                        });
                    });
                    $('#table_bodyCP2').html(html);
                    $('#lblTongTienCPKhac').val('Tổng tiền: ' + res.tt);
                }
                else
                    alert("Không có dữ liệu");
            }
        })
    },
    resetChiPhi: function () {
        $('#valTenCP').val('');
        $('#valsoluong').val('');
        $('#valgiadauvao').val('');
        $('#valdongia').val('');
        $('#valghichu').val('');
    },
    ThemChiPhi: function (idyc) {
        var tenCP = $('#valTenCP').val();
        var soluong = $('#valsoluong').val();
        var giadauvao = $('#valgiadauvao').val();
        var dongia = $('#valdongia').val();
        var ghichu = $('#valghichu').val();
        if (giadauvao % 1 !== 0 || dongia % 1 !== 0)
            alert("Giá đầu vào và giá bán phải là số nguyên!");
        else if (tenCP != "" && soluong != "" && dongia != "" && dongia > 0 && soluong >0) {
            $.ajax({
                url: "/admin/BienBanNghiemThu/ThemChiPhi",
                data: { idyc: idyc, tenCP: tenCP, soluong: soluong, giadauvao: giadauvao, dongia: dongia, ghichu:ghichu },
                dataType: "json",
                type: "POST",
                success: function (response) {
                    if (response.status) {
                        var s = '<tr id="rowcp2_' + response.id + '">' +
                            '<td>' + tenCP + '</td>' +
                            '<td>' + soluong + '</td>' +
                            '<td>' + giadauvao + '</td>' +
                            '<td>' + dongia + '</td>' +
                            '<td>' + dongia*soluong + '</td>' +
                            '<td>' + ghichu + '</td>' +
                            '<td><a data-id="' + response.id + '" data-idyc="'+idyc+'" class="btn btn-circle xoacp2">X</a></td>' +
                            '</tr > ';
                        $('#table_bodyCP2').append(s);
                        bbnt.LoadCPKhac(idyc);
                        bbnt.resetChiPhi();
                    }
                    else
                        alert("Không tìm thấy KTV, vui lòng kiểm tra lại mã");
                }
            })
        }
        else
            alert("Bạn chưa nhập đủ thông tin, số lượng > 0 và đơn giá > 0");
    },
    XoaChiPhi: function (id,idyc) {
        var r = confirm("Xác nhận xóa chi phí?")
        if (r == true) {
            $.ajax({
                url: "/admin/BienBanNghiemThu/XoaChiPhi",
                data: { id: id },
                dataType: "json",
                type: "POST",
                success: function (response) {
                    console.log(response);
                    if (response.status) {
                        $('#rowcp2_' + id).remove();
                        bbnt.LoadCPKhac(idyc);
                    }
                }
            })
        }
    },
LoadKTV: function (idbbnt) {
    $.ajax({
        url: "/admin/BienBanNghiemThu/LoadKTV",
        data: { idbbnt: idbbnt },
        dataType: "json",
        type: "GET",
        success: function (res) {
            if (res.status) {
                var data = res.data;
                var html = '';
                var template = $('#data-template').html();
                $.each(data, function (i, item) {
                    html += Mustache.render(template, {
                        ID: item.ID,
                        TenKTV: item.TenKTV,
                        Diem: item.Diem,
                        DanhGia: item.DanhGia
                    });
                });
                $('#table_body').html(html);
            }
        }
    })
},

    ThemKTV: function (idbbnt) {
        var s = $('#IDKTV').val().split('.');
        var idktv = s[0];
    var diem = $('#Diem').val();
    var dg = $('#DanhGia').val();
    if (idktv != "" && diem != "") {
        $.ajax({
            url: "/admin/BienBanNghiemThu/ThemKTV",
            data: { idktv: idktv, idbbnt: idbbnt, diem: diem, dg: dg },
            dataType: "json",
            type: "POST",
            success: function (response) {
                if (response.status) {
                    var s = '<tr id="rowktv_' + response.id + '">' +
                        '<td>' + response.data + '</td>' +
                        '<td>' + diem + '</td>' +
                        '<td>' + dg + '</td>' +
                        '<td><a data-id="' + response.id + '" class="xoaktv btn btn-circle">X</a></td>' +
                        '</tr > ';
                    $('#table_body').append(s);
                    bbnt.resetForm();
                }
                else
                    alert("Không tìm thấy KTV, vui lòng kiểm tra lại mã");
            }
        })
    }
    else
        alert("Bạn chưa nhập mã KTV hoặc điểm số");
},
XoaKTV: function (id) {
    var r = confirm("Xác nhận xóa chi tiết?")
    if (r == true) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/XoaKTV",
            data: { id: id },
            dataType: "json",
            type: "POST",
            success: function (response) {
                console.log(response);
                if (response.status) {
                    $('#rowktv_' + id).remove();
                }
            }
        })
    }
},
XacNhan: function (idyc,idkh,ngaylamtiep) {
    $.ajax({
        url: "/admin/BienBanNghiemThu/Complete",
        data: { idyc: idyc, ngaylamtiep: ngaylamtiep },
        dataType: "json",
        type: "POST",
        success: function (response) {
            if (response.status) {
                window.location.href = "/admin/BienBanNghiemThu?stt=" + idyc + "&kh=" + idkh;
            }
        }
    })
},
XoaCT: function (idct) {
    var r = confirm("Xác nhận xóa chi tiết?")
    if (r == true) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/XoaCT",
            data: { idct: idct },
            dataType: "json",
            type: "POST",
            success: function (response) {
                console.log(response);
                if (response.status) {
                    location.reload();
                }
            }
        })
    }
    },
XoaBBNT: function (idbbnt) {
        var r = confirm("Xác nhận xóa biên bản nghiệm thu?")
        if (r == true) {
            $.ajax({
                url: "/admin/BienBanNghiemThu/XoaBBNT",
                data: { idbbnt: idbbnt },
                dataType: "json",
                type: "POST",
                success: function (response) {
                    console.log(response);
                    if (response.status) {
                        location.reload();
                    }
                    else
                        alert(response.mess);
                }
            })
        }
    },
HoanThanhYC: function (idyc) {
    var r = confirm("Yêu cầu đã hoàn thành?")
    if (r == true) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/Success",
            data: { idyc: idyc },
            dataType: "json",
            type: "POST",
            success: function (response) {
                console.log(response);
                if (response.status) {
                    window.location = "/admin";
                }
            }
        })
    }
},
updateMaBBNT: function (idbbnt, ma) {
    var r = confirm("Cập nhật mã phiếu?")
    if (r == true) {
        $.ajax({
            url: "/admin/BienBanNghiemThu/updateMaBBNT",
            data: { idbbnt: idbbnt, ma:ma },
            dataType: "json",
            type: "POST",
            success: function (response) {
                console.log(response);
                if (response.status) {
                    alert(response.mess);
                }
                else
                    alert(response.mess);
            }
        })
    }
    },
}
bbnt.init();