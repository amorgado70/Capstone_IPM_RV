using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IPMRVPark.Services
{
    public class SessionService
    {
        IRepositoryBase<session> sessions;

        public const string SessionName = "IPMRVPark";

        public SessionService(IRepositoryBase<session> sessions)
        {
            this.sessions = sessions;
        }

        private session createNewSession(HttpContextBase httpContext)
        {
            //create a new session.

            //first create a new cookie.
            HttpCookie cookie = new HttpCookie(SessionName);
            //now create a new session and set the creation date.
            session _session = new session();
            _session.createDate = DateTime.Now;
            _session.lastUpdate = DateTime.Now;
            //create a sessionID as Guid and convert to a string to be stored in database
            _session.sessionGUID = Guid.NewGuid().ToString();


            //****** to be removed after test
            _session.idIPMEvent = 2;
            _session.isLoggedIn = true;
            _session.idStaff = 1176262;
            _session.idCustomer = 1281633;
            //****** to be removed after test

            //add and persist in the database.
            sessions.Insert(_session);
            sessions.Commit();

            //add the session id to a cookie
            cookie.Value = _session.sessionGUID;
            cookie.Expires = DateTime.Now.AddDays(1);
            httpContext.Response.Cookies.Add(cookie);

            return _session;
        }

        public session GetSession(HttpContextBase httpContext)
        {
            HttpCookie cookie = httpContext.Request.Cookies.Get(SessionName);
            session result;
            string _sessionID;
            Guid _sessionGUID;
            if (cookie != null)//checks if cookie is null
            {
                _sessionID = cookie.Value;
                if (Guid.TryParse(_sessionID, out _sessionGUID))
                {
                    if (_sessionGUID != null)//checks if Guid is null
                    {
                        var sessionList = sessions.GetAll();
                        sessionList = sessionList.Where(s => s.sessionGUID.Contains(_sessionID));
                        if (sessionList.Count() > 0 )//checks if Guid is in database
                        {
                            result = sessionList.First();
                            if (result != null)//session found in database
                                return result;
                        }
                    }
                }
            }
            //Session not found, create new
            result = createNewSession(httpContext);
            return result;
        }
    }
}
