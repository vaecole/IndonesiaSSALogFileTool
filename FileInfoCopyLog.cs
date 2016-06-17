using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndonesiaSSALogFileTool
{
    class FileInfoCopyLog
    {
        private OtherTool Tool = new OtherTool();
        private static FileInfo fileInfo;
        public FileInfoCopyLog()
        {

        }
        public static string currentSitePath = ".\\LogNameChange.txt";
        public FileInfoCopyLog(string filePath)
        {
            fileInfo = new FileInfo(filePath);
        }
        public void Assign(FileInfo _fileInfo)
        {
            fileInfo =_fileInfo;
        }
        public FileInfo CopyTo(string copyToPath, bool _override)
        {          
            Tool.RecordLogNameChange(fileInfo.Name, Path.GetFileName(copyToPath), currentSitePath);
            return fileInfo.CopyTo(copyToPath, _override);
        }

    }
}
