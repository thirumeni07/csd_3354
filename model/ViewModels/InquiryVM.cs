using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSD_3354_Project_Models.ViewModels
{
    public class InquiryVM
    {
        public InquiryHeader InquiryHeader { get; set; }
        public IEnumerable<InquiryDetail> InquiryDetail { get; set; }
    }
}
