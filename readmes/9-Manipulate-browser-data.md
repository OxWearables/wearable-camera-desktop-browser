## MANIPULATE BROWSER DATA
*<Make sure to backup your database before performing any action!>*

### 1. BACKUP DATABASE (Using windows explorer)
* 1. Open windows explorer
* 2. Browse to C:\AidenDoherty\DohertyBrowser
* 3. Select "doherty_sensecam.db", then copy and paste the file into the same folder, renaming the new file to "doherty_sensecam_backup<date>.db" e.g. "doherty_sensecam_backup08102014.db"

### 2. DELETE PARTICIPANTS FROM DATABASE LIST (Using DB Browser for SQLite software)
* 1. Open the SqliteBrowser software ([To install, see step 5 here](1 Equipment software setup))
* 2. Select "file" -> "open database"
* 3. Browse to C:\AidenDoherty\DohertyBrowser and then select "doherty_sensecam.db", then press "open"
* 4. Select the "Browse Data" tab
* 5. Beside "Table:" select "Users"
* 6. Select any record that you would like to delete, then select "Delete Record"
* 7. Select "file" -> "write changes"
* 8. Select "file" -> "close databases"