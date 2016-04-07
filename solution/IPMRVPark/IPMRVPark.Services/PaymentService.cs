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
    public class PaymentService
    {
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<payment> payments;
        IRepositoryBase<paymentreservationitem> paymentsreservationitems;

        public PaymentService(
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<reservationitem> reservationitems,
            IRepositoryBase<payment> payments,
            IRepositoryBase<paymentreservationitem> paymentsreservationitems
            )
        {
            this.selecteditems = selecteditems;
            this.reservationitems = reservationitems;
            this.payments = payments;
            this.paymentsreservationitems = paymentsreservationitems;
        }
        #region Common
        const long IDnotFound = -1;

        // Clean selected items
        const int cleanAll = 1;
        const int cleanNew = 2;
        const int cleanEdit = 3;

        public void CleanAllSelectedItems(long sessionID)
        {
            CleanSelectedItemList(sessionID, cleanAll);
        }
        public void CleanNewSelectedItems(long sessionID)
        {
            CleanSelectedItemList(sessionID, cleanNew);
        }
        public void CleanEditSelectedItems(long sessionID)
        {
            CleanSelectedItemList(sessionID, cleanEdit);
        }

        private void CleanSelectedItemList(long sessionID, int cleanCode)
        {
            // Clean edit items that are in selected table
            var _olditems_to_be_removed = selecteditems.GetAll().
                Where(c => c.idSession == sessionID);
            bool tryResult = false;
            try
            {
                var _oldselecteditem = _olditems_to_be_removed.FirstOrDefault();
                tryResult = !(_oldselecteditem.Equals(default(session)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
            if (tryResult)// Items found in database, remove them
            {
                foreach (var _olditem in _olditems_to_be_removed)
                {
                    if (cleanCode == cleanAll ||
                        (cleanCode == cleanNew && _olditem.reservationAmount == 0) ||
                        (cleanCode == cleanEdit && _olditem.reservationAmount != 0))
                    {
                        selecteditems.Delete(_olditem.ID);
                    }
                }
                selecteditems.Commit();
            }
        }
        #endregion
        #region Payments & Refunds
        public decimal GetProvinceTax(long sessionID)
        {
            return 13; // HST value for Ontario
        }
        public decimal GetCancelationFee(long sessionID)
        {
            return 50; // Value for 2016
        }

        // Sum and Count for Selected Items
        public decimal CalculateNewSelectedTotal(long sessionID, out int count)
        {
            var _selecteditem = selecteditems.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID);

            count = 0;
            decimal sum = 0;
            if (_selecteditem != null)
            {
                foreach (var i in _selecteditem)
                {
                    count = count + 1;
                    sum = sum + i.total;
                }
            }

            return sum;
        }

        // Sum and Count for Reserved Items
        public decimal CalculateReservedTotal(long customerID)
        {
            var _reserveditems = reservationitems.GetAll().
                Where(q => q.idCustomer == customerID).OrderByDescending(o => o.ID);

            int count = 0;
            decimal sum = 0;
            foreach (var i in _reserveditems)
            {
                count = count + 1;
                sum = sum + i.total;
            }

            return sum;
        }

        public payment CalculateEditSelectedTotal(long sessionID, long customerID)
        {
            payment _payment = new payment();

            var _selecteditems = selecteditems.GetAll();
            _selecteditems = _selecteditems.Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID);
            int count = 0;
            decimal selectionTotal = 0; // Thhis selection or edit reservation total
            decimal reservationTotal = 0; // Previous reservation total
            foreach (var _selecteditem in _selecteditems)
            {
                count = count + 1;
                selectionTotal = selectionTotal + _selecteditem.total;
                // Check if selected item was a reserved item,
                // this means the selected item is in edit reservation mode
                if (_selecteditem.idReservationItem != null && _selecteditem.idReservationItem != IDnotFound)
                {
                    var _reservationitem = reservationitems.GetById(_selecteditem.idReservationItem);
                    reservationTotal = reservationTotal + _reservationitem.total;
                }
            }

            // *****
            decimal dueAmount = Math.Max((selectionTotal - reservationTotal), 0);
            decimal refundAmount = Math.Max((reservationTotal - selectionTotal), 0);
            // Check if a cancellation fee applies
            decimal cancelationFee = GetCancelationFee(sessionID);
            if (selectionTotal < reservationTotal)
            {
                if ((reservationTotal - selectionTotal) < cancelationFee)
                {
                    refundAmount = 0;
                    dueAmount = cancelationFee - (reservationTotal - selectionTotal);
                }
                else
                {
                    refundAmount = refundAmount - cancelationFee;
                }
            }
            else
            {
                cancelationFee = 0;
            }

            // Value of previous reservation, just before edit reservation mode started
            _payment.primaryTotal = reservationTotal;
            _payment.selectionTotal = selectionTotal;
            _payment.cancellationFee = cancelationFee;
            /// Suggested value for payment
            _payment.amount = dueAmount - refundAmount - CustomerAccountBalance(customerID);
            _payment.tax = Math.Round((dueAmount * GetProvinceTax(sessionID) / 100), 2, MidpointRounding.AwayFromZero);
            _payment.withoutTax = dueAmount - _payment.tax;

            return _payment;
        }

        public decimal CustomerAccountBalance(long customerID)
        {
            if (customerID == IDnotFound)
            {
                return 0;
            };

            var _payments = payments.GetAll().
                Where(p => p.idCustomer == customerID).OrderBy(p => p.ID);

            var _last = _payments.ToList().LastOrDefault();
            decimal finalBalance = (_last != null) ? _last.balance : 0;

            return finalBalance;
        }

        public decimal CustomerPreviousBalance(long customerID, long paymentID)
        {
            if (customerID == IDnotFound)
            {
                return 0;
            };

            var _payments = payments.GetAll().
                Where(p => p.idCustomer == customerID && p.ID < paymentID ).OrderByDescending(p => p.ID);
            var _p = _payments.ToList();

            if (_p.Count() < 1)
            {
                return 0;
            }
            else
            {
                return _p.First().balance;
            };              
        }



        #endregion
    }
}
