var phieuxuat_khach = {
    init: function () {
        phieuxuat_khach.registerEvents();
        phieuxuat_khach.LapPhieu();
        phieuxuat_khach.enableField();
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

        $('.updatePN').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var ghichu = $('#ghichu_' + id).val();
                var tenkhach = $('#tenkhach_' + id).val();
                phieuxuat_khach.updateData(id, ghichu, tenkhach);
            }
        });

        $(".TaoCT").off('click').on('click', function () {
            var str = $("#IDVatTu").val().split('.');
            var idvattu = str[0];
            var soluong = $(".col-md-15 #valsoluong").val();
            var dongia = $(".col-md-15 #valdongia").val();
            if (idvattu != "" && soluong != "" && dongia != "" && idvattu > 0 && soluong > 0 && dongia > 0) {
                phieuxuat_khach.ThemCTPN(idvattu, soluong, dongia);
                //phieuxuat_khach.resetctbbnt();
            }
            else
                alert("Vui lòng kiểm tra thông tin: Vật tư, đơn giá (>0), số lượng (>0)");
        });

        $(document).delegate(".delct", 'click', function () {
            var id = $(this).data('id');
            $('#row_' + id).remove();
        });

        $('#IDVatTu').change(function () {
            phieuxuat_khach.autoDonGia();
        });

        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                phieuxuat_khach.DeletePhieu(id);
            })
            //$(".modal-body #lydo").val(id);
        });
        $('.OpendialogCT').off('click').on('click', function (e) {
            e.preventDefault();
            var idct = $(this).data('id');
            $('.XoaCT').off('click').on('click', function (f) {
                f.preventDefault();
                phieuxuat_khach.DeleteCTPhieu_Khach(idct);
            })
            //$(".modal-body #lydo").val(id);
        });
    },

    DeleteCTPhieu_Khach: function (idct) {
        $.ajax({
            url: '/admin/VatTu/DeleteCTPhieu_Khach',
            type: 'POST',
            dataType: 'json',
            data: { idct: idct },
            success: function (res) {
                if (res.status) {
                    $('#row_' + idct).remove();
                    alert("Xóa thành công");
                }
            }
        })
    },

    DeletePhieu: function (id) {
        $.ajax({
            url: '/admin/VatTu/DeletePX_Khach',
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

    ThemCTPN: function (idvattu, soluong, dongia) {
        $.ajax({
            url: "/admin/VatTu/ThemCTPX_KTV",
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
                    phieuxuat_khach.resetctbbnt();
                }
                else
                    alert(response.mess);
            }
        })
    },

    autoDonGia: function () {
        var str = $('#IDVatTu').val().split('.');
        var idvattu = str[0];
        if (idvattu != "" && idvattu > 0) {
            $.ajax({
                url: "/admin/VatTu/autoDonGia",
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

    resetctbbnt: function () {
        $("#IDVatTu").val('');
        $(".col-md-15 #valsoluong").val('');
        $(".col-md-15 #valdongia").val('');
    },

    LapPhieu: function () {
        var listCT = [];
        $(".LapPhieu").off('click').on('click', function () {
            var table = $('#tableCT tr').length;
            if (table > 0) {
                var tenKH = $("#valTenKhach").val();
                var ghichu = $("#ghichu").val();

                if (tenKH != "") {

                    var checkconfirm = confirm("Xác nhận lưu?");
                    if (checkconfirm == true) {
                        phieuxuat_khach.ListCTPX(listCT);
                        phieuxuat_khach.TaoPhieu(tenKH, ghichu, listCT);
                    }
                    //else
                    //    location.reload();
                }
                else
                    alert("Thông tin phiếu còn thiếu tên khách hàng!");
            }
            else
                alert("Bạn chưa nhập thông tin chi tiết");
        })
    },

    ListCTPX: function (listCT) {
        $('#tableCT tr').each(function () {
            var obj = {
                IDVatTu: $(this).find(".getvalueidvattu").html(),
                SoLuong: $(this).find(".getvaluesoluong").html(),
                DonGiaJSON: $(this).find(".getvaluedongia").html()
            };
            listCT.push(obj);
        })

    },

    TaoPhieu: function (tenKH, ghichu, listCT) {
        $.ajax({
            url: "/admin/VatTu/TaoPhieuXuat_Khach",
            type: "POST",
            data: { tenKH: tenKH, ghichu: ghichu, list: listCT },
            dataType: "json",
            success: function (response) {
                if (response.status == true) {
                    alert("Thêm phiếu thành công");
                    window.location.href = "/admin/VatTu/PhieuXuat_Khach";
                }
            }
        })
    },

    

    //btnEditPhieuXuat: function () {
    //    $('.btnEditPX').off('click').on('click', function () {

    //    })
    //},

    updateData: function (id, ghichu, tenkhach) {
        var checkconfirm = confirm("Xác nhận lưu?");
        if (checkconfirm == true) {
            $.ajax({
                url: '/admin/VatTu/UpdatePhieuXuat_Khach',
                type: 'POST',
                datatype: 'json',
                data: { id: id, ghichu: ghichu, tenkhach:tenkhach },
                success: function (response) {
                    if (response.status) {
                        alert("Cập nhật thành công");
                        location.reload();
                    }
                    else {
                        alert("cập nhật thất bại");
                        location.reload();
                    }
                }
            })
        }
        else
            location.reload();
    },

    enableField: function () {
        $('.btnEditPX').on('click', function () {
            var id = $(this).data('id');
            if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
                $('#ghichu_' + id).prop('hidden', false);
                $('#gc_' + id).prop('hidden', true);
                $('#tenkhach_' + id).prop('hidden', false);
                $('#tk_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#ghichu_' + id).prop('hidden', true);
                $('#gc_' + id).prop('hidden', false);
                $('#tenkhach_' + id).prop('hidden', true);
                $('#tk_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var ghichu = $('#ghichu_' + id).val();
                var tenkhach = $('#tenkhach_' + id).val();
                phieuxuat_khach.updateData(id, ghichu, tenkhach);
            }

        });
    },
}
phieuxuat_khach.init();