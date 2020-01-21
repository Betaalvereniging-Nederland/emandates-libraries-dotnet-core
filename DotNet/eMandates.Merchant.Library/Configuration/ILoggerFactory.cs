namespace eMandates.Merchant.Library.Configuration
{
    /// <summary>
    /// ILoggerFactory interface describes the factory class used to create ILogger objects
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Creates a ILogger object by using a custom Configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        ILogger Create(IConfiguration configuration);
    }
}
