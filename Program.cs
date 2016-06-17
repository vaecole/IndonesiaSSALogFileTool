using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace IndonesiaSSALogFileTool
{
    class Program
    {
        public static string appPath = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;
        
        static void Main(string[] args)
        {
            
            FileInfo configFile = new FileInfo(appPath + "\\config.ini");
            if(!configFile.Exists)
            {
                Console.WriteLine("No Config.ini File!");
                return;
            }
            string originalLogsPath = configFile.OpenText().ReadLine(); //@"D:\Guest User\TD Server\Indonesia\XL_LTE_Tuning\SSA Logfile B\";
            new ToolPackage().Copy_Unpack_Rename_Copy_ADP(originalLogsPath);

            new UsageReport().checkIntranet();
            Console.WriteLine("Ericsson @Mark Wu All Rights Reserved.\nContact: mark.wu@ericsson.com");
            new NDOToolTracker().SendToNDOTools("http://ndotools.sinaapp.com/");     
        }
    }
}
