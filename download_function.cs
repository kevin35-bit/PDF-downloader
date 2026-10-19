using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace PDF_downloader
{

    public static class download_function
    {
       private static string error2 = "";
       private static List<Thread> threads = new();
       private static Dictionary<string, (string, string)> got_downloaded;
        static public async Task<Dictionary<string, (string, string)>> GET_PDFs(Dictionary<string, (string, string)> links, string base_address)
        {
            error2 = "";
            Thread thread = new Thread(Thread => thread_handler(links,base_address));
            thread.Start();
            thread.Join();


            File.WriteAllText(@"C:\Users\Kevin Sjøholm\Documents\opgaver\PDF downloader\test_box" + "\\errors.txt", error2);
            return got_downloaded;
        }

        private static void thread_handler(Dictionary<string, (string, string)> links, string base_address)
        {

            while (links.Count > 0)
            {
                Thread.Sleep(100);
                if (threads.Count < 5)
                {
                    KeyValuePair<string, (string, string)> link = (KeyValuePair<string, (string, string)>)links.First();
                    Uri uri = new(link.Value.Item1 == "" ? link.Value.Item2 : link.Value.Item1);
                    string filename = link.Value.Item1 != "" ? string.Format("\\{0}. {1}", link.Key, link.Value.Item1.Split("/")[2].Split("/").First()) : string.Format("\\{0}. {1}", link.Key, link.Value.Item2.Split("/")[2].Split("/").First());
                    Thread thread = new Thread(Thread => task_starter(uri, filename, base_address,link));
                    thread.Start();
                    threads.Add(thread);
                }
                foreach (Thread t in threads)
                {
                    if (!t.IsAlive) threads.Remove(t) ;
                }
                Debug.WriteLine(threads.Count);
            }
        }

        private static void task_starter(Uri uri, string filename,string base_address, KeyValuePair<string, (string, string)> link)
        {
            http_download_function(uri, filename,base_address, link);
        }
        private static async Task http_download_function(Uri uri, string filename, string base_address, KeyValuePair<string, (string, string)> link)
        {
            using (var handler = new HttpClientHandler())
            {
                using (HttpClient client = new())
                {
                    handler.UseProxy = false;
                    handler.UseDefaultCredentials = true;
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    client.DefaultRequestHeaders.Add("user-agent", "PDF getter");
                    var web_check = await client.GetAsync(uri);
                    if (web_check==null)
                    {
                        error2 += ",  was null";
                        return; }

                    if (filename.EndsWith(".com") || filename.EndsWith(".org") || filename.EndsWith(".in") || filename.EndsWith(".no") || filename.EndsWith(".it") || filename.EndsWith(".gov") || filename.EndsWith(".au") || filename.EndsWith(".se") || filename.EndsWith(".int") || filename.EndsWith(".ca") || filename.EndsWith(".gh") || filename.EndsWith(".za") || filename.EndsWith(".uk") || filename.EndsWith(".ie") || filename.EndsWith(".dk") || filename.EndsWith(".hk")) filename = filename.Substring(0, filename.Length - filename.Split(".").Last().Length) + "pdf";
                    else filename += ".pdf";

                    string download_filename = "";
                    if (web_check.Content.Headers.ContentDisposition is not null)
                    { download_filename = web_check.Content.Headers.ContentDisposition.FileName; }
                    if (web_check.StatusCode == HttpStatusCode.OK)
                    {
                        //base_address.TrimEnd('\\') + filename + System.IO.Path.ChangeExtension(download_filename, ".pdf")

                        // start progress bar here
                        var web_stream = await client.GetStreamAsync(uri);
                        byte[] buffer = new byte[1024];
                        long Totalbytes = client.MaxResponseContentBufferSize;
                        for (int i = 0; i < Totalbytes; i += buffer.Length)
                        {
                            int bytesRead = await web_stream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;
                            // update progress bar here
                            Debug.WriteLine(link.Key + ". "+filename +"\n"+ bytesRead / 1024 + "Kb / " + Totalbytes / 1024 + " Kb");
                        }
                    }
                }
                got_downloaded.Add(link.Key,link.Value);
            }
        }
    }
}
