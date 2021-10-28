using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MPS.HZ.Core.Folders
{
    public class ImageFolder
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private List<ImageFolder> imageFolders;
        public List<ImageFolder> ImageFolders
        {
            get { return imageFolders; }
            set { imageFolders = value; }
        }

        private List<ImageFile> imageFiles;
        public List<ImageFile> ImageFiles
        {
            get { return imageFiles; }
            set { imageFiles = value; }
        }

        public ImageFolder()
        {
            ImageFiles = new List<ImageFile>();
            ImageFolders = new List<ImageFolder>();
        }

        public ImageFolder(string name) : this()
        {
            this.Name = name;
        }
    }
}
