
--Import Non Existing EPG into Main Database (EPG)
--INSERT INTO main_epg(channel_id, program_id, nam_epg_th, nam_epg_en, desc_epg_th, desc_epg_en, epg_start, epg_stop, cod_user_cre, cod_user_upd)
SELECT DISTINCT map.channel_id, map.program_id, e.title_th, e.title_en, e.desc_th, e.desc_en, e.start_time, e.stop_time, 'sat_sync', 'sat_sync'--,*
FROM
	epg_event e INNER JOIN
	main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND (REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') = map.nam_program_title OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = map.nam_program_title) INNER JOIN
	main_channel c ON map.channel_id = c.channel_id AND map.ref_epg_channel = c.ref_epg AND c.sta_RecStatus = 'A' AND c.source = 'SAT'
WHERE
	e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
	--e.start_time > CONVERT(DATE, CURRENT_TIMESTAMP) AND
	--e.stop_time <= CONVERT(DATE, CURRENT_TIMESTAMP) AND
	NOT EXISTS (SELECT * FROM main_epg t WHERE t.channel_id = map.channel_id AND t.epg_start >= e.start_time AND epg_stop <= e.stop_time AND sta_RecStatus = 'A') AND
	1 = 1
ORDER BY
	e.stop_time


--Sync EPG Information from Satellite EPG
SELECT *
--UPDATE me SET me.nam_epg_th = LTRIM(RTRIM(e.title_th)), me.nam_epg_en = LTRIM(RTRIM(e.title_en)), me.desc_epg_th = LTRIM(RTRIM(e.desc_th)), me.desc_epg_en = LTRIM(RTRIM(e.desc_en)), me.sta_RecStatus = 'A', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = 'sat_sync'
FROM
	main_channel c INNER JOIN
	main_epg me ON c.channel_id = me.channel_id INNER JOIN
	main_map_epg_event_program map ON me.program_id = map.program_id AND c.ref_epg = map.ref_epg_channel INNER JOIN
	epg_event e ON map.ref_epg_channel = e.channel_id AND (REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') = map.nam_program_title OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = map.nam_program_title) AND e.start_time = me.epg_start AND e.stop_time = me.epg_stop
WHERE
	e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
	c.source = 'SAT'


--Disable Overlapped EPG Informations (To be Enabled later on if Valid)
SELECT *
--UPDATE me SET me.sta_RecStatus = 'C', me.dte_upd = CURRENT_TIMESTAMP, me.cod_user_upd = 'sat_sync'
FROM
	main_channel c INNER JOIN
	main_epg me ON c.channel_id = me.channel_id
WHERE
	EXISTS (SELECT * FROM epg_event e WHERE e.channel_id = c.ref_epg AND me.epg_stop > e.start_time AND me.epg_start < e.stop_time AND e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP))) AND
	c.source = 'SAT' AND
	me.sta_RecStatus = 'A'