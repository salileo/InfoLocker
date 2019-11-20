using InfoLocker.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfoLocker.FileSystem;

namespace InfoLocker
{
    public class Testing
    {
        public static void Start()
        {
            ValidateReaderWriter();
            ValidateEncryptor();
            ValidateSynchronizer();
            ValidateOneDrive();
            ValidateLocalStorage();
        }

        private static void ValidateReaderWriter()
        {
            Cabinet cab1 = new Cabinet("dummy");

            Folder f1 = new Folder("f1");
            Folder f2 = new Folder("f2");
            Folder f3 = new Folder("f3");

            Card c1 = new Card("c1");
            Card c2 = new Card("c2");
            Card c3 = new Card("c3");
            Card c4 = new Card("c4");
            Card c5 = new Card("c5");
            Card c6 = new Card("c6");
            Card c7 = new Card("c7");
            Card c8 = new Card("c8");

            MultiLineEntry m1 = new MultiLineEntry("m1");
            m1.Content = "this is m1";
            MultiLineEntry m2 = new MultiLineEntry("m2");
            MultiLineEntry m3 = new MultiLineEntry("m3");
            m2.Content = "this is m3\r\n";
            MultiLineEntry m4 = new MultiLineEntry("m4");
            m4.Content = "this is \r\nm4";
            MultiLineEntry m5 = new MultiLineEntry("m5");
            MultiLineEntry m6 = new MultiLineEntry("m6");
            m6.Content = "this is \r\n\r\n\r\nm6";

            SingleLineEntry s1 = new SingleLineEntry("s1");
            s1.Content = "this is s1";
            SingleLineEntry s2 = new SingleLineEntry("s2");
            SingleLineEntry s3 = new SingleLineEntry("s3");
            s3.Content = "this is s3";
            SingleLineEntry s4 = new SingleLineEntry("s4");
            SingleLineEntry s5 = new SingleLineEntry("s5");
            s5.Content = "this is s5";
            SingleLineEntry s6 = new SingleLineEntry("s6");
            s6.Content = "this is s6";

            c1.AddChild(m1);
            c2.AddChild(m2);
            c2.AddChild(s1);
            c4.AddChild(s2);
            c5.AddChild(m3);
            c5.AddChild(m4);
            c6.AddChild(s3);
            c6.AddChild(s4);
            c7.AddChild(m5);
            c8.AddChild(s5);
            c8.AddChild(m6);
            c8.AddChild(s6);

            f1.AddChild(c1);
            f1.AddChild(c2);
            f1.AddChild(c3);
            f3.AddChild(c4);
            f3.AddChild(c5);
            f3.AddChild(c6);
            f3.AddChild(c7);
            f3.AddChild(c8);

            cab1.AddChild(f1);
            cab1.AddChild(f2);
            cab1.AddChild(f3);

            string data;
            ReaderWriter.WriteToString(cab1, out data);
            Cabinet cab2 = ReaderWriter.ReadFromString(data);
            if (!cab1.IsEqual(cab2))
            {
                throw new Exception();
            }
        }

        private static void ValidateEncryptor()
        {
            string data = "this is some random text. ";
            for (int i = 0; i < 10; i++)
            {
                data += data;
            }

            string encrypted = Encryptor.Encrypt(data, "sumi1234");
            string final = Encryptor.Decrypt(encrypted, "sumi1234");

            if (final != data)
            {
                throw new Exception();
            }
        }

        private static void ValidateSynchronizer()
        {
            /*
             * - property change in one node only
             * - property change in 2 nodes in the same hierarchy (2 cases for dates)
             * - property change in 2 siblings (2 cases for dates)
             * - children change in one node only (add and remove and cases for dates)
             * - children change in 2 nodes in the same hierarchy (add and remove and cases for dates)
             * - children change in 2 siblings (add and remove and cases for dates)
             * - property change and children change of same node (add and remove and cases for dates)
             * - property change with children change in the same hierarchy
             * - property change with children change in siblings
             */
        }

        private static async void ValidateOneDrive()
        {
            OneDriveFile file = await OneDriveFile.Create(@"\salil1\salil2\test.txt", "this is salil");
            if (file == null)
            {
                throw new Exception();
            }
            
            file = await OneDriveFile.Open(@"\salil1\salil2\test.txt");
            if (file == null)
            {
                throw new Exception();
            }

            string data = await file.Read();
            if (data != "this is salil")
            {
                throw new Exception();
            }

            await file.Write("this is kapoor");
            data = await file.Read();
            if (data != "this is kapoor")
            {
                throw new Exception();
            }

            bool success = await file.Delete();
            if (!success)
            {
                throw new Exception();
            }

            success = await file.Parent.Delete();
            if (!success)
            {
                throw new Exception();
            }

            success = await file.Parent.Parent.Delete();
            if (!success)
            {
                throw new Exception();
            }
        }

        private static async void ValidateLocalStorage()
        {
            StorageFile file = await StorageFile.Create(@"\salil1\salil2\test.txt", "this is salil", false);
            if (file == null)
            {
                throw new Exception();
            }

            file = await StorageFile.Open(@"\salil1\salil2\test.txt", false);
            if (file == null)
            {
                throw new Exception();
            }

            string data = await file.Read();
            if (data != "this is salil")
            {
                throw new Exception();
            }

            await file.Write("this is kapoor");
            data = await file.Read();
            if (data != "this is kapoor")
            {
                throw new Exception();
            }

            bool success = await file.Delete();
            if (!success)
            {
                throw new Exception();
            }

            success = await file.Parent.Delete();
            if (!success)
            {
                throw new Exception();
            }

            success = await file.Parent.Parent.Delete();
            if (!success)
            {
                throw new Exception();
            }
        }
    }
}
