using System;
using System.Drawing;

namespace AdminPanel
{
    /// <summary> Product that tokens can be used to redeem </summary>
    class Product
    {
        /// <summary> Enum of organizations a product organization </summary>
        public enum ProductOrg { MPL, TOM, Heritage, Conservation } // REQUIRED-FIELD : Organizations that have products in the hub
        /// <summary> This Products organization </summary>
        public ProductOrg Organization;
        /// <summary> The Name of the product </summary>
        public string ProductName;
        /// <summary> Full value of the product </summary>
        public string ProductValue;
        /// <summary> The % discount on this product </summary>
        public string Discount;
        /// <summary> The amount of tokens that this QR code is worth </summary>
        public long TokenValue;
        /// <summary> The raw string data </summary>
        public string RawData;
        /// <summary> The actual QR code </summary>
        public QRCoder.QRCode Code;

        // Constructor
        public Product(string name, string value, string discount, ProductOrg type, long tokenCost)
        {
            ProductName = name.Replace("/", " ");

            ProductValue = (value.Contains("."))? (value):(value + ".00");
            Discount = discount;
            Organization = type;
            TokenValue = tokenCost;
            RawData = Encode(ProductName, ProductValue, Discount, Organization, TokenValue.ToString());
        }

        // Parses the raw data and returns a QR code
        public static Product Parse(string rawData)
        {
            string name = "";
            string value = "";
            string discount = "";
            ProductOrg type = ProductOrg.MPL;
            long tokens = 0;

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
                            case "<NAME>":
                                name = tempString;
                                break;
                            case "<VALUE>":
                                value = tempString;
                                break;
                            case "<DISCOUNT>":
                                discount = tempString;
                                break;
                            case "<TYPE>":
                                type = (ProductOrg)Enum.Parse(typeof(ProductOrg), tempString);
                                break;
                            case "<TOKENS>":
                                tokens = long.Parse(tempString);
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
            Product returnCode = new Product(name, value, discount, type, tokens);
            returnCode.RawData = rawData;
            return returnCode;
        }

        // Encodes the data to create a QR code
        public static string Encode(string name, string value, string discount, ProductOrg type, string points)
        {
            string encoded = "";
            encoded += "<NAME>";
            encoded += name;
            encoded += "<VALUE>";
            encoded += value;
            encoded += "<DISCOUNT>";
            encoded += discount;
            encoded += "<TYPE>";
            encoded += type.ToString();
            encoded += "<TOKENS>";
            encoded += points;

            return encoded;
        }

        // Returns the bitmap image of the QR code
        public Bitmap GetImage()
        {
            return Code.GetGraphic(5, Color.Black, Color.White, null, 15, 2, true);
        }

        // returns a string for display of the QR code
        public override string ToString()
        {
            return String.Format("{0, -13} | {1, -32} | {2, -6} | {3, -3} | {4, -16}\t", new string[] { Organization.ToString(), ProductName, ProductValue, Discount, TokenValue.ToString() });
        }
    }
}
