using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BadMajor
{
    public class ContactFormModel
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        [AllowHtml]
        public string Body { get; set; }
        //public HttpPostedFileBase Attachment { get; set; }
    }
}