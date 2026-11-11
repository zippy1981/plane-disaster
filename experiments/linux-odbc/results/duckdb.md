### duckdb: native file

| Step | Result | Detail |
| --- | --- | --- |
| connect | pass | DuckDB v2.0.0 |
| CREATE TABLE | pass |  |
| INSERT with ? parameters | pass | 3 rows |
| GetTables (schema) | pass | 1 tables |
| GetColumnNames (schema) | pass | id, description |
| SELECT without brackets | pass | 3 rows |
| GetTableRowCount ([brackets]) | FAIL | OdbcException: ERROR [42000] ODBC_DuckDB->SQLExecDirect {"exception_type":"Parser","exception_message":"syntax error at or near \"[\"\n\nLINE 1: SELECT COUNT(*) FROM [tblTest]\n                             ^","error... |
| GetTableAsDataTable ([brackets]) | FAIL | OdbcException: ERROR [42000] ODBC_DuckDB->SQLExecDirect {"exception_type":"Parser","exception_message":"syntax error at or near \"[\"\n\nLINE 1: SELECT * FROM [tblTest]\n                      ^","error_subtype":"SYN... |
| GetSQLAsCSV | pass | id,description, |
| SupportsViews | FAIL | NotImplementedException: Currently the OdbcDba.SupportsViews property may only be called when a Microsft Access database is being connected. You are connected with the DuckDB driver |
| DROP TABLE | pass |  |

8 passed, 3 failed.

### duckdb: SQLite file via the sqlite extension

| Step | Result | Detail |
| --- | --- | --- |
| connect | pass | DuckDB v2.0.0 |
| setup: INSTALL sqlite | pass |  |
| setup: LOAD sqlite | pass |  |
| setup: ATTACH '/data/fixture.sqlite' AS fixture (TYPE sqlite) | pass |  |
| setup: USE fixture | pass |  |
| GetTables (schema) | pass | 1 tables |
| GetColumnNames (schema) | pass | id, description |
| SELECT without brackets | pass | 3 rows |
| GetTableRowCount ([brackets]) | FAIL | OdbcException: ERROR [42000] ODBC_DuckDB->SQLExecDirect {"exception_type":"Parser","exception_message":"syntax error at or near \"[\"\n\nLINE 1: SELECT COUNT(*) FROM [tblTest]\n                             ^","error... |
| GetTableAsDataTable ([brackets]) | FAIL | OdbcException: ERROR [42000] ODBC_DuckDB->SQLExecDirect {"exception_type":"Parser","exception_message":"syntax error at or near \"[\"\n\nLINE 1: SELECT * FROM [tblTest]\n                      ^","error_subtype":"SYN... |
| GetSQLAsCSV | pass | id,description, |
| SupportsViews | FAIL | NotImplementedException: Currently the OdbcDba.SupportsViews property may only be called when a Microsft Access database is being connected. You are connected with the DuckDB driver |

9 passed, 3 failed.
