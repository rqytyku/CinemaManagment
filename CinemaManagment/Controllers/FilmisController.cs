using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CinemaManagment.Models;
using NLog;

namespace CinemaManagment.Controllers
{
    public class FilmisController : Controller
    {
        private KinemaDBEntities db = new KinemaDBEntities();
        Logger logger = LogManager.GetCurrentClassLogger();

        // GET: Filmis
        public ActionResult Index()
        {
            var filmis = db.Filmis.Include(f => f.Kategori);
            return View(filmis.ToList());
        }

        // GET: Filmis/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                List<Filma_Salla> Fs = new List<Filma_Salla>();
                Fs = db.Filma_Salla.Where(x => x.Id_Filmi == id && x.Salla.Statusi == "Available").ToList();

                Filmi F = db.Filmis.Where(x => x.Id_Filmi == id).FirstOrDefault();
                MovieDetails MV = new MovieDetails();
                MV.filmi = F;
                MV.filmi_salla = Fs;

                if (MV == null)
                {
                    return HttpNotFound();
                }
                return View(MV);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Controller Filmi, Details Action", ex);
            }
            return View();
        }

        // GET: Filmis/Create
        public ActionResult Add()
        {
            if (Session["menaxher"] != null)
            {
                ViewBag.kategoria = new SelectList(db.Kategoris, "id_kategori", "Emri");
                return View();
            }
            else
            {

                return RedirectToAction("Index", "Kategoris");
            }
        }

        // POST: Filmis/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Filmi F)
        {
            try
            {
                string filename = Path.GetFileNameWithoutExtension(F.imagefile.FileName);
                string extenction = Path.GetExtension(F.imagefile.FileName);
                filename = filename + DateTime.Now.ToString("yymmssfff") + extenction;
                F.Foto = "~/fonts/Img/" + filename;
                F.imagefile.SaveAs(Server.MapPath("~/fonts/Img/" + filename));
                filename = Path.Combine(Server.MapPath("~/fonts/Img/") + filename);
                F.Statusi = "Activ";
                ViewBag.Kategoria = new SelectList(db.Kategoris, "id_kategori", "Emri", F.Kategoria);
                db.Filmis.Add(F);
                db.SaveChanges();
                TempData["rezsuccess"] = "<script>alert('U shtua me sukses!');</script>";

                ModelState.Clear();
                return View();
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Controller Filmi, Add Action", ex);
            }

            return View();
        }
        public ActionResult Rezervo(int? id)
        {
            if (Session["perdorues"] == null)
            {
                TempData["login"] = "<script>alert('Ju duhet te logoheni per te bere nje rezervim!');</script>";
                return RedirectToAction("LogIn", "Perdorues");
            }
            else
            {
                Rezervimi Rezervofilm = new Rezervimi();
                Rezervofilm.filmi_salla = db.Filma_Salla.Where(x => x.Id_Filmi == id).ToList<Filma_Salla>();
                Rezervofilm.film = db.Filmis.Where(x => x.Id_Filmi == id).FirstOrDefault();
                return View(Rezervofilm);
            }
        }
        // POST: Rezervimis/Create
        [HttpPost]
        public ActionResult Rezervo(Rezervimi rezervimi, int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DateTime? orari = rezervimi.orari;
                var id_salla = db.Filma_Salla.Where(x => x.Id_Filmi == id && x.Orari == orari).Select(x => x.Id_Salla).FirstOrDefault();
                var id_filma_salla = db.Filma_Salla.Where(x => x.Id_Filmi == id && x.Id_Salla == id_salla).Select(x => x.Id_filmi_salla).FirstOrDefault();
                rezervimi.ID_Perdorues = Convert.ToInt32(Session["perdorues"]);
                rezervimi.Id_filmi_salla = id_filma_salla;

                if (nr_vende(id_filma_salla) == nr_rezervime(id_filma_salla))
                {
                    TempData["msg1"] = "<script>alert('Full, nuk ka me vende te lira');</script>";

                    db.Filma_Salla.Where(p => p.Id_filmi_salla == id_filma_salla).ToList().ForEach(x => x.Salla.Statusi = "Fully booked");
                    db.SaveChanges();
                    return RedirectToAction("Rezervo", "Filmis", new { id = id });
                }
                else
                {
                    var v = db.Rezervimis.Where(x => x.Id_filmi_salla == id_filma_salla).Count();
                    rezervimi.vendi = v + 1;

                    if (ModelState.IsValid)
                    {
                        db.Rezervimis.Add(rezervimi);
                        db.SaveChanges();
                        TempData["rezsuccess"] = "<script>alert('U shtua me sukses!');</script>";
                    }

                    return RedirectToAction("Rezervimet", "Perdorues", new { id = Session["perdorues"] });
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Filmi Controller, rezervo Action", ex);
                return RedirectToAction("Rezervimet", "Perdorues", new { id = Session["perdorues"] });
            }
        }
        [HttpGet]
        public ActionResult ShfaqFilma(int id)
        {
            List<Filmi> Fs = new List<Filmi>();
            Fs = db.Filmis.Where(x => x.Kategoria == id && x.Statusi == "Activ").ToList();
            return View(Fs);
        }
        [NonAction]
        public int nr_rezervime(int id)
        {
            var v = db.Rezervimis.Where(x => x.Id_filmi_salla == id && x.Anulluar != true && x.Refuzuar != true).Count();
            return v;
        }
        [NonAction]
        public int nr_vende(int id)
        {

            var r = db.Filma_Salla.Where(x => x.Id_filmi_salla == id).Select(p => p.Salla.Nr_Kolonave * p.Salla.Nr_Rreshtave).FirstOrDefault();
            return Convert.ToInt32(r);
        }
        [NonAction]
        public bool ifvendeanulluar(int id)
        {
            var v = db.Rezervimis.Where(x => x.Id_filmi_salla == id && x.Anulluar == true || x.Refuzuar == true).Count();
            return v != null;
        }
    }
}