-- BankSystemPro database schema.
-- Run this once against a SQL Server instance to create the schema manually,
-- OR skip it entirely and let EF Core create the database for you via migrations
-- (see README: "Option B - EF Core Migrations").

IF DB_ID('BankSystemProDb') IS NULL
BEGIN
    CREATE DATABASE BankSystemProDb;
END
GO

USE BankSystemProDb;
GO

CREATE TABLE Banks (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(150) NOT NULL,
    Country     NVARCHAR(100) NOT NULL,
    SwiftCode   NVARCHAR(20)  NOT NULL UNIQUE
);
GO

CREATE TABLE Branches (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Name     NVARCHAR(150) NOT NULL,
    Address  NVARCHAR(250) NOT NULL,
    BankId   INT NOT NULL FOREIGN KEY REFERENCES Banks(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Users (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    FullName      NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(500) NOT NULL,
    Role          NVARCHAR(20)  NOT NULL, -- Admin | Employee | Customer
    BranchId      INT NULL FOREIGN KEY REFERENCES Branches(Id) ON DELETE SET NULL,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE Customers (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    NationalId   NVARCHAR(50) NOT NULL UNIQUE,
    PhoneNumber  NVARCHAR(30) NOT NULL,
    Address      NVARCHAR(250) NOT NULL
);
GO

CREATE TABLE Accounts (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    AccountNumber  NVARCHAR(20) NOT NULL UNIQUE,
    Type           NVARCHAR(20) NOT NULL, -- Checking | Savings
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active | Frozen | Closed
    Balance        DECIMAL(18,2) NOT NULL DEFAULT 0,
    CustomerId     INT NOT NULL FOREIGN KEY REFERENCES Customers(Id),
    BranchId       INT NOT NULL FOREIGN KEY REFERENCES Branches(Id),
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVersion     ROWVERSION -- optimistic concurrency token, mirrors Account.RowVersion in EF Core
);
GO

CREATE TABLE Transactions (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    AccountId         INT NOT NULL FOREIGN KEY REFERENCES Accounts(Id) ON DELETE CASCADE,
    Type              NVARCHAR(20) NOT NULL, -- Deposit | Withdrawal | TransferIn | TransferOut
    Amount            DECIMAL(18,2) NOT NULL,
    BalanceAfter      DECIMAL(18,2) NOT NULL,
    Description       NVARCHAR(250) NULL,
    RelatedAccountId  INT NULL,
    CreatedAt         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE Loans (
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId         INT NOT NULL FOREIGN KEY REFERENCES Customers(Id),
    Amount             DECIMAL(18,2) NOT NULL,
    InterestRate       DECIMAL(5,2)  NOT NULL,
    TermMonths         INT NOT NULL,
    Status             NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending | Approved | Rejected | Closed
    ReviewedByUserId   INT NULL FOREIGN KEY REFERENCES Users(Id),
    RejectionReason    NVARCHAR(250) NULL,
    RequestedAt        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ReviewedAt         DATETIME2 NULL
);
GO

-- ============================================================
-- Sample stored procedure: branch-level account statistics.
-- Demonstrates raw T-SQL / stored-procedure skills alongside the EF Core code,
-- since interviewers often ask about both.
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.GetBranchAccountSummary
    @BranchId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Id                                AS BranchId,
        b.Name                              AS BranchName,
        COUNT(a.Id)                         AS TotalAccounts,
        SUM(CASE WHEN a.Status = 'Active' THEN 1 ELSE 0 END) AS ActiveAccounts,
        ISNULL(SUM(a.Balance), 0)           AS TotalDeposits,
        ISNULL(AVG(a.Balance), 0)           AS AverageBalance
    FROM Branches b
    LEFT JOIN Accounts a ON a.BranchId = b.Id
    WHERE b.Id = @BranchId
    GROUP BY b.Id, b.Name;
END
GO

-- ============================================================
-- Sample seed data for local testing.
-- ============================================================
INSERT INTO Banks (Name, Country, SwiftCode) VALUES ('Meridian National Bank', 'USA', 'MRDNUS33');
INSERT INTO Branches (Name, Address, BankId) VALUES ('Downtown Branch', '100 Main St, Springfield', 1);
GO
