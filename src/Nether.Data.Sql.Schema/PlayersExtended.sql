﻿/* Copyright (c) Microsoft. All rights reserved.
Licensed under the MIT license. See LICENSE file in the project root for full license information. */

CREATE TABLE [dbo].[PlayersExtended]
(
	[Gamertag] VARCHAR(50) NOT NULL,
    [State] NVARCHAR(MAX) NULL,
	PRIMARY KEY ([Gamertag]),
    CONSTRAINT [AK_PlayersExtended_PlayerId] UNIQUE ([Gamertag])
)
GO

CREATE INDEX [IX_PlayersExtended_Gamertag] ON [dbo].[PlayersExtended] ([Gamertag])

GO
