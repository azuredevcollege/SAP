using System;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SamlWebApp.Samlp
{
    public class AuthResponse
    {
        private XmlDocument _xmlDoc;
        private XmlNamespaceManager _xmlNameSpaceManager;
        public void LoadXml(string xml)
		{
			_xmlDoc = new XmlDocument();
			_xmlDoc.PreserveWhitespace = true;
			_xmlDoc.XmlResolver = null;
			_xmlDoc.LoadXml(xml);

			_xmlNameSpaceManager = GetNamespaceManager(); //lets construct a "manager" for XPath queries
		}

		public void LoadXmlFromBase64(string response)
		{
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			LoadXml(enc.GetString(Convert.FromBase64String(response)));
		}

        private XmlNamespaceManager GetNamespaceManager()
		{
			XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
			manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
			manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
			manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

			return manager;
		}

        public string GetAssertion()
        {
            XmlNode node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion[1]", _xmlNameSpaceManager);
            return node.OuterXml;
        }
    }
}