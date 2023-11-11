using elk_tests;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private const string templateURL = "http://localhost:2323/api/search/{0}/100";
        private const string testdataFileName = "..\\..\\..\\testdata.txt";

        static async Task Main(string[] args)
        {

            List<string> searchStrings = new List<string>();
            DocumentsModel data = null;
            
            var sw = new Stopwatch();

            if(File.Exists(testdataFileName))
            {
                searchStrings = File.ReadLines(testdataFileName).ToList();               
            }
            


                using (HttpClient client = new HttpClient ())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var searchUrl = string.Format(templateURL, searchStrings.First());

                // Add an Accept header for JSON format.
                sw.Start ();

                // List data response.
                HttpResponseMessage response = client.GetAsync(searchUrl).Result;
                
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    data = await response.Content.ReadFromJsonAsync<DocumentsModel>().ConfigureAwait(false);
                    Console.WriteLine($"Request time = {sw.ElapsedMilliseconds} ms");
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
            }

            if (data != null && data.Documents.Any())
                Console.WriteLine($"Results count:{data.Documents.Count()}");
            else Console.WriteLine("No results");

            Console.WriteLine("Press any key for continue");
            Console.ReadLine();

        }
    }
}
