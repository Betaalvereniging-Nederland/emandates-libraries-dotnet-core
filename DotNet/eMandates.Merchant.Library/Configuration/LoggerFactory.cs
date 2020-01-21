namespace eMandates.Merchant.Library.Configuration
{
    /// <summary>
    /// The default logger factory. Will return a new Logger each time the <see cref="LoggerFactory.Create"/> method has been invoked. 
    /// </summary>
    internal class LoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Creates a new Logger object by using a custom Configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ILogger Create(IConfiguration configuration)
        {
            return new Logger(configuration);
        }
    }
}