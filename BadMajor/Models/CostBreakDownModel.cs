using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BadMajor
{
    public class CostBreakDownModel
    {
        public string InStateTuition { get; set; }
        public string OutStateTuition { get; set; }
        public string FeesAndOtherExp { get; set; }
        public string RoomAndBoard { get; set; }
        public string Books { get; set; }
    }
}