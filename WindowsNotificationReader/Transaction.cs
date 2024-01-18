using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsNotificationReader
{
    internal class Transaction
    {
        public string ClientName { get; set; }
        public decimal Amount{ get; set; }
    }
}
