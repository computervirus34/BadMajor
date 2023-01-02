using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BadMajor
{
    public class ReportViewModel
    {
        [Display(Name= "DEGREE TYPE")]
        public string creddesc { get; set; }
        [Display(Name = "STATE")]
        public string state { get; set; }
        [Display(Name = "COLLEGE")]
        public string instnm { get; set; }
        [Display(Name = "MAJOR GROUPING")]
        public string major_grouping_desc { get; set; }
        [Display(Name = "MAJOR")]
        public string major { get; set; }
        public IEnumerable<SelectListItem> creddescList { get; set; }
        public IEnumerable<SelectListItem> stateList { get; set; }
        public IEnumerable<SelectListItem> instnmList { get; set; }
        public IEnumerable<SelectListItem> majorGroupingList { get; set; }
        public IEnumerable<SelectListItem> majorList { get; set; }
        public string cost { get; set; }
        public string earnings { get; set; }
        public string roi { get; set; }
        public string years_to_recoup_cost { get; set; }
        public string grade { get; set; }
        public string equivalent { get; set; }

        public string degree { get; set; }
        public string instCode { get; set; }
        public string major_group { get; set; }
        public string majorcode { get; set; }
    }


    public class EarningDetailsModel
    {
        public EarningDetailsModel()
        {
            Reports = new List<ReportViewModel>();
        }
        public List<ReportViewModel> Reports { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int RecordCount { get; set; }
    }

}