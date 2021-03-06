﻿using System;
using Microsoft.Build.Utilities;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Text;

namespace BuildTasks
{
    public class CreateStoredProceduresTask : Task
    {
        private Database db;
        private Server srv;

        private string dllName;
        public string DllName
        {
            get { return dllName; }
            set { dllName = value; }
        }
        public override bool Execute()
        {
            //using Load method in order to not lock the dll
            Assembly dll = Assembly.Load(System.IO.File.ReadAllBytes(dllName));

            var customTypes = GetCustomDataTypes(dll);

            srv = new Server("dv-dsd-adtsql01");
            srv.ConnectionContext.LoginSecure = true;
            db = srv.Databases["db1"];

            //WriteStoredProcedure2(db, "SMOCreate", "dbo");

            foreach (Type t in customTypes)
            {
                
                TableAttribute ta = t.GetCustomAttribute<TableAttribute>();
                PropertyInfo[] props = t.GetProperties();


                PropertyInfo key = props.Where(prop => prop.IsDefined(typeof(KeyAttribute), false)).FirstOrDefault<PropertyInfo>();

                string keyname = key.GetCustomAttribute<ColumnAttribute>().Name;
                string keytype = key.GetCustomAttribute<ColumnAttribute>().TypeName;

                string paramstring = string.Concat(keyname, " ", keytype);

                

                string schema = ta.Schema;
                string tablename = ta.Name;

                //Convert.ChangeType();


                string allfields = string.Join(", ", props.Select(prop => prop.Name));


                StringBuilder sb = new StringBuilder("SELECT ");
                sb.Append(allfields);
                sb.Append(" FROM ");
                sb.AppendFormat("[{0}].[{1}] ", schema, tablename);
                sb.AppendFormat("WHERE {0}=@{0}", keyname);


                string s = sb.ToString();
                MessageBox.Show(s);
                GenerateStoredProcedure("newSP", schema, s, props);



                //string key = props.FirstOrDefault(p => p.GetCustomAttributes(false).Any(a => a.GetType() == typeof(KeyAttribute))).Name;
                //string keytype = props.Where(p => p.Name == key).First().GetCustomAttribute<ColumnAttribute>().TypeName;
                //string keyparam = string.Concat("(", key, " ", keytype, ")");

                //string tablename = string.Concat(ta.Schema, ".", ta.Name);
                //string conditionpart = string.Format("WHERE {0}=@{0}", key);

                ////comma separated list of fields for select statements 
                //string selectfieldlist = string.Join(", ", props.Select(prop => prop.Name));
                //string selectStatement = string.Format("SELECT {0} FROM {1} {2}", selectfieldlist, tablename, conditionpart);
                //string paramlist = string.Concat(key, " ", keytype);
                //WriteStoredProcedure(selectStatement, "SelectSP", keyparam);

                ////
                //string insertFieldList = string.Join(", ", props.Where(p => p.Name != key).Select(prop => prop.Name));
                //string valuelist = string.Join(", ", props.Select(prop => string.Concat("@", prop.Name)));
                //string insertStatement = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tablename, insertFieldList, valuelist);
                //WriteStoredProcedure(insertStatement, "InsertSP", "xxx");

                ////
                //string deleteStatement = string.Format("DELETE FROM {0} WHERE {1}", tablename, conditionpart);
                //WriteStoredProcedure(deleteStatement, "DeleteSP", keyparam);

                ////
                //var updatelist = string.Join(",", props.Select(prop => string.Concat(prop.Name, "=@", prop.Name)));
                //string updateStatement = string.Format("UPDATE {0} SET {1} WHERE {2}", tablename, updatelist, conditionpart);
                //WriteStoredProcedure(updateStatement, "UpdateSP", "xxx");

                //MessageBox.Show(updatelist);

                if (ta != null)
                {
                    //WriteCreateTableScript(tableName, fieldlist, valuelist, key);
                }              
            }
            return true;
        }

        private void GenerateStoredProcedure(string spname, string schema, string sqlstatement, PropertyInfo[] prps)
        {
            StoredProcedure sp = new StoredProcedure(db, spname, schema);

            sp.TextMode = false;
            sp.AnsiNullsStatus = true;
            sp.QuotedIdentifierStatus = false;

            foreach(PropertyInfo p in prps)
            {
                StoredProcedureParameter param = new StoredProcedureParameter(sp, "@bbb", Microsoft.SqlServer.Management.Smo.DataType.Int);
            }



            sp.Parameters.Add(param);

            sp.TextBody = sqlstatement;
            sp.Create();

        }

        private IEnumerable<Type> GetCustomDataTypes(Assembly dll)
        {
            return dll.GetTypes().Where(t => String.Equals(t.Namespace, "DataItem", StringComparison.Ordinal));
        }

        private void WriteCreateTableScript(string tablename, string schemaname, string fields, string key)
        {          
        }

        private void WriteStoredProcedure(string sqlstatement, string spname, string paramlist)
        {
            string sptemplate = "SET ANSI_NULLS ON\nGO\nSET QUOTED_IDENTIFIER ON\nGO\nCREATE PROCEDURE {0}\n\t{1}\nAS\nBEGIN\n\tSET NOCOUNT ON;\n\n\t{2}\nEND\nGO";
            string sp = String.Format(sptemplate, spname, paramlist, sqlstatement);
            MessageBox.Show(sp);
        }

        private void WriteStoredProcedurex(string schema, string spname, string sqlstatement)
        {
            

            
        }
    }
}
