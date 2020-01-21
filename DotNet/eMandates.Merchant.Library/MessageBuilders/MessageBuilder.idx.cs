using System;
using System.Globalization;
using System.Xml;
using eMandates.Merchant.Library.Configuration;
using eMandates.Merchant.Library.XML.Schemas.iDx;

namespace eMandates.Merchant.Library.MessageBuilders
{
    internal class IDxMessageBuilder
    {
        private IConfiguration Configuration { get; set; }
        private Instrumentation LocalInstrumentCode { get; set; }

        private static readonly string ProductId_CORE = "NL:BVN:eMandatesCore:1.0";
        private static readonly string ProductId_B2B = "NL:BVN:eMandatesB2B:1.0";

        public IDxMessageBuilder(IConfiguration configuration, Instrumentation instrumentCode)
        {
            this.Configuration = configuration;
            this.LocalInstrumentCode = instrumentCode;
        }

        private DateTime Now()
        {
            return DateTime.UtcNow;
        }

        private void VerifyExpirationPeriod(TimeSpan? expirationPeriod)
        {
            if (expirationPeriod.HasValue && expirationPeriod.Value > TimeSpan.FromDays(7))
            {
                throw new CommunicatorException("ExpirationPeriod should be less than 7 days");
            }
        }

        public string GetDirectoryRequest()
        {
            var directoryRequest = new DirectoryReq
            {
                createDateTimestamp = Now(),
                productID = this.LocalInstrumentCode == Instrumentation.Core? ProductId_CORE : ProductId_B2B,
                version = "1.0.0",

                Merchant = new DirectoryReqMerchant
                {
                    merchantID = Configuration.EMandateContractId,
                    subID = Configuration.EMandateContractSubId.ToString(CultureInfo.InvariantCulture)
                },
            };

            return ProcessDateTimes(directoryRequest.Serialize());
        }

        public string GetTransactionRequest(NewMandateRequest newMandateRequest, string containedData)
        {
            VerifyExpirationPeriod(newMandateRequest.ExpirationPeriod);

            var containedDocument = new XmlDocument();
            containedDocument.LoadXml(containedData);

            var acquirerTrxReq = new AcquirerTrxReq
            {
                createDateTimestamp = Now(),
                productID = this.LocalInstrumentCode == Instrumentation.Core ? ProductId_CORE : ProductId_B2B,
                version = "1.0.0",

                Merchant = new AcquirerTrxReqMerchant
                {
                    merchantID = Configuration.EMandateContractId,
                    subID = Configuration.EMandateContractSubId.ToString(CultureInfo.InvariantCulture),
                    merchantReturnURL = Configuration.MerchantReturnUrl
                },

                Issuer = new AcquirerTrxReqIssuer
                {
                    issuerID = newMandateRequest.DebtorBankId
                },

                Transaction = new AcquirerTrxReqTransaction
                {
                    entranceCode = newMandateRequest.EntranceCode,

                    expirationPeriod = newMandateRequest.ExpirationPeriod.HasValue ? XmlConvert.ToString(newMandateRequest.ExpirationPeriod.Value) : null,
                    language = newMandateRequest.Language.ToString(),
                    container = new Transactioncontainer
                    {
                        Any = new [] { containedDocument.DocumentElement }
                    }
                }
            };

            return ProcessDateTimes(acquirerTrxReq.Serialize());
        }

        public string GetStatusRequest(StatusRequest statusRequest)
        {
            var acquirerStatusReq = new AcquirerStatusReq
            {
                createDateTimestamp = Now(),
                productID = this.LocalInstrumentCode == Instrumentation.Core ? ProductId_CORE : ProductId_B2B,
                version = "1.0.0",

                Merchant = new AcquirerStatusReqMerchant
                {
                    merchantID = Configuration.EMandateContractId,
                    subID = Configuration.EMandateContractSubId.ToString(CultureInfo.InvariantCulture)
                },

                Transaction = new AcquirerStatusReqTransaction
                {
                    transactionID = statusRequest.TransactionId
                }
            };

            return ProcessDateTimes(acquirerStatusReq.Serialize());
        }

        public string GetTransactionRequest(AmendmentRequest amendmentRequest, string containedData)
        {
            VerifyExpirationPeriod(amendmentRequest.ExpirationPeriod);

            var containedDocument = new XmlDocument();
            containedDocument.LoadXml(containedData);

            var acquirerTrxReq = new AcquirerTrxReq
            {
                createDateTimestamp = Now(),
                productID = this.LocalInstrumentCode == Instrumentation.Core ? ProductId_CORE : ProductId_B2B,
                version = "1.0.0",

                Merchant = new AcquirerTrxReqMerchant
                {
                    merchantID = Configuration.EMandateContractId,
                    subID = Configuration.EMandateContractSubId.ToString(CultureInfo.InvariantCulture),
                    merchantReturnURL = Configuration.MerchantReturnUrl
                },

                Issuer = new AcquirerTrxReqIssuer
                {
                    issuerID = amendmentRequest.DebtorBankId
                },

                Transaction = new AcquirerTrxReqTransaction
                {
                    entranceCode = amendmentRequest.EntranceCode,

                    expirationPeriod = amendmentRequest.ExpirationPeriod.HasValue ? XmlConvert.ToString(amendmentRequest.ExpirationPeriod.Value) : null,
                    language = amendmentRequest.Language.ToString(),
                    container = new Transactioncontainer
                    {
                        Any = new[] { containedDocument.DocumentElement }
                    }
                }
            };

            return ProcessDateTimes(acquirerTrxReq.Serialize());
        }

        public string GetTransactionRequest(CancellationRequest cancellationRequest, string containedData)
        {
            VerifyExpirationPeriod(cancellationRequest.ExpirationPeriod);

            var containedDocument = new XmlDocument();
            containedDocument.LoadXml(containedData);

            var acquirerTrxReq = new AcquirerTrxReq
            {
                createDateTimestamp = Now(),
                productID = this.LocalInstrumentCode == Instrumentation.Core ? ProductId_CORE : ProductId_B2B,
                version = "1.0.0",

                Merchant = new AcquirerTrxReqMerchant
                {
                    merchantID = Configuration.EMandateContractId,
                    subID = Configuration.EMandateContractSubId.ToString(CultureInfo.InvariantCulture),
                    merchantReturnURL = Configuration.MerchantReturnUrl
                },

                Issuer = new AcquirerTrxReqIssuer
                {
                    issuerID = cancellationRequest.DebtorBankId
                },

                Transaction = new AcquirerTrxReqTransaction
                {
                    entranceCode = cancellationRequest.EntranceCode,

                    expirationPeriod = cancellationRequest.ExpirationPeriod.HasValue ? XmlConvert.ToString(cancellationRequest.ExpirationPeriod.Value) : null,
                    language = cancellationRequest.Language.ToString(),
                    container = new Transactioncontainer
                    {
                        Any = new[] { containedDocument.DocumentElement }
                    }
                }
            };

            return ProcessDateTimes(acquirerTrxReq.Serialize());
        }

        private string ProcessDateTimes(string input)
        {
            string[] dateTimeElementNames =
            {
                "directoryDateTimestamp", "createDateTimestamp", "transactionCreateDateTimestamp", "statusDateTimestamp",
            };

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);

            foreach (var elementName in dateTimeElementNames)
            {
                foreach (XmlElement element in doc.GetElementsByTagName(elementName, "*"))
                {
                    var existing = DateTime.Parse(element.InnerText, CultureInfo.InvariantCulture);
                    var newval = existing.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'");
                    element.InnerText = newval;
                }
            }

            return doc.OuterXml;
        }
    }
}
