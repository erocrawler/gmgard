using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System;
using System.Collections.Generic;

namespace GmGardMigrations.OneOffTasks
{
    static class ElasticCommon
    {
        public class BlogIndexed
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public int CategoryId { get; set; }
            public DateTime CreateDate { get; set; }
            public string Author { get; set; }
            public bool IsHarmony { get; set; }
            public bool? IsApproved { get; set; }
            public long BlogVisit { get; set; }
            public int PostCount { get; set; }
            public int Rating { get; set; }
            public string ImagePath { get; set; }
            public bool IsLocalImg { get; set; }
        }

        public static ElasticsearchClient CreateClient(string endpoint, string username, string password)
        {
            var settings = new ElasticsearchClientSettings(new Uri(endpoint))
                .DefaultIndex("blogs")
                .Authentication(new BasicAuthentication(username, password))
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            return new ElasticsearchClient(settings);
        }
    }
}
