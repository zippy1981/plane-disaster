# Test data

Access databases that other people published, for the smoke tests in `TestAccessSampleFiles.cs`. The files are unmodified apart from their names. They were downloaded on 2026-11-11.

Add a file here only if it is in the public domain or its licence allows redistribution, and record its source, licence and SHA-256 below. Keep files small: these three add about 9 MB to a checkout.

| File | Format | Source | Licence |
| --- | --- | --- | --- |
| `census-pl2000-access97.mdb` | Access 97 (Jet 3) | U.S. Census Bureau | Public domain |
| `nist-albatross.mdb` | Access 2000 (Jet 4) | National Institute of Standards and Technology | NIST open licence |
| `nist-albatross.accdb` | Access 2007 and later (ACE) | National Institute of Standards and Technology | NIST open licence |

## census-pl2000-access97.mdb

The file structure for the Census 2000 Redistricting Data (Public Law 94-171) Summary File: two lookup tables with rows, and three empty tables of up to 149 columns to import the data into.

- Source: <https://www2.census.gov/census_2000/datasets/redistricting_file--pl_94-171/0File_Structure/Access97/PL2000.mdb>
- Licence: a work of the United States government, in the public domain in the United States (17 U.S.C. 105).
- SHA-256: `3b4eb4d42cc4258a2cde9a81cb8df31d6343f7fafc71a0e2d6d4eb32e8d21898`

The Access 2000 version beside it on the Census site is not used: its tables are protected by user-level security, so they can't be read without the workgroup file.

## nist-albatross.mdb and nist-albatross.accdb

The Albatross Species Chemical Database and Annotated Bibliography: contaminant measurements in albatross tissues collected from published papers. It has 21 tables, saved queries, forms and reports. NIST publishes the same database in both formats.

- Source: National Institute of Standards and Technology (2020), Albatross Species Chemical Database and Annotated Bibliography, <https://doi.org/10.18434/mds2-2304> (accessed 2026-11-11).
- Licence: <https://www.nist.gov/open/license>. NIST asks that redistributed copies acknowledge NIST as the source of the data and carry its statement: the data is not subject to copyright protection in the United States under 17 U.S.C. 105, and is provided "AS IS", without warranty of any kind.
- SHA-256 of `nist-albatross.mdb`: `cdeddd13cebed5e543a1745def59dee10fe5bc019a95019d9f20143352a6b26a`
- SHA-256 of `nist-albatross.accdb`: `afa5d700521fd2f30908ea9c968affa8684943d37ecbe408729d6de9cc14f556`

Both hashes match the `.sha256` files NIST publishes beside the downloads.
