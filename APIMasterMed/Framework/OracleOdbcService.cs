using System.Data;
using System.Data.Odbc;

namespace APIMasterMed.Framework
{
    public class OracleOdbcService
    {
        private readonly string _connectionString;

        public OracleOdbcService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleODBC")
                      ?? throw new InvalidOperationException("Connection string 'OracleODBC' não foi encontrada.");
        }

        public DataTable Execute(string SQL)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = new OdbcConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new OdbcCommand(SQL, connection))
                    using (var adapter = new OdbcDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }

                return dataTable;
            }
            catch (OdbcException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

    }
}
