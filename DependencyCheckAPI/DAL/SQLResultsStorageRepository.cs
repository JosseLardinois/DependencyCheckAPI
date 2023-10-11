using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class SQLResultsStorageRepository : ISQLResultsStorageRepository
{
    private string _DBconnectionString;
    private readonly ILogger<SQLResultsStorageRepository> _logger;

    public SQLResultsStorageRepository(IConfiguration configuration, ILogger<SQLResultsStorageRepository> logger)
    {
        _DBconnectionString = Environment.GetEnvironmentVariable("DBConnectionString");
        _logger = logger;
    }

    public async Task InsertIntoDependencyCheckResults(string projectId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore)
    {
        try
        {
            projectId = projectId ?? "null";
            packageName = packageName ?? "null";
            highestSeverity = highestSeverity ?? "null";
            cveCount = cveCount ?? 0;
            evidenceCount = evidenceCount ?? 0;
            baseScore = baseScore ?? 0;

            using (SqlConnection connection = new SqlConnection(_DBconnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = CreateInsertCommand(connection, projectId, packageName, highestSeverity, cveCount, evidenceCount, baseScore);
                await ExecuteNonQueryCommand(command);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred while inserting into DependencyCheck_Results.");
            throw;
        }
    }

    public async Task<IEnumerable<DependencyCheckResults>> RetrieveDependencyCheckResults(string projectId, string userId)
    {
        if (!await DoesProjectExist(userId, projectId))
        {
            _logger.LogInformation("Project does not exist.");
            return null;
        }

        try
        {
            return await FetchResultsFromDatabase(projectId);
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred while retrieving results.");
            throw;
        }
    }

    public async Task<bool> CheckAndInsertIfNotExistsInProjects(string userId, string projectId)
    {
        try
        {
            string projectType = "Dependency Check";
            string dateTime = GetFormattedDateTime();

            using (SqlConnection connection = new SqlConnection(_DBconnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = CreateCheckAndInsertCommand(connection, userId, projectId, projectType, dateTime);
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred while checking and inserting project.");
            throw;
        }
    }

    private SqlCommand CreateInsertCommand(SqlConnection connection, string projectId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore)
    {
        SqlCommand command = new SqlCommand("INSERT INTO DependencyCheck_Results (ProjectId, PackageName, HighestSeverity, CveCount, EvidenceCount, Basescore) VALUES (@ProjectId, @PackageName, @HighestSeverity, @CveCount, @EvidenceCount, @Basescore)", connection);
        command.Parameters.AddWithValue("@ProjectId", projectId);
        command.Parameters.AddWithValue("@PackageName", packageName);
        command.Parameters.AddWithValue("@HighestSeverity", highestSeverity);
        command.Parameters.AddWithValue("@CveCount", cveCount);
        command.Parameters.AddWithValue("@EvidenceCount", evidenceCount);
        command.Parameters.AddWithValue("@Basescore", baseScore);
        return command;
    }

    private string GetFormattedDateTime()
    {
        return DateTime.Now.ToString();
    }

    private async Task<bool> DoesProjectExist(string userId, string projectId)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_DBconnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Projects WHERE UserId = @UserId AND ProjectId = @ProjectId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, "No existing project for user:" + userId + " with project:" + projectId);
            throw;
        }
    }

    private async Task<IEnumerable<DependencyCheckResults>> FetchResultsFromDatabase(string projectId)
    {
        List<DependencyCheckResults> resultList = new List<DependencyCheckResults>();
        using (SqlConnection connection = new SqlConnection(_DBconnectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("SELECT * FROM DependencyCheck_Results WHERE ProjectId = @ProjectId", connection))
            {
                command.Parameters.AddWithValue("@ProjectId", projectId);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        DependencyCheckResults results = new DependencyCheckResults
                        {
                            Id = (int)reader["Id"],
                            ProjectId = reader["ProjectId"] as string,
                            PackageName = reader["PackageName"] as string,
                            HighestSeverity = reader["HighestSeverity"] as string,
                            CveCount = (int)reader["CveCount"],
                            EvidenceCount = (int)reader["EvidenceCount"],
                            BaseScore = (double)reader["BaseScore"]
                        };
                        resultList.Add(results);
                    }
                }
            }
        }
        return resultList;
    }

    private SqlCommand CreateCheckAndInsertCommand(SqlConnection connection, string userId, string projectId, string projectType, string dateTime)
    {
        SqlCommand command = new SqlCommand("IF NOT EXISTS (SELECT 1 FROM Projects WHERE UserId = @UserId AND ProjectId = @ProjectId) BEGIN INSERT INTO Projects (UserId, ProjectId, ProjectType, CreationDate) VALUES (@UserId, @ProjectId, @ProjectType, @CreationDate) END", connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ProjectId", projectId);
        command.Parameters.AddWithValue("@ProjectType", projectType);
        command.Parameters.AddWithValue("@CreationDate", dateTime);
        return command;
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
