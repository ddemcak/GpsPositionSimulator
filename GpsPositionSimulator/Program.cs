using Ghostware.NMEAParser;
using Ghostware.NMEAParser.NMEAMessages;
using Ghostware.NMEAParser.NMEAMessages.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GpsPositionSimulator
{
    class Program
    {

        static FileInfo inputFile = null;
        static FileInfo outputFile = null;
        static int timeOffset = 0;
        static int speedMultiplier = 0;


        static void Main(string[] args)
        {

            if (args.Length != 4)
            {
                Console.WriteLine(@"Please provide an input arguments as follows:
1st - Input file with NMEA data,
2nd - Output JSON file path,
3rd - Time offset in seconds,
4th - Speed multipler constant.");
                return;
            }

            if (!ValidateInputArguments(args))
            {
                return;
            }


            StreamReader sr = new StreamReader(inputFile.FullName);
            List<NmeaMessage> messages = new List<NmeaMessage>();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                if (line.Contains("$GPGGA") || line.Contains("$GPRMC"))
                {
                    var parser = new NmeaParser();
                    messages.Add(parser.Parse(line));
                }

                if (messages.Count == 2)
                {
                    string message = NmeaSenteceToJson(messages, timeOffset, speedMultiplier);

                    StreamWriter sw = new StreamWriter(outputFile.FullName);
                    sw.WriteLine(message);

                    Console.WriteLine(message);

                    sw.Close();

                    messages.Clear();
                    Thread.Sleep(1000);
                }


            }
            sr.Close();

        }

        public static bool ValidateInputArguments(string[] args)
        {
            try
            {
                inputFile = new FileInfo(args[0]);
            }
            catch
            {
                Console.WriteLine(string.Format("Input argument {0} is not valid!", args[0]));
                return false;
            }

            try
            {
                outputFile = new FileInfo(args[1]);
            }
            catch
            {
                Console.WriteLine(string.Format("Input argument {0} is not valid!", args[1]));
                return false;
            }

            try
            {
                timeOffset = int.Parse(args[2]);
            }
            catch
            {
                Console.WriteLine(string.Format("Input argument {0} is not valid!", args[2]));
                return false;
            }

            try
            {
                speedMultiplier = int.Parse(args[3]);
            }
            catch
            {
                Console.WriteLine(string.Format("Input argument {0} is not valid!", args[3]));
                return false;
            }

            return true;
        }

        private static string NmeaSenteceToJson(List<NmeaMessage> messages, int offsetInSeconds, int speedMultipler)
        {
            if (messages.Count != 2) return null;

            GpggaMessage gpgga = messages[0] as GpggaMessage;
            GprmcMessage rmc = messages[1] as GprmcMessage;

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + offsetInSeconds;

            // Longitude and Latitude needs to by formatted in the same way as for the project
            // Speed is in knots 1 knt = 1.852 km/h
            return string.Format("{{\"quality\": 1, \"timestamp_us\": 0, \"timestamp\": {0}, \"altitude\": {1}, \"precision\": {2}, \"longitude\": {3}, \"latitude\": {4}, \"satellites\": {5}, \"speed\": {6}}}",
                unixTimestamp, gpgga.Altitude, gpgga.Hdop, gpgga.Longitude * 100, gpgga.Latitude * 100, gpgga.NumberOfSatellites, rmc.Speed * 1.852 * speedMultipler);
        }
    }
}
