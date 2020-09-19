using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
    public class LichSuMay
    {
        int id, idbbnt, iddichvu;
        string tenmay, model, congsuat, vitri, tendichvu,tenkhach;
        DateTime ngaythuchien;

        
        public LichSuMay(int id,int idbbnt, string tenmay, string tenkhach, string model, string congsuat, string vitri, int iddichvu, string tendichvu, DateTime ngaythuchien)
        {
            this.Id = id;
            this.Idbbnt = idbbnt;
            this.Tenmay = tenmay;
            this.Model = model;
            this.Tenkhach = tenkhach;
            this.Congsuat = congsuat;
            this.Vitri = vitri;
            this.Iddichvu = iddichvu;
            this.Tendichvu = tendichvu;
            this.Ngaythuchien = ngaythuchien;
        }

        public int Id { get => id; set => id = value; }
        public int Idbbnt { get => idbbnt; set => idbbnt = value; }
        public string Tenmay { get => tenmay; set => tenmay = value; }
        public string Tenkhach { get => tenkhach; set => tenkhach = value; }
        public string Model { get => model; set => model = value; }
        public string Congsuat { get => congsuat; set => congsuat = value; }
        public string Vitri { get => vitri; set => vitri = value; }
        public int Iddichvu { get => iddichvu; set => iddichvu = value; }
        public string Tendichvu { get => tendichvu; set => tendichvu = value; }
        public DateTime Ngaythuchien { get => ngaythuchien; set => ngaythuchien = value; }
    }
}