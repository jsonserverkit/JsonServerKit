USE [master]
GO
CREATE DATABASE [JsonServerKit_YourClient_Demo] ON PRIMARY 
( NAME = N'JsonServerKit_Y_D_D', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL15.ONE\MSSQL\DATA\JsonServerKit_Y_D_D.mdf' , SIZE = 32MB , MAXSIZE = 64MB , FILEGROWTH = 32MB ), 
 FILEGROUP [USER01]  DEFAULT 
( NAME = N'JsonServerKit_Y_D_D_Data1', FILENAME = N'Q:\MSSQL.MSSQLSERVER\MSSQL\Data\JsonServerKit_Y_D_D_Data1.ndf' , SIZE = 1024MB , MAXSIZE = UNLIMITED, FILEGROWTH = 128MB ), 
( NAME = N'JsonServerKit_Y_D_D_Data2', FILENAME = N'R:\MSSQL.MSSQLSERVER\MSSQL\Data\JsonServerKit_Y_D_D_Data2.ndf' , SIZE = 1024MB , MAXSIZE = UNLIMITED, FILEGROWTH = 128MB ), 
( NAME = N'JsonServerKit_Y_D_D_Data3', FILENAME = N'S:\MSSQL.MSSQLSERVER\MSSQL\Data\JsonServerKit_Y_D_D_Data3.ndf' , SIZE = 1024MB , MAXSIZE = UNLIMITED, FILEGROWTH = 128MB ), 
( NAME = N'JsonServerKit_Y_D_D_Data4', FILENAME = N'T:\MSSQL.MSSQLSERVER\MSSQL\Data\JsonServerKit_Y_D_D_Data4.ndf' , SIZE = 1024MB , MAXSIZE = UNLIMITED, FILEGROWTH = 128MB )
 LOG ON 
( NAME = N'log_01', FILENAME = N'L:\MSSQL.MSSQLSERVER\MSSQL\TLog\JsonServerKit_Y_D_D_Log.ldf' , SIZE = 10GB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024MB )
GO

USE [master]
GO
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'IUSR_Anonym_Demo')
CREATE LOGIN [IUSR_Anonym_Demo] WITH PASSWORD=N'o_kjllmuq$237', DEFAULT_DATABASE=[JsonServerKit_YourClient_Demo], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
USE [JsonServerKit_YourClient_Demo]
GO
/****** Object:  User [IUSR_Anonym_PL_Pds]    Script Date: 03/17/2011 13:05:00 ******/
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'IUSR_Anonym_Demo')
CREATE USER [IUSR_Anonym_Demo] FOR LOGIN [IUSR_Anonym_Demo] WITH DEFAULT_SCHEMA=[dbo]
GO
EXEC sp_addrolemember N'db_datawriter', N'IUSR_Anonym_Demo'
GO
EXEC sp_addrolemember N'db_datareader', N'IUSR_Anonym_Demo'
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tabAccount]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tabAccount](
	[lngId] bigint IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_tabAccount] PRIMARY KEY CLUSTERED 
(
	[lngId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [USER01]
) ON [USER01]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tabProduct]') AND type in (N'U'))
BEGIN

CREATE TABLE [dbo].[tabProduct](
	[lngId] bigint IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_tabProduct] PRIMARY KEY CLUSTERED 
(
	[lngId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [USER01]
) ON [USER01]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tabOrder]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tabOrder](
	[lngId] bigint IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_tabOrder] PRIMARY KEY CLUSTERED 
(
	[lngId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [USER01]
) ON [USER01]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tabStatistic]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tabStatistic](
	[lngId] bigint IDENTITY(1,1) NOT NULL,
	[lngMessageId] bigint NOT NULL,
	[timeInMsMessageSent] float,
	[timeInMsMessageReceived] float,
	[timeDiff] float,
	[strMessageType] nvarchar(256)
 CONSTRAINT [PK_tabStatistics] PRIMARY KEY CLUSTERED 
(
	[lngId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [USER01]
) ON [USER01]
END
GO
