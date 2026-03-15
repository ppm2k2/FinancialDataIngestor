/*-- 1. Create the database container
CREATE DATABASE IF NOT EXISTS FundAdminDB
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- 2. Switch to the new schema so subsequent commands apply to it
USE FundAdminDB;

-- 1. Clients Table: Added utf8mb4 for universal character support
CREATE TABLE Clients (
    client_id VARCHAR(50) PRIMARY KEY,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    email VARCHAR(255),
    advisor_id VARCHAR(50),
    last_updated DATETIME
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 2. Accounts Table: Added ON DELETE CASCADE for data cleanup
CREATE TABLE Accounts (
    account_id VARCHAR(50) PRIMARY KEY,
    client_id VARCHAR(50),
    account_type VARCHAR(50),
    custodian VARCHAR(100),
    opened_date DATE,
    status VARCHAR(20),
    cash_balance DECIMAL(18, 4),
    total_value DECIMAL(18, 4),
    FOREIGN KEY (client_id) REFERENCES Clients(client_id) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 3. Holdings Table: Added Indexes for high-frequency lookups
CREATE TABLE Holdings (
    holding_id INT AUTO_INCREMENT PRIMARY KEY,
    account_id VARCHAR(50),
    ticker VARCHAR(20),
    cusip VARCHAR(50),
    description VARCHAR(255),
    quantity DECIMAL(18, 4),
    market_value DECIMAL(18, 4),
    cost_basis DECIMAL(18, 4),
    price DECIMAL(18, 4),
    asset_class VARCHAR(50),
    FOREIGN KEY (account_id) REFERENCES Accounts(account_id) ON DELETE CASCADE,
    INDEX idx_account_id (account_id),
    INDEX idx_ticker (ticker)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; */