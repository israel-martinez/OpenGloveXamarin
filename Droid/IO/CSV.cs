using System.Collections.Generic;
using Android.OS;
using Java.IO;

namespace OpenGloveApp.Droid.IO
{
    public class CSV
    {   
        public string mFolderName;
        public string mFileName;
        public string mExternalStoragePath;

        public CSV(string mFolderName, string mFileName)
        {
            this.mFolderName = mFolderName;
            this.mFileName = mFileName;
        }

        public void Write(List<long> values, string columnTitle)
        {
            try
            {
                
                File folder = new File(Environment.ExternalStorageDirectory
                        + "/" + mFolderName);

                bool success = true;
                if (!folder.Exists())
                    success = folder.Mkdirs();

                if (success)
                {
                    string pathName = folder + "/" + mFileName; //TODO folder.ToString() ?
                    mExternalStoragePath = pathName;

                    PrintWriter pw = new PrintWriter(new File(pathName));
                    pw.Write(columnTitle + '\n');

                    foreach (long value in values)
                    {
                        pw.Write(value.ToString() + '\n');
                    }
                    pw.Close();
                    System.Diagnostics.Debug.WriteLine("done!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Folder no created");
                }

            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }

        override
        public string ToString(){
            return "FolderName: " + mFolderName +
                "\nFileName: " + mFileName +
                "\nExternal StoragePath:" + mExternalStoragePath;
        }
    }
}
