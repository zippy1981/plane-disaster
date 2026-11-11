### sqliteodbc: new SQLite file

| Step | Result | Detail |
| --- | --- | --- |
| connect | pass | sqlite3odbc.so 3.46.1 |
| CREATE TABLE | pass |  |
| INSERT with ? parameters | pass | 3 rows |
| GetTables (schema) | pass | 1 tables |
| GetColumnNames (schema) | pass | id, description |
| SELECT without brackets | pass | 3 rows |
| GetTableRowCount ([brackets]) | pass | 3 rows |
| GetTableAsDataTable ([brackets]) | pass | 3 rows |
| GetSQLAsCSV | pass | id,description, |
| SupportsViews | FAIL | NotImplementedException: Currently the OdbcDba.SupportsViews property may only be called when a Microsft Access database is being connected. You are connected with the sqlite3odbc.so driver |
| DROP TABLE | pass |  |

10 passed, 1 failed.
