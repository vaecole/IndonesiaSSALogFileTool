using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace IndonesiaSSALogFileTool
{
    class LogsTool
    {
        private OtherTool Tool = new OtherTool();
        PCIAndEarfcn pciAndEarfcnTool;
        public LogsTool()
        {
            try//尝试查找站点列表文件，找不到则退出
            {
                File.OpenText(Program.appPath + "\\SitePCIEarfcn.txt");
            }
            catch
            {
                Tool.RecordLog("There is no SitePCIEarfcn.txt in the exe file folder, please check!");
                return;
            }
            pciAndEarfcnTool = new PCIAndEarfcn(Program.appPath + "\\SitePCIEarfcn.txt");
        }
        //ADP改名
        public void RenameAndCopy(string workFolderPath, string copyToPath)
        {
            Tool.RecordLog("Dealing with folder:" + workFolderPath);
            foreach (DirectoryInfo item in new DirectoryInfo(copyToPath).GetDirectories())//删除以前的所有文件夹
            {
                item.Delete(true);
            }
            DirectoryInfo workFolder = new DirectoryInfo(workFolderPath);
            int folderNum = workFolder.GetDirectories().Count();//找出有多少个folder需要处理
            int siteNum = 0;
            int success = 0;//记录处理站点的次数
            DirectoryInfo[] siteFolders = null;//存储所有站点文件夹
            if (folderNum > 0)
                siteFolders = new DirectoryInfo(workFolderPath).GetDirectories();
            string siteFolderName;
            string[] siteItem = new string[3];
            Tool.RecordLog("--------------------------------------");
            for (int i = 0; i < folderNum; i++)//开始处理站点，共有folderNum个folder
            {
                siteFolderName = siteFolders[i].FullName.Substring(siteFolders[i].FullName.LastIndexOf('\\') + 1);
                Tool.RecordLog("Site folder name:" + siteFolderName);
                siteItem = pciAndEarfcnTool.FindEarfcnAndPCIsByFolderName(siteFolderName);//查找频点和PCI
                if (siteItem[0] != "-1")
                {
                    siteNum++;
                    //开始重命名
                    int earfcn = 0, firstPCI = 0;
                    if (int.TryParse(siteItem[2], out firstPCI) && int.TryParse(siteItem[3], out earfcn))
                    {
                        string sitePath = "";
                        DirectoryInfo siteFolder = new DirectoryInfo(workFolderPath + siteFolderName);
                        if (siteFolder.GetDirectories("*LTE*").Count() == 1 && siteFolder.GetDirectories("*sta*").Count() == 0)
                        {
                            siteFolder = siteFolder.GetDirectories("*LTE*")[0];
                            Tool.RecordLog("Warning: Double folder found! Processed!");
                        }
                        if (siteFolder.GetDirectories("*sta*").Count() > 0)
                        {
                            foreach (DirectoryInfo Folder in siteFolder.GetDirectories("*sta*"))
                            {
                                if (Folder.GetDirectories("*sec*").Count() > 0)
                                    sitePath = Folder.FullName;
                                else
                                    Tool.RecordLog("Error: No static or sec1/2/3 folder(s) found.");
                            }
                        }
                        else
                        {
                            if (siteFolder.GetDirectories("*sec*").Count() > 0)
                                sitePath = siteFolder.FullName;
                            else
                                Tool.RecordLog("Error: No static or sec1/2/3 folder(s) found.");
                        }
                        if (sitePath != "")
                        {
                            if (!new DirectoryInfo(copyToPath + siteItem[0] + "\\").Exists)
                                new DirectoryInfo(copyToPath + siteItem[0] + "\\").Create();
                            try
                            {
                                pciAndEarfcnTool.CopyAndRename(sitePath, copyToPath + siteItem[0] + "\\", earfcn, firstPCI);
                            }
                            catch
                            {
                                Tool.RecordLog("Failed: " + siteFolderName);
                            }
                            FileInfoCopyLog.currentSitePath = sitePath + "\\LogNameChange.log";
                            success++;
                            try
                            {
                                Tool.SearchJitterAndSpeedTest(workFolderPath + siteFolderName, workFolderPath + siteFolderName + "\\JitterAndSpTest\\");
                            }
                            catch
                            {
                                Tool.RecordLog("SearchJitterAndSpeedTest Failed!");
                            }
                            FileInfo nameChangeLogFile = new FileInfo(sitePath + "\\LogNameChange.txt");
                            if (nameChangeLogFile.Exists && !new FileInfo(workFolderPath + siteFolderName + "\\LogNameChange.txt").Exists)
                                nameChangeLogFile.MoveTo(workFolderPath + siteFolderName + "\\LogNameChange.txt");
                            try
                            {
                                Tool.Pack_Mobility_Jitter_SpeedTest_LogChange(workFolderPath + siteFolderName);
                            }
                            catch
                            {
                                Tool.RecordLog("Pack_Mobility_Jitter_SpeedTest_LogChange Failed!");
                            }
                            Tool.RecordLog("Success: " + siteFolderName);
                        }
                        else
                        {
                            Tool.RecordLog("Failed: " + siteFolderName);
                        }
                    }//if Earfcn or PCIs error
                }
                else//if No Earfcn or PCIs found!
                {
                    Tool.RecordLog("Failed: " + siteFolderName);
                    Tool.RecordLog("No Earfcn or PCIs found!");
                }
                Tool.RecordLog("--------------------------------------");
            }
            Tool.RecordLog("______________Total Folder:" + folderNum + " Folder(s)|Total Site:" + siteNum + " Site(s)|" + "Total Success:" + success + " site(s)______________");
            new NDOToolTracker().AddSiteNumber(success);
            Tool.RecordLog("");
        }

        public int RenameAndCopy_No_OtherWork(string workFolderPath, string copyToPath, int findWhat = 0)
        {
            DirectoryInfo workFolder = new DirectoryInfo(workFolderPath);
            int folderNum = workFolder.GetDirectories().Count();//找出有多少个folder需要处理
            int siteNum = 0;
            int success = 0;//记录处理站点的次数
            DirectoryInfo[] siteFolders = null;//存储所有站点文件夹
            if (folderNum > 0)
                siteFolders = new DirectoryInfo(workFolderPath).GetDirectories();
            string siteFolderName;
            string[] siteItem = new string[3];
            for (int i = 0; i < folderNum; i++)//开始处理站点，共有folderNum个folder
            {
                siteFolderName = siteFolders[i].FullName.Substring(siteFolders[i].FullName.LastIndexOf('\\') + 1);
                siteItem = pciAndEarfcnTool.FindEarfcnAndPCIsByFolderName(siteFolderName);//查找频点和PCI
                if (siteItem[0] != "-1")
                {
                    siteNum++;
                    //开始重命名
                    int earfcn = 0, firstPCI = 0;
                    if (int.TryParse(siteItem[2], out firstPCI) && int.TryParse(siteItem[3], out earfcn))
                    {
                        string sitePath = "";
                        DirectoryInfo siteFolder = new DirectoryInfo(workFolderPath + siteFolderName);
                        if (siteFolder.GetDirectories("*LTE*").Count() == 1 && siteFolder.GetDirectories("*sta*").Count() == 0)
                        {
                            siteFolder = siteFolder.GetDirectories("*LTE*")[0];
                        }
                        if (siteFolder.GetDirectories("*sta*").Count() > 0)
                        {
                            foreach (DirectoryInfo Folder in siteFolder.GetDirectories("*sta*"))
                            {
                                if (Folder.GetDirectories("*sec*").Count() > 0)
                                    sitePath = Folder.FullName;
                            }
                        }
                        else
                        {
                            if (siteFolder.GetDirectories("*sec*").Count() > 0)
                                sitePath = siteFolder.FullName;
                        }
                        if (sitePath != "")
                        {

                            if (findWhat == 0)
                            {
                                if (!new DirectoryInfo(copyToPath + siteItem[0] + "\\").Exists)
                                    new DirectoryInfo(copyToPath + siteItem[0] + "\\").Create();
                                //pciAndEarfcnTool.CopyAndRename_PING1500(sitePath, copyToPath + siteItem[0] + "\\", earfcn, firstPCI);
                            }
                            if (findWhat == 1)
                            {
                                pciAndEarfcnTool.CopyAndRename_2UE_PING1500(sitePath, copyToPath + "\\", siteItem[0]);
                            }
                            success++;
                        }
                    }//if Earfcn or PCIs error
                }
            }
            new NDOToolTracker().AddSiteNumber(success);
            return success;
        }
    }
}
