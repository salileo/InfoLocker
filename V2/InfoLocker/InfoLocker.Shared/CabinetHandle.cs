using InfoLocker.Parts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InfoLocker.FileSystem;
using System.Threading.Tasks;

namespace InfoLocker
{
    public class CabinetHandle
    {
        private CommonFile file;
        private Cabinet cabinet;

        private CabinetHandle(CommonFile file, Cabinet cabinet)
        {
            this.file = file;
            this.cabinet = cabinet;
        }

        public static async Task<CabinetHandle> Open(string path, bool oneDrive, string password)
        {
            CommonFile file = null;
            if (oneDrive)
            {
                file = await OneDriveFile.Open(path);
            }
            else
            {
                file = await StorageFile.Open(path, false);
            }

            if (file == null)
            {
                throw new Exception(string.Format("Could not open file {0}", path));
            }

            string data = await file.Read();
            data = Encryptor.Decrypt(data, password);

            Cabinet cab = ReaderWriter.ReadFromString(data);
            if (cab.Password != password)
            {
                throw new Exception("Invalid Password");
            }

            CabinetHandle handle = new CabinetHandle(file, cab);
            return handle;
        }

        public async void Save()
        {
            string data = null;
            ReaderWriter.WriteToString(this.cabinet, out data);

            data = Encryptor.Encrypt(data, this.cabinet.Password);
            await this.file.Write(data);
        }

        public async void SaveAs(string path, bool oneDrive)
        {
            CommonFile tmpFile = null;
            if (oneDrive)
            {
                tmpFile = await OneDriveFile.Create(path, " ");
            }
            else
            {
                tmpFile = await StorageFile.Create(path, " ", false);
            }

            if (tmpFile == null)
            {
                throw new Exception(string.Format("Could not open file {0}", path));
            }

            string data = null;
            ReaderWriter.WriteToString(this.cabinet, out data);

            data = Encryptor.Encrypt(data, this.cabinet.Password);
            await tmpFile.Write(data);

            this.file = tmpFile;
        }

        public void Close()
        {
            this.file.Close();
        }
    }
}
