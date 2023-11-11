using elk_tests;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LoadTestApp
{
    internal class Program
    {
        private const string templateURL = "http://localhost:2323/api/search/{0}/100";
        private const string testdataFileName = "..\\..\\..\\testcases\\names.txt";
        private const int clientsCount = 10;

        static async Task Main(string[] args)
        {
            DocumentsModel data = null;
            List<string> searchStrings = new List<string>();

            ConsoleUtils.WriteStepInfo(1, $"Load test preparation. {clientsCount} clients will do work");

            if (System.IO.File.Exists(testdataFileName))
            {
                searchStrings = System.IO.File.ReadLines(testdataFileName).ToList();
            }

            ConsoleUtils.WriteStepInfo(2, $"Test strings have loaded. There are {searchStrings.Count} strings");

            var splittedTestData = searchStrings.ChunkBy(searchStrings.Count / clientsCount).ToList();

            List<Task> tasks = new List<Task>(splittedTestData.Count);

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
                            var searchUrl = string.Format(templateURL, searchString);

                            sw.Start();
                            HttpResponseMessage response = await client.GetAsync(searchUrl).ConfigureAwait(false);
                            sw.Stop();

                            if (response.IsSuccessStatusCode)
                            {
                                data = await response.Content.ReadFromJsonAsync<DocumentsModel>().ConfigureAwait(false);

                                int dataCount = (data != null && data.Documents.Any()) ? data.Documents.Count() : 0;

                                Console.WriteLine(
                                    $"Client #{clientId}. Request ({dataCount} docs) time of [{searchString}] = [{sw.ElapsedMilliseconds} ms]");
                            }
                            else
                            {
                                Console.WriteLine($"Client #{clientId}. Error {0} ({1}). Request [{searchString}] ",
                                                  (int)response.StatusCode, response.ReasonPhrase);
                            }

                            sw.Reset();
                        }

                        // await Task.Delay(100);
                    };
                }));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("Press any key for continue");
            Console.ReadLine();
        }
    }
}
