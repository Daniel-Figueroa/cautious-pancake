using System;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace CodeFlip.CodeJar.Api
{
    public class SQL
    {
        public SqlConnection Connection { get; set; }
        public SQL(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
        }

        public void CreateCampaign(List<Code> codes, Campaign campaign)
        {
            Connection.Open();

            var command = Connection.CreateCommand();
            var transaction = Connection.BeginTransaction();
            command.Transaction = transaction;

            try
            {
                command.CommandText = @"
                DECLARE @codeIDStart int
                SET @codeIDStart = (SELECT ISNULL(MAX(CodeIDEnd), 0)FROM Campaign) + 1
                INSERT INTO Campaign (CampaignName, CampaignSize, CodeIDStart)
                VALUES (@campaignName,  @campaignSize, @codeIDStart)
                SELECT SCOPE_IDENTITY()
                ";
                command.Parameters.AddWithValue("@campaignName", campaign.CampaignName);
                command.Parameters.AddWithValue("@campaignSize", campaign.CampaignSize);
                campaign.ID = Convert.ToInt32(command.ExecuteScalar());

                CreateDigitalCode(codes, command);
                transaction.Commit();
            }
            catch (Exception foo)
            {
                transaction.Rollback();
            }
            Connection.Close();
        }

        public void CreateDigitalCode(List<Code> codes, SqlCommand command)
        {
            foreach (var code in codes)
            {
                command.CommandText = @"
                INSERT INTO Codes (State, SeedValue)
                VALUES (@state, @seedValue)";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@state", State.Active);
                command.Parameters.AddWithValue("@seedValue", code.SeedValue);
                command.ExecuteNonQuery();
            }
        }
        public long[] UpdateOffset(int campaignSize)
        {
            var firstLastOffset = new long[2];
            var incrementedOffset = campaignSize * 4;
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                UPDATE Offset SET OffsetValue = OffsetValue + @incrementedOffset
                OUTPUT INSERTED.OffsetValue WHERE ID = 1";
                command.Parameters.AddWithValue("@incrementedOffset", incrementedOffset);
                var updatedOffset = (long)command.ExecuteScalar();


                firstLastOffset[0] = updatedOffset - incrementedOffset;
                firstLastOffset[1] = updatedOffset;
            }
            Connection.Close();

            return firstLastOffset;
        }

        public void DeactivateCode(string alphabet, string code)
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
        public bool DeactivateCampaign(int id)
        {
            int codes;
            Connection.Open();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                DECLARE @codeIDStart int
                DECLARE @codeIDEnd int
                SET @codeIDStart = (SELECT CodeIDStart FROM Campaign WHERE ID = @id)
                SET @codeIDEnd = (SELECT CodeIDEnd FROM Campaign WHERE ID = @id)
                UPDATE Codes SET [State] = @inactive
                WHERE ID BETWEEN @codeIDStart AND @codeIDEnd
                AND [State] = @active";
                command.Parameters.AddWithValue("@active", State.Active);
                command.Parameters.AddWithValue("@inactive", State.Inactive);
                command.Parameters.AddWithValue("@id", id);
                codes = command.ExecuteNonQuery();
            }

            Connection.Close();
            if (codes >= 1)
            {
                return true;
            }
            return false;
        }
        public int CheckIfCodeCanBeRedeemed(int seedValue, string code)
        {
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

        public List<Campaign> GetAllCampaigns()
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
                        };
                        campaigns.Add(campaign);
                    }
                }
            }
            Connection.Close();
            return campaigns;
        }
        public Campaign GetCampaignByID(int id)
        {
            var campaign = new Campaign();
            Connection.Open();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"SELECT ID, CampaignName, CampaignSize FROM Campaign WHERE ID = @id";
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        campaign.ID = (int)reader["ID"];
                        campaign.CampaignName = (string)reader["CampaignName"];
                        campaign.CampaignSize = (int)reader["CampaignSize"];
                    }
                }
            }
            Connection.Close();
            return campaign;
        }
        public Code GetCode(string stringValue, CodeConverter codeConverter)
        {
            var seedValue = codeConverter.ConvertFromCode(stringValue);
            var code = new Code();

            Connection.Open();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"SELECT * FROM Codes WHERE SeedValue = @seedValue";
                command.Parameters.AddWithValue("@seedValue", seedValue);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var seed = (int)reader["SeedValue"];
                        code.State = State.ConvertToString((byte)reader["State"]);
                        code.StringValue = codeConverter.ConvertToCode(seed);
                    }
                }
            }
            Connection.Close();

            return code;
        }

        public List<Code> GetCodes(int campaignID, string alphabet, int pageNumber, int pageSize)
        {
            var codes = new List<Code>();
            var page = Pagination.PaginationPageNumber(pageNumber, pageSize);
            var codeConverter = new CodeConverter(alphabet);


            Connection.Open();


            using (var command = Connection.CreateCommand())
            {

                command.CommandText = @"
                DECLARE @codeIDStart int
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
