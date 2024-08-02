using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace CnCGenerals2EMU
{
    public static class Webserver
    {
        public static readonly object _sync = new object();
        public static bool _exit;
        public static RichTextBox box = null;
        public static TcpListener lHttp = null;

        public static void Start()
        {
            SetExit(false);
            Log("Starting Webserver...");
            Logger.Log("Starting Webserver...", System.Drawing.Color.DarkMagenta);
            new Thread(new ParameterizedThreadStart(tHTTPMain)).Start();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }

        public static void Stop()
        {
            Log("Webserver stopping...");
            Logger.Log("Webserver stopping...", System.Drawing.Color.DarkMagenta);
            if (lHttp != null) lHttp.Stop();
            SetExit(true);
            Log("Done.");
            Logger.Log("Done.", System.Drawing.Color.DarkMagenta);
        }

        public static void tHTTPMain(object obj)
        {
            try
            {
                Log("[WEBS] starting...");
                Logger.Log("[WEBS] starting...", System.Drawing.Color.DarkMagenta);
                lHttp = new TcpListener(IPAddress.Parse(ProviderInfo.backendIP), 80);
                Log("[WEBS] bound to  " + ProviderInfo.backendIP + ":80");
                Logger.Log("[WEBS] bound to  " + ProviderInfo.backendIP + ":80", System.Drawing.Color.DarkMagenta);
                lHttp.Start();
                Log("[WEBS] listening...");
                Logger.Log("[WEBS] listening...", System.Drawing.Color.DarkMagenta);
                while (!GetExit())
                {
                    new Thread(tHTTPClientHandler).Start(lHttp.AcceptTcpClient());
                }
            }
            catch (Exception ex)
            {
                LogError("WEBS", ex);
                Logger.LogError("WEBS", ex);
            }
        }

        public static void tHTTPClientHandler(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream ns = client.GetStream();
            byte[] data = Helper.ReadContentTCP(ns);
            try
            {
                ProcessHttp(Encoding.ASCII.GetString(data), ns);
            }
            catch { }
            client.Close();
        }

        public static void ProcessHttp(string data, Stream s)
        {
            string[] lines = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Log("[WEBS] Request: " + lines[0]);
            //Logger.Log("[WEBS] Request: " + lines[0], System.Drawing.Color.DarkMagenta);
            string cmd = lines[0].Split(' ')[0];
            string url = lines[0].Split(' ')[1].Split(':')[0];
            if (cmd == "GET")
            {
                if (url.Contains("?"))
                    url = url.Split('?')[0];
                if (url == "/")
                    ReplyWithText(s, GetTextFile("\\index.html"));
                else
                {
                    switch (Path.GetExtension(url).ToLower())
                    {
                        case ".png":
                        case ".jpg":
                        case ".gif":
                        case ".bmp":
                            ReplyWithBinary(s, GetBinaryFile(url.Replace("/", "\\")));
                            break;
                        default:
                            ReplyWithText(s, GetTextFile(url.Replace("/", "\\")));
                            break;
                    }
                }
            }
        }

        public static string GetTextFile(string path)
        {
            if (File.Exists("html" + path))
                return File.ReadAllText("html" + path);
            Log("[WEBS] Error file not found: " + path);
            return "";
        }

        public static byte[] GetBinaryFile(string path)
        {
            if (File.Exists("html" + path))
                return File.ReadAllBytes("html" + path);
            Log("[WEBS] Error file not found: " + path);
            return new byte[0];
        }

        public static void ReplyWithText(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Command&Conqer EMU");
            sb.AppendLine("Content-Type: text/html; charset=UTF-8");
            sb.AppendLine("Content-Encoding: UTF-8");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: close");
            sb.AppendLine();
            sb.Append(c);
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
        }

        public static void ReplyWithBinary(Stream s, byte[] b)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Command&Conqer EMU");
            sb.AppendLine("Content-Type: application/octet-stream");
            sb.AppendLine("Content-Length: " + b.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: close");
            sb.AppendLine();
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
            s.Write(b, 0, b.Length);
        }

        public static void ReplyWithXML(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Command&Conqer EMU");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: Keep-Alive");
            sb.AppendLine();
            sb.Append(c);
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
        }

        public static void ReplyWithJSON(Stream s, byte[] c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Command&Conqer EMU");
            sb.AppendLine("Content-Type: application/json; charset=UTF-8");
            sb.AppendLine("Content-Encoding: UTF-8");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: Keep-Alive");
            sb.AppendLine();
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
            s.Write(c, 0, c.Length);
        }

        public static void SetExit(bool state)
        {
            lock (_sync)
            {
                _exit = state;
            }
        }

        public static bool GetExit()
        {
            bool result;
            lock (_sync)
            {
                result = _exit;
            }
            return result;
        }

        public static void Log(string s, object color = null)
        {
            if (box == null) return;
            try
            {
                box.Invoke(new Action(delegate
                {
                    string stamp = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " : ";
                    Color c;
                    if (color != null)
                        c = (Color)color;
                    else
                        c = Color.Black;
                    box.SelectionStart = box.TextLength;
                    box.SelectionLength = 0;
                    box.SelectionColor = c;
                    box.AppendText(stamp + s + "\n");
                    BackendLog.Write(stamp + s + "\n");
                    box.SelectionColor = box.ForeColor;
                    box.ScrollToCaret();
                }));
            }
            catch { }
        }

        public static void LogError(string who, Exception e, string cName = "")
        {
            string result = "";
            if (who != "") result = "[" + who + "] " + cName + " ERROR: ";
            result += e.Message;
            if (e.InnerException != null)
                result += " - " + e.InnerException.Message;
            Log(result);
        }

    }
}
