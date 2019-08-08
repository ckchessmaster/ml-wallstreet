IF NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[PreviouslyRunScripts]') AND type in (N'U'))

BEGIN
CREATE TABLE [dbo].[PreviouslyRunScripts](
    ID int identity(1, 1) constraint PK_PreviouslyRunScripts primary key,
	ScriptName varchar(255) not null,
	RunDate datetime not null
)

END
