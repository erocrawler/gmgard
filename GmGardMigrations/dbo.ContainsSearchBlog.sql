create function ContainsSearchBlog(@term nvarchar(MAX))
                returns @T table([SearchRank] int, [BlogID] int) as
				begin
					declare @trimterm nvarchar(1000)
					set @trimterm = cast(@term as nvarchar(1000))
					insert into @T SELECT [Rank] as SearchRank, [Key] as BlogID FROM CONTAINSTABLE(Blogs, BlogTitle, @trimterm)
					return
				end