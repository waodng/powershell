using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/* ==============================================================================
 * 创建日期：2020/1/3 0:18:04
 * 创 建 者：wgd
 * 功能描述：ChangedItem  
 * ==============================================================================*/
namespace PublishTask.Common
{
    public class ChangedItem
    {
        public ChangedItem(ChangeType changeType, Microsoft.TeamFoundation.VersionControl.Client.Change change)
        {
            ChangeType = changeType;
            Change = change;
        }

        public ChangeType ChangeType { get; set; }
        public Microsoft.TeamFoundation.VersionControl.Client.Change Change { get; set; }

        public static IEnumerable<ChangedItem> Build(IEnumerable<Changeset> changesets)
        {
            var list = new List<ChangedItem>();
            changesets = changesets.OrderBy(t => t.CreationDate);
            var changes = new List<Microsoft.TeamFoundation.VersionControl.Client.Change>();
            changesets.ToList().ForEach(changeset => changes.AddRange(changeset.Changes));
            changes.Distinct(new ChangeComparer())
                .ToList()
                .ForEach(change =>
                {
                    var changeType = ChangeType.None;
                    changes.Where(t => t.Item.ItemId == change.Item.ItemId)
                        .ToList()
                        .ForEach(t => changeType = changeType | t.ChangeType);
                    list.Add(new ChangedItem(changeType, change));
                });
            return list;
        }
    }
}
