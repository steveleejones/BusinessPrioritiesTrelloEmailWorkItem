using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDataLibrary;
using SearchStore;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //BusinessPrioritiesTrelloEmail trelloTest = new BusinessPrioritiesTrelloEmail();
            //trelloTest.Run();
            //WorkItemTestbed.WorkItemTestbedWorkItem workItem = new WorkItemTestbedWorkItem();
            //workItem.Run();
            ReportLogger reportLogger = new ReportLogger("SearchStoreTest");
            SearchStoreWorkItem workItem = new SearchStoreWorkItem();
            bool val = workItem.DoWork(reportLogger);
        }
    }
}
