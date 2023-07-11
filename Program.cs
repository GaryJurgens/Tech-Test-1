// Mix Telematics Technical Interview 1
// By Geert Dirk (Gary) Jurgens 11-07-2023
namespace TechTest1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var records = new List<Record>();
            // build a list of fixed cordinates, can do this from a file or database
            List<CordinateModel> fixedCordinates = new List<CordinateModel>();
            fixedCordinates.Add(new CordinateModel { Position = 1, X = 34.544909f, Y = -102.100843f });
            fixedCordinates.Add(new CordinateModel { Position = 2, X = 32.345544f, Y = -99.123124f });
            fixedCordinates.Add(new CordinateModel { Position = 3, X = 33.234235f, Y = -100.214124f });
            fixedCordinates.Add(new CordinateModel { Position = 4, X = 35.195739f, Y = -95.348899f });
            fixedCordinates.Add(new CordinateModel { Position = 5, X = 31.895839f, Y = -97.789573f });
            fixedCordinates.Add(new CordinateModel { Position = 6, X = 32.895839f, Y = -101.789573f });
            fixedCordinates.Add(new CordinateModel { Position = 7, X = 34.115839f, Y = -100.225732f });
            fixedCordinates.Add(new CordinateModel { Position = 8, X = 32.335839f, Y = -99.992232f });
            fixedCordinates.Add(new CordinateModel { Position = 9, X = 33.535339f, Y = -94.792232f });
            fixedCordinates.Add(new CordinateModel { Position = 10, X = 32.234235f, Y = -100.222222f });

            //enclosing the stream in a using statement will ensure the stream is closed and disposed of correctly
            using (var stream = new FileStream("VehiclePositions.dat", FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream, Encoding.Default))
                {
                    //read the file until the end of the stream is reached
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        //create the new record
                        var record = new Record
                        {
                            Int32Field = reader.ReadInt32(),
                            StringField = ReadNullTerminatedString(reader),
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            // cast from Uint64 to long so it fits the DateTime parameters, it is Unlikley that the full range of the Uint64 will be used, as that will be millions of years in the future or past
                            DateTimeField = DateTimeOffset.FromUnixTimeSeconds((long)reader.ReadUInt64()).DateTime
                        };

                        records.Add(record);
                    }
                }
            }
            //loop through the fixed cordinates and find the nearest record
            foreach (var item in fixedCordinates)
            {
                // find the nearest record to the fixed cordinate, by using a Linq Order Statment and taking the 1st record in the list, as that is the closest. You can modify this to say Take(10) to find the 10 closest to the target
                var nearestRecords = records.OrderBy(r => Distance(r, item)).Take(1).ToList();
                Console.WriteLine();

                Console.WriteLine(item.Position + " Nearest Records");
                Console.WriteLine();
                // make the output look nice
                Console.WriteLine("{0,-10} {1,-30} {2,-15} {3,-15} {4,-30} {5,-30}", "VehicleID", "Registration", "Longitude", "Latitude", "Recorded Time UTC", "Dis from Target");
                Console.WriteLine(new string('-', 110)); // Line separator

                foreach (var record in nearestRecords)
                {
                    Console.WriteLine("{0,-10} {1,-30} {2,-15} {3,-15} {4,-30} {5,-30}",

                        record.Int32Field,
                        record.StringField,
                        record.X,
                        record.Y,
                        record.DateTimeField,
                        Math.Round(Distance(record, item), 0) + " meters");
                }
            }
        }

        // the Registration numbers are of Varinyng length, however they are null terminated, so we look for the terminator.
        private static string ReadNullTerminatedString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
                bytes.Add(b);
            return Encoding.Default.GetString(bytes.ToArray());
        }

        // use the Haversine formula as the Earth is Round, not Flat like on a paper map
        // we are calcualting the earth as a sphere, not an ellipsoid, so the results will be slightly off, but for this purpose it is fine
        private static double Distance(Record record, dynamic fixedCoordinate)
        {
            var R = 6371e3; // Radius of the earth in metres
            var lat1 = DegreesToRadians(record.X);
            var lat2 = DegreesToRadians(fixedCoordinate.X);
            var deltaLat = DegreesToRadians(fixedCoordinate.X - record.X);
            var deltaLon = DegreesToRadians(fixedCoordinate.Y - record.Y);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // in metres
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }

    public class Record
    {
        public int Int32Field { get; set; }
        public string StringField { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public DateTime DateTimeField { get; set; }
        public UInt64 TimeStamp { get; set; }
    }

    public class CordinateModel
    {
        public int Position { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}