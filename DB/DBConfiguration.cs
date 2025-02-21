using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.DB
{
    public class DBConfiguration
    {
        private static string dbConnectionString;
        private static string dbConnVJSQLDString;
        private static string dbConnVJSNSWString;
        private static string dbConnVJSVICString;
        private static string dbProviderName;

        static DBConfiguration()
        {
            dbConnectionString = A1RConsole.Properties.Settings.Default.A1RC;
            dbConnVJSQLDString = A1RConsole.Properties.Settings.Default.A1RVJSQLD;
            dbConnVJSNSWString = A1RConsole.Properties.Settings.Default.A1RVJSNSW;
            dbConnVJSVICString = A1RConsole.Properties.Settings.Default.A1RVJSVIC;
        }

        public static string DbConnectionString
        {
            get { return dbConnectionString; }
        }

        public static string DbConnVJSQLDString
        {
            get { return dbConnVJSQLDString; }
        }

        public static string DbConnVJSNSWString
        {
            get { return dbConnVJSNSWString; }
        }

        public static string DbConnVJSVICString
        {
            get { return dbConnVJSVICString; }
        }

        public static string DbProviderName
        {
            get { return dbProviderName; }
        }

    }
}
