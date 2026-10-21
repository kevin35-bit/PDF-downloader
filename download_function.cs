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
    public class C_thread 
    {
         public Thread thread;
         public bool is_running = false;
         public long input_time;
         public int data_total_amount;
        public int data_checked;

    }

    public static class download_function
    {
       private static string error2 = "";
        private static List<C_thread> threads = new();
        private static Dictionary<string, (string, string)> got_downloaded = new();
        private static progress_bar progress_Bar;

        static public async Task<Dictionary<string, (string, string)>> GET_PDFs(Dictionary<string, (string, string)> links, string base_address)
        {
            got_downloaded = new();
            error2 = "";
            Thread thread = new Thread(Thread => thread_handler(links,base_address));
            thread.Start();
            thread.Join();


            File.WriteAllText(Environment.CurrentDirectory + "\\error_list.txt", error2);
            MessageBox.Show(got_downloaded.Count + " files got downloaded", "download session has ended");
            return got_downloaded;
        }

        private static void thread_handler(Dictionary<string, (string, string)> links, string base_address)
        {
            while (links.Count > 0 || threads.Count > 0)
            {
                Thread.Sleep(100);
                if (threads.Count < 5 && links.Count >0)
                {
                    KeyValuePair<string, (string, string)> link = (KeyValuePair<string, (string, string)>)links.First();
                    Uri uri = new(link.Value.Item1 == "" ? link.Value.Item2 : link.Value.Item1);
                    string filename = link.Value.Item1 != "" ? string.Format("\\{0}. {1}", link.Key, link.Value.Item1.Split("/")[2].Split("/").First()) : string.Format("\\{0}. {1}", link.Key, link.Value.Item2.Split("/")[2].Split("/").First());
                    C_thread c_Thread = new C_thread() { };
                    c_Thread.thread = new Thread(Thread => task_starter(uri, filename, base_address, link, c_Thread));
                    c_Thread.is_running = true;
                    c_Thread.thread.IsBackground = true;
                    threads.Add(c_Thread);
                    threads.Last().thread.Start();
                    links.Remove(links.First().Key);
                }
                foreach (C_thread thread in threads.ToList())
                {
                    if (!thread.is_running)
                    {
                        threads.Remove(thread);
                    }
                    else if ( thread.input_time != 0 && (DateTime.Now.Ticks-thread.input_time)/TimeSpan.TicksPerSecond % TimeSpan.TicksPerSecond > 15)
                    {
                        if (thread.data_total_amount == thread.data_checked)
                        {
                            threads.Remove(thread);
                        }
                        else
                        {
                            thread.input_time = DateTime.Now.Ticks;
                            thread.data_checked = thread.data_total_amount;
                        }
                    }
                    if (thread.input_time == 0) thread.input_time = DateTime.Now.Ticks;
                }
                Debug.WriteLine(threads.Count);
            }
        }

        private static void task_starter(Uri uri, string filename,string base_address, KeyValuePair<string, (string, string)> link, C_thread c_thread)
        {
            http_download_function(uri, filename,base_address, link, c_thread);
        }
        private static async Task http_download_function(Uri uri, string filename, string base_address, KeyValuePair<string, (string, string)> link, C_thread c_thread)
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
                        return; 
                    }

                    if (filename.EndsWith(".com") || filename.EndsWith(".org") || filename.EndsWith(".in") || filename.EndsWith(".no") || filename.EndsWith(".it") || filename.EndsWith(".gov") || filename.EndsWith(".au") || filename.EndsWith(".se") || filename.EndsWith(".int") || filename.EndsWith(".ca") || filename.EndsWith(".gh") || filename.EndsWith(".za") || filename.EndsWith(".uk") || filename.EndsWith(".ie") || filename.EndsWith(".dk") || filename.EndsWith(".hk")) filename = filename.Substring(0, filename.Length - filename.Split(".").Last().Length) + "pdf";
                    else filename += ".pdf";


                    if (web_check.StatusCode == HttpStatusCode.OK)
                    {
                        string download_filename = "";
                        if (web_check.Content.Headers.ContentDisposition != null)
                        { download_filename = web_check.Content.Headers.ContentDisposition.FileName; }

                        //base_address.TrimEnd('\\') + filename + System.IO.Path.ChangeExtension(download_filename, ".pdf")
                        if (web_check.StatusCode == HttpStatusCode.OK)
                        {
                            // start progress bar here
                            using (var web_stream = await client.GetStreamAsync(uri))
                            {
                                byte[] buffer = new byte[1024 * 1024 * 25];
                                long bytes_total = web_check.Content.Headers.ContentLength ?? 0;
                                int byte_sum = 0;
                                int bytes_read = 0;
                                if (bytes_total<50*1024) { c_thread.is_running = false; return; }
                                Stream stream = new FileStream(base_address.TrimEnd('\\') + filename + System.IO.Path.ChangeExtension(download_filename, ".pdf"), FileMode.Create);
                                (byte_sum, bytes_read) = await read_and_write_file(filename, link, web_stream, buffer, bytes_total, byte_sum, bytes_read, stream,c_thread);
                            }
                        }
                    }
                    else {error2 += link.Key+".: "+ web_check.StatusCode ;  c_thread.is_running = false; return; }
                }
                c_thread.is_running = false;
                got_downloaded.Add(link.Key,link.Value);
            }
        }

        private static async Task<(int byte_sum, int bytes_read)> read_and_write_file(string filename, KeyValuePair<string, (string, string)> link, Stream web_stream, byte[] buffer, long bytes_total, int byte_sum, int bytes_read, Stream stream, C_thread c_thread)
        {
            //c_thread.input_time = DateTime.Now.Ticks;
            //c_thread.thread.SetApartmentState(ApartmentState.STA);
            //progress_bar progress_Bar = await Task.Run(() => new progress_bar(filename));

            for (int i = byte_sum; i <= bytes_total; i += bytes_read)
            {
                bytes_read = await web_stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytes_read == 0) break;
                // update progress bar here
                c_thread.data_total_amount += bytes_read;
                byte_sum += bytes_read;
                stream.Write(buffer, 0, bytes_read);
                Debug.WriteLine(link.Key + ". " + filename + "\n" + byte_sum / 1024 + " Kb / " + bytes_total / 1024 + " Kb\n\n");
            //  progress_Bar.update_progress(byte_sum, (int)bytes_total);
            }
            return (byte_sum, bytes_read);
        }
    }
}
