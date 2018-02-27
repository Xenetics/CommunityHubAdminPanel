using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AdminPanel
{
    public static class Config
    {
        private const string file = "/config.dat";
        public static Dictionary<string, string> settings;

        public static bool LoadConfigFile()
        {
            try
            {
                if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + file))
                {
                    settings = new Dictionary<string, string>();
                    using (StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + file))
                    {
                        string line = "";
                        do
                        {
                            line = sr.ReadLine();
                            if(line == "" || line == null)
                            {
                                line = null;
                                break;
                            }
                            string[] sep = line.Split('?');
                            settings.Add(sep[0], sep[1]);
                        }while (line != null) ;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Config file missing or malformed. Look in /n" + Path.GetDirectoryName(Application.ExecutablePath) + file, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool CreateConfigFile()
        {
            if (!File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + file))
            {
                FileStream fs = File.Create(Path.GetDirectoryName(Application.ExecutablePath) + "/config.dat");
                fs.Close();
                using (StreamWriter config = new StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + file, true))
                {
                    config.WriteLine("StorageKey?");
                    config.WriteLine("POIContainer?");
                    config.WriteLine("QRContainer?");
                    config.WriteLine("EventsContainer?");
                    config.WriteLine("ProductsContainer?");
                    config.WriteLine("TriviaContainer?");
                    config.WriteLine("UserContainer?");
                    config.WriteLine("AdminContainer?");
                    config.WriteLine("SierraURL?");
                    config.WriteLine("SierraSecret?");
                    config.WriteLine("AdminName?");
                    config.WriteLine("AdminPass?");
                }
                Process.Start("notepad.exe", Path.GetDirectoryName(Application.ExecutablePath) + file);

                MessageBox.Show("Fill out config file and restart Admin Panel.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            return false;
        }

        public static async void CreateInitialAdmin(CloudBlobClient blobClient)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(AdminUtilities.containerName);
            BlobContinuationToken blobConToken = null;
            BlobResultSegment result;

            do
            {
                result = await container.ListBlobsSegmentedAsync(blobConToken);
                blobConToken = result.ContinuationToken;
            } while (blobConToken != null);

            List<string> blobs = new List<string>();
            blobs.AddRange(result.Results.Cast<CloudBlockBlob>().Select(b => b.Name));

            if (blobs.Count == 9)
            {
                await AdminUtilities.CreateremoteData(settings["AdminName"], settings["AdminPass"], AdminUtilities.Organization.Admin.ToString(), AdminUtilities.Clearance.Admin.ToString(), blobClient);

                StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + file);
                List<string> lines = new List<string>();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                sr.Close();

                lines.RemoveAt(lines.Count - 1);
                lines.RemoveAt(lines.Count - 1);
                File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + file);
                FileStream fs1 = File.Create(Path.GetDirectoryName(Application.ExecutablePath) + file);
                fs1.Close();
                File.WriteAllLines(Path.GetDirectoryName(Application.ExecutablePath) + file, lines.ToArray());
            }
        }
    }
}
