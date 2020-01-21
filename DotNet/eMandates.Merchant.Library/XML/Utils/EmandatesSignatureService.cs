using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Text;

namespace eMandates.Merchant.Library.XML.Utils
{
    internal static class EmandatesSignatureOperationsService
    {
        public delegate XmlElement GetOrCreateContextForSignatureAdding(XmlDocument xmlDocument, XmlElement xmlElement);

        static EmandatesSignatureOperationsService()
        {
            XmlSignature.RegisterSignatureAlghorighm();
        }

        static bool IsEligibleForEmandatesSignature(XmlDocument doc, string elementNamespace)
        {
            //according to emandates protocol only acceptance reports with non errors in them should be signed
            //(meaning only those with <Accptd>true</Accptd>)

            var elements = doc.GetElementsByTagName("Accptd", elementNamespace);
            if (elements.Count > 0)
            {
                //strange condition but it works in multiple cases, even when having a formatted tag (e.g. <Accptd>  true </Accptd>)
                var val = elements[0].InnerText.Trim(new[] { ' ', '\r', '\n', '\t' });
                if (val.Equals("true") || val.Equals("1"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to sign the specified XML text.
        /// </summary>
        /// <param name="xmlText">The XML text.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="elementName">The name of the element to be signed.</param>
        /// <param name="elementNamespace">The namespace of the element to be signed.</param>
        /// <param name="xmlTextSigned">The initial XML text to which the signature was added.</param>
        /// <param name="useFingerprint">If true the public key of the certificate will be replaced with the fingerprint in the signature.</param>
        /// <returns>True if the signing was possible, false otherwise.</returns>
        public static bool TrySignElement(string xmlText, X509Certificate2 certificate, string elementName, string elementNamespace, out string xmlTextSigned, bool useFingerprint = true)
        {
            xmlTextSigned = null;

            var xmlWholeDocument = new XmlDocument();
            xmlWholeDocument.PreserveWhitespace = true;
            xmlWholeDocument.LoadXml(xmlText);

            if (String.IsNullOrEmpty(elementName))
            {
                return false;
            }
            if (String.IsNullOrEmpty(elementNamespace))
            {
                return false;
            }

            // we are signing only an element (part of the big document)
            var elements = xmlWholeDocument.GetElementsByTagName(elementName, elementNamespace);
            if (elements.Count == 0)
            {
                return false;
            }
            var elementToSign = elements[0] as XmlElement;
            var xmlElementDocument = new XmlDocument();
            xmlElementDocument.PreserveWhitespace = true;
            xmlElementDocument.LoadXml(elementToSign.OuterXml);

            EmandatesSignatureOperationsService.GetOrCreateContextForSignatureAdding getOrCreateContextForSignatureAdding = (doc, elem) =>
                {
                    return GetOrCreateContextForEmandatesSignatureAdding(doc, elem);
                };

            //todo cip!! you can optimize this one
            XmlElement signatureContainer = getOrCreateContextForSignatureAdding(xmlElementDocument, xmlElementDocument.DocumentElement);

            XmlSignature.Sign(ref xmlElementDocument, certificate, signatureContainer, "", useFingerprint);
            //var xmlSignature = XmlSignature.Sign(ref doc, certificate, xmlContainerElement, "", null, true);

            var newnode = xmlWholeDocument.ImportNode(xmlElementDocument.DocumentElement, true);
            elementToSign.ParentNode.ReplaceChild(newnode, elementToSign);

            var stringWriter = new StringWriter();
            var xmlTextWriter = XmlWriter.Create(stringWriter);

            xmlWholeDocument.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            xmlTextSigned = stringWriter.GetStringBuilder().ToString();
            
            return true;
        }

        /// <summary>
        /// Signs the specified XML text.
        /// </summary>
        /// <param name="xmlText">The XML text.</param>
        /// <param name="certificate">The certificate.</param>       
        /// <param name="useFingerprint">If true the public key of the certificate will be replaced with the fingerprint in the signature.</param>
        /// <returns>The initial XML text to which the signature was added.</returns>
        public static string SignDocument(string xmlText, X509Certificate2 certificate, bool useFingerprint = true)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xmlText);

            XmlSignature.Sign(ref xmlDoc, certificate, xmlDoc.DocumentElement, "", useFingerprint);

            var stringWriter = new StringWriter();
            var xmlTextWriter = XmlWriter.Create(stringWriter);

            xmlDoc.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            xmlText = stringWriter.GetStringBuilder().ToString();

            return xmlText;
        }

        /// <summary>
        /// Tries to verify the specified XML text signature.
        /// </summary>
        /// <param name="xmlText">The XML text.</param>
        /// <param name="elementName">The name of the element signature to be verified.</param>
        /// <param name="elementNamespace">The namespace of the element signature to be verified.</param>
        /// <param name="isValidSignature">True if the signature is valid and placed properly, false otherwise.</param>
        /// <returns>True if the verifying was possible, false otherwise.</returns>
        public static bool TryVerifyElement(string xmlText, string elementName, string elementNamespace, out bool isValidSignature)
        {
            isValidSignature = false;

            if (String.IsNullOrEmpty(elementName))
            {
                return false;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xmlText);

            var elements = xmlDoc.GetElementsByTagName(elementName, elementNamespace);
            if (elements.Count == 0)
            {
                return false;
            }
            XmlElement elementToSign = elements[0] as XmlElement;

            var xmlElementDoc = new XmlDocument();
            xmlElementDoc.PreserveWhitespace = true;
            xmlElementDoc.LoadXml(elementToSign.OuterXml);

            //XmlElement element = elementToSign;
            //var keyInfo = XmlSignature.GetElementUnderRoot(element as XmlElement, "KeyInfo");
            //var x509Data = XmlSignature.GetElementUnderRoot(keyInfo as XmlElement, "X509Data");
            //var x509Certificate = XmlSignature.GetElementUnderRoot(x509Data as XmlElement, "X509Certificate");

            //byte[] certificate = Encoding.Unicode.GetBytes(x509Certificate.InnerText);
            X509Certificate2 cert = ExtractCertificate(xmlElementDoc);

            // the signature should have been placed inside its 'MndtAccptncRpt' element (more specific, inside a <SplmtryData><Envlp>....</Envlp></SplmtryData> container)
            isValidSignature = XmlSignature.CheckSignature(xmlElementDoc, cert,
                GetEmandatesSignatureElement(xmlElementDoc.DocumentElement));
            return true;
        }

        private static X509Certificate2 ExtractCertificate(XmlDocument doc)
        {
            var signedXml = new SignedXml(doc);
            var nodeList = doc.GetElementsByTagName("Signature", "*");

            if (nodeList.Count == 0)
                throw new ApplicationException("Cannot extract the certificate.");

            signedXml.LoadXml((XmlElement)nodeList[0]);

            X509Certificate2 certificate = null;

            foreach (dynamic keyInfo in signedXml.KeyInfo)
            {
                certificate = keyInfo.Certificates[0];
            }

            return certificate;
        }

        /// <summary>
        /// Verifies the specified XML text signature.
        /// </summary>
        /// <param name="xmlText">The XML text.</param>
        /// <param name="certificate">The certificate.</param>
        ///<returns>True if the signature is valid, false otherwise</returns>
        public static bool VerifyDocument(string xmlText, X509Certificate2 certificate)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xmlText);

            XmlElement signatureElement = XmlSignature.GetElementUnderRoot(xmlDoc.DocumentElement, "Signature", "http://www.w3.org/2000/09/xmldsig#") as XmlElement;

            var result = XmlSignature.CheckSignature(xmlDoc, certificate, signatureElement);
            return result;
        }

        private static XmlElement GetOrCreateContextForEmandatesSignatureAdding(XmlDocument xmlDocument, XmlElement xmlElement)
        {
            XmlNode mndtAccptncRptNode = XmlSignature.GetElementUnderRoot(xmlElement, "MndtAccptncRpt");
            if (mndtAccptncRptNode == null)
                return null;

            // the signature should be placed inside the signed element (more specific, inside a <SplmtryData><Envlp>....</Envlp></SplmtryData> container)
            string elemPrefix = !String.IsNullOrEmpty(xmlDocument.Prefix) ? xmlDocument.Prefix + ":" : "";

            var splmtryDataElements = xmlDocument.GetElementsByTagName("SplmtryData");
            var envlpDataElements = xmlDocument.GetElementsByTagName("Envlp");

            XmlElement splmtryDataContainer;
            if (splmtryDataElements.Count == 0)
            {
                splmtryDataContainer = xmlDocument.CreateElement(elemPrefix + "SplmtryData", xmlElement.NamespaceURI);
                // append the 'SplmtryData' element at the end of the signed element                  
                mndtAccptncRptNode.AppendChild(splmtryDataContainer);
            }
            else
            {
                splmtryDataContainer = splmtryDataElements[0] as XmlElement;
            }

            XmlElement envlpContainer;
            if (envlpDataElements.Count == 0)
            {
                envlpContainer = xmlDocument.CreateElement(elemPrefix + "Envlp", xmlElement.NamespaceURI);
                // append the 'Envlp' into the 'SplmtryData' element
                splmtryDataContainer.AppendChild(envlpContainer);
            }
            else
            {
                envlpContainer = envlpDataElements[0] as XmlElement;
            }

            return envlpContainer;
        }

        private static XmlElement GetEmandatesSignatureElement(XmlElement xmlElement)
        {
            XmlNode mndtAccptncRptElement = XmlSignature.GetElementUnderRoot(xmlElement, "MndtAccptncRpt");

            if (mndtAccptncRptElement != null)
            {
                XmlNode splmtryDataElement = XmlSignature.GetElementUnderRoot(mndtAccptncRptElement as XmlElement, "SplmtryData");
                if (splmtryDataElement != null)
                {
                    XmlNode envlpElement = XmlSignature.GetElementUnderRoot(splmtryDataElement as XmlElement, "Envlp");
                    if (envlpElement != null)
                    {
                        var sig = XmlSignature.GetElementUnderRoot(envlpElement as XmlElement, "Signature", "http://www.w3.org/2000/09/xmldsig#") as XmlElement;
                        return sig;
                    }
                }
            }

            return null;
        }
    }
}
