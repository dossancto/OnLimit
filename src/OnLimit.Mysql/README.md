# OnLimit Mysql

- plan_usage_limits Example table

```sql
CREATE TABLE plan_usage_limits(
  `Id` CHAR(36) NOT NULL PRIMARY KEY DEFAULT (UUID()),
  `UserId` VARCHAR(50) NOT NULL,
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  `Budget` BIGINT NOT NULL DEFAULT 0
);
```

- plan_usage_link Example table

```sql
CREATE TABLE plan_usage_link(
  `Id` CHAR(36) NOT NULL PRIMARY KEY DEFAULT (UUID()),
  `Plan` VARCHAR(64) NOT NULL,
  `UserId` VARCHAR(50) NOT NULL,
  `Date` VARCHAR(10) NOT NULL,
  `ExternalPaymentId` VARCHAR(300),
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

- plan_usage_consumition Example table
```sql
CREATE TABLE plan_usage_consumition(
  `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `UserId` VARCHAR(50) NOT NULL,
  `Date` VARCHAR(10) NOT NULL,
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  `Tokens` BIGINT NOT NULL DEFAULT 0,
  `Money` BIGINT NOT NULL DEFAULT 0
);
```

