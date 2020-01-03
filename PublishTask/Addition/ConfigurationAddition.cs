using PublishTask.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/* ==============================================================================
 * 创建日期：2020/1/3 0:30:45
 * 创 建 者：wgd
 * 功能描述：ConfigurationAddition  
 * ==============================================================================*/
namespace PublishTask.Addition
{
    public class ConfigurationAddition:DefaultAddition
    {
        public ConfigurationAddition(ChangedItem changedItem)
            : base(changedItem)
        {
            this.ChangedItem = changedItem;

        }
        public override void Republish(string publishFolder, string tempFolder)
        {
            if (ChangedItem.ChangeType.HasFlag(ChangeType.Add))
            {
                base.Republish(publishFolder, tempFolder);
                return;
            }
            var docAddit = new XmlDocument();
            var docThis = new XmlDocument();
            docThis.Load(GetAbsolutePath(publishFolder));
            //参数SCOPE LABEL不确定是否正确
            var itemSpec = TfsUtility.GetSpecVersion(this.Scope, this.Label, this.ChangedItem.Change.Item);
            var docSpec = new XmlDocument();
            docSpec.Load(itemSpec.DownloadFile());
            //导入XML Declaration
            if (docThis.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                docAddit.AppendChild(docAddit.ImportNode(docThis.FirstChild, true));
            //设置DOcumentElement为增量文档的第一个节点
            var nodeThis = (XmlNode)docThis.DocumentElement;
            var nodeSpec = (XmlNode)docSpec.DocumentElement;
            var nodeAddit = docAddit.ImportNode(nodeThis, true);
            RecursiveCompareChildNode(nodeThis, nodeSpec, nodeAddit, docAddit);
            docAddit.AppendChild(nodeAddit);
            var path = Path.ChangeExtension(GetAbsolutePath(tempFolder), ".addition.config");
            FileUtility.CreateIfNotExists(path);
            docAddit.Save(path);
        }
        private void RecursiveCompareChildNode(XmlNode nodeThis, XmlNode nodeSpec, XmlNode nodeAddit, XmlDocument docAddit)
        {
            //如果当前节点是LIST对象，直接输出整个节点
            if (nodeThis.ParentNode != null &&
                nodeThis.ParentNode.ChildNodes.Cast<XmlNode>()
                    .Count(t => t.Name == nodeThis.Name && t.NodeType == XmlNodeType.Element) > 1)
                return;
            nodeAddit.InnerXml = string.Empty;

            var listThis = nodeThis.ChildNodes.Cast<XmlNode>().ToList();
            var listSpec = nodeSpec.ChildNodes.Cast<XmlNode>().ToList();
            //*完全一样的elements会被忽略掉
            var comparer = new XmlNodeComparer();
            var exceptsThis = listThis.Except(listSpec, comparer).ToList();
            var exceptsSpec = listSpec.Except(listThis, comparer).ToList();
            foreach (var exceptNode in exceptsThis)
            {
                var childAddit = docAddit.ImportNode(exceptNode, true);
                //如没有element子节点，直接加入到增量文档
                if (exceptNode.ChildNodes.Cast<XmlNode>().All(t => t.NodeType != XmlNodeType.Element))
                {
                    nodeAddit.AppendChild(childAddit);
                    continue;
                }
                var childSpec = nodeSpec.ChildNodes
                    .Cast<XmlNode>()
                    .ToList()
                    .FirstOrDefault(t => NodePathCompare(t, exceptNode));

                nodeAddit.AppendChild(childAddit);
            }
            foreach (var exceptNode in exceptsSpec)
            {
                if (!exceptsThis.Exists(t => t.Name == exceptNode.Name))
                    nodeAddit.AppendChild(docAddit.ImportNode(exceptNode, true));
            }
        }
        private bool NodePathCompare(XmlNode node1, XmlNode node2)
        {
            var node1path = node1.Name.GetHashCode();
            var node2path = node2.Name.GetHashCode();
            var node = node1;
            while (node.ParentNode != null)
            {
                node = node.ParentNode;
                node1path += node.Name.GetHashCode();
            }
            node = node2;
            while (node.ParentNode != null)
            {
                node = node.ParentNode;
                node2path += node.Name.GetHashCode();
            }
            return node1path == node2path;
        }
    }

    public class XmlNodeComparer : EqualityComparer<XmlNode>
    {
        public override bool Equals(XmlNode x, XmlNode y)
        {
            return x.Name == y.Name;
        }

        public override int GetHashCode(XmlNode obj)
        {
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return obj.ToString().GetHashCode();
            }
        }
    }
}
