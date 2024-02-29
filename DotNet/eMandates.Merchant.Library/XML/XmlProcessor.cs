using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using eMandates.Merchant.Library.Configuration;
using eMandates.Merchant.Library.XML.Schemas.iDx;
using eMandates.Merchant.Library.XML.Utils;

namespace eMandates.Merchant.Library.XML
{
    internal class XmlProcessor : IXmlProcessor
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        private static XmlSchemaSet SchemaSet;
        private static readonly Dictionary<Type, string> TypeSchemaMapping = new Dictionary<Type, string>
            {
                { typeof(SignatureType), "xmldsig-core-schema.xsd" },

                { typeof(DirectoryReq), "idx.merchant-acquirer.1.0.xsd" },
                { typeof(DirectoryRes), "idx.merchant-acquirer.1.0.xsd" },
                { typeof(AcquirerTrxReq), "idx.merchant-acquirer.1.0.xsd" },
                { typeof(AcquirerTrxRes), "idx.merchant-acquirer.1.0.xsd" },
                { typeof(AcquirerStatusReq), "idx.merchant-acquirer.1.0.xsd" },
                { typeof(AcquirerStatusRes), "idx.merchant-acquirer.1.0.xsd" },
                { typeof(AcquirerErrorRes), "idx.merchant-acquirer.1.0.xsd" },
                
                { typeof(Schemas.pain009.Document), "pain.009.001.04.xsd" },
                { typeof(Schemas.pain010.Document), "pain.010.001.04.xsd" },
                { typeof(Schemas.pain011.Document), "pain.011.001.04.xsd" },
                { typeof(Schemas.pain012.Document), "pain.012.001.04.xsd" },
            };

        static XmlProcessor()
        {
            SchemaSet = new XmlSchemaSet();

            var currentType = typeof(XmlProcessor);
            var typeNamespace = currentType.Namespace;// .Substring(0,currentType.Namespace.LastIndexOf("XML.", StringComparison.Ordinal));// + "Xml";


            foreach (var pair in TypeSchemaMapping)
            {
                var xmlRoot = (XmlTypeAttribute)Attribute.GetCustomAttribute(pair.Key, typeof(XmlTypeAttribute));

                if (!SchemaSet.Contains(xmlRoot.Namespace))
                {
                    using (var stream = typeof (XmlProcessor).Assembly.GetManifestResourceStream($"{typeNamespace}." + pair.Value))
                    {
                        SchemaSet.Add(xmlRoot.Namespace, XmlReader.Create(stream));
                    }

                }
            }

            SchemaSet.Compile();
        }

        public XmlProcessor(IConfiguration configuration)
        {
            this._logger = configuration.LoggerFactory.Create(configuration);
            this._configuration = configuration;
        }

        public string AddSignature(string xml)
        {
            _logger.Log("signing xml...");
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xml);

            var certificate = _configuration.SigningCertificate;

            XmlSignature.Sign(ref xmlDoc, certificate, xmlDoc.DocumentElement, "", true);

            var stringWriter = new StringWriter();
            var xmlTextWriter = XmlWriter.Create(stringWriter);

            xmlDoc.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            xml = stringWriter.GetStringBuilder().ToString();

            return xml;
        }

        public bool VerifySignature(string xml)
        {
            _logger.Log("checking iDx signature...");
            var xmldocument = new XmlDocument
            {
                PreserveWhitespace = true
            };
            xmldocument.LoadXml(xml);

            var signatures = xmldocument.GetElementsByTagName("Signature", "*");

            var signature = (XmlElement) signatures[signatures.Count - 1];
            if (!CheckIdxSignature(xmldocument, signature))
            {
                return false;
            }

            if (signatures.Count == 2)
            {
                _logger.Log("checking eMandate signature...");
                bool isValidSignature;
                EmandatesSignatureOperationsService.TryVerifyElement(xml, "Document", "urn:iso:std:iso:20022:tech:xsd:pain.012.001.04", out isValidSignature);

                return isValidSignature;
            }

            return true;
        }

        private bool CheckIdxSignature(XmlDocument document, XmlElement signature)
        {
            var signedXml = new SignedXml(document);
            signedXml.LoadXml(signature);

            X509Certificate2 incomingCertificate = null;

            foreach (object o in signedXml.KeyInfo)
            {
                dynamic clause = (KeyInfoClause)o;
                Type t = clause.GetType();
                if (t.GetProperties().Any(p => p.Name.Equals("Value")) && clause.Value.ToUpper() == _configuration.AcquirerCertificateFingerprint.ToUpper())
                {
                    incomingCertificate = _configuration.AcquirerCertificate;
                    break;
                }

                if(!string.IsNullOrEmpty(_configuration.AcquirerAlternateCertificateFingerprint))
                {
                    if (t.GetProperties().Any(p => p.Name.Equals("Value")) && clause.Value.ToUpper() == _configuration.AcquirerAlternateCertificateFingerprint.ToUpper())
                    {
                        incomingCertificate = _configuration.AcquirerAlternateCertificate;
                        break;
                    }
                }
            }

            if (incomingCertificate == null)
            {
                _logger.Log("the certificate used for signing is not the same as the one in the configuration");
                return false;
            }

            if (!signedXml.CheckSignature(incomingCertificate, true))
            {
                _logger.Log("signature is not valid");
                return false;
            }

            _logger.Log("signature is valid");
            return true;
        }

        public bool VerifySchema(string xml)
        {
            var xDoc = XDocument.Parse(xml);
            var root = xDoc.Root;

            foreach (var tuple in TypeSchemaMapping)
            {
                if (tuple.Key.Name == root.Name.LocalName)
                {
                    var xmlRoot = (XmlTypeAttribute)Attribute.GetCustomAttribute(tuple.Key, typeof(XmlTypeAttribute));
                    root.Validate(SchemaSet.GlobalElements[new XmlQualifiedName(tuple.Key.Name, xmlRoot.Namespace)], SchemaSet, null);
                }
            }

            return true;
        }
    }
}
