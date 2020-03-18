create trigger Rating_trigger
on ratings
after insert,update,delete
as
begin
update b
set b.Rating = (
	select isnull(SUM(ratings.value), 0) from Ratings where
	BlogID = i.BlogID
	)
from dbo.Blogs as b inner join 
(select BlogID from inserted union select BlogID from deleted) as i 
on i.BlogID = b.BlogID
end