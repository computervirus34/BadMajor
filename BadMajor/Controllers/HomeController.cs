using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace BadMajor.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            try
            {
                ReportViewModel spv = new ReportViewModel();
                SetInitialListValues(spv);
                return View(spv);
            }
            catch (Exception ex)
            {
                TempData["retMsg"] = ex.Message;
                return RedirectToAction("ErrorPage");
            }

        }


        public ActionResult IndexMobile()
        {
            try
            {
                ReportViewModel spv = new ReportViewModel();
                SetInitialListValues(spv);
                return View(spv);
            }
            catch (Exception ex)
            {
                TempData["retMsg"] = ex.Message;
                return RedirectToAction("ErrorPage");
            }

        }

        public ActionResult FAQ()
        {
            return View();

        }

        public ActionResult Contact()
        {
            return View();

        }

        [HttpPost]
        public ActionResult Contact(ContactFormModel model)
        {
            //Read SMTP section from Web.Config.
            SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            SendEmail(smtpSection, model);
            return View();
        }

        public void SendEmail(SmtpSection smtpSection, ContactFormModel contact)
        {
            try
            {

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSection.From),
                    Subject = contact.Subject,
                    Body = contact.Body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(smtpSection.From);

                var smtpClient = new SmtpClient(smtpSection.Network.Host)
                {
                    Port = 587,
                    Credentials = new NetworkCredential(smtpSection.From, smtpSection.Network.Password),
                    EnableSsl = true,
                };
                smtpClient.Send(mailMessage);
                ViewBag.Message = "Email sent sucessfully.";
            }
            catch(Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            
        }
        public ActionResult ErrorPage()
        {
            return View();
        }

        public void SetInitialListValues(ReportViewModel spv)
        {
            string query = @"SELECT credential_id value, creddesc description 
                                FROM dim_credential 
                                where creddesc != 'Highschool Diploma' 
                                order by credential_id";

            spv.creddescList = GetDropDownListValue(query, "dim_credential");

            query = @"SELECT state_cd value,state description 
                        FROM dim_state ORDER BY state";

            spv.stateList = GetDropDownListValue(query, "dim_state");

            //spv.majorGroupingList = GetDropDownListValue(query, "dim_major_grouping");
        }
        public IEnumerable<SelectListItem> GetDropDownListValue(string query, string dtName)
        {
            MySqlConnection con = DatabaseConnection.getDBConnection();

            if (con.State != ConnectionState.Open)
                con.Open();

            MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlDataAdapter returnVal = new MySqlDataAdapter(query, con);
            DataTable dt = new DataTable(dtName);
            returnVal.Fill(dt);
            con.Close();

            var data = dt.AsEnumerable().Select(row =>
                        new
                        {
                            value = row["value"].ToString(),
                            description = row["description"].ToString()
                        });

            return new SelectList(data.ToList(), "value", "description");
        }
        public JsonResult getInstnm(List<string> states, List<int> credential_id)
        {
            string inStates = string.Join(",", states.Select(i => $"'{i}'"));
            string credential_ids = String.Join(", ", credential_id.Select(n => n.ToString(CultureInfo.InvariantCulture)));
            string query = @"SELECT distinct i.institution_id value, INSTNM description 
                                FROM dim_institution i
                                INNER JOIN fct_major f ON f.institution_id = i.institution_id";


            query = $"{query} where STABBR in ({inStates}) and f.credential_id IN ({credential_ids}) ORDER BY INSTNM";
            List<SelectListItem> liState = GetDropDownListValue(query, "dim_institution").ToList();
            return Json(new SelectList(liState, "value", "text", JsonRequestBehavior.AllowGet));
        }

        public JsonResult GetMajorGroup(List<string> insts)
        {
            string instIds = string.Join(",", insts.Select(i => $"'{i}'"));

            string query = @"SELECT distinct mg.major_grouping_id value, major_grouping_desc description 
                                FROM dim_major_grouping mg
                                JOIN dim_major m ON m.major_grouping_id = mg.major_grouping_id
                                JOIN fct_major fm ON fm.major_id = m.major_id
                                JOIN dim_institution ins ON ins.institution_id = fm.institution_id";

            query = $"{query} WHERE fm.institution_id IN({instIds}) ORDER BY major_grouping_desc; ";
            List<SelectListItem> liIns = GetDropDownListValue(query, "dim_major_group").ToList();
            return Json(new SelectList(liIns, "value", "text", JsonRequestBehavior.AllowGet));
        }

        public JsonResult getMajor(List<int> credential_id, List<int> instId, List<int> major_grouping_id)
        {
            string credential_ids = String.Join(", ", credential_id.Select(n => n.ToString(CultureInfo.InvariantCulture)));
            string instIds = String.Join(", ", instId.Select(n => n.ToString(CultureInfo.InvariantCulture)));
            string major_grouping_ids = String.Join(", ", major_grouping_id.Select(n => n.ToString(CultureInfo.InvariantCulture)));

            string query = @"SELECT distinct cipcode value, major  description
                            FROM dim_major m  
                            INNER JOIN  fct_major fm ON fm.major_id = m.major_id
                            INNER JOIN dim_credential cre ON cre.credential_id = fm.credential_id
                            INNER JOIN dim_institution ins ON ins.institution_id = fm.institution_id
                            INNER JOIN  dim_major_grouping mg ON m.major_grouping_id = mg.major_grouping_id";
            query = $"{query} WHERE ins.institution_id in ({instIds}) AND cre.credential_id in ({credential_ids})";
            query = $"{query} AND mg.major_grouping_id in ({major_grouping_ids}) order by major";

            List<SelectListItem> liMajor = GetDropDownListValue(query, "dim_major").ToList();


            return Json(new SelectList(liMajor, "value", "text", JsonRequestBehavior.AllowGet));
        }

        [HttpPost]
        public JsonResult EarningDetails(int pageIndex, List<int> credential_id, List<int> instId, List<int> major_grouping_id, List<string> cipCode)
        {
            string credential_ids = String.Join(", ", credential_id.Select(n => n.ToString(CultureInfo.InvariantCulture)));
            string instIds = String.Join(", ", instId.Select(n => n.ToString(CultureInfo.InvariantCulture)));
            string major_grouping_ids = String.Join(", ", major_grouping_id.Select(n => n.ToString(CultureInfo.InvariantCulture)));
            string cipCodes = string.Join(",", cipCode.Select(i => $"{i}"));

            EarningDetailsModel earningDetails = new EarningDetailsModel();
            earningDetails.PageIndex = pageIndex;
            earningDetails.PageSize = 10;
            string query = GetBaseQuery();
            query = $"{query} WHERE a.credential_id in ({credential_ids}) AND a.institution_id in ({instIds}) ";
            query = $"{query} AND d.cipcode in({cipCodes}) AND dd.major_grouping_id in({major_grouping_ids})";
            query = $"{query} order by a.earnings desc";

            earningDetails.Reports = GetEarningsList(query, "result");
            earningDetails.RecordCount = earningDetails.Reports.Count();
            int startIndex = (pageIndex - 1) * earningDetails.PageSize;

            earningDetails.Reports = earningDetails.Reports.Skip(startIndex)
                .Take(earningDetails.PageSize).ToList(); ;

            return Json(earningDetails, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetCostChangeResult(string credential_id, string instId, string major_grouping_id, string cipCode, string cost)
        {
            ReportViewModel Report = new ReportViewModel();
            string query = GetBaseQuery();
            query = query.Replace(" cost ", cost);
            query = $"{query} WHERE a.credential_id ={credential_id} AND a.institution_id = {instId} ";
            query = $"{query} AND d.cipcode ={cipCode} AND dd.major_grouping_id ={major_grouping_id}";
            query = $"{query} order by a.earnings desc";
            Report = GetEarningsList(query, "result").SingleOrDefault();

            return Json(Report, JsonRequestBehavior.AllowGet);
        }
        private string GetBaseQuery()
        {
            return @"select 
                            b.creddesc,
                            b.credential_id degree,
                            c.institution_id instCode,
                            dd.major_grouping_id major_group,
                            d.cipcode majorcode,
                            e.state,
                            c.instnm,
                            dd.major_grouping_desc,
                            d.major,
                            cost 'cost',
                            a.earnings,
                            case
	                            when (a.`fv` - lower_bracket_fv - cost ) < 0 then 'This major at this institution has a negative Return on Investment'
	                            else cast(round(a.`fv` - lower_bracket_fv - cost , 0) as char(255))
                            end roi,
                            case
	                            when earnings - lower_bracket_earnings<=0 then 'You are not likely to recoup the cost.'
	                            when earnings - lower_bracket_earnings>0 then CONCAT('You should recoup the Cost of your degree in ' , cast(round( cost /(earnings - lower_bracket_earnings), 0)  as char(255)) , ' years')
                            end  years_to_recoup_cost,
                            concat(case-- Use ROI calc
	                            when a.fv - lower_bracket_fv - cost >750000 then 'A'
	                            when a.fv - lower_bracket_fv - cost >=250000 and a.fv - lower_bracket_fv - cost < 750000 THEN 'B'
	                            when a.fv - lower_bracket_fv - cost >=100000 and a.fv - lower_bracket_fv - cost < 250000 THEN 'C'
	                            when a.fv - lower_bracket_fv - cost >=50000 and  a.fv - lower_bracket_fv - cost < 100000 THEN 'D'
	                            when a.fv - lower_bracket_fv - cost < 50000 then 'F'
                            end 
                            ,
                            case  --  SQLINES DEMO ***  same as one degree higher, then plus. One degree lower, then minus
	                            when a.fv - lower_bracket_fv - cost < 50000  then ''  --  SQLINES DEMO ***  F+ or F-
	                            when b.fv <a.fv then '+'
	                            when b.fv > a.fv then '-'
                                else ''
                            end) grade,
                            CONCAT('You will earn about the same as others with a(n) ' ,x.creddesc) equivalent
                            from fct_major a
                            left outer join dim_credential b on a.credential_id = b.credential_id
                            left outer join dim_institution c on a.institution_id = c.institution_id
                            left outer join dim_major d on a.major_id = d.major_id
                            left outer join dim_major_grouping dd on d.major_grouping_id = dd.major_grouping_id
                            left outer join dim_state e on a.state_id = e.state_id
                            left outer join dim_credential x on a.earnings>=x.min and a.earnings<=x.max";
        }


        public List<ReportViewModel> GetEarningsList(string query, string dtName)
        {
            MySqlConnection con = DatabaseConnection.getDBConnection();

            if (con.State != ConnectionState.Open)
                con.Open();

            MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlDataAdapter returnVal = new MySqlDataAdapter(query, con);
            DataTable dt = new DataTable(dtName);
            returnVal.Fill(dt);
            con.Close();

            var data = dt.AsEnumerable().Select(row =>
                        new ReportViewModel
                        {
                            creddesc = row["creddesc"].ToString(),
                            state = row["state"].ToString(),
                            instnm = row["instnm"].ToString(),
                            major_grouping_desc = row["major_grouping_desc"].ToString(),
                            major = row["major"].ToString(),
                            cost = row["cost"].ToString(),
                            earnings = row["earnings"].ToString(),
                            roi = row["roi"].ToString(),
                            years_to_recoup_cost = row["years_to_recoup_cost"].ToString(),
                            grade = row["grade"].ToString(),
                            equivalent = row["equivalent"].ToString(),
                            degree = row["degree"].ToString(),
                            instCode = row["instCode"].ToString(),
                            major_group = row["major_group"].ToString(),
                            majorcode = row["majorcode"].ToString()
                        });

            return data.ToList();
        }

        public CostBreakDownModel GetInstCostBreakDown(string query, string dtName)
        {
            MySqlConnection con = DatabaseConnection.getDBConnection();

            if (con.State != ConnectionState.Open)
                con.Open();

            MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlDataAdapter returnVal = new MySqlDataAdapter(query, con);
            DataTable dt = new DataTable(dtName);
            returnVal.Fill(dt);
            con.Close();

            var data = dt.AsEnumerable().Select(row =>
                        new CostBreakDownModel
                        {
                            InStateTuition = row["InStateTuition"].ToString(),
                            OutStateTuition = row["OutStateTuition"].ToString(),
                            FeesAndOtherExp = row["FeesAndOtherExp"].ToString(),
                            RoomAndBoard = row["RoomAndBoard"].ToString(),
                            Books = row["Books"].ToString()
                        });

            return data.SingleOrDefault();
        }


        [HttpPost]
        public JsonResult GetCostBreakDown(string instCode)
        {
            CostBreakDownModel Report = new CostBreakDownModel();
            string query = @"select
                            coalesce(TUITIONFEE_IN,TUITIONFEE_PROG) `InStateTuition`,  --  SQLINES DEMO *** ithout decimals
                            coalesce(TUITIONFEE_out,TUITIONFEE_PROG) `OutStateTuition`,
                            coalesce(OTHEREXPENSE_ON,OTHEREXPENSE_OFF,OTHEREXPENSE_FAM) `FeesAndOtherExp`,
                            coalesce(ROOMBOARD_ON,ROOMBOARD_OFF) `RoomAndBoard`,
                            BOOKSUPPLY `Books`
                            from dim_institution";

            query = $"{query} WHERE institution_id ={instCode}";
            Report = GetInstCostBreakDown(query, "result");

            return Json(Report, JsonRequestBehavior.AllowGet);
        }
    }
}