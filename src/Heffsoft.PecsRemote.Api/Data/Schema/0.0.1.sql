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
	`Data` BLOB NULL,
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
	`Content` BLOB NOT NULL,
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
       ('database:lastmodified', NOW()),
	   ('thermal:power:min', '0.2'),
	   ('thermal:power:start', '1.0'),
	   ('thermal:control:pin', '13'),
	   ('thermal:control:frequency', '25'),
	   ('thermal:control:updatetime', '10000'),
	   ('thermal:control:sensor', '/sys/class/thermal/thermal_zone0/temp'),
	   ('thermal:control:curves:0:temp', '0.0'),
	   ('thermal:control:curves:0:power', '0.2'),
	   ('thermal:control:curves:1:temp', '20.0'),
	   ('thermal:control:curves:1:power', '0.2'),
	   ('thermal:control:curves:2:temp', '70.0'),
	   ('thermal:control:curves:2:power', '1.0'),
	   ('thermal:control:curves:3:temp', '100.0'),
	   ('thermal:control:curves:3:power', '1.0'),
	   ('display:backlight:idle', '600000'),
	   ('display:backlight:max', '1.0'),
	   ('display:backlight:min', '0.0'),
	   ('display:control:led:pin', '21'),
	   ('display:control:reset:pin', '20'),
	   ('display:control:reset:time', '120'),
	   ('display:control:dc:pin', '16'),
	   ('display:control:select:pin:0', '4'),
	   ('display:control:select:pin:1', '5'),
	   ('display:control:select:pin:2', '6'),
	   ('display:control:spi:bus', '0'),
	   ('display:control:spi:cs', '0'),
	   ('display:control:spi:frequency', '20000000'),
	   ('display:0:enabled', 'true'),
	   ('display:0:channel', '1'),
	   ('display:1:enabled', 'true'),
	   ('display:1:channel', '2'),
	   ('display:2:enabled', 'true'),
	   ('display:2:channel', '3'),
	   ('display:3:enabled', 'true'),
	   ('display:3:channel', '4'),
	   ('display:4:enabled', 'true'),
	   ('display:4:channel', '5'),
	   ('display:5:enabled', 'true'),
	   ('display:5:channel', '6'),
	   ('display:6:enabled', 'false'),
	   ('display:6:channel', '0'),
	   ('display:7:enabled', 'false'),
	   ('display:7:channel', '0'),
	   ('content:thumbnails:path', '/var/pecsremote/thumbs/'),
	   ('content:thumbnails:format', 'jpg'),
	   ('content:thumbnails:quality', '0.8'),
	   ('content:thumbnails:width', '1280'),
	   ('content:thumbnails:height', '720'),
	   ('content:media:path', '/var/pecsremote/media/'),
	   ('host:files:hostname', '/etc/hostname'),
	   ('host:files:hosts', '/etc/hosts'),
	   ('host:files:wlan', '/etc/network/interfaces.d/wlan0'),
	   ('host:files:wlanmac', '/etc/network/interfaces.d/wlan0'),
	   ('host:files:cpuinfo', '/proc/cpuinfo'),
	   ('host:files:uptime', '/proc/uptime'),
	   ('host:folders:dev', '/dev/'),
	   ('host:folders:language', '/home/pecsremote/adminapp/language'),
	   ('host:commands:checkupdates', '/usr/lib/update-notifier/apt-check'),
	   ('host:commands:reboot', 'reboot'),
	   ('host:commands:scanwifi', 'sudo iwlist wlan0 scan'),
	   ('host:commands:blacklist', '/sbin/iptables -A INPUT -s {ip} -j DROP'),
	   ('host:commands:blacklist6', '/sbin/ip6tables -A INPUT -s {ip} -j DROP'),
	   ('host:commands:unblacklist', '/sbin/iptables -D INPUT -s {ip} -j DROP'),
	   ('host:commands:unblacklist6', '/sbin/ip6tables -D INPUT -s {ip} -j DROP'),
	   ('host:commands:strength', '/sbin/iwconfig wlan0'),
	   ('host:commands:update', 'apt-get update && apt-get -y upgrade && apt-get -y dist-upgrade && apt-get -y autoremove && fstrim / && reboot'),
	   ('notifications:max', '1000'),
	   ('user:security:bantimeout', '1800000'),
	   ('user:security:passwordattempts', '3'),
	   ('user:security:passwordattemptwindow', '600000')
	   ;


INSERT INTO `Users` (`Username`, `HashedPassword`, `Salt`)
VALUES ('Admin', 'bf8d188e28d76e55e642402b27494bd0a50cc581c0b2bcebbe3b4e0226f894e7', 'JQX685l7dWpu4tCn3TcnVDRHXB5AADLzvTZJeiru1uznSnB95Q99KttIeBrRBLHW');

INSERT INTO `EventLog` (`Timestamp`, `Source`, `Message`)
VALUES (NOW(), 'System', 'Database Initialised');

SET FOREIGN_KEY_CHECKS = 1;

