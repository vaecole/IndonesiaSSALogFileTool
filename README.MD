##印尼SSA项目Log改名小工具：IndonesiaSSALogFileTool

###说明：
	程序包含主程序:IndonesiaSSALogFileTool.exe；SiteList文件，包含站号，PCI和频点：SitePCIEarfcn.txt。

###输入：
	包含所有log的常规命名的DT Log文件夹，文件夹名要求含有“站号”(例：XXX_1234567E_XXX)关键字。
	SitePCIEarfcn.txt：请拷贝至程序同文件夹下。

###用法：
	解压并拷贝至DT log文件夹外，运行后可处理当前目录所有的DT log文件夹。

###要求：
	DT log文件夹名称必须包含站号，文件夹内必须包含static文件夹或者sec1/2/3文件夹。

###输出：
	程序将能识别出站号的站的所有Log按照文件夹关键字依次命名，并拷贝至对应站Log文件夹的根目录内。

###例如：
	XXX_1234567E_XXXX文件夹下有static(里面有sec1/2/3)，或者直接是sec1/2/3(即没有static也可)，程序会将sec文件夹中含有CSFB，FTP，Attatch，PING32和PING1000关键字的文件夹中的log重命名为“频点+PCI+关键字”，并拷贝至XXX_1234567E_XXXX文件夹下。
其中，频点和PCI根据文件夹上的站号在SitePCIEarfcn.txt文件中自动查找到。



Mark Wu 吴伟伟
mark.wu@ericsson.com
20151203