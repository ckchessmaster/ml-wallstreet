IF NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[MLWUser]') AND type in (N'U'))

BEGIN
CREATE TABLE [dbo].[MLWUser](
    ID int identity(1, 1) constraint PK_MLWUser primary key,
	UserName varchar(255) not null,
	Email varchar(255) not null,
	IsActive bit not null constraint DF_MLWUser_IsActive default 0 
)

END
