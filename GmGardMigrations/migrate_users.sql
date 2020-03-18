USE [MyMVCWebUser]
GO

Update [dbo].[UserProfile] set
           [EmailConfirmed] = 'true'
           ,[LockoutEnabled] = 'true'
           ,[NormalizedEmail] = UPPER(u.Email)
           ,[NormalizedUserName] = UPPER(u.UserName)
           ,[PasswordHash] = m.[Password]
           ,[SecurityStamp] = NEWID()
           ,[TwoFactorEnabled] = 'false'
FROM UserProfile as u
LEFT OUTER JOIN webpages_Membership as m ON m.UserId = u.UserId
GO

SET IDENTITY_INSERT [dbo].[AspNetRoles] on

INSERT INTO [dbo].[AspNetRoles]
           ([Id]
           ,[ConcurrencyStamp]
           ,[Name]
           ,[NormalizedName])
select RoleId, null, RoleName, upper(RoleName)
from webpages_Roles
GO


INSERT INTO [dbo].[AspNetUserRoles]
           (UserId, RoleId)
select UserId, RoleId
from webpages_UsersInRoles
GO

SET IDENTITY_INSERT [dbo].[AspNetRoles] off
GO