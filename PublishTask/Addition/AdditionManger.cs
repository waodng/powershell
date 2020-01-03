using PublishTask.Addition;
using PublishTask.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* ==============================================================================
 * 创建日期：2020/1/3 0:27:51
 * 创 建 者：wgd
 * 功能描述：ChangeManger  
 * ==============================================================================*/
namespace PublishTask.Addition
{
    public class AdditionManger
    {
        static AdditionManger()
        {
            ChangeCollection = new List<IAdditionable>();
        }

        public static List<IAdditionable> ChangeCollection { get; set; }

        public static void AddChange(ChangedItem changeItem)
        {
            var ext = Path.GetExtension(changeItem.Change.Item.ServerItem).ToLower();
            IAdditionable changed = null;
            switch (ext)
            {
                case ".cs":
                    changed = new ProjectAddition(changeItem);
                    break;
                case ".config":
                    changed = new ConfigurationAddition(changeItem);
                    break;
                default:
                    changed = new DefaultAddition(changeItem);
                    break;
            }
            if (!ChangeCollection.Exists(t => t.Equals(changed))) ChangeCollection.Add(changed);
        }
        public static void AdditionChange()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var publishFolder = @"D:\HelloTask\HelloTask.Web\Publish";
            ChangeCollection.ForEach(t => t.Republish(publishFolder, tempFolder));
        }
    }
}
