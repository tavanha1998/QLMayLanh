var KhoController = {

    init: function () {
        KhoController.btnDangMuon();
        KhoController.enableField();
        KhoController.registerEvent();
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

        $('.updateKho').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var tenvatdung = $('#tenvatdung_' + id).val();
                //var dongia = $('#dongia_' + id).val();
                //var soluong = $('#soluong_' + id).val();
                var ngaymua = $('#ngaymua_' + id).val();
                var ghichu = $('#ghichu_' + id).val();
                KhoController.updateData(id, tenvatdung, ngaymua, ghichu);
            }
        });
        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                KhoController.DeleteDV(id);
            })
            //$(".modal-body #lydo").val(id);
        });

        
    },
    DeleteDV: function (id) {
        $.ajax({
            url: '/admin/Kho/Delete',
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
    updateData: function (id, tenvatdung, ngaymua, ghichu) {
        var dataObject = {
            ID: id,
            TenVatDung: tenvatdung,
            //DonGia: dongia,
            //SoLuong: soluong,
            GhiChu: ghichu
        };
        var cf = confirm("Xác nhận lưu?");
        if (cf == true) {
            $.ajax({
                url: '/admin/Kho/Update',
                type: 'POST',
                datatype: 'json',
                data: ({ kho: JSON.stringify(dataObject), ngaymua: ngaymua }),
                success: function (response) {
                    if (response.status) {
                        alert("Cập nhập thành công");
                        location.reload();
                    }
                    else {
                        alert(response.mess);
                        location.reload();
                    }
                }
            })
        }
        else
            location.reload();
    },
    btnDangMuon: function () {
        $('.btnDangMuon').off('click').on('click', function () {
            var idvatdung = $(this).data('id');
            $.ajax({
                url: '/admin/Kho/btnDangMuon',
                type: 'POST',
                datatype: 'json',
                data: { idvatdung: idvatdung },
                success: function (response) {
                    if (response.status) {
                        var idpx = response.idphieuxuat;
                        window.location.href = "/admin/Kho/CTPhieuXuatKho?idphieuxuat=" + idpx;
                    }
                    else {
                        alert("Có lỗi !");
                    }
                }
            })
        })
    },
    enableField: function () {
        $('.btnEditkho').on('click', function () {

            var id = $(this).data('id');
            if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
                $('#tenvatdung_' + id).prop('hidden', false);
                $('#tvd_' + id).prop('hidden', true);
                $('#ngaymua_' + id).prop('hidden', false);
                $('#nm_' + id).prop('hidden', true);
                $('#ghichu_' + id).prop('hidden', false);
                $('#gc_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#tenvatdung_' + id).prop('hidden', true);
                $('#tvd_' + id).prop('hidden', false);
                $('#ngaymua_' + id).prop('hidden', true);
                $('#nm_' + id).prop('hidden', false);
                $('#ghichu_' + id).prop('hidden', true);
                $('#gc_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');
                var tenvatdung = $('#tenvatdung_' + id).val();
                var ngaymua = $('#ngaymua_' + id).val();
                var ghichu = $('#ghichu_' + id).val();
                KhoController.updateData(id, tenvatdung, ngaymua, ghichu)
            }

        });
    },
    
}

KhoController.init();