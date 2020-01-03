using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using PublishTask.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace PublishTask
{
    public class PublishTask : Microsoft.Build.Utilities.Task 
    {
        private string _version = "3.0.*";
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                this._version = value;
            }
        }

        private string _versionPath = string.Empty;
        /// <summary>
        /// 修改版本号文件路径
        /// </summary>
        public string VersionPath
        {
            get
            {
                return _versionPath;
            }
            set
            {
                this._versionPath = value;
            }
        }

        
        private string _outPath = @"C:\Users\Administrator\Desktop\Temp";
        /// <summary>
        /// 输出路径
        /// </summary>
        //[Microsoft.Build.Framework.Required] 
        public string OutPath { get {
            return _outPath;
        }
            set {
                this._outPath = value;
            }
        }

        private string _publishDir = @"C:\Users\Administrator\Desktop\publish";
        /// <summary>
        /// 发布路径
        /// </summary>
        [Microsoft.Build.Framework.Required]
        public string publishDir
        {
            get {
                return _publishDir;
        }
            set {
                this._publishDir = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
#if DEBUG
            //System.Diagnostics.Debugger.Launch();
#endif

            string config = Path.Combine(OutPath, "web.config");
            //TfsUtility.SourceControl = TfsUtility.Open(@"E:\Projects\供应室完美版本");

            Log.LogWarning("发布任务的路径" + OutPath);
            RunCmdShow(@"C:\Windows\System32\cmd.exe", string.Format("/c xcopy {0} {1}  /e /h /y /i ", publishDir, OutPath));
            //替换版本号
            ReplaceContent(VersionPath);
            if (File.Exists(config))
            {
                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                xml.Load(config);
                System.Xml.XmlNode root = xml.DocumentElement;
                System.Xml.XmlNode node = root.SelectSingleNode(@"connectionStrings/add");
                node.Attributes["connectionString"].Value = "任务节点保存成功";

                System.Xml.XmlNodeList setts = root.SelectNodes(@"appSettings/add");

                string path = "";
                foreach (System.Xml.XmlNode item in setts)
                {
                    switch (item.Attributes["key"].Value)
                    {
                        case "SqlPath":
                            path = item.Attributes["value"].Value;
                            break;
                        case "SqlHash":
                            item.Attributes["value"].Value = CheckFileHash(path);
                            break;
                        default:
                            break;
                    }
                }
                xml.Save(config);
            }
            return true;
        }
        /// <summary>
        /// 修改文件内容
        /// </summary>
        /// <param name="path"></param>
        protected void ReplaceContent(string path)
        {
            if (File.Exists(path))
            {
                string strContent = File.ReadAllText(path);
                strContent = System.Text.RegularExpressions.Regex.Replace(strContent, @"({{\s*)([1-9]{1}.\d{1})(.\*\s*}})", "$2." + GetVersion());
                File.WriteAllText(path, strContent);
            }
        }

        /// <summary>
        /// 得到版本的后两位 日期数+ 刻度数前5位
        /// </summary>
        /// <returns></returns>
        protected string GetVersion()
        {
            string endVersion = (DateTime.Now - new DateTime(2000, 1, 1, 0, 0, 0)).Days.ToString();
            endVersion += "." + (DateTime.Now - DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 0:0:0"))).Ticks.ToString().Substring(0,5);
            return endVersion;
        }

        //获取本地文件Hash值
        protected string CheckFileHash(string LocalFilePath)
        {
            if (!File.Exists(LocalFilePath))
            {
                return "";
            }

            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] hash;
            using (FileStream fs = new FileStream(LocalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read,10240))
                hash = sha1.ComputeHash(fs);
            return BitConverter.ToString(hash);
        }
        /// <summary>
        /// 运行cmd命令
        /// 会显示命令窗口
        /// </summary>
        /// <param name="cmdExe">指定应用程序的完整路径</param>
        /// <param name="cmdStr">执行命令行参数</param>
        static bool RunCmdShow(string cmdExe, string cmdStr)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    //指定启动进程是调用的应用程序和命令行参数
                    ProcessStartInfo psi = new ProcessStartInfo(cmdExe, cmdStr);
                    myPro.StartInfo = psi;
                    myPro.Start();
                    myPro.WaitForExit();
                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }

        /// <summary>
        /// 运行cmd命令
        /// 不显示命令窗口
        /// </summary>
        /// <param name="cmdExe">指定应用程序的完整路径</param>
        /// <param name="cmdStr">执行命令行参数</param>
        static bool RunCmdNoShow(string cmdExe, string cmdStr)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = true;
                    myPro.Start();
                    //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                    string str = string.Format(@"""{0}"" {1} {2}", cmdExe, cmdStr, "&exit");

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    myPro.WaitForExit();

                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }
    }
}
