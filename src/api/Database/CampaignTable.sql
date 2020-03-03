USE ExamDatabase
GO
CREATE TABLE Campaign(
    ID int  PRIMARY KEY IDENTITY(1,1) NOT NULL,
    CampaignName varchar(50) NOT NULL,
    CampaignSize int NOT NULL,
    CodeIDStart int UNIQUE NOT NULL,
    CodeIDEnd AS CodeIDStart + CampaignSize -1
)
GO