CREATE TABLE s17455.dbo.RefreshToken (
	Id nvarchar(36) COLLATE Polish_CI_AS NOT NULL,
	IndexNumber nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	CONSTRAINT RefreshToken_pk PRIMARY KEY (Id)
) GO;


ALTER TABLE s17455.dbo.RefreshToken ADD CONSTRAINT RefreshToken_Student FOREIGN KEY (IndexNumber) REFERENCES s17455.dbo.Student(IndexNumber) GO;