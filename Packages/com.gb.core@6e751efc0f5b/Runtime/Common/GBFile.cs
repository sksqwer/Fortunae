using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace GB
{
    public struct GBFile
    {
        public static GBFile SaveFile(string fileName, string json,bool gzip = false, bool encrypt = false, bool editorFolder = false)
        {
            var file = new GBFile();
            file.json = json;
            file.Save(fileName,gzip,encrypt,editorFolder);
            return file;
        }

        public static GBFile LoadFile(string fileName, bool gzip = false, bool encrypt = false, bool editorFolder = false)
        {
            var file = new GBFile();
            file.Load(fileName,gzip,encrypt,editorFolder);
            return file;
        }

        public static void DeleteFile(string fileName,bool editorFolder = false)
        {
            string path = string.Empty;
            if (editorFolder) path = Path.Combine(Application.dataPath,  fileName + ".data");
            else path = Path.Combine(Application.persistentDataPath, "GBFile/" + fileName + ".data");
            
            FileInfo info = new FileInfo(path);
            if(info.Exists)
            info.Delete();
            
            string pKey = "GBFILE_" + fileName;
            PlayerPrefs.DeleteKey(pKey);
        }


        public string json;

        public T From<T>()
        {
            return json.FromJson<T>();
        }

        public bool Load(string fileName, bool gzip = false, bool encrypt = false, bool EditorFolder = false)
        {
            string pKey = "GBFILE_" + fileName;
            string key = PlayerPrefs.GetString(pKey, null);
            if (string.IsNullOrEmpty(key)) return false;

            string path = string.Empty;
            if (EditorFolder) path = Path.Combine(Application.dataPath, "GBFile/" + fileName + ".data");
            else path = Path.Combine(Application.persistentDataPath, "GBFile/" + fileName + ".data");
            if (!File.Exists(path)) return false;

            string data = File.ReadAllText(path);

            try
            {
                if (encrypt) data = GB.AESCrypto.Decrypt(key, data);
            }
            catch
            {
                Debug.LogError("GBFile Load AESCrypto.Decrypt Error : " + fileName);
                return false;
            }

            try
            {
                if (gzip) data = GB.Gzip.DeCompression(data);
            }
            catch
            {
                Debug.LogError("GBFile Load GB.Gzip.DeCompression Error : " + fileName);
                return false;
            }

            json = data;

            return true;
        }


        public void Save(string fileName, bool gzip = false, bool encrypt = false, bool EditorFolder = false)
        {

            string directoryPath = string.Empty;
            string data = json;

            if (EditorFolder) directoryPath = Path.Combine(Application.dataPath, "GBFile");
            else directoryPath = Path.Combine(Application.persistentDataPath, "GBFile");

            DirectoryInfo info = new DirectoryInfo(directoryPath);
            if (!info.Exists) info.Create();

            string pKey = "GBFILE_" + fileName;

            if (gzip) data = GB.Gzip.Compression(json);
            if (encrypt)
            {
                //저장 할때 마다 키를 변화
                var k = GB.AESCrypto.GenerateKeyOrIV(pKey + GenerateRandomString(10));
                PlayerPrefs.SetString(pKey, k);
                data = GB.AESCrypto.Encrypt(k, data);
            }
            else
            {
                PlayerPrefs.SetString(pKey, pKey);
            }

            string fullPath = Path.Combine(directoryPath, fileName + ".data");
            File.WriteAllText(fullPath, data); // 파일에 저장

        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_=*&^%$#@!";
            StringBuilder stringBuilder = new StringBuilder(length);

            System.Random random = new System.Random();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }

    }

}