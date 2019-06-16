﻿CREATE TABLE IF NOT EXISTS `Users` (
	`Id` INT NOT NULL AUTO_INCREMENT,
	`Username` VARCHAR(32) NOT NULL,
	`HashedPassword` CHAR(64) NOT NULL,
	`Salt` CHAR(64) NOT NULL,
	`Roles` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`Id`)
) ENGINE=INNODB;

INSERT INTO `Users` (`Username`, `HashedPassword`, `Salt`, `Roles`)
SELECT * FROM (SELECT 'Admin', 'bf8d188e28d76e55e642402b27494bd0a50cc581c0b2bcebbe3b4e0226f894e7', 'JQX685l7dWpu4tCn3TcnVDRHXB5AADLzvTZJeiru1uznSnB95Q99KttIeBrRBLHW', 'Admin') AS `tmp`;
WHERE NOT EXISTS (
	SELECT `Id` FROM `Users` WHERE `Username` = 'Admin' AND `Id` = 1
) LIMIT 1;