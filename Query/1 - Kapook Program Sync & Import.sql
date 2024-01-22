
-- Import Data From Kapook Database
--INSERT INTO main_program(channel_id, ref_kapook, nam_program_th, nam_program_en, cod_user_cre, cod_user_upd)
SELECT DISTINCT c.channel_id, p.program_key_id, p.nam_program_th, p.nam_program_en, 'kapook_sync', 'kapook_sync'
--SELECT DISTINCT c.channel_id, c.nam_channel_en, p.program_key_id, p.nam_program_th, p.nam_program_en, 'kapook_sync', 'kapook_sync'
FROM
	main_channel c INNER JOIN
	kapook_program p ON c.ref_kapook = p.channel_id
WHERE
	p.program_key_id NOT IN ('514be4cc61b01d547200000a','51495b5161b01d0d4d000008','532bfdd161b01d1d77b94818','53d8a03461b01dbe6c58edb8','53e305f561b01de20bfac9f0','53d08bfe61b01d68375f622e','5384359361b01da31de02f59') AND
	p.program_key_id NOT IN (SELECT ref_kapook FROM main_program WHERE sta_RecStatus = 'A' AND ref_kapook IS NOT NULL) AND
	1 = 1
ORDER BY p.nam_program_th


-- Sync with Kapook Program using Main Map EPG Event Program
SELECT DISTINCT c.channel_id, mp.program_id, kp.program_key_id, kp.nam_program_th, kp.nam_program_en, map.nam_program_title, 'kapook_sync', 'kapook_sync'
--UPDATE mp SET ref_kapook = kp.program_id, cod_user_upd = 'kapook_sync', dte_upd = CURRENT_TIMESTAMP
FROM
	main_channel c INNER JOIN
	kapook_program kp ON kp.channel_id = c.ref_kapook INNER JOIN
	main_map_epg_event_program map ON c.channel_id = map.channel_id AND (REPLACE(REPLACE(kp.nam_program_th, '-', ''), ' ', '') LIKE '%'+map.nam_program_title+'%' OR REPLACE(REPLACE(kp.nam_program_en, '-', ''), ' ', '') LIKE '%'+map.nam_program_title+'%' OR map.nam_program_title LIKE '%'+REPLACE(REPLACE(kp.nam_program_th, '-', ''), ' ', '')+'%' OR map.nam_program_title LIKE '%'+REPLACE(REPLACE(kp.nam_program_en, '-', ''), ' ', '')+'%') INNER JOIN
	main_program mp ON map.program_id = mp.program_id AND mp.ref_kapook IS NULL
WHERE
	kp.program_key_id NOT IN ('514be4cc61b01d547200000a','51495b5161b01d0d4d000008','532bfdd161b01d1d77b94818','53d8a03461b01dbe6c58edb8','53e305f561b01de20bfac9f0','53d08bfe61b01d68375f622e','5384359361b01da31de02f59') AND
	kp.program_key_id NOT IN (SELECT ref_kapook FROM main_program WHERE sta_RecStatus = 'A' AND ref_kapook IS NOT NULL) AND
	1 = 1
ORDER BY kp.nam_program_th


-- Sync with Kapook Program using Main Program
SELECT c.channel_id, mp.program_id, kp.program_key_id, kp.nam_program_th, kp.nam_program_en, mp.nam_program_th, mp.nam_program_en, 'kapook_sync', 'kapook_sync'
--UPDATE mp SET ref_kapook = kp.program_id, cod_user_upd = 'kapook_sync', dte_upd = CURRENT_TIMESTAMP
FROM
	main_channel c INNER JOIN
	kapook_program kp ON kp.channel_id = c.ref_kapook INNER JOIN
	main_program mp ON c.channel_id = c.channel_id AND mp.ref_kapook IS NULL AND (kp.nam_program_th LIKE '%'+mp.nam_program_en+'%' OR kp.nam_program_th LIKE '%'+mp.nam_program_en+'%' OR kp.nam_program_en LIKE '%'+mp.nam_program_en+'%' OR kp.nam_program_en LIKE '%'+mp.nam_program_en+'%' OR mp.nam_program_th LIKE '%'+kp.nam_program_en+'%' OR mp.nam_program_th LIKE '%'+kp.nam_program_en+'%' OR mp.nam_program_en LIKE '%'+kp.nam_program_en+'%' OR mp.nam_program_en LIKE '%'+kp.nam_program_en+'%')
WHERE
	kp.program_key_id NOT IN ('514be4cc61b01d547200000a','51495b5161b01d0d4d000008','532bfdd161b01d1d77b94818','53d8a03461b01dbe6c58edb8','53e305f561b01de20bfac9f0','53d08bfe61b01d68375f622e','5384359361b01da31de02f59') AND
	kp.program_key_id NOT IN (SELECT ref_kapook FROM main_program WHERE sta_RecStatus = 'A' AND ref_kapook IS NOT NULL) AND
	1 = 1
ORDER BY kp.nam_program_th
