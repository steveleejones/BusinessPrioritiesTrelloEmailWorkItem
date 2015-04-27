using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BusinessPrioritiesTrelloEmailWorkItem.Objects
{
    public class Emailer
    {
        public static void SendEmail(string toEmail, string Subject, string Message, bool UseNoReplyAddress, string replyToAddress)
        {
            string fromEmail = "";
            if (UseNoReplyAddress)
                fromEmail = "noreply@lowcostholidays.com";
            else
                fromEmail = "PPCEngine@lowcostholidays.com";


            if (Environment.UserInteractive)
            {
                fromEmail = "PPCEngine@lowcostholidays.com";
            }


            MailMessage mail = new MailMessage(fromEmail, toEmail, Subject, Message);
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Priority = MailPriority.Normal;
            mail.IsBodyHtml = true;
            mail.ReplyToList.Add(replyToAddress);

            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential("LOWCOSTBEDS\\SQLAdmin", "SQL4dmin123");
            client.Port = 25;
            client.Host = "svrexch1.lowcostbeds.com";
            client.EnableSsl = false;
            client.UseDefaultCredentials = true;
            Object mailState = mail;

            try
            {
                client.SendAsync(mail, mailState);
                //client.Send(mail);
            }
            catch (Exception)
            {

            }
        }
        public static string EmailContent(List<TrelloList> Lists, string name)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<br />");
            sb.Append("<br />");
            foreach (TrelloList list in Lists)
            {

                sb.Append("<table style='width:95%; border:1px solid black;font-family:Calibri;font-size:13px;'>");
                sb.Append("<tr style='color:red;'><td colspan='4'>" + list.Name + " (" + list.Cards.Count.ToString() + " items)</td></tr>");
                sb.Append("<tr style='font-weight:bold;'><td style='background-color:red;text-align:center;width:5%;'>LCTG ID</td><td style='background-color:yellow;text-align:center;width:5%;'>Zendesk ID</td><td>Summary</td><td></td></tr>");

                foreach (TrelloCard cardItem in list.Cards)
                {
                    int hyphenLocation = 0;
                    try
                    {
                        hyphenLocation = cardItem.Name.IndexOf('-');
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    string ids = cardItem.Name.Remove(hyphenLocation);
                    List<string> items = ids.Split('/').ToList();
                    string NameWithNoIds = cardItem.Name.Remove(0, hyphenLocation + 1);

                    sb.Append("<tr style='background-color:#46adad;border:none;'><td style='text-align:center;'>" + items[0] + "</td><td style='text-align:center;'>" + items[1] + "</td><td colspan='2'>" + NameWithNoIds + "</td></tr>");
                    sb.Append("<tr><td colspan='2'></td><td colspan='2'>" + cardItem.Desc.Replace("\n\n", "<br />") + "</td></tr>");


                    if (cardItem.Actions != null && cardItem.Actions.Count > 0)
                    {
                        sb.Append("<tr><td colspan='2'></td><td colspan='2'><font style='color:red;'>Last / Next Action:</font> -  " + cardItem.Actions[0].Description + "</td></tr>");

                    }
                }

                sb.Append("</table>");
            }

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Link to Board https://trello.com/b/41yw0LYH/");
            sb.Append(Environment.NewLine);
            sb.Append("The aim of this report is to highlight issues managed by the Application (iVector) Support team which have a high level of urgency due to financial or commercial impact. ");
            sb.Append(Environment.NewLine);
            sb.Append("Please feel free to raise any queries to me directly regarding the content or quality of this report. If advising of urgent items that are not considered in this report, please note LCTG ticket ID(s) where possible and detail the impact to the business.");

            return sb.ToString();
        }
    }
}
