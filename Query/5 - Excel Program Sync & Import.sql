DECLARE @channel_id AS INT = 3
DECLARE @name AS VARCHAR(255) = 'สวยสุดสยาม'
DECLARE @desc AS VARCHAR(255) = ''
DECLARE @user AS VARCHAR(255) = 'excel_sync'

-- 2) Import New Excel Program (Not Exists)
--INSERT INTO main_program(channel_id, nam_program_th, nam_program_en, desc_program_th, desc_program_en, cod_user_cre, cod_user_upd)
SELECT @channel_id, @name, @name, @desc, @desc, @user, @user
WHERE NOT EXISTS (SELECT * FROM main_map_excel_program map WHERE map.channel_id = @channel_id AND map.nam_program_title = REPLACE(REPLACE(@name, '-', ''), ' ', ''))


-- 1) Map Excel Program Name into Main Database (Sync Data)
--INSERT INTO main_map_excel_program
SELECT TOP 1 c.channel_id ref_main_channel, mp.program_id, REPLACE(REPLACE(@name, '-', ''), ' ', '') title
FROM
	main_channel c INNER JOIN
	main_program mp ON c.channel_id = mp.channel_id AND (REPLACE(REPLACE(@name, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(@name, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_en, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(@name, '-', ''), ' ', ''), '[', '[[]')+'%' OR REPLACE(REPLACE(mp.nam_program_th, '-', ''), ' ', '') LIKE '%'+REPLACE(REPLACE(REPLACE(@name, '-', ''), ' ', ''), '[', '[[]')+'%') LEFT JOIN
	main_map_excel_program map ON map.channel_id = mp.channel_id AND map.nam_program_title = REPLACE(REPLACE(@name, '-', ''), ' ', '')
WHERE
	c.channel_id = @channel_id AND
	map.channel_id IS NULL
