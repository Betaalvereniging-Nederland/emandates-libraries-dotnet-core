using System;
using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Library
{
    using p12 = XML.Schemas.pain012;

    /// <summary>
    /// Received as part of a status response, corresponding to the pain.012 message
    /// </summary>
    public class AcceptanceReport
    {
        /// <summary>
        /// Message Identification
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Message timestamp
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Validation reference
        /// </summary>
        public string ValidationReference { get; set; }

        /// <summary>
        /// Original Message ID
        /// </summary>
        public string OriginalMessageId { get; set; }

        /// <summary>
        /// Refers to the type of validation request that preceded the acceptance report
        /// </summary>
        public string MessageNameId { get; set; }

        /// <summary>
        /// Whether or not the mandate is accepted by the debtor
        /// </summary>
        public bool AcceptedResult { get; set; }

        /// <summary>
        /// Original mandate ID
        /// </summary>
        public string OriginalMandateId { get; set; }

        /// <summary>
        /// Mandate request ID
        /// </summary>
        public string MandateRequestId { get; set; }

        /// <summary>
        /// SEPA
        /// </summary>
        public string ServiceLevelCode { get; set; }

        /// <summary>
        /// Core or B2B
        /// </summary>
        public Instrumentation LocalInstrumentCode { get; set; }

        /// <summary>
        /// Sequence Type: recurring or one-off
        /// </summary>
        public SequenceType SequenceType { get; set; }

        /// <summary>
        /// Maximum amount
        /// </summary>
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// Reason for eMandate
        /// </summary>
        public string EMandateReason { get; set; }

        /// <summary>
        /// Direct Debit ID of the Creditor
        /// </summary>
        public string CreditorId { get; set; }

        /// <summary>
        /// SEPA
        /// </summary>
        public string SchemeName { get; set; }

        /// <summary>
        /// Name of the Creditor
        /// </summary>
        public string CreditorName { get; set; }

        /// <summary>
        /// Country of the postal address of the Creditor
        /// </summary>
        public string CreditorCountry { get; set; }

        /// <summary>
        /// The Creditor’s address: P.O. Box or street name + building + add-on + Postcode + City.
        /// Second Address line only to be used if 70 chars are exceeded in the first line
        /// </summary>
        public string[] CreditorAddressLine { get; set; }

        /// <summary>
        /// Name of the company (or daughter-company, or label etc.) for which the Creditor is processing eMandates.
        /// May only be used when meaningfully different from CreditorName
        /// </summary>
        public string CreditorTradeName { get; set; }

        /// <summary>
        /// Account holder name of the account that is used for the eMandate
        /// </summary>
        public string DebtorAccountName { get; set; }

        /// <summary>
        /// Reference ID that identifies the Debtor to the Creditor. Issued by the Creditor
        /// </summary>
        public string DebtorReference { get; set; }

        /// <summary>
        /// Debtor’s bank account number
        /// </summary>
        public string DebtorIban { get; set; }

        /// <summary>
        /// BIC of the Debtor bank
        /// </summary>
        public string DebtorBankId { get; set; }

        /// <summary>
        /// Name of the person signing the eMandate. In case of multiple signing, all signer names must be included in this field, separated by commas.
        /// If the total would exceed the maximum of 70 characters, the names are cut off at 65 characters and “e.a.” is added after the last name.
        /// </summary>
        public string DebtorSignerName { get; set; }

        /// <summary>
        /// The response XML
        /// </summary>
        public string RawMessage { get; set; }

        private AcceptanceReport(p12.Document document)
        {
            var grpHdr = document.MndtAccptncRpt.GrpHdr;
            var accDtls = document.MndtAccptncRpt.UndrlygAccptncDtls[0];

            MessageId = grpHdr.MsgId;
            DateTime = grpHdr.CreDtTm;
            ValidationReference = grpHdr.Authstn[0].Item as string;
            OriginalMessageId = accDtls.OrgnlMsgInf.MsgId;
            MessageNameId = accDtls.OrgnlMsgInf.MsgNmId;
            AcceptedResult = accDtls.AccptncRslt.Accptd;

            var origMndt = (p12.Mandate5)accDtls.OrgnlMndt.Item;

            OriginalMandateId = origMndt.MndtId;
            MandateRequestId = origMndt.MndtReqId;
            ServiceLevelCode = origMndt.Tp.SvcLvl.Item;
            LocalInstrumentCode = (Instrumentation) Enum.Parse(typeof(Instrumentation), origMndt.Tp.LclInstrm.Item, true);
            SequenceType = (SequenceType) origMndt.Ocrncs.SeqTp;
            MaxAmount = (origMndt.MaxAmt != null)? origMndt.MaxAmt.Value : 0;
            EMandateReason = (origMndt.Rsn) != null? origMndt.Rsn.Item : String.Empty;
            CreditorId = ((p12.PersonIdentification5) origMndt.CdtrSchmeId.Id.Item).Othr[0].Id;
            SchemeName = ((p12.PersonIdentification5) origMndt.CdtrSchmeId.Id.Item).Othr[0].SchmeNm.Item;
            CreditorName = origMndt.Cdtr.Nm;
            CreditorCountry = origMndt.Cdtr.PstlAdr.Ctry;
            CreditorAddressLine = origMndt.Cdtr.PstlAdr.AdrLine;
            CreditorTradeName = (origMndt.UltmtCdtr != null) ? origMndt.UltmtCdtr.Nm : String.Empty;
            DebtorAccountName = origMndt.Dbtr.Nm;
            DebtorReference = (origMndt.Dbtr.Id != null) ? ((p12.PersonIdentification5)origMndt.Dbtr.Id.Item).Othr[0].Id : String.Empty;
            DebtorIban = (string)origMndt.DbtrAcct.Id.Item;
            DebtorBankId = origMndt.DbtrAgt.FinInstnId.BICFI;
            DebtorSignerName = origMndt.UltmtDbtr.Nm;
        }

        internal static AcceptanceReport Parse(string xml)
        {
            var doc = p12.Document.Deserialize(xml);
            var acc = new AcceptanceReport(doc);
            acc.RawMessage = xml;
            return acc;
        }
    }
}
