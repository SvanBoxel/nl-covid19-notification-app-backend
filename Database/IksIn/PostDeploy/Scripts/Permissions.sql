﻿GRANT CONNECT TO [$(Domain)\$(Appbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Funcbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Ontwikkelaars)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccount)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccountReport)];
GRANT CONNECT TO [$(Domain)\$(Users)];

--subroles
GRANT CONNECT TO [$(Domain)\$(MobileAppAPI)];
GRANT CONNECT TO [$(Domain)\$(IccBackend)];
GRANT CONNECT TO [$(Domain)\$(EksEngine)];
GRANT CONNECT TO [$(Domain)\$(ManifestEngine)];
GRANT CONNECT TO [$(Domain)\$(ContentAPI)];
GRANT CONNECT TO [$(Domain)\$(CleanupJob)];
GRANT CONNECT TO [$(Domain)\$(ManagementPortal)];
GRANT CONNECT TO [$(Domain)\$(DbProvision)];
GRANT CONNECT TO [$(Domain)\$(GenTeks)];
GRANT CONNECT TO [$(Domain)\$(EfgsDownloader)];
GRANT CONNECT TO [$(Domain)\$(EfgsUploader)];

GRANT DELETE  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT EXECUTE ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT INSERT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT UPDATE  ON SCHEMA::[dbo] TO [Dbr_Service];

GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Funcbeheerders];

--specific permissions for DataProtectionKeys
--[Dbr_Service_CleanupJob] 
GRANT INSERT ON [dbo].[IksIn] TO [Dbr_Service_CleanupJob];
GRANT SELECT ON [dbo].[IksIn] TO [Dbr_Service_CleanupJob];
GRANT UPDATE ON [dbo].[IksIn] TO [Dbr_Service_CleanupJob];
GRANT DELETE ON [dbo].[IksIn] TO [Dbr_Service_CleanupJob];

--[Dbr_Service_EfgsDownnloader] 
GRANT INSERT ON [dbo].[IksIn] TO [Dbr_Service_EfgsDownloader];
GRANT SELECT ON [dbo].[IksIn] TO [Dbr_Service_EfgsDownloader];
GRANT UPDATE ON [dbo].[IksIn] TO [Dbr_Service_EfgsDownnloader];

GRANT INSERT ON [dbo].[IksInJob] TO [Dbr_Service_EfgsDownloader];
GRANT SELECT ON [dbo].[IksInJob] TO [Dbr_Service_EfgsDownloader];
GRANT UPDATE ON [dbo].[IksInJob] TO [Dbr_Service_EfgsDownloader];

--end

GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO PUBLIC;
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO PUBLIC;