CREATE TABLE IF NOT EXISTS `Users` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Username` VARCHAR(32) NOT NULL,
	`HashedPassword` CHAR(64) NOT NULL,
	`Salt` CHAR(64) NOT NULL,
	PRIMARY KEY (`Id`)
) ENGINE=INNODB;

INSERT INTO `Users` (`Username`, `HashedPassword`, `Salt`)
SELECT * FROM (SELECT 'Admin', 'bf8d188e28d76e55e642402b27494bd0a50cc581c0b2bcebbe3b4e0226f894e7', 'JQX685l7dWpu4tCn3TcnVDRHXB5AADLzvTZJeiru1uznSnB95Q99KttIeBrRBLHW') AS `tmp`
WHERE NOT EXISTS (
	SELECT `Id` FROM `Users` WHERE `Username` = 'Admin' AND `Id` = 1
) LIMIT 1;

CREATE TABLE IF NOT EXISTS `Settings` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Key` VARCHAR(64) NOT NULL,
	`Value` VARCHAR(MAX) NOT NULL,
	PRIMARY KEY (`Id`)
) ENGINE=INNODB;

CREATE TABLE IF NOT EXISTS `Playlists` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(64) NOT NULL,
	`Image` CHAR(36) NOT NULL,
	`Enabled` BIT NOT NULL,
	`PlaybackMode` INT NOT NULL,
	`Created` DATETIME NOT NULL,
	`CreatedByUserId` INT NOT NULL,
	`LastUpdated` DATETIME NOT NULL,
	`LastUpdatedByUserId` INT NOT NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

CREATE TABLE IF NOT EXISTS `Media` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(64) NOT NULL,
	`Image` CHAR(36) NOT NULL,
	`File` CHAR(36) NOT NULL,
	`Url` VARCHAR(2083) NOT NULL,
	`Duration` BIGINT NOT NULL,
	`Enabled` BIT NOT NULL,
	`Created` DATETIME NOT NULL,
	`CreatedByUserId` INT NOT NULL,
	`LastUpdated` DATETIME NOT NULL,
	`LastUpdatedByUserId` INT NOT NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

CREATE TABLE IF NOT EXISTS `Content` (
	`Id` CHAR(36) NOT NULL,
	`MimeType` VARCHAR(255)
	`Filename` VARCHAR(2046) NOT NULL,
	`Size` BIGINT NOT NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

CREATE TABLE IF NOT EXISTS `PlaylistEntries` (
    `PlaylistId` INT NOT NULL,
	`MediaId` INT NOT NULL,
	`Enabled` BIT NOT NULL,
	`Order` INT NOT NULL,
	`Added` DATETIME NOT NULL,
	`AddedByUserId` INT NOT NULL,
	PRIMARY KEY(`PlaylistId`, `MediaId`)
) ENGINE=INNODB;

CREATE TABLE IF NOT EXISTS `EventLog` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Timestamp` DATETIME NOT NULL,
	`Source` VARCHAR(64) NOT NULL,
	`Message` VARCHAR(2048) NOT NULL,
	`Data` BLOB(32768) NULL,
	PRIMARY KEY(`Id`)
) ENGINE=INNODB;

SET @x := (SELECT COUNT(*) FROM `information_schema`.`statistics` WHERE `table_name` = 'table' AND `index_name` = 'IX_Table_XYZ' AND `table_schema` = DATABASE());
SET @sql := IF( @x > 0, 'SELECT ''Index Exists.''', 'ALTER TABLE `PlaylistEntries` ADD INDEX `IX_PlaylistEntries_PlaylistId` (`PlaylistId`);');
PREPARE stmt FROM @sql;
EXECUTE stmt;

