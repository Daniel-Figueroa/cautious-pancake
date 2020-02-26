using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using Microsoft.Azure.Storage.Blob;


namespace CodeFlip.CodeJar.Api
{
    public class SQL
    {
        public SQL(string connectionString, string filePath)
        {
            Connection = new SqlConnection(connectionString);
            FilePath = filePath;
        }
        public SqlConnection Connection { get; set; }
        public string FilePath { get; set; }

        public Campaign HardCodedCampaignData()
        {
            var campaign = new Campaign
            {
                ID = 1,
                CampaignName = "Test",
                CodeIDStart = 1,
                CodeIDEnd = 10,
                CampaignSize = 10,
                DateActive = DateTime.Now
            };
            return campaign;
        }
        public void CreateCampaign(Campaign campaign)
        {
            //campaign = HardCodedCampaignData();
            //var campaignSize = campaign.CampaignSize;

            SqlTransaction transaction;
            Connection.Open();
            transaction = Connection.BeginTransaction();
            var command = Connection.CreateCommand();
            command.Transaction = transaction;

            try
            {
                command.CommandText = @"DECLARE @codeIDStart int
                SET @codeIDStart = (SELECT ISNULL(MAX(CodeIDEnd),0)FROM Campaign) + 1
                INSERT INTO Campaign (CampaignName, CodeIDStart, CampaignSize)
                VALUES (@campiagnName, @codeIDStart, @campaignSize)
                SELECT SCOPE_IDENTITY()";
                command.Parameters.AddWithValue("@campaignName", campaign.CampaignName);
                command.Parameters.AddWithValue("@codeIDStart", campaign.CodeIDStart);
                command.Parameters.AddWithValue("@campaignSize", campaign.CampaignSize);
                campaign.ID = Convert.ToInt32(command.ExecuteScalar());

                CreateDigitalCode(command, campaign.CampaignSize, campaign);
            }
            catch (Exception foo)
            {
                transaction.Rollback();
            }
            Connection.Close();
        }

        public void CreateDigitalCode(SqlCommand command, int campaignSize, Campaign campaign)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open)))
            {
                var firstLastOffset = UpdateOffset(command, campaignSize);

                if (firstLastOffset[0] % 4 != 0)
                {
                    throw new ArgumentException("Offset Must be divisable by 4");
                }
                for (var i = firstLastOffset[0]; i < firstLastOffset[1]; i += 4)
                {

                    reader.BaseStream.Position = i;
                    var seedvalue = reader.ReadInt32();

                    command.CommandText = $@"INSERT INTO Codes (SeedValue, State) VALUES (@Seedvalue, @Active)";
                    command.Parameters.AddWithValue("@Seedvalue", seedvalue);
                    command.Parameters.AddWithValue("@Active", State.Active);
                    command.ExecuteNonQuery();
                }
            }
        }
        public long[] UpdateOffset(SqlCommand command, int campaignSize)
        {
            var firstLastOffset = new long[2];
            var incrementedOffset = campaignSize * 4;
            command.CommandText = @"UPDATE Offset
            SET OffsetValue = OffsetValue + @incrementedOffset
            OUTPUT INSERTED.OffsetValue
            WHERE ID = 1";
            command.Parameters.AddWithValue("@incrementedOffset", incrementedOffset);
            var updatedOffset = (long)command.ExecuteScalar();

            firstLastOffset[0] = updatedOffset - incrementedOffset;
            firstLastOffset[1] = updatedOffset;
            return firstLastOffset;
        }

        public void DeactiveCode(string alphabet, string code)
        {
            var codeConverter = new CodeConverter(alphabet);
            var seedValue = codeConverter.ConvertFromCode(code);
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"UPDATE Codes SET [State] = @inactive
                WHERE SeedValue = @seedvalue AND [State] = @active";

                command.Parameters.AddWithValue("@inactive", State.Inactive);
                command.Parameters.AddWithValue("@active", State.Active);
                command.Parameters.AddWithValue("@seedvalue", seedValue);
                command.ExecuteNonQuery();
            }
            Connection.Close();
        }
        public void DeactivateCampaign(Campaign campaign)
        {
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"UPDATE Codes SET [State] = @inactive
                WHERE ID BETWEEN @codeIDStart AND @codeIDEnd AND [State] = @active";

                command.Parameters.AddWithValue("@inactive", State.Inactive);
                command.Parameters.AddWithValue("@active", State.Active);
                command.Parameters.AddWithValue("@codeIDStart", campaign.CodeIDStart);
                command.Parameters.AddWithValue("@codeIDEnd", campaign.CodeIDEnd);
                command.ExecuteNonQuery();
            }
            Connection.Close();
        }
        public int CheckIfCodeCanBeRedeemed(string code, string alphabet)
        {

            var codeConverter = new CodeConverter(alphabet);
            var seedValue = codeConverter.ConvertFromCode(code);
            int codeID = 0;
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"UPDATE Codes SET [State] = @redeemed
                OUTPUT INSERTED.ID WHERE SeedValue = @seedvalue AND [State] = @active";

                command.Parameters.AddWithValue("@redeemed", State.Redeemed);
                command.Parameters.AddWithValue("@active", State.Active);
                command.Parameters.AddWithValue("@seedvalue", seedValue);

                codeID = (int)command.ExecuteScalar();
            }
            Connection.Close();
            if (codeID != 0)
            {
                return codeID;
            }
            return -1;
        }
        public int PageCount(int id)
        {
            var pages = 0;
            var Remainder = 0;

            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"SELECT CampaignSize FROM Campaign WHERE ID = id";
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var pageNumber = (int)reader["CampaignSize"];

                        pages = pageNumber / 10;

                        Remainder = pageNumber % 10;

                        if (Remainder > 0)
                        {
                            pages++;
                        }
                    }
                }
            }
            Connection.Close();
            return pages;
        }

        public List<Campaign> GetCampaigns()
        {
            var campaigns = new List<Campaign>();
            Connection.Open();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"SELECT * FROM Campaign";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var campaign = new Campaign
                        {
                            ID = (int)reader["ID"],
                            CampaignName = (string)reader["CampaignName"],
                            CodeIDStart = (int)reader["CodeIDStart"],
                            CodeIDEnd = (int)reader["CodeIDEnd"],
                            CampaignSize = (int)reader["CampaignSize"],
                            DateActive = (DateTime)reader["DateActive"],
                        };
                        campaigns.Add(campaign);
                    }
                }
            }
            Connection.Close();
            return campaigns;
        }

        public List<Code> GetCodes(int campaignID, string alphabet, int pageNumber, int pageSize)
        {
            var codes = new List<Code>();
            Connection.Open();

            var page = Pagination.PaginationPageNumber(pageNumber, pageSize);

            using (var command = Connection.CreateCommand())
            {

                command.CommandText = @"DECLARE @codeIDStart int
                DECLARE @codeIDEnd int
                SET @codeIDStart = (SELECT CodeIDStart FROM Campaign WHERE ID = @campaignID)
                SET @codeIDEnd = (SELECT CodeIDEnd FROM Campaign WHERE ID = @campaignID)

                SELECT * FROM Codes WHERE ID BETWEEN @codeIDStart AND @codeIDEnd
                ORDER BY ID OFFSET @page ROWS FETCH NEXT @pageSize ROWS ONLY";

                command.Parameters.AddWithValue("@page", page);
                command.Parameters.AddWithValue("@pageSize", pageSize);
                command.Parameters.AddWithValue("@CampaignID", campaignID);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var code = new Code();
                        var seedValue = (int)reader["SeedValue"];
                        var codeConverter = new CodeConverter(alphabet);

                        code.State = State.ConvertToString((byte)reader["State"]);
                        code.StringValue = codeConverter.ConvertToCode(seedValue);
                        codes.Add(code);
                    }
                }
            }
            Connection.Close();
            return codes;
        }


    }
}
