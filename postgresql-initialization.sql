-- DROP SCHEMA simpleauth CASCADE;

SET search_path TO simpleauth;

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

-- DROP TABLE "RoleGroups";
CREATE TABLE "RoleGroups" (
    "Id" uuid NOT NULL CONSTRAINT "PK_RoleGroups" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Locked" INTEGER NOT NULL,
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
    "Locked" INTEGER NOT NULL
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

-- DROP TABLE "RoleRecords";
CREATE TABLE "RoleRecords" (
    "Id" UUID NOT NULL CONSTRAINT "PK_RoleRecords" PRIMARY KEY,
    "RoleId" TEXT NOT NULL,
    "Permission" INTEGER NOT NULL,
    "RoleGroupId" uuid NULL,
    CONSTRAINT "FK_RoleRecords_RoleGroups_RoleGroupId" FOREIGN KEY ("RoleGroupId") REFERENCES "RoleGroups" ("Id") ON DELETE RESTRICT
);

-- DROP TABLE "LocalUserInfo";
CREATE TABLE "LocalUserInfos" (
    "Id" UUID NOT NULL CONSTRAINT "PK_LocalUserInfos" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "Corp" TEXT NOT NULL,
    "EncryptedPassword" TEXT NULL,
    "Locked" INTEGER NOT NULL,
    CONSTRAINT "FK_LocalUserInfos_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- DROP TABLE "RoleGroupUsers";
CREATE TABLE "RoleGroupUsers" (
    "UserId" TEXT NOT NULL,
    "RoleGroupId" UUID NOT NULL,
    CONSTRAINT "PK_RoleGroupUsers" PRIMARY KEY ("UserId", "RoleGroupId"),
    CONSTRAINT "FK_RoleGroupUsers_RoleGroups_RoleGroupId" FOREIGN KEY ("RoleGroupId") REFERENCES "RoleGroups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RoleGroupUsers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_LocalUserInfos_Corp" ON "LocalUserInfos" ("Corp");

CREATE INDEX "IX_LocalUserInfos_Email" ON "LocalUserInfos" ("Email");

CREATE INDEX "IX_LocalUserInfos_Locked" ON "LocalUserInfos" ("Locked");

CREATE INDEX "IX_LocalUserInfos_NormalizedEmail" ON "LocalUserInfos" ("NormalizedEmail");

CREATE INDEX "IX_LocalUserInfos_UserId" ON "LocalUserInfos" ("UserId");

CREATE UNIQUE INDEX "IX_LocalUserInfos_NormalizedEmail_Corp" ON "LocalUserInfos" ("NormalizedEmail", "Corp");

CREATE UNIQUE INDEX "IX_LocalUserInfos_UserId_Corp" ON "LocalUserInfos" ("UserId", "Corp");

CREATE INDEX "IX_RoleGroups_App" ON "RoleGroups" ("App");

CREATE INDEX "IX_RoleGroups_Corp" ON "RoleGroups" ("Corp");

CREATE INDEX "IX_RoleGroups_Locked" ON "RoleGroups" ("Locked");

CREATE INDEX "IX_RoleGroups_Name" ON "RoleGroups" ("Name");

CREATE UNIQUE INDEX "IX_RoleGroups_Name_Corp_App" ON "RoleGroups" ("Name", "Corp", "App");

CREATE INDEX "IX_RoleGroupUsers_RoleGroupId" ON "RoleGroupUsers" ("RoleGroupId");

CREATE INDEX "IX_RoleGroupUsers_UserId" ON "RoleGroupUsers" ("UserId");

CREATE INDEX "IX_RoleRecords_Permission" ON "RoleRecords" ("Permission");

CREATE INDEX "IX_RoleRecords_RoleGroupId" ON "RoleRecords" ("RoleGroupId");

CREATE INDEX "IX_RoleRecords_RoleId" ON "RoleRecords" ("RoleId");

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
