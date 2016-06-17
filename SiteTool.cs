using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndonesiaSSALogFileTool
{
    class SiteTool
    {
        private OtherTool Tool = new OtherTool();

        private PCIAndEarfcn pciAndEarfcnTool = new PCIAndEarfcn(Program.appPath + "\\SitePCIEarfcn.txt");

        public string CopyAndUnpack(string originalLogsPath, int when_day = -1)
        {
            DateTime yesterday = DateTime.Now.Add(TimeSpan.FromDays(when_day));
            DateTime yeyeday = DateTime.Now.Add(TimeSpan.FromDays(when_day - 1));
            string dateName = yesterday.Year + (yesterday.Month < 10 ? ("0" + yesterday.Month) : yesterday.Month.ToString()) + (yesterday.Day < 10 ? ("0" + yesterday.Day) : yesterday.Day.ToString());
            string logsFolderName = "SSA " + dateName;
            originalLogsPath = originalLogsPath + logsFolderName + "\\";

            string workPath = Program.appPath;
            //拷贝
            DirectoryInfo destinationFolder = new DirectoryInfo(workPath + @"\SSA Logs And Reports\" + dateName + "\\");
            if (!destinationFolder.Exists)
            {
                destinationFolder = Directory.CreateDirectory(workPath + @"\SSA Logs And Reports\" + dateName + "\\");
            }
            try
            {
                DirectoryInfo ADPReportsFolder = new DirectoryInfo(workPath + @"\ADP Reports\");
                DirectoryInfo oldReportsFolder = new DirectoryInfo(destinationFolder.Parent.FullName + "\\" + yeyeday.Year + (yeyeday.Month < 10 ? ("0" + yeyeday.Month) : yeyeday.Month.ToString()) + (yeyeday.Day < 10 ? ("0" + yeyeday.Day) : (yesterday.Day - 1).ToString()) + "\\Reports\\");
                oldReportsFolder.Create();//创建存放old reports的目录
                foreach (DirectoryInfo item in ADPReportsFolder.GetDirectories())//将以前的report移动至log文件夹
                {
                    item.MoveTo(oldReportsFolder.FullName + item.Name);
                }
            }
            catch
            {
                //部分文件夹不可移动
            }
            DirectoryInfo originalLogsFolder = new DirectoryInfo(originalLogsPath);
            if (originalLogsFolder.Exists && originalLogsFolder.GetFiles().Count() > 0)
            {
                foreach (FileInfo siteLog in originalLogsFolder.GetFiles("*.rar"))
                {
                    Console.WriteLine("Copying " + siteLog.Name);
                    siteLog.CopyTo(destinationFolder.FullName + siteLog.Name, true);
                }
                foreach (FileInfo siteLog in originalLogsFolder.GetFiles("*.zip"))
                {
                    Console.WriteLine("Copying " + siteLog.Name);
                    siteLog.CopyTo(destinationFolder.FullName + siteLog.Name, true);
                }
                foreach (DirectoryInfo siteLog in originalLogsFolder.GetDirectories())
                {
                    Console.WriteLine("Copying " + siteLog.Name);
                    //siteLog.(destinationFolder.FullName + siteLog.Name, true);
                }
                //创建解压脚本
                TextWriter batFile = File.CreateText(destinationFolder.FullName + "LogAutoProcess.bat");
                FileInfo WinRarPath = new FileInfo(System.Environment.GetEnvironmentVariable("ProgramFiles") + "\\WinRAR\\WinRAR.exe");
                if (!WinRarPath.Exists)
                {
                    WinRarPath = new FileInfo(@"C:\Program Files\WinRAR\WinRar.exe");
                    if (!WinRarPath.Exists)
                    {
                        WinRarPath = new FileInfo(@".\WinRar\WinRar.exe");
                        if (!WinRarPath.Exists)
                        {
                            Console.WriteLine("No Winrar found!");
                            batFile.WriteLine("pause");
                        }
                    }
                }
                batFile.WriteLine("\"" + WinRarPath.FullName + "\"" + " x \"" + destinationFolder.FullName + "*.rar\" \"" + destinationFolder.FullName + "\" -y");
                batFile.WriteLine("\"" + WinRarPath.FullName + "\"" + " x \"" + destinationFolder.FullName + "*.zip\" \"" + destinationFolder.FullName + "\" -y");
                batFile.Close();
                Console.WriteLine("Unpacking...");
                Tool.RunBat(destinationFolder.FullName + "LogAutoProcess.bat");
                Console.WriteLine("Unpack Success...");
                return destinationFolder.FullName;
            }
            else
            {
                Tool.RecordLog("No Log found in the SSA Folder:" + originalLogsPath, destinationFolder.FullName + "error.txt");
                Tool.RecordLog("No Log found in the SSA Folder:" + originalLogsPath);
                return "";
            }
        }

        public string CreateDownloadAndUnpackBat(string savePath = @"./LogAutoProcess.bat", string winscpPath = @"./WinSCP/WinSCP", int when_day = -1, string winrarPath = @"./WinRar/WinRar")
        {

            DateTime yesterday = DateTime.Now.Add(TimeSpan.FromDays(when_day));
            string dateName = (yesterday.Day < 10 ? ("0" + yesterday.Day) : yesterday.Day.ToString()) +
                (yesterday.Month < 10 ? ("0" + yesterday.Month) : yesterday.Month.ToString()) + yesterday.Year;
            string logsFolderName = "SSA " + dateName;

            string applicationFolder = new DirectoryInfo(@"./").FullName;

            //创建WinSCP脚本
            string winSCPbatchPath = @"./SSA_download.batch";
            TextWriter winSCPbatch = File.CreateText(winSCPbatchPath);
            winSCPbatch.WriteLine("option batch continue");
            winSCPbatch.WriteLine("option confirm off");
            winSCPbatch.WriteLine("open ftp://Indonesia_LTE:indonesia@146.11.39.250:21");
            winSCPbatch.WriteLine("cd \"/XL_LTE_Tuning/SSA Logfile/" + logsFolderName + "\"");
            winSCPbatch.WriteLine("mget * " + applicationFolder + " -neweronly");
            winSCPbatch.WriteLine("exit");
            winSCPbatch.Close();

            //创建下载和解压脚本
            TextWriter batFile = File.CreateText(savePath);
            batFile.WriteLine("\"" + new FileInfo(winscpPath).FullName + "\"" + " /script=" + winSCPbatchPath + " /log=./winscp.log");
            batFile.WriteLine("\"" + new FileInfo(winrarPath).FullName + "\"" + @" x " + applicationFolder + "*.rar " + applicationFolder + " -y");
            batFile.WriteLine("exit");
            batFile.Close();

            return new DirectoryInfo(savePath).FullName;
        }

        public void RenameInCheckList(string logsFilePath = @"D:\Guest User\TD Server\Indonesia\XL_LTE_Tuning\SSA Logfile B\", string unpackPath = @"\AllFound\", string CopyToPath = @"\")
        {
            TextReader checklistFile;
            string siteItem;
            List<string> listSites = new List<string>(10000);
            checklistFile = File.OpenText(Program.appPath + "\\SSACheckList.txt");
            while (checklistFile.Peek() != -1)
            {
                siteItem = checklistFile.ReadLine().ToUpper();
                listSites.Add(siteItem);
            }
            string workPath = Program.appPath;
            int findSuccessCount = 0;
            int totalSuccessCount = 0;
            LogsTool LogsTool = new LogsTool();
            if (new DirectoryInfo(workPath + CopyToPath).Exists == false)
            {
                new DirectoryInfo(workPath + CopyToPath).Create();
            }
            if (new DirectoryInfo(workPath + unpackPath).Exists == false)
            {
                new DirectoryInfo(workPath + unpackPath).Create();
            }
            DirectoryInfo SSALogFolder = new DirectoryInfo(logsFilePath);
            string[] foundSiteItem = new string[4];
            List<FileInfo> packageItems = SSALogFolder.EnumerateFiles("*.rar", SearchOption.AllDirectories).ToList<FileInfo>();
            packageItems.Concat(SSALogFolder.EnumerateFiles("*.zip", SearchOption.AllDirectories).ToList<FileInfo>());
            foreach (FileInfo packageItem in packageItems)
            {
                foundSiteItem = pciAndEarfcnTool.FindEarfcnAndPCIsByFolderName(packageItem.Name);
                if (foundSiteItem[0] != "-1")
                {
                    foreach (string listItem in listSites)//此站在checklist中
                    {
                        if (listItem == foundSiteItem[0])
                        {
                            Console.WriteLine("Found:" + packageItem.FullName);
                            Tool.UnpackOne(packageItem.FullName, workPath + unpackPath);
                            findSuccessCount++;
                            break;
                        }
                    }
                }
            }
            totalSuccessCount += LogsTool.RenameAndCopy_No_OtherWork(workPath + unpackPath, workPath + CopyToPath, 1);
            Tool.RecordLog("______________Site Founded:" + findSuccessCount + "Success:" + totalSuccessCount + " site(s)______________", workPath + @"\FoundSummary.txt");
        }

        public void RenameAllSite(string logsFilePath)
        {
            string workPath = Program.appPath;
            int totalPackageCount = 0;
            int totalSuccessCount = 0;
            int currentPackageCount = 0;
            int currentSuccessCount = 0;
            LogsTool LogsTool = new LogsTool();
            if (new DirectoryInfo(workPath + @"\CSFB_PING1500_Processing\").Exists == false)
            {
                new DirectoryInfo(workPath + @"\CSFB_PING1500_Processing\").Create();
            }
            if (new DirectoryInfo(workPath + @"\CSFB_PING1500\").Exists == false)
            {
                new DirectoryInfo(workPath + @"\CSFB_PING1500\").Create();
            }
            DirectoryInfo[] SSALogFolders = new DirectoryInfo(logsFilePath).GetDirectories();
            foreach (DirectoryInfo dateFolder in SSALogFolders)
            {
                currentPackageCount = dateFolder.GetFiles().Count();
                Tool.Unpack(dateFolder.FullName + "\\", workPath + @"\CSFB_PING1500\");
                foreach (DirectoryInfo innerItem in dateFolder.GetDirectories())
                {
                    currentPackageCount += innerItem.GetFiles().Count();
                    Tool.Unpack(innerItem.FullName + "\\", workPath + @"\CSFB_PING1500\");
                }
                currentSuccessCount = LogsTool.RenameAndCopy_No_OtherWork(workPath + @"\CSFB_PING1500\", workPath + @"\CSFB_PING1500_Processing\");
                totalSuccessCount += currentSuccessCount;
                totalPackageCount += currentPackageCount;
                Tool.RecordLog("______________Folder:" + dateFolder.Name + " |Package:" + currentPackageCount + " |" + "Success:" + currentSuccessCount + " site(s)______________");
                currentPackageCount = 0;
                currentSuccessCount = 0;
            }
            Tool.RecordLog("$$$$$$$$$$$$$$$$$Total Package:" + totalPackageCount + " |Total Succuss:" + totalSuccessCount + " $$$$$$$$$$$$$$$$$$");

        }

        private void CopyDir(string srcPath, string aimPath)//拷贝文件夹
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加之 
                if (aimPath[aimPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    aimPath += System.IO.Path.DirectorySeparatorChar;
                }

                // 判断目标目录是否存在如果不存在则新建之 
                if (!System.IO.Directory.Exists(aimPath))
                {
                    System.IO.Directory.CreateDirectory(aimPath);
                }

                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组 
                // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法 
                // string[] fileList = Directory.GetFiles(srcPath); 
                string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);

                // 遍历所有的文件和目录 
                foreach (string file in fileList)
                {
                    // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件 
                    if (System.IO.Directory.Exists(file))
                    {
                        CopyDir(file, aimPath + System.IO.Path.GetFileName(file));
                    }

                    // 否则直接Copy文件 
                    else
                    {
                        System.IO.File.Copy(file, aimPath + System.IO.Path.GetFileName(file), true);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

    }
}
