//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CinemaManagment.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Rezervimi
    {
        internal List<Filma_Salla> filmi_salla;

        public int Id_Rezervim { get; set; }
        public Nullable<int> Id_filmi_salla { get; set; }
        public Nullable<int> ID_Perdorues { get; set; }
        public int vendi { get; set; }
        public Filmi film { get; set; }
        public Nullable<System.DateTime> orari { get; set; }
        public string statusi { get; set; }
        public Nullable<bool> Anulluar { get; set; }
        public Nullable<bool> Refuzuar { get; set; }
    
        public virtual Filma_Salla Filma_Salla { get; set; }
        public virtual Perdorue Perdorue { get; set; }
    }
}
