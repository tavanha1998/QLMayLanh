var PhieuXuatKho = {
    init: function () {
        PhieuXuatKho.registerEvent();
        PhieuXuatKho.chonVatDung();
        PhieuXuatKho.btnLapphieu();
        PhieuXuatKho.btnChuaTra();
        PhieuXuatKho.enableField();
        PhieuXuatKho.btnXoaChitietPhieuXuat();
        PhieuXuatKho.btnAddVatDung();
        PhieuXuatKho.btnaddvdclass();
    },
    registerEvent: function () {

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
            $.fn.dataTable.moment('HH:mm, DD/MM/YYYY');
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

        $('.updatePX').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            var idnv = $('#valKTV_' + id).val();
            if (e.which == 13) {
                PhieuXuatKho.updateData(id, idnv);
            }
        });
        //search vật dụng realtime
        $('#searchrealtime').on('keyup', function () {
            var value = $(this).val().toLowerCase();
            $('#tableCT tr').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        $('#searchrealtime').on('keyup', function () {
            var value = $(this).val().toLowerCase();
            $('#tableCTPX tr').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        //$(".col-md-10 #idNV").autocomplete({
        //    source: function (request, response) {
        //        $.ajax({
        //            url: "/admin/Kho/DSNhanVien",
        //            type: "POST",
        //            dataType: "json",
        //            data: { s: request.term },
        //            success: function (data) {
        //                response($.map(data, function (item) {
        //                    return { label: item.TenKTV + " - " + item.SDT, value: item.TenKTV + " - " + item.SDT, id: item.ID };
        //                }))
        //            }
        //        })
        //    },
        //    select: function (event, ui) {
        //        $("#valueKTV").val(ui.item.id);
        //    }
        //});
    },
    btnLapphieu: function () {
        $('.btnLap').off('click').on('click', function () {
            var checkconfirm = confirm("Xác nhận lưu?");
            if (checkconfirm == true) {
                PhieuXuatKho.ThemPhieuXuat();
            }
        })
        
    },

    btnaddvdclass: function () {
        $('.addvdclass').off('click').on('click', function () {
            var checkconfirm = confirm("Xác nhận thêm ?");
            if (checkconfirm == true) {
                PhieuXuatKho.LuuCTPhieuXuat();
            }
        })
    },

    btnChuaTra: function () {
        $('.btnStatus').off('click').on('click', function () {
            
            var id = $(this).data('idpx');
            var status = $(this).data('status');
            if (status == 0) {
                var checkconfirm = confirm("Xác nhận đã trả ?");
                if (checkconfirm == true) {
                    $('#icontinhtrang_' + id).toggleClass('fas fa-minus-square fas fa-check-square');
                    $(this).removeClass('btn-danger').addClass('btn-success');
                    $.ajax({
                        url: "/admin/Kho/ChangeStatusPhieu",
                        data: { idphieuxuat: id },
                        type: "POST",
                        dataType: "json",
                        success: function (response) {
                            if (response.status) {
                                location.reload();
                                console.log();
                            }
                        }
                    })
                }
            }
            else {
                var checkconfirm = confirm("Hủy đã trả ?");
                if (checkconfirm == true) {
                    $('#icontinhtrang_' + id).toggleClass('fas fa-check-square fas fa-minus-square');
                    $(this).removeClass('btn-success').addClass('btn-danger');
                    $.ajax({
                        url: "/admin/Kho/ChangeStatusPhieu",
                        data: { idphieuxuat: id },
                        type: "POST",
                        dataType: "json",
                        success: function (response) {
                            if (response.status) {
                                location.reload();
                                console.log();
                            }
                        }
                    })
                }
            }
            
        })
    },
    
    enableField: function () {
        $('.btnEditPhieuXuat').on('click', function () {
            var id = $(this).data('id');
            var status = $(this).data('status');

            //$('#tennv_' + id).autocomplete({
            //    source: function (request, response) {
            //        $.ajax({
            //            url: "/admin/Kho/DSNhanVien",
            //            type: "POST",
            //            dataType: "json",
            //            data: { s: request.term },
            //            success: function (data) {
            //                response($.map(data, function (item) {
            //                    return { label: item.TenKTV + " - " + item.SDT, value: item.TenKTV + " - " + item.SDT, id: item.ID };
            //                }))
            //            }
            //        })
            //    },
            //    select: function (event, ui) {
            //        $("#valKTV_" + id).val(ui.item.id);
            //    }
                    
            //});

            if ($('#tennv_' + id).is(':disabled')) {
                $('#tennv_' + id).prop('disabled', false);
                if (status != 0)
                    $('#ngaytra_' + id).prop('disabled', false);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#tennv_' + id).prop('disabled', true);
                $('#ngaytra_' + id).prop('disabled', true);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');
                var idNV = $('#valKTV_' + id).val();
                PhieuXuatKho.updateData(id, idNV);
            }

            //if ($('#ngaytra_' + id).is(':disabled')) {
            //    if (status != 0)
            //        $('#ngaytra_' + id).prop('disabled', false);
            //    $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            //}
            //else {
            //    $('#ngaytra_' + id).prop('disabled', true);
            //    $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

            //}
            
        });
    },

    updateData: function (id, idNV) {
        var dataObject = {
            ID: id,
            IDKTV: idNV
        };
        var checkconfirm = confirm("Xác nhận lưu?");
        if (checkconfirm == true) {
            $.ajax({
                url: '/admin/Kho/UpdatePhieuXuat',
                type: 'POST',
                datatype: 'json',
                data: ({ px: JSON.stringify(dataObject)}),
                success: function (response) {
                    if (response.status) {
                        alert("Cập nhập thành công");
                        location.reload();
                    }
                    else {
                        alert("Cập nhập thất bại");
                        location.reload();
                    }
                }
            })
        }
        else
            location.reload();
    },

    chonVatDung: function () {
        $('.btnchonVatDung').off('click').on('click', function () {
            
            var id = $(this).data('idvatdung');
            if ($('#iconchondungcu_' + id).hasClass("fas fa-check-square")) {
                $('#row_' + id).css('background-color', "rgba(0,0,0,.075)");
                $('#btnChon_' + id).css('background-color', "#e74a3b");
                $('#btnChon_' + id).css('border-color','#e74a3b');
                $('#iconchondungcu_' + id).toggleClass('fas fa-check-square fas fa-minus-square');
                $('#id_' + id).addClass("getidvatdung");
            }
            else {
                $('#row_' + id).css("background-color", "");
                $('#btnChon_' + id).css('background-color', "");
                $('#btnChon_' + id).css('border-color', '');
                $('#iconchondungcu_' + id).toggleClass('fas fa-minus-square fas fa-check-square');
                $('#id_' + id).removeClass("getidvatdung");
            }
            
        })
    },

    LuuCTPhieuXuat: function (idpx,idktv) {
        //table bên CreatePhieuXuatKho
        $('#tableCT tr').each(function () {
            
            var idvatdung = $(this).find(".getidvatdung").html();
            $.ajax({
                url: "/admin/Kho/LuuCTPhieuXuat",
                data: { idvatdung: idvatdung, idktv: idktv, idpx: idpx },
                type: "POST",
                dataType: "json",
                success: function (response) {
                    if (response.status) {
                        console.log();
                    }
                }
            })
        });
        //table bên CTPhieuXuatKho
        
    },
    ThemPhieuXuat: function () {
        var str = $('#idNV').val().split('.');
        var idktv = str[0];
        if (idktv != "" && idktv > 0) {
            $.ajax({
                url: "/admin/Kho/ThemPhieuXuat",
                data: { idktv: idktv },
                type: "POST",
                dataType: "json",
                success: function (response) {
                    if (response.status == true) {
                        PhieuXuatKho.LuuCTPhieuXuat(response.idpx,idktv);
                        alert("Thêm phiếu thành công");
                        window.location.href = "/admin/Kho/PhieuXuatKho";
                    }
                    else {
                        alert("Kĩ thuật viên chỉ được mượn 1 lần trong ngày");
                    }
                }
            })
        }
        else
            alert("Bạn chưa nhập KTV hoặc sai thông tin");
    },
    btnXoaChitietPhieuXuat: function () {
        $('.btnXoaVatDung').off('click').on('click', function () {
            var idctpx = $(this).data('idctpx');
            var checkconfirm = confirm("Xác nhận xóa ?");
            if (checkconfirm == true) {
                $.ajax({
                    url: "/admin/Kho/XoaChitietPhieuXuat",
                    data: { idctpx: idctpx },
                    type: "POST",
                    dataType: "json",
                    success: function (response) {
                        if (response.status) {
                            console.log();
                            location.reload();
                        }
                    }
                })
            }
            
        })
    },

    btnAddVatDung: function () {
        
        $('.btnAddVatDung').off('click').on('click', function () {
            if ($('#lbl').is(':hidden')) {
                $('#lbl').prop('hidden', false);
                $('#tblVatDung').prop('hidden', false);
                $('#btnadd').prop('value', 'Hủy');
                $('#btnadd').css('background-color', "#e74a3b");
                $('#btnadd').css('border-color', "#e74a3b");
            }
            else {
                $('#lbl').prop('hidden', true);
                $('#tblVatDung').prop('hidden', true);
                $('#btnadd').prop('value', '+ Thêm vật dụng');
                $('#btnadd').css('background-color', "#1cc88a");
                $('#btnadd').css('border-color', "#1cc88a");
            }
        })
    },
    
}
PhieuXuatKho.init();