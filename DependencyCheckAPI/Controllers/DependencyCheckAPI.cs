using Microsoft.AspNetCore.Mvc;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.DTO;

namespace DependencyCheckAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DependencyCheckAPI : ControllerBase
    {
        private readonly IDependencyScanService _dependencyScanRepository;
        private readonly IExtractJsonService _extractJson;
        private readonly ISQLResultsService _resultsService;

        public DependencyCheckAPI(IDependencyScanService dependencyScanRepository, IExtractJsonService extractJson, ISQLResultsService resultsService)
        {
            _dependencyScanRepository = dependencyScanRepository;
            _extractJson = extractJson;
            _resultsService = resultsService;
        }

        [HttpGet("GetResults")]
        public async Task<IActionResult> GetResults(string projectName)
        {
            return Ok(await _resultsService.GetResults(projectName));
            try
            {
                List<DependencyCheckResultsDTO> result = await _resultsService.GetResults(projectName);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Project does not exist, run scan again!");
                }
                if (!result.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, "No dependency vulnerabilities found, check the html report for assurance!");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
       
    }
}
