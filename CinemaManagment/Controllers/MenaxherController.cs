using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CinemaManagment.Models;
using NLog;

namespace CinemaManagment.Controllers
{
    public class MenaxherController : Controller
    {
        private KinemaDBEntities db = new KinemaDBEntities();
        Logger logger = LogManager.GetCurrentClassLogger();

        //Listimi i sallave
        public ActionResult Index()
        {
            if (Session["menaxher"] != null)
            {
                return View(db.Sallas.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Perdorues");
            }
        }
        public ActionResult Rezervime()
        {
            if (Session["menaxher"] == null)
            {
                return RedirectToAction("Login", "Perdorues");
            }
            else
            {
                return View(db.Rezervimis.Where(x => x.Refuzuar != true).ToList());
            }
        }


        // GET: Perdorues1/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Perdorue perdorue = db.Perdorues.Find(id);
            if (perdorue == null)
            {
                return HttpNotFound();
            }
            return View(perdorue);
        }

        // GET: Perdorues1/Create
        public ActionResult Create()
        {
            if (Session["menaxher"] == null)
            {
                return RedirectToAction("Login", "Perdorues");
            }
            else
            {
                return View();
            }
        }

        // POST: Perdorues1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Salla,Statusi,Nr_Rreshtave,Nr_Kolonave")] Salla salla)
        {
            try
            {
                salla.Statusi = "Available";
                if (ModelState.IsValid)
                {
                    db.Sallas.Add(salla);
                    db.SaveChanges();
                    TempData["rezsuccess"] = "<script>alert('U shtua me sukses!');</script>";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex) { logger.ErrorException("Error i gjendur ne kontroller   Manager dhe ne Create Action", ex); }

            return View(salla);
        }

        // GET: Perdorues1/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Perdorue perdorue = db.Perdorues.Find(id);
            if (perdorue == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_Roli = new SelectList(db.Rolis, "ID_Roli", "Roli1", perdorue.ID_Roli);
            return View(perdorue);
        }
        //Action qe te bejme DISABLE nje salle
        public ActionResult Disable(int? id)
        {
            if (Session["menaxher"] == null)
            {
                return RedirectToAction("Login", "Perdorues");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var IsExists = IfRezervime(id);
                        if (IsExists)
                        {
                            //alert
                            TempData["msg"] = "<script>alert('Ju nuk mund te beni disable sepse keni rezervime ne kete salle');</script>";
                        }
                        else
                        {
                            db.Sallas.Where(p => p.Id_Salla == id).ToList().ForEach(x => x.Statusi = "Disabled");
                            db.SaveChanges();
                        }
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}",
                                                        validationError.PropertyName,
                                                        validationError.ErrorMessage);
                            }
                        }

                        logger.ErrorException("Error u gjend ne Manager controller, Disable Action", dbEx);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return Index();
                }
            }
        }
        //Action per te bere Available nje salle 
        public ActionResult Enable(int? id)
        {
            if (Session["menaxher"] == null)
            {
                return RedirectToAction("Login", "Perdorues");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        db.Sallas.Where(p => p.Id_Salla == id).ToList().ForEach(x => x.Statusi = "Available");
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}",
                                                        validationError.PropertyName,
                                                        validationError.ErrorMessage);
                            }
                        }
                        logger.ErrorException("Error u gjend ne Manager Controller,Enable Action", dbEx);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return Index();
                }
            }
        }
        //Shtimi i nje filmi ne nje salle te caktuar ne nje ore te caktuar
        public ActionResult ShtoEvent(int? id)
        {
            if (Session["menaxher"] == null)
            {
                return RedirectToAction("Login", "Perdorues");
            }
            else
            {

                ViewBag.Id_Filmi = new SelectList(db.Filmis, "Id_Filmi", "Titulli");
                ViewBag.Id_Salla = new SelectList(db.Sallas, "Id_Salla", "Id_Salla");
                return View();
            }
        }
        // POST: Filma_Salla/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShtoEvent(int id, [Bind(Include = "Id_Filmi,Id_Salla,Orari")] Filma_Salla filma_Salla)
        {
            try
            {
                filma_Salla.Id_Salla = id;
                var IsExists = Iforari(filma_Salla.Id_Salla, Convert.ToDateTime(filma_Salla.Orari));
                if (IsExists)
                {
                    TempData["msg2"] = "<script>alert('Salla eshte e zene ne kete orar');</script>";


                }
                else if (ModelState.IsValid)
                {
                    db.Filma_Salla.Add(filma_Salla);
                    db.SaveChanges();
                    TempData["rezsuccess"] = "<script>alert('U shtua me sukses!');</script>";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Manager Controller, Shto Event Action ", ex);
            }

            ViewBag.Id_Filmi = new SelectList(db.Filmis, "Id_Filmi", "Titulli", filma_Salla.Id_Filmi);
            ViewBag.Id_Salla = new SelectList(db.Sallas, "Id_Salla", "Id_Salla", filma_Salla.Id_Salla);
            return View(filma_Salla);
        }
        [NonAction]
        public bool Iforari(int id_salla, DateTime orari)
        {
            var v = db.Filma_Salla.Where(x => x.Id_Salla == id_salla && x.Orari == orari).FirstOrDefault();
            return v != null;
        }
        //Refuzimi i nje rezervimi
        public ActionResult Refuzo(int? id, int? id_filmi_salla)
        {
            if (Session["menaxher"] == null)
            {
                return RedirectToAction("Login", "Perdorues");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        db.Rezervimis.Where(p => p.Id_Rezervim == id).ToList().ForEach(x => x.Refuzuar = true);
                        db.SaveChanges();
                        db.Filma_Salla.Where(p => p.Id_filmi_salla == id_filmi_salla).ToList().ForEach(x => x.Salla.Statusi = "Available");
                        db.SaveChanges();
                        TempData["anullo"] = "<script>alert('Ju keni Refuzuar kete Rezervim!');</script>";
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}",
                                                        validationError.PropertyName,
                                                        validationError.ErrorMessage);
                            }
                        }
                        logger.ErrorException("Error u gjend ne Manager Controller, Refuzo Action ", dbEx);
                    }
                    return RedirectToAction("Rezervime", "Menaxher");
                }
                else
                {
                    return RedirectToAction("Rezervime", "Menaxher");
                }
            }
        }
        //Metode qe kontrollon nese ka rezervime aktive ne nje salle
        [NonAction]
        public bool IfRezervime(int? id)
        {
            var v = db.Rezervimis.Where(x => x.Id_filmi_salla == id && x.Anulluar != true && x.Refuzuar != true).FirstOrDefault();
            return v != null;
        }
        public ActionResult RezervimeRefuzuara()
        {
            if (Session["menaxher"] == null)
            {
                return RedirectToAction("Login", "Perdorues");
            }
            else
            {
                return View(db.Rezervimis.Where(x => x.Refuzuar == true).ToList());
            }
        }
    }
}