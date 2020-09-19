﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP_MVC.EF
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class QLMayLanhEntities : DbContext
    {
        public QLMayLanhEntities()
            : base("name=QLMayLanhEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BienBanNghiemThu> BienBanNghiemThus { get; set; }
        public virtual DbSet<ChiPhiKhac> ChiPhiKhacs { get; set; }
        public virtual DbSet<CTBBNT_MayLanh> CTBBNT_MayLanh { get; set; }
        public virtual DbSet<CTBienBanNghiemThu> CTBienBanNghiemThus { get; set; }
        public virtual DbSet<CTPhieuNhapVatTu> CTPhieuNhapVatTus { get; set; }
        public virtual DbSet<CTPhieuXuatKho> CTPhieuXuatKhoes { get; set; }
        public virtual DbSet<CTPhieuXuatVatTu_Khach> CTPhieuXuatVatTu_Khach { get; set; }
        public virtual DbSet<CTPhieuXuatVatTu_KTV> CTPhieuXuatVatTu_KTV { get; set; }
        public virtual DbSet<DichVu_SanPham> DichVu_SanPham { get; set; }
        public virtual DbSet<KhachHang> KhachHangs { get; set; }
        public virtual DbSet<KhoVatDung> KhoVatDungs { get; set; }
        public virtual DbSet<KTV_BBNT> KTV_BBNT { get; set; }
        public virtual DbSet<MayLanh> MayLanhs { get; set; }
        public virtual DbSet<NhanVien> NhanViens { get; set; }
        public virtual DbSet<PhieuNhapVatTu> PhieuNhapVatTus { get; set; }
        public virtual DbSet<PhieuXuatKho> PhieuXuatKhoes { get; set; }
        public virtual DbSet<PhieuXuatVatTu_Khach> PhieuXuatVatTu_Khach { get; set; }
        public virtual DbSet<PhieuXuatVatTu_KTV> PhieuXuatVatTu_KTV { get; set; }
        public virtual DbSet<VatTu> VatTus { get; set; }
        public virtual DbSet<YeuCauPhucVu> YeuCauPhucVus { get; set; }
    
        public virtual ObjectResult<Nullable<bool>> Acc_Login(string username, string password)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("username", username) :
                new ObjectParameter("username", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("password", password) :
                new ObjectParameter("password", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("Acc_Login", usernameParameter, passwordParameter);
        }
    }
}