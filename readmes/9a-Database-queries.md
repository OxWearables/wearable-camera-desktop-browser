## EXTRACT DETAILED INFORMATION FROM BROWSER DATA
<*Make sure to backup your database before performing any action*>

### 1. BACKUP DATABASE (Using windows explorer)
* 1. Open windows explorer
* 2. Browse to C:\AidenDoherty\DohertyBrowser
* 3. Select "doherty_sensecam.db", then copy and paste the file into the same folder, renaming the new file to "doherty_sensecam_backup<date>.db" e.g. "doherty_sensecam_backup08102014.db"

### 2. GET IMAGE LEVEL ANNOTATIONS (Using DB Browser for SQLite software)
* 1. Open the SqliteBrowser software ([To install, see step 5 here](1 Equipment software setup))
* 2. Select "file" -> "open database"
* 3. Browse to C:\AidenDoherty\DohertyBrowser and then select "doherty_sensecam.db", then press "open"
* 4. Select the "Execute SQL" tab, and enter the following text:
*note you have to change the green line in the text below to enter the relevant participant name* i.e. the name that appears in the dropdown list from the dohertyBrowser software
```sql
SELECT u.name, img.image_time, REPLACE(coding.annotation_name,',',' ') as annotation
 
FROM Users AS u
                INNER JOIN All_Events AS evnt
                                ON u.[user_id] = evnt.[user_id]
                INNER JOIN All_Images AS img
                                ON evnt.event_id = img.event_id
LEFT OUTER JOIN SC_Browser_User_Annotations AS coding
                ON coding.[user_id] = u.[user_id] AND coding.event_id = evnt.event_id
 
WHERE u.name = 'p16' -- CHANGE TO '<participantID>' e.g. 'P002'
 
ORDER BY u.[user_id], img.image_time
```
* 5. Select the play button symbol (or press F5)
*check that some data is returned, if not make sure you entered in the participant name exactly*
* 6. Select the small icon to the bottom right, and click "export to csv"
* 7. For "quote character" select the blank/empty option
* 8. Click ok, and then specify the location to store the image annotations csv file e.g. AidenP001Annotations.csv ... click "save"
* 9. Select "file" -> "close databases"
*To allow the csv file to handle images with more than one annotation:*
* 10. Using windows explorer, find the .csv file that you have just saved, right click and select "open with" -> "wordpad"
* 10. Press ctrl+h, enter the following criteria to replace semicolons with commas:
{code:css}
Find what:       ;
Replace with:    , 
{code:css}
* 11. Click ctrl+s to save these changes, and close the file and wordpad

### 3. GET SENSOR READINGS (Using DB Browser for SQLite software)
* 1. Open the SqliteBrowser software ([To install, see step 5 here](1 Equipment software setup))
* 2. Select "file" -> "open database"
* 3. Browse to C:\AidenDoherty\DohertyBrowser and then select "doherty_sensecam.db", then press "open"
* 4. Select the "Execute SQL" tab, and enter the following text:
*note you have to change the green line in the text below to enter the relevant participant name*
*i.e. the name that appears in the dropdown list from the dohertyBrowser software*
```sql
SELECT *
FROM Sensor_Readings
WHERE [user_id]=
	(SELECT [user_id]
	FROM Users
	WHERE username='P001' -- CHANGE TO '<participantID>' e.g. 'P002'
	)
ORDER BY sample_time DESC
```
* 5. Select the play button symbol (or press F5)
*check that some data is returned, if not make sure you entered in the participant name exactly*
* 6. Select the small icon to the bottom right, and click "export to csv"
* 7. For "quote character" select the blank/empty option
* 8. Click ok, and then specify the location to store the sensor readings csv file e.g. AidenP001SensorReadings.csv ... click "save"
* 9. Select "file" -> "close databases"