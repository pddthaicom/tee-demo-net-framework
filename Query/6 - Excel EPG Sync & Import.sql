DECLARE @channel_id AS INT = 3
DECLARE @name AS VARCHAR(255) = 'ÊÇÂÊØ´ÊÂÒÁ'
DECLARE @desc AS VARCHAR(255) = ''
DECLARE @start AS DATETIME = '2014-10-30 00:00'
DECLARE @stop AS DATETIME = '2014-10-30 00:15'
DECLARE @user AS VARCHAR(255) = 'excel_sync'


-- 3) Import Non Existing EPG into Main Database (EPG)
--INSERT INTO main_epg(channel_id, program_id, nam_epg_th, nam_epg_en, desc_epg_th, desc_epg_en, epg_start, epg_stop, cod_user_cre, cod_user_upd)
SELECT map.channel_id, map.program_id, @name, @name, @desc, @desc, @start, @stop, @user, @user
FROM
	main_map_excel_program map
WHERE
	map.channel_id = @channel_id AND
	REPLACE(REPLACE(@name, '-', ''), ' ', '') = map.nam_program_title AND
	NOT EXISTS (SELECT *
		FROM
			main_epg me INNER JOIN
			main_map_excel_program map ON me.program_id = map.program_id
		WHERE
			map.channel_id = @channel_id AND
			REPLACE(REPLACE(@name, '-', ''), ' ', '') = map.nam_program_title AND
			me.epg_start = @start AND
			me.epg_stop = @stop)
	

-- 2) Sync EPG Information from Satellite EPG
SELECT *
--UPDATE me SET me.nam_epg_th = LTRIM(RTRIM(@name)), me.nam_epg_en = LTRIM(RTRIM(@name)), me.desc_epg_th = LTRIM(RTRIM(@desc)), me.desc_epg_en = LTRIM(RTRIM(@desc)), me.sta_RecStatus = 'A', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = 'sat_sync'
FROM
	main_epg me INNER JOIN
	main_map_excel_program map ON me.program_id = map.program_id
WHERE
	map.channel_id = @channel_id AND
	REPLACE(REPLACE(@name, '-', ''), ' ', '') = map.nam_program_title AND
	me.epg_start = @start AND
	me.epg_stop = @stop


-- 1) Deactivate Overlapped Program
SELECT *
--UPDATE me SET me.sta_RecStatus = 'C', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = @user
FROM
	main_epg me
WHERE
	me.epg_stop > @start AND
	me.epg_start < @stop AND
	me.channel_id = @channel_id