## DOWNLOAD PARTICIPANT IMAGES
<do every time after getting devices back from a new research participant>

### 1. DOWNLOAD IMAGES FROM CAMERA (Using DohertyBrowser software)
* 1.	Open the software (as above)
* 2.	Click the dropdown box in the middle of the opening screen to select a user
  * a.	If you select “<new participant>”, then enter a ParticipantID in the new text box that appears
  * b. [Click here to clean up the list of participants shown in the list box||manipulateBrowserData]
* 3.	Click the “START” button that appears
* 4.	Plug the camera into your computer via the USB cable
* 5.	Click the “Add” button
* 6.	Click the “Start” button (indicates software has identified plugged in camera)
<it now takes a couple of minutes for the data to download>

* 7.	The main screen then shows the middle image from the “chunk” of images captured each time the camera was switched on in a given day (if there are images)
* 8.	You can click on any “chunk” image to then show all the images captured within it
  * a.	To delete any image, press “toggle view”, move forwards/backwards to the intended image, press the “Delete” (bottom left corner of screen), then click “Close” (top right) to confirm the deletion of images from this event/chunk/grouping/episode
* 9.	You can click the “Calendar” button to select another day of images
* 10.	When finished, do not plug the camera out, unto the software allows you to click the application “Quit” button
* 11.	The images will now be deleted from the camera.

### 2. IMPORT IMAGES ALREADY DOWNLOADED TO PC BY DohertyBrowser (Using DohertyBrowser software)
<if you want to do a new type of analysis on a participant's data, it is generally helpful to create a new participantID account, and then reload the participant's data>
* 1.	Open the software (as above), and navigate into the main screen
* 2.	Click the “Add” button
* 3.	Click “Advanced”
* 4.	Click “Download from PC…”
* 5.	Click “Browse” beside “Path of images already on your PC:”
* 6.	Navigate to the folder where the images are located
* 7.	Then click “Start”

### 3. IMPORT IMAGES MANUALLY COPIED FROM CAMERA TO DISK (Using DohertyBrowser software)
<For cases where images were manually copied from the camera, rather than automatically downloaded using the DohertyBrowser. This will read the images from a specified location that is treated as if it were the camera plugged into your computer, and copy the images to the “PC Destination:” location. The images can then be used for normal analysis in the browser.>
* 1.	Open the software (as above), and navigate into the main screen
* 2.	Click the “Add” button
* 3.	Click “Browse” beside “SenseCam or Vicon Revue path:”
* 6.	Navigate to the location on disk where you manually copied the participant’s images to, and click on the “DATA” folder
* 7.	Then click “Start”

*Note that the image directory structure should be the same as on the Vicon Autographer device. i.e. 
DATA / <dateFolderContainingImages> & image_table.txt*

* Optionally, you can choose where to store the re-structured image directory too. After step 6, and before step 7, click "Browse" beside "PC Destination:", and then navigate to the desired location.
* If you wish, you can set the browser to not delete the images from your source file structure/directory. Open the “C:\AidenDoherty\DohertyBrowser\SenseCamBrowser1.exe.config” file, and edit the <add key="autoDeleteImagesOnUpload" value="1"/>” line. The value 1 means the source images will be deleted, the value 0 means they will not be deleted.

### 4. IMPORT AND RELATE EXTERNAL DATA TO WEARABLE CAMERA IMAGES (Using DohertyBrowser software)
* 1.	Prepare your heartRate/GPS/accelerometer episodes into a CSV file the same format as:
  * a.	Navigate to https://sensecambrowser.codeplex.com/ 
  * b.	Click “Downloads” along the top menu bar
  * c.	Download “episodeDefsEg.csv”
* 2.	Open the software
* 3.	Create a new user, e.g. “P001Accelerometer”
* 4.	Navigate into the main screen
* 5.	Click the “Add” button
* 6.	Click “Advanced”
* 7.	Click “Download from PC” if downloading images from PC (generally this will be the case)
* 8.	Click “Browse” beside “Path of images already on your PC:”
* 9.	Navigate to the folder where the images are located
* 10.	Click “Browse” beside “Episode definitions:”
* 11.	Navigate to the .csv file you prepared in step 1
* 12.	Then click “Start”