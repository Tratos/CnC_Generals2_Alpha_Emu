﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlazeLibWV;

namespace CnCGenerals2EMU
{
    public static class RedirectorServer
    {
        public static readonly object _sync = new object();
        public static bool _exit;
        public static bool _isRunning = false;
        public static bool useSSL = false;
        public static RichTextBox box = null;
        public static TcpListener lRedirector = null;
        public static string redi = "redirector.pfx";

        public static string backend;
        public static int backPort;
        public static string targethost;
        public static int targetPort;

        public static void Start()
        {
            SetExit(false);
            _isRunning = true;
            Log("Starting Redirector...");
            Logger.Log("Starting Redirector...", System.Drawing.Color.BlueViolet);
            new Thread(tRedirectorMain).Start();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }

        public static void Stop()
        {
            Log("Backend stopping...");
            Logger.Log("Backend stopping...", System.Drawing.Color.BlueViolet);
            if (lRedirector != null) lRedirector.Stop();
            SetExit(true);
            Log("Done.");
        }

        public static void tRedirectorMain(object obj)
        {
            X509Certificate2 cert = null;
            try
            {
                Log("[REDI] Redirector starting...");
                Logger.Log("[REDI] Redirector starting...", System.Drawing.Color.BlueViolet);
                lRedirector = new TcpListener(IPAddress.Parse(backend), backPort);
                Log("[REDI] Redirector bound to  " + backend + ":" + backPort.ToString());
                Logger.Log("[REDI] Redirector bound to  " + backend + ":" + backPort.ToString(), System.Drawing.Color.BlueViolet);
                lRedirector.Start();
                if (useSSL)
                {
                    Log("[REDI] Loading Cert...");
                    cert = new X509Certificate2(redi, "123456");
                }
                Log("[REDI] Redirector listening...");
                Logger.Log("[REDI] Redirector listening...", System.Drawing.Color.BlueViolet);
                TcpClient client;
                while (!GetExit())
                {
                    client = lRedirector.AcceptTcpClient();
                    Log("[REDI] Client connected");
                    if (useSSL)
                    {
                        Log("[REDI] [SSL] Send Redirector Packet...");
                        SslStream sslStream = new SslStream(client.GetStream(), false);
                        sslStream.AuthenticateAsServer(cert, false, SslProtocols.Default | SslProtocols.None | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, false);
                        byte[] data = Helper.ReadContentSSL(sslStream);
                        MemoryStream m = new MemoryStream();
                        m.Write(data, 0, data.Length);
                        data = CreateRedirectorPacket();
                        m.Write(data, 0, data.Length);
                        sslStream.Write(data);
                        sslStream.Flush();
                        client.Close();
                    }
                    else
                    {
                        Log("[REDI] Send Redirector Packet...");
                        NetworkStream stream = client.GetStream();
                        byte[] data = Helper.ReadContentTCP(stream);
                        MemoryStream m = new MemoryStream();
                        m.Write(data, 0, data.Length);
                        data = CreateRedirectorPacket();
                        m.Write(data, 0, data.Length);
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        client.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("REDI", ex);
            }
        }

        public static byte[] CreateRedirectorPacket()
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            List<Blaze.Tdf> VALU = new List<Blaze.Tdf>();
            VALU.Add(Blaze.TdfString.Create("HOST", targethost));
            VALU.Add(Blaze.TdfInteger.Create("IP\0\0", Blaze.GetIPfromString(targethost)));
            VALU.Add(Blaze.TdfInteger.Create("PORT", targetPort));
            Blaze.TdfUnion ADDR = Blaze.TdfUnion.Create("ADDR", 0, Blaze.TdfStruct.Create("VALU", VALU));
            Result.Add(ADDR);
            Result.Add(Blaze.TdfInteger.Create("SECU", 0)); //Change to 1 for SSL 
            Result.Add(Blaze.TdfInteger.Create("XDNS", 0));
            return Blaze.CreatePacket(5, 1, 0, 0x1000, 0, Result);
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
