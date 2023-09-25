using System;
using System.Configuration;

namespace BSSMessagingConsoleT24SVC
{
    class AppVars
    {
        #region Private Static
        private static string _T24DownloadFile, _T24DownloadFileDone, _T24LogPath, _T24Host, _T24UserName, _T24Password, _T24Directory, _T24DirectoryEmail, _T24DownloadFolder, _T24FolderBackup, _T24FolderError, _EmailAddress, _EmailDisplayName, _EmailPassword, _EmailIP, _EmailRunDate, _EmailRunStart, _EmailRunStop, _SMSIP, _EmailAddressIsError, _EmailAddressIsErrorDisplayName, _EmailDuration, _EmailCallType, _DWHPort, _T24Port, _FileCleansing;
        //private static bool _EmailIsActive, _SMSIsActive = true;
        private static string _DWHLogPath, _DWHhost, _DWHUsername, _DWHPassword, _DWHDirectory, _DWHDownloadFolder, _DWHFolderBackup, _DWHFolderError;
        private static int _EmailPort, _AlternativeEmailPort;
        private static string _AlternativeEmailAddress, _AlternativeEmailDisplayName, _AlternativeEmailUsername, _AlternativeEmailPassword, _AlternativeEmailIP, _AlternativeEmailDomain, _AlternativeEmailServer, _SMSPartnerId, _SMSPartnerName, _SMSPassword;
        private static bool _T24Service, _DWHService, _UsingAmountParameter;
        #endregion

        #region Public Static
        public static string T24LogPath
        {
            get { return _T24LogPath; }
        }

        public static string T24Host
        {
            get { return _T24Host; }
        }

        public static string T24UserName
        {
            get { return _T24UserName; }
        }

        public static string T24Password
        {
            get { return _T24Password; }
        }

        public static string T24Directory
        {
            get { return _T24Directory; }
        }

        public static string T24DirectoryEmail
        {
            get { return _T24DirectoryEmail; }
        }

        public static string T24DownloadFolder
        {
            get { return _T24DownloadFolder; }
        }

        public static string T24DownloadFile
        {
            get { return _T24DownloadFile; }
        }

        public static string T24DownloadFileDone
        {
            get { return _T24DownloadFileDone; }
        }

        public static string T24FolderBackup
        {
            get { return _T24FolderBackup; }
        }

        public static string T24FolderError
        {
            get { return _T24FolderError; }
        }

        public static string EmailAddress
        {
            get { return _EmailAddress; }
        }

        public static string EmailDisplayName
        {
            get { return _EmailDisplayName; }
        }

        public static string EmailPassword
        {
            get { return _EmailPassword; }
        }

        public static string EmailIP
        {
            get { return _EmailIP; }
        }

        public static int EmailPort
        {
            get { return _EmailPort; }
        }

        public static string EmailRunDate
        {
            get { return _EmailRunDate; }
        }

        public static string EmailRunStart
        {
            get { return _EmailRunStart; }
        }

        public static string EmailRunStop
        {
            get { return _EmailRunStop; }
        }

        public static string SMSIP
        {
            get { return _SMSIP; }
        }

        public static string DWHLogPath
        {
            get { return _DWHLogPath; }
        }

        public static string DWHhost
        {
            get { return _DWHhost; }
        }

        public static string DWHUsername
        {
            get { return _DWHUsername; }
        }

        public static string DWHPassword
        {
            get { return _DWHPassword; }
        }

        public static string DWHDirectory
        {
            get { return _DWHDirectory; }
        }

        public static string DWHDownloadFolder
        {
            get { return _DWHDownloadFolder; }
        }

        public static string DWHFolderBackup
        {
            get { return _DWHFolderBackup; }
        }

        public static string DWHFolderError
        {
            get { return _DWHFolderError; }
        }

        public static string EmailAddressIsError
        {
            get { return _EmailAddressIsError; }
        }

        public static string EmailAddressIsErrorDisplayName
        {
            get { return _EmailAddressIsErrorDisplayName; }
        }

        public static string EmailCallType
        {
            get { return _EmailCallType; }
        }

        public static string EmailDuration
        {
            get { return _EmailDuration; }
        }

        public static string DWHPort
        {
            get { return _DWHPort; }
        }

        public static string T24Port
        {
            get { return _T24Port; }
        }

        public static string FileCleansing
        {
            get { return _FileCleansing; }
        }

        public static string connstr
        {
            get { return decryptConnStr(_connstr); }
        }

        public static int dbtimeout
        {
            get { return _dbtimeout; }
        }

        public static int ThreadLoggingTime
        {
            get { return _threadLoggingTime; }
        }

        public static int ThreadSleepMilliSeconds
        {
            get { return _threadSleepMilliSeconds; }
        }

        public static string AlternativeEmailAddress
        {
            get { return _AlternativeEmailAddress; }
        }

        public static string AlternativeEmailDisplayName
        {
            get { return _AlternativeEmailDisplayName; }
        }

        public static string AlternativeEmailUsername
        {
            get { return _AlternativeEmailUsername; }
        }

        public static string AlternativeEmailPassword
        {
            get { return _AlternativeEmailPassword; }
        }

        public static string AlternativeEmailIP
        {
            get { return _AlternativeEmailIP; }
        }

        public static int AlternativeEmailPort
        {
            get { return _AlternativeEmailPort; }
        }

        public static string AlternativeEmailDomain
        {
            get { return _AlternativeEmailDomain; }
        }

        public static string AlternativeEmailServer
        {
            get { return _AlternativeEmailServer; }
        }

        public static bool T24Service
        {
            get { return _T24Service; }
        }

        public static bool DWHService
        {
            get { return _DWHService; }
        }

        public static string SMSPartnerId
        {
            get { return _SMSPartnerId; }
        }

        public static string SMSPartnerName
        {
            get { return _SMSPartnerName; }
        }

        public static string SMSPassword
        {
            get { return _SMSPassword; }
        }

        public static bool UsingAmountParameter
        {
            get { return _UsingAmountParameter; }
        }

        #endregion

        #region Tambahan
        private static bool _debugfile = false;
        private static bool _debugdb = false;
        private static bool _debugtrace = false;
        private static int _threadLoggingTime = 0;
        private static int _threadSleepMilliSeconds = 0;
        private static string _connstr;
        private static int _dbtimeout = 0;

        #endregion

        public static string decryptConnStr(string encryptedConnStr)
        {
            if (encryptedConnStr == null || encryptedConnStr.Trim() == "")
                return "";

            string connStr, encpwd, decpwd = "";
            int pos1, pos2;
            pos1 = encryptedConnStr.IndexOf("pwd=");
            pos2 = encryptedConnStr.IndexOf(";", pos1 + 4);
            encpwd = encryptedConnStr.Substring(pos1 + 4, pos2 - pos1 - 4);
            for (int i = 2; i < encpwd.Length; i++)
            {
                char chr = (char)(encpwd[i] - 2);
                decpwd += new string(chr, 1);
            }
            connStr = encryptedConnStr.Replace(encpwd, decpwd);
            return connStr;
        }


        static AppVars()
        {

            if (ConfigurationSettings.AppSettings["T24LogPath"] != null)
                _T24LogPath = ConfigurationSettings.AppSettings["T24LogPath"];

            if (ConfigurationSettings.AppSettings["T24Host"] != null)
                _T24Host = ConfigurationSettings.AppSettings["T24Host"];

            if (ConfigurationSettings.AppSettings["T24Username"] != null)
                _T24UserName = ConfigurationSettings.AppSettings["T24Username"];

            if (ConfigurationSettings.AppSettings["T24Password"] != null)
                _T24Password = ConfigurationSettings.AppSettings["T24Password"];

            if (ConfigurationSettings.AppSettings["T24Directory"] != null)
                _T24Directory = ConfigurationSettings.AppSettings["T24Directory"];

            if (ConfigurationSettings.AppSettings["T24DirectoryEmail"] != null)
                _T24DirectoryEmail = ConfigurationSettings.AppSettings["T24DirectoryEmail"];

            if (ConfigurationSettings.AppSettings["T24DownloadFolder"] != null)
                _T24DownloadFolder = ConfigurationSettings.AppSettings["T24DownloadFolder"];

            if (ConfigurationSettings.AppSettings["T24DownloadFile"] != null)
                _T24DownloadFile = ConfigurationSettings.AppSettings["T24DownloadFile"];

            if (ConfigurationSettings.AppSettings["T24DownloadFileDone"] != null)
                _T24DownloadFileDone = ConfigurationSettings.AppSettings["T24DownloadFileDone"];

            if (ConfigurationSettings.AppSettings["T24FolderBackup"] != null)
                _T24FolderBackup = ConfigurationSettings.AppSettings["T24FolderBackup"];

            if (ConfigurationSettings.AppSettings["T24FolderError"] != null)
                _T24FolderError = ConfigurationSettings.AppSettings["T24FolderError"];

            if (ConfigurationSettings.AppSettings["EmailAddress"] != null)
                _EmailAddress = ConfigurationSettings.AppSettings["EmailAddress"];

            if (ConfigurationSettings.AppSettings["EmailDisplayName"] != null)
                _EmailDisplayName = ConfigurationSettings.AppSettings["EmailDisplayName"];

            if (ConfigurationSettings.AppSettings["EmailPassword"] != null)
                _EmailPassword = ConfigurationSettings.AppSettings["EmailPassword"];

            if (ConfigurationSettings.AppSettings["EmailIP"] != null)
                _EmailIP = ConfigurationSettings.AppSettings["EmailIP"];

            if (ConfigurationSettings.AppSettings["EmailPort"] != null)
                _EmailPort = Convert.ToInt32(ConfigurationSettings.AppSettings["EmailPort"]);

            if (ConfigurationSettings.AppSettings["EmailRunDate"] != null)
                _EmailRunDate = ConfigurationSettings.AppSettings["EmailRunDate"];

            if (ConfigurationSettings.AppSettings["EmailRunStart"] != null)
                _EmailRunStart = ConfigurationSettings.AppSettings["EmailRunStart"];

            if (ConfigurationSettings.AppSettings["EmailRunStop"] != null)
                _EmailRunStop = ConfigurationSettings.AppSettings["EmailRunStop"];

            if (ConfigurationSettings.AppSettings["SMSIP"] != null)
                _SMSIP = ConfigurationSettings.AppSettings["SMSIP"];

            if (ConfigurationSettings.AppSettings["DWHLogPath"] != null)
                _DWHLogPath = ConfigurationSettings.AppSettings["DWHLogPath"];

            if (ConfigurationSettings.AppSettings["DWHHost"] != null)
                _DWHhost = ConfigurationSettings.AppSettings["DWHHost"];

            if (ConfigurationSettings.AppSettings["DWHUsername"] != null)
                _DWHUsername = ConfigurationSettings.AppSettings["DWHUsername"];

            if (ConfigurationSettings.AppSettings["DWHPassword"] != null)
                _DWHPassword = ConfigurationSettings.AppSettings["DWHPassword"];

            if (ConfigurationSettings.AppSettings["DWHDirectory"] != null)
                _DWHDirectory = ConfigurationSettings.AppSettings["DWHDirectory"];

            if (ConfigurationSettings.AppSettings["DWHDownloadFolder"] != null)
                _DWHDownloadFolder = ConfigurationSettings.AppSettings["DWHDownloadFolder"];

            if (ConfigurationSettings.AppSettings["DWHFolderBackup"] != null)
                _DWHFolderBackup = ConfigurationSettings.AppSettings["DWHFolderBackup"];

            if (ConfigurationSettings.AppSettings["DWHFolderError"] != null)
                _DWHFolderError = ConfigurationSettings.AppSettings["DWHFolderError"];

            if (ConfigurationSettings.AppSettings["EmailAddressIsError"] != null)
                _EmailAddressIsError = ConfigurationSettings.AppSettings["EmailAddressIsError"];

            if (ConfigurationSettings.AppSettings["EmailAddressIsErrorDisplayName"] != null)
                _EmailAddressIsErrorDisplayName = ConfigurationSettings.AppSettings["EmailAddressIsErrorDisplayName"];

            if (ConfigurationSettings.AppSettings["EmailDuration"] != null)
                _EmailDuration = ConfigurationSettings.AppSettings["EmailDuration"];

            if (ConfigurationSettings.AppSettings["EmailCallType"] != null)
                _EmailCallType = ConfigurationSettings.AppSettings["EmailCallType"];

            if (ConfigurationSettings.AppSettings["DWHPort"] != null)
                _DWHPort = ConfigurationSettings.AppSettings["DWHPort"];

            if (ConfigurationSettings.AppSettings["T24Port"] != null)
                _T24Port = ConfigurationSettings.AppSettings["T24Port"];

            if (ConfigurationSettings.AppSettings["FileCleansing"] != null)
                _FileCleansing = ConfigurationSettings.AppSettings["FileCleansing"];

            //if (ConfigurationSettings.AppSettings["EmailIsActive"] != null)
            //    _EmailIsActive = ConfigurationSettings.AppSettings["EmailIsActive"] == "on";

            //if (ConfigurationSettings.AppSettings["SMSIsActive"] != null)
            //    _SMSIsActive = ConfigurationSettings.AppSettings["SMSIsActive"] == "on";
            string s1_provider = "", s1_dbip = "", s1_dbname = "", s1_uid = "", s1_pwd = "", s1_othersetting = "";
            char[] separator = { ';' };

            //init log vars
            if (ConfigurationSettings.AppSettings["ThreadUpLoggingInMinutes"] != null)
                try
                {
                    _threadLoggingTime = int.Parse(ConfigurationSettings.AppSettings["ThreadUpLoggingInMinutes"]);
                }
                catch { }
            if (ConfigurationSettings.AppSettings["ThreadSleepTimeInMilliSeconds"] != null)
                try
                {
                    _threadSleepMilliSeconds = int.Parse(ConfigurationSettings.AppSettings["ThreadSleepTimeInMilliSeconds"]);
                }
                catch { }

            if (ConfigurationSettings.AppSettings["debugfile"] != null)
                _debugfile = ConfigurationSettings.AppSettings["debugfile"] == "on";
            if (ConfigurationSettings.AppSettings["debugdb"] != null)
                _debugdb = ConfigurationSettings.AppSettings["debugdb"] == "on";

            if (ConfigurationSettings.AppSettings["debugtrace"] != null)
                _debugtrace = ConfigurationSettings.AppSettings["debugtrace"] == "on";


            //init db vars
            if (ConfigurationSettings.AppSettings["dbtimeout"] != null)
                try
                {
                    _dbtimeout = int.Parse(ConfigurationSettings.AppSettings["dbtimeout"]);
                }
                catch { }

            if (ConfigurationSettings.AppSettings["db_provider"] != null)
                s1_provider = ConfigurationSettings.AppSettings["db_provider"];
            if (ConfigurationSettings.AppSettings["db_dbname"] != null)
                s1_dbname = ConfigurationSettings.AppSettings["db_dbname"];
            if (ConfigurationSettings.AppSettings["db_ip"] != null)
                s1_dbip = ConfigurationSettings.AppSettings["db_ip"];
            if (ConfigurationSettings.AppSettings["db_uid"] != null)
                s1_uid = ConfigurationSettings.AppSettings["db_uid"];
            if (ConfigurationSettings.AppSettings["db_pwd"] != null)
                s1_pwd = ConfigurationSettings.AppSettings["db_pwd"];
            if (ConfigurationSettings.AppSettings["db_othersetting"] != null)
                s1_othersetting = ConfigurationSettings.AppSettings["db_othersetting"];
            if (ConfigurationSettings.AppSettings["connstr"] != null)
                _connstr = ConfigurationSettings.AppSettings["connstr"];

            if ((s1_provider.ToUpper() == "SQLSERVER") && (_connstr == null || _connstr == "") && (s1_dbip != null && s1_dbip.Trim() != "" && s1_dbname != null && s1_dbname.Trim() != ""))
                _connstr = "Data Source=" + s1_dbip + ";Initial Catalog=" + s1_dbname + ";uid=" + s1_uid + ";pwd=" + s1_pwd + ";" + s1_othersetting;

            if (ConfigurationSettings.AppSettings["AlternativeEmailAddress"] != null)
                _AlternativeEmailAddress = ConfigurationSettings.AppSettings["AlternativeEmailAddress"];
            if (ConfigurationSettings.AppSettings["AlternativeEmailDisplayName"] != null)
                _AlternativeEmailDisplayName = ConfigurationSettings.AppSettings["AlternativeEmailDisplayName"];
            if (ConfigurationSettings.AppSettings["AlternativeEmailUsername"] != null)
                _AlternativeEmailUsername = ConfigurationSettings.AppSettings["AlternativeEmailUsername"];
            if (ConfigurationSettings.AppSettings["AlternativeEmailPassword"] != null)
                _AlternativeEmailPassword = ConfigurationSettings.AppSettings["AlternativeEmailPassword"];
            if (ConfigurationSettings.AppSettings["AlternativeEmailIP"] != null)
                _AlternativeEmailIP = ConfigurationSettings.AppSettings["AlternativeEmailIP"];
            if (ConfigurationSettings.AppSettings["AlternativeEmailPort"] != null)
                _AlternativeEmailPort = Convert.ToInt32(ConfigurationSettings.AppSettings["AlternativeEmailPort"]);
            if (ConfigurationSettings.AppSettings["AlternativeEmailDomain"] != null)
                _AlternativeEmailDomain = ConfigurationSettings.AppSettings["AlternativeEmailDomain"];
            if (ConfigurationSettings.AppSettings["AlternativeEmailServer"] != null)
                _AlternativeEmailServer = ConfigurationSettings.AppSettings["AlternativeEmailServer"];

            if (ConfigurationSettings.AppSettings["T24Service"] != null)
                _T24Service = ConfigurationSettings.AppSettings["T24Service"] == "on";

            if (ConfigurationSettings.AppSettings["DWHService"] != null)
                _DWHService = ConfigurationSettings.AppSettings["DWHService"] == "on";

            if (ConfigurationSettings.AppSettings["SMSPartnerId"] != null)
                _SMSPartnerId = ConfigurationSettings.AppSettings["SMSPartnerId"];

            if (ConfigurationSettings.AppSettings["SMSPartnerName"] != null)
                _SMSPartnerName = ConfigurationSettings.AppSettings["SMSPartnerName"];

            if (ConfigurationSettings.AppSettings["SMSPassword"] != null)
                _SMSPassword = ConfigurationSettings.AppSettings["SMSPassword"];

            if (ConfigurationSettings.AppSettings["UsingAmountParameter"] != null)
                _UsingAmountParameter = ConfigurationSettings.AppSettings["UsingAmountParameter"] == "on";
        }
    }
}
