SET FOREIGN_KEY_CHECKS = 0;

--
-- Clean out existing database
--
DROP TABLE IF EXISTS `Settings`;
DROP TABLE IF EXISTS `EventLog`;
DROP TABLE IF EXISTS `Users`;
DROP TABLE IF EXISTS `Media`;
DROP TABLE IF EXISTS `BannedAddresses`;
DROP TABLE IF EXISTS `Notifications`;
DROP TABLE IF EXISTS `Content`;

--
-- Create Tables
--
CREATE TABLE `Settings` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Key` VARCHAR(64) NOT NULL,
	`Value` VARCHAR(2048) NOT NULL,
	PRIMARY KEY (`Id`)
) ENGINE=INNODB;

CREATE TABLE `EventLog` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Timestamp` DATETIME NOT NULL,
	`Source` VARCHAR(64) NOT NULL,
	`Message` VARCHAR(2048) NOT NULL,
	`Data` BLOB(32768) NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

CREATE TABLE `Users` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Username` VARCHAR(32) NOT NULL,
	`HashedPassword` CHAR(64) NOT NULL,
	`Salt` CHAR(64) NOT NULL,
	PRIMARY KEY (`Id`)
) ENGINE=INNODB;

CREATE TABLE `Media` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(64) NOT NULL,
	`Image` CHAR(36) NOT NULL,
	`File` CHAR(36) NOT NULL,
	`Url` VARCHAR(2083) NOT NULL,
	`Duration` TIME NOT NULL,
	`Enabled` BIT NOT NULL,
	`ViewCount` INT NOT NULL,
	`Created` DATETIME NOT NULL,
	`CreatedByUserId` INT NOT NULL,
	`LastUpdated` DATETIME NOT NULL,
	`LastUpdatedByUserId` INT NOT NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

CREATE TABLE `BannedAddresses` (
	`Id` CHAR(36) NOT NULL,
	`LastBanned` DATETIME NOT NULL,
	`UnbanAt` DATETIME NULL,
	`BanCount` BIGINT NOT NULL,
	`IPAddress` VARCHAR(45) NOT NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

CREATE TABLE `Notifications` (
	`Id` CHAR(36) NOT NULL,
	`Timestamp` DATETIME NOT NULL,
	`Type` INT NOT NULL,
	`Title` VARCHAR(128) NOT NULL,
	`Image` VARCHAR(2083) NOT NULL,
	`Content` VARCHAR(16384) NOT NULL,
	`Read` BIT NOT NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

CREATE TABLE `Content` (
	`Id` CHAR(36) NOT NULL,
	`MimeType` VARCHAR(255),
	`Filename` VARCHAR(2046) NOT NULL,
	`Size` BIGINT NOT NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

--
-- Insert Base Data
--
INSERT INTO `Settings` (`Key`, `Value`)
VALUES ('database:version', '0.0.1'),
       ('database:lastmodified', NOW());

INSERT INTO `Users` (`Username`, `HashedPassword`, `Salt`)
VALUES ('Admin', 'bf8d188e28d76e55e642402b27494bd0a50cc581c0b2bcebbe3b4e0226f894e7', 'JQX685l7dWpu4tCn3TcnVDRHXB5AADLzvTZJeiru1uznSnB95Q99KttIeBrRBLHW');

INSERT INTO `EventLog` (`Timestamp`, `Source`, `Message`)
VALUES (NOW(), 'System', 'Database Initialised');

SET FOREIGN_KEY_CHECKS = 1;


