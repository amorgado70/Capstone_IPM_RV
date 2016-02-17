using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.Controllers
{
    public class CustomersController : Controller
    {
        private DataContext db = new DataContext();

        //
        // GET: /Customers/

        public ActionResult Index()
        {
            return View(db.customer_view.ToList());
        }

        //
        // GET: /Customers/Details/5

        public ActionResult Details(int id = 0)
        {
            customer_view customer_view = db.customer_view.Find(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }

        //
        // GET: /Customers/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Customers/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(customer_view customer_view)
        {
            if (ModelState.IsValid)
            {
                db.customer_view.Add(customer_view);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customer_view);
        }

        //
        // GET: /Customers/Edit/5

        public ActionResult Edit(int id = 0)
        {
            customer_view customer_view = db.customer_view.Find(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }

        //
        // POST: /Customers/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(customer_view customer_view)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer_view).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer_view);
        }

        //
        // GET: /Customers/Delete/5

        public ActionResult Delete(int id = 0)
        {
            customer_view customer_view = db.customer_view.Find(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }

        //
        // POST: /Customers/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            customer_view customer_view = db.customer_view.Find(id);
            db.customer_view.Remove(customer_view);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}