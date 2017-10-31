using System;
using Microsoft.Build.Utilities;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace BuildTasks
{
    public class CreateStoredProceduresTask : Task
    {
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

            foreach (Type t in customTypes)
            {
                TableAttribute ta = t.GetCustomAttribute<TableAttribute>();
                PropertyInfo[] props = t.GetProperties();

                string key = props.FirstOrDefault(p => p.GetCustomAttributes(false).Any(a => a.GetType() == typeof(KeyAttribute))).Name;
                string keytype = props.Where(p => p.Name == key).First().GetCustomAttribute<ColumnAttribute>().TypeName;


                string tablename = string.Concat(ta.Schema, ".", ta.Name);
                string conditionpart = string.Format("WHERE {0}=@{0}", key);

                //string deleteSP = string.Format("{0}.Delete{1}", ta.Schema, ta.Name);
                //string insertSP = string.Format("{0}.Insert{1}", ta.Schema, ta.Name);
                //string updateSP = string.Format("{0}.Update{1}", ta.Schema, ta.Name);
                //string selectSP = string.Format("{0}.Select{1}", ta.Schema, ta.Name);


                //comma separated list of fields for select statements 
                string selectfieldlist = string.Join(", ", props.Select(prop => prop.Name));
                string selectStatement = string.Format("SELECT {0} FROM {1} {2}", selectfieldlist, tablename, conditionpart);
                string paramlist = string.Concat(key, " ", keytype);
                WriteStoredProcedure(selectStatement, "SelectSP", "xxx");

                //
                string insertFieldList = string.Join(", ", props.Where(p => p.Name != key).Select(prop => prop.Name));
                string valuelist = string.Join(", ", props.Select(prop => string.Concat("@", prop.Name)));
                string insertStatement = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tablename, insertFieldList, valuelist);
                WriteStoredProcedure(insertStatement, "InsertSP", "xxx");
                
                //
                string deleteStatement = string.Format("DELETE FROM {0} WHERE {1}", tablename, conditionpart);
                WriteStoredProcedure(deleteStatement, "DeleteSP", "xxx");

                //
                var updatelist = string.Join(",", props.Select(prop => string.Concat(prop.Name, "=@", prop.Name)));
                string updateStatement = string.Format("UPDATE {0} SET {1} WHERE {2}", tablename, updatelist, conditionpart);
                WriteStoredProcedure(updateStatement, "UpdateSP", "xxx");

                //var paramlist = string.Join(", ", props.Select(prop => string.Concat("@", prop.Name) + " " + prop.GetCustomAttribute<ColumnAttribute>().TypeName).ToArray());
                //var valuelist = string.Join(", ", props.Select(prop => string.Concat("@", prop.Name)));
                // var paramlist = string.Join(", ", props.Select(prop => string.Concat("@", prop.Name) + " " + prop.GetCustomAttribute<ColumnAttribute>().TypeName).ToArray());

                

                MessageBox.Show(updatelist);

                if (ta != null)
                {
                    //WriteCreateTableScript(tableName, fieldlist, valuelist, key);
                }

                //WriteInsertStoredProcedure(tableName, fieldlist, valuelist, paramlist, insertSP, key, updatelist);
               
            }

            return true;
        }

        private IEnumerable<Type> GetCustomDataTypes(Assembly dll)
        {
            return dll.GetTypes().Where(t => String.Equals(t.Namespace, "DataItem", StringComparison.Ordinal));
        }

        private void WriteCreateTableScript(string tablename, string schemaname, string fields, string key)
        {
            // PropertyInfo[] props = t.GetProperties();
        }

        //private void WriteInsertStoredProcedure(string tablename, string fieldlist, string valuelist, string paramlist, string spName, string key, string updatelist)
        //{
        //    string insertStatement = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tablename, fieldlist, valuelist);
        //    WriteStoredProcedure(insertStatement, spName, paramlist);

        //    string condition = string.Format("{0}=@{0}", key);


        //    string deleteStatement = string.Format("DELETE FROM {0} WHERE {1}", tablename, condition);
        //    WriteStoredProcedure(deleteStatement, "Delete", paramlist);

        //    string selectStatement = string.Format("SELECT {0} FROM {1} WHERE {2}", fieldlist, tablename, condition);
        //    WriteStoredProcedure(selectStatement, "Select", paramlist);


        //    string updateStatement = string.Format("UPDATE {0} SET {1} WHERE {2}", tablename, updatelist, condition);
        //    WriteStoredProcedure(updateStatement, "Update", updatelist);


        //}
        private void WriteStoredProcedure(string sqlstatement, string spname, string paramlist)
        {
            string sptemplate = "SET ANSI_NULLS ON\nGO\nSET QUOTED_IDENTIFIER ON\nGO\nCREATE PROCEDURE {0}\n\t({1})\nAS\nBEGIN\n\tSET NOCOUNT ON;\n\n\t{2}\nEND\nGO";
            string sp = String.Format(sptemplate, spname, paramlist, sqlstatement);
            MessageBox.Show(sp);
        }
    }
}
