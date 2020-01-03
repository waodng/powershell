using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublishTask.Addition
{
    public interface IAdditionable
    {
        void Republish(string publishFolder, string tempFolder);
    }
}
