select * --into tmploch0002_141023
from loch0002 --document types
where module1 = 2

update loch0002 set dscriptn = 'NOTA DE DEBITO ELECTRONICA'
where lochdoccod = '56' and module1 = 2

update loch0002 set dscriptn = 'NOTA DE CREDITO ELECTRONICA', doctype = 8, rprttype = 1
where lochdoccod = '61' and module1 = 2

update loch0002 set doctype = 1
where lochdoccod = '55' and module1 = 2

select *
from loch0005

------------------------------
select *
from loch0004	--hist doc types per invoice
where lochdoccod = '61'
and module1 = 2
order by docdate

select *
from loch0004	--hist doc types per invoice
where lochdoccod = '55'
and module1 = 2
order by docdate

