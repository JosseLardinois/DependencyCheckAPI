using Microsoft.AspNetCore.Mvc;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Dto;

namespace DependencyCheckAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DependencyCheckAPI : ControllerBase
    {
        private readonly IDependencyScanRepository _dependencyScanRepository;
        private readonly IExtractJson _extractJson;
        private readonly IAzureFileRepository _azureRepository;
        private readonly ISQLResultsRepository _resultsRepository;


        public DependencyCheckAPI(IDependencyScanRepository dependencyScanRepository, IExtractJson extractJson, IAzureFileRepository azureRepository, ISQLResultsRepository resultsRepository)
        {
            _dependencyScanRepository = dependencyScanRepository;
            _extractJson = extractJson;
            _azureRepository = azureRepository;
            _resultsRepository = resultsRepository;
        }

        [HttpGet("GetResults")]
        public IActionResult GetResults(string userid, string filename)
        {
            try
            {
                List<DependencyCheckResultsDTO> result = _resultsRepository.GetResults(userid, filename);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Project does not exist, run scan again!");
                }
                if (result.Count() == 0) {
                    return StatusCode(StatusCodes.Status200OK, "No dependency vulnerabilities found, check the html report for ensurance!");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("DependencyCheck")]
        public async Task<IActionResult> DependencyCheck(string filename, string userId)
        {
            // Check if file exists
            if (!await _azureRepository.DoesFileExistInBlob(filename, userId))
            {
                return StatusCode(StatusCodes.Status404NotFound, $"File {filename} not found in container.");
            }
            try
            {
                // Download file
                BlobDto? file = await _azureRepository.GetBlobFile(filename, userId);
                if (file == null)
                {
                    // Was not, return error message to client
                    return StatusCode(StatusCodes.Status500InternalServerError, $"File {filename} could not be downloaded.");
                }

                // Execute dependencyscan
                await _dependencyScanRepository.UnzipFolder(filename);
                Console.WriteLine("dependecyscan executed");

                // Upload report to blob for later inspection
                await _azureRepository.UploadHtmlReport(filename, userId);

                //Make a new project with user
                _extractJson.MakeNewProject(userId, filename);

                // Store main vulnerabilities
                _extractJson.ExtractJson(filename);

                return Ok();
            }
            catch (Exception ex)
            {
                // Handle other exceptions here if needed, and send an appropriate response to the client.
                // You can also log the error if needed.
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

    }
}
