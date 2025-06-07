using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyrsovoi
{
    internal class Class1
    {
        public static string l = "";
        public static int k = 0;
        public static string numberPhone = "";
        public static string numberPhoneEmploye = "";
        public static string connection = $"host={Properties.Settings.Default.host};uid={Properties.Settings.Default.user};pwd={Properties.Settings.Default.passwordDB};database={Properties.Settings.Default.database};";
        public static string connectionVostan = $"host={Properties.Settings.Default.host};uid={Properties.Settings.Default.user};pwd={Properties.Settings.Default.passwordDB};";
        public static string saveQuery = "";
        public static int add = 0;
        public static string fioEmploes = "fio";
        public static int id_employes = 0;
        public static int employee_id = 0;
        public static int unit_id = 0;
        public static string booking_id = "";
        public static int role = 0;
        public static int id_service = 0;
        public static string phone = "";
        public static int klient = 0;

    }
}
