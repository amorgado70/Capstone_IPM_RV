using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;

namespace IPMRVPark.Controllers
{
    public class ReservationViewController : Controller
    {
        IRepositoryBase<reservation_view> reservations_view;

        public ReservationViewController(IRepositoryBase<reservation_view> reservations_view)
        {
            this.reservations_view = reservations_view;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var reservation_view = reservations_view.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                reservation_view = reservation_view.Where(s => s.fullName.Contains(searchString));
            }

            return View(reservation_view);
        }

        // GET: /Details/5
        public ActionResult Details(int? id)
        {
            var reservation_view = reservations_view.GetById(id);
            if (reservation_view == null)
            {
                return HttpNotFound();
            }
            return View(reservation_view);
        }

        // GET: /Create
        public ActionResult Create()
        {
            var reservation_view = new reservation_view();
            return View(reservation_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(reservation_view reservation_view)
        {
            reservations_view.Insert(reservation_view);
            reservations_view.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult Edit(int id)
        {
            reservation_view reservation_view = reservations_view.GetById(id);
            if (reservation_view == null)
            {
                return HttpNotFound();
            }
            return View(reservation_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(reservation_view reservation_view)
        {
            reservations_view.Update(reservation_view);
            reservations_view.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult Delete(int id)
        {
            reservation_view reservation_view = reservations_view.GetById(id);
            if (reservation_view == null)
            {
                return HttpNotFound();
            }
            return View(reservation_view);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            reservations_view.Delete(reservations_view.GetById(id));
            reservations_view.Commit();
            return RedirectToAction("Index");
        }
    }
}
