using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndonesiaSSALogFileTool
{
    class ToolPackage
    {
        SiteTool siteTool = new SiteTool();
        public void Copy_Unpack_Rename_Copy_ADP(string riginalLogsPath)
        {
            string newSitePath = "";
            newSitePath = siteTool.CopyAndUnpack(riginalLogsPath);
            if (newSitePath == "")
            {
                Console.WriteLine("Copy Failed, please check log folder!");
                return;
            }
            DirectoryInfo newSitesFolder = new DirectoryInfo(newSitePath);
            DirectoryInfo copyToFolder = new DirectoryInfo(newSitesFolder.Parent.Parent.FullName + @"\Processing\");
            if (copyToFolder.Exists == false)
                copyToFolder.Create();
            new LogsTool().RenameAndCopy(newSitesFolder.FullName, copyToFolder.FullName);
        }

        public void Rename_Copy()
        {
            string destinationPath = Program.appPath + @".\Processed Logs\";
            DirectoryInfo destinationFolder = new DirectoryInfo(destinationPath);
            if (destinationFolder.Exists == false)
                destinationFolder.Create();
            new LogsTool().RenameAndCopy(Program.appPath + "\\", destinationFolder.FullName + "\\");
        }

        public void RenameInCheckList(string logFilePath = @"D:\Guest User\TD Server\Indonesia\XL_LTE_Tuning\")
        {
            siteTool.RenameInCheckList(logFilePath + @"SSA\", @"\AllFound\SSA\", "");//,@"\SSA_PING1500\",@"\SSA_PING1500_Processing\");
            siteTool.RenameInCheckList(logFilePath + @"SSA Logfile\", @"\AllFound\SSA Logfile\", "");
            siteTool.RenameInCheckList(logFilePath + @"SSA Logfile B\", @"\AllFound\SSA Logfile B\", "");
        }

    }
}
