DECLARE @query VARCHAR(100) = 'tc'

SELECT * FROM epg_channel WHERE channel_name LIKE '%'+@query+'%' ORDER BY channel_name
SELECT * FROM kapook_channel WHERE nam_channel_en LIKE '%'+@query+'%' OR nam_channel_th LIKE '%'+@query+'%' ORDER BY nam_channel_en
SELECT * FROM main_channel WHERE nam_channel_en LIKE '%'+@query+'%' OR nam_channel_th LIKE '%'+@query+'%' ORDER BY nam_channel_en
