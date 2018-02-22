using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPanel
{
    /// <summary> Point class represents data for a location pin </summary>
    class Point
    {
        /// <summary> Type of location (organisation or toekn) </summary>
        public string type = "";
        /// <summary> Name or title for the location </summary>
        public string label = "";
        /// <summary> Angle -90 and 90 degree north to south </summary>
        public string latitude = "";
        /// <summary> Angle -180 and 180 degree east to west </summary>
        public string longitude = "";
        /// <summary> Address of this location </summary>
        public string address = "";
        /// <summary> Raw data of the location from the blobstore </summary>
        public string rawData = "";
        /// <summary> How many tokens this location is worth </summary>
        public int value = 0;
        /// <summary> The pins initial appearance date </summary>
        public string startDate = "";
        /// <summary> The datetime after which the location cese to exist </summary>
        public string endDate = "";

        // Point constructor
        public Point(string _type, string _label, string _lat, string _lon, string _address, int _value, string _rawData, string _startDate, string _endDate)
        {
            type = _type;
            label = _label;
            latitude = _lat;
            longitude = _lon;
            address = _address;
            rawData = _rawData;
            value = _value;
            startDate = _startDate;
            endDate = _endDate;
        }

        // Takes in raw string blob data and returns a Point
        public static Point ParsePinData(string rawData)
        {
            string type = "";
            string label = "";
            string lat = "";
            string lon = "";
            string address = "";
            int value = 0;
            string startDate = "";
            string endDate = "";

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
                                type = tempString;
                                break;
                            case "<LABEL>":
                                label = tempString;
                                break;
                            case "<LAT>":
                                lat = tempString;
                                break;
                            case "<LONG>":
                                lon = tempString;
                                break;
                            case "<ADDRESS>":
                                address = tempString;
                                break;
                            case "<VALUE>":
                                value += int.Parse(tempString);
                                break;
                            case "<START>":
                                startDate = tempString;
                                break;
                            case "<END>":
                                endDate = tempString;
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
            return new Point(type, label, lat, lon, address, value, rawData, startDate, endDate);
        }

        // converts point to string for easy listing
        public override string ToString()
        {
            return String.Format("{0, -15} | {1, -15} | {2, -15} | {3, -5} | {4, -10} | {5, -10} | {6, -30} | {7, -30}\t", new string[] { type, latitude, longitude, value.ToString(), startDate, endDate, label, address });
        }
    }
}
