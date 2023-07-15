using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        public string[] requestLines;
        public static int requstindex = 0;
        RequestMethod method;
        public string relativeURI;
        public Dictionary<string, string> headerLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        public HTTPVersion httpVersion;
        public string requestString;
        string[] contentLines;

        public Request(string requestString)
        {
            headerLines = new Dictionary<string, string>();
            this.requestString = requestString;
        }
        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error



        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {
            //throw new NotImplementedException();


            //TODO: parse the receivedRequest using the \r\n delimeter 
            string[] strlist = requestString.Split('\n');
            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)
            if (strlist.Length < 3)
            {
                return false;
            }
            // Parse Request line
            if (!ParseRequestLine(strlist[0]))
            {
                return false;
            }
            // Validate blank line exists
            if (!ValidateBlankLine(strlist[strlist.Length - 2]))
            {
                return false;
            }
            // Load header lines into HeaderLines dictionary
            if (!LoadHeaderLines(strlist))
            {
                return false;
            }

            return ValidateIsURI(relativeURI);
        }

        private bool ParseRequestLine(string requestLine)
        {
            string[] ReqLine = requestLine.Split(' ');

            relativeURI = ReqLine[1];

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(ReqLine[i]);
            }

            if (ReqLine[0].Equals("GET"))
            {
                method = RequestMethod.GET;
            }
            else if (ReqLine[0].Equals("POST"))
            {
                method = RequestMethod.POST;
            }
            else if (ReqLine[0].Equals("HEAD"))
            {
                method = RequestMethod.HEAD;
            }
            else
            {
                return false;
            }

            if (ReqLine[2].Equals("HTTP/1.1\r"))
            {
                httpVersion = HTTPVersion.HTTP11;
            }
            else if (ReqLine[2].Equals("HTTP/1.0\r"))
            {
                httpVersion = HTTPVersion.HTTP10;
            }
            else if (ReqLine[2].Equals("HTTP/0.9\r"))
            {
                httpVersion = HTTPVersion.HTTP09;
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines(string[] strlist)
        {
            for (int i = 1; i < strlist.Length - 2; i++)
            {
                string[] splitter = { ": " };
                string[] Headers = strlist[i].Split(splitter, StringSplitOptions.None);
                headerLines.Add(Headers[0], Headers[1]);
            }

            return true;
        }

        private bool ValidateBlankLine(string blankLine)
        {
            return blankLine.Equals("\r");
        }
    }
}
