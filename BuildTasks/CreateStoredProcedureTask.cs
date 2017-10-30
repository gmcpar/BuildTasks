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
        private string classFileName;
        public string ClassFileName
        {
            get { return classFileName; }
            set { classFileName = value; }
        }

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
                var ta = t.GetCustomAttribute<TableAttribute>();

                string tableName = ta.Schema + "." + ta.Name;

                string deleteSP = String.Format("{0}.Delete{1}", ta.Schema, ta.Name);
                string insertSP = String.Format("{0}.Insert{1}", ta.Schema, ta.Name);
                string updateSP = String.Format("{0}.Update{1}", ta.Schema, ta.Name);
                string selectSP = String.Format("{0}.Select{1}", ta.Schema, ta.Name);

                var props = t.GetProperties();

                //var columns = String.Join(", ", props.Select(prop => prop.Name + " " + prop.GetCustomAttribute<ColumnAttribute>().TypeName).ToArray());

                var fieldlist = String.Join(", ", props.Select(prop => prop.Name));
                var valuelist = String.Join(", ", props.Select(prop => "@" + prop.Name));
                var paramlist = String.Join(", ", props.Select(prop => "@" + prop.Name + " " + prop.GetCustomAttribute<ColumnAttribute>().TypeName).ToArray());

                if (ta != null)
                {
                    WriteCreateTableScript(tableName, fieldlist, valuelist);
                }

                WriteInsertStoredProcedure(tableName, fieldlist, valuelist, paramlist, insertSP);

            }

            return true;
        }

        private IEnumerable<Type> GetCustomDataTypes(Assembly dll)
        {
            return dll.GetTypes().Where(t => String.Equals(t.Namespace, "DataItem", StringComparison.Ordinal));
        }

        private void WriteCreateTableScript(string tablename, string schemaname, string fields)
        {
            // PropertyInfo[] props = t.GetProperties();
        }

        private void WriteInsertStoredProcedure(string tablename, string fieldlist, string valuelist, string paramlist, string spName)
        {
            string insertStatement = String.Format("INSERT INTO {0} ({1}) VALUES ({2});", tablename, fieldlist, valuelist);

            WriteStoredProcedures(insertStatement, spName, paramlist);

        }
        private void WriteStoredProcedures(string statement, string spName, string paramlist)
        {
            string spTemplate = "SET ANSI_NULLS ON\nGO\nSET QUOTED_IDENTIFIER ON\nGO\nCREATE PROCEDURE {0}\n\t{1}\nAS\nBEGIN\n\tSET NOCOUNT ON;\n\n\t{2}\nEND\nGO";

            string sp = String.Format(spTemplate, spName, paramlist, statement);
            //string deleteStatement = "DELETE FROM {0} WHERE {1}";
            //string updateStatement = "UPDATE {0} SET {1} WHERE {2}";
            //string selectStatement = "SELECT {0} FROM {1} WHERE {2}";

            MessageBox.Show(sp);
        }
    }
}
