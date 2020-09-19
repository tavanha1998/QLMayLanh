var change = {
    init: function () {
    },
    registerEvent: function () {
        $('.xacnhan').off('click').on('click', function (e) {
            e.preventDefault();
            var old = $('.col-md-3 #oldPass').val();
            var newpass = $('.col-md-3 #NewPass1').val();
            var temp = $('.col-md-3 #NewPass2').val();
            if (e.which == 13) {
                var tenktv = $('#tenktv_' + id).val();
                var sdt = $('#sdt_' + id).val();
                var loai = $('#load_' + id).val();
                var status = $('#status_' + id).val();
                UserController.updateData(id, tenktv, sdt, loai, status);
            }
        });
    },
    CheckPass: function (pass) {

    },
}
change.init()