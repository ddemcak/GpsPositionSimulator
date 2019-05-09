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

        //private const string GgaMessage1 = "$GPGGA,091317.80,5037.0968,N,00534.6232,E,9,07,1.4,73.65,M,46.60,M,05,0136*50";

        //{"quality": 1, "timestamp_us": 0, "timestamp": 1525421184, "altitude": 156.59999999999999, "precision": 0.73999999999999999, "longitude": 1424.4403500000001, "latitude": 5003.7251999999999, "satellites": 12, "speed": 0.109}


        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                Console.WriteLine("Please provide an input and an output file!");
                return;
            }
            StreamReader sr = new StreamReader(args[0]);
            

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
                    string message = NmeaSenteceToJson(messages);

                    StreamWriter sw = new StreamWriter(args[1]);
                    sw.WriteLine(message);

                    Console.WriteLine(message);

                    sw.Close();

                    messages.Clear();
                    Thread.Sleep(1000);
                }

                
            }

            sr.Close();
            

            //Console.ReadLine();

        }

        private static string NmeaSenteceToJson(List<NmeaMessage> messages)
        {
            if (messages.Count != 2) return null;

            GpggaMessage gpgga = messages[0] as GpggaMessage;
            GprmcMessage rmc = messages[1] as GprmcMessage;

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return string.Format("{{\"quality\": 1, \"timestamp_us\": 0, \"timestamp\": {0}, \"altitude\": {1}, \"precision\": {2}, \"longitude\": {3}, \"latitude\": {4}, \"satellites\": {5}, \"speed\": 0.109}}",
                unixTimestamp, gpgga.Altitude, gpgga.Hdop, gpgga.Longitude, gpgga.Latitude, gpgga.NumberOfSatellites, rmc.Speed);
        }
    }
}
