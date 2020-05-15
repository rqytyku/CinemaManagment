using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CinemaManagment.Models;
using NLog;

namespace CinemaManagment.Controllers
{
    public class AdminController : Controller
    {
        private KinemaDBEntities db = new KinemaDBEntities();
        Logger logger = LogManager.GetCurrentClassLogger();
        // GET: Admin
        public ActionResult Index()
        {
            try
            {
                if (Session["administrator"] == null)
                {
                    return RedirectToAction("Index", "Kategoris");
                }
                else
                {
                    return View(db.Perdorues.ToList());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Admin controller, Index Action", ex);
            }
            return View();
        }
        [HttpGet]
        public ActionResult Registration()
        {
            if (Session["administrator"] == null)
            {
                return RedirectToAction("Index", "Kategoris");
            }
            else
            {
                PopulateRolesDropDownList();
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] Perdorue P)
        {
            try
            {
                bool Status = false;
                string Message = "";
                if (ModelState.IsValid)
                {
                    #region //is email valid
                    var IsExists = IsEmailVerified(P.Email);
                    if (IsExists)
                    {
                        ModelState.AddModelError("EmailExists", "Ky email ekziston");
                        return View(P);

                    }
                    #endregion
                    #region generate activationcode
                    P.ActivationCode = Guid.NewGuid();
                    #endregion
                    #region password hashing
                    P.Fjalekalimi = Crypto.Hash(P.Fjalekalimi);
                    P.IsEmailVerified = true;
                    P.Statusi = "Activ";
                    #endregion
                    #region save data
                    
                  
                        db.Perdorues.Add(P);
                        db.SaveChanges();

                    
                   
                    //catch (DbEntityValidationException dbEx)
                    //{
                    //    //foreach (var validationErrors in dbEx.EntityValidationErrors)
                    //    //{
                    //    //    foreach (var validationError in validationErrors.ValidationErrors)
                    //    //    {
                    //    //        Trace.TraceInformation("Property: {0} Error: {1}",
                    //    //                                validationError.PropertyName,
                    //    //                                validationError.ErrorMessage);
                    //    //    }
                    //    //}
                    //}

                    #endregion
                    //#region Send email to users
                    //SendVerificationLinkEmail(P.Email, P.ActivationCode.ToString());
                    //Message = "Jeni rregjistruar me sukses. Klikoni ne linkun qe ju kemi drg ne email per te aktivizuar profilin" + P.Email;
                    //Status = true;
                    // #endregion
                }
                else
                {
                    Message = "Invalid request";
                }
                ViewBag.Message = Message;
                ViewBag.Status = Status;
                return View(P);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Admin controller, Registration Action", ex);
            }
            return View(P);
        }
        //Verify account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool status = false;
            db.Configuration.ValidateOnSaveEnabled = false;
            var v = db.Perdorues.Where(x => x.ActivationCode == new Guid(id)).FirstOrDefault();
            if (v != null)
            {
                v.IsEmailVerified = true;
                db.SaveChanges();
                status = true;
            }
            else
            {
                ViewBag.Message = "Invalid request";
            }
            ViewBag.Status = status;
            return View();
        }
        [NonAction]
        public bool IsEmailVerified(String Email)
        {
            var v = db.Perdorues.Where(x => x.Email == Email).FirstOrDefault();
            return v != null;
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Perdorue perdorue = db.Perdorues.Find(id);
            Perdorue perdorues = new Perdorue();
            perdorue.Emer = perdorue.Emer;

            if (perdorue == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_Roli = new SelectList(db.Rolis, "ID_Roli", "Roli1", perdorue.ID_Roli);
            return View(perdorue);
        }
        // POST: Perdorues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "ID_Perdorues,Emer,Mbiemer,NR_Kontakti,Email,ID_Roli,Username")] Perdorue perdorue)
        {
            try
            {
                var perdorues = db.Perdorues.FirstOrDefault(p => p.ID_Perdorues == p.ID_Perdorues);
                if (perdorues != null)
                {
                    perdorues.Emer = perdorue.Emer;
                    perdorues.Mbiemer = perdorue.Mbiemer;
                    perdorues.NR_Kontakti = perdorue.NR_Kontakti;
                    perdorues.Email = perdorue.Email;
                    perdorues.Username = perdorue.Username;
                    perdorues.ID_Roli = perdorue.ID_Roli;
                    perdorues.Fjalekalimi = perdorues.Fjalekalimi;
                    perdorues.ActivationCode = perdorues.ActivationCode;
                    perdorues.IsEmailVerified = perdorues.IsEmailVerified;
                    ViewBag.ID_Roli = new SelectList(db.Rolis, "ID_Roli", "Roli1", perdorue.ID_Roli);
                    db.Entry(perdorues).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(perdorues);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Admin Controller, Edit Action", ex);
                return View(perdorue);
            }
        }
        private void PopulateRolesDropDownList(object selectedRoli = null)
        {
            var RoliQuery = from d in db.Rolis
                            orderby d.ID_Roli
                            select d;
            ViewBag.ID_Roli = new SelectList(RoliQuery, "ID_Roli", "Roli1", selectedRoli);
        }
        public ActionResult Rezervime()
        {
            try
            {
                if (Session["administrator"] == null)
                {
                    return RedirectToAction("Index", "Kategoris");
                }
                else
                {
                    return View(db.Rezervimis.ToList());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Admin Controller, Rezervime Action", ex);
            }

            return View();
        }
        //Details
        public ActionResult Details(int? id)
        {
            try
            {
                if (Session["administrator"] == null)
                {
                    return RedirectToAction("Index", "Kategoris");
                }
                else
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
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Admin View, Details Action", ex);
            }
            return View();
        }

        public ActionResult Disable(int? id)
        {
            if (Session["administrator"] == null)
            {
                return RedirectToAction("Index", "Kategoris");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        db.Perdorues.Where(p => p.ID_Perdorues == id).ToList().ForEach(x => x.ConfirmFjalekalimi = x.Fjalekalimi);
                        db.Perdorues.Where(p => p.ID_Perdorues == id).ToList().ForEach(x => x.Statusi = "Pasiv");
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
                        logger.ErrorException("Error u Gjend ne Admin Kontroller, Disable Action", dbEx);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return Index();
                }
            }
        }
        public ActionResult Enable(int? id)
        {
            if (Session["administrator"] == null)
            {
                return RedirectToAction("Index", "Kategoris");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        db.Perdorues.Where(p => p.ID_Perdorues == id).ToList().ForEach(x => x.ConfirmFjalekalimi = x.Fjalekalimi);
                        db.Perdorues.Where(p => p.ID_Perdorues == id).ToList().ForEach(x => x.Statusi = "Activ");
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
                        logger.ErrorException("Error u Gjend ne Admin Kontroller, Disable Action", dbEx);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return Index();
                }
            }
        }
    }
}