SELECT * FROM main_channel c WHERE c.channel_id = 1
SELECT * FROM main_program p WHERE p.channel_id = 1
SELECT * FROM epg_channel
--WHERE channel_name LIKE '%thai%' 
ORDER BY channel_name
SELECT * FROM main_program p WHERE p.nam_program_th LIKE '%ªÐµÒ%'
SELECT * FROM epg_event WHERE channel_id IN ('123','67','124','125')
SELECT * FROM epg_event WHERE title_en LIKE '%168%'
SELECT ref_kapook, nam_program_th, nam_program_en, cod_user_cre, cod_user_upd FROM main_program
SELECT * FROM kapook_program WHERE program_key_id IN ('53cf658461b01dbd15c9c79f','532bfdd161b01d1d77b94818')

--SELECT DISTINCT c.channel_id ref_main_channel, c.nam_channel_en, p.program_id ref_main_program, p.nam_program_th, p.nam_program_en, e.channel_id, e.title_th, e.title_en--, *
--SELECT DISTINCT c.channel_id ref_main_channel, c.nam_channel_en, p.program_id ref_main_program, p.nam_program_th, p.nam_program_en, e.channel_id, e.title_th, e.title_en--, *
SELECT DISTINCT c.channel_id ref_main_channel, p.program_id ref_main_program, e.channel_id, e.title_th title
FROM
	main_channel c INNER JOIN
	main_program p ON c.channel_id = p.channel_id RIGHT JOIN
	epg_event e ON c.ref_epg = e.channel_id AND (REPLACE(e.title_th, ' ', '') LIKE '%'+REPLACE(p.nam_program_en, ' ', '')+'%' OR REPLACE(e.title_th, ' ', '') LIKE '%'+REPLACE(p.nam_program_th, ' ', '')+'%' OR REPLACE(e.title_en, ' ', '') LIKE '%'+REPLACE(p.nam_program_en, ' ', '')+'%' OR REPLACE(e.title_en, ' ', '') LIKE '%'+REPLACE(p.nam_program_th, ' ', '')+'%' OR REPLACE(p.nam_program_th, ' ', '') LIKE '%'+REPLACE(e.title_en, ' ', '')+'%' OR REPLACE(p.nam_program_th, ' ', '') LIKE '%'+REPLACE(e.title_th, ' ', '')+'%' OR REPLACE(p.nam_program_en, ' ', '') LIKE '%'+REPLACE(e.title_en, ' ', '')+'%' OR REPLACE(p.nam_program_en, ' ', '') LIKE '%'+REPLACE(e.title_th, ' ', '')+'%') LEFT JOIN
	main_map_epg_event_program m ON m.ref_epg_channel = e.channel_id AND (m.nam_program_title = e.title_en OR m.nam_program_title = e.title_th)
WHERE
	--m.channel_id IS NULL AND
	c.channel_id = 1

SELECT * FROM epg_event
SELECT LEFT(title_en, LEN(title_en) - CHARINDEX('(', REVERSE(title_en))), * FROM epg_event WHERE title_en LIKE '%(·%' OR title_en LIKE '%(Broadcast%'
SELECT LEFT(title_th, LEN(title_th) - CHARINDEX('(', REVERSE(title_th))), * FROM epg_event WHERE title_th LIKE '%(·%' OR title_th LIKE '%(Broadcast%'

SELECT channel_id, nam_program_th, nam_program_en, desc_program_th, desc_program_en, cod_user_cre, cod_user_upd FROM main_program
SELECT c.channel_id, e.title_th, e.title_en, e.desc_th, e.desc_en, 'epg_sync', 'epg_sync'
FROM
	main_channel c INNER JOIN
	epg_event e ON c.ref_epg = e.channel_id LEFT JOIN
	main_map_epg_event_program m ON m.ref_epg_channel = e.channel_id AND (m.nam_program_title = e.title_en OR m.nam_program_title = e.title_th)
WHERE
	m.channel_id IS NULL AND
	c.channel_id = 1
	AND title_en LIKE '%(%' AND title_th LIKE '%(%'
	
