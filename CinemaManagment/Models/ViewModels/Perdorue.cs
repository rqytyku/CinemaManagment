using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CinemaManagment.Models
{
    [MetadataType(typeof(PerdoruesMetaData))]
    //public partial class Perdorue
    //{
    //    //[NotMapped]
    //    //public string ConfirmFjalekalimi { get; set; }
    //}

    public class PerdoruesMetaData
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ju lutem vendosni emrin!")]
        [Display(Name = "Emer")]
        public string Emer { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Ju lutem vendosni mbiemrin!")]
        [Display(Name = "Mbiemer")]
        public string Mbiemer { get; set; }
        public Nullable<int> NR_Kontakti { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Ju lutem vendosni emailin!")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Ju lutem vendosni nje fjalekalim!")]
        [MinLength(8, ErrorMessage = "Fjalekalimi duhet te permbaje me shume se 8 karaktere!")]
        [DataType(DataType.Password)]
        public string Fjalekalimi { get; set; }

        [DataType(DataType.Password)]
        [Compare("Fjalekalimi", ErrorMessage = "Fjalekalimi juaj nuk perputhet")]
        public string ConfirmFjalekalimi { get; set; }
    }
}