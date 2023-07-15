using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using static System.Net.WebRequestMethods;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        StatusCode code;
        List<string> headerLines = new List<string>();
        public Response(StatusCode code, string contentType, string content, string redirectoinPath)
        {
          
            // TODO: Add headlines (Content-Type, Content-Length,Date, [location if there is redirection])

            this.code = code;
            responseString = GetStatusLine(code);
            //headerLines.Add();
            headerLines.Add(contentType);
            headerLines.Add(content.Length.ToString());
            headerLines.Add(DateTime.Now.ToString());
            responseString += "Content-Type: " + contentType + "\r\n";
            responseString += "Content-Length: " + content.Length.ToString() + "\r\n";
            responseString += "Date: " + DateTime.Now.ToString() + "\r\n";

            // TODO: Create the request string
            if ((int)code == (int)StatusCode.Redirect)
            {
                string location = "Location: " + redirectoinPath + "\r\n";
                responseString += location;
                headerLines.Add(redirectoinPath);
            }
            responseString += content;
            Console.WriteLine(responseString);
        }

        private string GetStatusLine(StatusCode code)
        {
            // TODO: Create the response status line and return it
            string statusLine = string.Empty;
            HTTPVersion http = HTTPVersion.HTTP11; 
            if (http == HTTPVersion.HTTP11)
            {
                statusLine += "HTTP/1.1 ";
            }
            else if (http == HTTPVersion.HTTP10)
            {
                statusLine += "HTTP/1.0 ";
            }
            else if (http == HTTPVersion.HTTP09)
            {
                statusLine += "HTTP/0.9 ";
            }
            statusLine += ((int)code).ToString() + " " + code.ToString() + "\r\n";

            return statusLine;
        }
    }
}
