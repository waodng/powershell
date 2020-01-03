using PublishTask.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* ==============================================================================
 * 创建日期：2020/1/3 0:23:53
 * 创 建 者：wgd
 * 功能描述：DefaultAddition  
 * ==============================================================================*/
namespace PublishTask.Addition
{
    public class DefaultAddition:IAdditionable
    {
        /// <summary>
        /// 范围
        /// </summary>
        public string Scope { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; set; }
        public ChangedItem ChangedItem { get; set; }

        public ProjectItem ProjectItem { get; set; }

        public DefaultAddition(ChangedItem changedItem)
        {
            this.ChangedItem = changedItem;
            this.ProjectItem = ProjectItem.Find(changedItem.Change.Item);
        }

        public virtual void Republish(string publishFolder, string tempFolder)
        {
            //FileUtility.CopyTo(GetAbsolutePath(publishFolder), GetAbsolutePath(tempFolder));
            File.Copy(GetAbsolutePath(publishFolder), GetAbsolutePath(tempFolder));
        }

        protected string GetRelativePath()
        {
            var start = ChangedItem.Change.Item.ServerItem.IndexOf(ProjectItem.Name) + ProjectItem.Name.Length;
            return ChangedItem.Change.Item.ServerItem.Substring(start).Trim('/').Trim('\\');
        }

        protected string GetAbsolutePath(string dir)
        {
            return Path.Combine(dir, GetRelativePath());
        }
    }
}
