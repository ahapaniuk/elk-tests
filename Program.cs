using elk_tests;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace LoadTestApp
{
    internal class Program
    {
        private const string templateURL = "http://localhost:2323/api/search/{0}/100";
        private const string testdataFileName = "..\\..\\..\\testcases\\names.txt";
        private const int clientsCount = 15;

        static async Task Main(string[] args)
        {
            int requests = 0;
            var lckRequests = new object();

            DocumentsModel data = null;
            List<string> searchStrings = new List<string>();

            ConsoleUtils.WriteStepInfo(1, $"Load test preparation. {clientsCount} clients will do work");

            if (System.IO.File.Exists(testdataFileName))
            {
                searchStrings = System.IO.File.ReadLines(testdataFileName).ToList();
            }

            ConsoleUtils.WriteStepInfo(2, $"Test strings have loaded. There are {searchStrings.Count} strings");

            var splittedTestData = searchStrings.ChunkBy(searchStrings.Count / clientsCount).ToList();

            List<Task> tasks = new List<Task>(splittedTestData.Count+1);


            int id = 0;
            foreach (var testdata in splittedTestData)
            {
                var clientTestData = testdata;
                int clientId = ++id;

                tasks.Add(Task.Run(async () => {
                    var sw = new Stopwatch();

                    foreach (var searchString in clientTestData)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                           client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var pattern = new Regex(@"[:!@#$%^&*()}{|\:?><\[\]\\;'/.,~_-]");                          
                            var searchUrl = string.Format(templateURL, pattern.Replace(searchString, " "));

                            sw.Start();
                            HttpResponseMessage response = client.GetAsync(searchUrl).ConfigureAwait(false).GetAwaiter().GetResult();
                            lock (lckRequests)
                            {
                                requests ++;
                            }
                            sw.Stop();

                            if (response.IsSuccessStatusCode)
                            {
                                data =  response.Content.ReadFromJsonAsync<DocumentsModel>().GetAwaiter().GetResult();

                                int dataCount = (data != null && data.Documents.Any()) ? data.Documents.Count() : 0;

                                Console.WriteLine(
                                    $"Client #{clientId}. Request ({dataCount} docs) time of [{searchString}] = [{sw.ElapsedMilliseconds} ms]");
                            }
                            else
                            {
                                Console.WriteLine($"Client #{clientId}. Error {(int)response.StatusCode} ({response.ReasonPhrase}) SEARCH={searchString}");
                            }

                            sw.Reset();
                        }

                        //await Task.Delay(10);
                    };
                }));
            }

            await Task.Delay(15000);
            
            tasks.Add(Task.Run(async () => {
                int oldRequests = 0;
                while (true)
                {
                    ConsoleUtils.WriteStepInfo(3, $"Requests per second = {requests - oldRequests}");
                    //Console.WriteLine($"Requests per second = {requests - oldRequests}");
                    oldRequests = requests;
                    await Task.Delay(1000).ConfigureAwait(false);
                }

            }));

            await Task.WhenAll(tasks);

            Console.WriteLine("Press any key for continue");
            Console.ReadLine();
        }
    }
}
