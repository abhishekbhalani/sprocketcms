IF OBJECT_ID(N'dbo.ForumCategory') IS NULL
BEGIN
	CREATE TABLE dbo.ForumCategory
	(
		ForumCategoryID bigint PRIMARY KEY,
		ClientSpaceID bigint NOT NULL REFERENCES ClientSpaces(ClientSpaceID) ON DELETE CASCADE,
		CategoryCode nvarchar(50) NULL,

		Name nvarchar(250) NOT NULL,
		URLToken nvarchar(50) NULL, -- used for quick access from the address bar e.g. /mysite/[urltoken]/someforum/
		DateCreated datetime NOT NULL,
		Rank int NULL, -- for specifying which categories appear in which order

		InternalUseOnly bit NOT NULL -- allows for the use of categories that will never show up in a public forum list, e.g. for content and blog comments
	)
	CREATE INDEX IX_ForumCategory ON ForumCategory(CategoryCode, URLToken)
END

IF OBJECT_ID(N'dbo.Forum') IS NULL
BEGIN
	CREATE TABLE dbo.Forum
	(
		ForumID bigint PRIMARY KEY,
		ForumCategoryID bigint NOT NULL FOREIGN KEY REFERENCES ForumCategory(ForumCategoryID) ON DELETE CASCADE,
		ForumCode nvarchar(50) NULL,
		
		Name nvarchar(250) NOT NULL,
		URLToken nvarchar(50) NULL, -- used for quick access from the address bar e.g. /mysite/category/[urltoken]/
		DateCreated datetime NOT NULL,
		Rank int NULL, -- for specifying which forums appear in which order
		
		WriteAccess smallint NOT NULL, -- 0: admin only, 1: specified members only, 2: general members only, 3: anonymous posting
		ReadAccess smallint NOT NULL, -- 0: admin only, 1: only users who can post, 2: everybody
		MarkupLevel smallint NOT NULL, -- 0: none, 1: bbcode, 2: limited HTML, 3: full HTML (moderators/admin always have access to full HTML)
		ShowSignatures bit NULL,
		AllowImagesInMessages bit NOT NULL,
		AllowImagesInSignatures bit NOT NULL,
		RequireModeration bit NOT NULL, -- posts must be approved before they are visible
		AllowVoting bit NOT NULL, -- allows a boolean good/bad vote to be applied to topics in this forum
		TopicDisplayOrder smallint NOT NULL, -- 0: topic date, 1: topic last message date, 2: vote weight

		Locked bit NOT NULL -- prevents new posts
	)
	CREATE INDEX IX_Forum ON Forum(ForumCode, URLToken)
END

IF OBJECT_ID(N'dbo.ForumTopic') IS NULL
BEGIN
	CREATE TABLE dbo.ForumTopic
	(
		ForumTopicID bigint PRIMARY KEY,
		ForumID bigint NOT NULL FOREIGN KEY REFERENCES Forum(ForumID) ON DELETE CASCADE,
		AuthorUserID bigint NULL, --FOREIGN KEY REFERENCES Users(UserID) ON DELETE NO ACTION,
		AuthorName nvarchar(100) NULL, -- used if AuthorUserID is null

		Subject nvarchar(500) NOT NULL,
		URLToken nvarchar(200) NULL,
		DateCreated datetime NOT NULL,
		Sticky bit NOT NULL,
		ModerationState smallint NOT NULL, -- 0: unreviewed, 1: approved, 2: flagged for review, 3: spam
		Locked bit NOT NULL -- if locked and sorting by vote weight, the topic will be sorted to the end of the list
	)
	CREATE INDEX IX_ForumTopic ON ForumTopic(DateCreated,AuthorUserID,ForumID)
END

IF OBJECT_ID(N'dbo.ForumTopicMessage') IS NULL
BEGIN
	CREATE TABLE dbo.ForumTopicMessage
	(
		ForumTopicMessageID bigint PRIMARY KEY,
		ForumTopicID bigint NOT NULL FOREIGN KEY REFERENCES ForumTopic(ForumTopicID) ON DELETE CASCADE,
		AuthorUserID bigint NULL,
		AuthorName nvarchar(100) NULL, -- used if AuthorUserID is null
		
		DateCreated datetime NOT NULL,
		Body nvarchar(max) NOT NULL,
		ModerationState smallint NOT NULL, -- 0: unreviewed, 1: approved, 2: flagged for review, 3: spam
		MarkupType smallint NOT NULL -- 0: none, 1: bbcode, 2: limited HTML, 3: full HTML
	)
	CREATE INDEX IX_ForumTopicMessage ON ForumTopicMessage(DateCreated)
END
go

IF OBJECT_ID(N'dbo.OnDeleteForumTopicAuthor') IS NOT NULL
	DROP TRIGGER dbo.OnDeleteForumTopicAuthor
go
CREATE TRIGGER dbo.OnDeleteForumTopicAuthor ON dbo.Users
FOR DELETE
AS
BEGIN
	IF (SELECT COUNT(*) FROM deleted) > 0
	BEGIN
		DELETE FROM ForumTopic
		WHERE AuthorUserID IN (SELECT UserID FROM deleted)
		DELETE FROM ForumTopicMessage
		WHERE AuthorUserID IN (SELECT UserID FROM deleted)
	END
END
go
