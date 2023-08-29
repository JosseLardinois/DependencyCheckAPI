using DependencyCheckAPI.Dto;
using DependencyCheckAPI.Interfaces;
using System.Data.SqlClient;

public class SQLResultsStorage : ISQLResultsStorage
{
    private string _DBconnectionString;
    private readonly ILogger<SQLResultsStorage> _logger;

    public SQLResultsStorage(IConfiguration configuration, ILogger<SQLResultsStorage> logger)
    {
            _DBconnectionString = Environment.GetEnvironmentVariable("DBConnectionString");
            _logger = logger;

    }

    public void InsertIntoDependencyCheckResults(string projectId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore)
    {
        try
        {
            projectId = projectId ?? "null";
            packageName = packageName ?? "null";
            highestSeverity = highestSeverity ?? "null";
            cveCount = cveCount ?? 0;
            evidenceCount = evidenceCount ?? 0;
            baseScore = baseScore ?? 0;

            _logger.LogInformation("Package Info:");
            _logger.LogInformation(projectId);
            _logger.LogInformation(packageName);
            _logger.LogInformation(highestSeverity);
            _logger.LogInformation(cveCount.ToString());
            _logger.LogInformation(evidenceCount.ToString());

            using (SqlConnection connection = new SqlConnection(_DBconnectionString))
            {
                connection.Open();
                SqlCommand command = CreateInsertCommand(connection, projectId, packageName, highestSeverity, cveCount, evidenceCount, baseScore);
                ExecuteNonQueryCommand(command);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred while inserting into DependencyCheck_Results.");
            throw;
        }
    }

    public List<DependencyCheckResultsDTO> RetrieveDependencyCheckResults(string projectId, string userId)
    {
        if (!DoesProjectExist(userId, projectId))
        {
            _logger.LogInformation("Project does not exist.");
            return null;
        }

        try
        {
            List<DependencyCheckResultsDTO> resultDtos = FetchResultsFromDatabase(projectId);
            return resultDtos;
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred while retrieving results.");
            throw;
        }
    }


    public bool CheckAndInsertIfNotExistsInProjects(string userId, string projectId)
    {
        try
        {
            string projectType = "Dependency Check";
            string dateTime = GetFormattedDateTime();

            using (SqlConnection connection = new SqlConnection(_DBconnectionString))
            {
                connection.Open();
                SqlCommand command = CreateCheckAndInsertCommand(connection, userId, projectId, projectType, dateTime);
                int rowsAffected = command.ExecuteNonQuery();
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

    private bool DoesProjectExist(string userId, string projectId)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_DBconnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Projects WHERE UserId = @UserId AND ProjectId = @ProjectId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, "No existing project for user:" + userId + "with project:"+ projectId);
            throw;
        }
    }

    private List<DependencyCheckResultsDTO> FetchResultsFromDatabase(string projectId)
    {
        List<DependencyCheckResultsDTO> resultDtos = new List<DependencyCheckResultsDTO>();
        using (SqlConnection connection = new SqlConnection(_DBconnectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand("SELECT * FROM DependencyCheck_Results WHERE ProjectId = @ProjectId", connection))
            {
                command.Parameters.AddWithValue("@ProjectId", projectId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DependencyCheckResultsDTO resultDto = new DependencyCheckResultsDTO
                        {
                            Id = (int)reader["Id"],
                            ProjectId = reader["ProjectId"] as string,
                            PackageName = reader["PackageName"] as string,
                            HighestSeverity = reader["HighestSeverity"] as string,
                            CveCount = (int)reader["CveCount"],
                            EvidenceCount = (int)reader["EvidenceCount"],
                            BaseScore = (double)reader["BaseScore"]
                        };
                        resultDtos.Add(resultDto);
                    }
                }
            }
        }
        return resultDtos;
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

    private void ExecuteNonQueryCommand(SqlCommand command)
    {
        try
        {
            command.ExecuteNonQuery();
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
