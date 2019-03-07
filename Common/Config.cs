﻿//using Android.Content;
using System;
using System.IO;

namespace Common
{
    public static class Config
    {
        //public static Context Context = null;
        public static Stream ConfigStream = null;

        //ActiveMQ&ApolloMQ Setting
        public static string MQUserID;
        public static string MQPwd;
        public static string MQ_service;
        public static string MQ_network;
        public static bool MQ_useSSL = false;
        public static string MQReceivedMessageReservedSeconds = "30";

        //EMS Setting
        public static string EMSUserID;
        public static string EMSPwd;
        public static string EMS_service;
        public static string EMS_network;
        public static bool EMS_useSSL = false;
        public static string EMSReceivedMessageReservedSeconds = "30";

        //DBWebService
        public static string dbWebService;
        public static int webServiceTimeOut = 60;

        //LogFolder
        public static string logDir = "";
        public static int preservedDaysForLog = 10;

        //PreservedRowsOfMessges
        public static int preservedRowsForMessge = 200;

        ////Y77
        public static string jefferiesExcuReport_Sender_Topic;
        public static string jefferiesExcuReport_Listener_Topic;
        public static string jefferiesExcuReportMaxThreads = "0";
        public static bool isUsingThreadPoolThreadForY77 = true;

        //Folder App PackageName
        public static string folderApp1;
        public static string folderApp2;

        //MoneySQWebSite
        public static string moneysqWebSite;

        public static void ReadParameter()
        {
            if (ConfigStream == null)
            {
                Common.LogHelper.MoneySQLogger.LogInfo("Config.cs: not yet be assigned config");
                return;
            }

            using (StreamReader sr = new StreamReader(ConfigStream))
            {
                try
                {
                    while (sr.Peek() > 0)
                    {
                        string line = sr.ReadLine().Trim();

                        if (line == "")
                            continue;

                        if (line[0] == '#')
                            continue;

                        int seperator = line.IndexOf("=");

                        if (seperator <= 0)
                            return;

                        string config_name = line.Substring(0, seperator);
                        string config_value = line.Substring(seperator + 1, line.Length - seperator - 1);
                        //string config_value = line.Substring(seperator + 1, line.Length - seperator - 1)
                        //    .Replace("#", string.IsNullOrEmpty(Util.GetMacAddress()) ? "#" : Util.GetMacAddress());

                        config_name = config_name.ToUpper();

                        switch (config_name)
                        {
                            //ActiveMQ&ApolloMQ Setting
                            case "MQUSERID":
                                MQUserID = config_value;
                                break;
                            case "MQPWD":
                                MQPwd = config_value;
                                break;
                            case "MQ_SERVICE":
                                MQ_service = config_value;
                                break;
                            case "MQ_NETWORK":
                                MQ_network = config_value;
                                break;
                            case "MQ_USESSL":
                                MQ_useSSL = Convert.ToBoolean(config_value);
                                break;
                            case "MQRECEIVEDMESSAGERESERVEDSECONDS":
                            {
                                int TestValue;
                                MQReceivedMessageReservedSeconds = int.TryParse(config_value, out TestValue)
                                    ? TestValue.ToString()
                                    : MQReceivedMessageReservedSeconds;
                                break;
                            }

                            //EMS Setting
                            case "EMSUSERID":
                                EMSUserID = config_value;
                                break;
                            case "EMSPWD":
                                EMSPwd = config_value;
                                break;
                            case "EMS_SERVICE":
                                EMS_service = config_value;
                                break;
                            case "EMS_NETWORK":
                                EMS_network = config_value;
                                break;
                            case "EMS_USESSL":
                                EMS_useSSL = Convert.ToBoolean(config_value);
                                break;
                            case "EMSRECEIVEDMESSAGERESERVEDSECONDS":
                                {
                                    int TestValue;
                                    EMSReceivedMessageReservedSeconds = int.TryParse(config_value, out TestValue)
                                        ? TestValue.ToString()
                                        : EMSReceivedMessageReservedSeconds;
                                    break;
                                }

                            //DBWebService
                            case "DBWEBSERVICE":
                                dbWebService = config_value;
                                break;
                            case "WEBSERVICETIMEOUT":
                                {
                                    int ConfigValue;
                                    webServiceTimeOut = int.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue
                                        : webServiceTimeOut;
                                    break;
                                }

                            //LogFolder
                            case "LOGDIR":
                                logDir = config_value;
                                break;
                            case "PRESERVEDDAYSFORLOG":
                            {
                                int ConfigValue;
                                preservedDaysForLog = int.TryParse(config_value, out ConfigValue)
                                    ? ConfigValue
                                    : preservedDaysForLog;
                                break;
                            }

                            //PreservedRowsOfMessges
                            case "PRESERVEDROWSFORMESSAGE":
                                {
                                    int ConfigValue;
                                    preservedRowsForMessge = int.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue
                                        : preservedRowsForMessge;
                                    break;
                                }

                            //Y77
                            case "JEFFERIESEXCUREPORT_SENDER_TOPIC":
                                jefferiesExcuReport_Sender_Topic = config_value;
                                break;
                            case "JEFFERIESEXCUREPORT_LISTENER_TOPIC":
                                jefferiesExcuReport_Listener_Topic = config_value;
                                break;
                            case "JEFFERIESEXCUREPORTMAXTHREADS":
                                {
                                    int ConfigValue;
                                    jefferiesExcuReportMaxThreads = int.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue.ToString()
                                        : jefferiesExcuReportMaxThreads;
                                    break;
                                }
                            case "ISUSINGTHREADPOOLTHREADFORY77":
                                {
                                    bool ConfigValue;
                                    isUsingThreadPoolThreadForY77 = bool.TryParse(config_value, out ConfigValue)
                                        ? ConfigValue
                                        : isUsingThreadPoolThreadForY77;
                                    break;
                                }
                            //Folder App PackageName
                            case "FOLDERAPP1":
                                folderApp1 = config_value;
                                break;
                            case "FOLDERAPP2":
                                folderApp2 = config_value;
                                break;           
                            //MoneySQWebSite
                            case "MONEYSQWEBSITE":
                                moneysqWebSite = config_value;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogHelper.MoneySQLogger.LogError(ex);
                }
            }
        }
    }
}
