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
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("BinaryFile")["Binary"]);

            return Ok(sql.GetCampaigns());
        }

        [HttpGet("campaigns/{id}")]
        public IActionResult GetCampaign(int id)
        {
            return Ok();
        }

        [HttpPost("campaigns")]
        public IActionResult CreateCampaign([FromBody] CreateCampaignRequest request, Campaign campaign)
        {
            //var binaryFile = new (_config.GetSection("SeedBlobUrl")

            if (campaign.DateActive >= DateTime.Now.Date)
            {
                var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("BinaryFile")["Binary"]);
                sql.CreateCampaign(campaign);

                return Ok(campaign);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("campaigns/{id}")]
        public IActionResult DeactivateCampaign(int id, [FromBody] Campaign campaign)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("BinaryFile")["Binary"]);
            sql.DeactivateCampaign(campaign);

            return Ok();
        }

        [HttpGet("campaigns/{id}/codes")]
        public IActionResult GetCodes([FromRoute] int id, [FromQuery] int page)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("BinaryFile")["Binary"]);
            var pageSize = Convert.ToInt32(_config.GetSection("Pagination")["PageNumber"]);
            var pages = sql.PageCount(id);
            var alphabet = _config.GetSection("Base26")["Alphabet"];
            var codes = sql.GetCodes(id, alphabet, page, pageSize);

            return Ok(new TableData(codes, pages));
        }

        [HttpDelete("campaigns/{campaignId}/codes/{code}")]
        public IActionResult DeactivateCode([FromRoute] int campaignId, [FromRoute] string code)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("BinaryFile")["Binary"]);
            var alphabet = _config.GetSection("Base26")["Alphabet"];
            sql.DeactiveCode(alphabet,code);

            return Ok();
        }

        [HttpPost("codes/{code}")]
        public IActionResult RedeemCode([FromRoute] string code)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("BinaryFile")["Binary"]);
            var alphabet = _config.GetSection("Base26")["Alphabet"];
            
            sql.CheckIfCodeCanBeRedeemed(code,alphabet);

            return Ok();
        }
    }
}
