IF (NOT EXISTS (SELECT 1
			FROM INFORMATION_SCHEMA.TABLES 
			WHERE TABLE_NAME = 'NewsArticle'))
BEGIN
    CREATE TABLE NewsArticle (
		NewsArticleID UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_NewsArticle PRIMARY KEY,
		Url varchar(255) NOT NULL,
		Date DATETIME NOT NULL,
		Title nvarchar(255),
		RawText nvarchar(MAX),
		CleanText nvarchar(MAX)
	)
END
