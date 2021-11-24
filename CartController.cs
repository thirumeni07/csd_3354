using Braintree;
using CSD_3354_Project_DataAccess.Data;
using CSD_3354_Project_DataAccess.Repository.IRepository;
using CSD_3354_Project_Models;
using CSD_3354_Project_Models.ViewModels;
using CSD_3354_Project_Utility;
using CSD_3354_Project_Utility.BrainTree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


//Password for all users: Temp1234*
namespace CSD_3354_Project.Controllers
{
    // Authorize property will only allow signed users/admin to access Cartcontroller action methods
    //For example, if you are not signed in then you cannot view your cart
    [Authorize]
    public class CartController : Controller
    {
        //private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;
        private readonly IBrainTreeGate _brain;     // For making payment

        //Binding property: Once you bind this property(ProductUserVM) in the post
        //you do not have to explicitly define this in the action method as parameter 
        //It will be by default available in summary post
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }
        //public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        //public CartController(IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        //{
        //    //_db = db;
        //    _webHostEnvironment = webHostEnvironment;
        //    _emailSender = emailSender;
        //}

        //dependency injection
        public CartController(IWebHostEnvironment webHostEnvironment, IEmailSender emailSender,
            IApplicationUserRepository userRepo, IProductRepository prodRepo,
            IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo,
            IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRepo, IBrainTreeGate brain)
        {
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _brain = brain;
            _userRepo = userRepo;
            _prodRepo = prodRepo;
            _inqDRepo = inqDRepo;
            _inqHRepo = inqHRepo;
            _orderDRepo = orderDRepo;
            _orderHRepo = orderHRepo;
        }
        public IActionResult Index()
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            { 
                //session exists
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            //IEnumerable<Product> prodList = _db.Product.Where(u => prodInCart.Contains(u.Id));
            //IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.Quantity = cartObj.Quantity;   // because we are storing Quantity in Session not in database 
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        //public IActionResult IndexPost()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, Quantity = prod.Quantity });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {

            //For making payment 
            var gateway = _brain.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;


            //Below two lines will get the userid of the logged in user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //var userId = User.FindFirstValue(ClaimTypes.Name); // another way to get the logged in user id

            ////////////////////////////////////////////////////////////////////////////////////
            //ApplicationUser applicationUser;
            //if (User.IsInRole(WC.AdminRole))
            //{
            //    if (HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
            //    {
            //        //cart has been loaded using an inquiry
            //        InquiryHeader inquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
            //        applicationUser = new ApplicationUser()
            //        {
            //            Email = inquiryHeader.Email,
            //            FullName = inquiryHeader.FullName,
            //            PhoneNumber = inquiryHeader.PhoneNumber
            //        };
            //    }
            //    else
            //    {
            //        applicationUser = new ApplicationUser();
            //    }
            //}
            //else
            //{
            //    var claimsIdentity = (ClaimsIdentity)User.Identity;
            //    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //    //var userId = User.FindFirstValue(ClaimTypes.Name);

            //    applicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value);
            //}

            ////////////////////////////////////////////////////////////////////////////////////

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            //IEnumerable<Product> prodList = _db.Product.Where(u => prodInCart.Contains(u.Id));
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                //Get all the details of logged in user from the database
                //ApplicationUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value),
                ApplicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value),
                //ApplicationUser = applicationUser,
                //ProductList = prodList.ToList()      // show the products present in cart on summary page
            };

            //Set the quantity value in Product object with Session quantity value.
            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.Quantity = cartObj.Quantity;
                ProductUserVM.ProductList.Add(prodTemp);
            }


            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        //public IActionResult SummaryPost(ProductUserVM ProductUserVM)   // Parameters not required as bind property is defined above
        //public IActionResult SummaryPost()
        public async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserVM ProductUserVM)   // IFormCollection is for using form data from summary.cshtml 
        {
            //Whenever you work with a credit card transaction, you authorize that transaction and then settle that transaction.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ////////////////////////////////////////////////////////////////////////////////////////

            //if (User.IsInRole(WC.AdminRole))
            //{
                //we need to create an order
                //var orderTotal = 0.0;
                //foreach (Product prod in ProductUserVM.ProductList)
                //{
                //    orderTotal += prod.Price * prod.Quantity;
                //}
                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = ProductUserVM.ProductList.Sum(x => x.Quantity * x.Price), // Calculate sum of price of all the products using linq instead of commented out foreach loop above
                    City = ProductUserVM.ApplicationUser.City,
                    StreetAddress = ProductUserVM.ApplicationUser.StreetAddress,
                    State = ProductUserVM.ApplicationUser.State,
                    PostalCode = ProductUserVM.ApplicationUser.PostalCode,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = WC.StatusPending
                };
                _orderHRepo.Add(orderHeader);
                _orderHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerQty = prod.Price,
                        Quantity = prod.Quantity,
                        ProductId = prod.Id
                    };
                    _orderDRepo.Add(orderDetail);

                }
                _orderDRepo.Save();

            string nonceFromTheClient = collection["payment_method_nonce"];

            var request = new TransactionRequest
            {
                Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
                PaymentMethodNonce = nonceFromTheClient,
                OrderId = orderHeader.Id.ToString(),
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            var gateway = _brain.GetGateway();
            Result<Transaction> result = gateway.Transaction.Sale(request);

            if (result.Target.ProcessorResponseText == "Approved")
            {
                orderHeader.TransactionId = result.Target.Id;
                orderHeader.OrderStatus = WC.StatusApproved;
            }
            else
            {
                orderHeader.OrderStatus = WC.StatusCancelled;
            }
            _orderHRepo.Save();

            return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });
            //}
            //else
            //{
            //    ////////////////////////////////////////////////////////////////////////////////////////
            //    //we need to create an inquiry
            //    var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
            //        + "templates" + Path.DirectorySeparatorChar.ToString() +
            //        "Inquiry.html";

            //    //in order to send an email, we need dependency injection to get an access to email sender class
            //    // this block is used to send an email
            //    var subject = "New Inquiry";
            //    string HtmlBody = "";
            //    using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
            //    {
            //        HtmlBody = sr.ReadToEnd();
            //    }
            //    //Name: { 0}
            //    //Email: { 1}
            //    //Phone: { 2}
            //    //Products: {3}

            //    StringBuilder productListSB = new StringBuilder();
            //    foreach (var prod in ProductUserVM.ProductList)
            //    {
            //        productListSB.Append($" - Name: { prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
            //    }

            //    string messageBody = string.Format(HtmlBody,
            //        ProductUserVM.ApplicationUser.FullName,
            //        ProductUserVM.ApplicationUser.Email,
            //        ProductUserVM.ApplicationUser.PhoneNumber,
            //        productListSB.ToString());
            //    await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

            //    InquiryHeader inquiryHeader = new InquiryHeader()
            //    {
            //        ApplicationUserId = claim.Value,
            //        FullName = ProductUserVM.ApplicationUser.FullName,
            //        Email = ProductUserVM.ApplicationUser.Email,
            //        PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
            //        InquiryDate = DateTime.Now

            //    };

            //    _inqHRepo.Add(inquiryHeader);
            //    _inqHRepo.Save();

            //    foreach (var prod in ProductUserVM.ProductList)
            //    {
            //        InquiryDetail inquiryDetail = new InquiryDetail()
            //        {
            //            InquiryHeaderId = inquiryHeader.Id,
            //            ProductId = prod.Id
            //        };
            //        _inqDRepo.Add(inquiryDetail);

            //    }
            //    _inqDRepo.Save();
            //    TempData[WC.Success] = "Inquiry completed successfully";
            //}            
            //return RedirectToAction(nameof(InquiryConfirmation));
        }
        //public IActionResult InquiryConfirmation()
        public IActionResult InquiryConfirmation(int id = 0)
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();    // we want to clear out the cart data and other session data once order is placed or sent for inquiry confirmation
            //return View();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, Quantity = prod.Quantity });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
