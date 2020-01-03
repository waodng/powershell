using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* ==============================================================================
 * 创建日期：2020/1/3 9:29:41
 * 创 建 者：wgd
 * 功能描述：TfsUtility  
 * ==============================================================================*/
namespace PublishTask.Common
{
    /// <summary>
    /// tfs操作公共类
    /// </summary>
    public class TfsUtility
    {
        public static VersionControlServer SourceControl { get; set; }
        /// <summary>
        /// 通过本地工作目录获取服务URI信息
        /// 然后再操作TFS  密码凭据获取本地的
        /// </summary>
        /// <param name="path">项目在本地所在的映射目录</param>
        /// <returns></returns>
        public static VersionControlServer Open(string path)
        {
            var info = Workstation.Current.GetLocalWorkspaceInfo(path);
            var uri = info.ServerUri;
            //var uri = "..."
            var tfsCollection = new TfsTeamProjectCollection(uri);
            return tfsCollection.GetService<VersionControlServer>();
        }

        public static void CreateLabel(string scope, string label)
        {
            var itemSpec = new ItemSpec(scope, RecursionType.Full);
            var labelItemSpec = new LabelItemSpec(itemSpec, VersionSpec.Latest, false);
            var vslabel = new VersionControlLabel(SourceControl, label, SourceControl.AuthorizedUser, scope, label);
            SourceControl.CreateLabel(vslabel, new[] { labelItemSpec }, LabelChildOption.Replace);
        }
        /// <summary>
        /// 查询标签
        /// </summary>
        /// <param name="scope">范围</param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static VersionControlLabel QueryLabel(string scope, string label)
        {
            return SourceControl.QueryLabels(label, null, null, true, scope, VersionSpec.Latest).FirstOrDefault();
        }
        public static IEnumerable<Changeset> Changes(string scope, string label1, string label2)
        {
            var vsLabel1 = QueryLabel(scope, label1);
            var vsLabel2 = QueryLabel(scope, label2);
            var vsLabelSpec1 = new LabelVersionSpec(vsLabel1.Name, vsLabel1.Scope);
            var vsLabelSpec2 = new LabelVersionSpec(vsLabel2.Name, vsLabel2.Scope);
            return SourceControl.QueryHistory(vsLabelSpec1.Scope,
                VersionSpec.Latest,
                0,
                RecursionType.Full,
                null,
                vsLabelSpec1,
                vsLabelSpec2,
                int.MaxValue,
                true,
                false)
                .Cast<Changeset>();
        }
        /// <summary>
        /// get spec version
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="label"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Item GetSpecVersion(string scope, string label, Item item)
        {
            var vslabel = QueryLabel(scope, label);
            return SourceControl.GetItem(item.ServerItem, new LabelVersionSpec(vslabel.Name, vslabel.Scope));
        }
    }
}
