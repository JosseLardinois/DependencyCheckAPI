using Dapper;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Data.SqlClient;

namespace DependencyCheckAPI.DAL
{
    public class SQLResultsStorageRepository : ISQLResultsStorageRepository
    {
        private string _DBconnectionString;
        private readonly ILogger<SQLResultsStorageRepository> _logger;

        public SQLResultsStorageRepository(IConfiguration configuration, ILogger<SQLResultsStorageRepository> logger)
        {
            _DBconnectionString = Environment.GetEnvironmentVariable("DCDBCS");
            _logger = logger;
        }



        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_DBconnectionString);
        }

        public async Task InsertIntoDependencyCheckResults(Guid scanId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore)
        {
            try
            {
                packageName = packageName ?? string.Empty;
                highestSeverity = highestSeverity ?? string.Empty;

                // For nullable integers and doubles
                cveCount = cveCount ?? 0;
                evidenceCount = evidenceCount ?? 0;
                baseScore = baseScore ?? 0.0;

                using (SqlConnection connection = new SqlConnection(_DBconnectionString))
                {
                    await connection.OpenAsync();

                    var Id = Guid.NewGuid();
                    using (SqlCommand command = new SqlCommand("INSERT INTO DependencyCheckResults (Id, ScanId, PackageName, HighestSeverity, CveCount, EvidenceCount, Basescore) VALUES (@Id, @ScanId, @PackageName, @HighestSeverity, @CveCount, @EvidenceCount, @Basescore)", connection))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        command.Parameters.AddWithValue("@ScanId", scanId);
                        command.Parameters.AddWithValue("@PackageName", packageName);
                        command.Parameters.AddWithValue("@HighestSeverity", highestSeverity);
                        command.Parameters.AddWithValue("@CveCount", cveCount);
                        command.Parameters.AddWithValue("@EvidenceCount", evidenceCount);
                        command.Parameters.AddWithValue("@Basescore", baseScore);


                        await ExecuteNonQueryCommand(command);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "An error occurred while inserting into DependencyCheck_Results.");
                throw;
            }
        }

        public async Task InsertDependencyInfosIntoDatabase(Guid scanId, List<DependencyCheckResults> dependencyCheckResults)
        {
            if (dependencyCheckResults == null || !dependencyCheckResults.Any())
            {
                await InsertIntoDependencyCheckResults(scanId, null, null, null, null, null);
            }
            else
            {
                foreach (DependencyCheckResults result in dependencyCheckResults)
                {
                    await InsertIntoDependencyCheckResults(scanId, result.PackageName, result.HighestSeverity, result.CveCount, result.EvidenceCount, result.BaseScore);
                }
            }
        }


        public async Task<Guid> CreateScan(string projectName, Guid createdBy)
        {
            var id = Guid.NewGuid();
            var createdAt = DateTimeOffset.Now;
            const string query = @"INSERT INTO scan (Id, ProjectName, CreatedAt, CreatedBy) VALUES (@Id, @ProjectName, @CreatedAt, @CreatedBy);";

            var parameters = new
            {
                Id = id,
                ProjectName = projectName,
                CreatedAt = createdAt,
                CreatedBy = createdBy
            };

            using (var connection = CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, parameters);
                return id;
            }
        }

        public async Task<IEnumerable<DependencyCheckResults>> RetrieveDependencyCheckResults(string projectName)
        {
            try
            {
                return await RetrieveResultsFromDatabase(projectName);
            }
            catch (Exception ex)
            {
                HandleError(ex, "An error occurred while retrieving DependencyCheck_Results.");
                throw;
            }
        }

        private async Task<IEnumerable<DependencyCheckResults>> RetrieveResultsFromDatabase(string projectName)
        {
            var resultList = new List<DependencyCheckResults>();

            using (SqlConnection connection = new SqlConnection(_DBconnectionString))
            {
                await connection.OpenAsync();
                string query = @"
            SELECT dcr.* 
            FROM DependencyCheckResults dcr
            INNER JOIN scan s ON dcr.Scanid = s.id
            WHERE s.ProjectName = @ProjectName";

                using (var command = new SqlCommand(query, connection as SqlConnection))
                {
                    command.Parameters.AddWithValue("@ProjectName", projectName);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            resultList.Add(MapReaderToDependencyCheckResults(reader));
                        }
                    }
                }
            }

            return resultList;
        }

        private DependencyCheckResults MapReaderToDependencyCheckResults(SqlDataReader reader)
        {
            return new DependencyCheckResults
            {
                Id = (Guid)reader["Id"],
                ScanId = reader.IsDBNull(reader.GetOrdinal("Scanid")) ? (Guid?)null : (Guid)reader["Scanid"],
                PackageName = reader["PackageName"] as string,
                HighestSeverity = reader["HighestSeverity"] as string,
                CveCount = reader.IsDBNull(reader.GetOrdinal("CveCount")) ? (int?)null : (int)reader["CveCount"],
                EvidenceCount = reader.IsDBNull(reader.GetOrdinal("EvidenceCount")) ? (int?)null : (int)reader["EvidenceCount"],
                BaseScore = (float)(reader.IsDBNull(reader.GetOrdinal("BaseScore")) ? (double?)null : (double)reader["BaseScore"])
            };
        }

        private async Task ExecuteNonQueryCommand(SqlCommand command)
        {
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error executing SQL command.");
                throw;
            }
        }

        private void HandleError(Exception ex, string errorMessage)
        {
            _logger.LogError(ex, errorMessage);
        }
    }
}
