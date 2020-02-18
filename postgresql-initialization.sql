-- DROP SCHEMA simpleauth CASCADE;

SET search_path TO simpleauth;

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

-- DROP TABLE "PermissionGroups";
CREATE TABLE "PermissionGroups" (
    "Id" uuid NOT NULL CONSTRAINT "PK_PermissionGroups" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Locked" BOOL NOT NULL,
    "Corp" TEXT NOT NULL,
    "App" TEXT NOT NULL
);

CREATE TABLE "Roles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Roles" PRIMARY KEY,
    "Corp" TEXT NOT NULL,
    "App" TEXT NOT NULL,
    "Env" TEXT NOT NULL,
    "Tenant" TEXT NOT NULL,
    "Module" TEXT NOT NULL,
    "SubModules" TEXT NULL,
    "Locked" BOOL NOT NULL
);

CREATE TABLE "TokenInfos" (
    "Id" UUID NOT NULL CONSTRAINT "PK_TokenInfos" PRIMARY KEY,
    "Corp" TEXT NOT NULL,
    "App" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE TABLE "Users" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
    "NormalizedId" TEXT NOT NULL
);

-- DROP TABLE "PermissionRecords";
CREATE TABLE "PermissionRecords" (
    "Id" UUID NOT NULL CONSTRAINT "PK_PermissionRecords" PRIMARY KEY,
    "RoleId" TEXT NOT NULL,
    "Verb" INTEGER NOT NULL,
    "PermissionGroupId" uuid NULL,
    "Env" TEXT NOT NULL,
    "Tenant" TEXT NOT NULL,
    CONSTRAINT "FK_PermissionRecords_PermissionGroups_PermissionGroupId" FOREIGN KEY ("PermissionGroupId") REFERENCES "PermissionGroups" ("Id") ON DELETE RESTRICT
);

-- DROP TABLE "LocalUserInfo";
CREATE TABLE "LocalUserInfos" (
    "Id" UUID NOT NULL CONSTRAINT "PK_LocalUserInfos" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "Corp" TEXT NOT NULL,
    "EncryptedPassword" TEXT NULL,
    "Locked" BOOL NOT NULL,
    CONSTRAINT "FK_LocalUserInfos_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- DROP TABLE "PermissionGroupUsers";
CREATE TABLE "PermissionGroupUsers" (
    "UserId" TEXT NOT NULL,
    "PermissionGroupId" UUID NOT NULL,
    CONSTRAINT "PK_PermissionGroupUsers" PRIMARY KEY ("UserId", "PermissionGroupId"),
    CONSTRAINT "FK_PermissionGroupUsers_PermissionGroups_PermissionGroupId" FOREIGN KEY ("PermissionGroupId") REFERENCES "PermissionGroups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PermissionGroupUsers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_LocalUserInfos_Corp" ON "LocalUserInfos" ("Corp");

CREATE INDEX "IX_LocalUserInfos_Email" ON "LocalUserInfos" ("Email");

CREATE INDEX "IX_LocalUserInfos_Locked" ON "LocalUserInfos" ("Locked");

CREATE INDEX "IX_LocalUserInfos_NormalizedEmail" ON "LocalUserInfos" ("NormalizedEmail");

CREATE INDEX "IX_LocalUserInfos_UserId" ON "LocalUserInfos" ("UserId");

CREATE UNIQUE INDEX "IX_LocalUserInfos_NormalizedEmail_Corp" ON "LocalUserInfos" ("NormalizedEmail", "Corp");

CREATE UNIQUE INDEX "IX_LocalUserInfos_UserId_Corp" ON "LocalUserInfos" ("UserId", "Corp");

CREATE INDEX "IX_PermissionGroups_App" ON "PermissionGroups" ("App");

CREATE INDEX "IX_PermissionGroups_Corp" ON "PermissionGroups" ("Corp");

CREATE INDEX "IX_PermissionGroups_Locked" ON "PermissionGroups" ("Locked");

CREATE INDEX "IX_PermissionGroups_Name" ON "PermissionGroups" ("Name");

CREATE UNIQUE INDEX "IX_PermissionGroups_Name_Corp_App" ON "PermissionGroups" ("Name", "Corp", "App");

CREATE INDEX "IX_PermissionGroupUsers_PermissionGroupId" ON "PermissionGroupUsers" ("PermissionGroupId");

CREATE INDEX "IX_PermissionGroupUsers_UserId" ON "PermissionGroupUsers" ("UserId");

CREATE INDEX "IX_PermissionRecords_Permission" ON "PermissionRecords" ("Verb");

CREATE INDEX "IX_PermissionRecords_PermissionGroupId" ON "PermissionRecords" ("PermissionGroupId");

CREATE INDEX "IX_PermissionRecords_RoleId" ON "PermissionRecords" ("RoleId");

CREATE INDEX "IX_Roles_App" ON "Roles" ("App");

CREATE INDEX "IX_Roles_Corp" ON "Roles" ("Corp");

CREATE INDEX "IX_Roles_Env" ON "Roles" ("Env");

CREATE INDEX "IX_Roles_Locked" ON "Roles" ("Locked");

CREATE INDEX "IX_Roles_Module" ON "Roles" ("Module");

CREATE INDEX "IX_Roles_SubModules" ON "Roles" ("SubModules");

CREATE INDEX "IX_Roles_Tenant" ON "Roles" ("Tenant");

CREATE INDEX "IX_TokenInfos_App" ON "TokenInfos" ("App");

CREATE INDEX "IX_TokenInfos_Corp" ON "TokenInfos" ("Corp");

CREATE UNIQUE INDEX "IX_TokenInfos_Corp_App" ON "TokenInfos" ("Corp", "App");

CREATE UNIQUE INDEX "IX_Users_NormalizedId" ON "Users" ("NormalizedId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200102170316_InitialCreate', '3.0.1');
