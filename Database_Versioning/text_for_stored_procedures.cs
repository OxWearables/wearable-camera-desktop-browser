using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseCamBrowser1.Database_Versioning
{
    class text_for_stored_procedures
    {


        #region richie

        public static string spValidate_User(string username, string password)
        {
            string end_string = "SELECT [user_id]";
            end_string += "\n" + "FROM Users";
            end_string += "\n" + "WHERE username = '" + username + "'";
            end_string += "\n" + "AND password = '" + password + "'";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spValidate_User] 
-- Add the parameters for the stored procedure here
@USERNAME AS VARCHAR(10),
@PASSWORD AS VARCHAR(10)

AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT [user_id]

FROM Users

WHERE username = @USERNAME
AND password = @PASSWORD
END;
             */

        } //close method spValidate_User()...



        public static string spUpdateEventComment(int user_id, int event_id, string comment)
        {
            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET comment = '" + comment + "'";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spUpdateEventComment] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT,
@COMMENT AS TEXT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
UPDATE All_Events

SET comment = @COMMENT

WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID

END
             * 
             */

        } //close method spUpdateEventComment()...



        

        public static string spUpdate_Images_With_Event_ID_split_into_multiple(int user_id)
        {
            //todo split into multiple...
            string end_string = "";
            end_string += "\n" + "";

            return end_string;
            /*
             * 
             * CREATE PROCEDURE [dbo].[spUpdate_Images_With_Event_ID] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

DECLARE @MOST_RECENT_EVENT_ID AS INT
SET @MOST_RECENT_EVENT_ID = ( (SELECT CASE WHEN MAX(event_id) IS NOT NULL THEN MAX(event_id) ELSE 0 END FROM All_Images where [user_id] = @USER_ID) - 1 ) --WILL JUST GO BACK 1 JUST TO BE ON THE SAFE SIDE SO THAT I DON'T MISS ASSIGNING THE EVENT TO ANY IMAGE

-- Insert statements for procedure here
UPDATE All_Images

SET event_id =
(
SELECT top 1 event_id
FROM All_Events
WHERE event_id > @MOST_RECENT_EVENT_ID
AND [user_id] = @USER_ID
AND start_time <= All_Images.image_time AND end_time >= All_Images.image_time
)

WHERE [user_id] = @USER_ID
AND event_id IS NULL;


--NEXT WE'LL ATTEMPT TO IDENTIFY THE EVENTS THAT EACH OF THE SENSOR READINGS BELONG TO...
--THIS FOLLOWS THE SAME PROCESS AS THE IMAGES ABOVE, EXCEPT FOR ONE SUBTLE DIFFERENCE...
--WHERE THE ALL_EVENTS TABLE TAKES THE START/END EVENT TIMES FROM THE ALL_IMAGES TABLE RATHER THAN SENSOR_READINGS
--AS THE SENSOR READINGS ARE MORE FINE GRAINED THAN THE IMAGE TIMES, THIS MEANS THERE'LL BE A FEW SENSOR VALUES MISSED OUT ON (BETWEEN EVENTS)
--THEREFORE WE LOOK FOR EVENTS THAT START +- 1 MINUTE OF THE DETECTED SENSOR TIME
UPDATE Sensor_Readings
SET event_id =
(
SELECT MAX(event_id) --IN CASE 2 EVENTS SATISFY THE CONDITION, WE'LL TAKE THE LATTER ONE OF THE TWO (I.E. THE LARGEST ID NUMBER ASSIGNED BY THE DB)
FROM All_Events
WHERE event_id > @MOST_RECENT_EVENT_ID
AND [user_id] = @USER_ID
AND DATEADD(MINUTE,-1,start_time) <= Sensor_Readings.sample_time AND DATEADD(MINUTE,1,end_time) >= Sensor_Readings.sample_time -- HERE'S WHERE WE USE ADD +-1 MINUTE, TO DEAL WITH THE FACT THAT SENSOR_READINGS ARE MORE FINE GRAINED THAN THE START/END TIME DEFINED IN THE ALL_EVENTS TABLE...
)

WHERE [user_id] = @USER_ID
AND event_id IS NULL;



-- LASTLY, IF THERE'S BEEN ANY UPLOADING PROBLEM WHERE THE CAMERA WAS ACCIDENTALLY
-- RESET ... I.E. WHERE NEW IMAGES APPEAR TO BELONG TO THE YEAR 2000
-- THEN WHAT GENERALLY SEEMS TO HAPPEN IS THAT ONE EVENT HAS A NORMAL START TIME
-- BUT AN END TIME SOMETIME IN THE YEAR 2000 ... SO WHAT WE NEED TO DO
-- IS DELETE THAT PARTICULAR EVENT AS IT HAS NO IMAGES
DELETE FROM All_Events
WHERE [user_id]=@USER_ID
and start_time>end_time;



--then IF ANY EVENTS ARE MISSED for ANY REASON (GENERALLY WHEN A CHUNK IS MINISCULE I.E. LESS THAN the TEXTTILING BOUNDARY WINDOW SIZE)
--WE'LL GIVE ANY EVENTS TO BE IN THE SAME EVENT AS THE VERY LAST EVENT_ID

--UPDATE All_Images
--SET event_id = (SELECT MAX(event_id) FROM All_Events WHERE [user_id] = @USER_ID)
--WHERE [user_id] = @USER_ID
--AND event_id IS NULL;

END
             * 
             * 
             */

        } //close method spUpdate_Images_With_Event_ID()


        public static string spUpdate_Images_With_Event_ID_step1_get_most_recent_event_id_for_user(int user_id)
        {
            string end_string = "";
            end_string += "\n" + "SELECT CASE WHEN MAX(event_id) IS NOT NULL THEN MAX(event_id)-1 ELSE -1 END "; //WILL JUST GO BACK 1 JUST TO BE ON THE SAFE SIDE SO THAT I DON'T MISS ASSIGNING THE EVENT TO ANY IMAGE
            end_string += "\n" + "FROM All_Images where [user_id] = " + user_id;

            return end_string;
            /*
             * 
             * CREATE PROCEDURE [dbo].[spUpdate_Images_With_Event_ID] 
            -- Add the parameters for the stored procedure here
            @USER_ID AS INT
            AS
            BEGIN
            -- SET NOCOUNT ON added to prevent extra result sets from
            -- interfering with SELECT statements.
            SET NOCOUNT ON;

            DECLARE @MOST_RECENT_EVENT_ID AS INT
            SET @MOST_RECENT_EVENT_ID = ( (SELECT CASE WHEN MAX(event_id) IS NOT NULL THEN MAX(event_id) ELSE 0 END FROM All_Images where [user_id] = @USER_ID) - 1 ) --WILL JUST GO BACK 1 JUST TO BE ON THE SAFE SIDE SO THAT I DON'T MISS ASSIGNING THE EVENT TO ANY IMAGE
            */
        } //close method spUpdate_Images_With_Event_ID_step1_get_most_recent_event_id_for_user()....


        public static string spUpdate_Images_With_Event_ID_step2_update_images_with_relevant_event_id(int user_id, int most_recent_event_id)
        {
            string end_string = "";
            end_string += "\n" + "UPDATE All_Images";
            end_string += "\n" + "SET event_id=";
            end_string += "\n" + "(";
            end_string += "\n" + "SELECT event_id"; //todo may have to change this to max(event_id) and remove limit 1 a few lines below?
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE event_id > " + most_recent_event_id;
            end_string += "\n" + "AND [user_id] = " + user_id;
            end_string += "\n" + "AND start_time <= All_Images.image_time AND end_time >= All_Images.image_time";
            end_string += "\n" + "LIMIT 1";
            end_string += "\n" + ")";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id IS NULL;";

            return end_string;
            /*
            UPDATE All_Images

            SET event_id =
            (
            SELECT top 1 event_id
            FROM All_Events
            WHERE event_id > @MOST_RECENT_EVENT_ID
            AND [user_id] = @USER_ID
            AND start_time <= All_Images.image_time AND end_time >= All_Images.image_time
            )

            WHERE [user_id] = @USER_ID
            AND event_id IS NULL;
            */
        } //close method spUpdate_Images_With_Event_ID_step2_update_images_with_relevant_event_id()....


        public static string spUpdate_Images_With_Event_ID_step3_update_sensor_readings_with_relevant_event_id(int user_id, int most_recent_event_id)
        {
            string end_string = "";
            end_string += "\n" + "UPDATE Sensor_Readings";
            end_string += "\n" + "SET event_id=";
            end_string += "\n" + "(";
            end_string += "\n" + "SELECT MAX(event_id)";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE event_id > " + most_recent_event_id;
            end_string += "\n" + "AND [user_id] = " + user_id;
            //end_string += "\n" + "AND DATEADD(MINUTE,-1,start_time) <= Sensor_Readings.sample_time AND DATEADD(MINUTE,1,end_time) >= Sensor_Readings.sample_time";
            end_string += "\n" + "AND DATETIME(start_time,'-1 minute') <= Sensor_Readings.sample_time AND DATETIME(end_time,'+1 minute') >= Sensor_Readings.sample_time";
            end_string += "\n" + ")";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id IS NULL;";

            return end_string;
            /*
            --NEXT WE'LL ATTEMPT TO IDENTIFY THE EVENTS THAT EACH OF THE SENSOR READINGS BELONG TO...
            --THIS FOLLOWS THE SAME PROCESS AS THE IMAGES ABOVE, EXCEPT FOR ONE SUBTLE DIFFERENCE...
            --WHERE THE ALL_EVENTS TABLE TAKES THE START/END EVENT TIMES FROM THE ALL_IMAGES TABLE RATHER THAN SENSOR_READINGS
            --AS THE SENSOR READINGS ARE MORE FINE GRAINED THAN THE IMAGE TIMES, THIS MEANS THERE'LL BE A FEW SENSOR VALUES MISSED OUT ON (BETWEEN EVENTS)
            --THEREFORE WE LOOK FOR EVENTS THAT START +- 1 MINUTE OF THE DETECTED SENSOR TIME
            UPDATE Sensor_Readings
            SET event_id =
            (
            SELECT MAX(event_id) --IN CASE 2 EVENTS SATISFY THE CONDITION, WE'LL TAKE THE LATTER ONE OF THE TWO (I.E. THE LARGEST ID NUMBER ASSIGNED BY THE DB)
            FROM All_Events
            WHERE event_id > @MOST_RECENT_EVENT_ID
            AND [user_id] = @USER_ID
            AND DATEADD(MINUTE,-1,start_time) <= Sensor_Readings.sample_time AND DATEADD(MINUTE,1,end_time) >= Sensor_Readings.sample_time -- HERE'S WHERE WE USE ADD +-1 MINUTE, TO DEAL WITH THE FACT THAT SENSOR_READINGS ARE MORE FINE GRAINED THAN THE START/END TIME DEFINED IN THE ALL_EVENTS TABLE...
            )

            WHERE [user_id] = @USER_ID
            AND event_id IS NULL;
            */
        } //close method spUpdate_Images_With_Event_ID_step2_update_images_with_relevant_event_id()....


        public static string spUpdate_Images_With_Event_ID_step4_tidy_up_stage(int user_id)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "and start_time>end_time;";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "UPDATE All_Images";
            end_string += "\n" + "SET event_id = (SELECT MAX(event_id) FROM All_Events WHERE [user_id] = " + user_id + ")";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id IS NULL;";

            return end_string;
            /*
            -- LASTLY, IF THERE'S BEEN ANY UPLOADING PROBLEM WHERE THE CAMERA WAS ACCIDENTALLY
            -- RESET ... I.E. WHERE NEW IMAGES APPEAR TO BELONG TO THE YEAR 2000
            -- THEN WHAT GENERALLY SEEMS TO HAPPEN IS THAT ONE EVENT HAS A NORMAL START TIME
            -- BUT AN END TIME SOMETIME IN THE YEAR 2000 ... SO WHAT WE NEED TO DO
            -- IS DELETE THAT PARTICULAR EVENT AS IT HAS NO IMAGES
            DELETE FROM All_Events
            WHERE [user_id]=@USER_ID
            and start_time>end_time;



            --then IF ANY EVENTS ARE MISSED for ANY REASON (GENERALLY WHEN A CHUNK IS MINISCULE I.E. LESS THAN the TEXTTILING BOUNDARY WINDOW SIZE)
            --WE'LL GIVE ANY EVENTS TO BE IN THE SAME EVENT AS THE VERY LAST EVENT_ID

            --UPDATE All_Images
            --SET event_id = (SELECT MAX(event_id) FROM All_Events WHERE [user_id] = @USER_ID)
            --WHERE [user_id] = @USER_ID
            --AND event_id IS NULL;
            */
        } //close method spUpdate_Images_With_Event_ID_step2_update_images_with_relevant_event_id()....



        public static string spUpdate_Event_Keyframe_Path(int user_id, int event_id, string keyframe_path)
        {
            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET keyframe_path = '" + keyframe_path + "'";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;

            return end_string;
            /*
             * create PROCEDURE [dbo].[spUpdate_Event_Keyframe_Path]
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT,
@KEYFRAME_PATH AS VARCHAR(256)

AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
UPDATE All_Events

SET keyframe_path = @KEYFRAME_PATH

WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID


END
             * 
             */

        } //end method spUpdate_Event_Keyframe_Path()...




        public static string spLog_User_Interaction(int user_id, DateTime interaction_time, string uixaml_element, string comma_seperated_parameters)
        {
            string end_string = "INSERT INTO User_Interaction_Log";
            end_string += "\n" + "VALUES(" + user_id + ", " + convert_datetime_to_sql_string(interaction_time) + ", '" + uixaml_element + "', '" + comma_seperated_parameters + "');";

            return end_string;
            /*
             * 
             * CREATE PROCEDURE [dbo].[spLog_User_Interaction]
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@INTERACTION_TIME AS DATETIME,
@UIXAML_ELEMENT AS VARCHAR(80),
@COMMA_SEPERATED_PARAMETERS AS VARCHAR(255)
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
INSERT INTO User_Interaction_Log
VALUES(@USER_ID, @INTERACTION_TIME, @UIXAML_ELEMENT, @COMMA_SEPERATED_PARAMETERS)

END
             * 
             */

        } //close method spLog_User_Interaction()...




        public static string spGet_Specific_Event(int user_id, int event_id)
        {
            string end_string = "SELECT event_id, start_time, end_time, keyframe_path, comment";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;

            return end_string;
            /*
             * 
             * CREATE PROCEDURE [dbo].[spGet_Specific_Event] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT event_id,
start_time,
end_time,
keyframe_path,
comment

FROM All_Events

WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID


END
             * 
             */

        } //close method spGet_Specific_Event()...




        public static string spGet_Paths_Of_All_Images_In_Events(int user_id, int event_id)
        {
            string end_string = "SELECT image_path, image_time";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;
            end_string += "\n" + "ORDER BY image_time";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spGet_Paths_Of_All_Images_In_Events] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT

AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT image_path, image_time

FROM All_Images

WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID

ORDER BY image_time

END
             * 
             * 
             */

        } //close method spGet_Paths_Of_All_Images_In_Events()...



        public static string spGet_Num_Images_In_Event(int user_id, int event_id)
        {
            string end_string = "SELECT COUNT(image_id)";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spGet_Num_Images_In_Event] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT COUNT(image_id)

FROM All_Images

WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID

END
             * 
             */

        } //close method spGet_Num_Images_In_Event()...




        public static string spGet_Num_Images_In_Day(int user_id, DateTime day)
        {
            string end_string = "SELECT COUNT(*)";
            end_string += "\n" + "";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id in";
            end_string += "\n" + "(";
            end_string += "\n" + "SELECT event_id";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + ")";
            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spGet_Num_Images_In_Day] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT COUNT(*)

FROM All_Images

WHERE [user_id] = @USER_ID
AND event_id in
(
SELECT event_id
FROM All_Events
WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)
)


END
             * 
             */

        } //close method spGet_Num_Images_In_Day()...




        public static string spGet_Morning_Events(int user_id, DateTime day)
        {
            string end_string = "SELECT event_id, start_time, end_time, keyframe_path, comment";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 11, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(HOUR, start_time) < 12";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spGet_Morning_Events] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT event_id,
start_time,
end_time,
keyframe_path,
comment

FROM All_Events

WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)
AND DATEPART(HOUR, start_time) < 12


END
             * 
             */
        } //close method spGet_Morning_Events()...




        public static string spGet_List_Of_All_Days_For_User(int user_id)
        {
            string end_string = "SELECT MIN([day])";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "GROUP BY strftime('%Y-%m-%d',day)";
            end_string += "\n" + "ORDER BY MIN([day]) DESC";

            return end_string;
            /*
             * 
             * CREATE PROCEDURE [dbo].[spGet_List_Of_All_Days_For_User] 
@USER_ID AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

SELECT MIN([day])
FROM All_Events
WHERE [user_id] = @USER_ID
GROUP BY DATEPART(YEAR, [day]), DATEPART(DAYOFYEAR, [day])
ORDER BY MIN([day]) desc
END
GO
             * 
             */

        } //close method spGet_List_Of_All_Days_For_User()...



        public static string spGet_Last_Keyframe_Path(int user_id)
        {
            string end_string = "SELECT keyframe_path";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "ORDER BY [day] DESC";
            end_string += "\n" + "LIMIT 1";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spGet_Last_Keyframe_Path] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
select top 1 keyframe_path
from All_Events
where [user_id] = @USER_ID
order by [day] desc

END
             * 
             */
        } //close method spGet_Last_Keyframe_Path()



        public static string spGet_Evening_Events(int user_id, DateTime day)
        {
            string end_string = "SELECT event_id, start_time, end_time, keyframe_path, comment";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 17, 00, 00)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(HOUR, start_time) > 17";

            return end_string;
            /*
             * 
             * CREATE PROCEDURE [dbo].[spGet_Evening_Events] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT event_id,
start_time,
end_time,
keyframe_path,
comment

FROM All_Events

WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)
AND DATEPART(HOUR, start_time) > 17


END
             * 
             */
        } //close method spGet_Evening_Events()...






        public static string spGet_Day_Start_and_End_Times(int user_id, DateTime day)
        {
            string end_string = "SELECT MIN(start_time) AS start_time, MAX(end_time) AS end_time";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";

            return end_string;
            /*
             * 
             * create PROCEDURE [dbo].[spGet_Day_Start_and_End_Times] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT min(start_time) as start_time, max(end_time) as end_time

FROM All_Events
WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)


END
             * 
             * 
             */

        } //close method spGet_Day_Start_and_End_Times()...




        public static string spGet_All_Events_In_Day(int user_id, DateTime day)
        {
            string end_string = "SELECT event_id, start_time, end_time, keyframe_path, comment";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year,day.Month,day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year,day.Month,day.Day,23,59,59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "ORDER BY start_time";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spGet_All_Events_In_Day] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT event_id,
start_time,
end_time,
keyframe_path,
comment

FROM All_Events

WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)

order by start_time 

END
             * 
             */
        } //close method spGet_All_Events_In_Day()...




        public static string spGet_Afternoon_Events(int user_id, DateTime day)
        {
            string end_string = "SELECT event_id, start_time, end_time, keyframe_path, comment";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 12, 00, 00)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 16, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(HOUR, start_time) >= 12";
            //end_string += "\n" + "AND DATEPART(HOUR, start_time) <= 17";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[spGet_Afternoon_Events] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT event_id,
start_time,
end_time,
keyframe_path,
comment

FROM All_Events

WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)
AND DATEPART(HOUR, start_time) >= 12
AND DATEPART(HOUR, start_time) <= 17


END
             * 
             */

        } //close method spGet_Afternoon_Events()...




        public static string spDelete_Image_From_Event(int user_id, int event_id, DateTime image_time)
        {
            string end_string = "DELETE";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;
            end_string += "\n" + "AND image_time = " + convert_datetime_to_sql_string(image_time) + ";";

            return end_string;
            /*
             * 
             * create PROCEDURE [dbo].[spDelete_Image_From_Event]
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT,
@IMAGE_TIME AS DATETIME

AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
DELETE

FROM All_Images

WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID
AND image_time = @IMAGE_TIME


END
             * 
             */
        } //close method spDelete_Image_From_Event()...



        public static string spDelete_Event(int user_id, int event_id)
        {
            string end_string = "DELETE";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;

            return end_string;
            /*
             * create PROCEDURE [dbo].[spDelete_Event]
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT

AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
DELETE

FROM All_Events

WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID


END
             */
        } //close method spDelete_Event()...




        public static string Oct10_UPDATE_EVENT_KEYFRAME_IMAGE()
        {
            //todo multiple query
            string end_string = "";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[Oct10_UPDATE_EVENT_KEYFRAME_IMAGE] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;



--STEP 1. FIRSTLY LET'S DETERMINE THE START/END TIMES OF THE EVENT...
DECLARE @NEW_START_TIME AS DATETIME
DECLARE @NEW_END_TIME AS DATETIME 

--firstly the event which has the images added to it...
SELECT @NEW_START_TIME = start_time, @NEW_END_TIME = end_time
FROM All_Events
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID






--STEP 2. FROM THE START/END TIMES, LET'S SET A TARGET (MIDDLE) TIME FROM WHICH TO SELECT THE KEYFRAME...
DECLARE @NEW_KEYFRAME_TARGET_TIME AS DATETIME
SET @NEW_KEYFRAME_TARGET_TIME = DATEADD(MINUTE, DATEDIFF(MINUTE,@NEW_START_TIME, @NEW_END_TIME)/2, @NEW_START_TIME)

-- also let's declare an allowable bit of time leeway from which to select the keyframe image...
DECLARE @ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES AS INT
SET @ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES=1




-- STEP 3. CONSIDERING OUR TARGET_TIME, AND THE AMOUND OF LEEWAY, LET'S SELECT A RANDOM IMAGE FROM AROUND THIS TIME TO BE OUR KEYFRAME PATH
DECLARE @NEW_KEYFRAME_PATH AS VARCHAR(255)
SET @NEW_KEYFRAME_PATH = 
(
SELECT TOP 1 image_path
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID
AND DATEDIFF(MINUTE, image_time, @NEW_KEYFRAME_TARGET_TIME) > -@ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES
AND DATEDIFF(MINUTE, image_time, @NEW_KEYFRAME_TARGET_TIME) < @ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES
ORDER BY NEWID()
)



-- STEP 4. JUST IN CASE, STEP 3 DIDN'T PRODUCE A KEYFRAME IMAGE (e.g. no images taken around that time window in the middle as privacy button was hit on the camera)
--
IF @NEW_KEYFRAME_PATH IS NULL
BEGIN
SET @NEW_KEYFRAME_PATH = 
(
SELECT TOP 1 image_path
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID
ORDER BY NEWID()
)
END



-- STEP 5. AND LASTLY LET'S UPDATE THE All_Events TABLE WITH THE NEW KEYFRAME PATH INFORMATION!
UPDATE All_Events
SET keyframe_path = @NEW_KEYFRAME_PATH
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID


END
             */
        } //close method Oct10_UPDATE_EVENT_KEYFRAME_IMAGE()...


        public static string Oct10_UPDATE_EVENT_KEYFRAME_IMAGE_select_random_image_from_event_target_window(int user_id, int event_id, DateTime target_time, int search_window_minutes)
        {
            //todo multiple query
            string end_string = "SELECT image_path";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id;
            end_string += "\n" + "AND image_time >= " + convert_datetime_to_sql_string(target_time.AddMinutes(-search_window_minutes));
            end_string += "\n" + "AND image_time <= " + convert_datetime_to_sql_string(target_time.AddMinutes(search_window_minutes));            
            end_string += "\n" + "ORDER BY RANDOM()";
            end_string += "\n" + "LIMIT 1;";
            
            return end_string;
            /*
             * 
-- STEP 3. CONSIDERING OUR TARGET_TIME, AND THE AMOUND OF LEEWAY, LET'S SELECT A RANDOM IMAGE FROM AROUND THIS TIME TO BE OUR KEYFRAME PATH
DECLARE @NEW_KEYFRAME_PATH AS VARCHAR(255)
SET @NEW_KEYFRAME_PATH = 
(
SELECT TOP 1 image_path
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID
AND DATEDIFF(MINUTE, image_time, @NEW_KEYFRAME_TARGET_TIME) > -@ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES
AND DATEDIFF(MINUTE, image_time, @NEW_KEYFRAME_TARGET_TIME) < @ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES
ORDER BY NEWID()
)


             */
        } //close method Oct10_UPDATE_EVENT_KEYFRAME_IMAGE()...


        public static string Oct10_UPDATE_EVENT_KEYFRAME_IMAGE_select_any_random_image_from_event(int user_id, int event_id)
        {
            //todo multiple query
            string end_string = "SELECT image_path";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id;
            end_string += "\n" + "ORDER BY RANDOM()";
            end_string += "\n" + "LIMIT 1;";
            
            return end_string;
            /*
             * 


-- STEP 4. JUST IN CASE, STEP 3 DIDN'T PRODUCE A KEYFRAME IMAGE (e.g. no images taken around that time window in the middle as privacy button was hit on the camera)
--
IF @NEW_KEYFRAME_PATH IS NULL
BEGIN
SET @NEW_KEYFRAME_PATH = 
(
SELECT TOP 1 image_path
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID
ORDER BY NEWID()
)
END

             */
        } //close method Oct10_UPDATE_EVENT_KEYFRAME_IMAGE()...


        public static string Oct10_UPDATE_EVENT_KEYFRAME_IMAGE_update_table(int user_id, int event_id, DateTime start_time, DateTime end_time, string new_keyframe_path)
        {
            //todo multiple query
            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET start_time = " + convert_datetime_to_sql_string(start_time) + ",";
            end_string += "\n" + "end_time = " + convert_datetime_to_sql_string(end_time) + ",";
            end_string += "\n" + "keyframe_path = '" + new_keyframe_path + "'";            
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id + ";";
            
            return end_string;
            /*
       
-- STEP 5. AND LASTLY LET'S UPDATE THE All_Events TABLE WITH THE NEW KEYFRAME PATH INFORMATION!
UPDATE All_Events
SET keyframe_path = @NEW_KEYFRAME_PATH
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID


END
             */
        } //close method Oct10_UPDATE_EVENT_KEYFRAME_IMAGE()...




        public static string JAN11_GET_ANNOTATED_EVENTS_IN_DAY(int user_id, DateTime day)
        {
            //string end_string = "SELECT annotations.event_id, annotations.annotation_name,DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds";
            string end_string = "SELECT annotations.event_id, annotations.annotation_name, strftime('%s',All_Events.end_time) - strftime('%s',All_Events.start_time) AS duration_in_seconds";
            end_string += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "";
            end_string += "\n" + "INNER JOIN All_Events";
            end_string += "\n" + "ON annotations.event_id = All_Events.[event_id]";
            end_string += "\n" + "";
            end_string += "\n" + "WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "";
            end_string += "\n" + "ORDER BY All_Events.start_time";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[JAN11_GET_ANNOTATED_EVENTS_IN_DAY] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME


AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here

SELECT annotations.event_id, annotations.annotation_name,
DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds


FROM SC_Browser_User_Annotations AS annotations

INNER JOIN All_Events
ON annotations.event_id = All_Events.[event_id]


WHERE annotations.[user_id]=@USER_ID
AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, @DAY)


ORDER BY All_Events.start_time


END
             */
        } //close method JAN11_GET_ANNOTATED_EVENTS_IN_DAY()...



        public static string JAN_11_GET_IMAGE_IN_EVENT_NEAREST_TARGET_TIME(int user_id, int event_id, DateTime target_time, int search_window_in_minutes)
        {
            string end_string = "SELECT image_path";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id]=";
            end_string += "\n" + "AND event_id = ";
            end_string += "\n" + "AND image_time >= DATEADD(MINUTE,-" + search_window_in_minutes + "," + convert_datetime_to_sql_string(target_time) + ")";
            end_string += "\n" + "AND image_time <= DATEADD(MINUTE," + search_window_in_minutes + "," + convert_datetime_to_sql_string(target_time) + ")";
            end_string += "\n" + "ORDER BY ABS(DATEDIFF(SECOND,image_time," + convert_datetime_to_sql_string(target_time) + "))";
            end_string += "\n" + "LIMIT 1";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[JAN_11_GET_IMAGE_IN_EVENT_NEAREST_TARGET_TIME]
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT,
@TARGET_TIME AS DATETIME,
@SEARCH_WINDOW_IN_MINUTES AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT TOP 1 image_path

FROM All_Images

WHERE [user_id]=@USER_ID
AND event_id = @EVENT_ID
AND image_time >= DATEADD(MINUTE,-@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)
AND image_time <= DATEADD(MINUTE,@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)

ORDER BY ABS(DATEDIFF(SECOND,image_time,@TARGET_TIME))



END
             */
        } //close method JAN_11_GET_IMAGE_IN_EVENT_NEAREST_TARGET_TIME()...



        #endregion richie







        #region aiden


        private static string convert_datetime_to_sql_string(DateTime time)
        {
            string month, day, hour, minute, second;
            if (time.Month < 10)
                month = "0" + time.Month;
            else month = time.Month.ToString();

            if (time.Day < 10)
                day = "0" + time.Day;
            else day = time.Day.ToString();
            
            if (time.Hour < 10)
                hour = "0" + time.Hour;
            else hour = time.Hour.ToString();

            if (time.Minute < 10)
                minute = "0" + time.Minute;
            else minute = time.Minute.ToString();

            if (time.Second < 10)
                second = "0" + time.Second;
            else second = time.Second.ToString();

            return "'" + time.Year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second + "'";// +"." + time.Millisecond + "'";
        } //close method convert_datetime_to_sql_string()...

        public static string JAN_11_GET_IMAGE_IN_DAY_NEAREST_TARGET_TIME(int user_id, DateTime day, DateTime target_time, int search_window_in_minutes)
        {
            string end_string = "";
            end_string += "SELECT image_path";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id IN (SELECT event_id";
            end_string += "\n" + "  FROM All_Events";
            end_string += "\n" + "  WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "      AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "      AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            /* NEED DIFFERENT VARIABLES FOR @DAY & @TARGET TIME, JUST IN CASE FOR CASES OF DATA AROUND MIDNIGHT...
--THE REASON FOR THIS IS THAT IF THE @TARGET_TIME IS AFTER MIDNIGHT,
--BUT ALL THE EVENTS WILL HAVE A @DAY TIME OF BEFORE MIDNIGHT, SO CAN'T CARRY OUT THE SEARCH ABOVE
--ALSO CAN'T USE START/END TIMES AS THEY COULD BE EITHER SIDE OF MIDNIGHT...
             */
            end_string += "\n" + "  )";
            end_string += "\n" + "AND image_time >= DATEADD(MINUTE,-" + search_window_in_minutes + "," + convert_datetime_to_sql_string(target_time) + ")";
            end_string += "\n" + "AND image_time <= DATEADD(MINUTE," + search_window_in_minutes + "," + convert_datetime_to_sql_string(target_time) + ")";
            end_string += "\n" + "";
            end_string += "\n" + "ORDER BY ABS(DATEDIFF(SECOND,image_time," + target_time + "));";
            end_string += "\n" + "LIMIT 1";



            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[JAN_11_GET_IMAGE_IN_DAY_NEAREST_TARGET_TIME]
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME, --NEED DIFFERENT VARIABLES FOR DAY & TARGET TIME, JUST IN CASE FOR CASES OF DATA AROUND MIDNIGHT...
@TARGET_TIME AS DATETIME,
@SEARCH_WINDOW_IN_MINUTES AS INT
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT TOP 1 image_path

FROM All_Images

WHERE [user_id]=@USER_ID
AND event_id IN (SELECT event_id
FROM All_Events
WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)
--NEED DIFFERENT VARIABLES FOR @DAY & @TARGET TIME, JUST IN CASE FOR CASES OF DATA AROUND MIDNIGHT...
--THE REASON FOR THIS IS THAT IF THE @TARGET_TIME IS AFTER MIDNIGHT,
--BUT ALL THE EVENTS WILL HAVE A @DAY TIME OF BEFORE MIDNIGHT, SO CAN'T CARRY OUT THE SEARCH ABOVE
--ALSO CAN'T USE START/END TIMES AS THEY COULD BE EITHER SIDE OF MIDNIGHT...
)
AND image_time >= DATEADD(MINUTE,-@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)
AND image_time <= DATEADD(MINUTE,@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)

ORDER BY ABS(DATEDIFF(SECOND,image_time,@TARGET_TIME))



END
             */
        } //close method JAN_11_GET_IMAGE_IN_DAY_NEAREST_TARGET_TIME()...



        public static string JAN_11_GET_ACC_SENSOR_VALUES(int user_id, DateTime day)
        {
            string end_string = "";
            end_string += "\n" + "SELECT event_id,sample_time,acc_x,acc_y,acc_z";
            end_string += "\n" + "FROM Sensor_Readings";
            end_string += "\n" + "WHERE [user_id]=" + user_id;
            end_string += "\n" + "AND event_id IN (SELECT event_id";
            end_string += "\n" + "  FROM All_Events";
            end_string += "\n" + "  WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "  AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "  AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + ")";
            end_string += "\n" + "";
            end_string += "\n" + "ORDER BY sample_time;";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[JAN_11_GET_ACC_SENSOR_VALUES]
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here

SELECT event_id,
sample_time,
acc_x,
acc_y,
acc_z

FROM Sensor_Readings

WHERE [user_id]=@USER_ID
AND event_id IN (SELECT event_id
FROM All_Events
WHERE [user_id] = @USER_ID
AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)
)

ORDER BY sample_time



END
             */
        } //close method JAN_11_GET_ACC_SENSOR_VALUES()...



        public static string feb10_spUpdateEvent_Number_Times_Viewed(int user_id, int event_id)
        {
            string end_string = "";
            end_string += "\n" + "UPDATE All_Events";
            end_string += "\n" + "SET number_times_viewed = number_times_viewed + 1";
            end_string += "\n" + "WHERE [user_id]= " + user_id;
            end_string += "\n" + "and event_id = " + event_id + ";";

            return end_string;

            /*
             * CREATE PROCEDURE [dbo].[feb10_spUpdateEvent_Number_Times_Viewed]
@USER_ID AS INT,
@EVENT_ID AS INT

AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

--let's now update the number of times that this event has been viewed
UPDATE All_Events
SET number_times_viewed = number_times_viewed + 1
WHERE [user_id]= @USER_ID
and event_id = @EVENT_ID;


END
             */
        } //close method feb10_spUpdateEvent_Number_Times_Viewed()...




        public static string feb_10_spInsert_New_User_Into_Database_and_Return_ID_part1_insert_into_users_table_and_get_id(string new_user_name)
        {
            //todo multiquery
            string end_string = "";
            end_string += "\n" + "insert into Users (username,password,name) values ('" + new_user_name + "','" + new_user_name + "','" + new_user_name + "');";
            end_string += "\n" + "";
            end_string += "\n" + "select max([user_id]) from Users;";
            
            return end_string;        
            /*
             * CREATE PROCEDURE [dbo].[feb_10_spInsert_New_User_Into_Database_and_Return_ID]
-- Add the parameters for the stored procedure here
@NEW_USER_NAME AS VARCHAR(50)
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
insert into Users values (@NEW_USER_NAME, @NEW_USER_NAME, @NEW_USER_NAME);

--then using that userid, we insert a dummy event into the All_Events table
declare @tmp_uid as int
set @tmp_uid = (select max([user_id]) from Users)
insert into All_Events values (@tmp_uid,'1999-01-23 08:00:00','1999-01-23 07:00:00', '1999-01-23 08:00:00', '1999-01-23 08:05:00', '','test event',0);

--then using the userid and the newly created Event_ID, we then insert a dummy image in the All_Images table…
declare @tmp_eid as int
set @tmp_eid = (select max(event_id) from All_Events where [user_id]=@tmp_uid)
insert into All_Images values (@tmp_uid, @tmp_eid,'','1991-01-23 08:02:00');



--and FINALLY RETURN the ID for THIS NEW USER…
select top 1 [user_id] from Users where [name]=@NEW_USER_NAME;

END
             */
        } //close method feb_10_spInsert_New_User_Into_Database_and_Return_ID()...
        

        public static string feb_10_spInsert_New_User_Into_Database_and_Return_ID_part2_insert_into_events_and_get_event_id(int user_id)
        {
            //todo multiquery
            string end_string = "";
            end_string += "\n" + "insert into All_Events (user_id,day,utc_day,start_time,end_time,keyframe_path,comment,number_times_viewed) values (" + user_id + ",'1999-01-23 08:00:00','1999-01-23 07:00:00', '1999-01-23 08:00:00', '1999-01-23 08:05:00', '','test event',0);";
            end_string += "\n" + "";
            end_string += "\n" + "select max(event_id) from All_Events where [user_id]=" + user_id + ";";

            return end_string;
        }


        public static string feb_10_spInsert_New_User_Into_Database_and_Return_ID_part3_insert_into_images(int user_id, int event_id)
        {
            //todo multiquery
            string end_string = "";
            end_string += "\n" + "insert into All_Images (user_id,event_id,image_path,image_time) values (" + user_id + "," + event_id + ",'','1991-01-23 08:02:00');";
            return end_string;
        }



        public static string feb_10_spGet_UserID_of_Most_Recent_Data_Upload()
        {
            string end_string = "";
            end_string += "\n" + "select [user_id]";
            end_string += "\n" + "from All_Events";
            end_string += "\n" + "group by [user_id]";
            end_string += "\n" + "order by max([day]) desc;";
            end_string += "\n" + "LIMIT 1";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[feb_10_spGet_UserID_of_Most_Recent_Data_Upload]
-- Add the parameters for the stored procedure here
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

select top 1 [user_id]
from All_Events
group by [user_id]
order by max([day]) desc


END
             */
        } //close method feb_10_spGet_UserID_of_Most_Recent_Data_Upload()...




        public static string feb_10_spGet_List_Of_Users()
        {
            string end_string = "";
            end_string += "\n" + "select [user_id], username, [password], [name]";
            end_string += "\n" + "from Users";
            end_string += "\n" + "order by [name]";
            end_string += "\n" + "";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[feb_10_spGet_List_Of_Users]
-- Add the parameters for the stored procedure here
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

select [user_id], username, [password], [name]
from Users
order by [name]


END
             */
        } //close method feb_10_spGet_List_Of_Users()...




        public static string AUG12_CLEAR_EVENT_ANNOTATIONS_INDIVIDUAL(int user_id, int event_id, string individual_annotation_text)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM SC_Browser_User_Annotations";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;
            end_string += "\n" + "AND [annotation_name] = '" + individual_annotation_text + "';";
            end_string += "\n" + "";
            end_string += "\n" + "";


            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[AUG12_CLEAR_EVENT_ANNOTATIONS_INDIVIDUAL] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT,
@INDIVIDUAL_ANNOTATION_TEXT AS VARCHAR(100)


AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
DELETE FROM SC_Browser_User_Annotations
WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID
AND [annotation_name] = @INDIVIDUAL_ANNOTATION_TEXT;


END
             */
        } //close method AUG12_CLEAR_EVENT_ANNOTATIONS_INDIVIDUAL()...



        public static string APR11_REMOVE_ANNOTATION_TYPE(string annotation_type_name)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM Annotation_Types";
            end_string += "\n" + "WHERE annotation_type = '" + annotation_type_name + "'";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[APR11_REMOVE_ANNOTATION_TYPE] 
-- Add the parameters for the stored procedure here
@ANNOTATION_TYPE_NAME AS VARCHAR(50)
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

--now delete the relevant entry
DELETE FROM Annotation_Types
WHERE annotation_type = @ANNOTATION_TYPE_NAME
END
             */
        } //close method APR11_REMOVE_ANNOTATION_TYPE()...



        public static string APR11_REMOVE_ALL_ANNOTATION_TYPES()
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM Annotation_Types;";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[APR11_REMOVE_ALL_ANNOTATION_TYPES] 
-- Add the parameters for the stored procedure here
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

--now delete all entries in the annotation types table
DELETE FROM Annotation_Types

END
             */
        } //close APR11_REMOVE_ALL_ANNOTATION_TYPES()...




        public static string NOV10_GET_LIST_OF_ANNOTATION_CLASSES()
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotation_id, annotation_type, [description]";
            end_string += "\n" + "FROM Annotation_Types";
            end_string += "\n" + "order by annotation_type;";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[NOV10_GET_LIST_OF_ANNOTATION_CLASSES] 
-- Add the parameters for the stored procedure here
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT annotation_id, annotation_type, [description]
FROM Annotation_Types
order by annotation_type


END
             */
        } //close method NOV10_GET_LIST_OF_ANNOTATION_CLASSES()...




        public static string NOV10_GET_EVENTS_IDS_IN_DAY_FOR_GIVEN_ACTIVITY(int user_id, DateTime day, string annotation_type)
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotations.event_id";
            end_string += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "INNER JOIN All_Events";
            end_string += "\n" + "ON annotations.event_id = All_Events.event_id";
            end_string += "\n" + "";
            end_string += "\n" + "WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "  AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "  AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR," + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR," + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "AND annotations.annotation_name='" + annotation_type + "'";
            end_string += "\n" + "";
            end_string += "\n" + "ORDER BY annotations.event_id;";
            end_string += "\n" + "";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[NOV10_GET_EVENTS_IDS_IN_DAY_FOR_GIVEN_ACTIVITY] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME,
@ANNOTATION_TYPE AS VARCHAR(100)


AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here

SELECT annotations.event_id


FROM SC_Browser_User_Annotations AS annotations
INNER JOIN All_Events
ON annotations.event_id = All_Events.event_id

WHERE annotations.[user_id]=@USER_ID
AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, @DAY)
AND annotations.annotation_name=@ANNOTATION_TYPE

ORDER BY annotations.event_id

END
             */
        } //close method NOV10_GET_EVENTS_IDS_IN_DAY_FOR_GIVEN_ACTIVITY()...




        public static string NOV10_GET_DAILY_ACTIVITY_SUMMARY_FROM_ANNOTATIONS(int user_id, DateTime day)
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotation_name, sum(duration_in_seconds) as total_time_spent_at_activity";
            end_string += "\n" + "";
            end_string += "\n" + "FROM (";
            end_string += "\n" + "  SELECT annotations.annotation_name, ";
            //end_string += "\n" + "  DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds";
            end_string += "\n" + "  strftime('%s', All_Events.end_time) - strftime('%s', All_Events.start_time) AS duration_in_seconds";
            end_string += "\n" + "";
            end_string += "\n" + "  FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "      INNER JOIN All_Events";
            end_string += "\n" + "      ON annotations.event_id = All_Events.[event_id]";
            end_string += "\n" + "";
            end_string += "\n" + "  WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "      AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "      AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "  AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR," + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "  AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR," + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "";
            end_string += "\n" + "  ) AS inner_table";
            end_string += "\n" + "";
            end_string += "\n" + "GROUP BY annotation_name";
            end_string += "\n" + "ORDER BY annotation_name;";
            end_string += "\n" + "";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[NOV10_GET_DAILY_ACTIVITY_SUMMARY_FROM_ANNOTATIONS] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@DAY AS DATETIME


AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here

SELECT annotation_name, sum(duration_in_seconds) as total_time_spent_at_activity

FROM
(
SELECT annotations.annotation_name, 
DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds

FROM SC_Browser_User_Annotations AS annotations

INNER JOIN All_Events
ON annotations.event_id = All_Events.[event_id]

WHERE annotations.[user_id]=@USER_ID
AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, @DAY)
AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, @DAY)
) AS inner_table

GROUP BY annotation_name

ORDER BY annotation_name


END
             */
        } //close method NOV10_GET_DAILY_ACTIVITY_SUMMARY_FROM_ANNOTATIONS()...




        public static string NOV10_GET_ANNOTATIONS_FOR_EVENT(int user_id, int event_id)
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotation_name";
            end_string += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "AND annotations.event_id=" + event_id;

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[NOV10_GET_ANNOTATIONS_FOR_EVENT] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT


AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
SELECT annotation_name

FROM SC_Browser_User_Annotations AS annotations

WHERE annotations.[user_id]=@USER_ID
AND annotations.event_id=@EVENT_ID


END
             */
        } //close method NOV10_GET_ANNOTATIONS_FOR_EVENT()...




        public static string NOV10_CLEAR_EVENT_ANNOTATIONS(int user_id, int event_id)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM SC_Browser_User_Annotations";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;

            return end_string;
            /*
             * create PROCEDURE [dbo].[NOV10_CLEAR_EVENT_ANNOTATIONS] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT



AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
DELETE FROM SC_Browser_User_Annotations
WHERE [user_id] = @USER_ID
AND [event_id] = @EVENT_ID


END
             */
        } //close method NOV10_CLEAR_EVENT_ANNOTATIONS()...




        public static string NOV10_ADD_EVENT_ANNOTATION(int user_id, int event_id, string event_annotation_name)
        {
            string end_string = "";
            end_string += "\n" + "INSERT INTO SC_Browser_User_Annotations";
            end_string += "\n" + "VALUES (" + user_id + "," + event_id + "," + convert_datetime_to_sql_string(DateTime.Now) + ",'" + event_annotation_name + "')";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[NOV10_ADD_EVENT_ANNOTATION] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID AS INT,
@EVENT_ANNOTATION_NAME AS VARCHAR(100)


AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

-- Insert statements for procedure here
INSERT INTO SC_Browser_User_Annotations
VALUES (@USER_ID, @EVENT_ID, GETDATE(), @EVENT_ANNOTATION_NAME)


END
             */
        } //close method NOV10_ADD_EVENT_ANNOTATION()...




        public static string Jan11_SPLIT_EVENT_INTO_TWO()
        {
            //todo I need to update UTC time in here
            //INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)
            //todo does UTC time really help at all??

            //todo multiple query
            string end_string = "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[Jan11_SPLIT_EVENT_INTO_TWO] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID_OF_SOURCE_IMAGES AS INT,
@TIME_OF_START_IMAGE AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

--VERY FIRST OF ALL, LET'S MAKE SURE THAT THE USER HASN'T SELECTED THE VERY FIRST IMAGE OF THE EVENT,
--AND THEN TRIED A SPLIT (AS WE TAKE THE IMAGE SELECTED AND ALL THE ONES AFTER THAT TO BE THE NEW EVENT)
-- IN THIS CASE THERE'S NO POINT IN TRYING TO SPILT THE EVENT, AS THERE'S NOTHING TO SPLIT...
DECLARE @EVENT_START_TIME AS DATETIME
SET @EVENT_START_TIME = (SELECT start_time FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_SOURCE_IMAGES)


-- NOW DOING CHECK TO MAKE SURE THAT IT ISN'T THE VERY START IMAGE IN THE EVENT THAT WE'RE USING...
IF(@TIME_OF_START_IMAGE > @EVENT_START_TIME AND @EVENT_START_TIME IS NOT NULL)
BEGIN




DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME
SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_SOURCE_IMAGES);



-- step 1, identify the ID of the new event...
DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT

--AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... 
INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)

--AND NOW OUR NEW EVENT TO APPEND THE END IMAGES TO, WILL BE THIS EVENT HERE...
SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID AND [day]=@DAY_OF_SOURCE_EVENT);




--step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
UPDATE All_Images
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES
AND image_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...

--ALSO UPDATE THE SENSOR_READINGS TABLE...
UPDATE Sensor_Readings
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES
AND sample_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...





-- step 3, update the start/end time, plus the keyframe path, of the two events in question ...
DECLARE @NEW_START_TIME AS DATETIME
DECLARE @NEW_END_TIME AS DATETIME 

--firstly the event which has the images added to it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO

UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;




--and secondly the event which has the images removed from it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES

--if the start_time is null, then that means that there's no images left in the source event
-- therefore we'll delete it
IF @NEW_START_TIME IS NULL
BEGIN
DELETE FROM All_Events
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES;
END --CLOSE IF @NEW_START_TIME IS NULL...

ELSE -- however for most scenarios it's more likely that there'll still be images left
--therefore we'll update the start/end times, plus the keyframe path too
BEGIN
UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_SOURCE_IMAGES;
END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL


END --CLOSE ... IF(@TIME_OF_START_IMAGE > @EVENT_START_TIME AND @EVENT_START_TIME IS NOT NULL)





--finally return the ID of our new event...
select @EVENT_ID_TO_APPEND_IMAGES_TO;

END
             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...



        public static string Jan11_SPLIT_EVENT_INTO_TWO_part1_get_day_of_source_event(int user_id, int event_id_of_source_images)
        {
            return Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part1_get_day_of_source_event(user_id, event_id_of_source_images);
            
            /*
             * 
DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME
SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_SOURCE_IMAGES);

END
             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...


        public static string Jan11_SPLIT_EVENT_INTO_TWO_part2_get_id_of_new_event(int user_id, DateTime day_of_source_event)
        {
            return Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part3_optional_create_new_event_to_append_images_to(user_id, day_of_source_event);
            
            /*
             

-- step 1, identify the ID of the new event...
DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT

--AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... 
INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)

--AND NOW OUR NEW EVENT TO APPEND THE END IMAGES TO, WILL BE THIS EVENT HERE...
SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID AND [day]=@DAY_OF_SOURCE_EVENT);

             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...


        public static string Jan11_SPLIT_EVENT_INTO_TWO_part3_update_image_sensors_tables_with_new_event_id(int user_id, int event_id_to_append_images_to, int event_id_of_source_images, DateTime time_of_start_image)
        {
            string end_string = "UPDATE All_Images";
            end_string += "\n" + "SET event_id = " + event_id_to_append_images_to;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_of_source_images;
            end_string += "\n" + "AND image_time >= " + convert_datetime_to_sql_string(time_of_start_image) + ";";
            end_string += "\n" + "";
            end_string += "\n" + "UPDATE Sensor_Readings";
            end_string += "\n" + "SET event_id = " + event_id_to_append_images_to;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_of_source_images;
            end_string += "\n" + "AND sample_time >= " + convert_datetime_to_sql_string(time_of_start_image) + ";";
            end_string += "\n" + "";

            return end_string;
            /*
           
--step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
UPDATE All_Images
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES
AND image_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...

--ALSO UPDATE THE SENSOR_READINGS TABLE...
UPDATE Sensor_Readings
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES
AND sample_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...

             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...


        public static string Jan11_SPLIT_EVENT_INTO_TWO_part4_update_start_end_time_of_newly_created_event(int user_id, int event_id_to_append_images_to)
        {
            return Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part5_update_start_end_time_of_newly_created_event(user_id, event_id_to_append_images_to);
            /*
           
-- step 3, update the start/end time, plus the keyframe path, of the two events in question ...
DECLARE @NEW_START_TIME AS DATETIME
DECLARE @NEW_END_TIME AS DATETIME 

--firstly the event which has the images added to it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO

UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;

             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...



        public static string Jan11_SPLIT_EVENT_INTO_TWO_part6_get_start_end_time_of_event_with_removed_images(int user_id, int event_id_of_source_images)
        {
            return Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part8_get_start_end_time_of_event_with_removed_images(user_id, event_id_of_source_images);
            /*
     

--and secondly the event which has the images removed from it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES

             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...


        public static string Jan11_SPLIT_EVENT_INTO_TWO_part7_optional_delete_event_with_removed_images(int user_id, int event_id_of_source_images)
        {
            return Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part9_optional_delete_event_with_removed_images(user_id, event_id_of_source_images);
            /*
     

--if the start_time is null, then that means that there's no images left in the source event
-- therefore we'll delete it
IF @NEW_START_TIME IS NULL
BEGIN
DELETE FROM All_Events
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES;
END --CLOSE IF @NEW_START_TIME IS NULL...

             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...


        public static string Jan11_SPLIT_EVENT_INTO_TWO_part8_optional_update_start_end_times_of_event_with_removed_images(int user_id, int event_id_of_source_images, DateTime start_time, DateTime end_time)
        {
            return Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part10_optional_update_start_end_times_of_event_with_removed_images(user_id, event_id_of_source_images, start_time, end_time);
            /*
     

ELSE -- however for most scenarios it's more likely that there'll still be images left
--therefore we'll update the start/end times, plus the keyframe path too
BEGIN
UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_SOURCE_IMAGES;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_SOURCE_IMAGES;
END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL


END --CLOSE ... IF(@TIME_OF_START_IMAGE > @EVENT_START_TIME AND @EVENT_START_TIME IS NOT NULL)





--finally return the ID of our new event...
select @EVENT_ID_TO_APPEND_IMAGES_TO;

END
             */
        } //close method Jan11_SPLIT_EVENT_INTO_TWO()...







        public static string APR11_ADD_ANNOTATION_TYPE(string annotation_type_name)
        {
            //todo multiple query
            string end_string = "";
            end_string += "\n" + "DELETE FROM Annotation_Types";
            end_string += "\n" + "WHERE annotation_type = '" + annotation_type_name + "';";
            end_string += "\n" + "INSERT INTO Annotation_Types (annotation_type,description) VALUES('" + annotation_type_name + "','" + annotation_type_name + "');";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[APR11_ADD_ANNOTATION_TYPE] 
-- Add the parameters for the stored procedure here
@ANNOTATION_TYPE_NAME AS VARCHAR(100)
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

--firstly make sure it doesn't already exist in the database
--so to achieve that, we just try to delete any entry that may already exist
EXEC APR11_REMOVE_ANNOTATION_TYPE @ANNOTATION_TYPE_NAME;

--and now we can add in our new entry safe in the knowledge there'll be no duplicate entry after this
INSERT INTO Annotation_Types VALUES(@ANNOTATION_TYPE_NAME, @ANNOTATION_TYPE_NAME)

END
             */
        } //close method APR11_ADD_ANNOTATION_TYPE()...




        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()
        {
            //todo multiple query...

            string end_string = "";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID_OF_NEW_SOURCE_IMAGES AS INT,
@TIME_OF_END_IMAGE AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;


DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME
SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_NEW_SOURCE_IMAGES);

-- step 1, identify the ID of the previous event...
DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT
SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT TOP 1 event_id
FROM All_Events
WHERE [user_id]=@USER_ID
AND event_id!=@EVENT_ID_OF_NEW_SOURCE_IMAGES --ad update on 25/01/10
AND start_time >= DATEADD(HOUR,-6,@TIME_OF_END_IMAGE)
AND start_time < @TIME_OF_END_IMAGE
AND [day] = @DAY_OF_SOURCE_EVENT
ORDER BY start_time DESC
);


-- STEP 2, CHECK TO SEE IF THERE'S NO PREVIOUS EVENT ... IN THIS CASE WE'LL HAVE TO ADD IN A NEW ONE...
IF @EVENT_ID_TO_APPEND_IMAGES_TO IS NULL
BEGIN
--AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... 
INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)

--AND NOW OUR NEW EVENT TO APPEND THINGS TO, WILL BE THIS EVENT HERE...
SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID);
END





--step 3, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
UPDATE All_Images
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES
AND image_time <= @TIME_OF_END_IMAGE

UPDATE Sensor_Readings
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES
AND sample_time <= @TIME_OF_END_IMAGE






-- step 4, update the start/end time, plus the keyframe path, of the two events in question ...
DECLARE @NEW_START_TIME AS DATETIME
DECLARE @NEW_END_TIME AS DATETIME 

--firstly the event which has the images added to it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO

UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;



--and secondly the event which has the images removed from it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES

--if the start_time is null, then that means that there's no images left in the source event
-- therefore we'll delete it
IF @NEW_START_TIME IS NULL
BEGIN
DELETE FROM All_Events
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;
END --CLOSE IF @NEW_START_TIME IS NULL...

ELSE -- however for most scenarios it's more likely that there'll still be images left
--therefore we'll update the start/end times, plus the keyframe path too
BEGIN
UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_NEW_SOURCE_IMAGES;
END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL


END
             */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...


        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part1_get_day_of_source_event(int user_id, int event_id_of_new_source_images)
        {
            //todo multiple query...

            string end_string = "SELECT [day] FROM All_Events WHERE [user_id]=" + user_id + " AND event_id=" + event_id_of_new_source_images + ";";           

            return end_string;
            /*
            DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME
            SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_NEW_SOURCE_IMAGES);

                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...



        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part2_get_id_of_event_to_append_images_to(int user_id, int event_id_of_new_source_images, DateTime time_of_end_image, DateTime day_of_source_event)
        {
            //todo multiple query...

            string end_string = "SELECT TOP 1 event_id";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id]=" + user_id;
            end_string += "\n" + "AND event_id!=" + event_id_of_new_source_images;// --ad update on 25/01/10";
            end_string += "\n" + "AND start_time >= " + convert_datetime_to_sql_string(time_of_end_image.AddHours(-6));
            end_string += "\n" + "AND start_time < " + convert_datetime_to_sql_string(time_of_end_image);
            end_string += "\n" + "AND [day] = " + convert_datetime_to_sql_string(day_of_source_event);
            end_string += "\n" + "ORDER BY start_time DESC;";            


            return end_string;
            /*
            -- step 1, identify the ID of the previous event...
            DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT
            SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT TOP 1 event_id
            FROM All_Events
            WHERE [user_id]=@USER_ID
            AND event_id!=@EVENT_ID_OF_NEW_SOURCE_IMAGES --ad update on 25/01/10
            AND start_time >= DATEADD(HOUR,-6,@TIME_OF_END_IMAGE)
            AND start_time < @TIME_OF_END_IMAGE
            AND [day] = @DAY_OF_SOURCE_EVENT
            ORDER BY start_time DESC
            );

                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...


        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part3_optional_create_new_event_to_append_images_to(int user_id, DateTime day_of_source_event)
        {
            //todo multiple query...

            string end_string = "INSERT INTO All_Events(user_id,day,utc_day,start_time,end_time,keyframe_path,number_times_viewed) VALUES (" + user_id + ", " + convert_datetime_to_sql_string(day_of_source_event) + ", " + convert_datetime_to_sql_string(day_of_source_event) + ", " + convert_datetime_to_sql_string(day_of_source_event) + ", " + convert_datetime_to_sql_string(day_of_source_event) + ", '', 0);";
            end_string += "\n" + "SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=" + user_id + ";";
            
            return end_string;
            /*
            

            -- STEP 2, CHECK TO SEE IF THERE'S NO PREVIOUS EVENT ... IN THIS CASE WE'LL HAVE TO ADD IN A NEW ONE...
            IF @EVENT_ID_TO_APPEND_IMAGES_TO IS NULL
            BEGIN
            --AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... 
            INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)

            --AND NOW OUR NEW EVENT TO APPEND THINGS TO, WILL BE THIS EVENT HERE...
            SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID);
            END

                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...


        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part4_update_images_sensors_tables_with_new_event_id(int user_id, int event_id_to_append_images_to, int event_id_of_new_source_images, DateTime time_of_end_image)
        {
            //todo multiple query...

            string end_string = "UPDATE All_Images";
            end_string += "\n" + "SET event_id = " + event_id_to_append_images_to;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_of_new_source_images;
            end_string += "\n" + "AND image_time <= " + convert_datetime_to_sql_string(time_of_end_image) + ";";
            end_string += "\n" + "";
            end_string += "\n" + "UPDATE Sensor_Readings";
            end_string += "\n" + "SET event_id = " + event_id_to_append_images_to;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_of_new_source_images;
            end_string += "\n" + "AND sample_time <= " + convert_datetime_to_sql_string(time_of_end_image) + ";";
            

            return end_string;
            /*
            --step 3, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
            UPDATE All_Images
            SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
            WHERE [user_id] = @USER_ID
            AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES
            AND image_time <= @TIME_OF_END_IMAGE

            UPDATE Sensor_Readings
            SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
            WHERE [user_id] = @USER_ID
            AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES
            AND sample_time <= @TIME_OF_END_IMAGE

                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...



        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part5_update_start_end_time_of_newly_created_event(int user_id, int event_id_to_append_images_to)
        {
            //todo multiple query...

            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET start_time = (SELECT MIN(image_time) FROM All_Images WHERE [user_id]= " + user_id + " AND event_id = " + event_id_to_append_images_to + "),";
            end_string += "\n" + "end_time = (SELECT MAX(image_time) FROM All_Images WHERE [user_id]= " + user_id + " AND event_id = " + event_id_to_append_images_to + ")";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_to_append_images_to + ";";            

            return end_string;
            /*
            -- step 4, update the start/end time, plus the keyframe path, of the two events in question ...
            DECLARE @NEW_START_TIME AS DATETIME
            DECLARE @NEW_END_TIME AS DATETIME 

            --firstly the event which has the images added to it...
            SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
            FROM All_Images
            WHERE [user_id] = @USER_ID
            AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO

            UPDATE All_Events
            SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
            WHERE [user_id] = @USER_ID
            AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;

            -- and let's update the keyframe image for the event by calling this stored procedure
            EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;

                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...


        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part8_get_start_end_time_of_event_with_removed_images(int user_id, int event_id_of_new_source_images)
        {
            //todo multiple query...

            string end_string = "SELECT MIN(image_time) as start_time, MAX(image_time) as end_time";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_of_new_source_images + ";";

            return end_string;
            /*
            
            --and secondly the event which has the images removed from it...
            SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
            FROM All_Images
            WHERE [user_id] = @USER_ID
            AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES

                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...



        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part9_optional_delete_event_with_removed_images(int user_id, int event_id_of_new_source_images)
        {
            //todo multiple query...

            string end_string = "DELETE FROM All_Events";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_of_new_source_images + ";";

            return end_string;
            /*
            
            --if the start_time is null, then that means that there's no images left in the source event
            -- therefore we'll delete it
            IF @NEW_START_TIME IS NULL
            BEGIN
            DELETE FROM All_Events
            WHERE [user_id] = @USER_ID
            AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;
            END --CLOSE IF @NEW_START_TIME IS NULL...
            
            END
                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...



        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT_part10_optional_update_start_end_times_of_event_with_removed_images(int user_id, int event_id_of_new_source_images, DateTime new_start_time, DateTime new_end_time)
        {
            //todo multiple query...

            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET start_time = " + convert_datetime_to_sql_string(new_start_time) + ", end_time = " + convert_datetime_to_sql_string(new_end_time);
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id_of_new_source_images + ";";

            return end_string;
            /*
            
            ELSE -- however for most scenarios it's more likely that there'll still be images left
            --therefore we'll update the start/end times, plus the keyframe path too
            BEGIN
            UPDATE All_Events
            SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
            WHERE [user_id] = @USER_ID
            AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;

            -- and let's update the keyframe image for the event by calling this stored procedure
            EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_NEW_SOURCE_IMAGES;
            END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL


            END
                         */
        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT()...



        public static string Oct10_ADD_NEW_MERGED_IMAGES_TO_NEXT_EVENT()
        {
            //todo multiple query...
            string end_string = "";
            end_string += "\n" + "";

            return end_string;
            /*
             * CREATE PROCEDURE [dbo].[Oct10_ADD_NEW_MERGED_IMAGES_TO_NEXT_EVENT] 
-- Add the parameters for the stored procedure here
@USER_ID AS INT,
@EVENT_ID_OF_NEW_SOURCE_IMAGES AS INT,
@TIME_OF_START_IMAGE AS DATETIME
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;


DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME
SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_NEW_SOURCE_IMAGES);



-- step 1, identify the ID of the next event...
DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT
SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT TOP 1 event_id
FROM All_Events
WHERE [user_id]=@USER_ID
AND event_id!=@EVENT_ID_OF_NEW_SOURCE_IMAGES
AND start_time > @TIME_OF_START_IMAGE
AND start_time <= DATEADD(HOUR,6,@TIME_OF_START_IMAGE)
AND [day] = @DAY_OF_SOURCE_EVENT
ORDER BY start_time --order ascending, i.e. select the next event (that's our target)
);




-- STEP 2, CHECK TO SEE IF THERE'S NO NEXT EVENT ... IN THIS CASE WE'LL HAVE TO ADD IN A NEW ONE...
IF @EVENT_ID_TO_APPEND_IMAGES_TO IS NULL
BEGIN
--AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... 
INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)

--AND NOW OUR NEW EVENT TO APPEND THINGS TO, WILL BE THIS EVENT HERE...
SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID);
END




--step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
UPDATE All_Images
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES
AND image_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...

--ALSO UPDATE THE SENSOR_READINGS TABLE...
UPDATE Sensor_Readings
SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES
AND sample_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...





-- step 3, update the start/end time, plus the keyframe path, of the two events in question ...
DECLARE @NEW_START_TIME AS DATETIME
DECLARE @NEW_END_TIME AS DATETIME 

--firstly the event which has the images added to it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO

UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;




--and secondly the event which has the images removed from it...
SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)
FROM All_Images
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES

--if the start_time is null, then that means that there's no images left in the source event
-- therefore we'll delete it
IF @NEW_START_TIME IS NULL
BEGIN
DELETE FROM All_Events
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;
END --CLOSE IF @NEW_START_TIME IS NULL...

ELSE -- however for most scenarios it's more likely that there'll still be images left
--therefore we'll update the start/end times, plus the keyframe path too
BEGIN
UPDATE All_Events
SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME
WHERE [user_id] = @USER_ID
AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;

-- and let's update the keyframe image for the event by calling this stored procedure
EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_NEW_SOURCE_IMAGES;
END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL






END
             */

        } //close method Oct10_ADD_NEW_MERGED_IMAGES_TO_NEXT_EVENT()...



        #endregion aiden

    } //close class text_for_stored_procedures...
} //close namespace...
