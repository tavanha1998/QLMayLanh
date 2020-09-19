var phieuxuat_ktv = {
    init: function () {
        phieuxuat_ktv.registerEvents();
        phieuxuat_ktv.LapPhieu();
        phieuxuat_ktv.enableField();
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
        //Edit ct phiếu
        $('.chinhsuact').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            if ($('#valSLTra_' + id).is(':hidden')) {
                $('#valSLTra_' + id).prop('hidden', false);
                $('#SLTra_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#valSLTra_' + id).prop('hidden', true);
                $('#SLTra_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var SLTra = $('#valSLTra_' + id).val();
                if (SLTra != "" && SLTra > 0)
                    phieuxuat_ktv.updateCT(id, SLTra);
                else
                    alert("Số lượng trả phải lớn hơn 0");
            }
        });

        $('.updateSLTra').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var SLTra = $('#valSLTra_' + id).val();
                phieuxuat_ktv.updateCT(id, SLTra);
            }
        });

        $('.updatePN').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var ghichu = $('#ghichu_' + id).val();
                phieuxuat_ktv.updateData(id, ghichu);
            }
        });
        var index = 1;
        $(".TaoCT").off('click').on('click', function () {
            var str = $("#IDVatTu").val().split('.');
            var idvattu = str[0];
            var soluong = $(".col-md-15 #valsoluong").val();
            var dongia = $(".col-md-15 #valdongia").val();
            if (idvattu != "" && soluong != "" && dongia != "" && idvattu > 0 && soluong > 0 && dongia > 0) {
                phieuxuat_ktv.ThemCTPN(idvattu, soluong, dongia, index);
                index++;
                //phieuxuat_ktv.resetctbbnt();
            }
            else
                alert("Vui lòng kiểm tra thông tin: Vật tư, đơn giá (>0), số lượng (>0)");
        });

        $(document).delegate(".delct", 'click', function () {
            var id = $(this).data('id');
            $('#row_' + id).remove();
        });

        $('#IDVatTu').change(function () {
            phieuxuat_ktv.autoDonGia();
        });

        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                phieuxuat_ktv.DeletePhieu(id);
            })
            //$(".modal-body #lydo").val(id);
        });

        $('.OpendialogCT').off('click').on('click', function (e) {
            e.preventDefault();
            var idct = $(this).data('id');
            $('.XoaCT').off('click').on('click', function (f) {
                f.preventDefault();
                phieuxuat_ktv.DeleteCTPhieu(idct);
            })
            //$(".modal-body #lydo").val(id);
        });
    },

    updateCT: function (id, SLTra) {
        $.ajax({
            url: '/admin/VatTu/updateCT',
            type: 'POST',
            dataType: 'json',
            data: { id: id, SLTra: SLTra },
            success: function (res) {
                if (res.status) {
                    alert(res.mess);
                    window.location.reload();
                }
                else
                    alert(res.mess);
            }
        })
    },

    DeletePhieu: function (id) {
        $.ajax({
            url: '/admin/VatTu/DeletePX_KTV',
            type: 'POST',
            dataType: 'json',
            data: { id: id },
            success: function (res) {
                if (res.status) {
                    $('#row_' + id).remove();
                    alert("Xóa thành công");
                }
            }
        })
    },

    DeleteCTPhieu: function (idct) {
        $.ajax({
            url: '/admin/VatTu/DeleteCTPhieu',
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

    ThemCTPN: function (idvattu, soluong, dongia,index) {
        $.ajax({
            url: "/admin/VatTu/ThemCTPX_KTV",
            data: { idvattu: idvattu, soluong:soluong },
            dataType: "json",
            type: "POST",
            success: function (response) {
                if (response.status) {
                    var s = '<tr id="row_' + index + '">' +
                        '<td class="getvalueidvattu">' + response.vattu.ID + '</td>' +
                        '<td>' + response.vattu.TenVatTu + '</td>' +
                        '<td class="getvaluesoluong">' + soluong + '</td>' +
                        '<td>' + response.vattu.DonVi + '</td>' +
                        '<td class="getvaluedongia">' + dongia + '</td>' +
                        '<td><a data-id="' + index + '" class="delct btn btn-circle">X</a></td>' +
                        '</tr > ';
                    $('#tableCT').append(s);
                    phieuxuat_ktv.resetctbbnt();
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
                var str = $("#IDKTV").val().split('.');
                var idktv = str[0];
                var strt = $("#IDYCPV").val().split('.');
                var idycpv = strt[0];
                var ghichu = $("#ghichu").val();

                if (idktv != "" && idktv > 0 && idycpv != "" && idycpv > 0) {

                    var checkconfirm = confirm("Xác nhận lưu?");
                    if (checkconfirm == true) {
                        phieuxuat_ktv.ListCTPX(listCT);
                        phieuxuat_ktv.TaoPhieu(idktv, idycpv, ghichu, listCT);
                    }
                    //else
                    //    location.reload();
                }
                else
                    alert("Bạn chưa nhập đủ thông tin phiếu!");
            }
            else
                alert("Bạn chưa nhập thông tin chi tiết");
        })
    },

    TaoPhieu: function (idktv, idycpv, ghichu, list) {
        $.ajax({
            url: "/admin/VatTu/TaoPhieuXuat_KTV",
            type: "POST",
            data: { idktv: idktv, idycpv:idycpv,ghichu:ghichu,list:list},
            dataType: "json",
            success: function (response) {
                if (response.status == true) {
                    //phieuxuat_ktv.TaoCTPhieuXuat(response.idphieu);
                    alert("Thêm phiếu thành công");
                    window.location.href = "/admin/VatTu/PhieuXuat_KTV";
                }
                else {
                    alert(response.mess);
                    list.length = 0;
                }
            }
        })
    },

    ListCTPX: function(listCT) {
        $('#tableCT tr').each(function () {
            var obj = {
                IDVatTu: $(this).find(".getvalueidvattu").html(),
                SLLay: $(this).find(".getvaluesoluong").html(),
                DonGiaJSON: $(this).find(".getvaluedongia").html()
            };
            listCT.push(obj);
        })
            
    },

    //btnEditPhieuXuat: function () {
    //    $('.btnEditPX').off('click').on('click', function () {

    //    })
    //},

    updateData: function (id, ghichu) {
        var checkconfirm = confirm("Xác nhận lưu?");
        if (checkconfirm == true) {
            $.ajax({
                url: '/admin/VatTu/UpdatePhieuXuat_KTV',
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
        $('.btnEditPX').on('click', function () {
            var id = $(this).data('id');
            if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
                $('#ghichu_' + id).prop('hidden', false);
                $('#gc_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#ghichu_' + id).prop('disabled', true);
                $('#gc_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var ghichu = $('#ghichu_' + id).val();
                phieuxuat_ktv.updateData(id, ghichu);
            }

        });
    },
}
phieuxuat_ktv.init();