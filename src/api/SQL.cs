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

        public CloudBlockBlob CBB { get; set; }

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



        public void CreateCampaign()
        {
            var campaign = HardCodedCampaignData();
            var campaignSize = campaign.CampaignSize;

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
                VALUES (@campiagnName, @codeIDStart, @batchSize)
                SELECT SCOPE_IDENTITY()";
                command.Parameters.AddWithValue("@campaignName", campaign.CampaignName);
                command.Parameters.AddWithValue("@codeIDStart", campaign.CodeIDStart);
                command.Parameters.AddWithValue("@batchSize", campaign.CampaignSize);
                campaign.ID = Convert.ToInt32(command.ExecuteScalar());

                CreateDigitalCode(command, campaignSize, campaign);
            }
            catch (Exception foo)
            {
                transaction.Rollback();
            }
            Connection.Close();
        }

        public void CreateDigitalCode(SqlCommand command, int campaignSize, Campaign campaign)
        {
            //using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open)))
            //{

                var firstLastOffset = UpdateOffset(command, campaignSize);
                if (firstLastOffset[0] % 4 != 0)
                {
                    throw new ArgumentException("Offset must be divisible by 4");
                }
                for (var i = firstLastOffset[0]; i < firstLastOffset[1]; i += 4)
                {

                    //reader.BaseStream.Position = i;
                    var seedvalue = //reader.ReadInt32();

                    command.CommandText = $@"INSERT INTO Codes (SeedValue, State) VALUES (@Seedvalue, @Active)";
                    command.Parameters.AddWithValue("@Seedvalue", seedvalue);
                    command.Parameters.AddWithValue("@Active", State.Active);
                    command.ExecuteNonQuery();
                }
            //}
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

        public void DeactiveCampaign()
        {

        }
    }
}
