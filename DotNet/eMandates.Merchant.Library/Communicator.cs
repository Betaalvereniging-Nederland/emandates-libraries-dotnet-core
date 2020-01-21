using System;
using System.Net.Http;
using eMandates.Merchant.Library.Configuration;
using eMandates.Merchant.Library.MessageBuilders;
using eMandates.Merchant.Library.XML;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Communicator class, to be used for sending messages where LocalInstrumentationCode = CORE
    /// </summary>
    public class CoreCommunicator
    {
        /// <summary>
        /// Configuration instance used with this CoreCommunicator
        /// </summary>
        protected internal IConfiguration LocalConfiguration { get; private set; }

        /// <summary>
        /// Logger instance, to be used for logging iso pain raw messages and library messages
        /// </summary>
        protected internal ILogger Logger { get; set; }

        /// <summary>
        /// XmlProcessor instance, used to process XMLs (signing, verifying, validating signature)
        /// </summary>
        protected internal IXmlProcessor XmlProcessor { get; set; }

        /// <summary>
        /// LocalInstrumentCode used by the current instance (can be CORE or B2B)
        /// </summary>
        protected internal Instrumentation LocalInstrumentCode { get; set; }

        /// <summary>
        /// Constructs a new Communicator, initializes the Configuration and sets LocalInstrumentCode = CORE
        /// </summary>
        /// <param name="configuration"></param>
        public CoreCommunicator(IConfiguration configuration)
        {
            LocalConfiguration = configuration;

            Logger = LocalConfiguration.LoggerFactory.Create(LocalConfiguration);
            XmlProcessor = new XmlProcessor(LocalConfiguration);
            LocalInstrumentCode = Instrumentation.Core;

            Logger.Log("communicator initialized with custom configuration");
        }

        /// <summary>
        /// Sign a message using the SigningCertificate
        /// </summary>
        protected internal string Sign(string xml)
        {
            return XmlProcessor.AddSignature(xml);
        }

        /// <summary>
        /// Verify an incoming message's signature using the AcquirerCertificate
        /// </summary>
        protected internal bool VerifySignature(string xml)
        {
            return XmlProcessor.VerifySignature(xml);
        }

        /// <summary>
        /// Verify that a message is correct according to the XML schemas
        /// </summary>
        protected internal bool VerifySchema(string xml)
        {
            return XmlProcessor.VerifySchema(xml);
        }

        /// <summary>
        /// Perform the http(s) request and return the result
        /// </summary>
        protected internal string PerformRequest(string xml, string url)
        {
            Logger.Log("sending request to {0}", url);

            try
            {
                VerifySchema(xml);
            }
            catch (Exception e)
            {
                Logger.Log("request xml schema is not valid: {0}", e.Message);
                throw new CommunicatorException("Request XML schema is not valid.", e);
            }

            Logger.LogXmlMessage(xml);

            var content = "";

            using (var httpClient = new HttpClient())
            {
                //httpClient.DefaultRequestHeaders.Add("Content-Type", "text/xml; charset='utf-8'");
                var task = httpClient.PostAsync(url, new StringContent(xml, System.Text.Encoding.UTF8, "text/xml"));

                var result = task.Result;
                Logger.Log("result status: {0}", result.StatusCode);
                if (!result.IsSuccessStatusCode)
                {
                    Logger.Log("http request failed: {0}", result.StatusCode);
                    throw new CommunicatorException("Http request failed, code=" + result.StatusCode);
                }

                content = result.Content.ReadAsStringAsync().Result;
            }

            Logger.LogXmlMessage(content);

            try
            {
                VerifySchema(content);
                Logger.Log("Response XML schema is valid.");
            }
            catch (Exception e)
            {
                Logger.Log("response xml schema is not valid: {0}", e.Message);
                throw new CommunicatorException("Response XML schema is not valid.", e);
            }

            bool signatureIsValid = VerifySignature(content);
            Logger.Log("signature is valid: {0}", signatureIsValid);
            if (!signatureIsValid)
            {
                Logger.Log("request xml signature is not valid");
                throw new CommunicatorException("Response XML signature is not valid.");
            }

            return content;
        }

        /// <summary>
        ///     Sends a directory request to the URL specified in Configuration.AcquirerUrl_DirectoryReq
        /// </summary>
        /// <returns>
        ///     A DirectoryResponse object which contains the response from the server (a list of debtor banks), or error information when an error occurs
        /// </returns>
        public DirectoryResponse Directory()
        {
            try
            {
                Logger.Log("sending new directory request");

                Logger.Log("building idx message");
                var directoryreq = new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetDirectoryRequest();

                Logger.Log("signing message");
                var xml = Sign(directoryreq);

                var content = PerformRequest(xml, LocalConfiguration.AcquirerUrlDirectoryReq);

                return DirectoryResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.Log("error : {0}", e);
                return DirectoryResponse.Get(e);
            }
        }

        /// <summary>
        ///     Sends a new mandate request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="newMandateRequest">A NewMandateRequest object</param>
        /// <returns>
        ///     A NewMandateResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        public NewMandateResponse NewMandate(NewMandateRequest newMandateRequest)
        {
            try
            {
                Logger.Log("sending new eMandate transaction");

                Logger.Log("building eMandate");
                var document = new EMandateMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetNewMandate(newMandateRequest);

                Logger.Log("building idx message");
                var acquirertrxreq =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetTransactionRequest(newMandateRequest, document);

                Logger.Log("signing message");
                var xml = Sign(acquirertrxreq);

                var content = PerformRequest(xml, LocalConfiguration.AcquirerUrlTransactionReq);

                return NewMandateResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.Log("error : {0}", e);
                return NewMandateResponse.Get(e);
            }
        }

        /// <summary>
        ///     Sends a transaction status request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="statusRequest">A StatusRequest object</param>
        /// <returns>
        ///     A StatusResponse object which contains the response from the server (transaction id, status message), or error information when an error occurs
        /// </returns>
        public StatusResponse GetStatus(StatusRequest statusRequest)
        {
            try
            {
                Logger.Log("sending new status request");

                Logger.Log("building idx message");
                var acquirerstsreq =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetStatusRequest(statusRequest);

                Logger.Log("signing message");
                var xml = Sign(acquirerstsreq);

                var content = PerformRequest(xml, LocalConfiguration.AcquirerUrlStatusReq);

                return StatusResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.Log("error : {0}", e);
                return StatusResponse.Get(e);
            }
        }

        /// <summary>
        ///     Sends an amendment request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="amendmentRequest">An AmendmentRequest object</param>
        /// <returns>
        ///     An AmendmentResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        public AmendmentResponse Amend(AmendmentRequest amendmentRequest)
        {
            try
            {
                Logger.Log("sending new amend request");

                Logger.Log("building eMandate");
                var document = new EMandateMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetAmend(amendmentRequest);

                Logger.Log("building idx message");
                var acquirertrxreq =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetTransactionRequest(amendmentRequest, document);

                Logger.Log("signing message");
                var xml = Sign(acquirertrxreq);

                var content = PerformRequest(xml, LocalConfiguration.AcquirerUrlTransactionReq);

                return AmendmentResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.Log("error : {0}", e);
                return AmendmentResponse.Get(e);
            }
        }
    }

    /// <summary>
    /// Communicator class, to be used for sending messages where LocalInstrumentationCode = B2B
    /// </summary>
    public class B2BCommunicator : CoreCommunicator, IB2BCommunicator
    {
        /// <summary>
        /// Default constructor, does initialization and sets LocalInstrumentCode to B2B
        /// </summary>
        /// <param name="configuration"></param>
        public B2BCommunicator(IConfiguration configuration):base(configuration)
        {
            LocalInstrumentCode = Instrumentation.B2B;
        }

        /// <summary>
        ///     Sends a cancellation request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="cancellationRequest">A CancellationRequest object</param>
        /// <returns>
        ///     A CancellationResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        public CancellationResponse Cancel(CancellationRequest cancellationRequest)
        {
            try
            {
                Logger.Log("sending new eMandate transaction");

                Logger.Log("building eMandate");
                var document = new EMandateMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetCancel(cancellationRequest);

                Logger.Log("building idx message");
                var acquirertrxreq =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetTransactionRequest(cancellationRequest, document);

                Logger.Log("signing message");
                var xml = Sign(acquirertrxreq);

                var content = PerformRequest(xml, LocalConfiguration.AcquirerUrlTransactionReq);

                return CancellationResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.Log("error : {0}", e);
                return CancellationResponse.Get(e);
            }
        }
    }
}
