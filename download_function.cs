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
       static public Dictionary<string, (string, string)> GET_PDFs2(Dictionary<string, (string, string)> links, string base_address)
        {
            string error = "";
            using (WebClient client = new())
            {
                List<Exception> errors = new List<Exception>();
                foreach (KeyValuePair<string, (string, string)> link in links)
                {



                   string filename = link.Value.Item1 != "" ? string.Format("\\{0}. {1}", link.Key, link.Value.Item1.Split("/")[2].Split("/").First()): string.Format("\\{0}. {1}", link.Key, link.Value.Item2.Split("/")[2].Split("/").First());
                    try
                    {
                        if (filename.EndsWith(".com") || filename.EndsWith(".org") || filename.EndsWith(".in") || filename.EndsWith(".it")|| filename.EndsWith(".gov") || filename.EndsWith(".au")) filename=filename.Substring(0,filename.Length-4) + ".pdf";
                            client.OpenRead(link.Value.Item1);
                        string download_filename = new ContentDisposition(client.ResponseHeaders["content-disposition"]).FileName;

                        client.DownloadFile(link.Value.Item1, base_address.TrimEnd('\\') + filename + System.IO.Path.ChangeExtension(download_filename, ".pdf"));
                        got_downloaded.Add(link.Key, (link.Value.Item1,""));
                        links.Remove(link.Key);
                    }

                    catch (Exception ex)
                    {
                        try
                        {
                            if (filename.EndsWith(".com") || filename.EndsWith(".org") || filename.EndsWith(".in")|| filename.EndsWith(".it") || filename.EndsWith(".gov") || filename.EndsWith(".au")) filename = filename.Substring(0, filename.Length - 4) + ".pdf";
                            client.OpenRead(link.Value.Item2);
                            string download_filename = new ContentDisposition(client.ResponseHeaders["content-disposition"]).FileName;
                            client.DownloadFile(link.Value.Item2, base_address.TrimEnd('\\')+ filename+ System.IO.Path.ChangeExtension(download_filename,".pdf"));
                            got_downloaded.Add(link.Key, ("",link.Value.Item2));
                        }

                        catch (Exception ex2)
                        {
                            error += string.Format("ID:{0}\n had this error on link 1: {1}\n and this error error on link2: {2}\n\n", link.Key, ex.Message, ex2);
                        }
                    }
                }
            }
            if (error != "") 
                {
                if (MessageBoxResult.Retry == MessageBox.Show("some files failed to download\n\n"+error, "PDF_download_error", MessageBoxButton.RetryCancel))
                    return GET_PDFs2(links,base_address);
                }
            else MessageBox.Show("all PDFs was downloaded succesfully", "success", MessageBoxButton.OK);
            return got_downloaded;
        }

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
                        return; }

                    if (filename.EndsWith(".com") || filename.EndsWith(".org") || filename.EndsWith(".in") || filename.EndsWith(".no") || filename.EndsWith(".it") || filename.EndsWith(".gov") || filename.EndsWith(".au") || filename.EndsWith(".se") || filename.EndsWith(".int") || filename.EndsWith(".ca") || filename.EndsWith(".gh") || filename.EndsWith(".za") || filename.EndsWith(".uk") || filename.EndsWith(".ie") || filename.EndsWith(".dk") || filename.EndsWith(".hk")) filename = filename.Substring(0, filename.Length - filename.Split(".").Last().Length) + "pdf";
                    else filename += ".pdf";

                    string download_filename = "";
                    if (web_check.Content.Headers.ContentDisposition is not null)
                    { download_filename = web_check.Content.Headers.ContentDisposition.FileName; }
                    if (web_check.StatusCode == HttpStatusCode.OK)
                    {
                        var web_stream = await client.GetStreamAsync(uri);
                        Stream f_stream = new FileStream(base_address.TrimEnd('\\') + filename + System.IO.Path.ChangeExtension(download_filename, ".pdf"), FileMode.CreateNew);
                        await web_stream.CopyToAsync(f_stream);
                    }
                }
                got_downloaded.Add(link.Key,link.Value);
            }
        }
    }
}
