using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CinemaManagment.Models;

namespace CinemaManagment.Models
{
    public class MovieDetails
    {
     

        public Filmi filmi { get; set; }
        public List<Filma_Salla> filmi_salla { get; set; }

    }
}