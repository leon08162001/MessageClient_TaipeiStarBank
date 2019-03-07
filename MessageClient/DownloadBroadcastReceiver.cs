using System;
using Android.App;
using Android.OS;
using Environment = Android.OS.Environment;
using Android.Content;
using Android.Database;
using Common.Utility;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace ch4_3_1
{
    [BroadcastReceiver]
    class DownloadBroadcastReceiver : BroadcastReceiver
    {
        private DownloadManager _DM;
        private long _DownloadID;
        private Activity _Activity;

        public DownloadBroadcastReceiver()
        {

        }
        public DownloadBroadcastReceiver(Activity Activity, DownloadManager DM, long DownloadID)
        {
            _Activity = Activity;
            _DM = DM;
            _DownloadID = DownloadID;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                Handler handler = new Handler(Looper.MainLooper);
                DownloadManager.Query query = new DownloadManager.Query();
                query.SetFilterById(_DownloadID);
                query.SetFilterByStatus(DownloadStatus.Successful | DownloadStatus.Running | DownloadStatus.Paused | DownloadStatus.Pending);
                ICursor c = _DM.InvokeQuery(query);
                if (c.MoveToFirst())
                {
                    int status = c.GetInt(c.GetColumnIndex(DownloadManager.ColumnStatus));
                    if (status == (int)DownloadStatus.Successful)
                    {
                        if (_Activity.GetType() == typeof(Tab2Activity))
                        {
                            //handler.PostDelayed(() => { (_Activity as Tab2Activity).StartWaitingUpdate(); }, 1000);
                            handler.Post(() => { (_Activity as Tab2Activity).StartWaitingUpdate(); });
                        }

                        string downPath = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).ToString();
                        string zipPath = _DM.GetUriForDownloadedFile(_DownloadID).Path;
                        string strErr = "";
                        if (Directory.Exists(downPath + @"/App Update"))
                        {
                            ClearFolder(downPath + @"/App Update");
                        }

                        if (UnZipFile(zipPath, downPath + @"/App Update", out strErr) && UnZipDirectory(downPath + @"/App Update"))
                        {
                            string apkPath = GetAPKPath(downPath + @"/App Update");
                            Intent promptInstall = new Intent(Intent.ActionView);
                            promptInstall.SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(apkPath)), "application/vnd.android.package-archive");
                            promptInstall.SetFlags(ActivityFlags.NewTask);
                            context.StartActivity(promptInstall);
                        }
                        //判斷哪個Activity觸發,而執行相對應的UI更新
                        if (_Activity.GetType() == typeof(Tab2Activity))
                        {
                            handler.Post(() => { (_Activity as Tab2Activity).AllowOrientation(); });
                            //handler.Post(() => { _Activity.FindViewById<Button>(Resource.Id.btnDownloadApp).Enabled = true; });
                            //handler.PostDelayed(() => { (_Activity as Tab2Activity).AllowOrientation(); }, 1000);
                        }
                    }
                    else if (status == (int)DownloadStatus.Running)
                    { 
                    }
                    else if (status == (int)DownloadStatus.Paused)
                    {
                    }
                    else if (status == (int)DownloadStatus.Pending)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message, Java.Lang.Throwable.FromException(ex));
            }
        }

        public string GetAPKPath(string unZipFolderPath)
        {
            string apkPath = "";
            DirectoryInfo di = new DirectoryInfo(unZipFolderPath);
            FileInfo[] fiList = di.GetFiles("ch4_3_1*.apk",SearchOption.TopDirectoryOnly);
            FileInfo apkFile = fiList.Length > 0 ? fiList[0] : null;
            if (apkFile != null)
            {
                apkPath = apkFile.FullName;
            }
            else
            {
                apkPath = "";
            }
            return apkPath;
        }

        private void ClearFolder(string FolderName)
        {
            //try
            //{
                DirectoryInfo dir = new DirectoryInfo(FolderName);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }

                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    ClearFolder(di.FullName);
                    di.Delete();
                }
            //}
            //catch (System.IO.FileNotFoundException fnte)
            //{

            //}
            //catch (System.IO.DirectoryNotFoundException dnte)
            //{

            //}
        }

        #region 解壓文件夾下所有zip文件（包含子目錄）
        /// <summary>
        /// 拷貝文件夾
        /// </summary>
        /// <param name="srcdir"></param>
        /// <param name="desdir"></param>
        private string UnZipDirectory(string srcdir, string desdir)
        {
            string strErr = "";
            string folderName = srcdir.Substring(srcdir.LastIndexOf("\\") + 1);
            string desfolderdir = desdir + "\\" + folderName;
            if (desdir.LastIndexOf("\\") == (desdir.Length - 1))
            {
                desfolderdir = desdir + folderName;
            }
            string[] filenames = Directory.GetFileSystemEntries(srcdir);
            foreach (string file in filenames)  //遍歷所有的文件和目錄
            {
                if (Directory.Exists(file))  //先當作目錄處理如果存在這個目錄就遞迴Copy該目錄下面的文件
                {
                    string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                    if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }
                    UnZipDirectory(file, desdir);
                }
                else //否則直接解壓文件                
                {
                    if (file.Substring(file.LastIndexOf(".") + 1).ToLower() == "zip")
                    {
                        UnZipFile(file, desfolderdir + "\\", out strErr);
                        if (strErr != "")
                        {
                            break;
                        }
                    }
                    else
                    {
                        string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);
                        srcfileName = desfolderdir + "\\" + srcfileName;
                        if (!Directory.Exists(desfolderdir))
                        {
                            Directory.CreateDirectory(desfolderdir);
                        }
                        System.IO.File.Copy(file, srcfileName, true);
                    }
                }
            }
            return strErr;
        }
        #endregion

        private bool UnZipDirectory(string SrcFolder)
        {
            bool isUnZipDirectory = false;
            DirectoryInfo DI = new DirectoryInfo(SrcFolder);
            foreach (FileInfo FI in DI.GetFiles("*.zip"))
            {
                string strErr = "";
                isUnZipDirectory = UnZipFile(FI.FullName, "", out strErr);
                if (!isUnZipDirectory) break;
            }
            return isUnZipDirectory;
        }

        #region 解壓ZIP
        /// <summary>
        /// 功能：解壓ZIP格式的文件。
        /// </summary>
        /// <param name="zipFilePath">壓缩文件路徑</param>
        /// <param name="unZipDir">解壓文件存放路徑,為空時預設與壓縮文件同一層目錄下，跟壓縮文件同名的文件夾</param>
        /// <param name="err">出错信息</param>
        /// <returns>解壓是否成功</returns>
        public bool UnZipFile(string zipFilePath, string unZipDir, out string err)
        {
            err = "";
            if (zipFilePath == string.Empty)
            {
                err = "壓缩文件不能為空！";
                return false;
            }
            if (!System.IO.File.Exists(zipFilePath))
            {
                err = "壓縮文件不存在！";
                return false;
            }
            //解壓文件夹為空時預設與壓縮文件同一層目錄下，跟壓縮文件同名的文件夾  
            if (unZipDir == string.Empty)
            {
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            }
            if (!Directory.Exists(unZipDir))
            {
                Directory.CreateDirectory(unZipDir);
            }
            if (!unZipDir.EndsWith("/"))
            {
                unZipDir += "/";
            }
            try
            {
                using (ZipInputStream s = new ZipInputStream(System.IO.File.OpenRead(zipFilePath)))
                {
                    //s.Password = "fubon8888";
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        //System.Text.Encoding defEncoding = System.Text.Encoding.Default;
                        //byte[] source = defEncoding.GetBytes(theEntry.Name);
                        //string convertedFileName = defEncoding.GetString(source);
                        //string directoryName = Path.GetDirectoryName(convertedFileName);
                        //string fileName = Path.GetFileName(convertedFileName);
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(unZipDir + directoryName);
                        }
                        if (!directoryName.EndsWith("/"))
                        {
                            directoryName += "/";
                        }
                        if (fileName != String.Empty)
                        {
                            using (FileStream streamWriter = System.IO.File.Create(unZipDir + theEntry.Name))
                            {
                                int size = 102400;
                                byte[] data = new byte[size];
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }//while
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message, Java.Lang.Throwable.FromException(ex));
                return false;
            }
            return true;
        }
        #endregion

        #region 將目錄下所有壓縮檔全部解壓
        public bool UnZipFileForDirectory(string strPathSource)
        {

            return true;
        }

        #endregion

        #region 刪除目錄下所有文件
        ///<summary>
        ///直接删除指定目錄下的所有文件及文件夾(保留目錄)
        ///</summary>
        ///<param name="strPath">文件夾路徑</param>
        ///<returns>執行结果</returns>
        public bool DeleteDir(string strPath)
        {
            try
            {
                //清除空格
                strPath = @strPath.Trim().ToString();
                //判断文件夾是否存在
                if (Directory.Exists(strPath))
                {
                    //獲得文件夾陣列
                    string[] strDirs = Directory.GetDirectories(strPath);

                    //獲得文件陣列
                    string[] strFiles = Directory.GetFiles(strPath);

                    //遍歷所有子文件夾
                    foreach (string strFile in strFiles)
                    {
                        //删除文件夾
                        System.IO.File.Delete(strFile);
                    }
                    //遍歷所有文件
                    foreach (string strdir in strDirs)
                    {
                        //删除文件
                        Directory.Delete(strdir, true);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message);
                Logger.Error(MethodBase.GetCurrentMethod().DeclaringType.ToString(), ex.Message, Java.Lang.Throwable.FromException(ex));
                return false;
            }
        }
        #endregion
    }
}