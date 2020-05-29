

CREATE TABLE s17455.dbo.Student (
	IndexNumber nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	FirstName nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	LastName nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	BirthDate date NOT NULL,
	IdEnrollment int NOT NULL,
	Password nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	Salt nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	CONSTRAINT Student_pk PRIMARY KEY (IndexNumber)
) GO;


ALTER TABLE s17455.dbo.Student ADD CONSTRAINT Student_Enrollment FOREIGN KEY (IdEnrollment) REFERENCES s17455.dbo.Enrollment(IdEnrollment) GO;