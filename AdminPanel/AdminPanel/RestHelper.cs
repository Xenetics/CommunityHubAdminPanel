using System;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;


public static class RestHelper
{
    // An object that holds info recieved as an authorization
    public class AuthObject
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }

    /// <summary> Library sierra server URL </summary>
    private static string m_url = "SIERRA SERVER URL"; // REQUIRED-FIELD : Library Sierra server URL
    /// <summary> Secret auth key for Sierra api </summary>
    private static string m_authSecret = "SIERRA API KEY"; // REQUIRED-FIELD : Library Sierra general API Key
    /// <summary> The current auth object </summary>
    public static AuthObject currentAuth;
    /// <summary> The time of the last auth </summary>
    public static DateTime lastAuth { private set; get; }

    /// <summary> Authenticates with the sierra server so prepare for making calls </summary>
    public static void Authenticate()
    {
        string url = m_url;
        url += "/token";
        byte[] buffer = Encoding.UTF8.GetBytes("grant_type=client_credentials");
        WebUtility.UrlEncodeToBytes(buffer, 0, buffer.Length);

        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = buffer.Length;
        request.Headers["Authorization"] = "Basic " + m_authSecret;

        Stream stream = request.GetRequestStream();
        stream.Write(buffer, 0, buffer.Length);
        stream.Close();

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
        {
            string result = sr.ReadToEnd();
            try
            {
                currentAuth = JsonConvert.DeserializeObject<AuthObject>(result);
                lastAuth = DateTime.Now;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error >>>>>>>>> " + e.Message);
            }
        }
    }

    // Uses the library card number to see if a patron exists
    public static bool GetUser(string libCard)
    {
        string url = m_url;
        url += "/patrons";
        url += "/find?barcode=";
        url += libCard;

        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
        request.Method = "GET";
        request.ContentType = "application/json;charset=UTF-8";
        request.Accept = "application/json";
        request.Headers["Authorization"] = currentAuth.token_type + " " + currentAuth.access_token;

        try
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}