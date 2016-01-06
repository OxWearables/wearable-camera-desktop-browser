## DEFINE ANNOTATION LIST
<can be done anytime>

### 1. IMPORT ANNOTATION LIST (Using DohertyBrowser software)
* 1.	Open the DohertyBrowser software, and navigate into the main screen, selecting day of interest
* 2.	Click any event, which opens up a view of all images in the event
* 3.	Clicking “Edit Annotation List”
  * a. You can add a custom annotation by entering text in the top left of the new screen that pops up, then click “Add +” button, and click “Close” (top right)
* 4. Click "Import" button
* 5. Navigate to .csv file defining your annotation schema
* 6. Click "ok", complete annotation schema should now be loaded into the browser.

### 2. EXPORT ANNOTATION LIST (Using DohertyBrowser software)
* 1.	Follow steps 1-3 above
* 2. Click "Export" button
* 3. Select location and name of .csv file to write your annotation schema to
* 4. Click "ok", complete annotation schema should now be backed up to .csv file


### FORMAT OF ANNOTATION SCHEMA CSV FILE
[Click here for an example annotation schema csv file](https://sensecambrowser.codeplex.com/downloads/get/910189)
<annotation,description>
<mainCategory;subCategory,description>
* No commas allowed within either field
* annotation field must be less than 100 characters
* description field must be less than 150 characters
* In <annotation> field, items can be made a subcategory using the ';' character. For example "walking;fast,pictures changing" would mean:
  * walking (main category)
  * fast (sub category)
  * pictures changing (description of this annotation, which will appear when mouse is hovered over the annotation text in the browser)