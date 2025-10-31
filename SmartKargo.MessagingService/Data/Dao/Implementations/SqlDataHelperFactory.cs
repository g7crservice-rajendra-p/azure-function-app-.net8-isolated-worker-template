using Microsoft.Extensions.DependencyInjection;
using SmartKargo.MessagingService.Data.Dao.Interfaces;

namespace SmartKargo.MessagingService.Data.Dao.Implementations
{
    /// <summary>
    /// Factory responsible for creating <see cref="ISqlDataHelperDao"/> instances
    /// with either read-only or read-write connection modes.
    /// This allows dynamic creation without directly exposing implementation details.
    /// </summary>
    public sealed class SqlDataHelperFactory : ISqlDataHelperFactory
    {
        #region Private Fields

        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataHelperFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider used for resolving dependencies.</param>
        public SqlDataHelperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an instance of <see cref="ISqlDataHelperDao"/> configured for 
        /// either read-only or read-write database operations.
        /// </summary>
        /// <param name="readOnly">If true, initializes a DAO with read-only connections.</param>
        /// <returns>An instance of <see cref="ISqlDataHelperDao"/>.</returns>
        public ISqlDataHelperDao Create(bool readOnly)
        {
            // TODO: Add logging — Log when DAO instance is being created (include readOnly flag)
            return ActivatorUtilities.CreateInstance<SqlDataHelperDao>(_serviceProvider, readOnly);
        }

        #endregion
    }
}
