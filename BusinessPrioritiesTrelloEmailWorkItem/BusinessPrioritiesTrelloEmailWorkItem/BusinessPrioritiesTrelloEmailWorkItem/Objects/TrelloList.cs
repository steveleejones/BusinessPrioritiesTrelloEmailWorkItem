using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessPrioritiesTrelloEmailWorkItem.Objects
{
    public class TrelloList
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Closed { get; set; }
        public string BoardId { get; set; }
        public int Pos { get; set; }
        public List<TrelloCard> Cards { get; set; }
    }
}
