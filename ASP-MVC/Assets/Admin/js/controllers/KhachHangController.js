var KhachHangController = {

    init: function () {
        KhachHangController.enableField();
        KhachHangController.registerEvent();
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
                pageLength: 100,
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

        $('.updateKH').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var hoten = $('#hoten_' + id).val();
                var sdt = $('#sdt_' + id).val();
                var diachi = $('#diachi_' + id).val();
                var ngaypv = $('#ngaypv_' + id).val();
                var loai = $('#loai_' + id).val();
                KhachHangController.updateData(id, hoten, sdt, diachi, ngaypv, loai);
            }
        });
    },

    updateData: function (id, hoten, sdt, diachi, ngaypv, loai) {
        var dataObject = {
            ID: id,
            HoTen: hoten,
            SDT: sdt,
            DiaChi: diachi,
            Loai: loai
        };
        var checkconfirm = confirm("Xác nhận lưu?");
        if (checkconfirm == true) {
            $.ajax({
                url: '/admin/KhachHang/Update',
                type: 'POST',
                datatype: 'json',
                data: ({ kh: JSON.stringify(dataObject), ngaypv:ngaypv }),
                success: function (response) {
                    if (response.status) {
                        alert("Cập nhật thành công");
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

    enableField: function () {
        $('.btnEditKH').on('click', function () {
            var id = $(this).data('idkh');
            if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
                $('#hoten_' + id).prop('hidden', false);
                $('#ht_' + id).prop('hidden', true);
                $('#sdt_' + id).prop('hidden', false);
                $('#sdtt_' + id).prop('hidden', true);
                $('#diachi_' + id).prop('hidden', false);
                $('#dc_' + id).prop('hidden', true);
                $('#ngaypv_' + id).prop('hidden', false);
                $('#npv_' + id).prop('hidden', true);
                $('#loai_' + id).prop('hidden', false);
                $('#loaihd_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#hoten_' + id).prop('hidden', true);
                $('#ht_' + id).prop('hidden', false);
                $('#sdt_' + id).prop('hidden', true);
                $('#sdtt_' + id).prop('hidden', false);
                $('#diachi_' + id).prop('hidden', true);
                $('#dc_' + id).prop('hidden', false);
                $('#ngaypv_' + id).prop('hidden', true);
                $('#npv_' + id).prop('hidden', false);
                $('#loai_' + id).prop('hidden', true);
                $('#loaihd_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var hoten = $('#hoten_' + id).val();
                var sdt = $('#sdt_' + id).val();
                var diachi = $('#diachi_' + id).val();
                var ngaypv = $('#ngaypv_' + id).val();
                var loai = $('#loai_' + id).val();
                KhachHangController.updateData(id, hoten, sdt, diachi, ngaypv, loai);
            }

        });
    },

}

KhachHangController.init();