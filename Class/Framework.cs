using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Configuration;

using DMSControls;
using DMS.Tools;

namespace DMS.Framework
{
    /// <summary>
    /// Summary description for CMS.
    /// </summary>
    /// 
    #region dynamicFramework
    public class dynamicFramework
    {
        private DbConnection conn = null;
        private DataTable dtForeignKey = null;
        private DataTable dtLinkedServer = null;
        private DataTable dtFormulaField = null;
        public Hashtable hashFieldFW = null;

        static string SeperatorStartLinkedServ = "{";
        static string SeperatorEndLinkedServ = "}";
        static string SeperatorSqlStartTable = "[";
        //static string SeperatorSqlEndTable = "]";
        static string SeperatorTableField = "].";
        static char[] SeperatoSqlrString = { '\'' };
        static int dbtimeout = 10000;

        public dynamicFramework(DbConnection conn)
        {
            this.conn = conn;
            hashFieldFW = new Hashtable();
            dtLinkedServer = conn.GetDataTable(
                "SELECT * FROM FRAMEWORK_LINDEDSERVER", null, dbtimeout);
            dtForeignKey = conn.GetDataTable(
                "SELECT * FROM FRAMEWORK_FOREIGNKEY", null, dbtimeout);
            dtFormulaField = conn.GetDataTable(
                "SELECT * FROM FRAMEWORK_FORMULAFIELD", null, dbtimeout);

        }

        #region dynamic formula

        public string retrvFieldId(string FieldFW)
        {
            return retrvFieldId(FieldFW, "");
        }

        public string retrvCondFW(string CondFW)
        {
            return retrvCondFW(CondFW, "");
        }

        private string retrvFieldId(string FieldFW, string chkFieldId)
        {
            int idx = FieldFW.IndexOf("@[");
            if (idx < 0) return "";
            int idx2 = FieldFW.IndexOf("]", idx);
            string FieldId = FieldFW.Substring(idx + 2, idx2 - idx - 2);

            if (chkFieldId.IndexOf(FieldFW) < 0)
                chkFieldId += FieldFW;
            else
                throw new Exception("Infinite Recursive Formula Field");

            if (!hashFieldFW.ContainsKey(FieldId))
            {
                DataView dv = new DataView(dtFormulaField, "FIELDID = '" + FieldId + "'", null, DataViewRowState.OriginalRows);
                NameValueCollection nvc = new NameValueCollection();
                for (int i = 0; i < dtFormulaField.Columns.Count; i++)
                    if (dv.Count != 0)
                        nvc[dtFormulaField.Columns[i].ColumnName] = dv[0][i].ToString();
                    else
                        nvc[dtFormulaField.Columns[i].ColumnName] = "";
                if (dv.Count > 0)
                {
                    if (nvc["FIELDID"] != FieldId)
                        throw new Exception("Case Sensitive " + nvc["FIELDID"] + " :" + FieldId);
                    nvc["FieldFW"] = retrvCondFW(nvc["FieldFW"], chkFieldId);
                }
                else
                {
                    int idxsep = FieldId.LastIndexOf("|");
                    if (idxsep != -1)
                        nvc["FieldFW"] = "[" + FieldId.Substring(0, idxsep) + "].[" + FieldId.Substring(idxsep + 1) + "]";
                    else
                        nvc["FieldFW"] = "[" + FieldId.Substring(0, FieldId.Length) + "].[" + FieldId.Substring(idxsep + 1) + "]";

                }
                hashFieldFW.Add(FieldId, nvc);
                dv.Dispose();
            }
            return FieldId;
        }

        private string retrvCondFW(string CondFW, string chkFieldId)
        {
            while (CondFW.IndexOf("@[") >= 0)
            {
                string FieldId = retrvFieldId(CondFW, chkFieldId);
                CondFW = CondFW.Replace("@[" + FieldId + "]", (hashFieldFW[FieldId] as NameValueCollection)["FieldFW"].ToString());
            }
            return CondFW;
        }

        #endregion

        #region dynamic query

        private NameValueCollection RetrieveListTable(string strSql)
        {
            NameValueCollection NVCTable = new NameValueCollection();

            string[] strSqlList = strSql.Split(SeperatoSqlrString);
            for (int x = 0; x < strSqlList.Length; x++)
                if (x % 2 == 0)
                {
                    string strParseTable = strSqlList[x];
                    int lenParseTable = strParseTable.Length;
                    int ParsePos = 0;
                    while (strParseTable.IndexOf(SeperatorTableField, ParsePos) != -1)
                    {
                        int ParsePos2 = strParseTable.IndexOf(SeperatorTableField, ParsePos) + 1;
                        int ParsePos3 = strParseTable.LastIndexOf(SeperatorSqlStartTable, ParsePos2);
                        if (ParsePos3 == -1) break;
                        if (ParsePos3 != 0 && strParseTable[ParsePos3 - 1].ToString() == SeperatorEndLinkedServ)
                        {
                            ParsePos3 = strParseTable.LastIndexOf(SeperatorStartLinkedServ, ParsePos2);
                            if (ParsePos3 == -1) break;
                        }
                        string tableName = strParseTable.Substring(ParsePos3, ParsePos2 - ParsePos3).ToUpper();
                        NVCTable[tableName] = "";
                        ParsePos = ParsePos2 + 2;
                    }
                }

            return NVCTable;
        }

        private void RetrieveTableFK(NameValueCollection NVCTable, string TableName, string TableTree, ref string HTree)
        {
            if (NVCTable[TableName] == "1")
            {
                string[] TableTree1 = TableTree.Split(',');
                bool flag = false;
                string TableTree2 = "";
                int treelen = TableTree1.Length - 2;
                for (int i = treelen; i >= 0; i--)
                {
                    if (NVCTable[TableTree1[i]] != null || flag)
                    {
                        flag = true;
                        NVCTable[TableTree1[i]] = "1";
                        TableTree2 = TableTree1[i] + "," + TableTree2;
                    }
                }
                if (TableTree2 != "")
                    HTree = HTree.Replace(TableName + ",", TableName + "," + TableTree2);
                return;
            }



            DataView dv = new DataView(dtForeignKey, "TABLENAME='" + TableName + "'", "", DataViewRowState.OriginalRows);
            for (int i = 0; i < dv.Count; i++)
            {
                string TableNameFK = dv[i]["TABLENAME_FK"].ToString();
                RetrieveTableFK(NVCTable, TableNameFK, TableName + "," + TableTree, ref HTree);

            }

            if (NVCTable[TableName] == null)
                NVCTable[TableName] = "";
            else if (NVCTable[TableName] == "")
            {
                NVCTable[TableName] = "1";
                HTree = TableName + "," + HTree;

                string[] TableTree1 = TableTree.Split(',');
                bool flag = false;
                string TableTree2 = "";
                int treelen = TableTree1.Length - 2;
                for (int i = treelen; i >= 0; i--)
                {
                    if (NVCTable[TableTree1[i]] != null || flag)
                    {
                        flag = true;
                        NVCTable[TableTree1[i]] = "1";
                        TableTree2 = TableTree1[i] + "," + TableTree2;
                    }
                }
                if (TableTree2 != "")
                    HTree = HTree.Replace(TableName + ",", TableName + "," + TableTree2);
            }
        }

        private string RetrieveJoinString(NameValueCollection NVCTable)
        {
            int tblCount = NVCTable.Count;
            string HTree = "";
            for (int i = 0; i < tblCount; i++)
            {
                string TableName = NVCTable.Keys[i].ToString();
                RetrieveTableFK(NVCTable, TableName, "", ref HTree);
            }

            //if (HTree.Substring(HTree.Length - 1, 1).Equals(","))
            //    HTree = HTree.Substring(0, HTree.Length - 1);

            string[] JoinTableName = HTree.Split(',');
            string strJoin = "FROM " + JoinTableName[0] + " \n";
            for (int i = 1; i < JoinTableName.Length - 1; i++)
            {
                string TableName = JoinTableName[i];
                if (NVCTable[TableName] == "1")
                {
                    DataView dv = new DataView(dtForeignKey, "TABLENAME='" + TableName + "'", "", DataViewRowState.OriginalRows);
                    if (dv.Count == 1 && dv[0]["TABLENAME_AS"].ToString() != "")
                        TableName = dv[0]["TABLENAME_AS"].ToString() + " AS " + TableName;
                    strJoin += " LEFT JOIN " + TableName + " ON 1=1";
                    for (int j = 0; j < dv.Count; j++)
                        if (NVCTable[dv[j]["TABLENAME_FK"].ToString()] == "1")
                            strJoin += "\n" + dv[j]["CONSTRAINT_SQL"].ToString();
                }
            }
            return strJoin;
        }

        private string RetrieveStrField(NameValueCollection nvcField)
        {
            string strField = "";
            for (int i = 0; i < nvcField.Count; i++)
            {
                string strFieldAs = " AS [" + nvcField.Keys[i] + "]";
                strField += nvcField[i] + strFieldAs + ",";
            }
            strField = strField.Substring(0, strField.Length - 1);
            return strField;
        }

        public string Retrieve(NameValueCollection nvcField, string strCond)
        {
            string strField = RetrieveStrField(nvcField);
            return Retrieve(strField, strCond);
        }

        public string Retrieve(string strField, string strCond)
        {
            string strSql = "SELECT " + strField + " " + RetrieveJoinString(RetrieveListTable(strField + " " + strCond)) + " WHERE 1=1 " + strCond;
            return RetrieveLinkedServer(strSql);

        }

        public string Retrieve(string strField, string strCond, bool Show100RecordOnly)
        {

            string strSql = "SELECT top 100 " + strField + " " + RetrieveJoinString(RetrieveListTable(strField + " " + strCond)) + " WHERE 1=1 " + strCond;
            return RetrieveLinkedServer(strSql);

        }

        private string RetrieveLinkedServer(string strSql)
        {
            for (int i = 0; i < dtLinkedServer.Rows.Count; i++)
            {
                string LinkedServID = SeperatorStartLinkedServ + dtLinkedServer.Rows[i]["LinkedServID"].ToString() + SeperatorEndLinkedServ;
                string LinkedServName = dtLinkedServer.Rows[i]["LinkedServName"].ToString();
                strSql = strSql.Replace(LinkedServID, LinkedServName);
            }
            return strSql;
        }

        #endregion
    }
    #endregion

    #region staticFramework
    public class staticFramework
    {
        static public string toSql(object value)
        {
            bool boolTest;
            double dblTest;
            float fltTest;
            decimal decTest;
            if (value == null || (value is string && value.ToString() == ""))
                return "NULL";
            if (value is bool)
                return ((bool)value) ? "1" : "0";
            else if (double.TryParse(value.ToString(), out dblTest) ||
                float.TryParse(value.ToString(), out fltTest) ||
                decimal.TryParse(value.ToString(), out decTest))
                return value.ToString().Replace(",", ".");
            else if (value is DateTime)
                return "'" + ((DateTime)value).ToString("yyyy/MM/dd HH:mm:ss") + "'";
            return "'" + value.ToString().Replace("'", "''") + "'";
        }
    }
    #endregion
}
