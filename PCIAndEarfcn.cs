using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IndonesiaSSALogFileTool
{
    class PCIAndEarfcn
    {
        private TextReader listFile;//站点列表文件
        private string[] siteItem;//存储找到的站点，频点和PCI
        private List<string[]> allSites = new List<string[]>(6000);//将站点列表装载进内存，方便查找
        private OtherTool Tool = new OtherTool();
        //构造函数
        public PCIAndEarfcn()
        {
        }

        public PCIAndEarfcn(string filepath)
        {
            try
            {
                listFile = File.OpenText(filepath);
                siteItem = new string[4];
                while (listFile.Peek() != -1)
                {
                    siteItem = listFile.ReadLine().ToUpper().Split('\t');
                    allSites.Add(siteItem);
                }
            }
            catch
            {
                Tool.RecordLog("Config File Error!");
                return;
            }
        }

        //根据文件夹名猜测站号
        public string findSiteID(string dirName)
        {
            string siteID = "";
            if (dirName.Contains("SH") && dirName.IndexOf("SH") < dirName.IndexOf("E_"))//SH开头的站点
            {
                siteID = dirName.Substring(dirName.IndexOf("SH"), dirName.IndexOf("E_") - dirName.IndexOf("SH") + 1);
            }
            else
                if (dirName.Contains("MC") && dirName.IndexOf("MC") < dirName.IndexOf("E_"))//MC开头的站点
                {
                    siteID = dirName.Substring(dirName.IndexOf("MC"), dirName.IndexOf("E_") - dirName.IndexOf("MC") + 1);
                }
                else
                {
                    char[] item = dirName.ToArray();
                    int numIndex = -1;
                    for (int i = 0; numIndex == -1 && i < item.Count(); i++)//找到第一个数字的索引存入numIndex
                    {
                        if (item[i] <= '9' && item[i] >= '0')
                            numIndex = i;
                    }
                    if (numIndex != -1 && numIndex < dirName.IndexOf("E_"))
                    {
                        siteID = dirName.Substring(numIndex, dirName.IndexOf("E_") - numIndex + 1);
                    }
                }
            if (siteID.Contains("_"))
                siteID = siteID.Substring(siteID.LastIndexOf("_") + 1);
            if (siteID.Contains("-"))
                siteID = siteID.Substring(0, siteID.LastIndexOf("-"));
            if (siteID.Length < 7)
                siteID = "";
            return siteID;
        }

        //根据站号查找频点和PCI，以字符串数组返回
        public string[] FindEarfcnAndPCIs(string SiteID)
        {
            foreach (string[] item in allSites)
            {
                if (item[0] == SiteID)
                {
                    return item;
                }
            }
            return new string[] { "-1", "-1", "-1" };
        }

        //根据文件夹猜测站的频点和PCI，若失败再猜测站号，并用站号再查找一遍，成功则以字符串数组返回，失败返回-1，-1，-1
        public string[] FindEarfcnAndPCIsByFolderName(string siteFolderName)
        {
            siteFolderName = siteFolderName.ToUpper();
            foreach (string[] item in allSites)
            {
                if (siteFolderName.Contains(item[0]))//包含站号
                {
                    return item;
                }
            }
            string siteID = findSiteID(siteFolderName);
            string siteName = "";
            if (siteFolderName.Contains("("))
            {
                try
                {
                    siteName = siteFolderName.Substring(siteFolderName.LastIndexOf(siteID) + siteID.Length + 1, siteFolderName.LastIndexOf("(") - siteFolderName.LastIndexOf(siteID) + siteID.Length);
                }
                catch
                {
                }
            }
            int nameLength = 0, itemIndex = 0;
            for (int i = 0; i < allSites.Count; i++)
            {
                siteFolderName = siteFolderName.Replace('_', ' ').ToUpper();
                allSites[i][1] = allSites[i][1].Replace('_', ' ').ToUpper();
                if (siteFolderName.Contains(allSites[i][1]))
                {
                    if (allSites[i][1].Length > nameLength)
                    {
                        nameLength = allSites[i][1].Length;
                        itemIndex = i;
                    }
                }
            }
            if (nameLength > 0)
                return allSites[itemIndex];
            return FindEarfcnAndPCIs(siteID);
        }

        //拷贝和重命名，格式：earfcn_PCI_关键字_sec1/2/3.trp
        public void CopyAndRename(string sitePath, string copyToPath, int earfcn = 0, int firstPCI = 0)
        {
            DirectoryInfo siteFolder = new DirectoryInfo(sitePath);
            DirectoryInfo[] secFolders = siteFolder.GetDirectories("*sec*");
            DirectoryInfo newSiteFolder = new DirectoryInfo(copyToPath);

            if (!newSiteFolder.Exists)
                newSiteFolder.Create();
            int[] PCIs = { 0, 8, 16 };
            string[] Sector = { "A", "B", "C" };
            PCIs[0] = firstPCI;
            PCIs[1] = PCIs[0] + 8;
            PCIs[2] = PCIs[1] + 8;
            string[] keyWords = { "CSFB", "AttachDetach", "FTP","TwoUE", "PING" };
            string[] searchPatterns = { "*CSFB*", "*Atta*", "*FTP*DL*","*Iperf*2*ue*", "*PING*" };

            if (secFolders.Count() == 0)
            {
                Console.WriteLine("No Sector Folder been found!!");
            }
            else
            {
                int secNum = 0;//sec索引号
                foreach (DirectoryInfo secFolder in secFolders)
                {
                    int.TryParse(secFolder.Name.Substring(secFolder.Name.Length - 1), out secNum);//从sec文件夹名获取这是第几个sector
                    secNum--;
                    DirectoryInfo keyFolder;
                    int keyNum = 0; //关键字索引号
                    foreach (string currentPattern in searchPatterns)
                    {
                        if (secFolder.GetDirectories(currentPattern).Count() > 0)
                        {
                            FileInfo logFile;
                            keyFolder = secFolder.GetDirectories(currentPattern)[0];
                            if (keyNum == 0)//若是CSFB，判断是否有spot文件夹
                            {
                                int spotNum = keyFolder.GetDirectories().Count();
                                DirectoryInfo[] spotFolders;
                                if (spotNum > 0)
                                {
                                    spotFolders = keyFolder.GetDirectories();
                                    for (int i = 0; i < spotNum && spotFolders[i].GetFiles("*.trp").Count() > 0; i++)//如果有spot文件夹，进入拷贝出来
                                    {
                                        Console.WriteLine("Found CSFB Spot " + i + " folder, handling...success");
                                        logFile = spotFolders[i].GetFiles("*.trp")[0];
                                        logFile.CopyTo(keyFolder.FullName + "\\" + logFile.Name + i + ".trp", true);
                                    }
                                }
                            }
                            if (keyFolder.GetFiles("*.trp").Count() > 0 && keyNum < 4)//处理CSFB, ATTACH, FTP, 2UE
                            {
                                if (keyNum == 0)//若是CSFB，尝试拷贝多个文件，加后缀ABCD...
                                {
                                    int csfbNum = keyFolder.GetFiles("*.trp").Count();
                                    char postfix = 'A';
                                    for (int i = 0; i < csfbNum; i++)
                                    {
                                        logFile = keyFolder.GetFiles("*.trp")[i];
                                        logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_" + Convert.ToChar(postfix + i) + ".trp", true);
                                        Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_" + Convert.ToChar(postfix + i) + ".trp", sitePath + "\\LogNameChange.txt");
                                        Console.WriteLine(keyWords[keyNum] + Convert.ToChar(postfix + i) + " Log...Success!");
                                    }
                                }
                                else
                                {
                                    logFile = keyFolder.GetFiles("*.trp")[0];
                                    logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + ".trp", true);
                                    Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + ".trp", sitePath + "\\LogNameChange.txt");
                                    Console.WriteLine(keyWords[keyNum] + " Log...Success!");
                                }
                            }
                            else//处理PING32, PING1000, PING1500
                            {
                                PingHandler(sitePath, copyToPath, earfcn, PCIs, Sector, keyWords, secNum, keyFolder, keyNum,"32","_A.trp");
                                PingHandler(sitePath, copyToPath, earfcn, PCIs, Sector, keyWords, secNum, keyFolder, keyNum, "1000", "_B.trp");
                                PingHandler(sitePath, copyToPath, earfcn, PCIs, Sector, keyWords, secNum, keyFolder, keyNum, "1500", "_C.trp");

                                //if (keyFolder.GetDirectories("*1000*").Count() > 0)
                                //{
                                //    DirectoryInfo pingFolder = keyFolder.GetDirectories("*PING*1000*")[0];
                                //    if (pingFolder.GetFiles("*.trp").Count() > 0)
                                //    {
                                //        logFile = pingFolder.GetFiles("*.trp")[0];
                                //        logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_B.trp", true);
                                //        Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_B.trp", sitePath + "\\LogNameChange.txt");
                                //        Console.WriteLine("PING1000 Log...Success!");
                                //    }
                                //    else//没有PING1000文件夹尝试直接拷贝Ping1000 log
                                //    {
                                //        Console.WriteLine("No PING1000 folder found! Try PING1000 log File directly...");
                                //        if (keyFolder.GetFiles("*PING*1000*.trp").Count() > 0)
                                //        {
                                //            logFile = keyFolder.GetFiles("*PING*1000*.trp")[0];
                                //            logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_B.trp", true);
                                //            Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_B.trp", sitePath + "\\LogNameChange.txt");
                                //            Console.WriteLine("PING1000 Log...Success!");
                                //        }
                                //    }
                                //}
                                //else//没有PING1000文件夹尝试直接拷贝Ping1000 log
                                //{
                                //    Console.WriteLine("No PING1000 folder found! Try PING1000 log File directly...");
                                //    if (keyFolder.GetFiles("*PING*1000*.trp").Count() > 0)
                                //    {
                                //        logFile = keyFolder.GetFiles("*PING*1000*.trp")[0];
                                //        logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_B.trp", true);
                                //        Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_B.trp", sitePath + "\\LogNameChange.txt");
                                //        Console.WriteLine("PING1000 Log...Success!");
                                //    }
                                //}
                                //if (keyFolder.GetDirectories("*1500*").Count() > 0)
                                //{
                                //    DirectoryInfo pingFolder = keyFolder.GetDirectories("*1500*")[0];
                                //    if (pingFolder.GetFiles("*.trp").Count() > 0)
                                //    {
                                //        logFile = pingFolder.GetFiles("*.trp")[0];
                                //        logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_C.trp", true);
                                //        Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_C.trp", sitePath + "\\LogNameChange.txt");
                                //        Console.WriteLine("PING1500 Log...Success!");
                                //    }
                                //    else//没有PING1500文件夹尝试直接拷贝Ping1500 log
                                //    {
                                //        Console.WriteLine("No PING1500 folder found! Try PING1000 log File directly...");
                                //        if (keyFolder.GetFiles("*1500*.trp").Count() > 0)
                                //        {
                                //            logFile = keyFolder.GetFiles("*1500*.trp")[0];
                                //            logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_C.trp", true);
                                //            Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_C.trp", sitePath + "\\LogNameChange.txt");
                                //            Console.WriteLine("PING1500 Log...Success!");
                                //        }
                                //    }
                                //}
                                //else//没有PING1500文件夹尝试直接拷贝Ping1500 log
                                //{
                                //    Console.WriteLine("No PING1500 folder found! Try PING1000 log File directly...");
                                //    if (keyFolder.GetFiles("*1500*.trp").Count() > 0)
                                //    {
                                //        logFile = keyFolder.GetFiles("*1500*.trp")[0];
                                //        logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_C.trp", true);
                                //        Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + "_sec" + Sector[secNum] + "_C.trp", sitePath + "\\LogNameChange.txt");
                                //        Console.WriteLine("PING1500 Log...Success!");
                                //    }
                                //}
                            }
                        }
                        else
                        {
                            Console.WriteLine("No " + keyWords[keyNum] + " folder found!");
                        }
                        keyNum++;
                    }
                }
            }

        }

        private void PingHandler(string sitePath, string copyToPath, int earfcn, int[] PCIs, string[] Sector, string[] keyWords, int secNum, DirectoryInfo keyFolder, int keyNum,string pingType,string postfix)
        {
            FileInfo logFile;
            const string _sec = "_sec";
            if (keyFolder.GetDirectories("*"+pingType+"*").Count() > 0)
            {
                DirectoryInfo pingFolder = keyFolder.GetDirectories("*PING*"+pingType+"*")[0];
                if (pingFolder.GetFiles("*.trp").Count() > 0)
                {
                    logFile = pingFolder.GetFiles("*.trp")[0];
                    logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + _sec + Sector[secNum] + postfix, true);
                    Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + _sec + Sector[secNum] + postfix, sitePath + "\\LogNameChange.txt");
                    Console.WriteLine("PING"+pingType+" Log...Success!");
                }
                else//文件夹中没有PING"+pingType+"文件尝试直接拷贝Ping"+pingType+" log
                {
                    Console.WriteLine("No PING"+pingType+" folder found! Try PING"+pingType+" log File directly...");
                    if (keyFolder.GetFiles("*"+pingType+"*.trp").Count() > 0)
                    {
                        logFile = keyFolder.GetFiles("*PING*"+pingType+"*.trp")[0];
                        logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + _sec + Sector[secNum] + postfix, true);
                        Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + _sec + Sector[secNum] + postfix, sitePath + "\\LogNameChange.txt");
                        Console.WriteLine("PING"+pingType+" Log...Success!");
                    }
                }
            }
            else//没有PING"+pingType+"文件夹尝试直接拷贝Ping"+pingType+" log
            {
                Console.WriteLine("No PING"+pingType+" folder found! Try PING"+pingType+" log File directly...");
                if (keyFolder.GetFiles("*"+pingType+"*.trp").Count() > 0)
                {
                    logFile = keyFolder.GetFiles("*PING*"+pingType+"*.trp")[0];
                    logFile.CopyTo(copyToPath + earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + _sec + Sector[secNum] + postfix, true);
                    Tool.RecordLogNameChange(logFile.Name, earfcn + "_" + PCIs[secNum] + keyWords[keyNum] + _sec + Sector[secNum] + postfix, sitePath + "\\LogNameChange.txt");
                    Console.WriteLine("PING"+pingType+" Log...Success!");
                }
            }
        }

        public void CopyAndRename_2UE_PING1500(string sitePath, string copytopath, string siteID)
        {
            if (new DirectoryInfo(copytopath + "2UE\\").Exists == false)
                new DirectoryInfo(copytopath + "2UE\\").Create();
            if (new DirectoryInfo(copytopath + "PING1500\\").Exists == false)
                new DirectoryInfo(copytopath + "PING1500\\").Create();
            if (new DirectoryInfo(copytopath + "Jitter\\").Exists == false)
                new DirectoryInfo(copytopath + "Jitter\\").Create();
            DirectoryInfo staticFolder = new DirectoryInfo(sitePath);

            string[] keyWords = { "2UE", "2UE", "PING" };
            string[] searchPatterns = { "*2*UE*", "*2*MS*", "*PING*" };
            string[] storePath = { "2UE", "2UE", "PING1500" };

            DirectoryInfo[] secFolders = staticFolder.GetDirectories("*sec*");
            int secNum = 0;//sec索引号
            foreach (DirectoryInfo secFolder in secFolders)
            {
                DirectoryInfo subFolder;
                int keyNum = 0; //关键字索引号
                foreach (string currentPattern in searchPatterns)
                {
                    if (secFolder.GetDirectories(currentPattern).Count() > 0)
                    {
                        FileInfo logFile;
                        subFolder = secFolder.GetDirectories(currentPattern)[0];
                        if (subFolder.GetFiles("*.trp").Count() > 0 && currentPattern != "*PING*")
                        {
                            logFile = subFolder.GetFiles("*.trp")[0];
                            logFile.CopyTo(copytopath + storePath[keyNum] + "\\" + siteID + "_" + keyWords[keyNum] + "_sec" + (secNum + 1) + ".trp", true);
                            Console.WriteLine(keyWords[keyNum] + " Log...Success!");
                        }
                        else//处理文件夹中的PING1500
                        {
                            if (subFolder.GetDirectories("*1500*").Count() == 0)//没有PING1500文件夹尝试直接拷贝Ping1500 log
                            {
                                if (subFolder.GetFiles("*.trp").Count() > 0)
                                {
                                    logFile = subFolder.GetFiles("*.trp")[0];
                                    logFile.CopyTo(copytopath + storePath[keyNum] + "\\" + siteID + "_" + keyWords[keyNum] + "1500" + "_sec" + (secNum + 1) + ".trp", true);
                                    Console.WriteLine("PING1500 Log...Success!");
                                }
                            }
                            else
                            {
                                DirectoryInfo pingFolder = subFolder.GetDirectories("*1500*")[0];
                                if (pingFolder.GetFiles("*.trp").Count() > 0)
                                {
                                    logFile = pingFolder.GetFiles("*.trp")[0];
                                    logFile.CopyTo(copytopath + storePath[keyNum] + "\\" + siteID + "_" + keyWords[keyNum] + "1500" + "_sec" + (secNum + 1) + ".trp", true);
                                    Console.WriteLine("PING1500 Log...Success!");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No " + keyWords[keyNum] + " folder found!");
                    }
                    keyNum++;
                }

                secNum++;
            }
            string JitterCopyToPath = copytopath + @"Jitter\";
            DirectoryInfo folder = new DirectoryInfo(sitePath);
            if (!new DirectoryInfo(JitterCopyToPath).Exists)
                new DirectoryInfo(JitterCopyToPath).Create();
            FileInfo[] UEtxtFiles = folder.EnumerateFiles("*ue*.txt", SearchOption.AllDirectories).ToArray();
            foreach (FileInfo item in UEtxtFiles)
            {
                if (!item.Name.Contains("UDP") && !item.Name.Contains("2UE") && !item.Name.Contains("UL"))
                    item.CopyTo(JitterCopyToPath + siteID + "_DL_" + item.Name, true);
            }
            FileInfo[] PCtxtFiles = folder.EnumerateFiles("*pc*.txt", SearchOption.AllDirectories).ToArray();
            foreach (FileInfo item in PCtxtFiles)
            {
                if (!item.Name.Contains("UDP") && !item.Name.Contains("DL"))
                    item.CopyTo(JitterCopyToPath + siteID + "_UL_" + item.Name, true);
            }
            FileInfo[] CRTtxtFiles = folder.EnumerateFiles("*crt*.txt", SearchOption.AllDirectories).ToArray();
            foreach (FileInfo item in CRTtxtFiles)
            {
                if (!item.Name.Contains("UDP") && !item.Name.Contains("DL"))
                    item.CopyTo(JitterCopyToPath + siteID + "_UL_" + item.Name, true);
            }
            //成功列表
            if (secNum > 0)
                Tool.RecordLog("Success:" + siteID, Program.appPath + @"\FoundSummary.txt");
        }

    }
}
