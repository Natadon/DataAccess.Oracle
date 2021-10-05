using Natadon.DataAccess.Core;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Natadon.DataAccess.Oracle
{
    public class OracleDataAccess : IDataAccess
    {
        // The connection string for this connection to the database
        private readonly string connectionString;

        // The OracleCommand is used to connect to and run commands on the database.
        private OracleCommand command;

        // The connection to the server
        private OracleConnection connection;

        // The database adapter allows us to convert the results from the database to a System.Data.DataSet
        private OracleDataAdapter dataAdapter;

        // The command timeout interval (in seconds) 0 is no timeout.
        private int commandTimeout;

        /// <summary>
        /// Creates an instance of the OracleDataAccess
        /// </summary>
        /// <param name="ServerName">The name of the Oracle server to connect to</param>
        /// <param name="UserName">Username to login with</param>
        /// <param name="Password">Password for the provided user account</param>
        /// <param name="CommandTimeout">Connection timeout in seconds (default is 30 seconds)</param>
        public OracleDataAccess(string ServerName, string UserName, string Password, int CommandTimeout = 30)
        {
            connectionString = string.Format("Data Source={0};User Id={1};Password={2}", ServerName, UserName, Password);

            connection = new OracleConnection(connectionString);
            commandTimeout = CommandTimeout;
        }

        /// <summary>
        /// Execute a query against the database
        /// </summary>
        /// <param name="SqlStatement">SQL Statement to run</param>
        /// <param name="Params">Optional list of parameters</param>
        public void ExecuteQuery(string SqlStatement, List<DataAccessParameter> Params = null)
        {
            connection.Open();

            try
            {
                command = new OracleCommand(SqlStatement, connection);
                command.CommandTimeout = commandTimeout;

                if(Params != null)
                {
                    List<OracleParameter> oracleParameters = getParams(Params).ToList();

                    command.Parameters.AddRange(oracleParameters.ToArray());
                }

                command.ExecuteNonQuery();
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                command.Dispose();

                connection.Close();
            }
        }

        /// <summary>
        /// Returns a dataset from the provided query
        /// </summary>
        /// <param name="SqlStatement">SQL Statement to run</param>
        /// <param name="Params">Optional list of parameters</param>
        /// <returns>System.Data.DataSet with the results of the query</returns>
        public DataSet GetDataSet(string SqlStatement, List<DataAccessParameter> Params = null)
        {
            connection.Open();

            try
            {
                command = new OracleCommand(SqlStatement, connection);
                command.CommandTimeout = commandTimeout;

                if(Params != null)
                {
                    List<OracleParameter> oracleParameters = getParams(Params).ToList();

                    command.Parameters.AddRange(oracleParameters.ToArray());
                }

                dataAdapter = new OracleDataAdapter(command);
                DataSet ds = new DataSet();

                dataAdapter.Fill(ds);

                return ds;
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                command.Dispose();

                dataAdapter.Dispose();

                connection.Close();
            }
        }

        /// <summary>
        /// Returns the first value in the results (first column of first row if multiple values result)
        /// </summary>
        /// <param name="SqlStatement">SQL Statement to run</param>
        /// <param name="Params">Optional list of parameters</param>
        /// <returns>The first result of the query as an object</returns>
        public object GetScalar(string SqlStatement, List<DataAccessParameter> Params = null)
        {
            connection.Open();

            try
            {
                command = new OracleCommand(SqlStatement, connection);
                command.CommandTimeout = commandTimeout;

                if (Params != null)
                {
                    List<OracleParameter> oracleParameters = getParams(Params).ToList();

                    command.Parameters.AddRange(oracleParameters.ToArray());
                }

                object obj = command.ExecuteScalar();

                return obj;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Dispose();

                connection.Close();
            }
        }

        /// <summary>
        /// Converts the list of Natadon.DataAccessParameters to OracleParameters
        /// </summary>
        /// <param name="accessParameters">The list of DataAccessParameters</param>
        /// <returns>The parameters as a list of OracleParameters</returns>
        private IEnumerable<OracleParameter> getParams(IEnumerable<DataAccessParameter> accessParameters)
        {
            List<OracleParameter> retVal = new List<OracleParameter>();

            foreach(var item in accessParameters)
            {
                retVal.Add(new OracleParameter(item.ParameterName, item.Value));
            }

            return retVal;
        }
    }
}
