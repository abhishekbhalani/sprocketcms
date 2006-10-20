using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Sprocket.Data
{
	public interface IDatabaseHandler
	{
		/// <summary>
		/// Create tables, check that required data exists, etc.
		/// </summary>
		/// <returns>A <see cref="Result" /> indicating success or
		/// failure</returns>
		Result Initialise();

		/// <summary>
		/// Check configuration settings, if any, and return a result
		/// indicating whether the database has enough information to
		/// successfully call <see cref="Initialise"/>.
		/// </summary>
		/// <returns>A <see cref="Result"/> value indicating if the
		/// <c cref="Initialise" /> method can be called</returns>
		Result CheckConfiguration();

		/// <summary>
		/// Create and return an instance of <see cref="DbConnection" />
		/// for the appropriate database type. Use default configuration
		/// settings for selecting the data source.
		/// </summary>
		/// <returns>A <see cref="DbConnection" /> instance initialised
		/// to point at the default data source</returns>
		DbConnection CreateDefaultConnection();
	}
}