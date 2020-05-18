using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using System.Xml;

namespace SamlWebApp.Samlp
{
    public class AuthRequest
    {
        private string _id;
        private string _issueInstant;
        private string _issuer;
        private string _assertionConsumerUrl;

        public AuthRequest(string issuer, string assertionConsumerUrl)
        {
            _issuer = issuer;
            _assertionConsumerUrl = assertionConsumerUrl;

            _id = "_" + System.Guid.NewGuid().ToString();
			_issueInstant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
        }

        public string GetRequest()
		{
			using (StringWriter sw = new StringWriter())
			{
				XmlWriterSettings xws = new XmlWriterSettings();
				xws.OmitXmlDeclaration = true;

				using (XmlWriter xw = XmlWriter.Create(sw, xws))
				{
					xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
					xw.WriteAttributeString("ID", _id);
					xw.WriteAttributeString("Version", "2.0");
					xw.WriteAttributeString("IssueInstant", _issueInstant);
					xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
					xw.WriteAttributeString("AssertionConsumerServiceURL", _assertionConsumerUrl);

					xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
					xw.WriteString(_issuer);
					xw.WriteEndElement();

					xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
					xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
					xw.WriteAttributeString("AllowCreate", "true");
					xw.WriteEndElement();
					xw.WriteEndElement();
				}

                var memoryStream = new MemoryStream();
                var writer = new StreamWriter(new DeflateStream(memoryStream, CompressionMode.Compress, true), new UTF8Encoding(false));
                writer.Write(sw.ToString());
                writer.Close();
                string result = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, Base64FormattingOptions.None);
                return result;
			}
		}

		//returns the URL you should redirect your users to (i.e. your SAML-provider login URL with the Base64-ed request in the querystring
		public string GetRedirectUrl(string samlEndpoint, string relayState = null)
		{
			var queryStringSeparator = samlEndpoint.Contains("?") ? "&" : "?";

			var url = samlEndpoint + queryStringSeparator + "SAMLRequest=" + HttpUtility.UrlEncode(this.GetRequest());

			if (!string.IsNullOrEmpty(relayState)) 
			{
				url += "&RelayState=" + HttpUtility.UrlEncode(relayState);
			}

			return url;
		}
    }
}