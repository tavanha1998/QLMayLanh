var vattu = {
    init: function () {
        vattu.registerEvent();
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
                    "decimal": ",",
                    "thousands": ".",
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


        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                vattu.DeleteDV(id);
            })
            //$(".modal-body #lydo").val(id);
        });

        //$(document).ready(function () {
        //    $('#dtBasicExample').DataTable();
        //    $('.dataTables_length').addClass('bs-select');
        //    $('.dataTables_length').css("float", "left");
        //    $('.dataTables_filter').css("float", "right");
        //});
        //$('#dtBasicExample tr > *:nth-child(2)').hide();
        $('.btnEditVatTu').on('click', function () {
            var id = $(this).data('id');
            vattu.enableField(id);
        });

        $('.btnLichSu').on('click', function () {
            var id = $(this).data('id');
            vattu.LoadLichSu(id);
        });

        $('.updateVatTu').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            if (e.which == 13) {
                var giaban = $('#valgiaban_' + id).val();
                var donvi = $('#valdonvi_' + id).val();
                var sl = $('#valslg_' + id).val();
                //var ghichu = $('#ghichu_' + id).val();
                vattu.updateData(id, giaban, donvi,sl);
            }
        });
    },
    updateData: function (id, giaban, donvi, sl) {
        if (sl > 0 && giaban > 0) {
            var cf = confirm("Xác nhận lưu?");
            if (cf == true) {
                $.ajax({
                    url: '/admin/VatTu/Update',
                    type: 'POST',
                    datatype: 'json',
                    data: { id: id, giaban: giaban, donvi: donvi, sl: sl },
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
        }
        else
            alert('Số lượng/Giá bán không được để trống và phải > 0')
    },
    enableField: function (id) {
        if ($('#valgiaban_' + id).is(':hidden')) {
            $('#valgiaban_' + id).prop('hidden', false);
            $('#giaban_' + id).prop('hidden', true);
            $('#valdonvi_' + id).prop('hidden', false);
            $('#donvi_' + id).prop('hidden', true);
            $('#valslg_' + id).prop('hidden', false);
            $('#slg_' + id).prop('hidden', true);
            //$('#ghichu_' + id).prop('disabled', false);
            $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
        }
        else {
            $('#valgiaban_' + id).prop('hidden', true);
            $('#giaban_' + id).prop('hidden', false);
            $('#valdonvi_' + id).prop('hidden', true);
            $('#donvi_' + id).prop('hidden', false);
            $('#valslg_' + id).prop('hidden', true);
            $('#slg_' + id).prop('hidden', false);
            //$('#ghichu_' + id).prop('disabled', true);
            $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');
            var giaban = $('#valgiaban_' + id).val();
            var donvi = $('#valdonvi_' + id).val();
            var sl = $('#valslg_' + id).val();
            //var ghichu = $('#ghichu_' + id).val();
            vattu.updateData(id, giaban, donvi,sl)
        }
        },

    DeleteDV: function (id) {
        $.ajax({
            url: '/admin/VatTu/Delete',
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
    LoadLichSu: function (id) {
        $.ajax({
            url: "/admin/VatTu/LoadLichSu",
            data: { id: id },
            dataType: "json",
            type: "POST",
            success: function (res) {
                if (res.status) {
                    var data = res.data;
                    var html = '';
                    var template = $('#data-templateLichSu').html();
                    $.each(data, function (i, item) {
                        html += Mustache.render(template, {
                            MaPN: item.MaPN,
                            TenVatTu: item.TenVatTu,
                            SoLuong: item.SoLuong,
                            GiaMua: item.GiaMua,
                            NCC: item.NCC,
                            NgayNhap: item.NgayNhap,
                            //GhiChu: item.TenKTV,
                        });
                    });
                    $('#table_bodyLichSu').html(html);
                    
                }
                else
                    alert("Không có dữ liệu");
            }
        })
    },
}
vattu.init();