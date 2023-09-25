using DMS.Tools;
using Microsoft.Exchange.WebServices.Data;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;

namespace BSSMessagingConsoleT24SVC
{
    public partial class Service1 : ServiceBase
    {
        Thread t1;
        Thread t2;
        Thread t3;
        Thread t4;

        public bool tRun = true;
        public bool t2Run = true;
        public bool t3Run = true;
        bool t4Run = true;
        bool isSendingT24 = false;
        bool isSendingDWH = false;
        bool isExecution = false;
        string durationT24, durationDWH, callTypeT24, callTypeDWH, dayExecution;
        private static string logT24 = AppVars.T24LogPath;
        private static string logDWH = AppVars.DWHLogPath;
        private static string moduleT24 = "T24";
        private static string moduleDWH = "DWH";
        public static string directoryEmail, directory, downloadFolder, folderError, folderBackup, folderLog, folder, downloadFolderT24, downloadFolderT24Done;
        private static string TransactionCodeT24 = "SELECT * FROM ParameterTransactionPendebetan WHERE TransactionCode = @1 AND IsActive = 1";
        private static string SQLMinimalTransaction = "SELECT * FROM ParameterTransaction WHERE NotificationType = @1 AND TransactionCode = @2";
        private static string SQLTransactionCodeDWH = "SELECT * FROM ParameterTransaction WHERE NotificationType = @1 AND IsActive = 1";

        public Service1()
        {
            InitializeComponent();
        }
        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                LogsInsert("masuk ke OnStart T24 ...", moduleT24);
                LogsInsert("masuk ke OnStart DWH ...", moduleDWH);
                t1 = new Thread(new ThreadStart(Thread1));
                t1.IsBackground = false;
                t1.Start();

                t2 = new Thread(new ThreadStart(Thread2));
                t2.IsBackground = false;
                t2.Start();

                t3 = new Thread(new ThreadStart(Thread3));
                t3.IsBackground = false;
                t3.Start();

                t4 = new Thread(new ThreadStart(Thread4));
                t4.IsBackground = false;
                t4.Start();
            }
            catch (Exception e)
            {
                LogsInsert("OnStart " + e.Message, moduleT24);
            }
            
            //GetFile();
            //Environment.Exit(0);
        }

        #region testing

        public void Start()
        {
            string hostT24 = AppVars.T24Host;
            string usernameT24 = AppVars.T24UserName;
            string passwordT24 = AppVars.T24Password;
            int portT24 = Convert.ToInt32(AppVars.T24Port);
            //GetFile(moduleDWH, hostT24, usernameT24, passwordT24, portT24);
            //GetFile(moduleT24, hostT24, usernameT24, passwordT24, portT24);
            //Thread1();
            Thread2();
            Thread3();
            //Thread4();
        }
        #endregion
        protected override void OnStop()
        {
            LogsInsert("Stopping services T24 ...", moduleT24);
            LogsInsert("Stopping services DWH ...", moduleDWH);
            //Environment.Exit(0);
        }

        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        protected void Thread1() // copy file from sftp t24 to local directory
        {

            while(tRun)
            {
                System.Threading.Thread.Sleep(50000);
                try
                {

                    LogsInsert("masuk ke Thread1 T24 ...", moduleT24);
                    LogsInsert("masuk ke Thread1 DWH ...", moduleDWH);
                    string hostT24 = AppVars.T24Host;
                    string usernameT24 = AppVars.T24UserName;
                    string passwordT24 = AppVars.T24Password;
                    int portT24 = Convert.ToInt32(AppVars.T24Port);

                    string hostDWH = AppVars.DWHhost;
                    string usernameDWH = AppVars.DWHUsername;
                    string passwordDWH = AppVars.DWHPassword;
                    int portDWH = Convert.ToInt32(AppVars.DWHPort);

                    if (AppVars.DWHService)
                    {
                        GetFile(moduleDWH, hostDWH, usernameDWH, passwordDWH, portDWH);
                    }

                    if (AppVars.T24Service)
                    {
                        GetFile(moduleT24, hostT24, usernameT24, passwordT24, portT24);
                    }

                    #region Email ke BSS Jika ada error
                    bool performT24 = false;
                    bool performDWH = false;

                    string daterun = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                    string now = DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00");

                    DateTime date1 = DateTime.Now.AddDays(-1);
                    string dateYesterday = date1.Year.ToString() + date1.Month.ToString("00") + date1.Day.ToString("00");

                    try
                    {
                        //dayRunDate = DateTime.Now.Day.ToString("00");
                        //if (string.IsNullOrEmpty(dayExecution))
                        //{
                        //    DateTime addDay1 = DateTime.Now.AddDays(1);
                        //    dayExecution = addDay1.Day.ToString("00");
                        //}

                        if (IsTodayExecution())
                        {
                            isSendingT24 = false;
                            isSendingDWH = false;
                            durationT24 = "1";
                            callTypeT24 = "1";
                            durationDWH = "1";
                            callTypeDWH = "1";
                            isExecution = true;
                        }
                        else
                        {
                            isExecution = false;
                        }

                        if (int.Parse(now.Replace(":", "")) == int.Parse(AppVars.EmailRunStart.Replace(":", "")) && (isSendingT24 == false) && (durationT24 == AppVars.EmailDuration) && (isExecution == true) && callTypeT24 == AppVars.EmailCallType)
                        {
                            performT24 = true;
                        }

                        if (int.Parse(now.Replace(":", "")) == int.Parse(AppVars.EmailRunStart.Replace(":", "")) && (isSendingDWH == false) && (durationDWH == AppVars.EmailDuration) && (isExecution == true) && callTypeDWH == AppVars.EmailCallType)
                        {
                            performDWH = true;
                        }

                        if (performT24)
                        {
                            PerformEmailSending(moduleT24);
                        }
                        if (performDWH)
                        {
                            PerformEmailSending(moduleDWH);
                        }

                        #region not used
                        //if (performT24)
                        //{
                        //    LogsInsert("=================================", moduleT24);
                        //    LogsInsert("=================================", moduleT24);

                        //    LogsInsert("=================================", moduleDWH);
                        //    LogsInsert("=================================", moduleDWH);

                        //    if (!Directory.Exists(AppVars.T24FolderError))
                        //    {
                        //        Directory.CreateDirectory(AppVars.T24FolderError);
                        //    }

                        //    if (!Directory.Exists(AppVars.DWHFolderError))
                        //    {
                        //        Directory.CreateDirectory(AppVars.DWHFolderError);
                        //    }

                        //    string[] fileNameT24 = Directory.GetFiles(AppVars.T24FolderError);
                        //    string[] fileNameDWH = Directory.GetFiles(AppVars.DWHFolderError);
                        //    string displayName = AppVars.EmailAddressIsErrorDisplayName;
                        //    string messageJoinT24 = string.Join(" \n", fileNameT24);
                        //    string messageJoinDWH = string.Join(" \n", fileNameDWH);
                        //    string messageBodyT24, messageBodyDWH;

                        //    if (IsDirectoryEmpty(AppVars.T24FolderError))
                        //    {
                        //        messageBodyT24 = "Tanggal " + dateYesterday + " tidak ada data yang error di folder SFTP T24 Error.";
                        //        LogsInsert("Tanggal " + dateYesterday + " tidak ada data yang error di folder SFTP T24 Error.", moduleT24);
                        //    }
                        //    else
                        //    {
                        //        messageBodyT24 = "Tanggal " + dateYesterday + " terdapat data error di folder SFTP T24 Error yang tidak berhasil terkirim yaitu : " + "<br><br>" + messageJoinT24;
                        //        LogsInsert("Tanggal " + dateYesterday + " terdapat data error di folder SFTP T24 Error.", moduleT24);
                        //    }

                        //    if (IsDirectoryEmpty(AppVars.DWHFolderError))
                        //    {
                        //        messageBodyDWH = "Tanggal " + dateYesterday + " tidak ada data yang error di folder SFTP DWH Error.";
                        //        LogsInsert("Tanggal " + dateYesterday + " tidak ada data yang error di folder SFTP DWH Error.", moduleDWH);
                        //    }
                        //    else
                        //    {
                        //        messageBodyDWH = "Tanggal " + dateYesterday + " terdapat data error di folder SFTP DWH Error yang tidak berhasil terkirim yaitu : " + "<br><br>" + messageJoinDWH;
                        //        LogsInsert("Tanggal " + dateYesterday + " terdapat data error di folder SFTP DWH Error.", moduleDWH);
                        //    }

                        //    string[] emailMessageT24 = new string[] { "777", "Folder Error per tanggal " + dateYesterday, "0", AppVars.EmailAddressIsError, messageBodyT24, displayName };

                        //    string[] emailMessageDWH = new string[] { "778", "Folder Error per tanggal " + dateYesterday, "0", AppVars.EmailAddressIsError, messageBodyDWH, displayName };

                        //    bool isEmailErrorT24 = Email(emailMessageT24, moduleT24);
                        //    bool isEmailErrorDWH = Email(emailMessageDWH, moduleDWH);

                        //    if (isEmailErrorT24)
                        //    {
                        //        LogsInsert("Email tidak berhasil terkirim. Silakan cek koneksi atau alamat email yang dituju.", moduleT24);
                        //        isSendingT24 = false;
                        //    }
                        //    else
                        //    {
                        //        LogsInsert("Email berhasil dikirim.", moduleT24);
                        //        isSendingT24 = true;
                        //        duration = "2";
                        //        callType = "2";
                        //        DateTime addDay = DateTime.Now.AddDays(1);
                        //        dayExecution = addDay.Day.ToString("00");
                        //        LogsInsert("Tanggal pengiriman email dari folder error berikutnya : " + dayExecution, moduleT24);
                        //    }

                        //    if (isEmailErrorDWH)
                        //    {
                        //        LogsInsert("Email tidak berhasil terkirim. Silakan cek koneksi atau alamat email yang dituju.", moduleDWH);
                        //        isSendingT24 = false;
                        //    }

                        //    else
                        //    {
                        //        LogsInsert("Email berhasil dikirim.", moduleDWH);
                        //        isSendingT24 = true;
                        //        duration = "2";
                        //        callType = "2";
                        //        DateTime addDay = DateTime.Now.AddDays(1);
                        //        dayExecution = addDay.Day.ToString("00");
                        //        LogsInsert("Tanggal pengiriman email dari folder error berikutnya : " + dayExecution, moduleDWH);
                        //    }

                        //    LogsInsert("=================================", moduleT24);
                        //    LogsInsert("=================================", moduleT24);

                        //    LogsInsert("=================================", moduleDWH);
                        //    LogsInsert("=================================", moduleDWH);

                        //}
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        LogsInsert("error: " + ex.Message.ToString(), moduleDWH);
                        LogsInsert("error: " + ex.Message.ToString(), moduleT24);
                        System.Threading.Thread.Sleep(60000);
                    }
                    #endregion

                    CleansingFolderError(AppVars.FileCleansing, AppVars.T24FolderError, moduleT24);
                    CleansingFolderError(AppVars.FileCleansing, AppVars.DWHFolderError, moduleDWH);
                    //Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    LogsInsert("error: " + ex.Message.ToString(), moduleT24);
                    LogsInsert("error: " + ex.Message.ToString(), moduleDWH);
                    System.Threading.Thread.Sleep(60000);
                }

                System.Threading.Thread.Sleep(30000);
            }
        }

        // ========= UPDATE BY FARDI FROM THIS
        protected void Thread2()
        {
            //while (t2Run)
            //{
                try
                {
                    LogsInsert("========================", moduleT24);
                    LogsInsert("masuk ke Thread2 T24 ...", moduleT24);
                    downloadFolderT24Done = AppVars.T24DownloadFileDone;
                    downloadFolderT24 = AppVars.T24DownloadFile;
                    folderError = AppVars.T24FolderError;
                    DirectoryInfo dir = new DirectoryInfo(downloadFolderT24);
                    var files = dir.GetFiles("*.csv");
                    if (files.Count() != 0)
                    {
                        foreach (var file in files)
                        {
                            string localFilename = downloadFolderT24 + "\\" + file.Name;
                            bool IsSuccess = LogsInsertPendebetan(File.OpenText(localFilename), moduleT24);

                            if (IsSuccess)
                            {
                                LogsInsert("Success insert LogPendebetan", moduleT24);
                                DirectoryInfo dirT24Done = new DirectoryInfo(downloadFolderT24Done);
                                if (!dirT24Done.Exists)
                                {
                                    dirT24Done.Create();
                                }
                                if (System.IO.File.Exists(dirT24Done + "\\" + file.Name))
                                {
                                    System.IO.File.Delete(dirT24Done + "\\" + file.Name);
                                }

                                File.Move(localFilename, dirT24Done + "\\" + file.Name);
                                LogsInsert("File Moved To : " + dirT24Done + "\\" + DateTime.Now.Millisecond.ToString() + file.Name, moduleT24);
                                LogsInsert("=================================", moduleT24);
                            }
                            else
                            {
                                LogsInsert("Failed insert LogPendebetan", moduleT24);
                                string destPath = folderError;
                                DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                                if (!dirBackUp.Exists)
                                {
                                    dirBackUp.Create();
                                }
                               
                                File.Move(localFilename, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                                LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, moduleT24);
                                LogsInsert("=================================", moduleT24);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogsInsert("Thread2 | " + e.Message, moduleT24);
                }

              //  System.Threading.Thread.Sleep(30000);
            //}
            
        }
        protected void Thread3()
        {
             //while (t3Run)
             //{
                try
                {
                    LogsInsert("========================", moduleT24);
                    LogsInsert("masuk ke Thread3 T24 ...", moduleT24);
                    using (DbConnection conn = new DbConnection(AppVars.connstr))
                    {
                        string No_LD = "";
                        string No_Rekening = "";
                        string No_Hp = "";
                        string Email = "";
                        string Nama = "";
                        string No_Rek = "";
                        string BodyEmail = "";
                        string BodySMS = "";
                        string NominalAngsuran = "";
                        string NominalBunga = "";
                        string Periode = "";
                        string Message = "";
                        string AngsuranKe = "";

                        bool isEmailSuccess = false;
                        bool isSMSSuccess = false;

                        string query = "Select Distinct No_LD, No_Rekening from LogPendebetan where Status = 1";
                        conn.ExecReader(query, null, AppVars.dbtimeout);
                        while (conn.hasRow())
                        {
                            No_LD = conn.GetFieldValue("No_LD");
                            No_Rekening = conn.GetFieldValue("No_Rekening");

                            using (DbConnection conn2 = new DbConnection(AppVars.connstr))
                            {
                                List<string> str = new List<string>();

                                object[] par = new object[] { No_Rekening, No_LD };
                                conn2.ExecReader("EXEC USP_CHECKDATAPENDEBETAN @1, @2", par, AppVars.dbtimeout);
                                while (conn2.hasRow())
                                {
                                    No_Hp = conn2.GetFieldValue("No_Hp");
                                    Email = conn2.GetFieldValue("Email");
                                    Nama = conn2.GetFieldValue("Nama");
                                    No_Rek = conn2.GetFieldValue("No_Rekening");
                                    BodyEmail = conn2.GetFieldValue("BodyEmail");
                                    BodySMS = conn2.GetFieldValue("BodySMS");
                                    NominalAngsuran = conn2.GetFieldValue("NominalAngsuran");
                                    NominalBunga = conn2.GetFieldValue("NominalBunga");
                                    Periode = conn2.GetFieldValue("Periode");
                                    Message = conn2.GetFieldValue("Message");
                                    AngsuranKe = conn2.GetFieldValue("AngsuranKe");

                                    str.Add(No_Hp);
                                    str.Add(Email);
                                    str.Add(Nama);
                                    str.Add(No_Rek);
                                    str.Add(BodyEmail);
                                    str.Add(BodySMS);
                                    str.Add(NominalAngsuran);
                                    str.Add(NominalBunga);
                                    str.Add(Periode);
                                    str.Add(Message);
                                    str.Add(No_LD);
                                    str.Add(AngsuranKe);

                                    string[] parameter = str.ToArray();

                                    isEmailSuccess = SendEmail(parameter);
                                    isSMSSuccess = SendSMS(parameter);

                                    if (isEmailSuccess && isSMSSuccess)
                                        LogsInsert("Email and SMS already sent successfully", moduleT24);
                                    else if (isEmailSuccess && !isSMSSuccess)
                                        LogsInsert("Email already sent but SMS failed to send", moduleT24);
                                    else if (!isEmailSuccess && isSMSSuccess)
                                        LogsInsert("Email failed to send but SMS already sent", moduleT24);
                                    else if (!isEmailSuccess && !isSMSSuccess)
                                        LogsInsert("Both Email and SMS failed to send", moduleT24);

                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogsInsert("Thread 3 | " + e.Message, moduleT24);
                }

               // System.Threading.Thread.Sleep(30000);
             //}
        }

        #region RESEND NOTIFICATIONS

        protected void Thread4()
        {
             while (t4Run)
             {
                 try
                 {
                    LogsInsert("Start Resend Notifications", moduleT24);

                     string No_LD = "";
                     string No_Rekening = "";
                     string Periode = "";
                     string No_Hp = "";
                     string Email = "";
                     string Nama = "";
                     string No_Rek = "";
                     string BodyEmail = "";
                     string BodySMS = "";
                     string NominalAngsuran = "";
                     string NominalBunga = "";
                     string Message = "";
                     string AngsuranKe = "";
                     int Flag;

                     bool ResendEmail, ResendSMS;

                     using (DbConnection conn = new DbConnection(AppVars.connstr))
                     {
                         List<string> str = new List<string>();
                         string query = "Select * from RFResendEmailSMSNotifikasi Where ResendNotifikasi = 0";
                         conn.ExecReader(query, null, AppVars.dbtimeout);
                         while (conn.hasRow())
                         {
                             No_LD = conn.GetFieldValue("No_LD");
                             No_Rekening = conn.GetFieldValue("No_Rekening");
                             Periode = conn.GetFieldValue("Periode");
                             Flag = Convert.ToInt32(conn.GetFieldValue("Flag"));

                             No_Hp = conn.GetFieldValue("No_Hp");
                             Email = conn.GetFieldValue("Email");
                             Nama = conn.GetFieldValue("Nama");
                             No_Rek = conn.GetFieldValue("No_Rekening");
                             BodyEmail = conn.GetFieldValue("Body_Email");
                             BodySMS = conn.GetFieldValue("Body_SMS");
                             NominalAngsuran = conn.GetFieldValue("Nominal_Angsuran");
                             NominalBunga = conn.GetFieldValue("Nominal_Bunga");
                             Periode = conn.GetFieldValue("Periode");
                             Message = conn.GetFieldValue("Message");
                             AngsuranKe = conn.GetFieldValue("AngsuranKe");

                             str.Add(No_Hp);
                             str.Add(Email);
                             str.Add(Nama);
                             str.Add(No_Rek);
                             str.Add(BodyEmail);
                             str.Add(BodySMS);
                             str.Add(NominalAngsuran);
                             str.Add(NominalBunga);
                             str.Add(Periode);
                             str.Add(Message);
                             str.Add(No_LD);
                             str.Add(AngsuranKe);

                             string[] parameter = str.ToArray();

                             if (Flag == 1) //Resend Email
                             {
                                 ResendEmail = SendEmail(parameter);
                                 if (ResendEmail)
                                     LogsInsert("Email already resend succesfully", moduleT24);
                                 else
                                     LogsInsert("Email failed to resend", moduleT24);
                             }
                             else if (Flag == 2) //Resend SMS
                             {
                                 ResendSMS = SendSMS(parameter);
                                 if (ResendSMS)
                                     LogsInsert("SMS already resend succesfully", moduleT24);
                                 else
                                     LogsInsert("SMS failed to resend", moduleT24);
                             }
                             else if (Flag == 3) //Both Resend Email and SMS
                             {
                                 ResendEmail = SendEmail(parameter);
                                 ResendSMS = SendSMS(parameter);

                                 if (ResendEmail && ResendSMS)
                                     LogsInsert("Email and SMS already resend successfully", moduleT24);
                                 else if (ResendEmail && !ResendSMS)
                                     LogsInsert("Email already resend but SMS failed to resend", moduleT24);
                                 else if (!ResendEmail && ResendSMS)
                                     LogsInsert("Email failed to resend but SMS already resend", moduleT24);
                                 else if (!ResendEmail && !ResendSMS)
                                     LogsInsert("Email and SMS failed to resend", moduleT24);
                             }

                             using (DbConnection conn2 = new DbConnection(AppVars.connstr))
                             {
                                 object[] par = new object[] { No_LD, No_Rekening, Periode, @Flag };
                                 conn2.ExecReader("EXEC USP_ExecuteResendNotification @1, @2, @3, @4", par, AppVars.dbtimeout);
                             }
                         }
                     }
                 }
                 catch (Exception e)
                 {
                     LogsInsert("Resend Email and SMS | " + e.Message.ToString(), moduleT24);
                 }

                 System.Threading.Thread.Sleep(30000);
             }
        }

        #endregion
        public static bool LogsInsertPendebetan(StreamReader reader, string module)
        {
            try
            {
                LogsInsert("Start Insert LogsPendebetan", moduleT24);

                bool value = true;
                string text;
                string transcationCode = "";
                string Periode = "";
                string NoHp = "";
                string Email = "";
                string Message = "";
                string Nama = "";
                string NoLD = "";
                string Nominal = "0";
                string NoRekening = "";
                bool Status = false;

                using (reader)
                {
                    while ((text = reader.ReadLine()) != null)
                    {
                        text = text.Replace(" | ", "|");
                        string[] parts = text.Replace("\r", string.Empty).Split('|', '\n');

                        transcationCode = parts[0];
                        Periode = parts[1];
                        NoHp = parts[2];
                        Email = parts[3];
                        Message = parts[4];
                        Nama = parts[5];
                        NoLD = parts[6];
                        Nominal = parts[7];
                        NoRekening = parts[8];
                        Status = true;

                        using (DbConnection conn2 = new DbConnection(AppVars.connstr))
                        {
                            try
                            {
                                object[] par = new object[10] { transcationCode, Periode, NoHp, Email, Message, Nama, NoLD, Nominal, NoRekening, Status };
                                conn2.ExecReader("EXEC USP_InsertLogPendebetan @1, @2, @3, @4, @5, @6, @7, @8, @9, @10", par, AppVars.dbtimeout);
                                if (conn2.hasRow())
                                {
                                    string message = conn2.GetFieldValue("Msg");
                                    if (message == "Success")
                                        value = true;
                                    else
                                        value = false;
                                }
                            }
                            catch (Exception e)
                            {
                                LogsInsert($"Failed While Insert into LogsPendebetan Table | {transcationCode} | {Periode} | {Nama} | {NoLD} | " + e.Message, moduleT24);
                            }
                        }
                    }
                }
                
                return value;
                
            }
            catch (Exception e)
            {
                LogsInsert("Error LogsInsertPendebetan | " + e.Message, moduleT24);
                return false;
            }
            

        }
        private static void LogInsertNotifEmailSMS(string[] parameter, bool? statusEmail, bool? statusSMS, string errorMessageEmail, string errorMessageSMS, string Flag)
        {
            LogsInsert("Start Insert LogInsertNotifEmailSMS", moduleT24);

            string No_Hp = parameter[0];
            string Email = parameter[1];
            string Nama = parameter[2];
            string No_Rek = parameter[3];
            string BodyEmail = parameter[4];
            string BodySMS = parameter[5];
            string NominalAngsuran = parameter[6];
            string NominalBunga = parameter[7];
            string Periode = parameter[8];
            string Message = parameter[9];
            string No_LD = parameter[10];
            string AngsuranKe = parameter[11];

            try
            {
                using (DbConnection conn = new DbConnection(AppVars.connstr))
                {
                    object[] par = new object[] {Periode, No_Hp, Email, Message, Nama, No_LD, NominalAngsuran, NominalBunga, No_Rek, BodySMS, BodyEmail, statusSMS, statusEmail, errorMessageSMS, errorMessageEmail, AngsuranKe, Flag};
                    conn.ExecReader("EXEC USP_LogSendEmailNotifSMS @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17", par, AppVars.dbtimeout);

                }

                LogsInsert("Success Insert LogInsertNotifEmailSMS", moduleT24);
            }
            catch (Exception e)
            {
                LogsInsert("Failed While Insert LogInsertNotifEmailSMS | " + e.Message, moduleT24);
            }
        }
        static bool SendEmail(string[] parameter)
        {
            bool isEmailSuccess = false;
            string messageHeader, messageBody;
            try
            {
                LogsInsert("****** Start Sending Email T24", moduleT24);
                LogsInsert("****** Processing Send Email to : " + parameter[1], moduleT24);

                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                string setUserName = AppVars.AlternativeEmailUsername;
                string setPassword = AppVars.AlternativeEmailPassword;
                string setUri = AppVars.AlternativeEmailServer;
                bool setUseDefaultCredentials = false;
                int setExchangeVersion = 0;


                var exchangeService = new ExchangeService((ExchangeVersion)setExchangeVersion)
                {
                    Url = new Uri(setUri),
                    //UseDefaultCredentials = setUseDefaultCredentials,
                    Credentials = new WebCredentials(setUserName, setPassword)
                };

                if (string.IsNullOrEmpty(parameter[1]) || string.IsNullOrWhiteSpace(parameter[1]))
                {
                    LogsInsert("****** Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung alamat email", moduleT24);
                    LogInsertNotifEmailSMS(parameter, false, null, "Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung alamat email", "", "Email");
                    isEmailSuccess = false;
                    return isEmailSuccess;
                }

                if (string.IsNullOrEmpty(parameter[4]) || string.IsNullOrWhiteSpace(parameter[4]))
                {
                    LogsInsert("****** Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung pesan", moduleT24);
                    LogInsertNotifEmailSMS(parameter, false, null, "Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung pesan", "", "Email");
                    isEmailSuccess = false;
                    return isEmailSuccess;
                }

                if (parameter[4].Contains("@"))
                {
                    LogsInsert("****** Data : " + parameter[1] + " Internal server error pada email message body", moduleT24);
                    LogInsertNotifEmailSMS(parameter, false, null, "Data : " + parameter[1] + " Internal server error pada email message body", "", "Email");
                    isEmailSuccess = false;
                    return isEmailSuccess;
                }

                var em = new EmailMessage(exchangeService);

                em.Body = parameter[4];
                em.Subject = "Notifikasi Pendebetan Angsuran PPM OLX Autos";
                //LogsInsert("Masuk 1 " + subjectEmail, module);
                em.From = new EmailAddress(AppVars.EmailAddress, AppVars.EmailDisplayName);
                //LogsInsert("Masuk 2 " + AppVars.EmailAddress +" - "+ AppVars.EmailDisplayName, module);
                em.ToRecipients.Add(new EmailAddress(parameter[1]));
                //em.ToRecipients.Add(new EmailAddress("rfardiansyahtanjung@gmail.com"));
                //LogsInsert("Masuk 3 "+ emailAddress + "  hrader " + messageHeader + " body : "+ messageBody, module);
                em.Send();

                LogsInsert("****** Email Send Successfully to : " + parameter[1], moduleT24);
                LogInsertNotifEmailSMS(parameter, true, null, "Email Sent", "", "Email");
                isEmailSuccess = true;
                return isEmailSuccess;

            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    LogsInsert("****** Fail Send Email to : " + parameter[1] + e, moduleT24);
                }
                LogsInsert("****** Error Email : " + e.Message.ToString(), moduleT24);
                LogInsertNotifEmailSMS(parameter, false, null, "Fail Send Email to : " + parameter[1] + e.Message.ToString(), "", "Email");
                isEmailSuccess = false;
                return isEmailSuccess;
            }

        }
        static bool SendSMS(string[] parameter)
        {
            string numberSMS, SMSBody;
            bool isSMSSuccess = false;
            try
            {
                LogsInsert("****** Start Sending SMS", moduleT24);
                LogsInsert("Processing Send SMS Number " + parameter[0], moduleT24);

                var smsAlternatif = ConfigurationManager.AppSettings["ServiceSMSEmailAlternative"] != null ? ConfigurationManager.AppSettings["ServiceSMSEmailAlternative"] : "";

                /*if (smsAlternatif == "on")
                    return SendSMSAlternative(parameter, moduleT24);*/

                numberSMS = parameter[0];
                SMSBody = SecurityElement.Escape(parameter[5]);
                double v;
                bool isNumber = double.TryParse(numberSMS, NumberStyles.Any, CultureInfo.InvariantCulture, out v);

                if (!isNumber || (numberSMS.Length < 6))
                {
                    LogsInsert("****** Nomor SMS tidak valid. ", moduleT24);
                    LogInsertNotifEmailSMS(parameter, null, false, "", "Nomor SMS tidak valid. ", "SMS");
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }

                if (SMSBody.Length == 0)
                {
                    LogsInsert("****** SMS tidak mengandung pesan. ", moduleT24);
                    LogInsertNotifEmailSMS(parameter, null, false, "", "SMS tidak mengandung pesan. ", "SMS");
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }

                if (SMSBody.Contains("@"))
                {
                    LogsInsert("****** Internal server error pada SMS message body ", moduleT24);
                    LogInsertNotifEmailSMS(parameter, null, false, "", "Internal server error pada SMS message body. ", "SMS");
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }

                XmlDocument xmlDoc = new XmlDocument();
                //Create an XML declaration. 
                XmlDeclaration xmldecl;
                xmldecl = xmlDoc.CreateXmlDeclaration("1.0", null, null);
                xmldecl.Encoding = "UTF-8";
                XmlElement root = xmlDoc.DocumentElement;
                xmlDoc.InsertBefore(xmldecl, root);

                XmlNode rootNode = xmlDoc.CreateElement("smsbc");
                xmlDoc.AppendChild(rootNode);
                XmlNode requestNode = xmlDoc.CreateElement("request");
                rootNode.AppendChild(requestNode);
                XmlNode datetimeNode = xmlDoc.CreateElement("datetime");
                datetimeNode.InnerText = DateTime.Now.ToString("MMddHHmmss");
                requestNode.AppendChild(datetimeNode);
                XmlNode rrnNode = xmlDoc.CreateElement("rrn");
                rrnNode.InnerText = DateTime.Now.ToString("MMddHHmmssfff"); //parameter[0];
                requestNode.AppendChild(rrnNode);
                XmlNode partnerIdNode = xmlDoc.CreateElement("partnerId");
                partnerIdNode.InnerText = AppVars.SMSPartnerId;
                requestNode.AppendChild(partnerIdNode);
                XmlNode partnerNameNode = xmlDoc.CreateElement("partnerName");
                partnerNameNode.InnerText = AppVars.SMSPartnerName;
                requestNode.AppendChild(partnerNameNode);
                XmlNode passwordNode = xmlDoc.CreateElement("password");
                passwordNode.InnerText = AppVars.SMSPassword;
                requestNode.AppendChild(passwordNode);
                XmlNode destinationNumberNode = xmlDoc.CreateElement("destinationNumber");
                destinationNumberNode.InnerText = numberSMS;
                requestNode.AppendChild(destinationNumberNode);
                XmlNode messageNode = xmlDoc.CreateElement("message");
                messageNode.InnerText = SMSBody;
                requestNode.AppendChild(messageNode);
                rootNode.AppendChild(requestNode);

                if (PostXMLTransaction(AppVars.SMSIP, xmlDoc, moduleT24) == null)
                {
                    LogsInsert("****** SMS CONNECTION FAILED", moduleT24);
                    LogInsertNotifEmailSMS(parameter, null, false, "", "SMS connection failed ", "SMS");
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }

                //PostXMLTransaction(AppVars.SMSIP, xmlDoc, module);

                xmlDoc.Save("test-doc.xml");   //Part Send SMS


                // using (var client = new WebClient())
                //{

                //  var responseString = client.DownloadString("http://128.199.128.171:8080/sms/sendSms?number=" + parameter[2] + "&message=" + parameter[4] + "&type=B");
                LogsInsert("****** SMS Send Successfully to : " + parameter[0], moduleT24);
                LogInsertNotifEmailSMS(parameter, null, true, "", "SMS Sent", "SMS");
                isSMSSuccess = true;
                return isSMSSuccess;
                //}
            }
            catch (Exception e)
            {
                LogsInsert("****** Fail Send SMS to : " + parameter[0] + " | " + e.Message.ToString(), moduleT24);
                LogInsertNotifEmailSMS(parameter, null, false, "", "Fail Send SMS to : " + parameter[0] + " | " + e.Message.ToString(), "SMS");
                isSMSSuccess = false;
                return isSMSSuccess;
            }

        }
        static bool SendSMSAlternative(string[] parameter, string module)
        {
            bool isSMSSuccess = false;
            string numberSMS, SMSBody, TransactionCode, NoRekening;

            var kodesmsolx = ConfigurationManager.AppSettings["KODESMSOLX"];
            //DbConnection conn = new DbConnection(AppVars.connstr);
            try
            {
                LogsInsert("****** Start Sending SMS", module);
                /*NEW IF 20220215**/

                /*TransactionCode = parameter[0];
                NoRekening = parameter[8];*/

                numberSMS = parameter[0];
                SMSBody = SecurityElement.Escape(parameter[5]);


                double v;
                bool isNumber = double.TryParse(numberSMS, NumberStyles.Any, CultureInfo.InvariantCulture, out v);

                if (!isNumber || (numberSMS.Length < 6))
                {
                    LogsInsert("****** Nomor SMS tidak valid. ", module);
                    LogsInsertLogEmailSMS(parameter, module, "SMS", "Nomor SMS tidak valid.", "0");
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }

                if (SMSBody.Length == 0)
                {
                    LogsInsert("****** SMS tidak mengandung pesan. ", module);
                    LogsInsertLogEmailSMS(parameter, module, "SMS", "SMS tidak mengandung pesan. ", "0");
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }

                if (SMSBody.Contains("@"))
                {
                    LogsInsert("****** Internal server error pada SMS message body ", moduleT24);
                    LogInsertNotifEmailSMS(parameter, null, false, "", "Internal server error pada SMS message body. ", "SMS");
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }

                var UrlAPISMSAlternative = ConfigurationManager.AppSettings["UrlAPISMSAlternative"];
                var FunctionAPISMSAlternative = ConfigurationManager.AppSettings["FunctionAPISMSAlternative"];
                var UrlAPISMSAlternativeUsername = ConfigurationManager.AppSettings["UrlAPISMSAlternativeUsername"];
                var UrlAPISMSAlternativePassword = ConfigurationManager.AppSettings["UrlAPISMSAlternativePassword"];
                var UrlAPISMSAlternativeSenderID = ConfigurationManager.AppSettings["UrlAPISMSAlternativeSenderID"];
                var client = new RestClient(UrlAPISMSAlternative +
                    FunctionAPISMSAlternative.Replace("[usernamegratika]",
                    UrlAPISMSAlternativeUsername).Replace("[passwordgratika]",
                    UrlAPISMSAlternativePassword).Replace("[senderidgratika]",
                    UrlAPISMSAlternativeSenderID).Replace("[messagebody]", SMSBody).Replace("[nomorhp]", numberSMS));

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client.ClearHandlers();
                client.AddHandler("*", new JsonDeserializer());

                var request = new RestRequest(Method.GET);
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "text/html");

                var response = client.Execute(request);

                LogsInsert("response : " + response.Content, module);
                if (response.StatusCode.ToString() != "OK")
                {
                    LogsInsert("****** SMS CONNECTION FAILED", module);
                    isSMSSuccess = false;
                    return isSMSSuccess;
                }


                //  var responseString = client.DownloadString("http://128.199.128.171:8080/sms/sendSms?number=" + parameter[2] + "&message=" + parameter[4] + "&type=B");
                LogsInsert("****** SMS Sent", moduleT24);
                LogInsertNotifEmailSMS(parameter, null, true, "", "SMS Sent", "SMS");
                isSMSSuccess = true;
                return isSMSSuccess;
                //}
            }
            catch (Exception e)
            {
                LogsInsert("****** Error SMS : " + e.Message.ToString(), moduleT24);
                LogInsertNotifEmailSMS(parameter, null, false, "", "Error SMS: " + e.Message.ToString(), "SMS");
                isSMSSuccess = false;
                return isSMSSuccess;
            }
        }

        // ========================== TO THIS

        #region log
        //Tayo Penambahan loh sms email ke Database
        //Transaction Code | Tgl Generate | No Telp | Email | Body | Name | Minimum Transaction
        private static void LogsInsertLogEmailSMS(string[] parameter, string type, string notif, string errorMessage, string status, bool emailTerpisah = false)
        {
            string periode = "";
            string transactionCode = "";
            string nominal = "0";
            string rekening = "";
            string message = "";
            string noHP = "";
            string email = "";
            string messageHeader = "";
            string messageBody = "";
            try
            {
                if (notif == "SMS")
                {

                    if (type == "T24")
                    {
                        periode = parameter[1];
                        transactionCode = parameter[0];
                        noHP = parameter[2];
                        message = SecurityElement.Escape(parameter[4]);

                        var transaksi = parameter[7];
                        nominal = transaksi;
                        email = parameter[3];
                        rekening = parameter[8];
                    }
                    else
                    {
                        periode = parameter[0];

                        using (DbConnection conn = new DbConnection(AppVars.connstr))
                        {
                            object[] par = new object[1] { type };
                            //string transactionCode;

                            conn.ExecReader(SQLTransactionCodeDWH, par, AppVars.dbtimeout);
                            while (conn.hasRow())
                            {
                                if (parameter[3].ToLower().Contains(conn.GetFieldValue("TransactionCode").ToLower()))
                                {
                                    transactionCode = conn.GetFieldValue("TransactionCode");
                                }
                            }
                        }

                        noHP = parameter[1];
                        message = SecurityElement.Escape(parameter[3]);

                        var transaksi = parameter[5];
                        transaksi = transaksi.Replace(",", "").Replace("Rp.", "");
                        if (transaksi.Contains("."))
                        {
                            transaksi = transaksi.Replace(".", ",");
                        }
                        nominal = transaksi;
                        email = parameter[2];
                        rekening = parameter[6];

                    }

                }
                else if (notif == "EMAIL")
                {

                    if (type == "T24")
                    {
                        periode = parameter[1];
                        transactionCode = parameter[0];
                        noHP = parameter[2];
                        messageHeader = parameter[5];
                        messageBody = parameter[4];
                        message = "Header : " + messageHeader + ", Body : " + messageBody;
                        email = parameter[3];

                        if (!emailTerpisah)
                        {
                            var transaksi = parameter[7];
                            rekening = parameter[8];
                            nominal = transaksi;

                        }
                        else
                        {
                            email = parameter[0];
                            object[] par = new object[1] { type };
                            //string transactionCode;

                            using (DbConnection conn = new DbConnection(AppVars.connstr))
                            {
                                conn.ExecReader(SQLTransactionCodeDWH, par, AppVars.dbtimeout);
                                if (conn.hasRow())
                                {
                                    if (parameter[3].ToLower().Contains(conn.GetFieldValue("TransactionCode").ToLower()))
                                    {
                                        transactionCode = conn.GetFieldValue("TransactionCode");
                                    }
                                    else
                                    {
                                        transactionCode = "";
                                    }
                                }
                                else
                                {
                                    transactionCode = "";
                                }
                            }
                        }

                    }
                    else
                    {
                        periode = parameter[0];

                        using (DbConnection conn = new DbConnection(AppVars.connstr))
                        {
                            object[] par = new object[1] { type };
                            //string transactionCode;

                            conn.ExecReader(SQLTransactionCodeDWH, par, AppVars.dbtimeout);
                            while (conn.hasRow())
                            {
                                if (parameter[3].ToLower().Contains(conn.GetFieldValue("TransactionCode").ToLower()))
                                {
                                    transactionCode = conn.GetFieldValue("TransactionCode");
                                }
                            }
                        }

                        noHP = parameter[1];
                        messageHeader = parameter[4];
                        messageBody = parameter[3];
                        message = "Header : " + messageHeader + ", Body : " + messageBody;

                        var transaksi = parameter[5];
                        transaksi = transaksi.Replace(",", "").Replace("Rp.", "");
                        if (transaksi.Contains("."))
                        {
                            transaksi = transaksi.Replace(".", ",");
                        }
                        nominal = transaksi;
                        email = parameter[2];
                        rekening = parameter[6];
                    }
                }
                else
                {
                    if (type == "T24")
                    {
                        periode = parameter[1];
                        transactionCode = parameter[0];
                        noHP = parameter[2];
                        email = parameter[3];
                        rekening = parameter[8];
                    }
                    else
                    {
                        periode = parameter[0];

                        using (DbConnection conn = new DbConnection(AppVars.connstr))
                        {
                            object[] par = new object[1] { type };
                            //string transactionCode;

                            conn.ExecReader(SQLTransactionCodeDWH, par, AppVars.dbtimeout);
                            while (conn.hasRow())
                            {
                                if (parameter[3].ToLower().Contains(conn.GetFieldValue("TransactionCode").ToLower()))
                                {
                                    transactionCode = conn.GetFieldValue("TransactionCode");
                                }
                            }
                        }

                        noHP = parameter[1];
                        email = parameter[2];
                        rekening = parameter[6];
                    }
                }
                using (DbConnection conn = new DbConnection(AppVars.connstr))
                {
                    object[] par = new object[11] { periode, type, transactionCode, notif, nominal, rekening, message, status, errorMessage, noHP, email };
                    conn.ExecReader("EXEC USP_InsertLogServices @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11", par, AppVars.dbtimeout);

                }
            }
            catch (Exception ex)
            {

                LogsInsert(ex.Message, type);
            }
        }

        private static void LogsInsert(string message, string module)
        {
            string date = folderName();
            if (module == "T24")
            {
                folder = createDirectory(logT24, DateTime.Today);
            }
            else if (module == "DWH")
            {
                folder = createDirectory(logDWH, DateTime.Today);
            }
            else
            {
                return;
            }

            if (!File.Exists(folder + @"\" + date + ".txt"))
            {
                File.Create(folder + @"\" + date + ".txt").Close();
            }
            try
            {
                using (StreamWriter sw = File.AppendText(folder + @"\" + date + ".txt"))
                {
                    sw.WriteLine(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + DateTime.Now.ToString("HH:mm:ss") + " " + message);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The process cannot access the file"))
                {
                    bool isInUse = true;
                    while (isInUse == true)
                    {
                        try
                        {
                            using (StreamWriter sw = File.AppendText(folder + @"\" + date + ".txt"))
                            {
                                sw.WriteLine(date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + DateTime.Now.ToString("HH:mm:ss") + " " + message);
                            }
                            isInUse = false;
                        }
                        catch
                        { }
                    }
                }
            }
        }
        protected void LogsError(string message)
        {
            try
            {

                string date = folderName();
                if (!Directory.Exists(logT24 + @"\error\" + date)) { Directory.CreateDirectory(logT24 + @"\error\" + date); }
                if (!File.Exists(logT24 + @"\error\" + date + ".txt")) { File.Create(logT24 + @"\error\" + date + ".txt").Close(); }
                //File.AppendAllText(logs + @"\" + date + @"\" + date + ".txt",  date + DateTime.Now.ToString("hh:mm:ss") + " " + message);
                using (StreamWriter sw = File.AppendText(logT24 + @"\error\" + date + ".txt"))
                {
                    sw.WriteLine(date + DateTime.Now.ToString("HHmmss") + "   " + message + "   ");
                }
            }
            catch
            { }

        }
        protected static string folderName()
        {
            string date = DateTime.Today.Year.ToString();
            int month = DateTime.Today.Month;
            if (month < 10) { date = date + "0" + month.ToString(); } else { date = date + month.ToString(); }
            int day = DateTime.Today.Day;
            if (day < 10) { date = date + "0" + day.ToString(); } else { date = date + day.ToString(); }
            return date;
        }

        protected static string createDirectory(string folder, DateTime date)
        {
            var tgl = date;
            var tahun = tgl.Year.ToString();
            var bulan = tgl.ToString("MMMM", CultureInfo.InvariantCulture);
            if (bulan.Length < 2) { bulan = "0" + bulan; }
            var hari = tgl.Day.ToString();
            if (hari.Length < 2) { hari = "0" + hari; }
            folder = folder + @"\" + tahun + @"\" + bulan + @"\" + hari;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        #endregion

        protected bool IsTodayExecution()
        {
            string dayRunDate = DateTime.Now.Day.ToString("00");
            if (string.IsNullOrEmpty(dayExecution))
            {
                DateTime addDay1 = DateTime.Now.AddDays(1);
                dayExecution = addDay1.Day.ToString("00");
            }

            if (dayRunDate == dayExecution)
            {
                return true;
            }

            return false;

        }

        protected void PerformEmailSending(string module)
        {
            try
            {
                DateTime date1 = DateTime.Now.AddDays(-1);
                string dateYesterday = date1.Year.ToString() + date1.Month.ToString("00") + date1.Day.ToString("00");
                LogsInsert("=================================", module);
                LogsInsert("=================================", module);

                if (module == moduleT24)
                {
                    folderError = AppVars.T24FolderError;
                }
                else
                {
                    folderError = AppVars.DWHFolderError;
                }

                if (!Directory.Exists(folderError))
                {
                    Directory.CreateDirectory(folderError);
                }

                string[] fileName = Directory.GetFiles(folderError);
                string displayName = AppVars.EmailAddressIsErrorDisplayName;
                string messageJoin = string.Join(" \n ", fileName);
                string messageBody;

                if (IsDirectoryEmpty(folderError))
                {
                    messageBody = "Tanggal " + dateYesterday + " tidak ada data yang error di folder SFTP " + module + " Error.";
                    LogsInsert("Tanggal " + dateYesterday + " tidak ada data yang error di folder SFTP " + module + " Error.", module);
                }
                else
                {
                    messageBody = "Tanggal " + dateYesterday + " terdapat data error di folder SFTP " + module + " Error yang tidak berhasil terkirim yaitu : " + "<br><br>" + messageJoin;
                    LogsInsert("Tanggal " + dateYesterday + " terdapat data error di folder SFTP " + module + " Error.", module);
                }

                string[] emailMessage;
                if (module == moduleT24)
                {
                    emailMessage = new string[] { "777", "Folder Error per tanggal " + dateYesterday, "0", AppVars.EmailAddressIsError, messageBody, displayName };
                }
                else
                {
                    emailMessage = new string[] { "888", "Folder Error per tanggal " + dateYesterday, AppVars.EmailAddressIsError, messageBody, displayName };
                }

                bool isEmailError = true;
                if (IsDirectoryEmpty(folderError))
                {
                    DateTime addDay = DateTime.Now.AddDays(1);
                    dayExecution = addDay.Day.ToString("00");
                    return;
                }
                else
                {
                    isEmailError = Email(emailMessage, module);
                }


                if (isEmailError)
                {
                    LogsInsert("Email tidak berhasil terkirim. Silakan cek koneksi atau alamat email yang dituju.", module);
                    LogsInsert("=================================", module);
                    LogsInsert("=================================", module);
                }
                else
                {
                    LogsInsert("Email berhasil dikirim.", module);
                    if (module == moduleT24)
                    {
                        isSendingT24 = true;
                        durationT24 = "2";
                        callTypeT24 = "2";
                    }
                    else
                    {
                        isSendingDWH = true;
                        durationDWH = "2";
                        callTypeDWH = "2";
                    }

                    DateTime addDay = DateTime.Now.AddDays(1);
                    dayExecution = addDay.Day.ToString("00");
                    LogsInsert("Tanggal pengiriman email dari folder error berikutnya : " + dayExecution, module);
                    LogsInsert("=================================", module);
                    LogsInsert("=================================", module);
                }
            }
            catch (Exception ex)
            {
                LogsInsert("****** Error Sending Email Harian :  " + ex.Message.ToString(), module);
                return;
            }

        }

        protected void CleansingFolderError(string dayKeep, string folder, string modul)
        {
            int day = Convert.ToInt32(dayKeep);

            if (IsDirectoryEmpty(folder))
            {
                return;
            }
            else
            {
                DirectoryInfo d = new DirectoryInfo(folder);

                var files = d.GetFiles("*.csv");
                foreach (var file in files)
                {
                    DateTime cleansing = file.CreationTime.Date.AddDays(day);
                    string localFileName = folder + "\\" + file.Name;
                    if (file.Name.StartsWith(".") && (cleansing == DateTime.Today))
                    {
                        file.Delete();
                        LogsInsert("File : " + file.Name + " sudah terhapus dari direktory folder error.", modul);
                    }
                }
            }
        }

        static void GetFile(string module, string hostModule, string usernameModule, string passwordModule, int portModule)
        {
            bool isError = false;
            try
            {
                #region Ori GET FILE
                string host = hostModule;
                string username = usernameModule;
                string password = passwordModule;
                int port = portModule;

                if (module == "T24")
                {
                    directoryEmail = AppVars.T24DirectoryEmail;
                    directory = AppVars.T24Directory;
                    downloadFolder = AppVars.T24DownloadFolder;
                    folderError = AppVars.T24FolderError;
                    folderBackup = AppVars.T24FolderBackup;
                    downloadFolderT24 = AppVars.T24DownloadFile;
                }
                else if (module == "DWH")
                {
                    directory = AppVars.DWHDirectory;
                    downloadFolder = AppVars.DWHDownloadFolder;
                    folderError = AppVars.DWHFolderError;
                    folderBackup = AppVars.DWHFolderBackup;
                    checkfolder(module, hostModule, usernameModule, passwordModule, portModule);
                }
                //send email T24
                //Update by wicaq 200221
                if (module == "T24")
                {
                    using (var sftp = new SftpClient(host, port, username, password))
                    {
                        //sftp.Connect();
                        //sftp.ChangeDirectory(directoryEmail + DateTime.Now.ToString("yyyyMMdd"));
                        //LogsInsert("Connected to " + host + " as " + username, module);
                        //DownloadDirectory(sftp, sftp.WorkingDirectory, downloadFolder, module);

                        DirectoryInfo d = new DirectoryInfo(downloadFolder);

                        var files = d.GetFiles("*.csv");


                        foreach (var file in files)
                        {
                            if (file.Name.ToUpper().StartsWith("EMAIL") && (file.CreationTime.Date == DateTime.Today) && module == moduleT24)
                            {
                                string localFilePath = downloadFolder;
                                string localFileName = downloadFolder + "\\" + file.Name;
                                LogsInsert("=================================", module);
                                LogsInsert("Processing " + file.Name, module);

                                string[] lines = System.IO.File.ReadAllLines(localFileName);

                                isError = ParseEmail(File.OpenText(localFileName), file.Name, module);

                                if (isError)
                                {
                                    string destPath = folderError;
                                    DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                                    if (!dirBackUp.Exists)
                                    {
                                        dirBackUp.Create();
                                    }
                                    if (module == moduleT24)
                                    {
                                        File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                                    }

                                    LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                                }
                                else
                                {
                                    string destPath = folderBackup;
                                    DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                                    if (!dirBackUp.Exists)
                                    {
                                        dirBackUp.Create();
                                    }
                                    File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                                    LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                                }
                                File.Delete(localFileName);
                                LogsInsert("=================================", module);
                            }

                            //================ UPDATE BY FARDI FROM THIS 

                            else if (file.Name.ToUpper().StartsWith("T24") && (file.CreationTime.Date == DateTime.Today) && module == moduleT24)
                            {
                                try
                                {
                                    LogsInsert("========================= T24 420 and 434", module);
                                    //LogsInsert("====== Enter T24 Condition", module);
                                    string text = "";
                                    string localFileName = downloadFolder + "\\" + file.Name;
                                    string destPathT24 = downloadFolderT24;
                                   
                                    string transactionCode = file.Name.Split('-', ' ')[1];

                                    using (DbConnection conn = new DbConnection(AppVars.connstr))
                                    {
                                        object[] par = new object[1] { transactionCode };

                                        conn.ExecReader(TransactionCodeT24, par, AppVars.dbtimeout);
                                        if (conn.hasRow())
                                        {
                                            //LogsInsert("====== Enter hasRow Condition", module);
                                            DirectoryInfo dirT24 = new DirectoryInfo(destPathT24);
                                            if (!dirT24.Exists)
                                            {
                                                dirT24.Create();
                                            }

                                            DirectoryInfo dir = new DirectoryInfo(downloadFolderT24);

                                            var fileExisting = dir.GetFiles("*.csv");
                                            foreach (var exist in fileExisting)
                                            {
                                                if (exist.ToString() == file.Name)
                                                {
                                                    File.Delete(destPathT24 + "\\" + exist);
                                                }
                                            }

                                            File.Move(localFileName, dirT24 + "\\" + file.Name);
                                            LogsInsert("File Moved To : " + dirT24 + "\\" + DateTime.Now.Millisecond.ToString() + file.Name, module);
                                            LogsInsert("=================================", module);
                                        }
                                    }
                                        
                                }
                                catch (Exception e)
                                {
                                    LogsInsert("Failed to move file T24 TC 420 or 434 " + e.Message, moduleT24);
                                }
                            }
                            //================================== TO THIS
                        }
                    }
                }

                using (var sftp = new SftpClient(host, port, username, password))
                {
                    try
                    {

                        //sftp.Connect();
                        //sftp.ChangeDirectory(directory + DateTime.Now.ToString("yyyyMMdd"));
                        //LogsInsert("Connected to DWH" + host + " as " + username, module);
                        //DownloadDirectory(sftp, sftp.WorkingDirectory, downloadFolder, module);

                        DirectoryInfo d = new DirectoryInfo(downloadFolder);

                        var files = d.GetFiles("*.csv");
                        bool perform = false;
                        foreach (var file in files)
                        {
                            if (!file.Name.ToUpper().StartsWith("EMAIL") && !file.Name.StartsWith(".") && (file.CreationTime.Date == DateTime.Today) && module == moduleT24)
                            {

                                string transactionCode = file.Name.Split('-', ' ')[1];
                                using (DbConnection conn = new DbConnection(AppVars.connstr))
                                {
                                    object[] par = new object[] { transactionCode };
                                    conn.ExecReader(TransactionCodeT24, par, AppVars.dbtimeout);
                                    if (conn.hasRow())
                                        perform = false;
                                    else
                                        perform = true;
                                }
                                   
                            }
                            else if (module == moduleDWH)
                            {
                                perform = true;
                            }
                            else
                            {
                                perform = false;
                            }

                            if (perform)
                            {
                                string localFilePath = downloadFolder;
                                string localFileName = downloadFolder + "\\" + file.Name;
                                LogsInsert("=================================", module);
                                LogsInsert("Processing " + file.Name, module);

                                string[] lines = System.IO.File.ReadAllLines(localFileName);

                                if ((lines.Count() > 1) && module == "T24")
                                {
                                    LogsInsert("Data : " + localFileName + " tidak di proses karena tidak valid. Data mengandung lebih dari 1 line.", module);
                                    isError = true;
                                }
                                else
                                {
                                    isError = Parse(File.OpenText(localFileName), file.Name, module);
                                }

                                if (isError)
                                {
                                    string destPath = folderError;
                                    DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                                    if (!dirBackUp.Exists)
                                    {
                                        dirBackUp.Create();
                                    }
                                    if (module == moduleT24)
                                    {
                                        File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                                    }

                                    LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                                }
                                else
                                {
                                    string destPath = folderBackup;
                                    DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                                    if (!dirBackUp.Exists)
                                    {
                                        dirBackUp.Create();
                                    }
                                    File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                                    LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                                }
                                File.Delete(localFileName);
                                LogsInsert("=================================", module);
                            }
                        }
                    }
                    catch (Exception Sftpex)
                    {

                    }


                    Console.ReadLine();
                }

                #endregion

                #region Testing

                //if (module == "T24")
                //{
                //    directoryEmail = AppVars.T24DirectoryEmail;
                //    directory = AppVars.T24Directory;
                //    downloadFolder = AppVars.T24DownloadFolder;
                //    folderError = AppVars.T24FolderError;
                //    folderBackup = AppVars.T24FolderBackup;
                //}
                //else if (module == "DWH")
                //{
                //    directory = AppVars.DWHDirectory;
                //    downloadFolder = AppVars.DWHDownloadFolder;
                //    folderError = AppVars.DWHFolderError;
                //    folderBackup = AppVars.DWHFolderBackup;
                //}

                //string host = hostModule;
                //string username = usernameModule;
                //LogsInsert("Connected to " + host + " as " + username, module);

                //DirectoryInfo df = new DirectoryInfo(downloadFolder);
                //LogsInsert(downloadFolder, module);
                //var files1 = df.GetFiles("*.csv");
                //bool perform1 = false;
                //foreach (var file in files1)
                //{
                //    if (!file.Name.StartsWith(".") && (module == moduleT24))
                //    {
                //        perform1 = true;
                //    }
                //    else if (module == moduleDWH)
                //    {
                //        perform1 = true;
                //    }
                //    else
                //    {
                //        perform1 = false;
                //    }

                //    if (file.Name.ToUpper().StartsWith("EMAIL") && module == moduleT24)
                //    {
                //        string localFilePath = downloadFolder;
                //        string localFileName = downloadFolder + "\\" + file.Name;
                //        LogsInsert("=================================", module);
                //        LogsInsert("Processing " + file.Name, module);

                //        string[] lines = System.IO.File.ReadAllLines(localFileName);

                //        isError = ParseEmail(File.OpenText(localFileName), file.Name, module);

                //        if (isError)
                //        {
                //            string destPath = folderError;
                //            DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                //            if (!dirBackUp.Exists)
                //            {
                //                dirBackUp.Create();
                //            }
                //            if (module == moduleT24)
                //            {
                //                File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                //            }

                //            LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                //        }
                //        else
                //        {
                //            string destPath = folderBackup;
                //            DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                //            if (!dirBackUp.Exists)
                //            {
                //                dirBackUp.Create();
                //            }
                //            File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                //            LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                //        }
                //        File.Delete(localFileName);
                //        LogsInsert("=================================", module);
                //        continue;
                //    }

                //    if (perform1)
                //    {
                //        string localFilePath = downloadFolder;
                //        string localFileName = downloadFolder + "\\" + file.Name;
                //        LogsInsert("=================================", module);
                //        LogsInsert("Processing " + file.Name, module);

                //        string[] lines = System.IO.File.ReadAllLines(localFileName);

                //        if ((lines.Count() > 1) && module == "T24")
                //        {
                //            LogsInsert("Data : " + localFileName + " tidak di proses karena tidak validf. Data mengandung lebih dari 1 line.", module);
                //            isError = true;
                //        }
                //        else
                //        {
                //            isError = Parse(File.OpenText(localFileName), file.Name, module);
                //        }

                //        if (isError)
                //        {
                //            string destPath = folderError;
                //            DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                //            if (!dirBackUp.Exists)
                //            {
                //                dirBackUp.Create();
                //            }
                //            if (module == moduleT24)
                //            {
                //                File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                //            }

                //            LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                //        }
                //        else
                //        {
                //            string destPath = folderBackup;
                //            DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                //            if (!dirBackUp.Exists)
                //            {
                //                dirBackUp.Create();
                //            }
                //            File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                //            LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                //        }
                //        File.Delete(localFileName);
                //        LogsInsert("=================================", module);
                //    }
                //}
                #endregion
                //Console.ReadLine();
            } //SFTP

            catch (Exception e)
            { LogsInsert("xxxxxxxxxx ERROR : " + e.ToString(), module); }
        }

        private static void checkfolder(string module, string hostModule, string usernameModule, string passwordModule, int portModule)
        {
            bool isError = false;
            if (module == "T24")
            {
                directoryEmail = AppVars.T24DirectoryEmail;
                directory = AppVars.T24Directory;
                downloadFolder = AppVars.T24DownloadFolder;
                folderError = AppVars.T24FolderError;
                folderBackup = AppVars.T24FolderBackup;
            }
            else if (module == "DWH")
            {
                directory = AppVars.DWHDirectory;
                downloadFolder = AppVars.DWHDownloadFolder;
                folderError = AppVars.DWHFolderError;
                folderBackup = AppVars.DWHFolderBackup;
            }

            string host = hostModule;
            string username = usernameModule;
            LogsInsert("Connected to " + host + " as " + username, module);

            DirectoryInfo df = new DirectoryInfo(downloadFolder);
            LogsInsert(downloadFolder, module);
            var files1 = df.GetFiles("*.csv");
            bool perform1 = false;
            foreach (var file in files1)
            {
                if (!file.Name.StartsWith(".") && (module == moduleT24))
                {
                    perform1 = true;
                }
                else if (module == moduleDWH)
                {
                    perform1 = true;
                }
                else
                {
                    perform1 = false;
                }

                if (file.Name.ToUpper().StartsWith("EMAIL") && module == moduleT24)
                {
                    string localFilePath = downloadFolder;
                    string localFileName = downloadFolder + "\\" + file.Name;
                    LogsInsert("=================================", module);
                    LogsInsert("Processing " + file.Name, module);

                    string[] lines = System.IO.File.ReadAllLines(localFileName);

                    isError = ParseEmail(File.OpenText(localFileName), file.Name, module);

                    if (isError)
                    {
                        string destPath = folderError;
                        DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                        if (!dirBackUp.Exists)
                        {
                            dirBackUp.Create();
                        }
                        if (module == moduleT24)
                        {
                            File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                        }

                        LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                    }
                    else
                    {
                        string destPath = folderBackup;
                        DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                        if (!dirBackUp.Exists)
                        {
                            dirBackUp.Create();
                        }
                        File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                        LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                    }
                    File.Delete(localFileName);
                    LogsInsert("=================================", module);
                    continue;
                }

                if (perform1)
                {
                    string localFilePath = downloadFolder;
                    string localFileName = downloadFolder + "\\" + file.Name;
                    LogsInsert("=================================", module);
                    LogsInsert("Processing " + file.Name, module);

                    string[] lines = System.IO.File.ReadAllLines(localFileName);

                    if ((lines.Count() > 1) && module == "T24")
                    {
                        LogsInsert("Data : " + localFileName + " tidak di proses karena tidak validf. Data mengandung lebih dari 1 line.", module);
                        isError = true;
                    }
                    else
                    {
                        isError = Parse(File.OpenText(localFileName), file.Name, module);
                    }

                    if (isError)
                    {
                        string destPath = folderError;
                        DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                        if (!dirBackUp.Exists)
                        {
                            dirBackUp.Create();
                        }
                        if (module == moduleT24)
                        {
                            File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                        }

                        LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                    }
                    else
                    {
                        string destPath = folderBackup;
                        DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                        if (!dirBackUp.Exists)
                        {
                            dirBackUp.Create();
                        }
                        File.Move(localFileName, dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name);
                        LogsInsert("File Moved To : " + dirBackUp + "\\." + DateTime.Now.Millisecond.ToString() + file.Name, module);
                    }
                    File.Delete(localFileName);
                    LogsInsert("=================================", module);
                }
            }
        }
        private static void DownloadDirectory(SftpClient client, string source, string destination, string module)
        {
            var files = client.ListDirectory(source);
            LogsInsert("Starting Download Directory ...", module);
            foreach (var file in files)
            {
                if (!file.IsDirectory && !file.IsSymbolicLink)
                {
                    DownloadFile(client, file, destination, module);
                }
                else if (file.IsSymbolicLink)
                {
                    Console.WriteLine("Ignoring symbolic link {0}", file.FullName);
                    LogsInsert("Ignoring symbolic link " + file.FullName, module);
                }
            }
        }

        private static void DownloadFile(SftpClient client, SftpFile file, string directory, string module)
        {
            Console.WriteLine("Downloading {0}", file.FullName);
            LogsInsert("Downloading " + file.FullName, module);
            using (Stream fileStream = File.OpenWrite(Path.Combine(directory, file.Name)))
            {
                client.DownloadFile(file.FullName, fileStream);
                LogsInsert("Download " + file.FullName + " done", module);
                file.Delete();
            }
        }

        static bool ParseEmail(StreamReader reader, string csvName, string module)
        {
            string text;
            bool isDataError = false;
            bool isEmailError = true;
            var to = string.Empty;
            var str = new StringBuilder();


            using (reader)
            {
                var line = 1;
                while ((text = reader.ReadLine()) != null)
                {
                    if (text.Contains("to="))
                    {
                        to = text.Substring(text.IndexOf("to=") + 3);
                    }
                    else str.AppendLine(text);
                    line++;
                }

                isEmailError = EmailExchangeServer(new string[] { to, "", "", "", str.ToString().Replace("\r\n", "</br>"), "" }, module, "Pemberitahuan Penerimaan Dana", true);

                if (isEmailError)
                {
                    isDataError = true;
                    LogsInsert("Data : " + to + " Email tidak dikirimkan ke nasabah. File dipindahkan ke folder error. Cek koneksi SMS Server, Email Server atau konten filenya.", module);
                }
                else
                {
                    isDataError = false;
                    LogsInsert("Data : " + to + " SUKSES.  Email telah dikirim ke nasabah. File akan dipindahkan ke folder Data BackUp Done.", module);
                }
            }
            return isDataError;
        }

        static bool Parse(StreamReader reader, string csvName, string module)
        {
            string text;
            bool isDataError = false;
            bool isSmsError, isEmailError = true;
            var resultCheckMinimumTransaction = false;
            var EmailTerpisah = false;

            using (reader)
                while ((text = reader.ReadLine()) != null)
                {
                    //Transaction Code | Tgl Generate | No Telp | Email | Body | Name | Minimum Transaction

                    text = text.Replace(" | ", "|");
                    string[] parts = text.Replace("\r", string.Empty).Split('|', '\n');

                    for (int i = 0; i < parts.Count(); i++)
                    {
                        LogsInsert(parts[i], module);
                    }

                    if (parts.Count() > 9)
                    {
                        LogsInsert("Error : Data tidak valid. Data mengandung lebih dari 9 delimeter.", module);
                        return isDataError = true;
                    }
                    else if (parts.Count() < 4)
                    {
                        LogsInsert("Error : Data tidak valid. Data mengandung kurang dari 4 delimeter.", module);
                        return isDataError = true;
                    }

                    if (AppVars.UsingAmountParameter)
                    {
                        #region new enhance
                        /// new enhance

                        if (module == moduleT24)
                        {
                            try
                            {
                                var transaksi = parts[7];
                                //if (transaksi.Contains("."))
                                //{
                                //    transaksi = transaksi.Replace(".", ",");
                                //}

                                var nominal = float.Parse(transaksi);
                                if (nominal == 0)
                                {
                                    resultCheckMinimumTransaction = true;
                                }
                                else
                                {
                                    resultCheckMinimumTransaction = CheckMinimumTransaction(module, parts[0], nominal);
                                }
                                EmailTerpisah = CheckEmailTerpisah(module, parts[0]);

                            }
                            catch
                            {
                                resultCheckMinimumTransaction = false;
                                LogsInsert("Tidak ada parameter nominal", module);
                            }
                        }
                        else
                        {
                            float nominal;
                            try
                            {
                                if (parts.Count() > 5)
                                {
                                    var transaksi = parts[5];
                                    transaksi = transaksi.Replace(",", "").Replace("Rp.", "");
                                    if (transaksi.Contains("."))
                                    {
                                        transaksi = transaksi.Replace(".", ",");
                                    }

                                    nominal = float.Parse(transaksi);


                                    using (DbConnection conn = new DbConnection(AppVars.connstr))
                                    {
                                        object[] par = new object[1] { module };
                                        string transactionCode;

                                        conn.ExecReader(SQLTransactionCodeDWH, par, AppVars.dbtimeout);
                                        while (conn.hasRow())
                                        {
                                            if (parts[3].ToLower().Contains(conn.GetFieldValue("TransactionCode").ToLower()))
                                            {
                                                transactionCode = conn.GetFieldValue("TransactionCode");
                                                if (nominal == 0)
                                                {
                                                    resultCheckMinimumTransaction = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    resultCheckMinimumTransaction = CheckMinimumTransaction(module, transactionCode, nominal);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    resultCheckMinimumTransaction = false;
                                    LogsInsert("Tidak ada parameter nominal", module);
                                }
                            }
                            catch
                            {
                                resultCheckMinimumTransaction = false;
                                LogsInsert("Tidak ada parameter nominal", module);
                            }

                        }

                        //////////////////////////

                        if (resultCheckMinimumTransaction)
                        {
                            isSmsError = SMS(parts, module);
                            if (!EmailTerpisah)
                                isEmailError = EmailExchangeServer(parts, module);
                            else isEmailError = false;
                        }
                        else
                        {
                            isSmsError = true;
                            isEmailError = true;
                            LogsInsert("SMS dan Email tidak dikirimkan ke nasabah karena nominal transaksi di bawah nilai yang ditentukan.", module);
                            LogsInsertLogEmailSMS(parts, module, "SMS EMAIL", "SMS dan Email tidak dikirimkan ke nasabah karena nominal transaksi di bawah nilai yang ditentukan. ", "0");
                        }

                        #endregion
                    }
                    else
                    {
                        if (module == moduleT24)
                            EmailTerpisah = CheckEmailTerpisah(module, parts[0]);
                        isSmsError = SMS(parts, module);
                        if (!EmailTerpisah)
                            isEmailError = EmailExchangeServer(parts, module);
                    }

                    //isSmsError = SMS(parts, module);
                    //isEmailError = EmailExchangeServer(parts, module);

                    if ((isSmsError) || (isEmailError))
                    {
                        isDataError = true;
                        if (module == moduleDWH)
                        {
                            string destPath = folderError;
                            DirectoryInfo dirBackUp = new DirectoryInfo(destPath);
                            if (!dirBackUp.Exists)
                            {
                                dirBackUp.Create();
                            }

                            string filePath = folderError + "\\." + DateTime.Now.Millisecond.ToString() + "Error-" + csvName;
                            string delimiter = " | ";

                            StringBuilder sb = new StringBuilder();
                            for (int index = 0; index < parts.Count(); index++)
                            {
                                sb.Append(parts[index]);
                                if (index != parts.Count() - 1)
                                {
                                    sb.Append(delimiter);
                                }
                            }
                            File.WriteAllText(filePath, sb.ToString());
                        }
                        LogsInsert("Data : " + parts[1] + " SMS dan Email tidak dikirimkan ke nasabah. File dipindahkan ke folder error. Cek koneksi SMS Server, Email Server atau konten filenya.", module);
                    }
                    else
                    {
                        isDataError = false;
                        LogsInsert("Data : " + parts[1] + " SUKSES. SMS dan Email telah dikirim ke nasabah. File akan dipindahkan ke folder Data BackUp Done.", module);
                    }

                }

            return isDataError;
        }

        static bool SMS(string[] parameter, string module)
        {
            var smsAlternatif = ConfigurationManager.AppSettings["ServiceSMSEmailAlternative"] != null ? ConfigurationManager.AppSettings["ServiceSMSEmailAlternative"] : "";
            string numberSMS, SMSBody, TransactionCode = "", NoRekening = "";

            /*Paramater SMSBODYOLX NEW*/
            /*if (module == moduleDWH)
            {
                NoRekening = parameter[6];
            }
            else if (module == moduleT24)
            {
                NoRekening = parameter[8];
                TransactionCode = parameter[0];
            }*/
            //NominalTransaksi = parameter[7];

            //if (smsAlternatif == "on")
            //    return SMSAlternative(parameter, module);


            bool isSmsError = false;

            try
            {
                LogsInsert("****** Start Sending SMS", module);
                if (module == moduleT24)
                {
                    /*START SMSBODYOLX NEW IF 20220215*/
                    //if (TransactionCode == kodesmsolx)
                    //{
                        DbConnection conn = new DbConnection(AppVars.connstr);
                        /*get smsbody olx 20220216*/
                        DataTable dt = new DataTable();
                        NoRekening = parameter[8];
                        TransactionCode = parameter[0];
                        object[] par = new object[2] { NoRekening, TransactionCode };
                        dt = conn.GetDataTable("exec USP_CHECKPRODUCT @1,@2", par, AppVars.dbtimeout);
                        int jmldt = dt.Rows.Count;

                        if (jmldt > 0)
                        {
                            SMSBody = dt.Rows[0].Field<string>("SMSBody");

                        }
                        else
                        {
                            SMSBody = SecurityElement.Escape(parameter[4]);
                        }


                    //}
                    //else
                    //{
                    //    SMSBody = SecurityElement.Escape(parameter[4]);
                    //}
                    numberSMS = parameter[2];

                }
                else
                {
                    NoRekening = parameter[6];
                    numberSMS = parameter[1];
                    SMSBody = SecurityElement.Escape(parameter[3]);
                }
                double v;
                bool isNumber = double.TryParse(numberSMS, NumberStyles.Any, CultureInfo.InvariantCulture, out v);

                if (!isNumber || (numberSMS.Length < 6))
                {
                    LogsInsert("****** Nomor SMS tidak valid. ", module);
                    LogsInsertLogEmailSMS(parameter, module, "SMS", "Nomor SMS tidak valid. ", "0");
                    isSmsError = true;
                    return isSmsError;
                }

                if (SMSBody.Length == 0)
                {
                    LogsInsert("****** SMS tidak mengandung pesan. ", module);
                    LogsInsertLogEmailSMS(parameter, module, "SMS", "SMS tidak mengandung pesan. ", "0");
                    isSmsError = true;
                    return isSmsError;
                }



                XmlDocument xmlDoc = new XmlDocument();
                //Create an XML declaration. 
                XmlDeclaration xmldecl;
                xmldecl = xmlDoc.CreateXmlDeclaration("1.0", null, null);
                xmldecl.Encoding = "UTF-8";
                XmlElement root = xmlDoc.DocumentElement;
                xmlDoc.InsertBefore(xmldecl, root);

                XmlNode rootNode = xmlDoc.CreateElement("smsbc");
                xmlDoc.AppendChild(rootNode);
                XmlNode requestNode = xmlDoc.CreateElement("request");
                rootNode.AppendChild(requestNode);
                XmlNode datetimeNode = xmlDoc.CreateElement("datetime");
                datetimeNode.InnerText = DateTime.Now.ToString("MMddHHmmss");
                requestNode.AppendChild(datetimeNode);
                XmlNode rrnNode = xmlDoc.CreateElement("rrn");
                rrnNode.InnerText = DateTime.Now.ToString("MMddHHmmssfff"); //parameter[0];
                requestNode.AppendChild(rrnNode);
                XmlNode partnerIdNode = xmlDoc.CreateElement("partnerId");
                partnerIdNode.InnerText = AppVars.SMSPartnerId;
                requestNode.AppendChild(partnerIdNode);
                XmlNode partnerNameNode = xmlDoc.CreateElement("partnerName");
                partnerNameNode.InnerText = AppVars.SMSPartnerName;
                requestNode.AppendChild(partnerNameNode);
                XmlNode passwordNode = xmlDoc.CreateElement("password");
                passwordNode.InnerText = AppVars.SMSPassword;
                requestNode.AppendChild(passwordNode);
                XmlNode destinationNumberNode = xmlDoc.CreateElement("destinationNumber");
                destinationNumberNode.InnerText = numberSMS;
                requestNode.AppendChild(destinationNumberNode);
                XmlNode messageNode = xmlDoc.CreateElement("message");
                messageNode.InnerText = SMSBody;
                requestNode.AppendChild(messageNode);
                rootNode.AppendChild(requestNode);

                if (PostXMLTransaction(AppVars.SMSIP, xmlDoc, module) == null)
                {
                    LogsInsert("****** SMS CONNECTION FAILED", module);
                    LogsInsertLogEmailSMS(parameter, module, "SMS", "SMS CONNECTION FAILED. ", "0");
                    isSmsError = true;
                    return isSmsError;
                }

                //PostXMLTransaction(AppVars.SMSIP, xmlDoc, module);

                xmlDoc.Save("test-doc.xml");
                // using (var client = new WebClient())
                //{

                //  var responseString = client.DownloadString("http://128.199.128.171:8080/sms/sendSms?number=" + parameter[2] + "&message=" + parameter[4] + "&type=B");
                LogsInsert("****** SMS Sent", module);
                LogsInsertLogEmailSMS(parameter, module, "SMS", "SMS Sent. ", "1");
                isSmsError = false;
                return isSmsError;
                //}
            }
            catch (Exception e)
            {
                LogsInsert("****** Error SMS : " + e.Message.ToString(), module);
                LogsInsertLogEmailSMS(parameter, module, "SMS", "Error SMS : " + e.Message.ToString(), "0");
                isSmsError = true;
                return isSmsError;
            }

        }

        static bool SMSAlternative(string[] parameter, string module)
        {
            bool isSmsError = false;
            string numberSMS, SMSBody, TransactionCode, NoRekening;

            var kodesmsolx = ConfigurationManager.AppSettings["KODESMSOLX"];
            DbConnection conn = new DbConnection(AppVars.connstr);
            try
            {
                LogsInsert("****** Start Sending SMS", module);
                /*NEW IF 20220215*/
                TransactionCode = parameter[0];
                NoRekening = parameter[8];
                if (module == moduleT24)
                {
                    //if (TransactionCode == kodesmsolx)
                    //{
                        
                        /*get smsbody olx 20220216*/
                        DataTable dt = new DataTable();
                        
                        object[] par = new object[2] { NoRekening, TransactionCode };
                        dt = conn.GetDataTable("exec USP_CHECKPRODUCT @1,@2", par, AppVars.dbtimeout);
                        int jmldt = dt.Rows.Count;

                        if(jmldt > 0)
                        {
                            SMSBody = dt.Rows[0].Field<string>("SMSBody");

                        }
                        else
                        {
                            SMSBody = SecurityElement.Escape(parameter[4]);
                        }

                    //}
                    //else
                    //{
                    //    SMSBody = SecurityElement.Escape(parameter[4]);
                    //}

                    numberSMS = parameter[2];
                    //SMSBody = SecurityElement.Escape(parameter[4]);
                }
                else
                {
                    numberSMS = parameter[1];
                    SMSBody = SecurityElement.Escape(parameter[3]);
                }
                double v;
                bool isNumber = double.TryParse(numberSMS, NumberStyles.Any, CultureInfo.InvariantCulture, out v);

                if (!isNumber || (numberSMS.Length < 6))
                {
                    LogsInsert("****** Nomor SMS tidak valid. ", module);
                    LogsInsertLogEmailSMS(parameter, module, "SMS", "Nomor SMS tidak valid.", "0");
                    isSmsError = true;
                    return isSmsError;
                }

                if (SMSBody.Length == 0)
                {
                    LogsInsert("****** SMS tidak mengandung pesan. ", module);
                    LogsInsertLogEmailSMS(parameter, module, "SMS", "SMS tidak mengandung pesan. ", "0");
                    isSmsError = true;
                    return isSmsError;
                }

                var UrlAPISMSAlternative = ConfigurationManager.AppSettings["UrlAPISMSAlternative"];
                var FunctionAPISMSAlternative = ConfigurationManager.AppSettings["FunctionAPISMSAlternative"];
                var UrlAPISMSAlternativeUsername = ConfigurationManager.AppSettings["UrlAPISMSAlternativeUsername"];
                var UrlAPISMSAlternativePassword = ConfigurationManager.AppSettings["UrlAPISMSAlternativePassword"];
                var UrlAPISMSAlternativeSenderID = ConfigurationManager.AppSettings["UrlAPISMSAlternativeSenderID"];
                var client = new RestClient(UrlAPISMSAlternative +
                    FunctionAPISMSAlternative.Replace("[usernamegratika]",
                    UrlAPISMSAlternativeUsername).Replace("[passwordgratika]",
                    UrlAPISMSAlternativePassword).Replace("[senderidgratika]",
                    UrlAPISMSAlternativeSenderID).Replace("[messagebody]", SMSBody).Replace("[nomorhp]", numberSMS));

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                client.ClearHandlers();
                client.AddHandler("*", new JsonDeserializer());

                var request = new RestRequest(Method.GET);
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "text/html");

                var response = client.Execute(request);

                LogsInsert("response : " + response.Content, module);
                if (response.StatusCode.ToString() != "OK")
                {
                    LogsInsert("****** SMS CONNECTION FAILED", module);
                    isSmsError = true;
                    return isSmsError;
                }


                //  var responseString = client.DownloadString("http://128.199.128.171:8080/sms/sendSms?number=" + parameter[2] + "&message=" + parameter[4] + "&type=B");
                LogsInsert("****** SMS Sent", module);
                LogsInsertLogEmailSMS(parameter, module, "SMS", "SMS Sent. ", "1");
                isSmsError = false;
                return isSmsError;
                //}
            }
            catch (Exception e)
            {
                LogsInsert("****** Error SMS : " + e.Message.ToString(), module);
                LogsInsertLogEmailSMS(parameter, module, "SMS", "Error SMS : " + e.Message.ToString(), "0");
                isSmsError = true;
                return isSmsError;
            }
        }

        static bool Email(string[] parameter, string module)
        {
            bool isEmailError = false;
            string messageHeader, messageBody;
            try
            {
                LogsInsert("****** Start Sending Email", module);
                var client = new SmtpClient(AppVars.EmailIP, AppVars.EmailPort)
                {
                    Credentials = new NetworkCredential(AppVars.EmailAddress, AppVars.EmailPassword),
                    EnableSsl = true
                };

                if (module == moduleT24)
                {
                    messageHeader = parameter[5];
                    messageBody = parameter[4];
                }
                else
                {
                    messageHeader = parameter[4];
                    messageBody = parameter[3];
                }

                var emailAddress = "";

                foreach (var item in parameter)
                {
                    if (item.Contains("@"))
                    {
                        emailAddress = item;
                    }
                }

                if (emailAddress == "")
                {
                    LogsInsert("****** Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung alamat email", module);
                    isEmailError = false;
                    return isEmailError;
                }

                if (string.IsNullOrEmpty(messageBody) || string.IsNullOrWhiteSpace(messageBody))
                {
                    LogsInsert("****** Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung pesan", module);
                    isEmailError = true;
                    return isEmailError;
                }

                MailMessage mail = new MailMessage();
                mail.Subject = "Pemberitahuan Transaksi";
                mail.From = new MailAddress(AppVars.EmailAddress, AppVars.EmailDisplayName);
                mail.To.Add(new MailAddress(emailAddress));
                mail.IsBodyHtml = true;
                mail.Body = "Kepada Yth " + messageHeader + ",<br><br>" + messageBody;
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object s, X509Certificate certificate,
                    X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    { return true; };

                client.Send(mail);

                LogsInsert("****** Email Sent", module);
                isEmailError = false;
                return isEmailError;

            }
            catch (Exception e)
            {
                LogsInsert("****** Error Email : " + e.Message.ToString(), module);
                isEmailError = true;
                return isEmailError;
            }
        }

        static bool EmailExchangeServer(string[] parameter, string module, string subjectEmail = "", bool emailTerpisah = false)
        {
            bool isEmailError = false;
            string messageHeader, messageBody;
            if (string.IsNullOrEmpty(subjectEmail))
                subjectEmail = "Pemberitahuan Transaksi";
            try
            {
                LogsInsert("****** Start Sending Email", module);

                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                string setUserName = AppVars.AlternativeEmailUsername;
                string setPassword = AppVars.AlternativeEmailPassword;
                string setUri = AppVars.AlternativeEmailServer;
                bool setUseDefaultCredentials = false;
                int setExchangeVersion = 0;


                var exchangeService = new ExchangeService((ExchangeVersion)setExchangeVersion)
                {
                    Url = new Uri(setUri),
                    //UseDefaultCredentials = setUseDefaultCredentials,
                    Credentials = new WebCredentials(setUserName, setPassword)
                };

                if (module == moduleT24)
                {
                    messageHeader = parameter[5];
                    messageBody = parameter[4];
                }
                else
                {
                    messageHeader = parameter[4];
                    messageBody = parameter[3];
                }

                var emailAddress = "";

                foreach (var item in parameter)
                {
                    if (item.Contains("@"))
                    {
                        emailAddress = item;
                    }
                }
                //Tambahan Tayo
                if (emailTerpisah)
                {
                    emailAddress = emailAddress.Replace(",,", "");
                    messageHeader = messageHeader.Replace(",,", "");
                    messageBody = messageBody.Replace(",,", "");
                }

                if (emailAddress == "")
                {
                    LogsInsert("****** Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung alamat email", module);
                    LogsInsertLogEmailSMS(parameter, module, "EMAIL", "Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung alamat email", "0", emailTerpisah);
                    isEmailError = false;
                    return isEmailError;
                }

                if (string.IsNullOrEmpty(messageBody) || string.IsNullOrWhiteSpace(messageBody))
                {
                    LogsInsert("****** Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung pesan", module);
                    LogsInsertLogEmailSMS(parameter, module, "EMAIL", "Data : " + parameter[1] + " tidak dikirimkan email karena data tidak mengandung pesan", "0", emailTerpisah);
                    isEmailError = true;
                    return isEmailError;
                }

                //LogsInsert("Masuk 0", module);
                var em = new EmailMessage(exchangeService);
                if (messageBody.ToLower().Contains("melakukan perubahan data"))
                {
                    subjectEmail = "Perubahan data Bank Sampoerna";
                    em.Body = "Kepada Yth.Bapak/Ibu/Sdr," + "<br>" + messageHeader + "<br><br>" + messageBody + "<br><br> Salam <br> Bank Sahabat Sampoerna";
                }
                else
                {
                    if (emailTerpisah)
                        em.Body = messageBody;
                    else
                        em.Body = "Kepada Yth " + messageHeader + ",<br><br>" + messageBody + "<br><br> Salam <br> Bank Sahabat Sampoerna";
                }

                em.Subject = subjectEmail;
                //LogsInsert("Masuk 1 " + subjectEmail, module);
                em.From = new EmailAddress(AppVars.EmailAddress, AppVars.EmailDisplayName);
                //LogsInsert("Masuk 2 " + AppVars.EmailAddress +" - "+ AppVars.EmailDisplayName, module);
                em.ToRecipients.Add(new EmailAddress(messageHeader, emailAddress));
                //LogsInsert("Masuk 3 "+ emailAddress + "  hrader " + messageHeader + " body : "+ messageBody, module);
                em.Send();
                LogsInsert("****** Email Sent", module);
                LogsInsertLogEmailSMS(parameter, module, "EMAIL", "Email Sent. ", "1", emailTerpisah);
                isEmailError = false;
                return isEmailError;

            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    LogsInsert("****** Error Email : " + e, module);
                }
                LogsInsert("****** Error Email : " + e.Message.ToString(), module);

                LogsInsertLogEmailSMS(parameter, module, "EMAIL", "Error Email : " + e.Message.ToString(), "0", emailTerpisah);
                isEmailError = true;
                return isEmailError;
            }

        }

        public static XmlDocument PostXMLTransaction(string v_strURL, XmlDocument v_objXMLDoc, string module)
        {
            LogsInsert("****** SMS CONNECTION : Start HTTP REQUEST", module);
            //Declare XMLResponse document
            XmlDocument XMLResponse = null;

            //Declare an HTTP-specific implementation of the WebRequest class.
            HttpWebRequest objHttpWebRequest;

            //Declare an HTTP-specific implementation of the WebResponse class
            HttpWebResponse objHttpWebResponse = null;

            //Declare a generic view of a sequence of bytes
            Stream objRequestStream = null;
            Stream objResponseStream = null;

            //Declare XMLReader
            XmlTextReader objXMLReader;

            //Creates an HttpWebRequest for the specified URL.
            objHttpWebRequest = (HttpWebRequest)WebRequest.Create(v_strURL);

            try
            {
                //---------- Start HttpRequest 

                //Set HttpWebRequest properties
                byte[] bytes;
                bytes = System.Text.Encoding.ASCII.GetBytes(v_objXMLDoc.InnerXml);
                objHttpWebRequest.Method = "POST";
                objHttpWebRequest.ContentLength = bytes.Length;
                objHttpWebRequest.ContentType = "text/xml; encoding='utf-8'";

                //Get Stream object 
                objRequestStream = objHttpWebRequest.GetRequestStream();

                //Writes a sequence of bytes to the current stream 
                objRequestStream.Write(bytes, 0, bytes.Length);

                //Close stream
                objRequestStream.Close();

                //---------- End HttpRequest

                //Sends the HttpWebRequest, and waits for a response.
                objHttpWebResponse = (HttpWebResponse)objHttpWebRequest.GetResponse();

                //---------- Start HttpResponse
                if (objHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    //Get response stream 
                    objResponseStream = objHttpWebResponse.GetResponseStream();

                    //Load response stream into XMLReader
                    objXMLReader = new XmlTextReader(objResponseStream);

                    //Declare XMLDocument
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(objXMLReader);

                    //Set XMLResponse object returned from XMLReader
                    XMLResponse = xmldoc;

                    //Close XMLReader
                    objXMLReader.Close();
                }

                //Close HttpWebResponse
                objHttpWebResponse.Close();
            }
            catch (WebException we)
            {
                //TODO: Add custom exception handling
                LogsInsert("****** Error SMS : " + we.Message.ToString(), module);
                XMLResponse = null;
                return XMLResponse;
                //throw new Exception(we.Message);

            }
            catch (Exception ex)
            {
                LogsInsert("****** Error SMS : " + ex.Message.ToString(), module);
                XMLResponse = null;
                return XMLResponse;
                //throw new Exception(ex.Message);
            }
            finally
            {
                //Close connections
                objRequestStream.Close();
                objResponseStream.Close();
                objHttpWebResponse.Close();

                //Release objects
                objXMLReader = null;
                objRequestStream = null;
                objResponseStream = null;
                objHttpWebResponse = null;
                objHttpWebRequest = null;
            }

            //Return
            LogsInsert("****** SMS CONNECTION : Return XMLResponse = " + XMLResponse.OuterXml, module);
            return XMLResponse;
        }

        public static string EscapeXMLValue(string xmlString)
        {

            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            return xmlString.Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("&", "&amp;");
        }

        public static bool CheckMinimumTransaction(string module, string transactionCode, float nominal)
        {
            var result = false;
            float minimalTransactionNominal;
            try
            {
                using (DbConnection conn = new DbConnection(AppVars.connstr))
                {
                    object[] par = new object[2] { module, transactionCode };
                    conn.ExecReader(SQLMinimalTransaction, par, AppVars.dbtimeout);
                    if (conn.hasRow())
                    {
                        minimalTransactionNominal = float.Parse((conn.GetFieldValue("MinimumTransacation")));
                        if (minimalTransactionNominal < 1)
                            return true;

                        if (nominal >= minimalTransactionNominal)
                        {
                            result = true;
                            LogsInsert(nominal.ToString() + " lebih dari " + minimalTransactionNominal.ToString(), module);
                            LogsInsert("Nominal transaksi berada di atas nominal minimal transaksi pengiriman notifikasi", module);
                        }
                        else
                        {
                            LogsInsert(nominal.ToString() + " kurang dari " + minimalTransactionNominal.ToString(), module);
                            LogsInsert("Nominal transaksi berada di bawah nominal minimal transaksi pengiriman notifikasi", module);
                        }
                    }
                }
            }
            catch
            {
                result = false;
                return result;
            }
            return result;
        }
        //update by wicak 20200221
        public static bool CheckEmailTerpisah(string module, string transactionCode)
        {
            var result = false;
            try
            {
                using (DbConnection conn = new DbConnection(AppVars.connstr))
                {
                    object[] par = new object[2] { module, transactionCode };
                    conn.ExecReader("SELECT * FROM ParameterTransaction WHERE NotificationType = @1 AND TransactionCode = @2", par, AppVars.dbtimeout);
                    if (conn.hasRow())
                    {
                        var email = bool.Parse(conn.GetFieldValue("EmailTerpisah"));
                        if (email)
                            return true;
                    }
                }
            }
            catch (Exception e)
            {
                result = false;
                return result;
            }
            return result;
        }

    }
}
