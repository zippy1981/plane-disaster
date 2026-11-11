# Linux ODBC experiments

Can PlaneDisaster.NET's data layer reach Access and SQLite databases on Linux through ODBC? These experiments run `PlaneDisaster.Dba`'s `OdbcDba`, compiled for plain `net11.0`, against five ODBC drivers under unixODBC, each in its own container.

## Results

Run on 2026-11-10 in WSL 2 (Ubuntu 26.04) Docker, with the `mcr.microsoft.com/dotnet/runtime:11.0` image (Ubuntu 26.04, unixODBC 2.3.14). Full output is in [`results/`](results/).

| Driver | Licence | Database | Read | Write | What breaks |
| --- | --- | --- | --- | --- | --- |
| [sqliteodbc](http://www.ch-werner.de/sqliteodbc/) 0.99991 (Christian Werner) | BSD | SQLite | yes | yes | Only `SupportsViews`, which throws for any non-Access driver |
| [DuckDB ODBC](https://github.com/duckdb/duckdb-odbc) 2.0.0.1 | MIT | DuckDB, and SQLite through the `sqlite` extension | yes | yes | `[bracket]` identifiers, which `dba` puts around every table name; `SupportsViews` |
| [MDB Tools](https://github.com/mdbtools/mdbtools) 1.0.1 | LGPL/GPL | Access `.mdb` and `.accdb` | partly | no | Lacks `SQLSetStmtAttr`, `SQLMoreResults` and `SQLTablesW`, so only `GetTableAsDataTable` works; no schema calls, no readers |
| [Easysoft ODBC-Access](https://www.easysoft.com/products/data_access/odbc-access-driver/) | commercial | Access | not run | not run | The download needs an Easysoft account and a trial licence |
| [CData Access ODBC](https://www.cdata.com/drivers/access/odbc/) | commercial | Access | not run | not run | The download needs a CData account and a trial key |

Every driver that ran passed connect, `CREATE TABLE` and `INSERT` with `?` parameters where it can write.

## What this says about PlaneDisaster on Linux

- **SQLite works today** through sqliteodbc with no code changes, and through System.Data.SQLite directly (the Mono CI job).
- **There is no free read-write Access ODBC driver for Linux.** MDB Tools reads, but too little of the ODBC API for System.Data.Odbc. The commercial drivers are the only option short of the Jackcess experiment (the `experiment/jackcess` branch).
- **Two portability fixes in `dba` would widen what works:**
  - Quote identifiers with the connection's own quote character (`DbCommandBuilder.QuoteIdentifier`, or `SQLGetInfo(SQL_IDENTIFIER_QUOTE_CHAR)`) instead of hard-coding `[...]`. DuckDB would then pass everything.
  - Make `OdbcDba.SupportsViews` and `SupportsProcedures` return false, or ask the driver, instead of throwing for drivers other than Access.
- `OdbcConnection.Driver` is empty for MDB Tools' wide build, and its version string is garbled; the ANSI build reports `1.0.1`.

## Running them

On a host whose Docker reaches a Linux engine (on Windows, run it inside WSL; see the `host-capabilities` skill):

```sh
experiments/linux-odbc/run.sh                        # every driver
experiments/linux-odbc/run.sh sqliteodbc duckdb      # some
```

`run.sh` builds one image per driver from [`Dockerfile`](Dockerfile): the harness, unixODBC, and what `drivers/<driver>/install.sh` installs. It then runs `drivers/<driver>/run.sh`, which calls the harness with that driver's connection string, and writes `results/<driver>.md`. Images are tagged `plane-disaster-odbc:<driver>` and labelled `com.zippy1981.created-by=claude-code`; remove them with the `safe-docker-cleanup` skill.

For the commercial drivers, put the vendor's Linux package in `drivers/easysoft/vendor/` or `drivers/cdata/vendor/` (see each `install.sh`) and rerun. Their connection-string attributes come from the vendors' documentation and haven't been tried.

## The harness

[`harness/`](harness/) compiles `PlaneDisaster.Dba/*.cs` for `net11.0` and runs, through `OdbcDba`: connect, `CREATE TABLE`, three `INSERT`s with `?` parameters, `GetTables`, `GetColumnNames`, a plain `SELECT`, `GetTableRowCount` and `GetTableAsDataTable` (both of which bracket the table name), `GetSQLAsCSV`, `SupportsViews` and `DROP TABLE`. Each step is reported pass or fail, and every step runs even when an earlier one failed.
