# OnLimit Postgres

- Extensions:

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

- plan_usage_limits Example table

```sql
CREATE TABLE plan_usage_limits(
  "Id" UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  "UserId" VARCHAR(50) NOT NULL,
  "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  "Budget" BIGINT not null default(0)
);
```

- plan_usage_link Example table

```sql
CREATE TABLE plan_usage_link(
  "Id" UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
  "Plan" VARCHAR(64) NOT NULL,
  "UserId" VARCHAR(50) NOT NULL,
  "Date" VARCHAR(10) NOT NULL,
  "ExternalPaymentId" VARCHAR(300),
  "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

- plan_usage_consumition Example table
```sql
CREATE TABLE plan_usage_consumition(
  "Id" SERIAL PRIMARY KEY,
  "UserId" VARCHAR(50) NOT NULL,
  "Date" VARCHAR(10) NOT NULL,
  "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  "Tokens" BIGINT not null default(0),
  "Money" BIGINT not null default(0)
);
```
