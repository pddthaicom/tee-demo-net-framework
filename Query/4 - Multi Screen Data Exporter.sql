SELECT * FROM channel
SELECT * FROM program
SELECT * FROM episode
SELECT * FROM calendar
SELECT * FROM program_schedule
SELECT * FROM rerun
SELECT nam_channel, txt_detail, flg_showchat, status, cod_upd, dtm_upd, id_ref FROM channel c
SELECT MIN(id_episode) id_episode, id_channel, dtm_from, dtm_to FROM episode WHERE status = 'A' GROUP BY id_channel, dtm_from, dtm_to HAVING COUNT(*) > 1

-- 8) Cleanup Duplicate Data in MultiScreen EPG
SELECT e.* 
--DELETE e 
FROM 
	episode e INNER JOIN
	(SELECT MIN(id_episode) id_episode, id_channel, dtm_from, dtm_to FROM episode WHERE status = 'A' GROUP BY id_channel, dtm_from, dtm_to HAVING COUNT(*) > 1) t ON e.id_channel = t.id_channel AND e.dtm_from = t.dtm_from AND e.dtm_to = t.dtm_to
WHERE
	e.status = 'A' AND
	e.id_episode <> t.id_episode



-- 7) Sync Main EPG with MultiScreen EPG
SELECT e.*
--UPDATE e SET dtm_from = me.epg_start, dtm_to = me.epg_stop, status = me.sta_RecStatus, dtm_upd = CURRENT_TIMESTAMP
FROM
	EPGSERVER.DTVEpg.dbo.main_epg me INNER JOIN
	episode e ON e.id_ref = me.epg_id
WHERE
	me.dte_upd > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)) AND
	me.dte_upd <> me.dte_cre



-- 6) Import Main EPG with MultiScreen EPG
--INSERT INTO episode(id_channel, id_program, nam_episode, txt_detail, url_image, url_image2, url_image_mobile, url_image_thumb, dtm_from, dtm_to, status, dtm_upd, cod_upd, id_ref)
SELECT c.id_channel, p.id_program, me.nam_epg_th, me.desc_epg_th, '', '', '', '', me.epg_start, me.epg_stop, me.sta_RecStatus, CURRENT_TIMESTAMP, '1', me.epg_id
FROM
	EPGSERVER.DTVEpg.dbo.main_epg me INNER JOIN
	channel c ON me.channel_id = c.id_ref INNER JOIN
	program p ON me.program_id = p.id_ref
WHERE
	me.epg_id NOT IN (SELECT id_ref FROM episode WHERE id_ref IS NOT NULL) AND
	me.sta_RecStatus = 'A'



-- 5) Disable Overlapped EPG Informations (To be Enabled later on if Valid)
SELECT me.*
--UPDATE me SET me.status = 'C', me.dtm_upd = CURRENT_TIMESTAMP, me.cod_upd = '1'
FROM
	channel c INNER JOIN
	episode me ON c.id_channel = me.id_channel
WHERE 
	EXISTS (SELECT * FROM EPGSERVER.DTVEpg.dbo.main_epg e WHERE e.channel_id = c.id_ref AND me.dtm_to > e.epg_start AND me.dtm_from < e.epg_stop AND e.dte_upd > CONVERT(DATE, DATEADD(DAY, -1, CURRENT_TIMESTAMP)))
	AND me.status = 'A'



-- 4) Import Main Program into MultiScreen Program
--INSERT INTO program(id_channel, nam_program, txt_detail, url_image, url_image2, status, cod_upd, dtm_upd, id_ref)
SELECT c.id_channel, LTRIM(RTRIM(mp.nam_program_th)), LTRIM(RTRIM(mp.desc_program_th)), '', '', 'A', '1', CURRENT_TIMESTAMP, mp.program_id
FROM
	EPGSERVER.DTVEpg.dbo.main_program mp INNER JOIN
	channel c ON mp.channel_id = c.id_ref
WHERE
	mp.program_id NOT IN (SELECT id_ref FROM program WHERE id_ref IS NOT NULL)



-- 3) Sync Main Program with MultiScreen Program (Update Program Reference)
SELECT mp.program_id, mp.nam_program_th, mp.nam_program_en, mp.desc_program_th, mp.desc_program_en, p.id_program, p.id_channel, p.id_ref, p.nam_program--, mp.*, c.*, p.*
--UPDATE p SET p.id_ref = mp.program_id, p.dtm_upd = CURRENT_TIMESTAMP
FROM
	EPGSERVER.DTVEpg.dbo.main_program mp INNER JOIN
	channel c ON mp.channel_id = c.id_ref INNER JOIN
	program p ON c.id_channel = p.id_channel AND p.id_ref IS NULL AND (REPLACE(REPLACE(p.nam_program, '-', ''), ' ', '') COLLATE Thai_CI_AS = REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') COLLATE Thai_CI_AS OR REPLACE(REPLACE(p.nam_program, '-', ''), ' ', '') COLLATE Thai_CI_AS = REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') COLLATE Thai_CI_AS)
WHERE
	mp.program_id NOT IN (SELECT id_ref FROM program WHERE id_ref IS NOT NULL) AND
	mp.sta_RecStatus = 'A'



-- 2) Import Main Channel into MultiScreen Channel
--INSERT INTO channel(nam_channel, txt_detail, flg_showchat, num_order, id_content_provider, status, cod_upd, dtm_upd, id_ref)
SELECT LTRIM(RTRIM(nam_channel_th)) nam_channel, LTRIM(RTRIM(nam_channel_en)) txt_detail, 'N' flg_showchat, o.channel_no, 1, 'A' status, '1' cod_upd, CURRENT_TIMESTAMP dtm_upd, mc.channel_id id_ref
FROM
	EPGSERVER.DTVEpg.dbo.main_channel mc INNER JOIN
	EPGSERVER.DTVEpg.dbo.mst_channel_order o ON mc.channel_id = o.channel_id
WHERE
	mc.channel_id NOT IN (SELECT id_ref FROM channel WHERE id_ref IS NOT NULL) AND
	mc.sta_RecStatus = 'A' AND
	o.provider_id = @provider_id AND
	o.sta_RecStatus = 'A'



-- 1) Sync Main Channel into MultiScreen Channel
SELECT c.*
--UPDATE c SET num_order = o.channel_no, id_content_provider = '1', cod_upd = '1', dtm_upd = CURRENT_TIMESTAMP
FROM
	channel c INNER JOIN
	EPGSERVER.DTVEpg.dbo.main_channel mc ON mc.channel_id = c.id_ref INNER JOIN
	EPGSERVER.DTVEpg.dbo.mst_channel_order o ON mc.channel_id = o.channel_id
WHERE
	mc.sta_RecStatus = 'A' AND
	o.provider_id = @provider_id AND
	o.sta_RecStatus = 'A'