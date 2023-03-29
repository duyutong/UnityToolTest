using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class RefreshToolDll : EditorWindow
{
    private static string remotComparPath = "G:/study/MyUnity3dTools/DUnity3dTools/bin/Debug/DllCompar.txt";
    private static string localComparPath = Application.dataPath + "/Plugins/DllCompar.txt";
    private static string locPath = Application.dataPath + "/Plugins";
    private static string dllPath = "G:/study/MyUnity3dTools/DUnity3dTools/bin/Debug";
    private static List<string> needUpdate = new List<string>();
    private static List<string> needRemove = new List<string>();
    private static List<string> needDll = new List<string>()
    { "DUnity3dTools.dll","DUnity3dTools.xml","Excel.dll","ExCSS.Unity.dll","ICSharpCode.SharpZipLib.dll","Newtonsoft.Json.dll" };
    [MenuItem("RefreshToolDll/Upload")]
    private static void UploadDll()
    {
        DirectoryInfo directory = Directory.CreateDirectory(dllPath);
        FileInfo[] fileInfos = directory.GetFiles();
        string comparisonStr = "";
        foreach (FileInfo info in fileInfos)
        {
            if (!needDll.Contains(info.Name)) continue;
            string md5 = GetMD5(info.FullName);
            comparisonStr += info.Name + "," + info.Length + "," + md5 + ";";
        }
        if (File.Exists(remotComparPath)) File.Delete(remotComparPath);
        using (FileStream file = File.Create(remotComparPath))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(comparisonStr);
            file.Write(bytes, 0, bytes.Length);
            file.Flush();
            file.Close();
        }
        foreach (FileInfo info in fileInfos)
        {
            if (!needDll.Contains(info.Name)) continue;
            FtpUploadFile(info.FullName, info.Name);
        }
    }
    private static string GetMD5(string filePath)
    {
        using (FileStream file = File.OpenRead(filePath))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(file);
            file.Close();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in bytes) stringBuilder.Append(b.ToString("x2"));

            return stringBuilder.ToString();
        }
    }
    [MenuItem("RefreshToolDll/DownLoad")]
    private static void DownDall()
    {
        CheckUpdate();
        foreach (string fileName in needUpdate)
        {
            FtpDownLoadFile(locPath + "/ " + fileName, fileName);
        }
    }
    [MenuItem("RefreshToolDll/Copy")]
    private static void Copy()
    {
        CheckUpdate();
        CopyDllToLoc(remotComparPath, localComparPath);
        foreach (string fileName in needUpdate)
        {
            string filePath = dllPath + "/" + fileName;
            string localPath = locPath + "/" + fileName;
            CopyDllToLoc(filePath, localPath);
        }
    }
    private static void RemoveFile() 
    {
        foreach (string fileName in needRemove) 
        {
            string localPath = locPath + "/" + fileName;
            if (File.Exists(localPath)) File.Delete(localPath);
        }
    }
    private static void CheckUpdate()
    {
        Dictionary<string, DllCompar> remotDic = new Dictionary<string, DllCompar>();
        Dictionary<string, DllCompar> localDic = new Dictionary<string, DllCompar>();
        needUpdate.Clear();
        needRemove.Clear();
        string remotStr = File.ReadAllText(remotComparPath);
        string[] remotInfo = remotStr.Split(';');
        bool isLocalExists = File.Exists(localComparPath);
        foreach (string _info in remotInfo)
        {
            if (string.IsNullOrEmpty(_info)) continue;
            string[] _i = _info.Split(',');
            if (_i.Length == 0) continue;
            remotDic.Add(_i[0], new DllCompar(_i[0], _i[1], _i[2]));
            if (!isLocalExists) needUpdate.Add(_i[0]);
        }
        if (isLocalExists)
        {
            string localStr = File.ReadAllText(localComparPath);
            string[] localInfo = localStr.Split(';');
            foreach (string _info in localInfo)
            {
                if (string.IsNullOrEmpty(_info)) continue;
                string[] _i = _info.Split(',');
                if (_i.Length == 0) continue;
                localDic.Add(_i[0], new DllCompar(_i[0], _i[1], _i[2]));
            }
            foreach (KeyValuePair<string, DllCompar> keyValuePair in remotDic)
            {
                string checkName = keyValuePair.Key;
                string checkmd5 = keyValuePair.Value.md5;
                if (localDic.ContainsKey(checkName) && localDic[checkName].md5 == checkmd5) continue;
                needUpdate.Add(checkName);
            }
            foreach (KeyValuePair<string, DllCompar> keyValuePair in localDic)
            {
                string checkName = keyValuePair.Key;
                string checkmd5 = keyValuePair.Value.md5;
                if (remotDic.ContainsKey(checkName)) continue;
                needRemove.Add(checkName);
            }
        }
        RemoveFile();
    }

    private async static void CopyDllToLoc(string readPath, string targetPath)
    {
        await Task.Run(() =>
        {
            try
            {
                FileStream readFile = File.OpenRead(readPath);
                FileStream writFile = File.Create(targetPath);
                byte[] btyes = new byte[2048];
                //依次读取文件信息，每次最多读2048个字节
                int contentLength = readFile.Read(btyes, 0, btyes.Length);
                while (contentLength != 0)
                {
                    //循环的写入流对象
                    writFile.Write(btyes, 0, contentLength);
                    contentLength = readFile.Read(btyes, 0, btyes.Length);
                }
                readFile.Close();
                writFile.Close();
                Debug.Log(readPath + "复制成功");
            }
            catch (Exception ex) { Debug.LogError(ex.Message); }
        });
        AssetDatabase.Refresh();
    }
    private async static void FtpUploadFile(string filePath, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest ftpWebRequest = FtpWebRequest.Create(new Uri("ftp://192.168.0.226/DUnity3dTools/" + fileName)) as FtpWebRequest;
                //避免TCP冲突，设置代理为空
                ftpWebRequest.Proxy = null;
                //是否保持连接
                ftpWebRequest.KeepAlive = false;
                //指定行为函数:上传
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                //使用二进制上传
                ftpWebRequest.UseBinary = true;
                //获取FTP流对象
                Stream uploadStream = ftpWebRequest.GetRequestStream();
                //获取文件信息
                using (FileStream file = File.OpenRead(filePath))
                {
                    byte[] btyes = new byte[2048];
                    //依次读取文件信息，每次最多读2048个字节
                    int contentLength = file.Read(btyes, 0, btyes.Length);
                    while (contentLength != 0)
                    {
                        //循环的写入流对象
                        uploadStream.Write(btyes, 0, contentLength);
                        contentLength = file.Read(btyes, 0, btyes.Length);
                    }
                    file.Close();
                    uploadStream.Close();
                }
                Debug.Log(fileName + "上传成功");
            }
            catch (Exception ex) { Debug.LogError("上传失败 msg " + ex.Message); }
        });
    }
    private async static void FtpDownLoadFile(string targetPath, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest ftpWebRequest = FtpWebRequest.Create(new Uri("ftp://192.168.0.226/DUnity3dTools/" + fileName)) as FtpWebRequest;
                //避免TCP冲突，设置代理为空
                ftpWebRequest.Proxy = null;
                //是否保持连接
                ftpWebRequest.KeepAlive = false;
                //指定行为函数:下载
                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                //使用二进制下载
                ftpWebRequest.UseBinary = true;
                //获取FTP流对象
                FtpWebResponse ftpWebResponse = ftpWebRequest.GetResponse() as FtpWebResponse;
                Stream downdStream = ftpWebResponse.GetResponseStream();
                if (File.Exists(targetPath)) File.Delete(targetPath);
                using (FileStream file = File.Create(targetPath))
                {
                    byte[] btyes = new byte[2048];
                    //依次读取文件信息，每次最多读2048个字节
                    int contentLength = downdStream.Read(btyes, 0, btyes.Length);
                    while (contentLength != 0)
                    {
                        //循环的写入流对象
                        file.Write(btyes, 0, contentLength);
                        contentLength = downdStream.Read(btyes, 0, btyes.Length);
                    }
                    file.Close();
                    downdStream.Close();
                }
                Debug.Log(targetPath + " 下载成功 ");
            }
            catch (Exception ex) { Debug.LogError("下载失败 msg " + ex.Message); }
        });
        AssetDatabase.Refresh();
    }

    private class DllCompar
    {
        public string dllName;
        public long size;
        public string md5;
        public DllCompar(string _name, string _size, string _md5)
        {
            this.dllName = _name;
            this.size = long.Parse(_size);
            this.md5 = _md5;
        }
    }
}
