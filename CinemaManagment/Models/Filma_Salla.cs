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
    
    public partial class Filma_Salla
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Filma_Salla()
        {
            this.Rezervimis = new HashSet<Rezervimi>();
        }
    
        public int Id_filmi_salla { get; set; }
        public int Id_Filmi { get; set; }
        public int Id_Salla { get; set; }
        public Nullable<System.DateTime> Orari { get; set; }
    
        public virtual Filmi Filmi { get; set; }
        public virtual Salla Salla { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rezervimi> Rezervimis { get; set; }
    }
}
