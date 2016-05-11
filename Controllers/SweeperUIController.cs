using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.SqlTypes;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Net.Mime;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.SessionState;
using SweeperUI.Models;
using Sweeper_DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using UMS_Util;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace SweeperUI.Controllers
{
    [Authorize]
    public class SweeperUIController : Controller
    {
        public SweeperUIController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public SweeperUIController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        public ActionResult Index()
        {
            return View("Diagnoses");
        }

        public ActionResult Diagnoses()
        {
            return View("Diagnoses");
        }

        public ActionResult Procedures()
        {
            return View("Procedures");
        }

        public ActionResult Providers()
        {
            return View("Providers");
        }

        public ActionResult Claimants()
        {
            return View("Claimants");
        }

        public ActionResult Claims()
        {
            return View("Claims");
        }

        public ClaimManager ClaimsManager
        {
            get
            {
                return new ClaimManager(UserManager.GetClaims(User.Identity.GetUserId()));
            }
        }

        public async Task<ActionResult> GetUserClaims()
        {
            var claims = await UserManager.GetClaimsAsync(User.Identity.GetUserId());

            var manager = new ClaimManager(claims);

            return Json(new {
                claims = claims.Select(c => new { c.Type, c.Value, c.ValueType }),
                master = manager.getBool(UMSClaims.CLAIM_MASTER),
                dev = manager.getBool(UMSClaims.CLAIM_DEV),
                admin = manager.getBool(UMSClaims.CLAIM_USHC_ADMIN),
                SweeperUI = manager.getBool(UMSClaims.CLAIM_ACCESS_DMS)
            }, "GetClaimsResult",
                JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetClaimCostsResults(string startDate, string endDate, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cm = (from clm in con.MC_Claim
                      join pat in con.MC_Patient on clm.ClaimID equals pat.ClaimID
                      join svc in con.MC_Service on clm.ClaimID equals svc.ClaimID
                      join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                      from cptOuter in oj.DefaultIfEmpty()
                      where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                      orderby svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName ascending
                      select new { svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, svc.ChargeAmount, cptOuter.CPTCode, cptOuter.Description });

            var result = cm.ToList().Select(l => new
            {
                StatementDate = l.Date == null ? "" : ((DateTime)l.Date).ToString("yyyy-MM-dd"),
                ClaimNumber = l.ClaimNumber,
                FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                DateOfBirth = l.DateOfBirth == null ? "" : l.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                TotalClaim = l.ChargeAmount.ToString("C"),
                TotalClaimRaw = l.ChargeAmount,
                CPTCode = l.CPTCode,
                CPTDescription = l.Description
            });

            return Json(result, "Claims", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDiagnosesICDResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundICD))
            {
                lowerBoundICD = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundICD))
            {
                upperBoundICD = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cmClaims = (from d in con.MC_Diagnosis
                            join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                            join svc in con.MC_Service on d.ClaimID equals svc.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(d.Code, lowerBoundICD) != -1 && string.Compare(d.Code, upperBoundICD) != 1
                            group new { d, svc } by new { d.Code, icd9.DescriptionLong, icd9.DescriptionShort } into g
                            select new
                            {
                                ICDCode = g.Key.Code,
                                DescriptionLong = g.Key.DescriptionLong,
                                DescriptionShort = g.Key.DescriptionShort,
                                ClaimCount = g.Count(),
                                ClaimTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ClaimMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ClaimAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmEvents = (from d in con.MC_Diagnosis
                            join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                            join svc in con.MC_Service on d.ClaimID equals svc.ClaimID
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(d.Code, lowerBoundICD) != -1 && string.Compare(d.Code, upperBoundICD) != 1
                            group new { d, clm } by new { d.Code } into g
                            select new
                            {
                                ICDCode = g.Key.Code,
                                EventCount = g.Select(x => x.clm.ClaimID).Distinct().Count()
                            });

            var cmClaimants = (from d in con.MC_Diagnosis
                              join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on clm.ClaimID equals pat.ClaimID
                              group new { d, pat } by new { d.Code } into g
                              select new {
                                  ICDCode = g.Key.Code,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from d in con.MC_Diagnosis
                              join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on clm.ClaimID equals sub.ClaimID
                              group new { d, sub } by new { d.Code } into g
                              select new
                              {
                                  ICDCode = g.Key.Code,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from d in con.MC_Diagnosis
                               join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on d.ClaimID equals prv.ClaimID
                               group new { d, prv } by new { d.Code } into g
                               select new
                               {
                                   ICDCode = g.Key.Code,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join evt in cmEvents on clm.ICDCode equals evt.ICDCode
                      join pat in cmClaimants on clm.ICDCode equals pat.ICDCode
                      join sub in cmEmployees on clm.ICDCode equals sub.ICDCode
                      join prv in cmProviders on clm.ICDCode equals prv.ICDCode
                      orderby clm.ICDCode
                      select new
                      {
                          clm.ICDCode,
                          clm.DescriptionLong,
                          clm.DescriptionShort,
                          ICDDescription = clm.ICDCode,
                          clm.ClaimTotalRaw,
                          clm.ClaimMaximumRaw,
                          clm.ClaimAverageRaw,
                          clm.ClaimCount,
                          evt.EventCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ICDCode,
                l.DescriptionLong,
                l.DescriptionShort,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw,
                l.ClaimCount,
                l.EventCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return Json(result, "Diagnoses by ICD", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDiagnosesICDClaimsResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundICD))
            {
                lowerBoundICD = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundICD))
            {
                upperBoundICD = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cm = (from d in con.MC_Diagnosis
                      join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                      join svc in con.MC_Service on d.ClaimID equals svc.ClaimID
                      join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                      join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                      join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                      from cptOuter in oj.DefaultIfEmpty()
                      where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                            && string.Compare(d.Code, lowerBoundICD) != -1 && string.Compare(d.Code, upperBoundICD) != 1
                      orderby svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName ascending
                      select new { svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, svc.ChargeAmount, cptOuter.CPTCode, cptOuter.Description });

            var result = cm.ToList().Select(l => new
            {
                StatementDate = l.Date == null ? "" : ((DateTime)l.Date).ToString("yyyy-MM-dd"),
                ClaimNumber = l.ClaimNumber,
                FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                DateOfBirth = l.DateOfBirth == null ? "" : l.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                TotalClaim = l.ChargeAmount.ToString("C"),
                TotalClaimRaw = l.ChargeAmount,
                CPTCode = l.CPTCode,
                CPTDescription = l.Description
            });

            return Json(result, "Diagnoses by ICD Claims", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDiagnosesGroupResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundICD))
            {
                lowerBoundICD = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundICD))
            {
                upperBoundICD = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cmClaims = (from d in con.MC_Diagnosis
                            join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                            join svc in con.MC_Service on d.ClaimID equals svc.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(d.Code, lowerBoundICD) != -1 && string.Compare(d.Code, upperBoundICD) != 1
                            group new { dg, svc } by new { dg.GroupId, dg.GroupName } into g
                            select new
                            {
                                g.Key.GroupId,
                                g.Key.GroupName,
                                ClaimCount = g.Count(),
                                ClaimTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ClaimMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ClaimAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmEvents = (from d in con.MC_Diagnosis
                            join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                            join svc in con.MC_Service on d.ClaimID equals svc.ClaimID
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(d.Code, lowerBoundICD) != -1 && string.Compare(d.Code, upperBoundICD) != 1
                            group new { d, clm } by new { dg.GroupId } into g
                            select new
                            {
                                g.Key.GroupId,
                                EventCount = g.Select(x => x.clm.ClaimID).Distinct().Count()
                            });

            var cmClaimants = (from d in con.MC_Diagnosis
                              join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on d.ClaimID equals pat.ClaimID
                              group new { dg, pat } by new { dg.GroupId } into g
                              select new
                              {
                                  g.Key.GroupId,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from d in con.MC_Diagnosis
                              join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on d.ClaimID equals sub.ClaimID
                              group new { dg, sub } by new { dg.GroupId } into g
                              select new
                              {
                                  g.Key.GroupId,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from d in con.MC_Diagnosis
                               join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on d.ClaimID equals prv.ClaimID
                               group new { dg, prv } by new { dg.GroupId } into g
                               select new
                               {
                                   g.Key.GroupId,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join evt in cmEvents on clm.GroupId equals evt.GroupId
                      join pat in cmClaimants on clm.GroupId equals pat.GroupId
                      join sub in cmEmployees on clm.GroupId equals sub.GroupId
                      join prv in cmProviders on clm.GroupId equals prv.GroupId
                      join dg in con.MC_DiagnosticGroups on clm.GroupId equals dg.GroupId
                      orderby clm.GroupName
                      select new
                      {
                          DiagnosticGroup = dg.GroupFromCode + "-" + dg.GroupToCode,
                          clm.GroupName,
                          clm.ClaimTotalRaw,
                          clm.ClaimMaximumRaw,
                          clm.ClaimAverageRaw,
                          clm.ClaimCount,
                          evt.EventCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.DiagnosticGroup,
                l.GroupName,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw,
                l.ClaimCount,
                l.EventCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });
            return Json(result, "Diagnoses by Group", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDiagnosesSubgroupResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundICD))
            {
                lowerBoundICD = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundICD))
            {
                upperBoundICD = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cmClaims = (from d in con.MC_Diagnosis
                            join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                            join svc in con.MC_Service on d.ClaimID equals svc.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(d.Code, lowerBoundICD) != -1 && string.Compare(d.Code, upperBoundICD) != 1
                            group new { dsg, svc } by new { dsg.SubgroupId, dsg.SubgroupName } into g
                            select new
                            {
                                g.Key.SubgroupId,
                                g.Key.SubgroupName,
                                ClaimCount = g.Count(),
                                ClaimTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ClaimMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ClaimAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmEvents = (from d in con.MC_Diagnosis
                            join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                            join svc in con.MC_Service on d.ClaimID equals svc.ClaimID
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(d.Code, lowerBoundICD) != -1 && string.Compare(d.Code, upperBoundICD) != 1
                            group new { d, clm } by new { dsg.SubgroupId } into g
                            select new
                            {
                                g.Key.SubgroupId,
                                EventCount = g.Select(x => x.clm.ClaimID).Distinct().Count()
                            });

            var cmClaimants = (from d in con.MC_Diagnosis
                              join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on d.ClaimID equals pat.ClaimID
                              group new { dsg, pat } by new { dsg.SubgroupId, dsg.SubgroupName } into g
                              select new
                              {
                                  g.Key.SubgroupId,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from d in con.MC_Diagnosis
                              join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on d.ClaimID equals sub.ClaimID
                              group new { dsg, sub } by new { dsg.SubgroupId, dsg.SubgroupName } into g
                              select new
                              {
                                  g.Key.SubgroupId,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from d in con.MC_Diagnosis
                               join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on d.ClaimID equals prv.ClaimID
                               group new { dsg, prv } by new { dsg.SubgroupId, dsg.SubgroupName } into g
                               select new
                               {
                                   g.Key.SubgroupId,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join evt in cmEvents on clm.SubgroupId equals evt.SubgroupId
                      join pat in cmClaimants on clm.SubgroupId equals pat.SubgroupId
                      join sub in cmEmployees on clm.SubgroupId equals sub.SubgroupId
                      join prv in cmProviders on clm.SubgroupId equals prv.SubgroupId
                      join dsg in con.MC_DiagnosticSubgroups on clm.SubgroupId equals dsg.SubgroupId
                      orderby clm.SubgroupName
                      select new
                      {
                          DiagnosticSubgroup = dsg.SubgroupFromCode + "-" + dsg.SubgroupToCode,
                          clm.SubgroupName,
                          clm.ClaimTotalRaw,
                          clm.ClaimMaximumRaw,
                          clm.ClaimAverageRaw,
                          clm.ClaimCount,
                          evt.EventCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.DiagnosticSubgroup,
                l.SubgroupName,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw,
                l.ClaimCount,
                l.EventCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return Json(result, "Diagnoses by Subgroup", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProvidersResults(string startDate, string endDate, string providerNPI, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            if (string.IsNullOrWhiteSpace(providerNPI))
            {
                var cmClaims = (from prv in con.MC_Provider
                                join svc in con.MC_Service on prv.ClaimID equals svc.ClaimID
                                where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                group new { prv, svc } by new { prv.NameLastName, prv.NameFirstName, prv.NameMiddleName, prv.Npi } into g
                                select new
                                {
                                    g.Key.NameLastName,
                                    g.Key.NameFirstName,
                                    g.Key.NameMiddleName,
                                    g.Key.Npi,
                                    ClaimCount = g.Count(),
                                    ClaimTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                    ClaimMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                    ClaimAverageRaw = g.Average(i => i.svc.ChargeAmount)
                                });

                var cmEvents = (from prv in con.MC_Provider
                                join svc in con.MC_Service on prv.ClaimID equals svc.ClaimID
                                join clm in con.MC_Claim on prv.ClaimID equals clm.ClaimID
                                where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                group new { prv, clm } by new { prv.NameLastName, prv.NameFirstName, prv.NameMiddleName, prv.Npi } into g
                                select new
                                {
                                    g.Key.NameLastName,
                                    g.Key.NameFirstName,
                                    g.Key.NameMiddleName,
                                    g.Key.Npi,
                                    EventCount = g.Select(i => i.clm.ClaimID).Distinct().Count()
                                });

                var cm = (from clm in cmClaims
                          join evt in cmEvents on clm.Npi equals evt.Npi
                          select new
                          {
                              clm.NameLastName,
                              clm.NameFirstName,
                              clm.NameMiddleName,
                              clm.Npi,
                              evt.EventCount,
                              clm.ClaimCount,
                              clm.ClaimTotalRaw,
                              clm.ClaimMaximumRaw,
                              clm.ClaimAverageRaw
                          });

                var result = cm.ToList().Select(l => new
                {
                    FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                    l.Npi,
                    l.EventCount,
                    l.ClaimCount,
                    ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                    l.ClaimTotalRaw,
                    ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                    l.ClaimMaximumRaw,
                    ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                    l.ClaimAverageRaw
                });

                return Json(result, "Providers", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var cmClaims = (from prv in con.MC_Provider
                                join svc in con.MC_Service on prv.ClaimID equals svc.ClaimID
                                where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                    && prv.Npi == providerNPI
                                group new { prv, svc } by new { prv.NameLastName, prv.NameFirstName, prv.NameMiddleName, prv.Npi } into g
                                select new
                                {
                                    g.Key.NameLastName,
                                    g.Key.NameFirstName,
                                    g.Key.NameMiddleName,
                                    g.Key.Npi,
                                    ClaimCount = g.Count(),
                                    ClaimTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                    ClaimMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                    ClaimAverageRaw = g.Average(i => i.svc.ChargeAmount)
                                });

                var cmEvents = (from prv in con.MC_Provider
                                join svc in con.MC_Service on prv.ClaimID equals svc.ClaimID
                                join clm in con.MC_Claim on prv.ClaimID equals clm.ClaimID
                                where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                    && prv.Npi == providerNPI
                                group new { prv, clm } by new { prv.NameLastName, prv.NameFirstName, prv.NameMiddleName, prv.Npi } into g
                                select new
                                {
                                    g.Key.NameLastName,
                                    g.Key.NameFirstName,
                                    g.Key.NameMiddleName,
                                    g.Key.Npi,
                                    EventCount = g.Select(i => i.clm.ClaimID).Distinct().Count()
                                });

                var cm = (from clm in cmClaims
                          join evt in cmEvents on clm.Npi equals evt.Npi
                          select new
                          {
                              clm.NameLastName,
                              clm.NameFirstName,
                              clm.NameMiddleName,
                              clm.Npi,
                              evt.EventCount,
                              clm.ClaimCount,
                              clm.ClaimTotalRaw,
                              clm.ClaimMaximumRaw,
                              clm.ClaimAverageRaw
                          });

                var result = cm.ToList().Select(l => new
                {
                    FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                    l.Npi,
                    l.EventCount,
                    l.ClaimCount,
                    ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                    l.ClaimTotalRaw,
                    ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                    l.ClaimMaximumRaw,
                    ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                    l.ClaimAverageRaw
                });

                return Json(result, "Providers", JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetProvidersClaimsResults(string startDate, string endDate, string providerNPI, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            if (string.IsNullOrWhiteSpace(providerNPI))
            {
                var cm = (from prv in con.MC_Provider
                          join svc in con.MC_Service on prv.ClaimID equals svc.ClaimID
                          join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                          from cptOuter in oj.DefaultIfEmpty()
                          join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                          join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                          where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                          orderby svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName ascending
                          select new { svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, svc.ChargeAmount, cptOuter.CPTCode, cptOuter.Description });
                var result = cm.ToList().Select(l => new
                {
                    StatementDate = l.Date == null ? "" : ((DateTime)l.Date).ToString("yyyy-MM-dd"),
                    ClaimNumber = l.ClaimNumber,
                    FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                    DateOfBirth = l.DateOfBirth == null ? "" : l.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                    TotalClaim = l.ChargeAmount.ToString("C"),
                    TotalClaimRaw = l.ChargeAmount,
                    CPTCode = l.CPTCode,
                    CPTDescription = l.Description
                });

                return Json(result, "Providers Claims", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var cm = (from prv in con.MC_Provider
                          join svc in con.MC_Service on prv.ClaimID equals svc.ClaimID
                          join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                          from cptOuter in oj.DefaultIfEmpty()
                          join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                          join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                          where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                              && prv.Npi == providerNPI
                          orderby svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName ascending
                          select new { svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, svc.ChargeAmount, cptOuter.CPTCode, cptOuter.Description });

                var result = cm.ToList().Select(l => new
                {
                    StatementDate = l.Date == null ? "" : ((DateTime)l.Date).ToString("yyyy-MM-dd"),
                    ClaimNumber = l.ClaimNumber,
                    FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                    DateOfBirth = l.DateOfBirth == null ? "" : l.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                    TotalClaim = l.ChargeAmount.ToString("C"),
                    TotalClaimRaw = l.ChargeAmount,
                    CPTCode = l.CPTCode,
                    CPTDescription = l.Description
                });

                return Json(result, "Providers Claims", JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetClaimantsResults(string startDate, string endDate, string claimantID, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            if (string.IsNullOrWhiteSpace(claimantID))
            {
                var cmClaims = (from pat in con.MC_Patient
                          join svc in con.MC_Service on pat.ClaimID equals svc.ClaimID
                          where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                          group new { pat, svc } by new { pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, pat.MemberId, pat.PlanNumber, pat.GroupNumber } into g
                          select new
                          {
                              FullName = g.Key.NameLastName + (string.IsNullOrEmpty(g.Key.NameFirstName) ? "" : (", " + g.Key.NameFirstName)) + (string.IsNullOrEmpty(g.Key.NameMiddleName) ? "" : (" " + g.Key.NameMiddleName)),
                              g.Key.DateOfBirth,
                              g.Key.MemberId,
                              g.Key.PlanNumber,
                              g.Key.GroupNumber,
                              ClaimCount = g.Count(),
                              ClaimTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                              ClaimMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                              ClaimAverageRaw = g.Average(i => i.svc.ChargeAmount)
                          });

                var cmEvents = (from pat in con.MC_Patient
                                join svc in con.MC_Service on pat.ClaimID equals svc.ClaimID
                                join clm in con.MC_Claim on pat.ClaimID equals clm.ClaimID
                                where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                group new { pat, clm } by new { pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, pat.MemberId, pat.PlanNumber, pat.GroupNumber } into g
                                select new
                                {
                                    FullName = g.Key.NameLastName + (string.IsNullOrEmpty(g.Key.NameFirstName) ? "" : (", " + g.Key.NameFirstName)) + (string.IsNullOrEmpty(g.Key.NameMiddleName) ? "" : (" " + g.Key.NameMiddleName)),
                                    g.Key.DateOfBirth,
                                    g.Key.MemberId,
                                    EventCount = g.Select(i => i.clm.ClaimID).Distinct().Count()
                                });

                var cm = (from clm in cmClaims
                          join evt in cmEvents on new { clm.FullName, clm.DateOfBirth, clm.MemberId } equals new { evt.FullName, evt.DateOfBirth, evt.MemberId }
                          select new
                          {
                              clm.FullName,
                              clm.DateOfBirth,
                              clm.MemberId,
                              clm.PlanNumber,
                              clm.GroupNumber,
                              evt.EventCount,
                              clm.ClaimCount,
                              clm.ClaimTotalRaw,
                              clm.ClaimMaximumRaw,
                              clm.ClaimAverageRaw
                          });

                var result = cm.ToList().Select(l => new
                {
                    l.FullName,
                    l.DateOfBirth,
                    l.MemberId,
                    l.PlanNumber,
                    l.GroupNumber,
                    l.EventCount,
                    l.ClaimCount,
                    ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                    l.ClaimTotalRaw,
                    ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                    l.ClaimMaximumRaw,
                    ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                    l.ClaimAverageRaw
                });

                return Json(result, "Claimants", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var cmClaims = (from pat in con.MC_Patient
                                join svc in con.MC_Service on pat.ClaimID equals svc.ClaimID
                                where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                    && pat.MemberId == claimantID
                                group new { pat, svc } by new { pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, pat.MemberId, pat.PlanNumber, pat.GroupNumber } into g
                                select new
                                {
                                    FullName = g.Key.NameLastName + (string.IsNullOrEmpty(g.Key.NameFirstName) ? "" : (", " + g.Key.NameFirstName)) + (string.IsNullOrEmpty(g.Key.NameMiddleName) ? "" : (" " + g.Key.NameMiddleName)),
                                    g.Key.DateOfBirth,
                                    g.Key.MemberId,
                                    g.Key.PlanNumber,
                                    g.Key.GroupNumber,
                                    ClaimCount = g.Count(),
                                    ClaimTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                    ClaimMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                    ClaimAverageRaw = g.Average(i => i.svc.ChargeAmount)
                                });

                var cmEvents = (from pat in con.MC_Patient
                                join svc in con.MC_Service on pat.ClaimID equals svc.ClaimID
                                join clm in con.MC_Claim on pat.ClaimID equals clm.ClaimID
                                where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                group new { pat, clm } by new { pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, pat.MemberId, pat.PlanNumber, pat.GroupNumber } into g
                                select new
                                {
                                    FullName = g.Key.NameLastName + (string.IsNullOrEmpty(g.Key.NameFirstName) ? "" : (", " + g.Key.NameFirstName)) + (string.IsNullOrEmpty(g.Key.NameMiddleName) ? "" : (" " + g.Key.NameMiddleName)),
                                    g.Key.DateOfBirth,
                                    g.Key.MemberId,
                                    EventCount = g.Select(i => i.clm.ClaimID).Distinct().Count()
                                });

                var cm = (from clm in cmClaims
                          join evt in cmEvents on new { clm.FullName, clm.DateOfBirth, clm.MemberId } equals new { evt.FullName, evt.DateOfBirth, evt.MemberId }
                          select new
                          {
                              clm.FullName,
                              clm.DateOfBirth,
                              clm.MemberId,
                              clm.PlanNumber,
                              clm.GroupNumber,
                              evt.EventCount,
                              clm.ClaimCount,
                              clm.ClaimTotalRaw,
                              clm.ClaimMaximumRaw,
                              clm.ClaimAverageRaw
                          });

                var result = cm.ToList().Select(l => new
                {
                    l.FullName,
                    l.DateOfBirth,
                    l.MemberId,
                    l.PlanNumber,
                    l.GroupNumber,
                    l.EventCount,
                    l.ClaimCount,
                    ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                    l.ClaimTotalRaw,
                    ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                    l.ClaimMaximumRaw,
                    ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                    l.ClaimAverageRaw
                });

                return Json(result, "Claimants", JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetProceduresCodeResults(string startDate, string endDate, string lowerBoundCPT, string upperBoundCPT, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundCPT))
            {
                lowerBoundCPT = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundCPT))
            {
                upperBoundCPT = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cmClaims = (from svc in con.MC_Service
                            join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                            from cptOuter in oj.DefaultIfEmpty()
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(svc.ProcedureCode, lowerBoundCPT) != -1 && string.Compare(svc.ProcedureCode, upperBoundCPT) != 1
                            group new { svc } by new { svc.ProcedureCode, cptOuter.Description } into g
                            select new
                            {
                                g.Key.ProcedureCode,
                                g.Key.Description,
                                ProcedureCount = g.Count(),
                                ProcedureTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ProcedureMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ProcedureAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmEvents = (from svc in con.MC_Service
                            join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                            from cptOuter in oj.DefaultIfEmpty()
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(svc.ProcedureCode, lowerBoundCPT) != -1 && string.Compare(svc.ProcedureCode, upperBoundCPT) != 1
                            group new { svc, clm } by new { svc.ProcedureCode } into g
                            select new
                            {
                                g.Key.ProcedureCode,
                                EventCount = g.Select(x => x.clm.ClaimID).Distinct().Count()
                            });

            var cmClaimants = (from svc in con.MC_Service
                               join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                               from cptOuter in oj.DefaultIfEmpty()
                               join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                               join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                               group new { svc, pat } by new { svc.ProcedureCode } into g
                               select new
                               {
                                   g.Key.ProcedureCode,
                                   ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                               });

            var cmEmployees = (from svc in con.MC_Service
                               join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                               from cptOuter in oj.DefaultIfEmpty()
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on svc.ClaimID equals sub.ClaimID
                              group new { svc, sub } by new { svc.ProcedureCode } into g
                              select new
                              {
                                  g.Key.ProcedureCode,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from svc in con.MC_Service
                               join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                               from cptOuter in oj.DefaultIfEmpty()
                               join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on svc.ClaimID equals prv.ClaimID
                               group new { svc, prv } by new { svc.ProcedureCode } into g
                               select new
                               {
                                   g.Key.ProcedureCode,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join evt in cmEvents on clm.ProcedureCode equals evt.ProcedureCode
                      join pat in cmClaimants on clm.ProcedureCode equals pat.ProcedureCode
                      join sub in cmEmployees on clm.ProcedureCode equals sub.ProcedureCode
                      join prv in cmProviders on clm.ProcedureCode equals prv.ProcedureCode
                      orderby clm.ProcedureCode
                      select new
                      {
                          clm.ProcedureCode,
                          clm.Description,
                          clm.ProcedureCount,
                          clm.ProcedureTotalRaw,
                          clm.ProcedureMaximumRaw,
                          clm.ProcedureAverageRaw,
                          evt.EventCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ProcedureCode,
                l.Description,
                l.ProcedureCount,
                ProcedureTotal = l.ProcedureTotalRaw.ToString("C"),
                l.ProcedureTotalRaw,
                ProcedureMaximum = l.ProcedureMaximumRaw.ToString("C"),
                l.ProcedureMaximumRaw,
                ProcedureAverage = l.ProcedureAverageRaw.ToString("C"),
                l.ProcedureAverageRaw,
                l.EventCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return Json(result, "Procedures by Code", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProceduresCodeClaimsResults(string startDate, string endDate, string lowerBoundCPT, string upperBoundCPT, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundCPT))
            {
                lowerBoundCPT = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundCPT))
            {
                upperBoundCPT = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cm = (from svc in con.MC_Service
                      join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                      from cptOuter in oj.DefaultIfEmpty()
                      join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                      join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                      where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                            && string.Compare(svc.ProcedureCode, lowerBoundCPT) != -1 && string.Compare(svc.ProcedureCode, upperBoundCPT) != 1
                      orderby svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName ascending
                      select new { svc.Date, clm.ClaimNumber, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, svc.ChargeAmount, cptOuter.CPTCode, cptOuter.Description });

            var result = cm.ToList().Select(l => new
            {
                StatementDate = l.Date == null ? "" : ((DateTime)l.Date).ToString("yyyy-MM-dd"),
                ClaimNumber = l.ClaimNumber,
                FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                DateOfBirth = l.DateOfBirth == null ? "" : l.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                TotalClaim = l.ChargeAmount.ToString("C"),
                TotalClaimRaw = l.ChargeAmount,
                CPTCode = l.CPTCode,
                CPTDescription = l.Description
            });

            return Json(result, "Procedures by Code Claims", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProceduresGroupResults(string startDate, string endDate, string lowerBoundCPT, string upperBoundCPT, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundCPT))
            {
                lowerBoundCPT = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundCPT))
            {
                upperBoundCPT = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cmClaims = (from svc in con.MC_Service
                            join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(svc.ProcedureCode, lowerBoundCPT) != -1 && string.Compare(svc.ProcedureCode, upperBoundCPT) != 1
                            group new { svc, cg } by new { cg.GroupFromCode, cg.GroupToCode, cg.GroupName } into g
                            select new
                            {
                                ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                Description = g.Key.GroupName,
                                ProcedureGroupCount = g.Count(),
                                ProcedureGroupTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ProcedureGroupMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ProcedureGroupAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmEvents = (from svc in con.MC_Service
                            join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(svc.ProcedureCode, lowerBoundCPT) != -1 && string.Compare(svc.ProcedureCode, upperBoundCPT) != 1
                            group new { cg, clm } by new { cg.GroupFromCode, cg.GroupToCode, cg.GroupName } into g
                            select new
                            {
                                ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                EventCount = g.Select(x => x.clm.ClaimID).Distinct().Count()
                            });

            var cmClaimants = (from svc in con.MC_Service
                              join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                              group new { cg, pat } by new { cg.GroupFromCode, cg.GroupToCode } into g
                              select new
                              {
                                  ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from svc in con.MC_Service
                              join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on svc.ClaimID equals sub.ClaimID
                              group new { cg, sub } by new { cg.GroupFromCode, cg.GroupToCode } into g
                              select new
                              {
                                  ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from svc in con.MC_Service
                               join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                               join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on svc.ClaimID equals prv.ClaimID
                               group new { cg, prv } by new { cg.GroupFromCode, cg.GroupToCode } into g
                               select new
                               {
                                   ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join evt in cmEvents on clm.ProcedureGroup equals evt.ProcedureGroup
                      join pat in cmClaimants on clm.ProcedureGroup equals pat.ProcedureGroup
                      join sub in cmEmployees on clm.ProcedureGroup equals sub.ProcedureGroup
                      join prv in cmProviders on clm.ProcedureGroup equals prv.ProcedureGroup
                      orderby clm.ProcedureGroup
                      select new
                      {
                          clm.ProcedureGroup,
                          clm.Description,
                          clm.ProcedureGroupCount,
                          clm.ProcedureGroupTotalRaw,
                          clm.ProcedureGroupMaximumRaw,
                          clm.ProcedureGroupAverageRaw,
                          evt.EventCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ProcedureGroup,
                l.Description,
                l.ProcedureGroupCount,
                ProcedureGroupTotal = l.ProcedureGroupTotalRaw.ToString("C"),
                l.ProcedureGroupTotalRaw,
                ProcedureGroupMaximum = l.ProcedureGroupMaximumRaw.ToString("C"),
                l.ProcedureGroupMaximumRaw,
                ProcedureGroupAverage = l.ProcedureGroupAverageRaw.ToString("C"),
                l.ProcedureGroupAverageRaw,
                l.EventCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return Json(result, "Procedures by Group", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProceduresSubgroupResults(string startDate, string endDate, string lowerBoundCPT, string upperBoundCPT, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            if (string.IsNullOrWhiteSpace(lowerBoundCPT))
            {
                lowerBoundCPT = "0";
            }

            if (string.IsNullOrWhiteSpace(upperBoundCPT))
            {
                upperBoundCPT = "999999";
            }

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);
            decimal lowerBoundClaimAmountAsDecimal = GetClaimAmountFromString(lowerBoundClaimAmount, 0);
            decimal upperBoundClaimAmountAsDecimal = GetClaimAmountFromString(upperBoundClaimAmount, 99999999);

            var cmClaims = (from svc in con.MC_Service
                            join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(svc.ProcedureCode, lowerBoundCPT) != -1 && string.Compare(svc.ProcedureCode, upperBoundCPT) != 1
                            group new { csg, svc } by new { csg.SubgroupFromCode, csg.SubgroupToCode, csg.SubgroupName } into g
                            select new
                            {
                                ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                Description = g.Key.SubgroupName,
                                ProcedureSubgroupCount = g.Count(),
                                ProcedureSubgroupTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ProcedureSubgroupMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ProcedureSubgroupAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmEvents = (from svc in con.MC_Service
                            join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            where svc.Date >= startDateTime && svc.Date <= endDateTime && svc.ChargeAmount >= lowerBoundClaimAmountAsDecimal && svc.ChargeAmount <= upperBoundClaimAmountAsDecimal
                                  && string.Compare(svc.ProcedureCode, lowerBoundCPT) != -1 && string.Compare(svc.ProcedureCode, upperBoundCPT) != 1
                            group new { csg, clm } by new { csg.SubgroupFromCode, csg.SubgroupToCode, csg.SubgroupName } into g
                            select new
                            {
                                ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                EventCount = g.Select(x => x.clm.ClaimID).Distinct().Count()
                            });

            var cmClaimants = (from svc in con.MC_Service
                              join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                              group new { csg, pat } by new { csg.SubgroupFromCode, csg.SubgroupToCode } into g
                              select new
                              {
                                  ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from svc in con.MC_Service
                              join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on svc.ClaimID equals sub.ClaimID
                              group new { csg, sub } by new { csg.SubgroupFromCode, csg.SubgroupToCode } into g
                              select new
                              {
                                  ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from svc in con.MC_Service
                               join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                               join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on svc.ClaimID equals prv.ClaimID
                               group new { csg, prv } by new { csg.SubgroupFromCode, csg.SubgroupToCode } into g
                               select new
                               {
                                   ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join evt in cmEvents on clm.ProcedureSubgroup equals evt.ProcedureSubgroup
                      join pat in cmClaimants on clm.ProcedureSubgroup equals pat.ProcedureSubgroup
                      join sub in cmEmployees on clm.ProcedureSubgroup equals sub.ProcedureSubgroup
                      join prv in cmProviders on clm.ProcedureSubgroup equals prv.ProcedureSubgroup
                      orderby clm.ProcedureSubgroup
                      select new
                      {
                          clm.ProcedureSubgroup,
                          clm.Description,
                          clm.ProcedureSubgroupCount,
                          clm.ProcedureSubgroupTotalRaw,
                          clm.ProcedureSubgroupMaximumRaw,
                          clm.ProcedureSubgroupAverageRaw,
                          evt.EventCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ProcedureSubgroup,
                l.Description,
                l.ProcedureSubgroupCount,
                ProcedureSubgroupTotal = l.ProcedureSubgroupTotalRaw.ToString("C"),
                l.ProcedureSubgroupTotalRaw,
                ProcedureSubgroupMaximum = l.ProcedureSubgroupMaximumRaw.ToString("C"),
                l.ProcedureSubgroupMaximumRaw,
                ProcedureSubgroupAverage = l.ProcedureSubgroupAverageRaw.ToString("C"),
                l.ProcedureSubgroupAverageRaw,
                l.EventCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return Json(result, "Procedures by Subgroup", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadClaimCostsResults()
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cm = (from clm in con.MC_Claim
                      join pat in con.MC_Patient on clm.ClaimID equals pat.ClaimID
                      orderby pat.NameLastName, pat.NameFirstName, pat.NameMiddleName ascending
                      select new { clm.SerializableStatementFromDate, pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, clm.TotalClaimChargeAmount });

            var result = cm.ToList().Select(l => new
            {
                StatementDate = l.SerializableStatementFromDate == null ? "" : ((DateTime)l.SerializableStatementFromDate).ToString("yyyy-MM-dd"),
                FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                DateOfBirth = l.DateOfBirth == null ? "" : l.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                TotalClaim = l.TotalClaimChargeAmount == null ? "" : l.TotalClaimChargeAmount.ToString("C"),
                TotalClaimRaw = l.TotalClaimChargeAmount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadDiagnosesICDResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cmClaims = (from d in con.MC_Diagnosis
                            join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                            join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                            group new { d, clm } by new { d.Code, icd9.DescriptionLong, icd9.DescriptionShort } into g
                            select new
                            {
                                ICDCode = g.Key.Code,
                                DescriptionLong = g.Key.DescriptionLong,
                                DescriptionShort = g.Key.DescriptionShort,
                                ClaimCount = g.Count(),
                                ClaimTotalRaw = g.Sum(i => i.clm.TotalClaimChargeAmount),
                                ClaimMaximumRaw = g.Max(i => i.clm.TotalClaimChargeAmount),
                                ClaimAverageRaw = g.Average(i => i.clm.TotalClaimChargeAmount)
                            });

            var cmClaimants = (from d in con.MC_Diagnosis
                               join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join pat in con.MC_Patient on clm.ClaimID equals pat.ClaimID
                               group new { d, pat } by new { d.Code } into g
                               select new
                               {
                                   ICDCode = g.Key.Code,
                                   ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                               });

            var cmEmployees = (from d in con.MC_Diagnosis
                               join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join sub in con.MC_Subscriber on clm.ClaimID equals sub.ClaimID
                               group new { d, sub } by new { d.Code } into g
                               select new
                               {
                                   ICDCode = g.Key.Code,
                                   EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                               });

            var cmProviders = (from d in con.MC_Diagnosis
                               join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on d.ClaimID equals prv.ClaimID
                               group new { d, prv } by new { d.Code } into g
                               select new
                               {
                                   ICDCode = g.Key.Code,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join pat in cmClaimants on clm.ICDCode equals pat.ICDCode
                      join sub in cmEmployees on clm.ICDCode equals sub.ICDCode
                      join prv in cmProviders on clm.ICDCode equals prv.ICDCode
                      orderby clm.ICDCode
                      select new
                      {
                          clm.ICDCode,
                          clm.DescriptionLong,
                          clm.DescriptionShort,
                          ICDDescription = clm.ICDCode,
                          clm.ClaimTotalRaw,
                          clm.ClaimMaximumRaw,
                          clm.ClaimAverageRaw,
                          clm.ClaimCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ICDCode,
                l.DescriptionLong,
                l.DescriptionShort,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw,
                l.ClaimCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadDiagnosesICDClaimsResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cmClaims = (from d in con.MC_Diagnosis
                            join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                            join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                            group new { d, clm } by new { d.Code, icd9.DescriptionLong, icd9.DescriptionShort } into g
                            select new
                            {
                                ICDCode = g.Key.Code,
                                DescriptionLong = g.Key.DescriptionLong,
                                DescriptionShort = g.Key.DescriptionShort,
                                ClaimCount = g.Count(),
                                ClaimTotalRaw = g.Sum(i => i.clm.TotalClaimChargeAmount),
                                ClaimMaximumRaw = g.Max(i => i.clm.TotalClaimChargeAmount),
                                ClaimAverageRaw = g.Average(i => i.clm.TotalClaimChargeAmount)
                            });

            var cmClaimants = (from d in con.MC_Diagnosis
                               join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join pat in con.MC_Patient on clm.ClaimID equals pat.ClaimID
                               group new { d, pat } by new { d.Code } into g
                               select new
                               {
                                   ICDCode = g.Key.Code,
                                   ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                               });

            var cmEmployees = (from d in con.MC_Diagnosis
                               join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join sub in con.MC_Subscriber on clm.ClaimID equals sub.ClaimID
                               group new { d, sub } by new { d.Code } into g
                               select new
                               {
                                   ICDCode = g.Key.Code,
                                   EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                               });

            var cmProviders = (from d in con.MC_Diagnosis
                               join icd9 in con.MC_ICD9 on d.CodeNoDot equals icd9.Code
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on d.ClaimID equals prv.ClaimID
                               group new { d, prv } by new { d.Code } into g
                               select new
                               {
                                   ICDCode = g.Key.Code,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join pat in cmClaimants on clm.ICDCode equals pat.ICDCode
                      join sub in cmEmployees on clm.ICDCode equals sub.ICDCode
                      join prv in cmProviders on clm.ICDCode equals prv.ICDCode
                      orderby clm.ICDCode
                      select new
                      {
                          clm.ICDCode,
                          clm.DescriptionLong,
                          clm.DescriptionShort,
                          ICDDescription = clm.ICDCode,
                          clm.ClaimTotalRaw,
                          clm.ClaimMaximumRaw,
                          clm.ClaimAverageRaw,
                          clm.ClaimCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ICDCode,
                l.DescriptionLong,
                l.DescriptionShort,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw,
                l.ClaimCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadDiagnosesGroupResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cmClaims = (from d in con.MC_Diagnosis
                            join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                            join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                            group new { dg, clm } by new { dg.GroupId, dg.GroupName } into g
                            select new
                            {
                                g.Key.GroupId,
                                g.Key.GroupName,
                                ClaimCount = g.Count(),
                                ClaimTotalRaw = g.Sum(i => i.clm.TotalClaimChargeAmount),
                                ClaimMaximumRaw = g.Max(i => i.clm.TotalClaimChargeAmount),
                                ClaimAverageRaw = g.Average(i => i.clm.TotalClaimChargeAmount)
                            });

            var cmClaimants = (from d in con.MC_Diagnosis
                              join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on d.ClaimID equals pat.ClaimID
                              group new { dg, pat } by new { dg.GroupId } into g
                              select new
                              {
                                  g.Key.GroupId,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from d in con.MC_Diagnosis
                              join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on d.ClaimID equals sub.ClaimID
                              group new { dg, sub } by new { dg.GroupId } into g
                              select new
                              {
                                  g.Key.GroupId,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from d in con.MC_Diagnosis
                               join dg in con.MC_DiagnosticGroups on d.GroupId.Value equals dg.GroupId
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on d.ClaimID equals prv.ClaimID
                               group new { dg, prv } by new { dg.GroupId } into g
                               select new
                               {
                                   g.Key.GroupId,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join pat in cmClaimants on clm.GroupId equals pat.GroupId
                      join sub in cmEmployees on clm.GroupId equals sub.GroupId
                      join prv in cmProviders on clm.GroupId equals prv.GroupId
                      join dg in con.MC_DiagnosticGroups on clm.GroupId equals dg.GroupId
                      orderby clm.GroupName
                      select new
                      {
                          DiagnosticGroup = dg.GroupFromCode + "-" + dg.GroupToCode,
                          clm.GroupName,
                          clm.ClaimTotalRaw,
                          clm.ClaimMaximumRaw,
                          clm.ClaimAverageRaw,
                          clm.ClaimCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.DiagnosticGroup,
                l.GroupName,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw,
                l.ClaimCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadDiagnosesSubgroupResults(string startDate, string endDate, string lowerBoundICD, string upperBoundICD, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cmClaims = (from d in con.MC_Diagnosis
                            join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                            join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                            group new { dsg, clm } by new { dsg.SubgroupId, dsg.SubgroupName } into g
                            select new
                            {
                                g.Key.SubgroupId,
                                g.Key.SubgroupName,
                                ClaimCount = g.Count(),
                                ClaimTotalRaw = g.Sum(i => i.clm.TotalClaimChargeAmount),
                                ClaimMaximumRaw = g.Max(i => i.clm.TotalClaimChargeAmount),
                                ClaimAverageRaw = g.Average(i => i.clm.TotalClaimChargeAmount)
                            });

            var cmClaimants = (from d in con.MC_Diagnosis
                              join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on d.ClaimID equals pat.ClaimID
                              group new { dsg, pat } by new { dsg.SubgroupId } into g
                              select new
                              {
                                  g.Key.SubgroupId,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from d in con.MC_Diagnosis
                              join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                              join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on d.ClaimID equals sub.ClaimID
                              group new { dsg, sub } by new { dsg.SubgroupId } into g
                              select new
                              {
                                  g.Key.SubgroupId,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from d in con.MC_Diagnosis
                               join dsg in con.MC_DiagnosticSubgroups on d.SubgroupId.Value equals dsg.SubgroupId
                               join clm in con.MC_Claim on d.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on d.ClaimID equals prv.ClaimID
                               group new { dsg, prv } by new { dsg.SubgroupId } into g
                               select new
                               {
                                   g.Key.SubgroupId,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join pat in cmClaimants on clm.SubgroupId equals pat.SubgroupId
                      join sub in cmEmployees on clm.SubgroupId equals sub.SubgroupId
                      join prv in cmProviders on clm.SubgroupId equals prv.SubgroupId
                      join dsg in con.MC_DiagnosticSubgroups on clm.SubgroupId equals dsg.SubgroupId
                      orderby clm.SubgroupName
                      select new
                      {
                          DiagnosticSubgroup = dsg.SubgroupFromCode + "-" + dsg.SubgroupToCode,
                          clm.SubgroupName,
                          clm.ClaimTotalRaw,
                          clm.ClaimMaximumRaw,
                          clm.ClaimAverageRaw,
                          clm.ClaimCount,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.DiagnosticSubgroup,
                l.SubgroupName,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw,
                l.ClaimCount,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadProvidersResults(string providerNPI)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cm = (from prv in con.MC_Provider
                      join clm in con.MC_Claim on prv.ClaimID equals clm.ClaimID
                      group new { prv, clm } by new { prv.NameLastName, prv.NameFirstName, prv.NameMiddleName, prv.Npi } into g
                      select new
                      {
                          g.Key.NameLastName,
                          g.Key.NameFirstName,
                          g.Key.NameMiddleName,
                          g.Key.Npi,
                          ClaimCount = g.Count(),
                          ClaimTotalRaw = g.Sum(i => i.clm.TotalClaimChargeAmount),
                          ClaimMaximumRaw = g.Max(i => i.clm.TotalClaimChargeAmount),
                          ClaimAverageRaw = g.Average(i => i.clm.TotalClaimChargeAmount)
                      });

            var result = cm.ToList().Select(l => new
            {
                FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                l.Npi,
                l.ClaimCount,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadClaimantsResults(string claimantID)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cm = (from pat in con.MC_Patient
                      join clm in con.MC_Claim on pat.ClaimID equals clm.ClaimID
                      join sub in con.MC_Subscriber on pat.ClaimID equals sub.ClaimID
                      group new { pat, clm, sub } by new { pat.NameLastName, pat.NameFirstName, pat.NameMiddleName, pat.DateOfBirth, sub.MemberId, sub.PlanNumber, sub.GroupNumber } into g
                      select new
                      {
                          g.Key.NameLastName,
                          g.Key.NameFirstName,
                          g.Key.NameMiddleName,
                          g.Key.DateOfBirth,
                          g.Key.MemberId,
                          g.Key.PlanNumber,
                          g.Key.GroupNumber,
                          ClaimCount = g.Count(),
                          ClaimTotalRaw = g.Sum(i => i.clm.TotalClaimChargeAmount),
                          ClaimMaximumRaw = g.Max(i => i.clm.TotalClaimChargeAmount),
                          ClaimAverageRaw = g.Average(i => i.clm.TotalClaimChargeAmount)
                      });

            var result = cm.ToList().Select(l => new
            {
                FullName = l.NameLastName + (string.IsNullOrEmpty(l.NameFirstName) ? "" : (", " + l.NameFirstName)) + (string.IsNullOrEmpty(l.NameMiddleName) ? "" : (" " + l.NameMiddleName)),
                DateOfBirth = l.DateOfBirth == null ? "" : l.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                l.MemberId,
                l.PlanNumber,
                l.GroupNumber,
                l.ClaimCount,
                ClaimTotal = l.ClaimTotalRaw.ToString("C"),
                l.ClaimTotalRaw,
                ClaimMaximum = l.ClaimMaximumRaw.ToString("C"),
                l.ClaimMaximumRaw,
                ClaimAverage = l.ClaimAverageRaw.ToString("C"),
                l.ClaimAverageRaw
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadProceduresCodeResults(string startDate, string endDate, string lowerBoundCPT, string upperBoundCPT, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cmClaims = (from svc in con.MC_Service
                            join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                            from cptOuter in oj.DefaultIfEmpty()
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            group new { svc, cptOuter } by new { svc.ProcedureCode, cptOuter.Description } into g
                            select new
                            {
                                g.Key.ProcedureCode,
                                g.Key.Description,
                                ProcedureCount = g.Count(),
                                ProcedureTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ProcedureMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ProcedureAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmClaimants = (from svc in con.MC_Service
                               join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                               from cptOuter in oj.DefaultIfEmpty()
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                              group new { svc, pat } by new { svc.ProcedureCode } into g
                              select new
                              {
                                  g.Key.ProcedureCode,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from svc in con.MC_Service
                               join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                               from cptOuter in oj.DefaultIfEmpty()
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on svc.ClaimID equals sub.ClaimID
                              group new { svc, sub } by new { svc.ProcedureCode } into g
                              select new
                              {
                                  g.Key.ProcedureCode,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from svc in con.MC_Service
                               join cpt in con.MC_CPTCodes on svc.ProcedureCode equals cpt.CPTCode into oj
                               from cptOuter in oj.DefaultIfEmpty()
                               join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on svc.ClaimID equals prv.ClaimID
                               group new { svc, prv } by new { svc.ProcedureCode } into g
                               select new
                               {
                                   g.Key.ProcedureCode,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join pat in cmClaimants on clm.ProcedureCode equals pat.ProcedureCode
                      join sub in cmEmployees on clm.ProcedureCode equals sub.ProcedureCode
                      join prv in cmProviders on clm.ProcedureCode equals prv.ProcedureCode
                      orderby clm.ProcedureCode
                      select new
                      {
                          clm.ProcedureCode,
                          clm.Description,
                          clm.ProcedureCount,
                          clm.ProcedureTotalRaw,
                          clm.ProcedureMaximumRaw,
                          clm.ProcedureAverageRaw,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ProcedureCode,
                l.Description,
                l.ProcedureCount,
                ProcedureTotal = l.ProcedureTotalRaw.ToString("C"),
                l.ProcedureTotalRaw,
                ProcedureMaximum = l.ProcedureMaximumRaw.ToString("C"),
                l.ProcedureMaximumRaw,
                ProcedureAverage = l.ProcedureAverageRaw.ToString("C"),
                l.ProcedureAverageRaw,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadProceduresGroupResults(string startDate, string endDate, string lowerBoundCPT, string upperBoundCPT, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cmClaims = (from svc in con.MC_Service
                            join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            group new { svc, cg } by new { cg.GroupFromCode, cg.GroupToCode, cg.GroupName } into g
                            select new
                            {
                                ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                Description = g.Key.GroupName,
                                ProcedureGroupCount = g.Count(),
                                ProcedureGroupTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ProcedureGroupMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ProcedureGroupAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmClaimants = (from svc in con.MC_Service
                              join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                              group new { cg, pat } by new { cg.GroupFromCode, cg.GroupToCode } into g
                              select new
                              {
                                  ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from svc in con.MC_Service
                              join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on svc.ClaimID equals sub.ClaimID
                              group new { cg, sub } by new { cg.GroupFromCode, cg.GroupToCode } into g
                              select new
                              {
                                  ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from svc in con.MC_Service
                               join cg in con.MC_CPTGroups on svc.GroupId equals cg.GroupId
                               join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on svc.ClaimID equals prv.ClaimID
                               group new { cg, prv } by new { cg.GroupFromCode, cg.GroupToCode } into g
                               select new
                               {
                                   ProcedureGroup = g.Key.GroupFromCode + "-" + g.Key.GroupToCode,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join pat in cmClaimants on clm.ProcedureGroup equals pat.ProcedureGroup
                      join sub in cmEmployees on clm.ProcedureGroup equals sub.ProcedureGroup
                      join prv in cmProviders on clm.ProcedureGroup equals prv.ProcedureGroup
                      orderby clm.ProcedureGroup
                      select new
                      {
                          clm.ProcedureGroup,
                          clm.Description,
                          clm.ProcedureGroupCount,
                          clm.ProcedureGroupTotalRaw,
                          clm.ProcedureGroupMaximumRaw,
                          clm.ProcedureGroupAverageRaw,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ProcedureGroup,
                l.Description,
                l.ProcedureGroupCount,
                ProcedureGroupTotal = l.ProcedureGroupTotalRaw.ToString("C"),
                l.ProcedureGroupTotalRaw,
                ProcedureGroupMaximum = l.ProcedureGroupMaximumRaw.ToString("C"),
                l.ProcedureGroupMaximumRaw,
                ProcedureGroupAverage = l.ProcedureGroupAverageRaw.ToString("C"),
                l.ProcedureGroupAverageRaw,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        public ActionResult DownloadProceduresSubgroupResults(string startDate, string endDate, string lowerBoundCPT, string upperBoundCPT, string lowerBoundClaimAmount, string upperBoundClaimAmount)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cmClaims = (from svc in con.MC_Service
                            join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                            join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                            group new { csg, svc } by new { csg.SubgroupFromCode, csg.SubgroupToCode, csg.SubgroupName } into g
                            select new
                            {
                                ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                Description = g.Key.SubgroupName,
                                ProcedureSubgroupCount = g.Count(),
                                ProcedureSubgroupTotalRaw = g.Sum(i => i.svc.ChargeAmount),
                                ProcedureSubgroupMaximumRaw = g.Max(i => i.svc.ChargeAmount),
                                ProcedureSubgroupAverageRaw = g.Average(i => i.svc.ChargeAmount)
                            });

            var cmClaimants = (from svc in con.MC_Service
                              join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join pat in con.MC_Patient on svc.ClaimID equals pat.ClaimID
                              group new { csg, pat } by new { csg.SubgroupFromCode, csg.SubgroupToCode } into g
                              select new
                              {
                                  ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                  ClaimantCount = g.Select(x => x.pat.MemberId).Distinct().Count()
                              });

            var cmEmployees = (from svc in con.MC_Service
                              join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                              join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                              join sub in con.MC_Subscriber on svc.ClaimID equals sub.ClaimID
                              group new { csg, sub } by new { csg.SubgroupFromCode, csg.SubgroupToCode } into g
                              select new
                              {
                                  ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                  EmployeeCount = g.Select(x => x.sub.MemberId).Distinct().Count()
                              });

            var cmProviders = (from svc in con.MC_Service
                               join csg in con.MC_CPTSubgroups on svc.SubgroupId equals csg.SubgroupId
                               join clm in con.MC_Claim on svc.ClaimID equals clm.ClaimID
                               join prv in con.MC_Provider on svc.ClaimID equals prv.ClaimID
                               group new { csg, prv } by new { csg.SubgroupFromCode, csg.SubgroupToCode } into g
                               select new
                               {
                                   ProcedureSubgroup = g.Key.SubgroupFromCode + "-" + g.Key.SubgroupToCode,
                                   ProviderCount = g.Select(x => x.prv.Npi).Distinct().Count()
                               });

            var cm = (from clm in cmClaims
                      join pat in cmClaimants on clm.ProcedureSubgroup equals pat.ProcedureSubgroup
                      join sub in cmEmployees on clm.ProcedureSubgroup equals sub.ProcedureSubgroup
                      join prv in cmProviders on clm.ProcedureSubgroup equals prv.ProcedureSubgroup
                      orderby clm.ProcedureSubgroup
                      select new
                      {
                          clm.ProcedureSubgroup,
                          clm.Description,
                          clm.ProcedureSubgroupCount,
                          clm.ProcedureSubgroupTotalRaw,
                          clm.ProcedureSubgroupMaximumRaw,
                          clm.ProcedureSubgroupAverageRaw,
                          pat.ClaimantCount,
                          sub.EmployeeCount,
                          prv.ProviderCount
                      });

            var result = cm.ToList().Select(l => new
            {
                l.ProcedureSubgroup,
                l.Description,
                l.ProcedureSubgroupCount,
                ProcedureSubgroupTotal = l.ProcedureSubgroupTotalRaw.ToString("C"),
                l.ProcedureSubgroupTotalRaw,
                ProcedureSubgroupMaximum = l.ProcedureSubgroupMaximumRaw.ToString("C"),
                l.ProcedureSubgroupMaximumRaw,
                ProcedureSubgroupAverage = l.ProcedureSubgroupAverageRaw.ToString("C"),
                l.ProcedureSubgroupAverageRaw,
                l.ClaimantCount,
                l.EmployeeCount,
                l.ProviderCount
            });

            return File(DumpExcel(LINQToDataTable(result)), "application/Snapshot.xlsx", "Snapshot.xlsx");
        }

        private DataTable LINQToDataTable<T>(IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow 
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue(rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        private byte[] DumpExcel(DataTable tbl)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                // Create the worksheet
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Snapshot");

                // Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                worksheet.Cells["A1"].LoadFromDataTable(tbl, true);

                // Autofit the columns, to make the spreadsheet readable on first opening
                for (int i = 1; i <= worksheet.Dimension.Columns; i++)
                {
                    if (tbl.Columns[i - 1].DataType.FullName.ToLower().Contains("date"))
                    {
                        worksheet.Column(i).Style.Numberformat.Format = "m/d/yyyy";
                    }
                    worksheet.Column(i).AutoFit();
                }

                return pck.GetAsByteArray();
            }
        }

        public async Task<ActionResult> GetFullClaimsSummary()
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            var cm = (from svc in con.MC_Service
                      select svc.ChargeAmount).ToList();
 
            var result = new {
                Count = cm.Count.ToString(),
                Total = cm.Sum(s => s).ToString(),
                Maximum = cm.Max(m => m).ToString(),
                Average = cm.Average(a => a).ToString()
            };

            return Json(result, "Full Claims Summary", JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFilteredClaimsSummary(string startDate, string endDate)
        {
            var con = new Sweeper_DAL.SweeperUIEntitiesQA();

            var manager = ClaimsManager;

            var master = manager.checkAccess(new string[] { UMSClaims.CLAIM_DEV, UMSClaims.CLAIM_USHC_ADMIN, UMSClaims.CLAIM_MASTER });

            DateTime startDateTime = GetDateFromString(startDate, new DateTime(2000, 1, 1));
            DateTime endDateTime = GetDateFromString(endDate, DateTime.Now);

            var cm = (from svc in con.MC_Service
                      where svc.Date >= startDateTime && svc.Date <= endDateTime
                      select svc.ChargeAmount).ToList();

            var result = new
            {
                Count = cm.Count.ToString(),
                Total = cm.Sum(s => s).ToString(),
                Maximum = cm.Max(m => m).ToString(),
                Average = cm.Average(a => a).ToString()
            };

            return Json(result, "Filtered Claims Summary", JsonRequestBehavior.AllowGet);
        }


        private decimal GetClaimAmountFromString(string claimAmountAsString, decimal defaultValue)
        {
            decimal claimAmountAsDecimal;
            if (decimal.TryParse(claimAmountAsString, out claimAmountAsDecimal))
            {
                return claimAmountAsDecimal;
            }
            else
            {
                return defaultValue;
            }
        }

        private DateTime GetDateFromString(string dateAsString, DateTime defaultValue)
        {
            DateTime dateAsDateTime;
            if (DateTime.TryParse(dateAsString, out dateAsDateTime))
            {
                return dateAsDateTime;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}