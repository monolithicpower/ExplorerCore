using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MPS.HZ.Core.Folders
{
    public class ImageFileDomain
    {
        public ImageFolder GetAllFiles(string path)
        {
            var result = new ImageFolder(Path.GetFileNameWithoutExtension(path));
            if (!Directory.Exists(path))
                return result;
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var cResult = GetAllFiles(dir);
                result.ImageFolders.Add(cResult);
            }
            var di = new DirectoryInfo(path);
            var fis = di.GetFiles().Where(p => p.Name.EndsWith(".png") || p.Name.EndsWith(".jpg"));
            foreach (var fi in fis)
            {
                result.ImageFiles.Add(new ImageFile(fi.Name, fi.LastWriteTime));
            }
            return result;
        }

        /// <summary>
        /// 比较左和右，左侧较新的部分会放在结果，若完全没有不同则返回null
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ImageFolder Compare(ImageFolder left, ImageFolder right, string fileName)
        {
            var result = new ImageFolder(fileName);
            foreach (var fi in left.ImageFiles)
            {
                var rightFi = right.ImageFiles.FirstOrDefault(p => p.Name == fi.Name);
                if (rightFi == null || rightFi.UpdatedTime < fi.UpdatedTime)
                    result.ImageFiles.Add(fi);
            }
            foreach (var fd in left.ImageFolders)
            {
                var rightFd = right.ImageFolders.FirstOrDefault(p => p.Name == fd.Name);
                if (rightFd == null)
                    result.ImageFolders.Add(fd);
                else
                {
                    var comparedFd = Compare(fd, rightFd, fd.Name);
                    if (comparedFd != null)
                        result.ImageFolders.Add(fd);
                }
            }
            if ((result.ImageFiles == null || result.ImageFiles.Count == 0) && (result.ImageFolders == null || result.ImageFolders.Count == 0))
                return null;
            return result;
        }
    }
}
