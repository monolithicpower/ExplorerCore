using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MPS.HZ.Core.Folders
{
    public class ImageFile
    {
        private DateTime updatedTime;
        public DateTime UpdatedTime
        {
            get { return updatedTime; }
            set { updatedTime = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public ImageFile() { }

        public ImageFile(string name, DateTime updatedTime)
        {
            this.Name = name;
            this.UpdatedTime = updatedTime;
        }
    }
}
