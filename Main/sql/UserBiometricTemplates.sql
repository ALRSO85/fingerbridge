CREATE TABLE dbo.UserBiometricTemplates
(
    UserBiometricTemplateId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    FingerCode TINYINT NULL,

    -- Provider que gerou o template. Ex.: futronic.fs80h, nitgen.hamsterdx.
    ProviderKey NVARCHAR(80) NOT NULL,

    -- Modelo amigavel do dispositivo. Ex.: FS80H, Hamster DX.
    DeviceModel NVARCHAR(120) NULL,

    -- Formato do template. Ex.: Futronic-SDK-4.2, Nitgen-NBioBSP-TextFIR.
    TemplateFormat NVARCHAR(80) NOT NULL,

    -- Template gerado pelo SDK. Nao grave imagem da digital.
    Template VARBINARY(MAX) NOT NULL,

    Quality INT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_UserBiometricTemplates_IsActive DEFAULT (1),
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_UserBiometricTemplates_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NULL,
    RevokedAt DATETIME2(0) NULL,
    RevokedBy NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_UserBiometricTemplates_User_Active
ON dbo.UserBiometricTemplates(UserId, IsActive)
INCLUDE (ProviderKey, TemplateFormat, DeviceModel, Quality);
GO
