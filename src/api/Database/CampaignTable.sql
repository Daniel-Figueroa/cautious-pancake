USE ExamDatabase
GO
CREATE TABLE Campaign(
    ID int  PRIMARY KEY IDENTITY(1,1) NOT NULL,
    CampaignName varchar(50) NOT NULL,
    CodeIDStart int UNIQUE NOT NULL,
    CodeIDEnd AS CodeIDStart + CampaginSize -1,
    CampaignSize int NOT NULL,
    DateActive DATETIME)