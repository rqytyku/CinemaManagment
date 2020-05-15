using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using CinemaManagment.Models;
namespace CinemaManagment.Models
{
    public class UserLogin
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ju lutem vendosni emailin!")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Ju lutem vendosni nje fjalekalim!")]
        [DataType(DataType.Password)]
        public string Fjalekalimi { get; set; }

        [Display(Name = "RememberME")]
        public bool RememberMe { get; set; }
    }
}