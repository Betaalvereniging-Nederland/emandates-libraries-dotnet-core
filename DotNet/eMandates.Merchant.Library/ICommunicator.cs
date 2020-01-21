using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// ICoreCommunicator interface, implemented by CoreCommunicator: has all methods except Cancel mandate
    /// </summary>
    public interface ICoreCommunicator
    {
        /// <summary>
        ///     Sends a directory request to the URL specified in Configuration.AcquirerUrl_DirectoryReq
        /// </summary>
        /// <returns>
        ///     A DirectoryResponse object which contains the response from the server (a list of debtor banks), or error information when an error occurs
        /// </returns>
        DirectoryResponse Directory();

        /// <summary>
        ///     Sends a new mandate request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="newMandateRequest">A NewMandateRequest object</param>
        /// <returns>
        ///     A NewMandateResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        NewMandateResponse NewMandate(NewMandateRequest newMandateRequest);

        /// <summary>
        ///     Sends a transaction status request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="statusRequest">A StatusRequest object</param>
        /// <returns>
        ///     A StatusResponse object which contains the response from the server (transaction id, status message), or error information when an error occurs
        /// </returns>
        StatusResponse GetStatus(StatusRequest statusRequest);

        /// <summary>
        ///     Sends an amendment request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="amendmentRequest">An AmendmentRequest object</param>
        /// <returns>
        ///     An AmendmentResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        AmendmentResponse Amend(AmendmentRequest amendmentRequest);
    }

    /// <summary>
    /// IB2BCommunicator interface, implemented by B2BCommunicator; inherits ICoreCommunicator and adds the Cancel mandate method
    /// </summary>
    public interface IB2BCommunicator : ICoreCommunicator
    {
        /// <summary>
        ///     Sends a cancellation request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="cancellationRequest">A CancellationRequest object</param>
        /// <returns>
        ///     A CancellationResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        CancellationResponse Cancel(CancellationRequest cancellationRequest);
    }
}
