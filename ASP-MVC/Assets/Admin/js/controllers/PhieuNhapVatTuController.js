var phieunhap = {
    init: function () {
        phieunhap.registerEvents();
        phieunhap.LapPhieu();
        phieunhap.enableField();
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
                phieunhap.updateData(id, ghichu);
            }
        });
        // them moi vat tu o form create phieu nhap
        $('.addNew').off('click').on('click', function () {
            var vattu = $('#valvattu').val();
            if (vattu != "")
                phieunhap.ThemVT(vattu);
            else
                alert("Bạn chưa nhập tên vật tư");
        });

        $(".TaoCT").off('click').on('click', function () {
            var str = $("#IDVatTu").val().split('.')
            var idvattu = str[0];
            var soluong = $(".col-md-10 #txtSoLuong").val();
            var ncc = $(".col-md-10 #txtNCC").val();
            var dongia = $(".col-md-10 #txtDonGia").val();
            if (idvattu != "" && soluong != "" && dongia != "" && idvattu > 0 && soluong > 0 && dongia > 0) {
                phieunhap.ThemCTPN(idvattu, soluong, dongia, ncc);
                phieunhap.resetctbbnt();
            }
            else
                alert("Vui lòng kiểm tra thông tin: Vật tư, đơn giá (>0), số lượng (>0)");
        });

        $(document).delegate(".delct", 'click', function () {
            var id = $(this).data('id');
            $('#row_' + id).remove();
        });

        $("#IDVatTu").change(function () {
            var str = $("#IDVatTu").val().split('.')
            var idvattu = str[0];
            $.ajax({
                url: "/admin/VatTu/LayVatTu",
                data: { idvattu: idvattu },
                dataType: "json",
                type: "POST",
                success: function (response) {
                    if (response.status) {
                        $(".col-md-10 #txtDonVi").val(response.vattu.DonVi);
                    }
                }
            })
        });
    },

    ThemCTPN: function (idvattu, soluong, dongia, ncc, ghichu) {
        $.ajax({
            url: "/admin/VatTu/ThemCTPN",
            data: { idvattu: idvattu },
            dataType: "json",
            type: "POST",
            success: function (response) {
                if (response.status) {
                    var s = '<tr id="row_' + response.vattu.ID + '" class="checkCT">' +
                        '<td class="getvalueidvattu">' + response.vattu.ID + '</td>' +
                        '<td>' + response.vattu.TenVatTu + '</td>' +
                        '<td class="getvaluesoluong">' + soluong + '</td>' +
                        '<td>' + response.vattu.DonVi + '</td>' +
                        '<td class="getvaluedongia">' + dongia + '</td>' +
                        '<td class="getvaluencc">' + ncc + '</td>' +
                        '<td><a data-id="' + response.vattu.ID + '" class="delct btn btn-circle">X</a></td>' +
                        '</tr > ';
                    $('#tableCT').append(s);
                    phieunhap.resetctbbnt();
                }
            }
        })
    },

    ThemVT: function (vattu) {
        $.ajax({
            url: "/admin/VatTu/ThemVT",
            data: { vattu: vattu },
            dataType: "json",
            type: "POST",
            success: function (response) {
                if (response.status) {
                    $('#IDVatTu').val(response.id + '. ' + response.tenvt);
                    $('#valvattu').val('');
                    var html = '<option value="' + response.id + '. ' + response.tenvt + '"></option>';
                    //window.location.reload();
                    $('#data').append(html);
                    alert("Thành công");
                }
                else
                    alert('Tên vật tư đã tồn tại, vui lòng kiểm tra lại');
            }
        })
    },

    resetctbbnt: function () {
        $("#IDVatTu").val('');
        $(".col-md-10 #txtSoLuong").val('');
        $(".col-md-10 #txtDonVi").val('');
        $(".col-md-10 #txtDonGia").val('');     
        $(".col-md-10 #txtNCC").val('');
    },

    LapPhieu: function () {
        $(".LapPhieu").off('click').on('click', function () {
            var checkconfirm = confirm("Xác nhận lưu?");
            if (checkconfirm == true) {
                if ($('#tableCT tr').hasClass('checkCT'))
                    phieunhap.TaoPhieuNhap();
                else
                    alert("Chưa tạo chi tiết phiếu nhập");
            }
            //else
            //    location.reload();
        })
    },

    TaoPhieuNhap: function () {
    var ghichu = $(".col-md-10 #ghichu").val().replace(/\n/g, ", ");;
        $.ajax({
            url: "/admin/VatTu/TaoPhieuNhap",
            type: "POST",
            data: { ghichu: ghichu },
            dataType: "json",
            success: function (response) {
                if (response.status == true) {
                    phieunhap.TaoCTPhieuNhap(response.idpn);
                    
                    window.location.href = "/admin/VatTu/PhieuNhap";
                }
            }
        })
    },

    TaoCTPhieuNhap: function (idpn) {
        $('#tableCT tr').each(function () {

            var idvattu = $(this).find(".getvalueidvattu").html();
            var soluong = $(this).find(".getvaluesoluong").html();
            var dongia = $(this).find(".getvaluedongia").html();
            var ncc = $(this).find(".getvaluencc").html();
            $.ajax({
                url: "/admin/VatTu/TaoCTPhieuNhap",
                data: { idpn: idpn, idvattu: idvattu, soluong: soluong, dongia: dongia, ncc: ncc},
                type: "POST",
                dataType: "json",
                success: function (response) {
                    if (response.status) {
                        alert("Thêm phiếu thành công");
                        console.log();
                    }
                }
            })
        });
        //table bên CTPhieuXuatKho

    },

    btnEditPhieuNhap: function () {
        $('.btnEditPhieuNhap').off('click').on('click', function () {

        })
    },

    updateData: function (id, ghichu) {
        var checkconfirm = confirm("Xác nhận lưu?");
        if (checkconfirm == true) {
            $.ajax({
                url: '/admin/VatTu/UpdatePhieuNhap',
                type: 'POST',
                datatype: 'json',
                data: { id: id, ghichu: ghichu },
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
        $('.btnEditPN').on('click', function () {
            var id = $(this).data('id');
            if ($('#ghichu_' + id).is(':disabled')) {
                $('#ghichu_' + id).prop('disabled', false);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#ghichu_' + id).prop('disabled', true);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var ghichu = $('#ghichu_' + id).val();
                phieunhap.updateData(id, ghichu);
            }

        });
    },
}
phieunhap.init();