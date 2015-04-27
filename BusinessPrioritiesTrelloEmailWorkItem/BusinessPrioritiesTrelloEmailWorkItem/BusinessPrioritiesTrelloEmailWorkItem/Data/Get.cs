using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessPrioritiesTrelloEmailWorkItem.Data
{
    public class Get
    {
        public static Dictionary<string, string> Contacts()
        {
            DateTime mostRecentDate = new DateTime();

            DataTable data = new DataTable();
            string sql = "SELECT * FROM BusinessPrioritiesContacts";
            using (SqlConnection _con = new SqlConnection(CoreDataLibrary.Data.DataConnection.SqlConnNonCoreData))
            using (SqlCommand _cmd = new SqlCommand(sql, _con))
            {
                _cmd.CommandTimeout = 60;//Try for 1 min
                _con.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(_cmd);
                adapter.Fill(data);
            }

            Dictionary<string, string> contacts = new Dictionary<string, string>();

            foreach (DataRow row in data.Rows)
            {
                contacts.Add(row["EmailAddress"].ToString(), row["Name"].ToString());
            }

            return contacts;
        }
    }
}
