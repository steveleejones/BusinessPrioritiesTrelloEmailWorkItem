using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BusinessPrioritiesTrelloEmailWorkItem.Data;
using BusinessPrioritiesTrelloEmailWorkItem.Objects;
using CoreDataLibrary;
using CoreDataLibrary.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Emailer = BusinessPrioritiesTrelloEmailWorkItem.Objects.Emailer;

namespace BusinessPrioritiesTrelloEmailWorkItem
{
    public class BusinessPrioritiesTrelloEmail : BaseCoreDataWorkItem
    {
        string key = "c7ab58296ffb2e4c5d4582b75fc95a66";
        string token = "b6d02a5f06081f60ca5ad1f42299ba9804dcad60d3dae3e5e2b8c2bf796011d1";

        public override string Name
        {
            get { return "Business Priorities Trello Email"; }
        }

        public override bool DueToRun()
        {
            if (QuedToRun)
            {
                QuedToRun = false;
                return true;
            }

            //ReportLogger.AddMessage("Trello", "Due To Run");
            ReadFromDb();
            //ReportLogger.AddMessage("Trello", "ReadFrom Db() completed.");
            var dateTime = DateTime.Now;
            var timeSpan = dateTime - LastRun;

            //ReportLogger.AddMessage("Trello", "Check Timespan to see if it is due to run.");
            if (timeSpan.TotalMinutes >= 60 && dateTime.Hour == 7)
            {
                ReportLogger.AddMessage("Trello", "Due To Run.");
                LastRun = dateTime;
                return true;
            }
            return false;
        }

        public override bool Run()
        {
            if (DueToRun())
            {

                ReportLogger.AddMessage("Trello", "Passed Due To Run.");
                ReportLogger reportLogger = new ReportLogger(Name);
                int stepId = reportLogger.AddStep();
                try
                {
                    ReportLogger.AddMessage("Trello", "Running");
                    List<TrelloList> lists = GetLists(key, token);
                    //Returns list of ids etc in correct order as per on screen
                    ReportLogger.AddMessage("Trello", "GetList");

                    Dictionary<string, string> contacts = Get.Contacts();

                    foreach (KeyValuePair<string, string> contact in contacts)
                    {
                        string message = Emailer.EmailContent(lists, contact.Value);

                        Emailer.SendEmail(contact.Key,
                            "LCTG - Daily Business Priorities", message, false,
                            "dave.evans@lowcosttravelgroup.com");
                        ReportLogger.AddMessage("Trello", "Email sent.");
                    }
                    reportLogger.EndStep(stepId);
                    ReportLogger.AddMessage("Trello", "Completed");
                }
                catch (Exception ex)
                {
                    reportLogger.EndStep(stepId, ex);
                    return false;
                }
                reportLogger.EndLog();
                return true;
            }
            return false;
        }

        private List<TrelloList> GetLists(string key, string token)
        {
            List<TrelloList> lists = new List<TrelloList>();

            string content = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.trello.com/1/board/51f0df83770f50bd3e00783e/lists?key=" + key + "&token=" + token);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    content = reader.ReadToEnd();
                }


                dynamic stuff = JsonConvert.DeserializeObject(content);
                JArray jListItems = (JArray)stuff;

                foreach (var subitem in jListItems)
                {
                    TrelloList list = new TrelloList();
                    list.Name = subitem["name"].ToString();
                    list.BoardId = subitem["idBoard"].ToString();
                    list.Closed = Convert.ToBoolean(subitem["closed"]);
                    list.Id = subitem["id"].ToString();
                    list.Pos = Convert.ToInt32(subitem["pos"]);

                    list.Cards = new List<TrelloCard>();
                    list.Cards = GetListsCards(key, token, list.Id);

                    lists.Add(list);
                }

            }
            catch (Exception ex)
            {

                string error = ex.InnerException.ToString();
            }

            return lists;
        }

        private List<TrelloCard> GetListsCards(string key, string token, string listid)
        {
            List<TrelloCard> cards = new List<TrelloCard>();

            string content = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.trello.com/1/lists/" + listid + "/cards?key=" + key + "&token=" + token);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    content = reader.ReadToEnd();
                }


                dynamic stuff = JsonConvert.DeserializeObject(content);
                JArray jListItems = (JArray)stuff;

                foreach (var subitem in jListItems)
                {
                    TrelloCard card = new TrelloCard();

                    card.Id = subitem["id"].ToString();
                    card.Desc = subitem["desc"].ToString();
                    card.Name = subitem["name"].ToString();
                    card.ShortUrl = subitem["shortUrl"].ToString();
                    card.Actions = GetCardsActions(key, token, card.Id);
                    cards.Add(card);
                }

            }
            catch (Exception ex)
            {

                string error = ex.InnerException.ToString();
            }

            return cards;
        }

        private List<TrelloAction> GetCardsActions(string key, string token, string cardid)
        {
            List<TrelloAction> actions = new List<TrelloAction>();

            string content = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.trello.com/1/cards/" + cardid + "/actions?key=" + key + "&token=" + token);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    content = reader.ReadToEnd();
                }
                dynamic stuff = JsonConvert.DeserializeObject(content);
                JArray jListItems = (JArray)stuff;

                foreach (var subitem in jListItems)
                {
                    if (subitem["type"].ToString() == "commentCard")
                    {
                        TrelloAction action = new TrelloAction();

                        //This is a comment made on the card
                        var dataItem = subitem["data"];
                        action.Description = dataItem["text"].ToString();
                        action.Date = Convert.ToDateTime((subitem["date"].ToString()));
                        actions.Add(action);
                    }
                }

            }
            catch (Exception ex)
            {

                string error = ex.InnerException.ToString();
            }

            return actions;
        }
    }
}
