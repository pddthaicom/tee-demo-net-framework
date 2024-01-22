DECLARE @query VARCHAR(100) = ''

SELECT * FROM kapook_program WHERE nam_program_en LIKE '%' + @query + '%' OR nam_program_th LIKE '%' + @query + '%' ORDER BY nam_program_en
SELECT * FROM epg_event WHERE title_en LIKE '%' + @query + '%' OR title_th LIKE '%' + @query + '%' ORDER BY title_en
SELECT * FROM main_program WHERE nam_program_en LIKE '%' + @query + '%' OR nam_program_th LIKE '%' + @query + '%' ORDER BY nam_program_en
SELECT * FROM main_map_epg_event_program WHERE nam_program_title LIKE '%' + @query + '%'
