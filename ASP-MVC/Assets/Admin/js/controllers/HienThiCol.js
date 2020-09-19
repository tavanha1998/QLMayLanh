class HienThi{
    constructor(checkbox) {
        this.checkbox = checkbox;
    }
    Check() {
        var rs = document.getElementById(this.checkbox);
        return rs.checked;
    }
}