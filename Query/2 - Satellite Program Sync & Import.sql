-- Clean Up Duplicate Program Record (On The Air)
SELECT *
--DELETE map 
FROM main_program map
WHERE EXISTS 
(
	SELECT channel_id, nam_program_en, MIN(program_id), COUNT(*) FROM main_program t WHERE t.channel_id = map.channel_id AND t.nam_program_en = map.nam_program_en GROUP BY channel_id, nam_program_en HAVING COUNT(*) > 1 AND MIN(program_id) <> map.program_id
)

-- Import Data From Satellite EPG (On The Air)
--INSERT INTO main_program(channel_id, nam_program_th, nam_program_en, desc_program_th, desc_program_en, cod_user_cre, cod_user_upd)
SELECT DISTINCT c.channel_id, LTRIM(RTRIM(e.title_th)), LTRIM(RTRIM(e.title_en)), LTRIM(RTRIM(e.desc_th)), LTRIM(RTRIM(e.desc_en)), 'epg_sync', 'epg_sync'
FROM
	main_channel c INNER JOIN
	epg_event e ON c.ref_epg = e.channel_id LEFT JOIN
	main_map_epg_event_program map ON map.ref_epg_channel = e.channel_id AND (map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') OR map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''))
WHERE
	map.channel_id IS NULL AND
	c.sta_RecStatus = 'A' AND
	c.source = 'SAT' AND
	e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP))


--Clean Up Duplicate Mapped Program Record (SAT)
SELECT *
--DELETE map
FROM main_map_epg_event_program map
WHERE EXISTS 
(
	SELECT channel_id, nam_program_title, MIN(program_id), COUNT(*) FROM main_map_epg_event_program t WHERE t.channel_id = map.channel_id AND t.nam_program_title = map.nam_program_title AND t.ref_epg_channel = map.ref_epg_channel GROUP BY channel_id, nam_program_title, ref_epg_channel HAVING COUNT(*) > 1 AND MIN(program_id) <> map.program_id
)
ORDER BY channel_id, nam_program_title


-- Map Captured Program Name into Main Database (Sync Data)
--INSERT INTO main_map_epg_event_program
SELECT DISTINCT * FROM 
(
	SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') title
	FROM
		main_channel c INNER JOIN
		main_program mp ON c.channel_id = mp.channel_id INNER JOIN
		epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
		main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '')
	WHERE
		map.channel_id IS NULL AND
		c.sta_RecStatus = 'A' AND
		c.source = 'SAT' AND
		e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
		1 = 1
	UNION
	SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') title
	FROM
		main_channel c INNER JOIN
		main_program mp ON c.channel_id = mp.channel_id INNER JOIN
		epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(e.title_en, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
		main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', '')
	WHERE
		map.channel_id IS NULL AND
		c.sta_RecStatus = 'A' AND
		c.source = 'SAT' AND
		e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
		1 = 1
	UNION
	SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') title
	FROM
		main_channel c INNER JOIN
		main_program mp ON c.channel_id = mp.channel_id INNER JOIN
		epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')) LEFT JOIN
		main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_en, '-', ''), ' ', '')
	WHERE
		map.channel_id IS NULL AND
		c.sta_RecStatus = 'A' AND
		c.source = 'SAT' AND
		e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
		1 = 1
	UNION
	SELECT c.channel_id ref_main_channel, mp.program_id ref_main_program, e.channel_id, REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') title
	FROM
		main_channel c INNER JOIN
		main_program mp ON c.channel_id = mp.channel_id INNER JOIN
		epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') OR REPLACE(REPLACE(e.title_th, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]') OR REPLACE(REPLACE(e.title_en, '-', ''), ' ', '') LIKE REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')) LEFT JOIN
		main_map_epg_event_program map ON e.channel_id = map.ref_epg_channel AND map.nam_program_title = REPLACE(REPLACE(e.title_th, '-', ''), ' ', '')
	WHERE
		map.channel_id IS NULL AND
		c.sta_RecStatus = 'A' AND
		c.source = 'SAT' AND
		e.last_update > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
		1 = 1
) A
WHERE title <> ''
ORDER BY title
	

-- 1) Clean Up Program Name (ทดลองออกอากาศ), (Broadcast testing), Leading Spaces, Ending Spaces, Fill Blank Program Name, Delete Blank Program Name
SELECT LEFT(title_en, LEN(title_en) - CHARINDEX('(', REVERSE(title_en))), * FROM epg_event WHERE (title_en LIKE '%(ท%' OR title_en LIKE '%(Broadcast%')
SELECT LEFT(title_th, LEN(title_th) - CHARINDEX('(', REVERSE(title_th))), * FROM epg_event WHERE (title_th LIKE '%(ท%' OR title_th LIKE '%(Broadcast%')
--DELETE FROM epg_event WHERE title_th = '' AND title_en = ''
--UPDATE epg_event SET title_th = title_en WHERE (title_th = '')
--UPDATE epg_event SET title_en = title_th WHERE (title_en = '')
--UPDATE epg_event SET title_th = LTRIM(RTRIM(title_th)) WHERE (title_th LIKE ' %' OR title_th LIKE '% ')
--UPDATE epg_event SET title_en = LTRIM(RTRIM(title_en)) WHERE (title_en LIKE ' %' OR title_en LIKE '% ')
--UPDATE epg_event SET title_th = LEFT(title_th, LEN(title_th) - CHARINDEX('(', REVERSE(title_th))) WHERE (title_th LIKE '%(ท%' OR title_th LIKE '%(Broadcast%')
--UPDATE epg_event SET title_en = LEFT(title_en, LEN(title_en) - CHARINDEX('(', REVERSE(title_en))) WHERE (title_en LIKE '%(ท%' OR title_en LIKE '%(Broadcast%')
