using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/* ==============================================================================
 * 创建日期：2020/1/3 0:16:51
 * 创 建 者：wgd
 * 功能描述：ProjectItem  
 * ==============================================================================*/
namespace PublishTask.Common
{
    public class ProjectItem
    {
        public static List<ProjectItem> ProjectCollection { get; set; }

        public static void GetAll(string solutionDir)
        {
            ProjectCollection = new List<ProjectItem>();
            Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories)
                .ToList()
                .ForEach(t => ProjectCollection.Add(new ProjectItem(t)));
        }

        public static ProjectItem Find(Item item)
        {
            return ProjectCollection.ToList()
                .FirstOrDefault(t => item.ServerItem.IndexOf(t.Name) >= 0);
        }

        public string Name { get; set; }

        public string Path { get; set; }

        public string AssemblyName { get; set; }

        public bool Changed { get; set; }

        public string OutputType { get; set; }

        public ProjectItem(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            var doc = new XmlDocument();
            doc.Load(path);
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            var node = doc.SelectSingleNode("//ns:PropertyGroup//ns:OutputType", ns);
            OutputType = node != null ? node.InnerText : string.Empty;
            node = doc.SelectSingleNode("//ns:PropertyGroup//ns:AssemblyName", ns);
            AssemblyName = node != null ? node.InnerText : string.Empty;
            this.Changed = false;
        }
    }
}
