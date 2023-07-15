using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            LoadRedirectionRules(redirectionMatrixPath);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint hostEndPoint = new IPEndPoint(IPAddress.Any, portNumber);
            serverSocket.Bind(hostEndPoint);
        }

        public void StartServer()
        {
            serverSocket.Listen(100);
            while (true)
            {
                Socket clientSocket = this.serverSocket.Accept();
                Console.WriteLine("New client accepted: {0}", clientSocket.RemoteEndPoint);
                Thread newthread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newthread.Start(clientSocket);
            }
        }

        public void HandleConnection(object obj)
        {
            Socket clientSocket = (Socket)obj;
            clientSocket.ReceiveTimeout = 0;
            while (true)
            {
                try
                {
                    byte[] dataReceived = new byte[1024 * 1024];
                    int len = clientSocket.Receive(dataReceived);
                    if (len == 0)
                        break;

                    string received = Encoding.ASCII.GetString(dataReceived, 0, len);
                    Request req = new Request(received);
                    Response res = HandleRequest(req);
                    byte[] resp = Encoding.ASCII.GetBytes(res.ResponseString);
                    clientSocket.Send(resp);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
            clientSocket.Close();
        }

        Response HandleRequest(Request request)
        {
            string content;
            Response res;
            try
            {
                bool isValidRequest = request.ParseRequest();
                StatusCode code;
                string path;
                bool redirectionExists = Configuration.RedirectionRules.ContainsKey(request.relativeURI);

                if (redirectionExists)
                {
                    code = StatusCode.Redirect;
                    string newURI = Configuration.RedirectionRules[request.relativeURI];
                    path = Configuration.RootPath + '/' + newURI;
                    content = File.ReadAllText(path);
                    res = new Response(code, "html", content, newURI);
                }
                else
                {
                    path = Configuration.RootPath + request.relativeURI;
                    if (!File.Exists(path))
                    {
                        code = StatusCode.NotFound;
                        path = Configuration.RootPath + '/' + Configuration.NotFoundDefaultPageName;
                    }
                    else
                    {
                        if (isValidRequest)
                            code = StatusCode.OK;
                        else
                        {
                            code = StatusCode.BadRequest;
                            path = Configuration.RootPath + '/' + Configuration.BadRequestDefaultPageName;
                        }
                    }
                    content = File.ReadAllText(path);
                    res = new Response(code, "html", content, "");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                string path = Configuration.RootPath + '/' + Configuration.InternalErrorDefaultPageName;
                content = File.ReadAllText(path);
                res = new Response(StatusCode.InternalServerError, "html", content, "");
            }
            return res;
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            if (Configuration.RedirectionRules.ContainsKey(relativePath))
                return Configuration.RedirectionRules[relativePath];
            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            if (!File.Exists(filePath))
            {
                Logger.LogException(new FileNotFoundException("Default page not found."));
                return string.Empty;
            }
            return File.ReadAllText(filePath);
        }



        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                // TODO: using the filepath paramter read the redirection rules from file 
                // then fill Configuration.RedirectionRules dictionary 
                string[] Text;
                Text = File.ReadAllLines(filePath);
                Configuration.RedirectionRules = new Dictionary<string, string>();
                for (int i = 0; i < Text.Length; i++)
                {
                    string[] Redirections = Text[i].Split(',');
                    Configuration.RedirectionRules.Add(Redirections[0], Redirections[1]);
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
