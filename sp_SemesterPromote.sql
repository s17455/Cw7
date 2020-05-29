CREATE PROCEDURE sp_SemesterPromote(
    @id_study AS INT
    ,@semester AS INT
)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

    DECLARE @current_id_enrollment INT=(
        SELECT IdEnrollment
        FROM Enrollment
        WHERE IdStudy = @id_study AND Semester = @semester
    );
    IF @current_id_enrollment IS NULL
    BEGIN
        ROLLBACK;
        RETURN;
    END;
    DECLARE @next_semester INT = (@semester + 1)
    DECLARE @next_id_enrollment INT = (
        SELECT IdEnrollment
        FROM Enrollment
        WHERE IdStudy = @id_study AND Semester = @next_semester
    );
    IF @next_id_enrollment IS NULL
    BEGIN
        SET @next_id_enrollment = (
            SELECT MAX(IdEnrollment)
            FROM Enrollment
        ) + 1;
        INSERT INTO 
            Enrollment
        VALUES(
            @next_id_enrollment, 
            @semester + 1,
            @id_study,
            GETDATE());
    END;
    UPDATE
        Student
    SET
        IdEnrollment = @next_id_enrollment
    WHERE
        IdEnrollment = @current_id_enrollment
  	SELECT * FROM Enrollment WHERE IdEnrollment = @next_id_enrollment
    COMMIT;
END;