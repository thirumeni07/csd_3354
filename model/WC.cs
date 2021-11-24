using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CSD_3354_Project_Utility
{
    public static class WC
    {
        public const string ImagePath = @"\images\product\";
        public const string SessionCart = "ShoppingCartSession";
        public const string SessionInquiryId = "InquirySession";
        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";
        public const string EmailAdmin = "mac071092 @protonmail.com";
        public const string CategoryName = "Category";
        public const string ApplicationTypeName = "ApplicationType";
        public const string Success = "Success";    // for success notification
        public const string Error = "Error";    // for error notification
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static readonly IEnumerable<string> listStatus = new ReadOnlyCollection<string>(
            new List<string>
            {
                StatusApproved,StatusCancelled,StatusInProcess,StatusPending,StatusRefunded,StatusShipped
            });
    }
}
