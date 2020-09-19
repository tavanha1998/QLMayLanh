var ycpv = {
    init: function () {
        //ycpv.TimKHCreate();
        ycpv.SelectOption();
        //ycpv.btnDSMayChoThue();
        ycpv.SendID();
        ycpv.enterupdate();
        ycpv.registerEvents();
        
    },

    TimKHCreate: function () {
        $('.searchKH').off('click').on('click', function () {
            var s = $('#timKH').val();
            $.ajax({
                url: '/admin/admin/searchKH',
                type: 'POST',
                dataType: 'json',
                data: { s: s },
                success: function (res) {
                    if (res.status) {
                        window.location.href = "/admin/admin/Create?timKH=" + s;
                    }
                }
            })
        })
        },

    SelectOption: function () {
            $('.bbnt').off('click').on('click', function () {
                var id = $(this).data('id');
                var idkh = $(this).data('idkh');
                window.location.href = "/admin/BienBanNghiemThu?stt=" + id + "&kh=" + idkh;
            })
            //$('.chinhsua').off('click').on('click', function () {
            //    var id = $(this).data('id');
            //    window.location.href = "/admin/admin/Edit/" + id;
            //})
        },
    SendID: function () {
        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                var val = $('#lydo').val();
                ycpv.DeleteYCPV(id, val);
            }) 
            //$(".modal-body #lydo").val(id);
        })
    },
    DeleteYCPV: function (id, value) {
        var data = {
            ID: id,
            LiDo: value
        };
        $.ajax({
            url: '/admin/admin/Delete',
            type: 'POST',
            dataType: 'json',
            data: {model: JSON.stringify(data)},
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
                $('td:nth-child(' + num + '), th:nth-child(' + num + ')').hide();
            else
                $('td:nth-child(' + num + '), th:nth-child(' + num + ')').show();
        });
        //if ($('#checkbox0').is(':checked'))
        //    $('td:nth-child(0),th:nth-child(0)').hide();

        $(document).ready(function () {
            $.fn.dataTable.moment('DD/MM/YYYY');
            $('#dataTable').DataTable({
                autoWidth: true,
                scrollX: true,
                pageLength: 10,
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

        $(document).ready(function () {
            $('#dataTable1').DataTable({
                autoWidth: true,
                scrollX: true,
                pageLength: 10,
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

        $('.changestatus').off('click').on('click', function (e) {
            e.preventDefault();
            var change = $(this);
            var id = change.data('id');
            var status = change.data('status');
            var icon = $('.iconstatus_'+id);
            if (status == 2) {
                var r = confirm("Yêu cầu đang được xử lý (Đang làm)?")
                if (r == true) {
                    $.ajax({
                        url: "/admin/admin/ChangeStatus",
                        data: { id: id },
                        dataType: "json",
                        type: "POST",
                        success: function (response) {
                            if (response.status == 1) {
                                //$('#page-top').load("/admin/admin");
                                icon.removeClass();
                                icon.addClass("fas fa-people-carry iconstatus_" + id);
                                change.prop('title', 'Đang làm');
                                change.removeClass();
                                change.addClass("btn btn-warning btn-sm changestatus");
                            }
                        }
                    })
                }
            }
        });
        $('#filterKH').on('keyup', function () {
            var value = $(this).val().toLowerCase();
            $('#tableKH tr').filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        $('.idKH').off('click').on('click', function () {
            var id = $(this).data('id');
            $(".col-md-10 #idKH").val(id);
        });

        //$(".col-md-10 #idKH").autocomplete({
        //    source: function (request, response) {
        //        $.ajax({
        //            url: "/admin/admin/DropDownKH",
        //            type: "POST",
        //            dataType: "json",
        //            data: { s: request.term },
        //            success: function (data) {
        //                response($.map(data, function (item) {
        //                    return { label: item.HoTen + " - " + item.SDT, value: item.HoTen + " - " + item.SDT, idKH: item.ID, diachi: item.DiaChi };
        //                }))
        //            }
        //        })
        //    },
        //    select: function (event, ui) {
        //        $(".col-md-10 #valueKH").val(ui.item.idKH);
        //        $('.col-md-10 #DiaChiKH').val(ui.item.diachi);
        //    }
        //});

        //Them KH mới trong yều cầu
        $('.addKH').off('click').on('click',function(){
            var ht = $("#hoten").val();
            var sdt = $("#sdt").val();
            var diachi = $("#DiaChi").val();
            if (ht != "" && sdt != "")
                $.ajax({
                    url: "/admin/admin/ThemKH",
                    type: "POST",
                    dataType: "json",
                    data: { ht: ht, sdt: sdt, diachi: diachi },
                    success: function (res) {
                        if (res.status) {
                            //window.location.reload();
                            var html = '<option value="'+res.id+'. '+res.name+'"></option>';
                            $('#dataKH').append(html);
                            $('#idKH').val(res.id+'. '+res.name);
                            $(".col-md-10 #valueKH").val(res.id);
                            $('.col-md-10 #DiaChiKH').val(res.addr);
                        }
                    }
                })
            else
                alert("Vui lòng nhập đủ thông tin khách hàng!");
        });
            
        var check = $(".col-md-10 #Loai").val();
        if (check == "2") {
            $(".col-md-10 #dukien").removeAttr("disabled");
        }
        else
            $(".col-md-10 #dukien").attr("disabled", "disabled");

        $(".col-md-10 #Loai").change(function () {
            var value = $(this).val();
            if (value.toString() == "2") {
                $(".col-md-10 #dukien").removeAttr("disabled");
            }
            else
                $(".col-md-10 #dukien").attr("disabled", "disabled");
        });

        $(".chinhsua").off('click').on('click', function () {
            var id = $(this).data('id');
           
            if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
                $('#valdiachi_' + id).prop('hidden', false);
                $('#diachi_' + id).prop('hidden', true);
                $('#valyeucau_' + id).prop('hidden', false);
                $('#yeucau_' + id).prop('hidden', true);
                $('#valngaybatdau_' + id).prop('hidden', false);
                $('#ngaybatdau_' + id).prop('hidden', true);
                $('#valngaylamtiep_' + id).prop('hidden', false);
                $('#ngaylamtiep_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#valdiachi_' + id).prop('hidden', true);
                $('#diachi_' + id).prop('hidden', false);
                $('#valyeucau_' + id).prop('hidden', true);
                $('#yeucau_' + id).prop('hidden', false);
                $('#valngaybatdau_' + id).prop('hidden', true);
                $('#ngaybatdau_' + id).prop('hidden', false);
                $('#valngaylamtiep_' + id).prop('hidden', true);
                $('#ngaylamtiep_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var diachi = $('#valdiachi_' + id).val();
                var yeucau = $('#valyeucau_' + id).val();
                var ngaybatdau = $('#valngaybatdau_' + id).val();
                var ngaylamtiep = $('#valngaylamtiep_' + id).val();
                ycpv.updateData(id, diachi, yeucau, ngaybatdau, ngaylamtiep)
            }
        });

        //Lay gia tri khi selectitem
        $('.col-md-10 #idKH').on('change', function () {
            var value = $(this).val();
            value = value.split('.')[0];
            if (value != '' && value > 0)
            {
                $.ajax({
                    url: "/admin/admin/timKH",
                    type: "POST",
                    dataType: "json",
                    data: { idkh: value },
                    success: function (data) {
                        $('#valueKH').val(value);
                        $('#DiaChiKH').val(data);
                    }
                })
            }
            else {
                $('.col-md-10 #idKH').val('');
                $('#DiaChiKH').val('');
            }
        })
        
    },
    updateData: function (id, diachi, yeucau, ngaybatdau, ngaylamtiep) {
        var dataObject = {
            ID: id,
            DiaChiPhucVu: diachi,
            YeuCau: yeucau,
        };
        var cf = confirm("Xác nhận lưu?");
        if (cf == true) {
            $.ajax({
                url: '/admin/admin/Update',
                type: 'POST',
                datatype: 'json',
                data: ({ ycpv: JSON.stringify(dataObject), ngaybatdau: ngaybatdau, ngaylamtiep: ngaylamtiep }),
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

    enterupdate: function () {
        $('.updateYCPV').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var diachi = $('#valdiachi_' + id).val();
                var yeucau = $('#valyeucau_' + id).val();
                var ngaybatdau = $('#valngaybatdau_' + id).val();
                var ngaylamtiep = $('#valngaylamtiep_' + id).val();
                ycpv.updateData(id, diachi, yeucau, ngaybatdau, ngaylamtiep);
            }
        });
    },

}
ycpv.init();