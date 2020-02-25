using System;
using Microsoft.AspNetCore.Mvc;
using CodeFlip.CodeJar.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace CodeFlip.CodeJar.Api.Controllers
{
    [ApiController]
    public class CampaginsController : ControllerBase
    {

        public CampaginsController(IConfiguration config)
        {
            _config = config;
        }
        private IConfiguration _config;



        [HttpGet("campaigns")]
        public IActionResult GetAllCampaigns()
        {

            return Ok();
        }

        [HttpGet("campaigns/{id}")]
        public IActionResult GetCampaign(int id)
        {
            return Ok();
        }

        [HttpPost("campaigns")]
        public IActionResult CreateCampaign([FromBody] CreateCampaignRequest request, Campaign campaign)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));
            //var binaryFile = new (_config.GetSection("SeedBlobUrl")

            sql.CreateCampaign(campaign);

            return Ok(campaign);
        }

        [HttpDelete("campaigns/{id}")]
        public IActionResult DeactivateCampaign(int id)
        {
            return Ok();
        }

        [HttpGet("campaigns/{id}/codes")]
        public IActionResult GetCodes([FromRoute] int id, [FromQuery] int page)
        {
            return Ok();
        }

        [HttpDelete("campaigns/{campaignId}/codes/{code}")]
        public IActionResult DeactivateCode([FromRoute] int campaignId, [FromRoute] string code)
        {
            return Ok();
        }

        [HttpPost("codes/{code}")]
        public IActionResult RedeemCode([FromRoute] string code)
        {
            return Ok();
        }
    }
}
