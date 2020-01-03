using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* ==============================================================================
 * 创建日期：2020/1/3 0:19:12
 * 创 建 者：wgd
 * 功能描述：ChangeComparer  
 * ==============================================================================*/
namespace PublishTask.Common
{
    public class ChangeComparer : IEqualityComparer<Microsoft.TeamFoundation.VersionControl.Client.Change>
    {
        public bool Equals(Microsoft.TeamFoundation.VersionControl.Client.Change x, Microsoft.TeamFoundation.VersionControl.Client.Change y)
        {
            return x.Item.ItemId == y.Item.ItemId;
        }

        public int GetHashCode(Microsoft.TeamFoundation.VersionControl.Client.Change obj)
        {
            return obj.Item.ItemId.GetHashCode();
        }
    }
}
