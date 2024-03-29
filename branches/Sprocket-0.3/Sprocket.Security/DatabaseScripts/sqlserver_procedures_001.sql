IF EXISTS(SELECT id FROM sysobjects WHERE name='IsValidLogin' AND type='P')
	DROP PROCEDURE IsValidLogin

IF EXISTS(SELECT id FROM sysobjects WHERE name='ListClients' AND type='P')
	DROP PROCEDURE ListClients
IF EXISTS(SELECT id FROM sysobjects WHERE name='CreateClient' AND type='P')
	DROP PROCEDURE CreateClient
IF EXISTS(SELECT id FROM sysobjects WHERE name='UpdateClient' AND type='P')
	DROP PROCEDURE UpdateClient
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeleteClient' AND type='P')
	DROP PROCEDURE DeleteClient

IF EXISTS(SELECT id FROM sysobjects WHERE name='ListUsers' AND type='P')
	DROP PROCEDURE ListUsers
IF EXISTS(SELECT id FROM sysobjects WHERE name='CreateUser' AND type='P')
	DROP PROCEDURE CreateUser
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeleteUser' AND type='P')
	DROP PROCEDURE DeleteUser
IF EXISTS(SELECT id FROM sysobjects WHERE name='UpdateUser' AND type='P')
	DROP PROCEDURE UpdateUser
IF EXISTS(SELECT id FROM sysobjects WHERE name='IsUsernameAvailable' AND type='P')
	DROP PROCEDURE IsUsernameAvailable

IF EXISTS(SELECT id FROM sysobjects WHERE name='CreateRole' AND type='P')
	DROP PROCEDURE CreateRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeleteRole' AND type='P')
	DROP PROCEDURE DeleteRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='UpdateRole' AND type='P')
	DROP PROCEDURE UpdateRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListRoles' AND type='P')
	DROP PROCEDURE ListRoles

IF EXISTS(SELECT id FROM sysobjects WHERE name='RoleInheritsFromRole' AND type='P')
	DROP PROCEDURE RoleInheritsFromRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='InheritRoleFrom' AND type='P')
	DROP PROCEDURE InheritRoleFrom
IF EXISTS(SELECT id FROM sysobjects WHERE name='DisinheritRoleFrom' AND type='P')
	DROP PROCEDURE DisinheritRoleFrom
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListInheritedRoles' AND type='P')
	DROP PROCEDURE ListInheritedRoles

IF EXISTS(SELECT id FROM sysobjects WHERE name='ListUserRoles' AND type='P')
	DROP PROCEDURE ListUserRoles
IF EXISTS(SELECT id FROM sysobjects WHERE name='IsUserInRole' AND type='P')
	DROP PROCEDURE IsUserInRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='AssignUserToRole' AND type='P')
	DROP PROCEDURE AssignUserToRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='UnassignRoleFromUser' AND type='P')
	DROP PROCEDURE UnassignRoleFromUser

IF EXISTS(SELECT id FROM sysobjects WHERE name='CreatePermissionType' AND type='P')
	DROP PROCEDURE CreatePermissionType
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeletePermissionType' AND type='P')
	DROP PROCEDURE DeletePermissionType
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListPermissionTypes' AND type='P')
	DROP PROCEDURE ListPermissionTypes
IF EXISTS(SELECT id FROM sysobjects WHERE name='AssignPermission' AND type='P')
	DROP PROCEDURE AssignPermission
IF EXISTS(SELECT id FROM sysobjects WHERE name='RemovePermission' AND type='P')
	DROP PROCEDURE RemovePermission

IF EXISTS(SELECT id FROM sysobjects WHERE name='UserHasPermission' AND type='P')
	DROP PROCEDURE UserHasPermission
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListPermissionValues' AND type='P')
	DROP PROCEDURE ListPermissionValues

IF EXISTS(SELECT id FROM sysobjects WHERE name='IsEmailAddressAvailable' AND type='P')
	DROP PROCEDURE IsEmailAddressAvailable
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListAccessibleRoles' AND type='P')
	DROP PROCEDURE ListAccessibleRoles
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListRolePermissionStates' AND type='P')
	DROP PROCEDURE ListRolePermissionStates
IF EXISTS(SELECT id FROM sysobjects WHERE name='RemoveRolesAndPermissionsFromUser' AND type='P')
	DROP PROCEDURE RemoveRolesAndPermissionsFromUser
IF EXISTS(SELECT id FROM sysobjects WHERE name='RemoveRolesAndPermissionsFromRole' AND type='P')
	DROP PROCEDURE RemoveRolesAndPermissionsFromRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListPermissionValuesForRole' AND type='P')
	DROP PROCEDURE ListPermissionValuesForRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListRoleToRoleAssignmentStates' AND type='P')
	DROP PROCEDURE ListRoleToRoleAssignmentStates
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListDescendentRoles' AND type='P')
	DROP PROCEDURE ListDescendentRoles

go

CREATE PROCEDURE dbo.IsValidLogin
	@Username		nvarchar(50),
	@PasswordHash	nvarchar(200),
	@IsValid		bit=NULL OUTPUT
AS
BEGIN
	SELECT @IsValid = u.Enabled
	  FROM Users u
 LEFT JOIN Clients c ON u.ClientID = c.ClientID
	 WHERE u.Username = @Username
	   AND u.PasswordHash = @PasswordHash
	   AND u.Enabled = 1
	   AND c.Enabled = 1

	IF @IsValid IS NULL
		SET @IsValid = 0
END

go

CREATE PROCEDURE dbo.ListClients
	@ClientID		uniqueidentifier = null,
	@Name			nvarchar(100) = null,
	@Enabled		bit = null,
	@OwnerClientID	uniqueidentifier = null
AS
BEGIN
	SELECT *
	  FROM Clients
	 WHERE (@ClientID IS NULL OR ClientID = @ClientID)
	   AND (@Name IS NULL OR Name LIKE ('%' + @Name + '%'))
	   AND (@Enabled IS NULL OR Enabled = @Enabled)
	   AND (@OwnerClientID IS NULL OR OwnerClientID = @OwnerClientID)
  ORDER BY Name
END

go

CREATE PROCEDURE dbo.CreateClient
	@ClientID		uniqueidentifier,
	@Name			nvarchar(100),
	@Enabled		bit,
	@PrimaryUserID	uniqueidentifier,
	@OwnerClientID	uniqueidentifier
AS
BEGIN
	INSERT INTO Clients
		(ClientID, Name, Enabled, PrimaryUserID, OwnerClientID)
	VALUES
		(@ClientID, @Name, @Enabled, @PrimaryUserID, @OwnerClientID)
END

go

CREATE PROCEDURE dbo.DeleteClient
	@ClientID	uniqueidentifier
AS
BEGIN
	DELETE FROM UserToRole WHERE RoleID IN (SELECT RoleID FROM Roles WHERE ClientID = @ClientID)
	DELETE FROM RoleToRole WHERE InheritsRoleID IN (SELECT RoleID FROM Roles WHERE ClientID = @ClientID)
	DELETE FROM Permissions WHERE OwnerID IN (SELECT RoleID FROM Roles WHERE ClientID = @ClientID)
	DELETE FROM Permissions WHERE OwnerID IN (SELECT UserID FROM Users WHERE ClientID = @ClientID)
	DELETE FROM Clients WHERE ClientID = @ClientID
	DELETE FROM Roles WHERE ClientID = @ClientID
	DELETE FROM Users WHERE ClientID = @ClientID
END

go

CREATE PROCEDURE dbo.UpdateClient
	@ClientID		uniqueidentifier,
	@Name			nvarchar(100),
	@Enabled		bit,
	@PrimaryUserID	uniqueidentifier,
	@OwnerClientID	uniqueidentifier
AS
BEGIN
	UPDATE Clients SET
		Name = COALESCE(@Name, Name),
		Enabled = COALESCE(@Enabled, Enabled),
		PrimaryUserID = COALESCE(@PrimaryUserID, PrimaryUserID),
		OwnerClientID = COALESCE(@OwnerClientID, OwnerClientID)
	WHERE
		ClientID = @ClientID
END

go

CREATE PROCEDURE dbo.IsUserInRole
	@UserID uniqueidentifier,
	@RoleCode nvarchar(100),
	@IsUserInRole bit=NULL OUTPUT
AS
BEGIN
	DECLARE @RoleID uniqueidentifier
	SELECT @RoleID = RoleID
	  FROM Roles
	 WHERE RoleCode = @RoleCode
	   AND ClientID = (SELECT ClientID
						 FROM Users
						WHERE UserID = @UserID)
	
	CREATE TABLE #r ( RoleID uniqueidentifier )
	INSERT INTO #r (RoleID)
		SELECT DISTINCT RoleID
		  FROM UserToRole
		 WHERE UserID = @UserID
	
	WHILE @@ROWCOUNT > 0
		INSERT INTO #r (RoleID)
			SELECT InheritsRoleID
			  FROM RoleToRole
			 WHERE RoleID IN (SELECT RoleID FROM #r)
			   AND InheritsRoleID NOT IN (SELECT RoleID FROM #r)
	
	DELETE FROM #r
	 WHERE RoleID IN
		(SELECT RoleID FROM Roles WHERE Enabled=0)
	
	IF EXISTS (SELECT RoleID FROM #r WHERE RoleID = @RoleID)
		SET @IsUserInRole = 1
	ELSE
		SET @IsUserInRole = 0
END

go

CREATE PROCEDURE dbo.ListUsers
	@UserID		uniqueidentifier = null,
	@ClientID	uniqueidentifier = null,
	@Username	nvarchar(50) = null,
	@FirstName	nvarchar(50) = null,
	@Surname	nvarchar(50) = null,
	@Email		varchar(100) = null,
	@Enabled	bit = null,
	@Hidden		bit = null,
	@Locked		bit = null,
	@Deleted	bit = null,
	@Activated	bit = null,
	@MaxRecords	int = null,
	@ExactMatches bit = null,
	@EditableByUserID uniqueidentifier = null,
	@TotalMatches int=null OUTPUT
AS
BEGIN
	IF @MaxRecords IS NULL AND @EditableByUserID IS NULL
	BEGIN
		SELECT *
		  FROM Users
		 WHERE	(@UserID IS NULL OR UserID = @UserID)
			AND (@ClientID IS NULL OR ClientID = @ClientID)
			AND (@Username IS NULL OR (Username LIKE '%' + @Username + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @Username = Username )
			AND (@FirstName IS NULL OR (FirstName LIKE '%' + @FirstName + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @FirstName = FirstName)
			AND (@Surname IS NULL OR (Surname LIKE '%' + @Surname + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @Surname = Surname)
			AND (@Email IS NULL OR (Email LIKE '%' + @Email + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @Email = Email)
			AND (@Enabled IS NULL OR Enabled = @Enabled)
			AND (@Hidden IS NULL OR Hidden = @Hidden)
			AND (@Locked IS NULL OR Locked = @Locked)
			AND (@Deleted IS NULL OR Deleted = @Deleted)
			AND (@Activated IS NULL OR Activated = @Activated)
		ORDER BY Username
	END
	ELSE
	BEGIN
		SELECT UserID
		  INTO #ids
		  FROM Users
		 WHERE	(@UserID IS NULL OR UserID = @UserID)
			AND (@ClientID IS NULL OR ClientID = @ClientID)
			AND (@Username IS NULL OR (Username LIKE '%' + @Username + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @Username = Username )
			AND (@FirstName IS NULL OR (FirstName LIKE '%' + @FirstName + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @FirstName = FirstName)
			AND (@Surname IS NULL OR (Surname LIKE '%' + @Surname + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @Surname = Surname)
			AND (@Email IS NULL OR (Email LIKE '%' + @Email + '%' AND (@ExactMatches=0 OR @ExactMatches IS NULL)) OR @Email = Email)
			AND (@Enabled IS NULL OR Enabled = @Enabled)
			AND (@Hidden IS NULL OR Hidden = @Hidden)
			AND (@Locked IS NULL OR Locked = @Locked)
			AND (@Deleted IS NULL OR Deleted = @Deleted)
			AND (@Activated IS NULL OR Activated = @Activated)

		DECLARE @SuperUser bit
		EXEC IsUserInRole @EditableByUserID, 'SUPERUSER', @SuperUser OUTPUT

		IF @EditableByUserID IS NOT NULL AND @SuperUser = 0
		BEGIN
			-- get all roles available to the specified user
			CREATE TABLE #r ( RoleID uniqueidentifier )
			INSERT INTO #r (RoleID)
				SELECT DISTINCT RoleID
				  FROM UserToRole
				 WHERE UserID = @EditableByUserID
			
			WHILE @@ROWCOUNT > 0
				INSERT INTO #r (RoleID)
					SELECT InheritsRoleID
					  FROM RoleToRole
					 WHERE RoleID IN (SELECT RoleID FROM #r)
					   AND InheritsRoleID NOT IN (SELECT RoleID FROM #r)
			
			-- remove the disabled roles from the list
			DELETE FROM #r
				  WHERE RoleID IN (SELECT RoleID
									 FROM Roles
									WHERE Enabled = 0)
			
			-- get all permissions tied to the above roles and to the specified user
			CREATE TABLE #p ( PermissionTypeID uniqueidentifier )
			INSERT INTO #p
				 SELECT PermissionTypeID
				   FROM Permissions
				  WHERE OwnerID = @EditableByUserID
					 OR OwnerID IN (SELECT RoleID FROM #r)

			-- remove all found users that have roles or permissions that the specified user does NOT have
			DELETE FROM #ids
				  WHERE UserID IN (SELECT UserID
									 FROM UserToRole
									WHERE RoleID NOT IN (SELECT RoleID FROM #r))
					 OR UserID IN (SELECT OwnerID
									 FROM Permissions
									WHERE Value = 1
									  AND PermissionTypeID NOT IN (SELECT PermissionTypeID FROM #p))
		END

		IF @MaxRecords IS NOT NULL
		BEGIN
			SELECT @TotalMatches = COUNT(*) FROM #ids
			EXECUTE('SELECT TOP ' + @MaxRecords + ' u.*
					   FROM #ids i
				  LEFT JOIN Users u
						 ON i.UserID = u.UserID
				   ORDER BY u.Username')
		END
		ELSE
		BEGIN
			SELECT u.*
			  FROM #ids i
		 LEFT JOIN Users u
				ON i.UserID = u.UserID
		  ORDER BY u.Username
		END
	END
END

go

CREATE PROCEDURE dbo.CreateUser
	@UserID			uniqueidentifier,
	@ClientID		uniqueidentifier,
	@Username		nvarchar(50),
	@PasswordHash	nvarchar(200),
	@FirstName		nvarchar(50),
	@Surname		nvarchar(50),
	@Email			varchar(100),
	@Enabled		bit,
	@Hidden			bit,
	@Locked			bit,
	@Activated		bit,
	@ActivationReminderSent bit
AS
BEGIN
	INSERT INTO Users
		(UserID, ClientID, Username, PasswordHash, FirstName, Surname, Email, Enabled, Hidden, Locked, Deleted, Activated, ActivationReminderSent, Created)
	VALUES
		(@UserID, @ClientID, @Username, @PasswordHash, @FirstName, @Surname, @Email, @Enabled, @Hidden, @Locked, 0, @Activated, @ActivationReminderSent, getdate())
END

go

CREATE PROCEDURE dbo.DeleteUser
	@UserID uniqueidentifier,
	@DeletePermanently bit
AS
BEGIN
	DELETE FROM Permissions WHERE OwnerID = @UserID
	DELETE FROM UserToRole WHERE UserID = @UserID
	IF @DeletePermanently = 1
		DELETE FROM Users WHERE UserID = @UserID
	ELSE
		UPDATE Users SET Deleted = 1 WHERE UserID = @UserID
END

go

CREATE PROCEDURE dbo.UpdateUser
	@UserID			uniqueidentifier,
	@ClientID		uniqueidentifier,
	@Username		nvarchar(50),
	@PasswordHash	nvarchar(200),
	@FirstName		nvarchar(50),
	@Surname		nvarchar(50),
	@Email			varchar(100),
	@Enabled		bit,
	@Hidden			bit,
	@Locked			bit,
	@Activated		bit
AS
BEGIN
	UPDATE Users SET
		ClientID = COALESCE(@ClientID, ClientID),
		Username = COALESCE(@Username, Username),
		PasswordHash = COALESCE(@PasswordHash, PasswordHash),
		FirstName = COALESCE(@FirstName, FirstName),
		Surname = COALESCE(@Surname, Surname),
		Email = COALESCE(@Email, Email),
		Enabled = COALESCE(@Enabled, Enabled),
		Hidden = COALESCE(@Hidden, Hidden),
		Locked = COALESCE(@Locked, Locked),
		Activated = COALESCE(@Activated, Activated)
	WHERE
		UserID = @UserID
END

go

CREATE PROCEDURE dbo.IsUsernameAvailable
	@ClientID		uniqueidentifier,
	@Username		nvarchar(50),
	@ExcludeUserID	uniqueidentifier=null,
	@Available		bit=null OUTPUT
AS
BEGIN
	IF EXISTS(
		SELECT *
		  FROM Users
		 WHERE Username = @Username
		   AND ClientID = @ClientID
		   AND (@ExcludeUserID IS NULL OR UserID <> @ExcludeUserID) )
		SET @Available = 0
	ELSE
		SET @Available = 1
END

go

CREATE PROCEDURE dbo.IsEmailAddressAvailable
	@ClientID		uniqueidentifier,
	@Email			nvarchar(100),
	@ExcludeUserID	uniqueidentifier=null,
	@Available		bit=null OUTPUT
AS
BEGIN
	IF EXISTS(
		SELECT *
		  FROM Users
		 WHERE Email = @Email
		   AND ClientID = @ClientID
		   AND (UserID <> @ExcludeUserID OR @ExcludeUserID IS NULL))
		SET @Available = 0
	ELSE
		SET @Available = 1
END

go

CREATE PROCEDURE dbo.CreateRole
	@RoleID		uniqueidentifier,
	@RoleCode	nvarchar(100),
	@ClientID	uniqueidentifier,
	@Name		nvarchar(100),
	@Enabled	bit,
	@Locked		bit,
	@Hidden		bit
AS
BEGIN
	INSERT INTO Roles
		(RoleID, RoleCode, ClientID, Name, Enabled, Locked, Hidden)
	VALUES
		(@RoleID, @RoleCode, @ClientID, @Name, @Enabled, @Locked, @Hidden)
END

go

CREATE PROCEDURE dbo.DeleteRole
	@RoleID		uniqueidentifier
AS
BEGIN
	DELETE FROM RoleToRole WHERE InheritsRoleID = @RoleID OR RoleID = @RoleID
	DELETE FROM UserToRole WHERE RoleID = @RoleID
	DELETE FROM Permissions WHERE OwnerID = @RoleID
	DELETE FROM Roles WHERE RoleID = @RoleID
END

go

CREATE PROCEDURE dbo.UpdateRole
	@RoleID		uniqueidentifier,
	@RoleCode	nvarchar(100),
	@ClientID	uniqueidentifier,
	@Name		nvarchar(100),
	@Enabled	bit,
	@Locked		bit,
	@Hidden		bit
AS
BEGIN
	UPDATE Roles SET
		RoleCode = COALESCE(@RoleCode, RoleCode),
		ClientID = COALESCE(@ClientID, ClientID),
		Name = COALESCE(@Name, Name),
		Enabled = COALESCE(@Enabled, Enabled),
		Locked = COALESCE(@Locked, Locked),
		Hidden = COALESCE(@Hidden, Hidden)
	WHERE
		RoleID = @RoleID
END

go

CREATE PROCEDURE dbo.ListRoles
	@RoleID		uniqueidentifier = null,
	@RoleCode	nvarchar(100) = null,
	@ClientID	uniqueidentifier = null,
	@Name		nvarchar(100) = null,
	@Enabled	bit = null,
	@Locked		bit = null,
	@Hidden		bit = null,
	@AccessibleByUserID uniqueidentifier = null
AS
BEGIN
	IF @AccessibleByUserID IS NULL
	BEGIN
		SELECT *
		  FROM Roles
		 WHERE	(@RoleID IS NULL OR RoleID = @RoleID)
			AND (@RoleCode IS NULL OR RoleCode = @RoleCode)
			AND (@ClientID IS NULL OR ClientID = @ClientID)
			AND (@Name IS NULL OR Name = @Name)
			AND (@Enabled IS NULL OR Enabled = @Enabled)
			AND (@Locked IS NULL OR Locked = @Locked)
			AND (@Hidden IS NULL OR Hidden = @Hidden)
		ORDER BY Name
	END
	ELSE
	BEGIN
		CREATE TABLE #r ( RoleID uniqueidentifier )
		INSERT INTO #r (RoleID)
			SELECT DISTINCT RoleID
			  FROM UserToRole
			 WHERE UserID = @AccessibleByUserID
		
		WHILE @@ROWCOUNT > 0
			INSERT INTO #r (RoleID)
				SELECT InheritsRoleID
				  FROM RoleToRole
				 WHERE RoleID IN (SELECT RoleID FROM #r)
				   AND InheritsRoleID NOT IN (SELECT RoleID FROM #r)
		
		SELECT *
		  FROM Roles
		 WHERE	(@RoleID IS NULL OR RoleID = @RoleID)
			AND (@RoleCode IS NULL OR RoleCode = @RoleCode)
			AND (@ClientID IS NULL OR ClientID = @ClientID)
			AND (@Name IS NULL OR Name = @Name)
			AND (@Enabled IS NULL OR Enabled = @Enabled)
			AND (@Locked IS NULL OR Locked = @Locked)
			AND (@Hidden IS NULL OR Hidden = @Hidden)
			AND RoleID IN (SELECT RoleID FROM #r)
		ORDER BY Name
	END
END

go

CREATE PROCEDURE dbo.RoleInheritsFromRole
	@ChildRoleID uniqueidentifier,
	@AncestorRoleID uniqueidentifier,
	@IsAncestor bit=null OUTPUT
AS
BEGIN
	CREATE TABLE #r ( RoleID uniqueidentifier )
	INSERT INTO #r (RoleID)
		SELECT InheritsRoleID
		  FROM RoleToRole
		 WHERE RoleID = @ChildRoleID
	WHILE @@ROWCOUNT > 0
		INSERT INTO #r (RoleID)
			SELECT InheritsRoleID
			  FROM RoleToRole
			 WHERE RoleID IN (SELECT RoleID FROM #r)
			   AND InheritsRoleID NOT IN (SELECT RoleID FROM #r)
	IF EXISTS (SELECT RoleID FROM #r WHERE RoleID = @AncestorRoleID)
		SET @IsAncestor = 1
	ELSE
		SET @IsAncestor = 0
END

go

CREATE PROCEDURE dbo.InheritRoleFrom
	@RoleID			 uniqueidentifier,
	@InheritRoleCode nvarchar(100)
AS
BEGIN
	DECLARE @IsAncestor bit, @InheritRoleID uniqueidentifier
	SELECT @InheritRoleID = RoleID
	  FROM Roles
	 WHERE RoleCode = @InheritRoleCode
	   AND ClientID = (SELECT ClientID
						 FROM Roles
						WHERE RoleID = @RoleID)
	 
	EXEC RoleInheritsFromRole @RoleID, @InheritRoleID, @IsAncestor OUTPUT
	IF @IsAncestor = 0 BEGIN
		EXEC RoleInheritsFromRole @InheritRoleID, @RoleID, @IsAncestor OUTPUT
		IF @IsAncestor = 0
			INSERT INTO RoleToRole (RoleID, InheritsRoleID)
				VALUES (@RoleID, @InheritRoleID)
	END
END

go

CREATE PROCEDURE dbo.DisinheritRoleFrom
	@RoleID				uniqueidentifier,
	@DisinheritRoleID	uniqueidentifier
AS
BEGIN
	DELETE
	  FROM RoleToRole
	 WHERE RoleID = @RoleID
	   AND InheritsRoleID = @DisinheritRoleID
END
  
go

CREATE PROCEDURE dbo.ListInheritedRoles
	@RoleID uniqueidentifier
AS
BEGIN
	SELECT *
	  FROM Roles
	 WHERE RoleID IN
		(SELECT InheritsRoleID
		   FROM RoleToRole
		  WHERE RoleID = @RoleID)
END

go

CREATE PROCEDURE dbo.ListUserRoles
	@UserID uniqueidentifier
AS
BEGIN
	SELECT *
	  FROM Roles
	 WHERE RoleID IN
		(SELECT RoleID
		   FROM UserToRole
		  WHERE UserID = @UserID)
END

go

CREATE PROCEDURE dbo.AssignUserToRole
	@UserID uniqueidentifier,
	@RoleCode nvarchar(100)
AS
BEGIN
	DECLARE @RoleID uniqueidentifier
	SELECT @RoleID = RoleID
	  FROM Roles
	 WHERE RoleCode = @RoleCode
	   AND ClientID = (SELECT ClientID
						 FROM Users
						WHERE UserID = @UserID)
	IF NOT EXISTS
		(SELECT RoleID
		   FROM UserToRole
		  WHERE UserID = @UserID)
		INSERT INTO UserToRole (UserID, RoleID)
			VALUES (@UserID, @RoleID)
END

go

CREATE PROCEDURE dbo.UnassignRoleFromUser
	@UserID uniqueidentifier,
	@RoleCode nvarchar(100)
AS
BEGIN
	DECLARE @RoleID uniqueidentifier
	SELECT @RoleID = RoleID
	  FROM Roles
	 WHERE RoleCode = @RoleCode
	   AND ClientID = (SELECT ClientID
						 FROM Users
						WHERE UserID = @UserID)
	DELETE
	  FROM UserToRole
	 WHERE UserID = @UserID
	   AND RoleID = @RoleID
END

go

CREATE PROCEDURE dbo.CreatePermissionType
	@PermissionTypeID		uniqueidentifier,
	@PermissionTypeCode		varchar(100),
	@Description			nvarchar(1000),
	@DefaultValue			bit,
	@ModuleRegistrationCode	varchar(100)
AS
BEGIN
	IF NOT EXISTS (SELECT *
					 FROM PermissionTypes
					WHERE PermissionTypeID = @PermissionTypeID
					   OR PermissionTypeCode = @PermissionTypeCode)
	INSERT INTO PermissionTypes
		(PermissionTypeID, PermissionTypeCode, Description, DefaultValue, ModuleRegistrationCode)
	VALUES
		(@PermissionTypeID, @PermissionTypeCode, @Description, @DefaultValue, @ModuleRegistrationCode)
END

go

CREATE PROCEDURE dbo.DeletePermissionType
	@PermissionTypeCode	varchar(100),
	@PermissionTypeID	uniqueidentifier=null
AS
BEGIN
	IF @PermissionTypeID IS NULL
		SELECT @PermissionTypeID = PermissionTypeID
		  FROM PermissionTypes
		 WHERE PermissionTypeCode = @PermissionTypeCode

	DELETE FROM Permissions     WHERE PermissionTypeID = @PermissionTypeID
	DELETE FROM PermissionTypes WHERE PermissionTypeID = @PermissionTypeID
END

go

CREATE PROCEDURE dbo.ListPermissionTypes
	@PermissionTypeID		uniqueidentifier,
	@PermissionTypeCode		varchar(100),
	@Description			nvarchar(1000),
	@DefaultValue			bit,
	@ModuleRegistrationCode	varchar(100)
AS
BEGIN
	SELECT *
	  FROM PermissionTypes
	 WHERE (@PermissionTypeID IS NULL OR PermissionTypeID = @PermissionTypeID)
	   AND (@PermissionTypeCode IS NULL OR PermissionTypeCode = @PermissionTypeCode)
	   AND (@Description IS NULL OR Description LIKE '%' + @Description + '%')
	   AND (@DefaultValue IS NULL OR DefaultValue = @DefaultValue)
	   AND (@ModuleRegistrationCode IS NULL OR ModuleRegistrationCode = @ModuleRegistrationCode)
  ORDER BY ModuleRegistrationCode, PermissionTypeCode
END

go

CREATE PROCEDURE dbo.AssignPermission
	@PermissionTypeCode	varchar(100),
	@PermissionTypeID	uniqueidentifier,
	@OwnerID			uniqueidentifier
AS
BEGIN
	IF @PermissionTypeID IS NULL
		SELECT @PermissionTypeID = PermissionTypeID
		  FROM PermissionTypes
		 WHERE PermissionTypeCode = @PermissionTypeCode
	DELETE
	  FROM Permissions
	 WHERE OwnerID = @OwnerID
	   AND PermissionTypeID = @PermissionTypeID
	
	INSERT INTO Permissions
		(PermissionTypeID, OwnerID, Value)
	VALUES
		(@PermissionTypeID, @OwnerID, 1)
END

go

CREATE PROCEDURE dbo.RemovePermission
	@PermissionTypeCode	varchar(100),
	@PermissionTypeID	uniqueidentifier,
	@OwnerID			uniqueidentifier
AS
BEGIN
	IF @PermissionTypeID IS NULL
		SELECT @PermissionTypeID = PermissionTypeID
		  FROM PermissionTypes
		 WHERE PermissionTypeCode = @PermissionTypeCode
	DELETE
	  FROM Permissions
	 WHERE OwnerID = @OwnerID
	   AND PermissionTypeID = @PermissionTypeID
END

go

CREATE PROCEDURE dbo.UserHasPermission
	@UserID				uniqueidentifier,
	@PermissionTypeCode	varchar(100),
	@HasPermission		bit=null OUTPUT
AS
BEGIN
	CREATE TABLE #r ( RoleID uniqueidentifier )
	INSERT INTO #r (RoleID)
		SELECT DISTINCT RoleID
		  FROM UserToRole
		 WHERE UserID = @UserID
	
	WHILE @@ROWCOUNT > 0
		INSERT INTO #r (RoleID)
			SELECT InheritsRoleID
			  FROM RoleToRole
			 WHERE RoleID IN (SELECT RoleID FROM #r)
			   AND InheritsRoleID NOT IN (SELECT RoleID FROM #r)
	
	DELETE FROM #r
	 WHERE RoleID IN
		(SELECT RoleID FROM Roles WHERE Enabled=0)
	
	IF EXISTS (SELECT Value
				 FROM Permissions
				WHERE PermissionTypeID = (SELECT PermissionTypeID
											FROM PermissionTypes
										   WHERE PermissionTypeCode = @PermissionTypeCode)
				  AND (OwnerID = @UserID
				   OR OwnerID IN (SELECT RoleID FROM #r)))
		SET @HasPermission = 1
	ELSE
		SET @HasPermission = 0
END

go

CREATE PROCEDURE dbo.ListPermissionValues
	@UserID					uniqueidentifier
AS
BEGIN
	CREATE TABLE #r ( RoleID uniqueidentifier )
	INSERT INTO #r (RoleID)
		SELECT DISTINCT RoleID
		  FROM UserToRole
		 WHERE UserID = @UserID
	
	WHILE @@ROWCOUNT > 0
		INSERT INTO #r (RoleID)
			SELECT InheritsRoleID
			  FROM RoleToRole
			 WHERE RoleID IN (SELECT RoleID FROM #r)
			   AND InheritsRoleID NOT IN (SELECT RoleID FROM #r)

	DELETE FROM #r
	 WHERE RoleID IN
		(SELECT RoleID FROM Roles WHERE Enabled=0)
	
	SELECT PermissionTypeCode,
		   CAST(CASE WHEN COUNT(Value) > 0 THEN 1 ELSE 0 END AS bit) AS [HasPermission],
		   CAST(CASE WHEN p.OwnerID = @UserID OR COUNT(Value) = 0 THEN 0 ELSE 1 END AS bit) AS [Inherited]
	  FROM PermissionTypes pt
 LEFT JOIN Permissions p
		ON pt.PermissionTypeID = p.PermissionTypeID
	   AND (p.OwnerID = @UserID
			OR p.OwnerID IN (SELECT RoleID FROM #r))
  GROUP BY PermissionTypeCode, p.OwnerID
END

go

CREATE PROCEDURE dbo.ListAccessibleRoles
	@UserID	uniqueidentifier
AS
BEGIN
	DECLARE @SuperUser bit
	EXEC IsUserInRole @UserID, 'SUPERUSER', @SuperUser OUTPUT
	IF @SuperUser = 1
	BEGIN
		SELECT *, CAST(0 AS bit) AS [Inherited]
		  FROM Roles
	  ORDER BY Name
	END
	ELSE
	BEGIN
		CREATE TABLE #r ( RoleID uniqueidentifier, Inherited bit )
		INSERT INTO #r (RoleID, Inherited)
			SELECT DISTINCT RoleID, 0
			  FROM UserToRole
			 WHERE UserID = @UserID
		
		WHILE @@ROWCOUNT > 0
			INSERT INTO #r (RoleID, Inherited)
				SELECT InheritsRoleID, 1
				  FROM RoleToRole
				 WHERE RoleID IN (SELECT RoleID FROM #r)
				   AND InheritsRoleID NOT IN (SELECT RoleID FROM #r)

		DELETE FROM #r
		 WHERE RoleID IN
			(SELECT RoleID FROM Roles WHERE Enabled=0)

		SELECT r.*, Inherited
		  FROM #r
	 LEFT JOIN Roles r
			ON #r.RoleID = r.RoleID
	  ORDER BY r.Name
	END
END

go

CREATE PROCEDURE dbo.ListRolePermissionStates
	@UserID uniqueidentifier
AS
BEGIN
	SELECT r.*, CAST(CASE WHEN ur.RoleID IS NOT NULL THEN 1 ELSE 0 END AS bit) AS HasRole
	  FROM Roles r
 LEFT JOIN UserToRole ur
		ON r.RoleID = ur.RoleID
	   AND ur.UserID = @UserID
	 WHERE r.Enabled = 1
  ORDER BY r.Name

	SELECT pt.*, CAST(CASE p.Value WHEN 1 THEN 1 ELSE 0 END AS bit) as HasPermission
	  FROM PermissionTypes pt
 LEFT JOIN Permissions p
		ON p.PermissionTypeID = pt.PermissionTypeID
	   AND p.Value = 1
	   AND p.OwnerID = @UserID
  ORDER BY pt.ModuleRegistrationCode
END

go

CREATE PROCEDURE dbo.RemoveRolesAndPermissionsFromUser
	@UserID uniqueidentifier
AS
BEGIN
	DELETE FROM UserToRole WHERE UserID = @UserID
	DELETE FROM Permissions WHERE OwnerID = @UserID
END

go

CREATE PROCEDURE dbo.RemoveRolesAndPermissionsFromRole
	@RoleID uniqueidentifier
AS
BEGIN
	DELETE FROM RoleToRole WHERE RoleID = @RoleID
	DELETE FROM Permissions WHERE OwnerID = @RoleID
END

go

CREATE PROCEDURE dbo.ListPermissionValuesForRole
	@RoleID uniqueidentifier,
	@ShowAllPermissions bit=0
AS
BEGIN
	SELECT *
	  FROM PermissionTypes pt
 LEFT JOIN Permissions p
		ON p.PermissionTypeID = pt.PermissionTypeID
	   AND p.OwnerID = @RoleID
	 WHERE (@ShowAllPermissions = 1 OR (p.OwnerID = @RoleID AND p.Value = 1))
  ORDER BY pt.ModuleRegistrationCode
END

go

CREATE PROCEDURE dbo.ListRoleToRoleAssignmentStates
	@RoleID uniqueidentifier
AS
BEGIN
	SELECT *, CAST(CASE WHEN rr.InheritsRoleID IS NULL THEN 0 ELSE 1 END AS bit) as [Inherited]
	  FROM Roles r
 LEFT JOIN RoleToRole rr
		ON r.RoleID = rr.InheritsRoleID
	   AND rr.RoleID = @RoleID
	 WHERE r.RoleID <> @RoleID
  ORDER BY r.Name
END

go

CREATE PROCEDURE dbo.ListDescendentRoles
	@RoleID uniqueidentifier
AS
BEGIN
	CREATE TABLE #r ( RoleID uniqueidentifier )
	INSERT INTO #r (RoleID)
		SELECT DISTINCT RoleID
		  FROM RoleToRole
		 WHERE InheritsRoleID = @RoleID
	
	WHILE @@ROWCOUNT > 0
		INSERT INTO #r (RoleID)
			SELECT RoleID
			  FROM RoleToRole
			 WHERE InheritsRoleID IN (SELECT RoleID FROM #r)
			   AND RoleID NOT IN (SELECT RoleID FROM #r)
	
	SELECT *
	  FROM Roles
	 WHERE RoleID IN (SELECT RoleID FROM #r)
  ORDER BY Name
END
