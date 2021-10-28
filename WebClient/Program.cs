using MPS.HZ.Core.Folders;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace WebClient
{
    class Program
    {
        static string uri = "http://10.10.82.126:5000";
        static HttpClient hClinet;
        static void Main(string[] args)
        {
            FileInfo fi = new FileInfo("Imgs/sch - 副本.png");
            fi.LastWriteTime = DateTime.Now.AddDays(-1);
            
            Console.WriteLine(fi.Extension);
            Console.ReadLine();
            return;
            hClinet = new HttpClient();
            var client = new System.Net.WebClient();
            var stream = client.OpenRead($"{uri}/GetAllFiles");
            var sr = new StreamReader(stream);
            var remoteImgFds = JsonConvert.DeserializeObject<ImageFolder>(sr.ReadToEnd());
            stream.Dispose();
            var imgDomain = new ImageFileDomain();
            var path = $"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Imgs";
            var localImgFds = imgDomain.GetAllFiles(path);
            var needUpdateFds = imgDomain.Compare(remoteImgFds, localImgFds, "Imgs");
            if (needUpdateFds != null)
                DownloadFiles(needUpdateFds, needUpdateFds.Name, client);
            var needUploadFds = imgDomain.Compare(localImgFds, remoteImgFds, "Imgs");
            client.Headers.Add("Content-Type", "application/form-data");
            if (needUploadFds != null)
                UploadFiles(needUploadFds, needUploadFds.Name);
            client.Dispose();
            Console.ReadLine();
        }

        static void DownloadFiles(ImageFolder imgFd, string path, System.Net.WebClient client)
        {
            foreach (var file in imgFd.ImageFiles)
            {
                var filePath = $"{path}/{file.Name}";
                var url = $"{uri}/StaticFiles/{filePath}";
                client.DownloadFile(url, filePath);
            }
            foreach (var fd in imgFd.ImageFolders)
            {
                var fdPath = $"{path}/{fd.Name}";
                if (!Directory.Exists(fdPath))
                    Directory.CreateDirectory(fdPath);
                DownloadFiles(fd, fdPath, client);
            }
        }

        static async void UploadFiles(ImageFolder imgFd, string path)
        {
            foreach (var file in imgFd.ImageFiles)
            {
                var filePath = $"{path}/{file.Name}";
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var content = new MultipartFormDataContent();
                var bytes = File.ReadAllBytes(filePath);
                Console.WriteLine($"Upload File Path : {filePath}；Content:{(bytes == null ? "" : Encoding.UTF8.GetString(bytes))}");
                content.Add(new ByteArrayContent(bytes), "file", file.Name);
                var url = $"{uri}/UploadFile?path={filePath}";
                await hClinet.PostAsync(url, content);
            }
            foreach (var fd in imgFd.ImageFolders)
            {
                var fdPath = $"{path}/{fd.Name}";
                if (!Directory.Exists(fdPath))
                    Directory.CreateDirectory(fdPath);
                UploadFiles(fd, fdPath);
            }
        }
    }
}
