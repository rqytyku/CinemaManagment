using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CinemaManagment.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagment.Models
{
    public class RezervoFilm
    {
        [NotMapped]
        public List<Filma_Salla> filmi_salla { get; set; }
        [NotMapped]
        public List<Filmi> film { get; set; }
        public List<Rezervimi> Rezervim { get; set; }

    }
}