/*
FILE                : LoggingEngine.cs
PROJECT			    : A3 - Network Application Development
PROGRAMMER		    : Lazir Pascual, Rohullah Noory
FIRST VERSION       : 2/15/2022
DESCRIPTION		    : This file contains the source code needed for the functionalities of the logging service, This includes
                      processing the requests received from the client, protection against spam requests and writing log messags to a plain text file.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace LoggingService
{
    class LoggingEngine
    {
        private static Dictionary<string, List<DateTime>> IP_Tracker = new Dictionary<string, List<DateTime>>();
        string logPath = ConfigurationManager.AppSettings.Get("logPath");
        string logFormat = ConfigurationManager.AppSettings.Get("logFormat");
        string timeoutDuration = ConfigurationManager.AppSettings.Get("timeoutDuration");
        string logMessage = "";
        string blockedIP = "";
        Stopwatch stopWatch = new Stopwatch();

        /*  -- Function Header
            Name	:	ProcessRequest()
            Purpose :	This method is is used to process the request that the client is sending to the server.
                        Depending on the request type, it will perform the logging logic accordingly and sends back
                        a response.
            Inputs	:	string queryString - query string that is being processed (sent from the client)
            Returns	:	response being sent back to the client
        */
        public string ProcessRequest(string queryString)
        {
            NameValueCollection qsCollection = HttpUtility.ParseQueryString(queryString);  // parse query string sent from client
            string requestType = qsCollection["request"];   // parse request type of the query string
            string local_ip = qsCollection["local_ip"];
            string response = null;


            switch (requestType)
            {
                case "LOGGING":
                    if (stopWatch.ElapsedMilliseconds > Int32.Parse(timeoutDuration) * 1000)
                    {
                        blockedIP = "";
                        stopWatch.Reset();
                    }

                    string msgResponse = checkLogAbuse(local_ip);
                    if (msgResponse != null)
                    {
                        blockedIP = local_ip;
                        stopWatch.Start();
                        return msgResponse;
                    }                 

                    if (blockedIP == local_ip && stopWatch.ElapsedMilliseconds < Int32.Parse(timeoutDuration) * 1000)
                    {
                        return $"Your IP has been blocked. Please try again in {timeoutDuration} seconds!";
                    }
  
                    DateTime localDate = DateTime.Now;
                    string hostname = qsCollection["hostname"];                  
                    string errorLevel = qsCollection["errorLevel"];
                    string message = qsCollection["message"];

                    if (logFormat.ToLower() == "standard")
                    {
                        logMessage = $"{localDate} {local_ip} {hostname} {errorLevel}: {message}";
                    }
                    else if (logFormat.ToLower() == "ncsa")
                    {
                        logMessage = $"{local_ip} - {hostname} [{localDate}] {errorLevel}: {message} {queryString.Length}";
                    }
                    else if (logFormat.ToLower() == "w3c")
                    {
                        string destination_ip = ConfigurationManager.AppSettings.Get("ipAddress");
                        string port = ConfigurationManager.AppSettings.Get("port");
                        logMessage = $"{localDate} LOG TCP {local_ip} {destination_ip} {port} {queryString.Length} {errorLevel}: {message}";
                    }

                    LogText(logMessage, logPath);
                    response = $"{errorLevel} level successfully logged.";
                    break;
                default:
                    response = "Command unknown";
                    break;
            }

            DisplayOutput(qsCollection, response, requestType, local_ip);
            return response;
        }


        /*  -- Function Header
            Name	:	checkLogAbuse()
            Purpose :	This method is used to ensure a client is not spam sending requests.
            Inputs	:	string currentIP - IP address of the client as a string
            Returns	:	a message if too many requests have been received within a short amount of time or null if otherwise
        */
        public string checkLogAbuse(string currentIP)
        {         
            // if the ip doesn't exist in the dictionary, add it and log the time of the request
            if (!(IP_Tracker.ContainsKey(currentIP)))
            {
                List<DateTime> currentIPCount = new List<DateTime>();
                currentIPCount.Add(DateTime.Now);
                IP_Tracker.Add(currentIP, currentIPCount);
            }
            // if the ip already exists, simply log the time of the request and check if more than
            // 5 requests have been received from the same IP within one second
            else
            {
                var currentIPValue = IP_Tracker[currentIP];
                currentIPValue.Add(DateTime.Now);

                if (currentIPValue.Count % 5 == 0)
                {
                    var firstDateVal = currentIPValue.ElementAt(currentIPValue.Count - 1);
                    var secondDateVal = currentIPValue.ElementAt(currentIPValue.Count - 5);
                    var dateDifference = (firstDateVal - secondDateVal).TotalSeconds;

                    if (dateDifference < 1)
                    {
                        return "You have sent too many requests.";
                    }
                }
            }

            return null;
        }

        /*  -- Function Header Comment
            Name	      :	LogText()
            Purpose       :	This method is used to log messages in a plain text file
            Inputs	      :	string message - message that is being sent to the text file
                            string logPath - path of the log file
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
            Inputs	:	NameValueCollection qsCollection - parsed request sent from client
                        string response - string that is being sent back to the client
                        string responseType - type of request currently being processed
            Returns	:	NONE
        */
        public void DisplayOutput(NameValueCollection qsCollection, string response, string requestType, string ipAddress)
        {
            Console.WriteLine($"A client with IP of {ipAddress} has connected with a {requestType} request.");
            Console.WriteLine("Received: Hostname: {0}, LogLevel: {1}, Details: {2}", qsCollection["hostname"], qsCollection["errorLevel"], qsCollection["message"]);
            Console.WriteLine("Sent: {0}\n", response);
        }
    }
}
