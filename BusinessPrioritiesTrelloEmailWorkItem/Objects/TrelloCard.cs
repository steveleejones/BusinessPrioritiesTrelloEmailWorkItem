using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessPrioritiesTrelloEmailWorkItem.Objects
{
    public class TrelloCard
    {
        public string Id { get; set; }
        public string Desc { get; set; }
        public string Name { get; set; }
        public string ShortUrl { get; set; }
        public List<TrelloAction> Actions { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
