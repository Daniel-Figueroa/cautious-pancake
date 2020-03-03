using System;
using Microsoft.AspNetCore.Mvc;
using CodeFlip.CodeJar.Api.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
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
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));

            return Ok(sql.GetAllCampaigns());
        }

        [HttpGet("campaigns/{id}")]
        public IActionResult GetCampaign(int id)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));
            return Ok(sql.GetCampaignByID(id));
        }

        [HttpPost("campaigns")]
        public IActionResult CreateCampaign([FromBody] CreateCampaignRequest request)
        {
            if (request.campaignSize >= 1 && request.campaignSize <= 30000)
            {
                var campaign = new Campaign()
                {
                    CampaignName = request.campaignName,
                    CampaignSize = request.campaignSize
                };

                var cloudPath = new CloudPath(_config.GetSection("SeedBlobUrl")["BlobUrl"]);
                var sql = new SQL(_config.GetConnectionString("SQLConnnection"));

                var offsetUpdate = sql.UpdateOffset(campaign.CampaignSize);
                var listOfCodes = cloudPath.GenerateCodesFromCloudFile(offsetUpdate);

                sql.CreateCampaign(listOfCodes, campaign);

                return Ok(campaign);
            }
            return BadRequest();
        }

        [HttpDelete("campaigns/{id}")]
        public IActionResult DeactivateCampaign(int id, [FromBody] Campaign campaign)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));

            sql.DeactivateCampaign(id);

            return Ok();
        }

        [HttpGet("campaigns/{id}/codes")]
        public IActionResult GetCodes([FromRoute] int id, [FromQuery] int page)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));
            var pageSize = Convert.ToInt32(_config.GetSection("Pagination")["PageNumber"]);
            var alphabet = _config.GetSection("Base26")["Alphabet"];

            var codes = sql.GetCodes(id, alphabet, page, pageSize);
            var pages = sql.PageCount(id);


            return Ok(new TableData(codes, pages, page));
        }

        [HttpDelete("campaigns/{campaignId}/codes/{code}")]
        public IActionResult DeactivateCode([FromRoute] int campaignId, [FromRoute] string code)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));
            var alphabet = _config.GetSection("Base26")["Alphabet"];

            sql.DeactivateCode(alphabet, code);

            return Ok();
        }

        [HttpPost("codes/{code}")]
        public IActionResult RedeemCode([FromRoute] string code)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));
            var codeConverter = new CodeConverter(_config.GetSection("Base26")["Alphabet"]);
            var seedValue = codeConverter.ConvertFromCode(code);

            sql.CheckIfCodeCanBeRedeemed(seedValue, code);

            return Ok();
        }

        [HttpGet("codes/{code}")]
        public IActionResult SearchCode([FromRoute]string code)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"));
            var codeConverter = new CodeConverter(_config.GetSection("Base26")["Alphabet"]);
            var seedValue = codeConverter.ConvertFromCode(code);

            sql.GetCode(code, codeConverter);

            return Ok(code);
        }
    }
}
