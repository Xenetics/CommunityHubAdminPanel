using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Runtime.InteropServices;

public static class UserUtilities
{
    /// <summary> Blobstore container where users are stored </summary>
    public static string containerName = "users"; // REQUIRED-FIELD : Container for the Users of the app can be created in azure portal or microsoft azure storage explorer
    /// <summary> Minimum length of a password </summary>
    public static int minPassLength = 6;
    /// <summary> Minimum length of a username </summary>
    public static int minUsernameLength = 3;
    /// <summary> The exact length of a Library card number </summary>
    public static int libraryCardLength = 14;
    /// <summary> Maximum input length for any saved input </summary>
    public static int MaxInputLength = 32;

    /// <summary> The exact user Struct for both game and admin panel
    /// THIS SHOULD NEVER CHANGE </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UserData // Never remove anything from this structure or it will not be backwards compatible. You may add
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string CardNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Username;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Password;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string EMail;
        public int CurrentPoints;
        public int TotalPoints;
        public long LastModified;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Active; // Active valid inputs "Active" "Banned" "TempBan[0-9]" "Deactivated"

        public override string ToString()
        {
            return String.Format("{0, -16} | {1, -32} | {2, -12} | {3, -12} | {4, -16}\t", new string[] { Username, EMail, CurrentPoints.ToString(), TotalPoints.ToString(), Active });
        }
    }

    // Gets the user data for a specific user on the blobstore
    public static async Task<UserData> GetRemoteData(string cardnum, CloudBlobClient blobClient)
    {
        if (cardnum.Length > 0)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockblob = container.GetBlockBlobReference(cardnum);
            if (await blockblob.ExistsAsync())
            {
                UserData ud = Parse(blockblob.DownloadText());

                return ud;
            }
            return default(UserData);
        }
        return default(UserData);
    }

    /// Checks a password for viability based on length and contents
    private static bool PasswordViability(string pass)
    {
        if (pass.Length < minPassLength)
        {
            return false;
        }
        if (!pass.Any(c => char.IsDigit(c)))
        {
            return false;
        }
        if (!pass.Any(c => char.IsUpper(c)))
        {
            return false;
        }
        return true;
    }

    // Serielizes the userdata for transport to the blob store
    private static byte[] UserDataToBytes(UserData ud)
    {
        int size = Marshal.SizeOf(ud);
        byte[] buffer = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(ud, ptr, true);
        Marshal.Copy(ptr, buffer, 0, size);
        Marshal.FreeHGlobal(ptr);

        return buffer;
    }

    // Deserializes data from blob store into userdata
    public static UserData BytesToUserData(byte[] bytes)
    {
        UserData ud = new UserData();
        int size = Marshal.SizeOf(ud);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(bytes, 0, ptr, size);
        ud = (UserData)Marshal.PtrToStructure(ptr, ud.GetType());
        Marshal.FreeHGlobal(ptr);
        return ud;
    }

    // Parses the raw data and returns a UserDAta
    public static UserData Parse(string rawData)
    {
        string card = "";
        string name = "";
        string pass = "";
        string email = "";
        int current = 0;
        int total = 0;
        long lastmod = 0;
        string status = "";

        bool tagged = false;
        string stringTag = "";
        string tempString = "";
        for (int i = 0; i < rawData.Length; i++)
        {
            if (!tagged)
            {
                stringTag += rawData[i];
                if (rawData[i] == '>')
                {
                    tagged = true;
                }
            }
            else
            {
                if (rawData[i] != '<')
                {
                    tempString += rawData[i];
                }
                if (rawData[i] == '<' || i + 1 == rawData.Length)
                {
                    switch (stringTag)
                    {
                        case "<CARD>":
                            card = tempString;
                            break;
                        case "<USER>":
                            name = tempString;
                            break;
                        case "<PASS>":
                            pass = tempString;
                            break;
                        case "<EMAIL>":
                            email = tempString;
                            break;
                        case "<CURRENT>":
                            current = int.Parse(tempString);
                            break;
                        case "<TOTAL>":
                            total = int.Parse(tempString);
                            break;
                        case "<LASTMOD>":
                            lastmod = long.Parse(tempString);
                            break;
                        case "<STATUS>":
                            status = tempString;
                            break;
                        default:
                            break;
                    }
                    stringTag = "<";
                    tempString = "";
                    tagged = false;
                }
            }
        }
        UserData returnData = new UserData();
        returnData.CardNumber = card;
        returnData.Username = name;
        returnData.Password = pass;
        returnData.EMail = email;
        returnData.CurrentPoints = current;
        returnData.TotalPoints = total;
        returnData.LastModified = lastmod;
        returnData.Active = status;
        return returnData;
    }

    // Encodes UserDate to string
    public static string UserDataToString(UserData user, bool newUser = false)
    {
        string rawData = (newUser) ? ("!") : ("");
        rawData += "<CARD>" + user.CardNumber;
        rawData += "<USER>" + user.Username;
        rawData += "<PASS>" + user.Password;
        rawData += "<EMAIL>" + user.EMail;
        rawData += "<CURRENT>" + user.CurrentPoints;
        rawData += "<TOTAL>" + user.TotalPoints;
        rawData += "<LASTMOD>" + user.LastModified;
        rawData += "<STATUS>" + user.Active;
        return rawData;
    }
}