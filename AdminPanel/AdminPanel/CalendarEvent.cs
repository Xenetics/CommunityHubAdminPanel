using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AdminPanel
{
    // Hold information for one event and is also a table entity compatible with a blobstore table
    public class CalendarEvent : TableEntity
    {
        /// <summary> Start date for the event </summary>
        public string Start { get; set; }
        /// <summary> End date for the event </summary>
        public string End { get; set; }
        /// <summary> Event name </summary>
        public string Name { get; set; }
        /// <summary> Details of the event </summary> // TODO: Markdown compatible
        public string Details { get; set; }
        /// <summary> The organization that owns this event </summary>
        public string Org { get; set; }

        // Default constructor
        public CalendarEvent() { }
        // Constructor
        public CalendarEvent(DateTime start, DateTime end, string name, string details, string org)
        {
            this.PartitionKey = DateToPartitionMonth(start);
            this.RowKey = NameToRow(name);
            Start = start.ToString();
            End = end.ToString();
            Name = name;
            Details = details;
            Org = org;
        }

        // Converts a date to a string YYYYMMDD
        public static string DateToPartition(DateTime date)
        {
            return date.Year.ToString().ToLower() + date.Month.ToString("00").ToLower() + date.Day.ToString("00").ToLower();
        }

        // Converts a date to a string YYYYMM
        public static string DateToPartitionMonth(DateTime date)
        {
            return date.Year.ToString().ToLower() + date.Month.ToString("00").ToLower();
        }

        // Converts date to string YY
        public static string DateToPartitionYear(DateTime date)
        {
            return date.Year.ToString().ToLower();
        }

        // Convets name to blob compatible string
        public static string NameToRow(string name)
        {
            string newName = name.ToLower();
            string[] split = newName.Split(null);
            newName = "";
            for(int i = 0; i < split.Length; ++i)
            {
                newName += split[i];
            }
            return newName;
        }

        // Returns the start date as a datetime
        public DateTime GetStart()
        {
            return DateTime.Parse(Start);
        }

        // Returns the end date as a datetime
        public DateTime GetEnd()
        {
            return DateTime.Parse(End);
        }

        // Returns a string of the event for display in a list
        public override string ToString()
        {
            return String.Format("{0, -32} | {1, -12} | {2, -12}\t", new string[] { Name, GetStart().Date.ToString("MM,dd,yyyy"), GetEnd().Date.ToString("MM,dd,yyyy") });
        }

        // Overloads == equivinant opperator for 2 calendar events
        public static bool operator ==(CalendarEvent left, CalendarEvent right)
        {
            if (object.ReferenceEquals(right, null) || object.ReferenceEquals(left, null))
            {
                if(object.ReferenceEquals(right, null) && object.ReferenceEquals(left, null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            bool equal = (left.Name == right.Name) ?(true):(false);
            equal = (left.Start == right.Start) ? (true) : (false);
            equal = (left.End == right.End) ? (true) : (false);
            equal = (left.Details == right.Details) ? (true) : (false);
            equal = (left.Org == right.Org) ? (true) : (false);

            return equal;
        }

        // Overloads != not equivinant opperator for 2 calendar events
        public static bool operator !=(CalendarEvent left, CalendarEvent right)
        {
            if (object.ReferenceEquals(right, null) || object.ReferenceEquals(left, null))
            {
                if (object.ReferenceEquals(right, null) && object.ReferenceEquals(left, null))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            bool equal = (left.Name != right.Name) ? (true) : (false);
            equal = (left.Start != right.Start) ? (true) : (false);
            equal = (left.End != right.End) ? (true) : (false);
            equal = (left.Details != right.Details) ? (true) : (false);
            equal = (left.Org != right.Org) ? (true) : (false);

            return equal;
        }
    }
}
