using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;

namespace IPMRVPark.Controllers
{
    public class CustomersController : Controller
    {

        IRepositoryBase<customer> customers;

        public CustomersController(IRepositoryBase<customer> customers)
        {
            this.customers = customers;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var customer = customers.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                customer = customer.Where(s => s.person.firstName.Contains(searchString) ||
                s.person.lastName.Contains(searchString) );
            }

            return View(customer);
        }

        // GET: /Details/5
        public ActionResult Details(int? id)
        {
            var customer = customers.GetById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: /Create
        public ActionResult Create()
        {
            var customer = new customer();
            return View(customer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(customer customer)
        {
            customers.Insert(customer);
            customers.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult Edit(int id)
        {
            customer customer = customers.GetById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(customer customer)
        {
            customers.Update(customer);
            customers.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult Delete(int id)
        {
            customer customer = customers.GetById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            customers.Delete(customers.GetById(id));
            customers.Commit();
            return RedirectToAction("Index");
        }
    }
}