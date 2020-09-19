var UserController = {

    init: function () {
        UserController.enableField();
        UserController.registerEvent();
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

        $('.updateUser').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var tenktv = $('#tenktv_'+id).val();
                var sdt = $('#sdt_' + id).val();
                var loai = $('#loai_' + id).val();
                var status = $('#status_' + id).val();
                UserController.updateData(id, tenktv, sdt, loai, status);
            }
        });

        //Reset 
        $('.Reset').off('click').on('click', function () {
            var idktv = $(this).data('id');
            $('.rsPass').off('click').on('click', function () {
                UserController.ResetPass(idktv);
            });
            $('.rsAll').off('click').on('click', function () {
                UserController.ResetAll(idktv);
            });
        });

        // Xem điểm KTV
        $('.XemDiem').off('click').on('click', function () {
            var idnv = $(this).data('idnv');
            UserController.LoadDiem(idnv);
        });

        //Xem dụng cụ đang mượn
        $(".dsDC").off('click').on('click', function () {
            var idktv = $(this).data('idnv');
            UserController.LoadDungCu(idktv);
        });
    },

    ResetAll: function (idktv) {
        $.ajax({
            url: "/admin/User/ResetAll",
            data: { idktv: idktv },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    alert(res.mess);
                }
            }
        })
    },

    ResetPass: function (idktv) {
        $.ajax({
            url: "/admin/User/RenewPassword",
            data: { idktv: idktv },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    alert(res.mess);
                }
            }
        })
    },

    LoadDiem: function (idnv) {
        $.ajax({
            url: "/admin/User/LoadDiem",
            data: { idnv: idnv },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-template').html();
                    $.each(data, function (i, item) {
                        html += Mustache.render(template, {
                            Thang: item.Thang,
                            Diem: item.Diem,
                        });
                    });
                    $('#table_body').html(html);
                }
            }
        })
    },

    //
    LoadDungCu: function (idktv) {
        $.ajax({
            url: "/admin/User/LoadDungCu",
            data: { idktv: idktv },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-templatedc').html();
                    if (data != "") {
                        $.each(data, function (i, item) {
                            html += Mustache.render(template, {
                                TenVatDung: item.TenDC,
                                NgayXuat: item.NgayXuat,
                                GhiChu: item.GhiChu,
                            });
                        });
                    }
                    else {
                        html += '<p style="text-align:center;column-span:2;color:blue">Không có dữ liệu</p>';
                        $('#table_bodydc').parent().html(html);
                    }
                    $('#table_bodydc').html(html);
                }
            }
        })
    },

    updateData: function (id, tenktv, sdt, loai, status) {
        var dataObject = {
            ID: id,
            TenKTV: tenktv,
            SDT: sdt,
            Loai: loai,
            Status: status
        };
        var cf = confirm("Xác nhận lưu?");
        if (cf == true) {
            $.ajax({
                url: '/admin/User/Update',
                type: 'POST',
                datatype: 'json',
                data: ({ nv: JSON.stringify(dataObject) }),
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
    },

    enableField: function () {
        $('.btnEditNV').on('click', function () {

            var id = $(this).data('idnv');

            if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
                $('#tenktv_' + id).prop('hidden', false);
                $('#tktv_' + id).prop('hidden', true);
                $('#sdt_' + id).prop('hidden', false);
                $('#dt_' + id).prop('hidden', true);
                $('#loai_' + id).prop('hidden', false);
                $('#lnv_' + id).prop('hidden', true);
                $('#status_' + id).prop('hidden', false);
                $('#sta_' + id).prop('hidden', true);
                    $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#tenktv_' + id).prop('hidden', true);
                $('#tktv_' + id).prop('hidden', false);
                $('#sdt_' + id).prop('hidden', true);
                $('#dt_' + id).prop('hidden', false);
                $('#loai_' + id).prop('hidden', true);
                $('#lnv_' + id).prop('hidden', false);
                $('#status_' + id).prop('hidden', true);
                $('#sta_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');
                var tenktv = $('#tenktv_' + id).val();
                var sdt = $('#sdt_' + id).val();
                var loai = $('#loai_' + id).val();
                var status = $('#status_' + id).val();
                UserController.updateData(id, tenktv, sdt, loai, status)
            }
               
        });
    },

}

UserController.init();