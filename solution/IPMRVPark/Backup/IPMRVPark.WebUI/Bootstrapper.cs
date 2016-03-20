using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc3;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;

namespace IPMRVPark.WebUI
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<IRepositoryBase<countrycode>, CountryRepository>();
            container.RegisterType<IRepositoryBase<customer>, CustomerRepository>();
            container.RegisterType<IRepositoryBase<customer_view>, CustomerViewRepository>();
            container.RegisterType<IRepositoryBase<reservation_view>, ReservationViewRepository>();

            return container;
        }
    }
}
