using System;
using Microsoft.Build.Utilities;
using System.Reflection;
using System.Diagnostics;

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

        public override bool Execute()
        {
            Type t = Type.GetType(classFileName, true, true);
            PropertyInfo[] props = t.GetProperties();
            foreach(PropertyInfo p in props)
            {
                Debug.Print(p.Name);
            }
            return true;
        }
    }
}
