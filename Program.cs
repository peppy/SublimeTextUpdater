using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace SublimeTextUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient wc = new WebClient();

            Console.WriteLine("Attempting to find latest build...");
            string updatePage = wc.DownloadString("http://www.sublimetext.com/nightly");

            Match m = Regex.Match(updatePage, @"<a href=""(http://[^""]*/Sublime Text 2 Build [0-9]* x64\.zip)"">");

            if (m.Groups.Count < 2)
            {
                Console.WriteLine("Error parsing download page");
                Environment.Exit(0);
            }

            string downloadUrl = m.Groups[1].ToString();
            string filename = Path.GetFileName(downloadUrl);

            File.Delete(filename);

            Console.WriteLine("Downloading " + filename + "...");

            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileAsync(new Uri(downloadUrl), filename, null);

            while (wc.IsBusy)
                Thread.Sleep(10);

            Console.WriteLine();
            Console.WriteLine("Extracting new files...");

            FastZip zip = new FastZip();
            zip.ExtractZip(filename, ".", FastZip.Overwrite.Always, null, ".*", ".*", true);

            File.Delete(filename);
            Console.WriteLine("Update completed successfully!");
        }

        static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("\r" + (" " + e.ProgressPercentage + "%").PadLeft((int)(Console.WindowWidth * e.ProgressPercentage / 100f), '.'));
        }
    }
}
