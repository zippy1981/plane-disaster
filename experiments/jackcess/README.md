# Jackcess compiled to .NET

Can PlaneDisaster.NET read and write Access files on Linux without Microsoft's drivers? [Jackcess](https://jackcess.sourceforge.io/) is a pure Java library for the Access file format, and [UCanAccess](https://ucanaccess.sourceforge.net/) puts SQL on top of it with HSQLDB. [IKVM](https://github.com/ikvmnet/ikvm) compiles both to .NET assemblies. This experiment checks that they work from C# on Linux, and what it would take to turn them into our own ODBC driver.

## Result: it works

Run on 2026-11-10 in WSL 2 Docker, `mcr.microsoft.com/dotnet/runtime:10.0`, IKVM 8.16.3, UCanAccess 5.0.1 (with the Jackcess and HSQLDB it was built with). Every step passed:

| Step | Detail |
| --- | --- |
| Jackcess: create `new.accdb` (Access 2010) with `tblTest` and 3 rows | 360,448 bytes |
| Jackcess: read it back | `V2010`, 3 rows |
| UCanAccess: `CREATE TABLE`, 3 `INSERT`s with `?` parameters, a `JOIN` with `COUNT` and `MAX` | 3 joined rows |
| Jackcess: UCanAccess's table is in the file | `[tblSql, tblTest]` |
| The same four steps on `new.mdb` (Access 2002-2003) | all passed |
| Jackcess: read Northwind, written by Access 97 | 9 tables, 91 customers |
| UCanAccess: `TOP 1 ... GROUP BY ... ORDER BY` on Northwind | Save-a-lot Markets, 31 orders |

MDB Tools, which shares no code with Jackcess, reads the files back: `new.mdb` as JET4 and `new.accdb` as ACE14, with both tables and the inserted rows, quotes and semicolons intact.

To run it, from the repository root on a host with a Linux Docker engine (the `host-capabilities` skill):

```sh
docker build -f experiments/jackcess/Dockerfile -t plane-disaster-jackcess:probe \
  --label com.zippy1981.created-by=claude-code .
docker run --rm plane-disaster-jackcess:probe
```

## What it took

- **Java 8 releases.** IKVM implements Java SE 8. Jackcess 5 and UCanAccess 5.1+ target Java 11, so the probe uses UCanAccess 5.0.1 (`net.sf.ucanaccess`), the last Java 8 release, which brings Jackcess 3.0.1 and HSQLDB 2.5.0.
- **No Java at build or run time.** `IKVM.Maven.Sdk` downloads the jars from Maven Central and compiles them during `dotnet build`. The published app is 337 MB, almost all of it IKVM's Java class library and runtime image; a publish for one runtime identifier should be smaller, which this experiment didn't try.
- **.NET 10, not 11.** `IKVM.Maven.Sdk` 1.12.2's resolve task crashes under the .NET 11 SDK's MSBuild (`MSB4217: Task host node exited prematurely`). With the .NET 10 SDK it builds. The assemblies should still run on .NET 11; only the build is affected.
- The build warns about optional classes it can't find (log4j, Apache POI, servlets). Jackcess uses POI only for OLE attachments.

## Our own ODBC driver

PlaneDisaster talks to Access through `OleDba` and `OdbcDba`. On Linux there is no OLE DB, and no free ODBC driver both reads and writes Access (the `experiment/linux-odbc` branch). An ODBC driver built on UCanAccess would fill that gap for PlaneDisaster and for every other ODBC client.

### How it would be built

```text
unixODBC ──► libplanedisasterodbc.so  (C, ~1,500 lines)
               exports SQLAllocHandle, SQLDriverConnectW, SQLExecDirectW, SQLFetch, SQLGetData, ...
               hosts .NET with nethost/hostfxr: load_assembly_and_get_function_pointer
                 ──► PlaneDisaster.Odbc.dll  ([UnmanagedCallersOnly] entry points, handle tables, diagnostics)
                       ──► UCanAccess JDBC (IKVM) ──► Jackcess + HSQLDB ──► .mdb / .accdb
```

- **The C shim is required.** NativeAOT could export the ODBC functions from C# directly, but IKVM needs runtime code generation, which NativeAOT doesn't allow. So a small C library hosts the .NET runtime and forwards each call.
- **The managed side** maps ODBC handles to JDBC objects: environment, connection (`java.sql.Connection`), statement (`PreparedStatement` and `ResultSet`), and descriptors. It converts SQL types, and turns Java exceptions into `SQLGetDiagRec` records.
- **What System.Data.Odbc needs.** The MDB Tools trace shows it calls `SQLSetStmtAttrW`, `SQLMoreResults`, `SQLTablesW`, `SQLColAttributeW` and many `SQLGetInfoW` types. PlaneDisaster's schema calls add `SQLColumns` and `SQLProcedures`. That is roughly 40 functions for a driver PlaneDisaster and `isql` can use.

### Estimate and risks

| Part | Size | Risk |
| --- | --- | --- |
| C shim: exports, runtime hosting, wide strings | ~1,500 lines | Low: mechanical |
| Managed driver: handles, types, fetch, diagnostics | ~3,000 lines | Medium: ODBC's state machine and its many `SQLGetInfo` answers |
| Catalog functions over Jackcess metadata | ~800 lines | Low |
| Packaging: runtime plus IKVM (337 MB as published here) per platform | n/a | Medium: big, and the driver would start a .NET runtime inside every ODBC client |

UCanAccess's own limits carry over: it loads tables into HSQLDB in memory (`memory=false` puts them in temp files), so the first query on a large database is slow, and writes are serialized.

### A cheaper first step

PlaneDisaster itself doesn't need ODBC to use UCanAccess. A `UCanAccessDba : dba`, over a small `DbConnection` adapter for `java.sql.Connection`, would give the Linux build of PlaneDisaster read-write Access support in about 600 lines, with no native code. Build the ODBC driver only if other ODBC clients need it too.
