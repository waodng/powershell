using PublishTask.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* ==============================================================================
 * 创建日期：2020/1/3 0:26:26
 * 创 建 者：wgd
 * 功能描述：ProjectAddition  
 * ==============================================================================*/
namespace PublishTask.Addition
{
    public class ProjectAddition:DefaultAddition
    {
        public ProjectAddition(ChangedItem changedItem)
            : base(changedItem)
        {
            this.ChangedItem = changedItem;
        }

        public override void Republish(string publishFolder, string tempFolder)
        {
            var bin = "bin";
            var assembly = string.Format("{0}.{1}", ProjectItem.AssemblyName, ProjectItem.OutputType == "Library" ? "dll" : "exe");
            var pdb = string.Format("{0}.pdb", ProjectItem.AssemblyName);

            var assemblyFrom = Path.Combine(publishFolder, bin, assembly);
            var assemblyTo = Path.Combine(tempFolder, bin, assembly);
            var pdbFrom = Path.Combine(publishFolder, bin, pdb);
            var pdbTo = Path.Combine(tempFolder, bin, pdb);

            FileUtility.CopyTo(assemblyFrom, assemblyTo);
            FileUtility.CopyTo(pdbFrom, pdbTo);
        }
    }
}
