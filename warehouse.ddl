-- Таблица ролей пользователей
CREATE TABLE Roles (
    Id INT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL
);

-- Таблица аккаунтов пользователей (информация для входа)
CREATE TABLE Accounts (
    Mail VARCHAR(50) PRIMARY KEY,
    Phone VARCHAR(20),
    Password VARCHAR(255) NOT NULL,
    Created_date DATE,
    isActive BOOLEAN NOT NULL DEFAULT TRUE,
    UserId VARCHAR(10) UNIQUE  -- Добавляем UserId для связи с Users
);

-- Таблица пользователей (основная информация)
CREATE TABLE Users (
    Id VARCHAR(10) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
    Surname VARCHAR(50) NOT NULL,
    Patronomic VARCHAR(50),
    RoleId INT,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Добавляем внешний ключ в Accounts после создания таблиц
ALTER TABLE Accounts
ADD CONSTRAINT FK_Accounts_Users
FOREIGN KEY (UserId) REFERENCES Users(Id);

-- Таблица категорий товаров
CREATE TABLE Category (
    Id INT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL
);

-- Таблица товаров
CREATE TABLE Products (
    Id VARCHAR(10) PRIMARY KEY,
    Price DECIMAL(10, 2) NOT NULL,
    Name VARCHAR(50) NOT NULL,
    Count INT NOT NULL,
    CategoryId INT,
    FOREIGN KEY (CategoryId) REFERENCES Category(Id)
);

-- Таблица статусов отгрузки
CREATE TABLE Status (
    Id INT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL
);

-- Таблица отгрузок
CREATE TABLE Shipments (
    Id VARCHAR(10) PRIMARY KEY,
    Date DATE NOT NULL,
    StatusId INT,
    UserId VARCHAR(10),  -- Добавляем UserId
    FOREIGN KEY (StatusId) REFERENCES Status(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)  -- Внешний ключ к таблице Users
);

-- Таблица позиций в отгрузке
CREATE TABLE ShipmentItems (
    Id VARCHAR(10) PRIMARY KEY,
    ProductId VARCHAR(10) NOT NULL,
    Count INT NOT NULL,
    ShipmentId VARCHAR(10) NOT NULL,
    FOREIGN KEY (ShipmentId) REFERENCES Shipments(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE Sales (
    Id VARCHAR(10) PRIMARY KEY,
    Date DATE NOT NULL,
    UserId VARCHAR(10),  -- Добавляем UserId
    FOREIGN KEY (UserId) REFERENCES Users(Id)  -- Внешний ключ к таблице Users
);

-- Таблица позиций в продаже
CREATE TABLE SaleItems (
    Id VARCHAR(10) PRIMARY KEY,
    ProductId VARCHAR(10) NOT NULL,
    Count INT NOT NULL,
    SaleId VARCHAR(10) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);