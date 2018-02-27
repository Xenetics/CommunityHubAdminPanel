using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Runtime.InteropServices;

public static class AdminUtilities
{
    /// <summary> blobstore Container for admin data </summary>
    public static string containerName = "admin";
    /// <summary> Enum for organitations that an admin can be a part of </summary>
    public enum Organization { MPL, TOM, Heritage, Conservation, Admin } // REQUIRED-FIELD : Organisations that have admin privilages
    /// <summary> Enum for the clearence levels an admin can have </summary>
    public enum Clearance { Employee, Manager, Admin }
    /// <summary> Currently logged in admin </summary>
    public static AdminData adminUserData;

    /// <summary> The exact Admin Struct
    /// THIS SHOULD NEVER CHANGE </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AdminData // Never remove anything from this structure or it will not be backwards compatible. You may add
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Username;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Password;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string AdminType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ClearanceLevel;

        public override string ToString()
        {
            return String.Format("{0, -16} | {1, -32} | {2, -12} | {3, -12}\t", new string[] { Username, Password, AdminType, ClearanceLevel });
        }
    }

    // Checks remotely to see if a admin exists and if the password is correct
    public static async Task<string> CheckRemoteData(string username, string pass, CloudBlobClient blobClient)
    {
        if (username.Length > 0 && pass.Length > 0)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockblob = container.GetBlockBlobReference(username);

            if (await blockblob.ExistsAsync())
            {
                byte[] data = new byte[blockblob.Properties.Length];
                await blockblob.DownloadToByteArrayAsync(data, 0);
                AdminData aud = BytesToUserData(data);

                if (aud.Username != username || aud.Password != pass)
                {
                    return "Incorrect";
                }
                adminUserData = aud;
                return "Correct";
            }
            return "NonExistant";
        }
        return "Empty";
    }

    // Gets the admin data for a specific user on the blobstore
    public static async Task<AdminData> GetRemoteData(string cardnum, CloudBlobClient blobClient)
    {
        if (cardnum.Length > 0)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockblob = container.GetBlockBlobReference(cardnum);
            if (await blockblob.ExistsAsync())
            {
                byte[] data = new byte[blockblob.Properties.Length];
                await blockblob.DownloadToByteArrayAsync(data, 0);
                AdminData ud = BytesToUserData(data);

                return ud;
            }
            return default(AdminData);
        }
        return default(AdminData);
    }

    // Creates a new admin and pushes it to the admin blobstore
    public static async Task<string> CreateremoteData(string userName, string pass, string type, string clearance, CloudBlobClient blobClient)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockblob = container.GetBlockBlobReference(userName);
        if (await blockblob.ExistsAsync())
        {
            return "UserExists";
        }
        AdminData ad;
        ad.Username = userName;
        ad.Password = pass;
        ad.AdminType = type;
        ad.ClearanceLevel = clearance;

        byte[] data = UserDataToBytes(ad);

        await blockblob.UploadFromByteArrayAsync(data, 0, data.Length);

        return "Created";
    }

    // Serielizes the Admin for transport to the blob store
    private static byte[] UserDataToBytes(AdminData ud)
    {
        int size = Marshal.SizeOf(ud);
        byte[] buffer = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(ud, ptr, true);
        Marshal.Copy(ptr, buffer, 0, size);
        Marshal.FreeHGlobal(ptr);

        return buffer;
    }

    // Deserializes data from blob store into admindata
    public static AdminData BytesToUserData(byte[] bytes)
    {
        AdminData ud = new AdminData();
        int size = Marshal.SizeOf(ud);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(bytes, 0, ptr, size);
        ud = (AdminData)Marshal.PtrToStructure(ptr, ud.GetType());
        Marshal.FreeHGlobal(ptr);
        return ud;
    }

    // Determines if a user is cleared to modify information
    public static bool ModificationClearenceCheck(string organization)
    {
        if ((adminUserData.AdminType == Organization.Admin.ToString())
        || ((adminUserData.AdminType == organization)
        && (adminUserData.ClearanceLevel != Clearance.Employee.ToString())))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Determines if a user is cleared to View information
    public static bool ViewClearenceCheck(string organization)
    {
        if ((adminUserData.AdminType == Organization.Admin.ToString())
        || (adminUserData.AdminType == organization))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}