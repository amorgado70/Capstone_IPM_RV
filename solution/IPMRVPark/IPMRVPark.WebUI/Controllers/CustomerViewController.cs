using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;

namespace IPMRVPark.WebUI.Controllers
{
    public class CustomerViewController : Controller
    {
        IRepositoryBase<customer_view> customers_view;

        public CustomerViewController(IRepositoryBase<customer_view> customers_view)
        {
            this.customers_view = customers_view;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var customer_view = customers_view.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                customer_view = customer_view.Where(s => s.fullName.Contains(searchString));
            }

            return View(customer_view);
        }

        // GET: /Details/5
        public ActionResult Details(int? id)
        {
            var customer_view = customers_view.GetById(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }

        // GET: /Create
        public ActionResult Create()
        {
            var customer_view = new customer_view();
            return View(customer_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(customer_view customer_view)
        {
            customers_view.Insert(customer_view);
            customers_view.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult Edit(int id)
        {
            customer_view customer_view = customers_view.GetById(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(customer_view customer_view)
        {
            customers_view.Update(customer_view);
            customers_view.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult Delete(int id)
        {
            customer_view customer_view = customers_view.GetById(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            customers_view.Delete(customers_view.GetById(id));
            customers_view.Commit();
            return RedirectToAction("Index");
        }
    }
}
