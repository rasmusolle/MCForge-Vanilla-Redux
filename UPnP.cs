/*
Copyright 2012 MCForge
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
http://www.opensource.org/licenses/ecl2.php
http://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
//This upnp class comes from http://www.codeproject.com/Articles/27992/NAT-Traversal-with-UPnP-in-C, Modified for use with MCForge

namespace MCForge.Core {

    public class UPnP {

        public static bool CanUseUpnp { get {return Discover(); } }

        private const string req = "M-SEARCH * HTTP/1.1\r\n" +
                                                            "HOST: 239.255.255.250:1900\r\n" +
                                                            "ST:upnp:rootdevice\r\n" +
                                                            "MAN:\"ssdp:discover\"\r\n" +
                                                            "MX:3\r\n\r\n";

        static TimeSpan _timeout = new TimeSpan(0, 0, 0, 3);
        public static TimeSpan TimeOut {
            get { return _timeout; }
            set { _timeout = value; }
        }
        static string _serviceUrl;
        private static bool Discover() {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            byte[] data = Encoding.ASCII.GetBytes(req);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            byte[] buffer = new byte[0x1000];

            DateTime start = DateTime.Now;
            try {
                do {
                    s.SendTo(data, ipe);
                    s.SendTo(data, ipe);
                    s.SendTo(data, ipe);

                    int length = -1;
                    do {
                        SocketError error;
                        s.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, out error, new AsyncCallback((result) => {
                            length = s.Receive(buffer);
                        }), null);
                        
                        while(length == -1){
                            if ((DateTime.Now - start).TotalSeconds > 10 ) {
                                return false;
                            }
                            Thread.Sleep(1000);
                        }

                        string resp = Encoding.ASCII.GetString(buffer, 0, length);
                        if (resp.Contains("upnp:rootdevice")) {
                            resp = resp.Substring(resp.ToLower().IndexOf("location:") + 9);
                            resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
                            if (!string.IsNullOrEmpty(_serviceUrl = GetServiceUrl(resp))) {
                                return true;
                            }
                        }
                    } while (length > 0);
                } while (start.Subtract(DateTime.Now) < _timeout);
                return false;
            }
            catch {
                return false;
            }
        }

        private static string GetServiceUrl(string resp) {
#if !DEBUG
            try {
#endif
                XmlDocument desc = new XmlDocument();
                var request = WebRequest.CreateDefault(new Uri(resp));
                desc.Load(request.GetResponse().GetResponseStream());
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
                nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
                XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
                if ( !typen.Value.Contains("InternetGatewayDevice") )
                    return null;
                XmlNode node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL/text()", nsMgr);
                if ( node == null )
                    return null;
             //   XmlNode eventnode = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:eventSubURL/text()", nsMgr);
                return CombineUrls(resp, node.Value);
#if !DEBUG
            }
            catch { return null; }
#endif
        }

        private static string CombineUrls(string resp, string p) {
            int n = resp.IndexOf("://");
            n = resp.IndexOf('/', n + 3);
            return resp.Substring(0, n) + p;
        }

        public static void ForwardPort(int port, ProtocolType protocol, string description) {
            if ( string.IsNullOrEmpty(_serviceUrl) )
                throw new Exception("No UPnP service available or Discover() has not been called");
         /*   XmlDocument xdoc = SOAPRequest(_serviceUrl, "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + port.ToString() + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
                "<NewInternalPort>" + port.ToString() + "</NewInternalPort><NewInternalClient>" + GetLocalIP() +
                "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + description +
            "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>", "AddPortMapping");
        */}

        public static void DeleteForwardingRule(int port, ProtocolType protocol) {
            if ( string.IsNullOrEmpty(_serviceUrl) )
                throw new Exception("No UPnP service available or Discover() has not been called");
           // XmlDocument xdoc = SOAPRequest(_serviceUrl,
           // "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
           // "<NewRemoteHost>" +
          //  "</NewRemoteHost>" +
          //  "<NewExternalPort>" + port + "</NewExternalPort>" +
           // "<NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
          //  "</u:DeletePortMapping>", "DeletePortMapping");

        }

        [DebuggerStepThrough]
        public static IPAddress GetExternalIP() {
            if ( string.IsNullOrEmpty(_serviceUrl) )
                throw new Exception("No UPnP service available or Discover() has not been called");
            XmlDocument xdoc = SOAPRequest(_serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
            "</u:GetExternalIPAddress>", "GetExternalIPAddress");
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
            string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
            return IPAddress.Parse(IP);
        }

        [DebuggerStepThrough]
        public static string GetLocalIP() {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach ( IPAddress ip in host.AddressList ) {
                if ( ip.AddressFamily == AddressFamily.InterNetwork ) {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        private static XmlDocument SOAPRequest(string url, string soap, string function) {
            string req = "<?xml version=\"1.0\"?>" +
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            "<s:Body>" +
            soap +
            "</s:Body>" +
            "</s:Envelope>";
            WebRequest r = HttpWebRequest.Create(url);
            r.Method = "POST";
            byte[] b = Encoding.UTF8.GetBytes(req);
            r.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"");
            r.ContentType = "text/xml; charset=\"utf-8\"";
            r.ContentLength = b.Length;
            r.GetRequestStream().Write(b, 0, b.Length);
            XmlDocument resp = new XmlDocument();
            WebResponse wres = r.GetResponse();
            Stream ress = wres.GetResponseStream();
            resp.Load(ress);
            return resp;
        }
    }
}
