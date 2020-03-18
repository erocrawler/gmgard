create trigger PostRating_trigger
on PostRatings
after insert,update,delete
as
begin
update p
set p.Rating = (
	select isnull(SUM(PostRatings.[Value]), 0) from PostRatings where
	PostID = i.PostID
	)
from dbo.Posts as p inner join 
(select PostID from inserted union select PostID from deleted) as i 
on i.PostID = p.PostID
end