var MayPhucVuController = {

    init: function () {
      MayPhucVuController.xemlichsu();
      MayPhucVuController.enableField();
      MayPhucVuController.registerEvent();
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

        $('.updateMPV').off('keypress').on('keypress', function (e) {
            var id = $(this).data('id');
            var idkh = $(this).data('idkh');
            if (e.which == 13) {
                var tenmay = $('#tenmay_' + id).val();
                var model = $('#model_' + id).val();
                var congsuat = $('#congsuat_' + id).val();
                var vitri = $('#vitri_' + id).val();
                MayPhucVuController.updateData(id, tenmay, model, congsuat, vitri, idkh)
            }
        });
        $('.Opendialog').off('click').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            $('.xacnhan').off('click').on('click', function (f) {
                f.preventDefault();
                MayPhucVuController.DeleteMay(id);
            })
            //$(".modal-body #lydo").val(id);
        });
        $('.btnXemKTV').off('click').on('click', function () {
            var idbbnt = $(this).data('idbbnt');
            MayPhucVuController.LoadKTV(idbbnt);
        });
    },
    updateData: function (id, tenmay, model, congsuat, vitri, idkh) {
        var dataObject = {
            ID: id,
            TenMay: tenmay,
            Model: model,
            ViTri: vitri,
            CongSuat: congsuat,
        };
        var checkconfirm = confirm("Xác nhận lưu?");
        if (checkconfirm == true) {
            $.ajax({
                url: '/admin/MayPhucVu/Update',
                type: 'POST',
                datatype: 'json',
                data: ({ mayphucvu: JSON.stringify(dataObject), idkh:idkh }),
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
    enableField: function () {
        $('.btnEditMayPhucVu').on('click', function () {
            var idkh = $(this).data('idkh');
            var id = $(this).data('idmayphucvu');

            if ($('#icon_' + id).hasClass('fa-pencil-alt')) {
                $('#tenmay_' + id).prop('hidden', false);
                $('#tm_' + id).prop('hidden', true);
                $('#model_' + id).prop('hidden', false);
                $('#md_' + id).prop('hidden', true);
                $('#congsuat_' + id).prop('hidden', false);
                $('#cs_' + id).prop('hidden', true);
                $('#vitri_' + id).prop('hidden', false);
                $('#vt_' + id).prop('hidden', true);
                $('#icon_' + id).toggleClass('fas fa-pencil-alt fas fa-save');
            }
            else {
                $('#tenmay_' + id).prop('hidden', true);
                $('#tm_' + id).prop('hidden', false);
                $('#model_' + id).prop('hidden', true);
                $('#md_' + id).prop('hidden', false);
                $('#congsuat_' + id).prop('hidden', true);
                $('#cs_' + id).prop('hidden', false);
                $('#vitri_' + id).prop('hidden', true);
                $('#vt_' + id).prop('hidden', false);
                $('#icon_' + id).toggleClass('fas fa-save fas fa-pencil-alt');

                var tenmay = $('#tenmay_' + id).val();
                var model = $('#model_' + id).val();
                var congsuat = $('#congsuat_' + id).val();
                var vitri = $('#vitri_' + id).val();
                MayPhucVuController.updateData(id, tenmay, model, congsuat, vitri, idkh)
            }

        });
    },
    xemlichsu: function () {
        $('.btnXemLichSu').off('click').on('click', function () {
            var idkh = $(this).data('idkh');
            var idmay = $(this).data('idmay');
            window.location.href = "/admin/MayPhucVu/LichSuSuaChua?idkh=" + idkh + "&idmay=" + idmay;
        })
    },
    DeleteMay: function (id) {
        $.ajax({
            url: '/admin/MayPhucVu/Delete',
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

}

MayPhucVuController.init();