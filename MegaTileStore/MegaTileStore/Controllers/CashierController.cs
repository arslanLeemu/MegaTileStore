using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MegaTileStore.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace MegaTileStore.Controllers
{
    [Authorize(Roles = "cashier")]
    public class CashierController : Controller
    {
        private Entities db = new Entities();
        
        // GET: Cashier
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GenerateBill(int order_no)
        {
            var order = db.Orders.Find(order_no);
            if(order == null)
            {
                return HttpNotFound();
            }
            ViewBag.Order = order;
            ViewBag.Ledger = db.LedgerCustomers.Where(ledger => ledger.CustomerId == order.CustomerId).First();
            return View();
        }

        [HttpPost]
        public ActionResult GenerateBill()
        {
            var orderId = Convert.ToInt32(Request.Form["order_id"]);
            var amountPaid = Convert.ToDecimal(Request.Form["amount_paid"]);
            var balance = Convert.ToDecimal(Request.Form["bill_balance"]);
            var payable = Convert.ToDecimal(Request.Form["amount_payable"]);

            var bill = new Bill
            {
                OrderId = orderId,
                AmountPayed = amountPaid,
                Balance = balance
            };

            db.Bills.Add(bill);
            db.SaveChanges();

            var order = db.Orders.Find(orderId);

            var ledger = db.LedgerCustomers.Where(l => l.CustomerId == order.CustomerId).First();
            ledger.Balance = ledger.Balance + balance;
            db.Entry(ledger).State = EntityState.Modified;
            db.SaveChanges();

            var productOrders = db.ProductOrders.Where(po => po.OrderId == orderId).ToList();
            var modifiedInventories = new List<Inventory>();
            foreach (var po in productOrders)
            {
                var inventoryToModify = db.Inventories.Where(inventory => inventory.ProductId == po.ProductId)
                    .First();
                inventoryToModify.NoOfBoxes -= po.NoOfBoxes;
                modifiedInventories.Add(inventoryToModify);
            }
            modifiedInventories.ForEach(inventory => db.Entry(inventory).State = EntityState.Modified);
            db.SaveChanges();

            var user = User.Identity.GetUserId();
            var userName = db.AspNetUsers.Find(user).UserName;

            ViewBag.BillNo = bill.Id;
            ViewBag.Customer = order.Customer;
            ViewBag.OrderNo = orderId;
            ViewBag.Payable = payable;
            ViewBag.Paid = amountPaid;
            ViewBag.LedgerBalance = ledger.Balance;
            ViewBag.Username = userName;
            

            return View("ViewBill");
        }
    }
}