var MayChoThueController = {

    init: function () {
        MayChoThueController.registerEvents();
    },
    
    updateData: function (id, tenmay, model, congsuat, ngaymua, iddichvu, ghichu,status) {
        var dataObject = {
            ID: id,
            TenMay: tenmay,
            Model: model,
            CongSuat: congsuat,
            IDDichVu: iddichvu,
            GhiChu: ghichu,
            Status: status
        };
        var checkconfirm = confirm("Xác nhận lưu?");
        if (checkconfirm == true) {
            $.ajax({
                url: '/admin/MayChoThue/Update',
                type: 'POST',
                datatype: 'json',
                data: ({ mct: JSON.stringify(dataObject), ngaymua: ngaymua }),
                success: function (response) {
                    if (response.status) {
                        alert("Cập nhập thành công");
                        location.reload();
                    }
                    else {
                        alert(response.mess);
                        //location.reload();
                    }
                }
            })
        }
        else
            location.reload();
    },

    enableField: function (id) {
        if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
            $('#tenmay_' + id).prop('hidden', false);
            $('#tm_' + id).prop('hidden', true);
            $('#model_' + id).prop('hidden', false);
            $('#md_' + id).prop('hidden', true);
            $('#congsuat_' + id).prop('hidden', false);
            $('#cs_' + id).prop('hidden', true);
            $('#ngaymua_' + id).prop('hidden', false);
            $('#nm_' + id).prop('hidden', true);
            $('#iddichvu_' + id).prop('hidden', false);
            $('#iddv_' + id).prop('hidden', true);
            $('#ghichu_' + id).prop('hidden', false);
            $('#gc_' + id).prop('hidden', true);
            $('#status_' + id).prop('hidden', false);
            $('#sta_' + id).prop('hidden', true);
            $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
        }
        else {
            $('#tenmay_' + id).prop('hidden', true);
            $('#tm_' + id).prop('hidden', false);
            $('#model_' + id).prop('hidden', true);
            $('#md_' + id).prop('hidden', false);
            $('#congsuat_' + id).prop('hidden', true);
            $('#cs_' + id).prop('hidden', false);
            $('#ngaymua_' + id).prop('hidden', true);
            $('#nm_' + id).prop('hidden', false);
            $('#iddichvu_' + id).prop('hidden', true);
            $('#iddv_' + id).prop('hidden', false);
            $('#ghichu_' + id).prop('hidden', true);
            $('#gc_' + id).prop('hidden', false);
            $('#status_' + id).prop('hidden', true);
            $('#sta_' + id).prop('hidden', false);
            $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

            var tenmay = $('#tenmay_' + id).val();
            var model = $('#model_' + id).val();
            var congsuat = $('#congsuat_' + id).val();
            var ngaymua = $('#ngaymua_' + id).val();
            var iddichvu = $('#iddichvu_' + id).val();
            var ghichu = $('#ghichu_' + id).val();
            var status = $('#status_' + id).val();
            MayChoThueController.updateData(id, tenmay, model, congsuat, ngaymua, iddichvu, ghichu,status)
        }
    },
    // xác nhận xóa máy cho thuê
    XacNhan: function () {
        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                //ycpv.DeleteMayChoThue(id);
            })
            //$(".modal-body #lydo").val(id);
        })
    },
    DeleteMay: function (id) {
        $.ajax({
            url: '/admin/MayChoThue/Delete',
            type: 'POST',
            dataType: 'json',
            data: { id: id },
            success: function (res) {
                if (res.status) {
                    $('#row_' + id).remove();
                }
            }
        })
    },
    registerEvents: function () {

        $('span').on('click', function () {
            var checkbox = event.target.id;
            var num = checkbox.split('x')[1];
            var ht = new HienThi(checkbox);
            if (ht.Check() == false)
                $('td:nth-child(' + num + '),th:nth-child(' + num + ')').hide();
            else
                $('td:nth-child(' + num + '),th:nth-child(' + num + ')').show();
        });

        $(document).ready(function () {
            $.fn.dataTable.moment('DD/MM/YYYY');
            $('#dataTable').DataTable({
                autoWidth: true,
                scrollX: true,
                pageLength: 25,
                language: {
                    "lengthMenu": "Hiển thị _MENU_ ",
                    "zeroRecords": "Không có dữ liệu",
                    "info": "Trang _PAGE_",
                    "infoEmpty": "Không có dữ liệu",
                    "infoFiltered": "",
                    "search": "Tìm kiếm:",
                    "paginate": {
                        "first": "<<",
                        "last": ">>",
                        "next": ">",
                        "previous": "<"
                    },
                },

            });
        });

        $('.btnEditMayChoThue').on('click', function () {
            var id = $(this).data('idmaychothue');
            //$('#iddichvu_' + id).autocomplete({
            //    source: function (request, response) {
            //        $.ajax({
            //            url: "/admin/BienBanNghiemThu/DropDownDV",
            //            type: "POST",
            //            dataType: "json",
            //            data: { s: request.term },
            //            success: function (data) {
            //                response($.map(data, function (item) {
            //                    return { label: item.ID + " - " + item.TenDichVu_SanPham, value: item.ID};
            //                }))
            //            }
            //        })
            //    }
            //});
            MayChoThueController.enableField(id);
        });
        $('.updateMCT').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var tenmay = $('#tenmay_' + id).val();
                var model = $('#model_' + id).val();
                var congsuat = $('#congsuat_' + id).val();
                var ngaymua = $('#ngaymua_' + id).val();
                var iddichvu = $('#iddichvu_' + id).val();
                var ghichu = $('#ghichu_' + id).val();
                var status = $('#status_' + id).val();
                MayChoThueController.updateData(id, tenmay, model, congsuat, ngaymua, iddichvu , ghichu,status)
            }
        });
        //$('.col-md-10 #madichvu').autocomplete({
        //    source: function (request, response) {
        //        $.ajax({
        //            url: "/admin/BienBanNghiemThu/DropDownDV",
        //            type: "POST",
        //            dataType: "json",
        //            data: { s: request.term },
        //            success: function (data) {
        //                response($.map(data, function (item) {
        //                    return { label: item.ID + " - " + item.TenDichVu_SanPham, value: item.ID };
        //                }))
        //            }
        //        })
        //    }
        //});
        //Delete MAY
        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                MayChoThueController.DeleteMay(id);
            })
            //$(".modal-body #lydo").val(id);
        });
        // Xem lich su
        $('.btnXemLichSu').off('click').on('click', function () {
            var idmay = $(this).data('idmay');
            window.location.href = "/admin/MayChoThue/LichSuSuaChua?idmay=" + idmay;
        });
        // Xem lich su
        $('.btnLichSuThue').off('click').on('click', function () {
            var idmay = $(this).data('idmay');
            window.location.href = "/admin/MayChoThue/LichSuChoThue?idmay=" + idmay;
        });
        $('.btnXemKTV').off('click').on('click', function () {
            var idbbnt = $(this).data('idbbnt');
            MayChoThueController.LoadKTV(idbbnt);
        });
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
}

MayChoThueController.init();