﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace eMandates.Merchant.Library.XML.Utils
{
    internal class PrefixedSignedXml : SignedXml
    {
        private string prefixNs;

        public PrefixedSignedXml(string prefix, XmlDocument document)
            : base(document)
        {
            prefixNs = prefix;
        }

        public PrefixedSignedXml(string prefix, XmlElement element)
            : base(element)
        {
            prefixNs = prefix;
        }

        public PrefixedSignedXml(string prefix)
            : base()
        {
            prefixNs = prefix;
        }

        public new void ComputeSignature()
        {
            this.ComputeSignature(this.prefixNs);
        }

        void ComputeSignature(string prefix)
        {
            this.BuildDigestedReferences();

            AsymmetricAlgorithm signingKey = this.SigningKey;
            if (signingKey == null)
            {
                throw new CryptographicException("Cryptography_Xml_LoadKeyFailed");
            }

            if (this.SignedInfo.SignatureMethod == null)
            {
                if (!(signingKey is DSA))
                {
                    if (!(signingKey is RSA))
                    {
                        throw new CryptographicException("Cryptography_Xml_CreatedKeyFailed");
                    }
                    if (this.SignedInfo.SignatureMethod == null)
                    {
                        this.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                    }
                }
                else
                {
                    this.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
                }
            }
            SignatureDescription description =
                CryptoConfig.CreateFromName(this.SignedInfo.SignatureMethod) as SignatureDescription;
            if (description == null)
            {
                throw new CryptographicException("Cryptography_Xml_SignatureDescriptionNotCreated");
            }
            HashAlgorithm hash = description.CreateDigest();
            if (hash == null)
            {
                throw new CryptographicException("Cryptography_Xml_CreateHashAlgorithmFailed");
            }
            this.GetC14NDigest(hash, prefix);
            this.m_signature.SignatureValue = description.CreateFormatter(signingKey).CreateSignature(hash);
        }

        public new XmlElement GetXml()
        {
            return this.GetXml(this.prefixNs);
        }

        XmlElement GetXml(string prefix)
        {
            XmlElement e = base.GetXml();
            SetPrefix(prefix, e);
            return e;
        }

        //Invokes private method of SignedXml - SignedXml.BuildDigestedReferences
        private void BuildDigestedReferences()
        {
            Type t = typeof(SignedXml);
            MethodInfo m = t.GetMethod("BuildDigestedReferences", BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(this, new object[] { });
        }

        private byte[] GetC14NDigest(HashAlgorithm hash, string prefix)
        {
            //string securityUrl = (this.m_containingDocument == null) ? null : this.m_containingDocument.BaseURI;
            //XmlResolver xmlResolver = new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;

            XmlElement e = this.SignedInfo.GetXml();
            document.AppendChild(document.ImportNode(e, true));
            //CanonicalXmlNodeList namespaces = (this.m_context == null) ? null : Utils.GetPropagatedAttributes(this.m_context);
            //Utils.AddNamespaces(document.DocumentElement, namespaces);

            Transform canonicalizationMethodObject = this.SignedInfo.CanonicalizationMethodObject;
            //canonicalizationMethodObject.Resolver = xmlResolver;
            //canonicalizationMethodObject.BaseURI = securityUrl;
            SetPrefix(prefix, document.DocumentElement);
            //establecemos el prefijo antes de se que calcule el hash (o de lo contrario la firma no será válida)
            canonicalizationMethodObject.LoadInput(document);

            return canonicalizationMethodObject.GetDigestedOutput(hash);
        }

        private void SetPrefix(string prefix, XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
                SetPrefix(prefix, n);
            node.Prefix = prefix;
        }

    }
}
