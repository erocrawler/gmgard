CREATE FULLTEXT CATALOG [blogs_catalog]
    WITH ACCENT_SENSITIVITY = OFF
    AUTHORIZATION [dbo];
CREATE FULLTEXT INDEX ON [dbo].[Blogs]
    ([BlogTitle] LANGUAGE 1033)
    KEY INDEX [PK_dbo.Blogs]
    ON [blogs_catalog];