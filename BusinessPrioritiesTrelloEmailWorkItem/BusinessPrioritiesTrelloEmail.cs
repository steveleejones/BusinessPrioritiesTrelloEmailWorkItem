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
    public class BusinessPrioritiesTrelloEmail : BaseNonCoreWorkItem
    {
        string key = "c7ab58296ffb2e4c5d4582b75fc95a66";
        string token = "b6d02a5f06081f60ca5ad1f42299ba9804dcad60d3dae3e5e2b8c2bf796011d1";

        public override string Name
        {
            get { return "Business Priorities Trello Email"; }
        }

        public override bool DoWork(ReportLogger reportLogger)
        {
            int stepId = reportLogger.AddStep();
            try
            {
                List<TrelloList> lists = GetLists(key, token, reportLogger);
                //Returns list of ids etc in correct order as per on screen

                Dictionary<string, string> contacts = Get.Contacts();

                foreach (KeyValuePair<string, string> contact in contacts)
                {
                    // TODO message could possibly be move out of the foreach loop. 
                    // TODO contact.Value is not used in the EmailContent function.
                    string message = Emailer.EmailContent(lists, contact.Value);

                    Emailer.SendEmail(contact.Key,
                        "LCTG Business Priorities", message, false,
                        "dave.evans@lowcosttravelgroup.com");
                }
                reportLogger.EndStep(stepId);
            }
            catch (Exception ex)
            {
                reportLogger.EndStep(stepId, ex);
                return false;
            }
            return true;
        }

        public override bool DueToRun()
        {
            if (QuedToRun)
            {
                QuedToRun = false;
                LastRun = DateTime.Now;
                return true;
            }

            ReadFromDb();
            var dateTime = DateTime.Now;
            var timeSpan = dateTime - LastRun;

            if (timeSpan.TotalMinutes >= 60 & (dateTime.Hour == 7 | dateTime.Hour == 19))
            {
                LastRun = dateTime;
                return true;
            }
            return false;
        }

        private List<TrelloList> GetLists(string key, string token, ReportLogger reportLogger)
        {
            List<TrelloList> lists = new List<TrelloList>();

            string content = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.trello.com/1/board/51f0df83770f50bd3e00783e/lists?key=" + key + "&token=" + token);
            int stepId = reportLogger.AddStep();

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
                    list.Closed = Convert.ToBoolean(subitem["closed"].Value<bool>());
                    list.Id = subitem["id"].ToString();
                    list.Pos = Convert.ToInt32(subitem["pos"].Value<int>());
                    

                    list.Cards = new List<TrelloCard>();
                    list.Cards = GetListsCards(key, token, list.Id, reportLogger);

                    lists.Add(list);
                }
                reportLogger.EndStep(stepId);
            }
            catch (Exception ex)
            {
                reportLogger.EndStep(stepId, ex);
            }

            return lists;
        }

        private List<TrelloCard> GetListsCards(string key, string token, string listid, ReportLogger reportLogger)
        {
            List<TrelloCard> cards = new List<TrelloCard>();

            string content = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.trello.com/1/lists/" + listid + "/cards?key=" + key + "&token=" + token);

            int stepId = reportLogger.AddStep();

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
                    card.Actions = GetCardsActions(key, token, card.Id, reportLogger);
                    card.DateCreated = CreatedDate(subitem["id"].ToString());
                    cards.Add(card);
                }
                reportLogger.EndStep(stepId);
            }
            catch (Exception ex)
            {
                reportLogger.EndStep(stepId, ex);
            }
            return cards;
        }

        private List<TrelloAction> GetCardsActions(string key, string token, string cardid, ReportLogger reportLogger)
        {
            List<TrelloAction> actions = new List<TrelloAction>();

            string content = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.trello.com/1/cards/" + cardid + "/actions?key=" + key + "&token=" + token);

            int stepId = reportLogger.AddStep();

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
                reportLogger.EndStep(stepId);
            }
            catch (Exception ex)
            {
                reportLogger.EndStep(stepId, ex);
            }
            return actions;
        }

        private DateTime CreatedDate(string id)
        {
            int seconds = int.Parse(id.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
            DateTime baseDateTime = new DateTime(1970,1,1);
            DateTime creationDate = baseDateTime.AddSeconds(seconds);
            return creationDate;
        }
    }
}
