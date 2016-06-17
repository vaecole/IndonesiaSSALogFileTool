using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndonesiaSSALogFileTool
{
    class OtherTool
    {
        public void RecordLog(string recordContent = "Log File", string logSavePath = "")
        {
            if (logSavePath == "")
                logSavePath = Program.appPath + "\\run_summary" + DateTime.Now.Month.ToString() + ".log";
            StreamWriter logFile;
            if (!File.Exists(logSavePath))
            {
                logFile = File.CreateText(logSavePath);
            }
            else
                logFile = new StreamWriter(logSavePath, true);
            logFile.WriteLine(DateTime.Now.ToString() + ": " + recordContent);
            logFile.Close();
        }

        //搜索jitter和speedtest放入JitterAndSpeedTest文件夹
        public string SearchJitterAndSpeedTest(string path, string copyToPath)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            if (!new DirectoryInfo(copyToPath).Exists)
                new DirectoryInfo(copyToPath).Create();
            FileInfo[] UEtxtFiles = folder.EnumerateFiles("*ue*.txt", SearchOption.AllDirectories).ToArray();
            foreach (FileInfo item in UEtxtFiles)
            {
                if (!item.Name.Contains("UDP") && !item.Name.Contains("UL"))
                    item.CopyTo(copyToPath + "DL_" + item.Name, true);
            }
            FileInfo[] PCtxtFiles = folder.EnumerateFiles("*pc*.txt", SearchOption.AllDirectories).ToArray();
            foreach (FileInfo item in PCtxtFiles)
            {
                if (!item.Name.Contains("UDP") && !item.Name.Contains("DL"))
                    item.CopyTo(copyToPath + "UL_" + item.Name, true);
            }
            FileInfo[] CRTtxtFiles = folder.EnumerateFiles("*crt*.txt", SearchOption.AllDirectories).ToArray();
            foreach (FileInfo item in CRTtxtFiles)
            {
                if (!item.Name.Contains("UDP") && !item.Name.Contains("DL"))
                    item.CopyTo(copyToPath + "UL_" + item.Name, true);
            }

            //speed test
            DirectoryInfo[] stFolders = folder.EnumerateDirectories("*speed*", SearchOption.AllDirectories).ToArray();

            for (int i = 0; i < stFolders.Count(); i++)
            {
                new DirectoryInfo(copyToPath + (i + 1) + ".Speedtest\\").Create();
                foreach (FileInfo item in stFolders[i].GetFiles())
                {
                    try
                    {
                        item.CopyTo(copyToPath + (i + 1) + ".Speedtest\\" + item.Name, true);
                    }
                    catch
                    {
                    }
                }
            }
            return copyToPath;
        }

        //生成trp文件更改记录
        public string RecordLogNameChange(string oldName, string newName, string logSavePath = ".\\LogNameChang.txt")
        {
            StreamWriter logFile;
            if (!File.Exists(logSavePath))
            {
                logFile = File.CreateText(logSavePath);
                logFile.WriteLine("OriginalLogName\tNewLogName");
            }
            else
                logFile = new StreamWriter(logSavePath, true);
            logFile.WriteLine(oldName + "\t" + newName);
            logFile.Close();
            return logSavePath;
        }


        //public void CopyMobility(string mobilityPath, string copyToPath)
        //{
        //    DirectoryInfo logFolder = new DirectoryInfo(mobilityPath);
        //    FileInfo[] logFiles = logFolder.GetFiles("*.trp");
        //    foreach (FileInfo item in logFiles)
        //    {
        //        item.CopyTo(copyToPath + "Mobility_" + item.Name, true);
        //    }
        //}

        //运行bat脚本
        public string RunBat(string batPath)
        {
            if (batPath != "")
            {
                Process pro = new Process();

                FileInfo file = new FileInfo(batPath);
                pro.StartInfo.WorkingDirectory = file.Directory.FullName;
                pro.StartInfo.FileName = batPath;
                pro.StartInfo.CreateNoWindow = false;
                pro.Start();
                pro.WaitForExit();
            }
            return batPath;
        }

        //打包mobility, jitter, speedtest和trp文件更改记录
        public string Pack_Mobility_Jitter_SpeedTest_LogChange(string sitePath)
        {
            TextWriter batFile = File.CreateText(sitePath + "\\pack.bat");
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
            string cmdLineMobility = "\"" + WinRarPath.FullName + "\"" + " a -ep1 \"" + sitePath + "\\Mobility_Jitter_SpeedTest_LogChange.rar\"" + " \"" + sitePath + "\\Mobility\\*\" -y";
            string cmdLineJitterSpeedtest = "\"" + WinRarPath.FullName + "\"" + " a -r -ep1 \"" + sitePath + "\\Mobility_Jitter_SpeedTest_LogChange.rar\"" + " \"" + sitePath + "\\JitterAndSpTest\\\" -y";
            string cmdLineLogName = "\"" + WinRarPath.FullName + "\"" + " a -ep1 \"" + sitePath + "\\Mobility_Jitter_SpeedTest_LogChange.rar\"" + " \"" + sitePath + "\\LogNameChange.txt\" -y";
            batFile.WriteLine(cmdLineMobility);
            batFile.WriteLine(cmdLineJitterSpeedtest);
            batFile.WriteLine(cmdLineLogName);
            batFile.Close();
            Console.WriteLine("packing Mobility, JitterSpeedtest and LogNameChange...");
            RunBat(sitePath + "\\pack.bat");
            Console.WriteLine("Done...");
            return sitePath + "\\Mobility_Jitter_SpeedTest_LogChange.rar";
        }

        public string Unpack(string originalLogsPath, string destinationPath)
        {
            DirectoryInfo originalLogsFolder = new DirectoryInfo(originalLogsPath);
            DirectoryInfo destinationFolder = new DirectoryInfo(destinationPath);
            if (originalLogsFolder.Exists && originalLogsFolder.GetFiles().Count() > 0)
            {
                //创建解压脚本
                TextWriter batFile = File.CreateText(destinationFolder.FullName + "Unpack.bat");
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
                batFile.WriteLine("\"" + WinRarPath.FullName + "\"" + " x \"" + originalLogsPath + "*.rar\" \"" + destinationFolder.FullName + "\" -y");
                batFile.WriteLine("\"" + WinRarPath.FullName + "\"" + " x \"" + originalLogsPath + "*.zip\" \"" + destinationFolder.FullName + "\" -y");
                batFile.Close();
                Console.WriteLine("Unpacking...");
                RunBat(destinationFolder.FullName + "Unpack.bat");
                Console.WriteLine("Unpack Success...");
                new FileInfo(destinationFolder.FullName + "Unpack.bat").Delete();
                return destinationFolder.FullName;
            }
            else
            {
                //RecordLog("No Log found in the SSA Folder:" + originalLogsPath, destinationFolder.FullName + "error.txt");
                RecordLog("No Log found in the SSA Folder:" + originalLogsPath);
                return "";
            }
        }

        public string UnpackOne(string logFilePath, string destinationPath)
        {
            DirectoryInfo destinationFolder = new DirectoryInfo(destinationPath);
            if (new FileInfo(logFilePath).Exists)
            {
                //创建解压脚本
                TextWriter batFile = File.CreateText(destinationFolder.FullName + "\\UnpackOne.bat");
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
                batFile.WriteLine("\"" + WinRarPath.FullName + "\"" + " x \"" + logFilePath + "\" \"" + destinationFolder.FullName + "\" -y");
                batFile.Close();
                Console.WriteLine("Unpacking...");
                RunBat(destinationFolder.FullName + "\\UnpackOne.bat");
                Console.WriteLine("Unpack Success...");
                new FileInfo(destinationFolder.FullName + "\\UnpackOne.bat").Delete();
                return destinationFolder.FullName;
            }
            else
            {
                //RecordLog("No Log found in the SSA Folder:" + originalLogsPath, destinationFolder.FullName + "error.txt");
                RecordLog("No Log found:" + logFilePath);
                return "";
            }
        }

    }
}
