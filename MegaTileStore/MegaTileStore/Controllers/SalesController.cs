using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MegaTileStore.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace MegaTileStore.Controllers
{
    [Authorize(Roles = "salesperson")]
    public class SalesController : Controller
    {

        private Entities db = new Entities();

        // GET: Sales
        public ActionResult Index()
        {
            ViewBag.Customers = db.Customers.ToList();
            ViewBag.Products = db.Products.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult GenerateOrder()
        {
            var time = DateTime.Now;
            var productsCount = Convert.ToInt32(Request.Form["product_count"]);
            var totalPrice = Convert.ToDecimal(Request.Form["total_price"]);
            var discount = Convert.ToDecimal(Request.Form["discount"]);
            var deliveryCharges = Convert.ToDecimal(Request.Form["delivery_charges"]);
            var amountPayable = Convert.ToDecimal(Request.Form["amount_payable"]);
            var deliveryAddress = Request.Form["delivery_address"];
            var customerId = Convert.ToInt32(Request.Form["customer_id"]);

            var order = new Order
            {
                AmountPayable = amountPayable,
                ProductCount = productsCount,
                TotalPrice = totalPrice,
                Discount = discount,
                DelieveryCharges = deliveryCharges,
                DelieveryAddress = deliveryAddress,
                CustomerId = customerId,
                Date = time
            };

            db.Orders.Add(order);
            db.SaveChanges();

            var productOrders = new List<ProductOrder>();
            var subOrders = new List<SubOrder>();
            for (int i = 1; i <= productsCount; i++)
            {
                productOrders.Add(new ProductOrder
                {
                    ProductId = Convert.ToInt32(Request.Form[$"product_id_{i}"]),
                    NoOfBoxes = Convert.ToInt32(Request.Form[$"quantity_product_{i}"]),
                    UnitPrice = Convert.ToDecimal(Request.Form[$"unit_price_box_{i}"]),
                    OrderId = order.Id
                });
            }
            productOrders.ForEach(po => db.ProductOrders.Add(po));
            db.SaveChanges();

            var userId = User.Identity.GetUserId();
            var loggedInUserName = db.AspNetUsers.Find(userId).UserName;

            ViewBag.OrderNo = order.Id;
            ViewBag.Customer = db.Customers.Find(customerId);
            ViewBag.ShipmentAddress = deliveryAddress;
            ViewBag.ProductCount = productsCount;
            ViewBag.ProductOrders = productOrders;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.Discount = discount;
            ViewBag.ShipmentCharges = deliveryCharges;
            ViewBag.Payable = amountPayable;
            ViewBag.DateTime = time;
            ViewBag.Username = loggedInUserName;
            ViewBag.Products = db.Products;

            return View();
        }

        public ActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateCustomer(Customer model)
        {
            if(ModelState.IsValid)
            {
                db.Customers.Add(model);
                db.SaveChanges();
                var ledger = new LedgerCustomer
                {
                    Balance = 0M,
                    CustomerId = model.Id
                };
                db.LedgerCustomers.Add(ledger);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return HttpNotFound();
        }

        public ActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateProduct(Product product)
        {
            if(ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                var inventory = new Inventory
                {
                    NoOfBoxes = Convert.ToInt32(Request.Form["noofboxes"]),
                    ProductId = product.Id
                };
                db.Inventories.Add(inventory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Please fill in the fields properly");
                return View(product);
            }
        }




        public JsonResult GetCustomerBalance(int cid)
        {
            var customer = db.Customers.Find(cid);
            if(customer == null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(customer.LedgerCustomers.Where(l => l.CustomerId == cid).First().Balance, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductIds()
        {
            var ids = db.Products.Select(product => product.Id).ToArray();
            return Json(ids, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsProductIdValid(int productId)
        {
            var valid = db.Products.Find(productId) != null;
            return Json(valid, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetProductUnitPrice(int pid)
        //{
        //    var product = db.Products.Find(pid);
        //    if(product == null)
        //    {
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(product.Inventories.Where(i => i.ProductId == pid).First().PricePerBox, JsonRequestBehavior.AllowGet);
        //}
    }
}