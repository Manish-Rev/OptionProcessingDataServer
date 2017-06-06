USE [DataServer]
GO

/****** Object:  Table [dbo].[Users]    Script Date: 07.03.2014 15:03:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[Login] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Login] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [DataServer]
GO

/****** Object:  Table [dbo].[OHLCV_1MinBars]    Script Date: 07.03.2014 15:03:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OHLCV_1MinBars](
	[Timestamp] [int] NOT NULL,
	[Symbol] [nchar](24) NOT NULL,
	[Open] [decimal](18, 5) NOT NULL,
	[High] [decimal](18, 5) NOT NULL,
	[Low] [decimal](18, 5) NOT NULL,
	[Close] [decimal](18, 5) NOT NULL,
	[Volume] [bigint] NOT NULL,
	[Type] [int] NOT NULL,
	[Feed] [nchar](10) NOT NULL,
	[Confirmed] [bit] NOT NULL,
 CONSTRAINT [PK_OHLCV_1MinBars] PRIMARY KEY CLUSTERED 
(
	[Type] ASC,
	[Feed] ASC,
	[Timestamp] ASC,
	[Symbol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [DataServer]
GO

/****** Object:  Table [dbo].[OHLCV_1DayBars]    Script Date: 07.03.2014 15:04:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OHLCV_1DayBars](
	[Timestamp] [int] NOT NULL,
	[Symbol] [nchar](24) NOT NULL,
	[Open] [decimal](18, 5) NOT NULL,
	[High] [decimal](18, 5) NOT NULL,
	[Low] [decimal](18, 5) NOT NULL,
	[Close] [decimal](18, 5) NOT NULL,
	[Volume] [bigint] NOT NULL,
	[Type] [int] NOT NULL,
	[Feed] [nchar](10) NOT NULL,
 CONSTRAINT [PK_OHLCV_1DayBars] PRIMARY KEY CLUSTERED 
(
	[Type] ASC,
	[Feed] ASC,
	[Timestamp] ASC,
	[Symbol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


