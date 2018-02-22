using System;
using System.Drawing;

namespace AdminPanel
{
    /// <summary> QR code containing all information for a QR code and the image itself </summary>
    public class QRCode
    {
        /// <summary> enum of the types of QR codes </summary>
        public enum QRTypes { MPL, TOM, Heritage, Conservation, Tokens } // REQUIRED-FIELD : Organizations that can be connected to a QR Code
        /// <summary> This QR codes type </summary>
        public QRTypes QRType;
        /// <summary> The amount of tokens that this QR code is worth </summary>
        public int TokenValue;
        /// <summary> The raw string data </summary>
        public string RawData;
        /// <summary> The actual QR code </summary>
        public QRCoder.QRCode Code;

        // Constructor
        public QRCode(QRTypes type, int tokens)
        {
            QRType = type;
            TokenValue = tokens;
            RawData = Encode(type, TokenValue.ToString());
        }

        // Parses the raw data and returns a QR code
        public static QRCode QRParse(string rawData)
        {
            QRTypes type = QRTypes.Tokens;
            int tokens = 0;


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
                            case "<TYPE>":
                                type = (QRTypes)Enum.Parse(typeof(QRTypes), tempString);
                                break;
                            case "<TOKENS>":
                                tokens = int.Parse(tempString);
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
            QRCode returnCode = new QRCode(type, tokens);
            returnCode.RawData = rawData;
            return returnCode;
        }

        // Encodes the data to create a QR code
        public static string Encode(QRTypes type, string points)
        {
            string encoded = "";
            encoded += "<TYPE>";
            encoded += type.ToString();
            encoded += "<TOKENS>";
            encoded += points;

            return encoded;
        }

        // Returns the bitmap image of the QR code
        public Bitmap GetImage()
        {
            return Code.GetGraphic(8, Color.Black, Color.White, null, 15, 2, true);
        }

        // returns a string for display of the QR code
        public override string ToString()
        {
            return String.Format("{0, -12} | {1, -10}\t", new string[] { QRType.ToString(), TokenValue.ToString()});
        }
    }
}
