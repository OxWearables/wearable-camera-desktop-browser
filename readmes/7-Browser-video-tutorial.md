# Video Tutorial on using Oxford/CLARITY SenseCam Browser
This browser can be used along with either the SenseCam or the Vicon Revue. For the May 2011 release, below are video clips showing the various features/functions of the browser, and how to carry them out:

## Opening browser and adding participants
* 1) [Opening up browser](http://www.youtube.com/watch?v=cDOlv8cQLJE)
* 2) [Adding new participant](http://www.youtube.com/watch?v=Kd1UgO0ZFaw)
* 3) [Loading up existing participant's images](http://www.youtube.com/watch?v=Uv_loFV-oXY)

## Uploading participant image data
* 4) [Loading in images from SenseCam or Vicon Revue](http://www.youtube.com/watch?v=U34vYvnW6gY)
* 5) [Loading images already on PC](http://www.youtube.com/watch?v=P9lZxQPdQ3E)

## Browsing through images
* 6) [Using calendar to navigate through images](http://www.youtube.com/watch?v=NGFfGhIPYS8)
* 7) [Navigating through events](http://www.youtube.com/watch?v=S8W1KbGaJNs)
* 8) [Zooming in/out of event keyframes](http://www.youtube.com/watch?v=ehdfqPrljvU)
* 9) [Changing play speed when looking at event images](http://www.youtube.com/watch?v=ilIWfh7pvdI)
* 10) [Toggle between movie player and image wall when in event images viewer](http://www.youtube.com/watch?v=QiZEd6wScsc)
* 11) [Zooming in/out  of event images when in wall view of event image viewer](http://www.youtube.com/watch?v=X2upFuEkpkw)

## Deleting images
* 12) [Deleting SenseCam images](http://www.youtube.com/watch?v=plqyrO2R2Qg)
* 13) [Undoing the deletion of SenseCam images](http://www.youtube.com/watch?v=dyFFA7lNj5s)

## Correcting event boundaries
* 14) [Correcting event boundaries when using split event into two button](http://www.youtube.com/watch?v=2M-muYe3BNY)
* 15) [Correcting event boundaries when merging events together using merge prev/next buttons](http://www.youtube.com/watch?v=Ni_Pi-HtyuQ)

## Annotating events
* 16) Annotate event, [example 1](http://www.youtube.com/watch?v=cbrATLqwtEw) + [example 2](http://www.youtube.com/watch?v=G-yYvaCpbMo)
* 17) [Annotate multiple events using the same label](http://www.youtube.com/watch?v=VBmGWjQ4sdo)
* 18) [Remove event annotation](http://www.youtube.com/watch?v=2JJ2AUGHyRo)

## Extracting more detailed information from the database. [Using Microsoft SQL Server Management Studio Express](http://go.microsoft)com/fwlink/?linkid=65110]
* 19) [Open MS SQL Server Management studio express](http://www.youtube.com/watch?v=GNpwW594AxI)
* 20) [Open Oxford CLARITY SenseCam management database](http://www.youtube.com/watch?v=pG9m1jwoxdo)
* 21) [Get list of users from database](http://www.youtube.com/watch?v=86EMgflBWKQ)
* 22) [Get list of events in database](http://www.youtube.com/watch?v=wqgOa-2GFgA)
* 23) [Get list of events for a given participant in database](http://www.youtube.com/watch?v=fNeVbssxv10)
* 24) [Get list of all images for user in database](http://www.youtube.com/watch?v=3nKlgOEFXXU)
* 25) [Find path of a given SenseCam image](http://www.youtube.com/watch?v=Ec9NtmudTJU)

### DATABASE QUERIES
Open Microsoft SQL Server Management Studio Express as detailed in [Open MS SQL Server Management studio express](http://www.youtube.com/watch?v=GNpwW594AxI)
and [Open Oxford CLARITY SenseCam management database](http://www.youtube.com/watch?v=pG9m1jwoxdo)
* 26) To retrieve event codings/annotations...
```sql
SELECT u.name, evnt.start_time, evnt.end_time, coding.annotation_name

FROM SC_Browser_User_Annotations AS coding
INNER JOIN Users AS u
	ON coding.[user_id] = u.[user_id]
	INNER JOIN All_Events AS evnt
		ON coding.[user_id] = evnt.[user_id] AND coding.event_id = evnt.event_id

ORDER BY u.[user_id], evnt.start_time
```