using System;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Describes a new mandate request
    /// </summary>
    public class NewMandateRequest
    {
        /// <summary>
        /// Parameterless constructor, so it can be used as a Model in views
        /// </summary>
        public NewMandateRequest()
        {
        }

        /// <summary>
        /// Constructor that highlights all required fields for this object; use this one to specify your own messageId
        /// </summary>
        public NewMandateRequest(string entranceCode, string language, string eMandateId, string eMandateReason, string debtorReference,
            string debtorBankId, string purchaseId, SequenceType sequenceType, string messageId)
        {
            this.EntranceCode = entranceCode;
            this.Language = language;
            this.EMandateId = eMandateId;
            this.EMandateReason = eMandateReason;
            this.DebtorReference = debtorReference;
            this.DebtorBankId = debtorBankId;
            this.PurchaseId = purchaseId;
            this.SequenceType = sequenceType;
            this.MessageId = messageId;
        }

        /// <summary>
        /// Constructor that highlights all required fields for this object; use this one if you wish the library to generate a MessageId
        /// </summary>
        public NewMandateRequest(string entranceCode, string language, string eMandateId, string eMandateReason, string debtorReference,
            string debtorBankId, string purchaseId, SequenceType sequenceType)
        {
            this.EntranceCode = entranceCode;
            this.Language = language;
            this.EMandateId = eMandateId;
            this.EMandateReason = eMandateReason;
            this.DebtorReference = debtorReference;
            this.DebtorBankId = debtorBankId;
            this.PurchaseId = purchaseId;
            this.SequenceType = sequenceType;
            this.MessageId = Misc.MessageIdGenerator.New();
        }

        /// <summary>
        /// An 'authentication identifier' to facilitate continuation of the session between creditor and debtor, even
        /// if the existing session has been lost. It enables the creditor to recognise the debtor associated with a (completed) transaction.
        /// </summary>
        public string EntranceCode { get; set; }

        /// <summary>
        /// This field enables the debtor bank's site to select the debtor's preferred language (e.g. the language selected on the creditor's site),
        /// if the debtor bank's site supports this: Dutch = 'nl', English = 'en'
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Optional: The period of validity of the transaction request as stated by the creditor measured from the receipt by the debtor bank.
        /// The debtor must authorise the transaction within this period.
        /// </summary>
        public TimeSpan? ExpirationPeriod { get; set; }

        /// <summary>
        /// Message ID for pain message
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// BIC of the Debtor Bank
        /// </summary>
        public string DebtorBankId { get; set; }

        /// <summary>
        /// ID that identifies the mandate and is issued by the creditor
        /// </summary>
        public string EMandateId { get; set; }

        /// <summary>
        /// Indicates type of eMandate: one-off or sequenceType direct debit. 
        /// </summary>
        public SequenceType SequenceType { get; set; }

        /// <summary>
        /// Reason of the mandate
        /// </summary>
        public string EMandateReason { get; set; }

        /// <summary>
        /// Reference ID that identifies the debtor to creditor, which is issued by the creditor
        /// </summary>
        public string DebtorReference { get; set; }

        /// <summary>
        /// A purchaseID that acts as a reference from eMandate to the purchase-order
        /// </summary>
        public string PurchaseId { get; set; }

        /// <summary>
        /// Maximum amount. Not allowed for Core, optional for B2B.
        /// </summary>
        public decimal? MaxAmount { get; set; }
    }
}
