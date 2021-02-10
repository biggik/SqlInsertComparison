# SqlInsertComparison

This is a simple program to demonstrate the differences between three different modes of "bulk" inserting into Microsoft SQL Server

## Pre-requisites
The program assumes the SqlLocalDb is installed. You can get it from [here](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15)

## Running
You can run the program (after compilation). By default 1000 (one-thousand) rows are inserted into three separate tables and the results shown as follows:

<pre>
Table 'IterativeInsert'              has 1000 rows inserted in 00:00:00.5047456 [100.00% of max]
Table 'BatchInsert'                  has 1000 rows inserted in 00:00:00.0935600 [18.54% of max]
Table 'BulkInsert'                   has 1000 rows inserted in 00:00:00.2014659 [39.91% of max]
</pre>

You can also specify another row-count on the commandline as follows

<pre>
SqlInsertComparision 100000
</pre>

would insert 100000 (one-hundred-thousand) records

## Cleanup
If everything goes well, the program creates a new LocalDB instance named <i>InstanceName</i>, creates a new database named <i>BulkInsertSample</i> and after all the inserts are over-and-done with, it will drop the database and delete the instance.

If, however, something doesn't quite work out as planned, you can simply cleanup manually by doing

<pre>
SqlLocalDb p InstanceName
SqlLocalDb d InstanceName
</pre>

which will stop and delete the instance

and then take a look into %userprofile% and delete the database files manually, e.g.

<pre>
cd %userprofile%
dir BulkInsertSample*.*
REM if any files are shown, you can delete them manually
</pre>