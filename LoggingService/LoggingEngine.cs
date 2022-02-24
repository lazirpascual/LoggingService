using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;
using System.Configuration;
using System.IO;

namespace LoggingService
{
    class LoggingEngine
    {
        string logPath = "./ServiceLog.txt";
        string logFormat = ConfigurationManager.AppSettings.Get("logFormat");
        string logMessage = "";

        /*  -- Function Header
            Name	:	ProcessRequest()
            Purpose :	This method is is used to process the request that the client is sending to the server.
                        Depending on the request type, it will perform the game logic accordingly and sends back
                        a response.
            Inputs	:	string queryString - query string that is being processed (sent from the client)
            Returns	:	response being sent back to the client
        */
        public string ProcessRequest(string queryString)
        {
            NameValueCollection qsCollection = HttpUtility.ParseQueryString(queryString);  // parse query string sent from client
            string requestType = qsCollection["request"];   // parse request type of the query string           
            string timeStamp = qsCollection["timeStamp"];
            string hostname = qsCollection["hostname"];
            string local_ip = qsCollection["local_ip"];
            string response = null;

            switch (requestType)
            {
                case "LOGTEST":
                    string errorLevel = qsCollection["errorLevel"];
                    string message = qsCollection["message"];

                    if (logFormat == "STANDARD")
                    {
                        logMessage = $"{timeStamp} {local_ip} {hostname} {errorLevel}: {message}";
                    }
                    else if (logFormat == "NCSA")
                    {
                        logMessage = $"{local_ip} - {hostname} [{timeStamp}] {errorLevel}: {message} {queryString.Length}";
                    }
                    
                    LogText(logMessage, logPath);
                    response = $"{errorLevel} level successfully logged.";
                    break;
                default:
                    response = "Command unknown";
                    break;
            }

            DisplayOutput(queryString, response, requestType, local_ip);
            return response;
        }

        /*  -- Function Header Comment
            Name	      :	LogText()
            Purpose       :	This method logs events such as starting the server, stopping the server, 
                            requests, responses and any exceptions encountered, to a text file.
            Inputs	      :	string message - message that is being sent to the text file
            Returns	      :	NONE
        */
        public void LogText(string message, string logPath)
        {
            using (StreamWriter w = File.AppendText(logPath))
            {
                w.Write($"{message}\n");
            }
        }

        /*  -- Function Header
            Name	:	DisplayOutput()
            Purpose :	This method contains the logic of displaying output when a client makes a 
                        connects with a server. The request type, data being received and data being
                        sent back are all displayed on the console.
            Inputs	:	string request - string that contains data being received from client
                        string response - string that is being sent back to the client
                        string responseType - type of request currently being processed
            Returns	:	NONE
        */
        public void DisplayOutput(string request, string response, string requestType, string ipAddress)
        {
            Console.WriteLine($"A client with IP of {ipAddress} has connected with a {requestType} request.");
            Console.WriteLine("Received: {0}", request);
            Console.WriteLine("Sent: {0}\n", response);
        }
    }
}
