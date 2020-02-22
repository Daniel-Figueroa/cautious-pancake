using System;
using Microsoft.AspNetCore.Mvc;
using CodeFlip.CodeJar.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage.Blob;

namespace CodeFlip.CodeJar.Api.Controllers
{
    [ApiController]
    public class CampaginsController : ControllerBase
    {

        SQL foo {get;set;}

        private IConfiguration _config { get; set; }


        public ctr(IConfiguration Config)
        {
           _config = Config;
        }

        public CloudBlockBlob CBB { get; set; }


        [HttpGet("campaigns")]
        public IActionResult GetAllCampaigns()
        {

            //CloudBlockBlob foo = new CloudBlockBlob(_config.GetSection("SeedBlobUrl").DownloadRangeToByteArray());
            //var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("SeedBlobUrl"));


            return Ok(
                new[]
                {
                    new { id = "1", name = "campaign 01", numberOfCodes = "10", dateActive = DateTime.Now, dateExpires = DateTime.Now.AddDays(1) },
                    new { id = "2", name = "campaign 02", numberOfCodes = "100", dateActive = DateTime.Now.AddDays(1), dateExpires = DateTime.Now.AddDays(2) },
                    new { id = "3", name = "campaign 03", numberOfCodes = "1000", dateActive = DateTime.Now.AddDays(2), dateExpires = DateTime.Now.AddDays(3) }
                }
            );
        }

        [HttpGet("campaigns/{id}")]
        public IActionResult GetCampaign(int id)
        {
            return Ok(new { id = "1", name = "campaign 01", numberOfCodes = "10", dateActive = DateTime.Now, dateExpires = DateTime.Now.AddDays(1) });
        }

        [HttpPost("campaigns")]
        public IActionResult CreateCampaign([FromBody] CreateCampaignRequest request)
        {
            var sql = new SQL(_config.GetConnectionString("SQLConnnection"), _config.GetSection("SeedBlobUrl"));

            sql.CreateCampaign();

            return Ok();
        }

        [HttpDelete("campaigns/{id}")]
        public IActionResult DeactivateCampaign(int id)
        {
            return Ok();
        }

        [HttpGet("campaigns/{id}/codes")]
        public IActionResult GetCodes([FromRoute] int id, [FromQuery] int page)
        {
            return Ok(
                new
                {
                    pageNumber = page,
                    pageCount = 1,
                    codes = new[] { new { stringValue = "ASKJSJQ", state = 1 }, new { stringValue = "AWEORJZ", state = 2 } }
                }
            );
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
