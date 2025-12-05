using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;
using TTA_API.Data;
using TTA_API.Models;

namespace TTA_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private IConfigurationRoot configuration;
        private string FromMailAddress;
        private string SmtpClient;
        private string Credentials;
        private string EmailFooterName;
        private string SiteURL;
        private int SmtpClientPort;

        public EmailService(ApplicationDbContext context)
        {
            _context = context;
            configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())   
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

             FromMailAddress = configuration["EmailService:FromMailAddress"];
             SmtpClient = configuration["EmailService:SmtpClient"];
             Credentials = configuration["EmailService:Credentials"];
             SmtpClientPort = int.Parse(configuration["EmailService:SmtpClientPort"]);
             EmailFooterName = configuration["EmailService:EmailFooterName"];
             SiteURL = configuration["EmailService:SiteURL"];
        }
        public async Task<IEnumerable<TravelSchedules>> SendEmailAsync(bool isForNewUserCreation = false, User newUser= null, int month =default, int year= default)
        {
            if(isForNewUserCreation)
            {
                SendEmailForNewUserCreation(newUser);
            }
            else
            {
                SendEmailAllocationSchedule(month,year);
            }

            

            return Enumerable.Empty<TravelSchedules>();

        }

        private void SendEmailForNewUserCreation(User newUser)
        {
            StringBuilder htmlBody = new StringBuilder();
            htmlBody.AppendLine(@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        body {
            font-family: Arial, sans-serif;
            color: #333;
            margin: 0;
            padding: 20px;
        }
        .container {
            max-width: 600px;
            margin: auto;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 20px;
            background-color: #fafafa;
        }
        h2 {
            text-align: center;
            color: #444;
        }
        .info-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }
        th, td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }
        th {
            background-color: #f4f4f4;
            font-weight: bold;
        }
        .badge {
            padding: 6px 10px;
            border-radius: 12px;
            font-size: 12px;
            color: #fff;
        }
        .pin-badge {
            background-color: #d6a8ff;
            font-weight: bold;
            letter-spacing: 3px;
            font-size: 18px;
        }
        .highlight {
            color: #4a4aff;
            font-weight: bold;
        }
        .footer {
            text-align: center;
            margin-top: 30px;
            font-size: 13px;
            color: #777;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <p style=""text-align:left; font-size:14px; color:#666;"">
            ");
            htmlBody.Append($"Dear <b>{newUser.Name}</b>,  </p>");

            htmlBody.Append(@"
    <p style=""text-align:left; font-size:14px; color:#666;"">
        Welcome to <b>C2C-TTA</b>!<br><br>We’re excited to have you on board! Your account has been successfully created.
    </p>

    <h2>Account Details</h2>

    <table class=""info-table"">
        <tr>
            <th>Email</th>
            <td>");

            htmlBody.Append(newUser.Email);
            htmlBody.Append(@"</td>
            </tr>
            <tr>
                <th>User Name</th>
                <td>");
            htmlBody.Append(newUser.Name);
            htmlBody.Append(@"</td>
            </tr>
            <tr>
                <th>Registered On</th>
                <td>");
            htmlBody.Append(newUser.CreatedTime);
            htmlBody.Append(@"</td>
            </tr>
            <tr>
                <th>PIN</th>
                <td><span class=""badge pin-badge"">");
            htmlBody.Append(newUser.Pin);
            htmlBody.Append(@"</span></td>
            </tr>
        </table>

        <p style=""text-align:left; margin-top:20px; font-size:14px; color:#666;"">
            Please use this above <span class=""highlight"">PIN</span> to log-in your account. Please change The Pin from profile section if needed.  
        </p>

        <p style=""text-align:left; margin-top:20px; font-size:14px; color:#666;"">
            Thank You, <br>
            <b>");
            htmlBody.Append(EmailFooterName.Contains("Admin") ? _context.Users.Where(x=>x.Name.Equals("System Admin")).Select(x => x.Name).FirstOrDefault() : "Sarath S");
            htmlBody.Append(@"</b>
        </p>

        <div class=""footer"">
            Proceed with your First Login Using the below Link.<br>
            <a href=""");
            htmlBody.Append(SiteURL);
            htmlBody.Append(@""" style=""color:#4a4aff; text-decoration:none;"">");
            htmlBody.Append(SiteURL);
            htmlBody.Append(@"</a>
        </div>
    </div>
</body>
</html>
");
            string subject = $"{newUser.Name}, Welcome to C2C-TTA";
            SendEmail(newUser.Email, subject,htmlBody.ToString());
        }

        private void SendEmailAllocationSchedule(int month, int year)
        {
            var userAllocationIds = _context.AllocationTravelers
                    .Select(at => at.AllocationId)
                    .Distinct()
                    .ToList();

            // Step 2: Get co-travelers (already in memory)
            var coTravelers = _context.AllocationTravelers
                //.Where(at => at.UserId != currentUserId && userAllocationIds.Contains(at.AllocationId))
                .GroupBy(at => at.AllocationId)
                .Select(g => new
                {
                    AllocationId = g.Key,
                    TravelingWithName = string.Join(", ", g.Select(t => t.User.Name))
                })
                .ToList(); // Materialize here

            // Step 3: Get travel schedule (materialize Allocations too)
            var allocations = _context.Allocations
                .Where(a => userAllocationIds.Contains(a.Id) && a.AllocationDate.Month == month && a.AllocationDate.Year == year)
                .OrderBy(a => a.AllocationDate)
                .ToList(); // Materialize before projection


            var travelSchedules = (from a in allocations
                                   join ct in coTravelers on a.Id equals ct.AllocationId into ctGroup
                                   from ct in ctGroup.DefaultIfEmpty()
                                   where a.Booker != null
                                   select new TravelSchedules
                                   {
                                       TravelDate = a.AllocationDate.ToString("dddd, MMMM d"),
                                       TravelMonth = a.AllocationDate.ToString("MMM").ToUpper(),
                                       TripType = a.TripType,
                                       Booker = a.Booker.Name,
                                       TravelingWith = ct?.TravelingWithName ?? "Just me!"
                                   }).ToList();


            var usersToSendEmail = _context.Users.Where(user => user.SendEmail).ToList();
            foreach (var user in usersToSendEmail)
            {
                var availableSchedules = travelSchedules.Where(x => x.Booker == user.Name).ToList();

                if (availableSchedules.Any())
                {
                    PrepareContentAndSendEmail(travelSchedules.Where(x => x.Booker == user.Name).ToList(), user.Email, user.Name);
                }
            }
        }

        private void PrepareContentAndSendEmail(List<TravelSchedules> schedules, string toEmail, string name)
        {
            var systemSettings = _context.SystemSettings.FirstOrDefault();
            StringBuilder allocationScheduleList = new StringBuilder();
            var travelMonth = string.Join(", ", schedules.Select(x => x.TravelMonth).ToHashSet());

            foreach (var schedule in schedules)
            {
                allocationScheduleList.Append("<tr>");

                allocationScheduleList.Append("<td>" + schedule.TravelDate + "</td>");
                //if (schedule.TripType == "Departure" || schedule.TripType.Contains("dep"))
                if (schedule.TripType == systemSettings.DepartureLabel)
                {
                    allocationScheduleList.Append("<td><span class=\"badge departure\">");
                    allocationScheduleList.Append(systemSettings.DepartureLabel);
                    allocationScheduleList.Append("</span></td>");
                }
                else
                {
                    allocationScheduleList.Append("<td><span class=\"badge arrival\">");
                    allocationScheduleList.Append(systemSettings.ArrivalLabel);
                    allocationScheduleList.Append("</span></td>");
                }
                allocationScheduleList.Append("<td class=\"booker\">" + schedule.Booker + "</td>");
                allocationScheduleList.Append("<td>" + schedule.TravelingWith + "</td>");
                allocationScheduleList.Append("</tr>");

            }

            string Subject = $"{travelMonth} Month Allocation Schedule";

            // HTML body
            StringBuilder htmlBody = new StringBuilder();
            htmlBody.AppendLine(@"
            <!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        body {
            font-family: Arial, sans-serif;
            color: #333;
            margin: 0;
            padding: 20px;
        }
        .container {
            max-width: 600px;
            margin: auto;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 20px;
            background-color: #fafafa;
        }
        h2 {
            text-align: center;
            color: #444;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }
        th, td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }
        th {
            background-color: #f4f4f4;
            font-weight: bold;
        }
        .badge {
            padding: 4px 8px;
            border-radius: 12px;
            font-size: 12px;
            color: #fff;
        }
        .departure {
            background-color: #4da3ff;
        }
        .arrival {
            background-color: #d6a8ff;
        }
        .booker {
            color: #4a4aff;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <p style=""text-align:left; margin-top:20px; font-size:14px; color:#666;"">
            Hi ");
            htmlBody.Append($"<b>{name}</b>,");
            htmlBody.Append(@"</p>
        <p style=""text-align:left; margin-top:20px; font-size:14px; color:#666;"">
            Here is your <b>");
            htmlBody.Append(travelMonth);
            htmlBody.Append(@"</b> Month travel schedule summary.
        </p>
        <h2>");
            htmlBody.Append(travelMonth);
            htmlBody.Append(@" Allocation Schedule</h2>
        <table>
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Source Location</th>
                    <th>Booker</th>
                    <th>Travelers</th>
                </tr>
            </thead>
            <tbody>");
            htmlBody.AppendLine(allocationScheduleList.ToString());
            htmlBody.AppendLine(@"</tbody>
        </table>
        <p style=""text-align:center; margin-top:20px; font-size:14px; color:#666;"">
             Please review and plan accordingly.
        </p>
        <p style=""text-align:left; margin-top:20px; font-size:14px; color:#666;"">
            Thank You, 
            <br><b>");
            htmlBody.Append(EmailFooterName.Contains("Admin") ? _context.Users.Where(x => x.Name.Equals("Allocation Admin")).Select(x => x.Name).FirstOrDefault() : "Sarath S");
            htmlBody.Append(@"</b>
        </p>
    </div>
</body>
</html>
");

            SendEmail(toEmail, Subject, htmlBody.ToString());
        }

        private void SendEmail(string toEmailAddress, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(FromMailAddress);
                mail.To.Add(toEmailAddress);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient(SmtpClient, SmtpClientPort);
                smtp.Credentials = new NetworkCredential(FromMailAddress, Credentials);
                smtp.EnableSsl = true;

                // Send email
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }



    public class TravelSchedules
    {
        public string TravelDate { get; set; }
        public string TravelMonth { get; set; }
        public string TripType { get; set; }
        public string Booker { get; set; }
        public string TravelingWith { get; set; }
    }
}
